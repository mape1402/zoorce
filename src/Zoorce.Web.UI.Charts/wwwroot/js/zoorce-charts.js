(() => {
    "use strict";

    const svgNamespace = "http://www.w3.org/2000/svg";
    const palette = ["var(--z-data-primary)", "var(--z-data-secondary)", "var(--z-data-tertiary)", "var(--z-data-quaternary)", "var(--z-data-quinary)"];
    const controllers = new WeakMap();
    const chartKindsWithoutAxes = new Set(["Donut", "Gauge", "Funnel", "Heatmap", "Bullet", "Radial", "Treemap", "Sankey"]);
    const number = value => Number.isFinite(Number(value)) ? Number(value) : 0;
    const clamp = (value, minimum, maximum) => Math.min(maximum, Math.max(minimum, value));
    const unique = values => [...new Set(values)];
    const svg = name => document.createElementNS(svgNamespace, name);
    const format = value => Number.isFinite(value)
        ? new Intl.NumberFormat(undefined, { maximumFractionDigits: Math.abs(value) < 10 && value % 1 ? 1 : 0 }).format(value)
        : "—";
    const withSuffix = (value, suffix) => {
        const unit = String(suffix ?? "");
        return `${format(value)}${unit && /^\p{L}/u.test(unit) ? " " : ""}${unit}`;
    };
    const niceNumber = value => {
        const exponent = Math.floor(Math.log10(Math.max(Math.abs(value), 1)));
        const fraction = Math.abs(value) / (10 ** exponent);
        const base = fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10;
        return base * (10 ** exponent);
    };
    const ticks = (minimum, maximum, count = 4) => {
        const step = niceNumber((maximum - minimum || 1) / count);
        const start = Math.floor(minimum / step) * step;
        const end = Math.ceil(maximum / step) * step;
        const result = [];
        for (let current = start; current <= end + step / 100; current += step) result.push(Number(current.toFixed(8)));
        return result;
    };
    const setAttributes = (element, attributes) => Object.entries(attributes).forEach(([name, value]) => {
        if (value !== undefined && value !== null) element.setAttribute(name, String(value));
    });
    const setPaint = (element, paint) => Object.entries(paint).forEach(([name, value]) => {
        if (value !== undefined && value !== null) element.style.setProperty(name, String(value));
    });
    const appendText = (parent, x, y, value, className, anchor = "start") => {
        const text = svg("text");
        setAttributes(text, { class: className, x, y, "text-anchor": anchor });
        text.textContent = value;
        parent.append(text);
        return text;
    };
    // SVG paths remain true vectors when the browser zoom level changes.  Avoiding
    // dash calculations on <circle> also gives rings a stable, exact seam.
    const polar = (centerX, centerY, radius, angle) => {
        const radians = angle * Math.PI / 180;
        return { x: centerX + Math.sin(radians) * radius, y: centerY - Math.cos(radians) * radius };
    };
    const arcPath = (centerX, centerY, radius, startAngle, sweepAngle) => {
        const sweep = clamp(sweepAngle, 0, 359.999);
        if (sweep < .01) return "";
        const start = polar(centerX, centerY, radius, startAngle);
        const end = polar(centerX, centerY, radius, startAngle + sweep);
        return `M ${start.x} ${start.y} A ${radius} ${radius} 0 ${sweep > 180 ? 1 : 0} 1 ${end.x} ${end.y}`;
    };
    const ringPath = (centerX, centerY, radius) => {
        const start = polar(centerX, centerY, radius, 0);
        const opposite = polar(centerX, centerY, radius, 180);
        return `M ${start.x} ${start.y} A ${radius} ${radius} 0 1 1 ${opposite.x} ${opposite.y} A ${radius} ${radius} 0 1 1 ${start.x} ${start.y}`;
    };
    const sectorPath = (centerX, centerY, outerRadius, innerRadius, startAngle, sweepAngle) => {
        const sweep = clamp(sweepAngle, 0, 359.999);
        if (sweep < .01) return "";
        const outerStart = polar(centerX, centerY, outerRadius, startAngle);
        const outerEnd = polar(centerX, centerY, outerRadius, startAngle + sweep);
        const large = sweep > 180 ? 1 : 0;
        if (innerRadius <= .5) return `M ${centerX} ${centerY} L ${outerStart.x} ${outerStart.y} A ${outerRadius} ${outerRadius} 0 ${large} 1 ${outerEnd.x} ${outerEnd.y} Z`;
        const innerEnd = polar(centerX, centerY, innerRadius, startAngle + sweep);
        const innerStart = polar(centerX, centerY, innerRadius, startAngle);
        return `M ${outerStart.x} ${outerStart.y} A ${outerRadius} ${outerRadius} 0 ${large} 1 ${outerEnd.x} ${outerEnd.y} L ${innerEnd.x} ${innerEnd.y} A ${innerRadius} ${innerRadius} 0 ${large} 0 ${innerStart.x} ${innerStart.y} Z`;
    };
    const straightPath = points => points.map((point, index) => `${index ? "L" : "M"} ${point.x} ${point.y}`).join(" ");
    const monotonePath = points => {
        if (points.length < 3) return straightPath(points);
        const widths = []; const slopes = [];
        for (let index = 0; index < points.length - 1; index += 1) {
            const width = points[index + 1].x - points[index].x;
            const slope = (points[index + 1].y - points[index].y) / width;
            if (!Number.isFinite(width) || !Number.isFinite(slope) || width <= 0) return straightPath(points);
            widths.push(width); slopes.push(slope);
        }
        const tangents = new Array(points.length).fill(0);
        tangents[0] = slopes[0]; tangents[tangents.length - 1] = slopes.at(-1);
        for (let index = 1; index < points.length - 1; index += 1) {
            const previous = slopes[index - 1]; const next = slopes[index];
            if (!previous || !next || previous * next <= 0) continue;
            const previousWidth = widths[index - 1]; const nextWidth = widths[index];
            const firstWeight = 2 * nextWidth + previousWidth;
            const secondWeight = nextWidth + 2 * previousWidth;
            tangents[index] = (firstWeight + secondWeight) / (firstWeight / previous + secondWeight / next);
            if (!Number.isFinite(tangents[index])) return straightPath(points);
        }
        // Fritsch–Carlson's limiter makes each Hermite segment shape-preserving,
        // including non-uniform x intervals and local extrema.
        for (let index = 0; index < slopes.length; index += 1) {
            const slope = slopes[index];
            if (!slope) { tangents[index] = 0; tangents[index + 1] = 0; continue; }
            let left = tangents[index] / slope; let right = tangents[index + 1] / slope;
            if (!Number.isFinite(left) || !Number.isFinite(right)) return straightPath(points);
            if (left < 0 || right < 0) { tangents[index] = 0; tangents[index + 1] = 0; continue; }
            const magnitude = left * left + right * right;
            if (magnitude > 9) {
                const limiter = 3 / Math.sqrt(magnitude);
                tangents[index] = limiter * left * slope;
                tangents[index + 1] = limiter * right * slope;
            }
        }
        let path = `M ${points[0].x} ${points[0].y}`;
        for (let index = 0; index < points.length - 1; index += 1) {
            const current = points[index]; const next = points[index + 1]; const width = widths[index];
            const first = { x: current.x + width / 3, y: current.y + tangents[index] * width / 3 };
            const second = { x: next.x - width / 3, y: next.y - tangents[index + 1] * width / 3 };
            if (![first.x, first.y, second.x, second.y].every(Number.isFinite)) return straightPath(points);
            path += ` C ${first.x} ${first.y}, ${second.x} ${second.y}, ${next.x} ${next.y}`;
        }
        return path;
    };
    const pathFor = (points, curve) => {
        if (!points.length) return "";
        if (points.length === 1) return `M ${points[0].x} ${points[0].y}`;
        if (curve === "Step") {
            return points.reduce((path, point, index) => index === 0
                ? `M ${point.x} ${point.y}`
                : `${path} H ${point.x} V ${point.y}`, "");
        }
        if (curve === "Monotone") return monotonePath(points);
        if (curve !== "Smooth") return straightPath(points);
        let path = `M ${points[0].x} ${points[0].y}`;
        for (let index = 0; index < points.length - 1; index += 1) {
            const previous = points[index - 1] ?? points[index];
            const current = points[index];
            const next = points[index + 1];
            const after = points[index + 2] ?? next;
            const firstControl = { x: current.x + (next.x - previous.x) / 6, y: current.y + (next.y - previous.y) / 6 };
            const secondControl = { x: next.x - (after.x - current.x) / 6, y: next.y - (after.y - current.y) / 6 };
            path += ` C ${firstControl.x} ${firstControl.y}, ${secondControl.x} ${secondControl.y}, ${next.x} ${next.y}`;
        }
        return path;
    };
    const normalizeSeries = input => Array.isArray(input)
        ? input.filter(series => Array.isArray(series?.points)).map((series, index) => ({
            name: String(series.name || `Series ${index + 1}`),
            color: typeof series.color === "string" && series.color.trim() ? series.color.trim() : null,
            points: series.points.filter(point => point && Number.isFinite(Number(point.value))).map(point => ({
                label: String(point.label ?? ""),
                value: number(point.value),
                x: Number.isFinite(Number(point.x)) ? number(point.x) : null,
                low: Number.isFinite(Number(point.low)) ? number(point.low) : null,
                high: Number.isFinite(Number(point.high)) ? number(point.high) : null,
                size: Number.isFinite(Number(point.size)) ? number(point.size) : null,
                group: point.group == null ? null : String(point.group),
                timestamp: point.timestamp == null || Number.isNaN(Date.parse(point.timestamp)) ? null : String(point.timestamp)
            }))
        }))
        : [];

    class NativeChartController {
        constructor(root, configuration) {
            this.root = root;
            this.configuration = {
                curve: "Smooth", barMode: "Grouped", areaMode: "Overlay", showLegend: true, showGrid: true, showAxes: true, showTooltip: true,
                appearance: { surface: "Card" }, header: { visible: true, showMetric: true }, legend: { visible: true, interactive: true }, tooltip: { visible: true },
                donutInnerRadius: .56, ...configuration
            };
            this.canvas = root.querySelector("[data-z-chart-canvas]");
            this.visual = root.querySelector("[data-z-chart-visual]");
            this.legend = root.querySelector("[data-z-chart-legend]");
            this.metric = root.querySelector("[data-z-chart-metric]");
            this.delta = root.querySelector("[data-z-chart-delta]");
            this.state = root.querySelector("[data-z-chart-state]");
            this.tooltip = root.querySelector("[data-z-chart-tooltip]");
            this.abortController = new AbortController();
            this.hiddenSeries = new Set();
            this.series = normalizeSeries(configuration.series);
            this.lastSize = "";
            this.renderHandle = 0;
            this.root.addEventListener("zoorce:chart:replace", event => {
                this.hiddenSeries.clear();
                this.render(event.detail?.series ?? this.series);
                this.root.dispatchEvent(new CustomEvent("zoorce:chart:updated", { bubbles: true, detail: { series: this.series } }));
            }, { signal: this.abortController.signal });
            this.root.addEventListener("zoorce:chart:refresh", () => this.load(), { signal: this.abortController.signal });
            this.initializeCanvas();
            this.render(this.series);
            this.observeSize();
            this.connectInvalidation();
            if (configuration.binding?.source) this.load();
        }

        initializeCanvas() {
            if (!this.canvas) return;
            const title = this.canvas.querySelector("title") ?? svg("title");
            const description = this.canvas.querySelector("desc") ?? svg("desc");
            this.layers = {
                definitions: svg("defs"),
                grid: svg("g"),
                axis: svg("g"),
                data: svg("g"),
                annotation: svg("g"),
                interaction: svg("g")
            };
            setAttributes(this.layers.grid, { class: "z-native-chart-grid-layer", "aria-hidden": "true" });
            setAttributes(this.layers.axis, { class: "z-native-chart-axis", "aria-hidden": "true" });
            setAttributes(this.layers.interaction, { class: "z-native-chart-interaction" });
            this.canvas.replaceChildren(title, description, this.layers.definitions, this.layers.grid, this.layers.axis, this.layers.data, this.layers.annotation, this.layers.interaction);
        }

        observeSize() {
            if (!this.visual) return;
            const rerender = () => {
                const box = this.visual.getBoundingClientRect();
                const signature = `${Math.round(box.width)}x${Math.round(box.height)}`;
                if (signature === this.lastSize || !box.width || !box.height) return;
                this.lastSize = signature;
                this.scheduleRender();
            };
            if ("ResizeObserver" in window) {
                this.resizeObserver = new ResizeObserver(rerender);
                this.resizeObserver.observe(this.visual);
            } else {
                window.addEventListener("resize", rerender, { signal: this.abortController.signal });
            }
        }

        scheduleRender() {
            if (this.renderHandle) return;
            this.renderHandle = window.requestAnimationFrame(() => {
                this.renderHandle = 0;
                this.render(this.series);
            });
        }

        color(index, series) {
            return series?.color || palette[index % palette.length];
        }

        visibleSeries() {
            return this.series.filter((_, index) => !this.hiddenSeries.has(index));
        }

        clearLayers() {
            Object.values(this.layers ?? {}).forEach(layer => layer.replaceChildren());
        }

        dimensions() {
            const box = (this.visual ?? this.canvas).getBoundingClientRect();
            return {
                width: Math.max(280, Math.round(box.width || 600)),
                height: Math.max(180, Math.round(box.height || 270))
            };
        }

        boundedDomain(values, includeZero = false) {
            let minimum = this.configuration.minimumValue ?? Math.min(...values);
            let maximum = this.configuration.maximumValue ?? Math.max(...values);
            if (includeZero) {
                minimum = Math.min(minimum, 0);
                maximum = Math.max(maximum, 0);
            }
            const spread = maximum - minimum;
            const padding = spread ? spread * 0.1 : Math.max(Math.abs(maximum) * 0.14, 1);
            if (this.configuration.minimumValue == null) minimum -= padding;
            if (this.configuration.maximumValue == null) maximum += padding;
            return { minimum, maximum: maximum === minimum ? minimum + 1 : maximum };
        }

        layout(width, height, horizontalLabels = false, compact = false) {
            if (compact) return { left: 8, right: width - 8, top: 8, bottom: height - 8 };
            return {
                left: horizontalLabels ? Math.min(122, width * 0.34) : Math.min(58, width * 0.17),
                right: width - 18,
                top: 18,
                bottom: height - 37
            };
        }

        render(series) {
            if (!this.canvas || !this.layers) return;
            this.series = normalizeSeries(series);
            this.clearLayers();
            this.hideTooltip();
            const visible = this.visibleSeries().filter(seriesItem => seriesItem.points.length);
            const kind = this.configuration.kind || "Line";
            this.updateLegend();
            this.updateMetadata(visible);
            if (kind === "Candlestick" && Array.isArray(this.configuration.ohlcPoints) && this.configuration.ohlcPoints.length) return this.renderCandlestick(...Object.values(this.dimensions()));
            if (kind === "BoxPlot" && Array.isArray(this.configuration.boxPlotPoints) && this.configuration.boxPlotPoints.length) return this.renderBoxPlot(...Object.values(this.dimensions()));
            if (kind === "Treemap" && Array.isArray(this.configuration.treemapItems) && this.configuration.treemapItems.length) return this.renderTreemap(...Object.values(this.dimensions()));
            if (kind === "Sankey" && Array.isArray(this.configuration.sankeyNodes) && this.configuration.sankeyNodes.length) return this.renderSankey(...Object.values(this.dimensions()));
            if (kind === "Gantt" && Array.isArray(this.configuration.ganttItems) && this.configuration.ganttItems.length) return this.renderGantt(...Object.values(this.dimensions()));
            if (!visible.length) {
                this.setState(this.series.length ? "Select a data series to display it." : "No data is available.");
                return;
            }
            this.setState("");
            const { width, height } = this.dimensions();
            setAttributes(this.canvas, {
                width,
                height,
                viewBox: `0 0 ${width} ${height}`,
                preserveAspectRatio: "xMinYMin meet"
            });
            if (kind === "Donut") return this.renderDonut(visible, width, height);
            if (kind === "Radial") return this.renderRadial(visible, width, height);
            if (kind === "Gauge") return this.renderGauge(visible, width, height);
            if (kind === "Funnel") return this.renderFunnel(visible, width, height);
            if (kind === "Heatmap") return this.renderHeatmap(visible, width, height);
            if (kind === "CalendarHeatmap") return this.renderCalendarHeatmap(visible, width, height);
            if (kind === "Scatter" || kind === "Bubble") return this.renderScatter(visible, width, height, kind === "Bubble");
            if (kind === "Bullet") return this.renderBullet(visible, width, height);
            if (kind === "Histogram") return this.renderHistogram(visible, width, height);
            if (kind === "Waterfall") return this.renderWaterfall(visible, width, height);
            if (kind === "HorizontalBar") return this.renderHorizontalBars(visible, width, height);
            if (kind === "Bar") return this.renderBars(visible, width, height);
            if (kind === "RangeArea") return this.renderRangeArea(visible, width, height);
            if (kind === "Combo") return this.renderCombo(visible, width, height);
            if (kind === "Radar") return this.renderRadar(visible, width, height);
            if (kind === "Lollipop") return this.renderLollipops(visible, width, height);
            if (kind === "Slope") return this.renderSlope(visible, width, height);
            if (kind === "Area" && this.configuration.areaMode !== "Overlay") return this.renderStackedArea(visible, width, height);
            this.renderTrend(visible, width, height, kind === "Area", kind === "Sparkline");
        }

        updateMetadata(series) {
            const primary = series[0];
            const points = primary?.points ?? [];
            const kind = this.configuration.kind || "Line";
            if (!points.length) {
                const specialized = (() => {
                    if (kind === "Candlestick") {
                        const point = this.configuration.ohlcPoints?.at(-1); return point ? { value: number(point.close), label: "Close" } : null;
                    }
                    if (kind === "BoxPlot") {
                        const point = this.configuration.boxPlotPoints?.at(-1); return point ? { value: number(point.median), label: "Median" } : null;
                    }
                    if (kind === "Treemap") return this.configuration.treemapItems?.length ? { value: this.configuration.treemapItems.reduce((sum, item) => sum + number(item.value), 0), label: "Total" } : null;
                    if (kind === "Sankey") return this.configuration.sankeyLinks?.length ? { value: this.configuration.sankeyLinks.reduce((sum, link) => sum + number(link.value), 0), label: "Flow" } : null;
                    if (kind === "Gantt") return this.configuration.ganttItems?.length ? { value: this.configuration.ganttItems.reduce((sum, item) => sum + number(item.progress), 0) / this.configuration.ganttItems.length, label: "Complete" } : null;
                    return null;
                })();
                if (specialized) {
                    if (this.metric) this.metric.textContent = withSuffix(specialized.value, this.configuration.valueSuffix);
                    if (this.delta) { this.delta.textContent = specialized.label; this.delta.classList.remove("is-positive", "is-negative"); }
                    return;
                }
            }
            const slices = kind === "Donut" ? this.distribution(series) : [];
            const leadingSlice = slices.reduce((leading, slice) => !leading || slice.value > leading.value ? slice : leading, null);
            const latest = kind === "Donut" ? leadingSlice?.value : points.at(-1)?.value;
            const previous = points.length > 1 ? points.at(-2)?.value : null;
            const suffix = this.configuration.valueSuffix ?? "";
            if (this.metric) this.metric.textContent = latest == null ? "—" : withSuffix(latest, suffix);
            if (this.delta) {
                if (kind === "Donut" && leadingSlice) {
                    const total = slices.reduce((sum, slice) => sum + slice.value, 0) || 1;
                    this.delta.textContent = `${leadingSlice.name} · ${format(leadingSlice.value / total * 100)}% share`;
                    this.delta.classList.remove("is-positive", "is-negative");
                    return;
                }
                if ((kind === "Gauge" || kind === "Bullet") && this.configuration.targetValue != null && latest != null) {
                    const meetsTarget = latest >= this.configuration.targetValue;
                    this.delta.textContent = `Target ${withSuffix(this.configuration.targetValue, suffix)}`;
                    this.delta.classList.toggle("is-positive", meetsTarget);
                    this.delta.classList.toggle("is-negative", !meetsTarget);
                    return;
                }
                if (kind === "Funnel" && points.length > 1 && latest != null) {
                    const first = points[0].value;
                    const conversion = first ? latest / first * 100 : 0;
                    this.delta.textContent = `${format(conversion)}% converted`;
                    this.delta.classList.toggle("is-positive", conversion >= 50);
                    this.delta.classList.toggle("is-negative", conversion < 50);
                    return;
                }
                const difference = latest == null || previous == null ? null : latest - previous;
                this.delta.textContent = difference == null ? "Awaiting data" : `${difference >= 0 ? "+" : ""}${withSuffix(difference, suffix)} vs. previous`;
                this.delta.classList.toggle("is-positive", difference > 0);
                this.delta.classList.toggle("is-negative", difference < 0);
            }
        }

        updateLegend() {
            if (!this.legend) return;
            this.legend.hidden = !this.configuration.showLegend || !this.configuration.legend?.visible || this.series.length < 2;
            if (this.legend.hidden) {
                this.legend.replaceChildren();
                return;
            }
            const buttons = this.series.map((seriesItem, index) => {
                const button = document.createElement("button");
                button.type = "button";
                button.className = "z-chart-legend-button";
                button.dataset.zChartSeries = String(index);
                button.setAttribute("aria-pressed", String(!this.hiddenSeries.has(index)));
                button.style.setProperty("--z-series-color", this.color(index, seriesItem));
                const swatch = document.createElement("i");
                swatch.setAttribute("aria-hidden", "true");
                button.append(swatch, document.createTextNode(seriesItem.name));
                button.disabled = this.configuration.legend?.interactive === false;
                button.addEventListener("click", () => {
                    if (this.configuration.legend?.interactive === false) return;
                    if (this.hiddenSeries.has(index)) this.hiddenSeries.delete(index);
                    else if (this.hiddenSeries.size < this.series.length - 1) this.hiddenSeries.add(index);
                    this.render(this.series);
                }, { signal: this.abortController.signal });
                return button;
            });
            this.legend.replaceChildren(...buttons);
        }

        renderFrame(labels, domain, plot, toX, toY, options = {}) {
            const { xTicks = labels, xFormatter = value => value, horizontal = false } = options;
            if (this.configuration.showGrid) {
                ticks(domain.minimum, domain.maximum).forEach(value => {
                    const coordinate = horizontal ? toX(value) : toY(value);
                    const line = svg("line");
                    setAttributes(line, horizontal
                        ? { class: "z-native-chart-grid", x1: coordinate, x2: coordinate, y1: plot.top, y2: plot.bottom }
                        : { class: "z-native-chart-grid", x1: plot.left, x2: plot.right, y1: coordinate, y2: coordinate });
                    this.layers.grid.append(line);
                });
            }
            if (!this.configuration.showAxes) return;
            ticks(domain.minimum, domain.maximum).forEach(value => {
                const coordinate = horizontal ? toX(value) : toY(value);
                appendText(this.layers.axis, horizontal ? coordinate : plot.left - 10, horizontal ? plot.bottom + 21 : coordinate + 3.5, format(value), "z-native-chart-axis-label", horizontal ? "middle" : "end");
            });
            const interval = Math.max(1, Math.ceil(xTicks.length / 6));
            xTicks.forEach((label, index) => {
                if (index % interval && index !== xTicks.length - 1) return;
                if (horizontal) {
                    const y = plot.top + ((index + .5) / xTicks.length) * (plot.bottom - plot.top);
                    appendText(this.layers.axis, plot.left - 9, y + 3.5, xFormatter(label), "z-native-chart-axis-label", "end");
                } else {
                    appendText(this.layers.axis, toX(index, xTicks.length), plot.bottom + 21, xFormatter(label), "z-native-chart-axis-label", "middle");
                }
            });
        }

        bindTooltip(element, message, position) {
            if (!this.configuration.showTooltip || this.configuration.tooltip?.visible === false) return;
            element.setAttribute("tabindex", "0");
            element.setAttribute("focusable", "true");
            element.setAttribute("aria-label", message);
            element.addEventListener("pointerenter", event => this.showTooltip(message, event.clientX, event.clientY), { signal: this.abortController.signal });
            element.addEventListener("pointermove", event => this.showTooltip(message, event.clientX, event.clientY), { signal: this.abortController.signal });
            element.addEventListener("pointerleave", () => this.hideTooltip(), { signal: this.abortController.signal });
            element.addEventListener("focus", () => this.showTooltip(message, position.x, position.y, true), { signal: this.abortController.signal });
            element.addEventListener("blur", () => this.hideTooltip(), { signal: this.abortController.signal });
        }

        showTooltip(message, x, y, relative = false) {
            if (!this.tooltip || !this.visual) return;
            const box = this.visual.getBoundingClientRect();
            const localX = relative ? x : x - box.left;
            const localY = relative ? y : y - box.top;
            this.tooltip.textContent = message;
            this.tooltip.hidden = false;
            const tipWidth = Math.min(220, Math.max(112, this.tooltip.offsetWidth));
            this.tooltip.style.left = `${clamp(localX, tipWidth / 2 + 7, box.width - tipWidth / 2 - 7)}px`;
            this.tooltip.style.top = `${clamp(localY, 20, box.height - 16)}px`;
        }

        hideTooltip() {
            if (this.tooltip) this.tooltip.hidden = true;
        }

        addHit(point, message, position) {
            const hit = svg("circle");
            setAttributes(hit, { class: "z-native-chart-hit", cx: point.x, cy: point.y, r: 10 });
            this.bindTooltip(hit, message, position);
            this.layers.interaction.append(hit);
        }

        renderTrend(series, width, height, area, sparkline) {
            const values = series.flatMap(seriesItem => seriesItem.points.map(point => point.value));
            const domain = this.boundedDomain(values);
            const plot = this.layout(width, height, false, sparkline);
            const labels = series[0].points.map(point => point.label);
            const primaryPoints = series[0].points;
            const hasContinuousX = primaryPoints.length > 1 && primaryPoints.every((point, index) => Number.isFinite(point.x) && (!index || point.x > primaryPoints[index - 1].x));
            const xMinimum = hasContinuousX ? primaryPoints[0].x : 0;
            const xMaximum = hasContinuousX ? primaryPoints.at(-1).x : Math.max(primaryPoints.length - 1, 1);
            const toX = (index, count) => count <= 1 ? (plot.left + plot.right) / 2 : hasContinuousX
                ? plot.left + ((primaryPoints[index]?.x ?? xMinimum) - xMinimum) / Math.max(xMaximum - xMinimum, 1) * (plot.right - plot.left)
                : plot.left + index * (plot.right - plot.left) / (count - 1);
            const pointX = (point, index, count) => hasContinuousX && Number.isFinite(point.x)
                ? plot.left + (point.x - xMinimum) / Math.max(xMaximum - xMinimum, 1) * (plot.right - plot.left)
                : toX(index, count);
            const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top);
            if (!sparkline) this.renderFrame(labels, domain, plot, toX, toY);
            series.forEach((seriesItem, seriesIndex) => {
                const color = this.color(seriesIndex, seriesItem);
                const points = seriesItem.points.map((point, index) => ({ x: pointX(point, index, seriesItem.points.length), y: toY(point.value), point }));
                const linePath = pathFor(points, this.configuration.curve);
                if (area) {
                    const id = `z-chart-gradient-${this.root.id.replace(/[^a-z0-9_-]/gi, "")}-${seriesIndex}`;
                    const gradient = svg("linearGradient");
                    setAttributes(gradient, { id, x1: 0, x2: 0, y1: 0, y2: 1 });
                    const top = svg("stop"); const bottom = svg("stop");
                    setAttributes(top, { offset: "0%", "stop-color": color, "stop-opacity": seriesIndex ? ".18" : ".31" });
                    setAttributes(bottom, { offset: "100%", "stop-color": color, "stop-opacity": "0" });
                    gradient.append(top, bottom); this.layers.definitions.append(gradient);
                    const fill = svg("path");
                    setAttributes(fill, { class: "z-native-chart-area", d: `${linePath} L ${points.at(-1).x} ${plot.bottom} L ${points[0].x} ${plot.bottom} Z`, fill: `url(#${id})` });
                    this.layers.data.append(fill);
                }
                const line = svg("path");
                setAttributes(line, { class: "z-native-chart-series", d: linePath });
                setPaint(line, { stroke: color });
                this.layers.data.append(line);
                const last = points.at(-1);
                const marker = svg("circle");
                setAttributes(marker, { class: "z-native-chart-marker", cx: last.x, cy: last.y, r: 3.8 });
                setPaint(marker, { fill: color });
                this.layers.data.append(marker);
                points.forEach(item => this.addHit(item, `${seriesItem.name} · ${item.point.label}: ${withSuffix(item.point.value, this.configuration.valueSuffix)}`, item));
            });
        }

        renderStackedArea(series, width, height) {
            const labels = series[0].points.map(point => point.label);
            const mode = this.configuration.areaMode || "Stacked";
            const totals = labels.map((_, pointIndex) => series.reduce((sum, seriesItem) => sum + Math.max(0, number(seriesItem.points[pointIndex]?.value)), 0));
            const maximums = mode === "Stacked100"
                ? labels.map(() => 100)
                : labels.map((_, pointIndex) => series.reduce((sum, seriesItem) => sum + number(seriesItem.points[pointIndex]?.value), 0));
            const domain = this.boundedDomain([0, ...maximums], true);
            const plot = this.layout(width, height);
            const toX = (index, count) => count <= 1 ? (plot.left + plot.right) / 2 : plot.left + index * (plot.right - plot.left) / (count - 1);
            const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top);
            this.renderFrame(labels, domain, plot, toX, toY);
            const accumulators = labels.map(() => 0);
            series.forEach((seriesItem, seriesIndex) => {
                const lower = []; const upper = [];
                seriesItem.points.forEach((point, index) => {
                    const raw = number(point.value);
                    const value = mode === "Stacked100" ? raw / Math.max(totals[index], 1) * 100 : raw;
                    const start = accumulators[index]; const end = start + value; accumulators[index] = end;
                    lower.push({ x: toX(index, labels.length), y: toY(start), point, value, start });
                    upper.push({ x: toX(index, labels.length), y: toY(end), point, value, start });
                });
                const color = this.color(seriesIndex, seriesItem);
                const area = svg("path");
                const boundary = [...lower].reverse();
                setAttributes(area, { class: "z-native-chart-area z-native-chart-stacked-area", d: `${pathFor(upper, this.configuration.curve)} ${boundary.map(point => `L ${point.x} ${point.y}`).join(" ")} Z` });
                setPaint(area, { fill: color, opacity: ".54" }); this.layers.data.append(area);
                const line = svg("path"); setAttributes(line, { class: "z-native-chart-series", d: pathFor(upper, this.configuration.curve) }); setPaint(line, { stroke: color }); this.layers.data.append(line);
                upper.forEach((point, index) => this.addHit(point, `${seriesItem.name} · ${point.point.label}: ${withSuffix(point.point.value, this.configuration.valueSuffix)}${mode === "Stacked100" ? ` (${format(point.value)}%)` : ""}`, point));
            });
        }

        renderBars(series, width, height) {
            const labels = series[0].points.map(point => point.label);
            const mode = this.configuration.barMode || "Grouped";
            const totals = labels.map((_, pointIndex) => series.reduce((sum, seriesItem) => sum + number(seriesItem.points[pointIndex]?.value), 0));
            const values = mode === "Stacked100" ? [0, 100] : mode === "Stacked" ? totals : series.flatMap(seriesItem => seriesItem.points.map(point => point.value));
            const domain = this.boundedDomain(values, true);
            const plot = this.layout(width, height);
            const toX = (index, count) => plot.left + (index + .5) * (plot.right - plot.left) / Math.max(count, 1);
            const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top);
            this.renderFrame(labels, domain, plot, toX, toY);
            const band = (plot.right - plot.left) / Math.max(labels.length, 1);
            const baseline = toY(0);
            if (mode === "Grouped") {
                const gap = Math.min(5, band * .08);
                const barWidth = Math.max(4, (band * .74 - gap * Math.max(0, series.length - 1)) / series.length);
                series.forEach((seriesItem, seriesIndex) => seriesItem.points.forEach((point, index) => {
                    const valueY = toY(point.value);
                    const x = toX(index, labels.length) - (barWidth * series.length + gap * (series.length - 1)) / 2 + seriesIndex * (barWidth + gap);
                    this.appendBar(x, Math.min(valueY, baseline), barWidth, Math.max(1, Math.abs(baseline - valueY)), this.color(seriesIndex, seriesItem), `${seriesItem.name} · ${point.label}: ${withSuffix(point.value, this.configuration.valueSuffix)}`);
                }));
                return;
            }
            labels.forEach((label, pointIndex) => {
                let positive = 0;
                series.forEach((seriesItem, seriesIndex) => {
                    const raw = number(seriesItem.points[pointIndex]?.value);
                    const value = mode === "Stacked100" ? raw / Math.max(totals[pointIndex], 1) * 100 : raw;
                    const start = positive;
                    positive += value;
                    const y1 = toY(start); const y2 = toY(positive);
                    this.appendBar(toX(pointIndex, labels.length) - band * .31, Math.min(y1, y2), band * .62, Math.max(1, Math.abs(y1 - y2)), this.color(seriesIndex, seriesItem), `${seriesItem.name} · ${label}: ${withSuffix(raw, this.configuration.valueSuffix)}`);
                });
            });
        }

        appendBar(x, y, width, height, color, message) {
            const bar = svg("rect");
            setAttributes(bar, { class: "z-native-chart-bar", x, y, width, height, rx: 4 });
            setPaint(bar, { fill: color });
            this.bindTooltip(bar, message, { x: x + width / 2, y });
            this.layers.data.append(bar);
        }

        renderHorizontalBars(series, width, height) {
            const points = [...series[0].points].sort((left, right) => right.value - left.value);
            const domain = this.boundedDomain(points.map(point => point.value), true);
            const plot = this.layout(width, height, true);
            const toX = value => plot.left + ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.right - plot.left);
            const toY = index => plot.top + index * (plot.bottom - plot.top) / points.length;
            this.renderFrame(points.map(point => point.label), domain, plot, toX, toY, { horizontal: true, xTicks: points.map(point => point.label) });
            const baseline = toX(0);
            points.forEach((point, index) => {
                const y = toY(index) + 4;
                const x = toX(point.value);
                this.appendBar(Math.min(x, baseline), y, Math.max(1, Math.abs(x - baseline)), Math.max(8, (plot.bottom - plot.top) / points.length - 8), this.color(0, series[0]), `${point.label}: ${withSuffix(point.value, this.configuration.valueSuffix)}`);
            });
        }

        distribution(series) {
            if (series.length === 1 && series[0].points.length > 1) return series[0].points.map((point, index) => ({ name: point.label || `Segment ${index + 1}`, value: Math.max(0, point.value), color: this.distributionColor(point, index, series[0]) }));
            return series.map((seriesItem, index) => ({ name: seriesItem.name, value: Math.max(0, seriesItem.points.at(-1)?.value ?? 0), color: this.color(index, seriesItem) }));
        }

        distributionColor(point, index, series) {
            if (series?.color) return series.color;
            const label = String(point.label ?? "").toLocaleLowerCase();
            if (/(healthy|success|complete|passed|resolved)/.test(label)) return "var(--z-success)";
            if (/(watch|warning|pending|attention|at risk)/.test(label)) return "var(--z-warning)";
            if (/(risk|error|failed|critical|blocked)/.test(label)) return "var(--z-danger)";
            return this.color(index, series);
        }

        renderDonut(series, width, height) {
            const slices = this.distribution(series).filter(slice => slice.value > 0);
            const total = slices.reduce((sum, slice) => sum + slice.value, 0) || 1;
            const centerX = width / 2; const centerY = height / 2; const radius = Math.max(48, Math.min(width, height) * .31);
            const innerRadius = radius * clamp(number(this.configuration.donutInnerRadius), 0, .9);
            if (innerRadius > .5) {
                const track = svg("path");
                setAttributes(track, { class: "z-native-chart-ring-track", d: ringPath(centerX, centerY, (radius + innerRadius) / 2) });
                setPaint(track, { "stroke-width": radius - innerRadius });
                this.layers.data.append(track);
            }
            const gap = Math.min(3.2, Math.max(1.3, 10 / Math.max(slices.length, 1)));
            let angle = 0;
            slices.forEach((slice, index) => {
                const share = slice.value / total * 100;
                const sweep = share * 3.6;
                const arc = svg("path");
                setAttributes(arc, { class: "z-native-chart-ring", d: sectorPath(centerX, centerY, radius, innerRadius, angle + gap / 2, Math.max(0, sweep - gap)) });
                setPaint(arc, { fill: slice.color, stroke: "none" });
                this.bindTooltip(arc, `${slice.name}: ${format(slice.value)} (${format(share)}%)`, { x: centerX, y: centerY - radius });
                this.layers.data.append(arc); angle += sweep;
            });
            const leading = slices.reduce((result, slice) => !result || slice.value > result.value ? slice : result, null);
            if (innerRadius > .5) {
                appendText(this.layers.annotation, centerX, centerY - 3, withSuffix(leading?.value ?? 0, this.configuration.valueSuffix), "z-native-chart-ring-value", "middle");
                appendText(this.layers.annotation, centerX, centerY + 17, leading?.name ?? "No data", "z-native-chart-ring-label", "middle");
            }
        }

        renderGauge(series, width, height) {
            const point = series[0].points.at(-1);
            const minimum = this.configuration.minimumValue ?? 0;
            const maximum = this.configuration.maximumValue ?? 100;
            const value = clamp(point?.value ?? 0, minimum, maximum);
            const progress = (value - minimum) / Math.max(maximum - minimum, 1);
            const startAngle = 225; const span = 270;
            const centerX = width / 2; const centerY = height * .54; const radius = Math.max(50, Math.min(width, height) * .3);
            const track = svg("path");
            setAttributes(track, { class: "z-native-chart-gauge-track", d: arcPath(centerX, centerY, radius, startAngle, span) });
            this.layers.data.append(track);
            if (progress > 0) {
                const indicator = svg("path");
                setAttributes(indicator, { class: "z-native-chart-gauge", d: arcPath(centerX, centerY, radius, startAngle, progress * span) });
                setPaint(indicator, { stroke: "var(--z-data-primary)" });
                this.bindTooltip(indicator, `${series[0].name}: ${withSuffix(value, this.configuration.valueSuffix)}`, { x: centerX, y: centerY - radius });
                this.layers.data.append(indicator);
            }
            if (this.configuration.targetValue != null) {
                const targetAngle = startAngle + clamp((this.configuration.targetValue - minimum) / Math.max(maximum - minimum, 1), 0, 1) * span;
                const inside = polar(centerX, centerY, radius - 13, targetAngle);
                const outside = polar(centerX, centerY, radius + 11, targetAngle);
                const marker = svg("line");
                setAttributes(marker, { class: "z-native-chart-target", x1: inside.x, y1: inside.y, x2: outside.x, y2: outside.y });
                this.layers.annotation.append(marker);
            }
            appendText(this.layers.annotation, centerX, centerY - 4, withSuffix(value, this.configuration.valueSuffix), "z-native-chart-ring-value", "middle");
            appendText(this.layers.annotation, centerX, centerY + 17, series[0].name, "z-native-chart-ring-label", "middle");
            appendText(this.layers.annotation, centerX - radius, centerY + radius + 24, format(minimum), "z-native-chart-axis-label", "middle");
            appendText(this.layers.annotation, centerX + radius, centerY + radius + 24, format(maximum), "z-native-chart-axis-label", "middle");
        }

        renderFunnel(series, width, height) {
            const points = series[0].points;
            const center = width / 2;
            const top = 17; const bottom = height - 17;
            const availableHeight = bottom - top;
            const gap = Math.min(12, Math.max(7, height * .035));
            const segmentHeight = Math.max(16, (availableHeight - gap * Math.max(0, points.length - 1)) / points.length);
            const minimumWidth = Math.max(66, width * .14);
            const maximumWidth = width * .64;
            const terminalWidth = Math.max(minimumWidth, maximumWidth * .5);
            // Each slice is cut from one inverted triangle. Gaps separate the
            // stages, while both edges retain the same continuous slope.
            const widthAt = y => maximumWidth - (maximumWidth - terminalWidth) * ((y - top) / availableHeight);
            const stages = points.map((point, index) => {
                const stageTop = top + index * (segmentHeight + gap);
                const stageBottom = stageTop + segmentHeight;
                return {
                    point,
                    top: stageTop,
                    bottom: stageBottom,
                    topWidth: widthAt(stageTop),
                    bottomWidth: widthAt(stageBottom)
                };
            });

            stages.forEach((stage, index) => {
                const polygon = svg("path");
                setAttributes(polygon, { class: "z-native-chart-funnel", d: `M ${center - stage.topWidth / 2} ${stage.top} H ${center + stage.topWidth / 2} L ${center + stage.bottomWidth / 2} ${stage.bottom} H ${center - stage.bottomWidth / 2} Z` });
                setPaint(polygon, { fill: `var(--z-funnel-stage-${index % 4 + 1})` });
                this.bindTooltip(polygon, `${stage.point.label}: ${withSuffix(stage.point.value, this.configuration.valueSuffix)}`, { x: center, y: stage.top + segmentHeight / 2 });
                this.layers.data.append(polygon);
                appendText(this.layers.annotation, center, stage.top + segmentHeight / 2 + 4, `${stage.point.label} · ${format(stage.point.value)}`, "z-native-chart-funnel-label", "middle");
            });
        }

        renderHeatmap(series, width, height) {
            const points = series.flatMap(seriesItem => seriesItem.points);
            const rows = unique(points.map(point => point.group || series[0].name));
            const columns = unique(points.map(point => point.label));
            const values = points.map(point => point.value);
            const minimum = Math.min(...values); const maximum = Math.max(...values);
            const plot = { left: Math.min(70, width * .22), right: width - 16, top: 23, bottom: height - 31 };
            const cellWidth = (plot.right - plot.left) / Math.max(columns.length, 1);
            const cellHeight = (plot.bottom - plot.top) / Math.max(rows.length, 1);
            rows.forEach((row, rowIndex) => appendText(this.layers.axis, plot.left - 9, plot.top + (rowIndex + .5) * cellHeight + 3.5, row, "z-native-chart-axis-label", "end"));
            columns.forEach((column, columnIndex) => appendText(this.layers.axis, plot.left + (columnIndex + .5) * cellWidth, plot.bottom + 19, column, "z-native-chart-axis-label", "middle"));
            points.forEach(point => {
                const row = rows.indexOf(point.group || series[0].name);
                const column = columns.indexOf(point.label);
                const intensity = .16 + .84 * (point.value - minimum) / Math.max(maximum - minimum, 1);
                const cell = svg("rect");
                setAttributes(cell, { class: "z-native-chart-heat-cell", x: plot.left + column * cellWidth + 2, y: plot.top + row * cellHeight + 2, width: Math.max(1, cellWidth - 4), height: Math.max(1, cellHeight - 4), rx: 5, opacity: intensity });
                setPaint(cell, { fill: "var(--z-data-primary)" });
                this.bindTooltip(cell, `${point.group || series[0].name} · ${point.label}: ${withSuffix(point.value, this.configuration.valueSuffix)}`, { x: plot.left + (column + .5) * cellWidth, y: plot.top + (row + .5) * cellHeight });
                this.layers.data.append(cell);
            });
        }

        renderScatter(series, width, height, bubble) {
            const points = series.flatMap((seriesItem, seriesIndex) => seriesItem.points.map((point, pointIndex) => ({ point, seriesItem, seriesIndex, pointIndex, x: point.x ?? pointIndex + 1 })));
            const xDomain = this.boundedDomain(points.map(item => item.x));
            const yDomain = this.boundedDomain(points.map(item => item.point.value));
            const plot = this.layout(width, height);
            const toX = value => plot.left + ((value - xDomain.minimum) / (xDomain.maximum - xDomain.minimum)) * (plot.right - plot.left);
            const toY = value => plot.bottom - ((value - yDomain.minimum) / (yDomain.maximum - yDomain.minimum)) * (plot.bottom - plot.top);
            this.renderFrame([], yDomain, plot, () => 0, toY);
            if (this.configuration.showGrid) ticks(xDomain.minimum, xDomain.maximum).forEach(value => {
                const x = toX(value); const line = svg("line");
                setAttributes(line, { class: "z-native-chart-grid", x1: x, x2: x, y1: plot.top, y2: plot.bottom }); this.layers.grid.append(line);
                appendText(this.layers.axis, x, plot.bottom + 21, format(value), "z-native-chart-axis-label", "middle");
            });
            points.forEach(item => {
                const radius = bubble ? clamp(item.point.size ?? 9, 5, 26) : 6;
                const dot = svg("circle");
                setAttributes(dot, { class: bubble ? "z-native-chart-bubble" : "z-native-chart-scatter", cx: toX(item.x), cy: toY(item.point.value), r: radius });
                setPaint(dot, { fill: this.color(item.seriesIndex, item.seriesItem) });
                this.bindTooltip(dot, `${item.seriesItem.name} · ${item.point.label}: ${withSuffix(item.point.value, this.configuration.valueSuffix)} at ${format(item.x)}`, { x: toX(item.x), y: toY(item.point.value) });
                this.layers.data.append(dot);
            });
        }

        renderBullet(series, width, height) {
            const point = series[0].points.at(-1);
            const minimum = this.configuration.minimumValue ?? 0;
            const maximum = this.configuration.maximumValue ?? Math.max(point?.high ?? 0, point?.value ?? 0, this.configuration.targetValue ?? 0, 100);
            const plot = { left: 24, right: width - 24, top: height * .36, bottom: height * .64 };
            const toX = value => plot.left + (clamp(value, minimum, maximum) - minimum) / Math.max(maximum - minimum, 1) * (plot.right - plot.left);
            const ranges = [0.55, 0.8, 1];
            ranges.forEach((share, index) => {
                const range = svg("rect");
                const prior = index ? ranges[index - 1] : 0;
                setAttributes(range, { class: "z-native-chart-bullet-range", x: toX(minimum + (maximum - minimum) * prior), y: plot.top, width: toX(minimum + (maximum - minimum) * share) - toX(minimum + (maximum - minimum) * prior), height: plot.bottom - plot.top, rx: index === 0 ? 7 : 0 });
                this.layers.data.append(range);
            });
            const actual = svg("rect");
            setAttributes(actual, { class: "z-native-chart-bullet-value", x: plot.left, y: plot.top + 8, width: Math.max(1, toX(point?.value ?? minimum) - plot.left), height: plot.bottom - plot.top - 16, rx: 4 });
            setPaint(actual, { fill: this.color(0, series[0]) });
            this.bindTooltip(actual, `${series[0].name}: ${withSuffix(point?.value ?? 0, this.configuration.valueSuffix)}`, { x: toX(point?.value ?? minimum), y: plot.top });
            this.layers.data.append(actual);
            if (this.configuration.targetValue != null) {
                const target = svg("line"); const x = toX(this.configuration.targetValue);
                setAttributes(target, { class: "z-native-chart-target", x1: x, x2: x, y1: plot.top - 6, y2: plot.bottom + 6 }); this.layers.annotation.append(target);
            }
            appendText(this.layers.annotation, plot.left, plot.bottom + 25, format(minimum), "z-native-chart-axis-label");
            appendText(this.layers.annotation, plot.right, plot.bottom + 25, format(maximum), "z-native-chart-axis-label", "end");
        }

        renderHistogram(series, width, height) {
            const points = series[0].points;
            const domain = this.boundedDomain(points.map(point => point.value), true);
            const plot = this.layout(width, height);
            const toX = (index, count) => plot.left + (index + .5) * (plot.right - plot.left) / count;
            const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top);
            this.renderFrame(points.map(point => point.label), domain, plot, toX, toY);
            const widthPerBar = (plot.right - plot.left) / points.length;
            const baseline = toY(0);
            points.forEach((point, index) => this.appendBar(plot.left + index * widthPerBar + 1, Math.min(toY(point.value), baseline), Math.max(1, widthPerBar - 2), Math.max(1, Math.abs(baseline - toY(point.value))), this.color(0, series[0]), `${point.label}: ${format(point.value)}`));
        }

        renderWaterfall(series, width, height) {
            const points = series[0].points;
            let total = 0;
            const steps = points.map(point => {
                const start = total; total += point.value; return { point, start, end: total };
            });
            const domain = this.boundedDomain([0, ...steps.flatMap(step => [step.start, step.end])], true);
            const plot = this.layout(width, height);
            const toX = (index, count) => plot.left + (index + .5) * (plot.right - plot.left) / count;
            const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top);
            this.renderFrame(points.map(point => point.label), domain, plot, toX, toY);
            const barWidth = (plot.right - plot.left) / points.length * .6;
            steps.forEach((step, index) => {
                const x = toX(index, steps.length) - barWidth / 2;
                const y = Math.min(toY(step.start), toY(step.end));
                const color = step.point.value >= 0 ? "var(--z-success)" : "var(--z-danger)";
                this.appendBar(x, y, barWidth, Math.max(1, Math.abs(toY(step.start) - toY(step.end))), color, `${step.point.label}: ${step.point.value >= 0 ? "+" : ""}${withSuffix(step.point.value, this.configuration.valueSuffix)}`);
                if (index < steps.length - 1) {
                    const connector = svg("line");
                    setAttributes(connector, { class: "z-native-chart-waterfall-connector", x1: x + barWidth, x2: toX(index + 1, steps.length) - barWidth / 2, y1: toY(step.end), y2: toY(step.end) }); this.layers.data.append(connector);
                }
            });
        }

        renderRangeArea(series, width, height) {
            const points = series[0].points.filter(point => point.low != null && point.high != null);
            if (points.length < 2) return this.renderTrend(series, width, height, true, false);
            const domain = this.boundedDomain(points.flatMap(point => [point.low, point.high]));
            const plot = this.layout(width, height);
            const labels = points.map(point => point.label);
            const toX = (index, count) => count <= 1 ? (plot.left + plot.right) / 2 : plot.left + index * (plot.right - plot.left) / (count - 1);
            const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top);
            this.renderFrame(labels, domain, plot, toX, toY);
            const upper = points.map((point, index) => ({ x: toX(index, points.length), y: toY(point.high), point }));
            const lower = points.map((point, index) => ({ x: toX(index, points.length), y: toY(point.low), point }));
            const color = this.color(0, series[0]);
            const fill = svg("path");
            setAttributes(fill, { class: "z-native-chart-range-area", d: `${pathFor(upper, this.configuration.curve)} L ${lower.at(-1).x} ${lower.at(-1).y} ${[...lower].reverse().slice(1).map(point => `L ${point.x} ${point.y}`).join(" ")} Z` });
            setPaint(fill, { fill: color }); this.layers.data.append(fill);
            [upper, lower].forEach((edge, edgeIndex) => {
                const line = svg("path"); setAttributes(line, { class: "z-native-chart-series", d: pathFor(edge, this.configuration.curve) }); setPaint(line, { stroke: color, "stroke-opacity": edgeIndex ? ".62" : "1" }); this.layers.data.append(line);
            });
            upper.forEach((point, index) => this.addHit(point, `${series[0].name} · ${point.point.label}: ${withSuffix(point.point.low, this.configuration.valueSuffix)} – ${withSuffix(point.point.high, this.configuration.valueSuffix)}`, point));
        }

        renderCombo(series, width, height) {
            const labels = series[0].points.map(point => point.label);
            const primary = series.filter(item => item.axis !== "Secondary"); const secondary = series.filter(item => item.axis === "Secondary");
            const primaryDomain = this.boundedDomain((primary.length ? primary : series).flatMap(item => item.points.map(point => point.value)), true);
            const secondaryDomain = secondary.length ? this.boundedDomain(secondary.flatMap(item => item.points.map(point => point.value)), true) : null;
            const plot = this.layout(width, height);
            const toX = (index, count) => plot.left + (index + .5) * (plot.right - plot.left) / Math.max(count, 1);
            const toY = (value, axis = "Primary") => {
                const domain = axis === "Secondary" && secondaryDomain ? secondaryDomain : primaryDomain;
                return plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top);
            };
            this.renderFrame(labels, primaryDomain, plot, toX, value => toY(value));
            if (secondaryDomain && this.configuration.showAxes) ticks(secondaryDomain.minimum, secondaryDomain.maximum).forEach(value => appendText(this.layers.axis, plot.right + 6, toY(value, "Secondary") + 3.5, format(value), "z-native-chart-axis-label"));
            const barSeries = series.filter(item => item.renderAs === "Bar"); const barWidth = Math.max(5, (plot.right - plot.left) / Math.max(labels.length, 1) * .52 / Math.max(barSeries.length, 1));
            series.forEach((seriesItem, seriesIndex) => {
                const renderAs = seriesItem.renderAs === "Auto" ? "Line" : seriesItem.renderAs;
                const color = this.color(seriesIndex, seriesItem);
                const points = seriesItem.points.map((point, index) => ({ x: toX(index, labels.length), y: toY(point.value, seriesItem.axis), point }));
                if (renderAs === "Bar") {
                    const order = barSeries.indexOf(seriesItem); const baseline = toY(0, seriesItem.axis);
                    points.forEach((point, index) => this.appendBar(point.x - barWidth * barSeries.length / 2 + order * barWidth, Math.min(point.y, baseline), barWidth - 2, Math.max(1, Math.abs(baseline - point.y)), color, `${seriesItem.name} · ${point.point.label}: ${withSuffix(point.point.value, this.configuration.valueSuffix)}`));
                    return;
                }
                if (renderAs === "Area") {
                    const area = svg("path"); setAttributes(area, { class: "z-native-chart-area", d: `${pathFor(points, this.configuration.curve)} L ${points.at(-1).x} ${toY(0, seriesItem.axis)} L ${points[0].x} ${toY(0, seriesItem.axis)} Z` }); setPaint(area, { fill: color, opacity: ".2" }); this.layers.data.append(area);
                }
                const line = svg("path"); setAttributes(line, { class: "z-native-chart-series", d: pathFor(points, this.configuration.curve) }); setPaint(line, { stroke: color }); this.layers.data.append(line);
                points.forEach(point => this.addHit(point, `${seriesItem.name} · ${point.point.label}: ${withSuffix(point.point.value, this.configuration.valueSuffix)}`, point));
            });
        }

        renderRadar(series, width, height) {
            const labels = series[0].points.map(point => point.label); const count = labels.length;
            const maximum = this.configuration.maximumValue ?? Math.max(...series.flatMap(item => item.points.map(point => point.value)), 1);
            const minimum = this.configuration.minimumValue ?? 0; const centerX = width / 2; const centerY = height / 2; const radius = Math.max(38, Math.min(width, height) * .31);
            const vertex = (index, value = 1) => polar(centerX, centerY, radius * value, index * 360 / Math.max(count, 1));
            [1, .75, .5, .25].forEach(scale => {
                const vertices = labels.map((_, index) => vertex(index, scale)); const polygon = svg("polygon"); setAttributes(polygon, { class: "z-native-chart-radar-grid", points: vertices.map(point => `${point.x},${point.y}`).join(" ") }); this.layers.grid.append(polygon);
            });
            labels.forEach((label, index) => { const edge = vertex(index, 1.13); appendText(this.layers.axis, edge.x, edge.y + 3, label, "z-native-chart-axis-label", "middle"); });
            series.forEach((seriesItem, seriesIndex) => {
                const vertices = seriesItem.points.map((point, index) => vertex(index, clamp((point.value - minimum) / Math.max(maximum - minimum, 1), 0, 1)));
                const polygon = svg("polygon"); setAttributes(polygon, { class: "z-native-chart-radar", points: vertices.map(point => `${point.x},${point.y}`).join(" ") }); setPaint(polygon, { fill: this.color(seriesIndex, seriesItem), stroke: this.color(seriesIndex, seriesItem) }); this.layers.data.append(polygon);
                vertices.forEach((point, index) => this.addHit(point, `${seriesItem.name} · ${seriesItem.points[index].label}: ${withSuffix(seriesItem.points[index].value, this.configuration.valueSuffix)}`, point));
            });
        }

        renderRadial(series, width, height) {
            const items = this.distribution(series).filter(item => item.value >= 0); const maximum = this.configuration.maximumValue ?? Math.max(...items.map(item => item.value), 1);
            const centerX = width / 2; const centerY = height / 2; const outer = Math.min(width, height) * .34; const thickness = Math.max(10, Math.min(21, outer / Math.max(items.length, 1) * .76));
            items.forEach((item, index) => {
                const radius = outer - index * (thickness + 7); if (radius < thickness) return;
                const track = svg("path"); setAttributes(track, { class: "z-native-chart-ring-track", d: ringPath(centerX, centerY, radius) }); setPaint(track, { "stroke-width": thickness }); this.layers.data.append(track);
                const ring = svg("path"); setAttributes(ring, { class: "z-native-chart-ring", d: arcPath(centerX, centerY, radius, 0, item.value / maximum * 360) }); setPaint(ring, { stroke: item.color, "stroke-width": thickness }); this.bindTooltip(ring, `${item.name}: ${withSuffix(item.value, this.configuration.valueSuffix)}`, { x: centerX, y: centerY - radius }); this.layers.data.append(ring);
            });
            appendText(this.layers.annotation, centerX, centerY - 2, withSuffix(items.reduce((sum, item) => sum + item.value, 0), this.configuration.valueSuffix), "z-native-chart-ring-value", "middle");
            appendText(this.layers.annotation, centerX, centerY + 18, "Total", "z-native-chart-ring-label", "middle");
        }

        renderCalendarHeatmap(series, width, height) {
            const points = series.flatMap(item => item.points).filter(point => point.timestamp);
            if (!points.length) return this.renderHeatmap(series, width, height);
            const values = points.map(point => point.value); const minimum = Math.min(...values); const maximum = Math.max(...values);
            const ordered = [...points].sort((left, right) => Date.parse(left.timestamp) - Date.parse(right.timestamp)); const weeks = Math.max(1, Math.ceil(ordered.length / 7));
            const left = 34; const top = 24; const cellWidth = (width - left - 12) / weeks; const cellHeight = Math.max(12, (height - top - 12) / 7);
            ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].forEach((day, index) => appendText(this.layers.axis, left - 7, top + (index + .5) * cellHeight + 3, day, "z-native-chart-axis-label", "end"));
            ordered.forEach((point, index) => {
                const date = new Date(point.timestamp); const week = Math.floor(index / 7); const day = date.getUTCDay(); const cell = svg("rect");
                setAttributes(cell, { class: "z-native-chart-heat-cell", x: left + week * cellWidth + 2, y: top + day * cellHeight + 2, width: Math.max(2, cellWidth - 4), height: Math.max(2, cellHeight - 4), rx: 3, opacity: .16 + .84 * (point.value - minimum) / Math.max(maximum - minimum, 1) }); setPaint(cell, { fill: "var(--z-data-primary)" }); this.bindTooltip(cell, `${point.label}: ${withSuffix(point.value, this.configuration.valueSuffix)}`, { x: left + week * cellWidth, y: top + day * cellHeight }); this.layers.data.append(cell);
            });
        }

        renderLollipops(series, width, height) {
            const points = series[0].points; const domain = this.boundedDomain(points.map(point => point.value), true); const plot = this.layout(width, height); const toX = (index, count) => plot.left + (index + .5) * (plot.right - plot.left) / Math.max(count, 1); const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top); this.renderFrame(points.map(point => point.label), domain, plot, toX, toY); const baseline = toY(0);
            points.forEach((point, index) => { const x = toX(index, points.length); const y = toY(point.value); const stem = svg("line"); setAttributes(stem, { class: "z-native-chart-lollipop-stem", x1: x, x2: x, y1: baseline, y2: y }); this.layers.data.append(stem); const head = svg("circle"); setAttributes(head, { class: "z-native-chart-lollipop", cx: x, cy: y, r: 6 }); setPaint(head, { fill: this.color(0, series[0]) }); this.bindTooltip(head, `${point.label}: ${withSuffix(point.value, this.configuration.valueSuffix)}`, { x, y }); this.layers.data.append(head); });
        }

        renderSlope(series, width, height) {
            const all = series.flatMap(item => item.points.slice(0, 2).map(point => point.value)); const domain = this.boundedDomain(all); const plot = this.layout(width, height); const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top); const left = plot.left + 45; const right = plot.right - 45;
            [left, right].forEach((x, index) => appendText(this.layers.axis, x, plot.bottom + 21, series[0].points[index]?.label ?? (index ? "End" : "Start"), "z-native-chart-axis-label", "middle"));
            series.forEach((seriesItem, index) => { const first = seriesItem.points[0]; const last = seriesItem.points[1]; if (!first || !last) return; const line = svg("line"); setAttributes(line, { class: "z-native-chart-series", x1: left, y1: toY(first.value), x2: right, y2: toY(last.value) }); setPaint(line, { stroke: this.color(index, seriesItem) }); this.layers.data.append(line); [ { x: left, y: toY(first.value), point: first }, { x: right, y: toY(last.value), point: last } ].forEach(point => this.addHit(point, `${seriesItem.name}: ${withSuffix(point.point.value, this.configuration.valueSuffix)}`, point)); });
        }

        renderCandlestick(width, height) {
            const points = this.configuration.ohlcPoints.filter(point => [point.open, point.high, point.low, point.close].every(value => Number.isFinite(Number(value)))); if (!points.length) return;
            const domain = this.boundedDomain(points.flatMap(point => [number(point.low), number(point.high)])); const plot = this.layout(width, height); const toX = (index, count) => plot.left + (index + .5) * (plot.right - plot.left) / Math.max(count, 1); const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top); this.renderFrame(points.map(point => point.label), domain, plot, toX, toY); const bodyWidth = Math.max(4, (plot.right - plot.left) / points.length * .52);
            points.forEach((point, index) => { const x = toX(index, points.length); const rising = number(point.close) >= number(point.open); const color = rising ? "var(--z-success)" : "var(--z-danger)"; const wick = svg("line"); setAttributes(wick, { class: "z-native-chart-candle-wick", x1: x, x2: x, y1: toY(number(point.high)), y2: toY(number(point.low)) }); setPaint(wick, { stroke: color }); this.layers.data.append(wick); const body = svg("rect"); setAttributes(body, { class: "z-native-chart-candle", x: x - bodyWidth / 2, y: Math.min(toY(number(point.open)), toY(number(point.close))), width: bodyWidth, height: Math.max(2, Math.abs(toY(number(point.open)) - toY(number(point.close)))), rx: 2 }); setPaint(body, { fill: color }); this.bindTooltip(body, `${point.label}: O ${format(number(point.open))} · H ${format(number(point.high))} · L ${format(number(point.low))} · C ${format(number(point.close))}`, { x, y: toY(number(point.high)) }); this.layers.data.append(body); });
        }

        renderBoxPlot(width, height) {
            const points = this.configuration.boxPlotPoints.filter(point => [point.minimum, point.firstQuartile, point.median, point.thirdQuartile, point.maximum].every(value => Number.isFinite(Number(value)))); if (!points.length) return;
            const domain = this.boundedDomain(points.flatMap(point => [number(point.minimum), number(point.maximum), ...(point.outliers ?? []).map(number)])); const plot = this.layout(width, height); const toX = (index, count) => plot.left + (index + .5) * (plot.right - plot.left) / Math.max(count, 1); const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top); this.renderFrame(points.map(point => point.label), domain, plot, toX, toY); const size = Math.max(9, (plot.right - plot.left) / points.length * .45);
            points.forEach((point, index) => { const x = toX(index, points.length); const color = this.color(index, null); const whisker = svg("line"); setAttributes(whisker, { class: "z-native-chart-box-whisker", x1: x, x2: x, y1: toY(number(point.maximum)), y2: toY(number(point.minimum)) }); this.layers.data.append(whisker); const box = svg("rect"); setAttributes(box, { class: "z-native-chart-box", x: x - size / 2, y: toY(number(point.thirdQuartile)), width: size, height: Math.max(2, toY(number(point.firstQuartile)) - toY(number(point.thirdQuartile))), rx: 3 }); setPaint(box, { fill: color }); this.bindTooltip(box, `${point.label}: min ${format(number(point.minimum))} · Q1 ${format(number(point.firstQuartile))} · median ${format(number(point.median))} · Q3 ${format(number(point.thirdQuartile))} · max ${format(number(point.maximum))}`, { x, y: toY(number(point.median)) }); this.layers.data.append(box); const median = svg("line"); setAttributes(median, { class: "z-native-chart-box-median", x1: x - size / 2, x2: x + size / 2, y1: toY(number(point.median)), y2: toY(number(point.median)) }); this.layers.data.append(median); (point.outliers ?? []).forEach(value => { const outlier = svg("circle"); setAttributes(outlier, { class: "z-native-chart-box-outlier", cx: x, cy: toY(number(value)), r: 3 }); this.layers.data.append(outlier); }); });
        }

        renderTreemap(width, height) {
            const items = this.configuration.treemapItems.filter(item => Number(item.value) > 0); if (!items.length) return; const total = items.reduce((sum, item) => sum + number(item.value), 0); const bounds = { x: 8, y: 12, width: width - 16, height: height - 24 }; let offset = 0;
            items.forEach((item, index) => { const vertical = bounds.width >= bounds.height; const share = number(item.value) / total; const span = (vertical ? bounds.width : bounds.height) * share; const x = vertical ? bounds.x + offset : bounds.x; const y = vertical ? bounds.y : bounds.y + offset; const itemWidth = vertical ? span : bounds.width; const itemHeight = vertical ? bounds.height : span; offset += span; const tile = svg("rect"); setAttributes(tile, { class: "z-native-chart-treemap", x: x + 2, y: y + 2, width: Math.max(1, itemWidth - 4), height: Math.max(1, itemHeight - 4), rx: 6 }); setPaint(tile, { fill: item.color || this.color(index, null) }); this.bindTooltip(tile, `${item.label}: ${withSuffix(number(item.value), this.configuration.valueSuffix)}`, { x: x + itemWidth / 2, y: y + itemHeight / 2 }); this.layers.data.append(tile); if (itemWidth > 70 && itemHeight > 34) appendText(this.layers.annotation, x + 11, y + 21, item.label, "z-native-chart-treemap-label"); });
        }

        renderSankey(width, height) {
            const nodes = this.configuration.sankeyNodes ?? []; const links = (this.configuration.sankeyLinks ?? []).filter(link => Number(link.value) > 0); if (!nodes.length || !links.length) return;
            const ids = new Set(nodes.map(node => node.id)); const incoming = new Set(links.map(link => link.targetId)); const outgoing = new Set(links.map(link => link.sourceId)); const sources = nodes.filter(node => outgoing.has(node.id) && !incoming.has(node.id)); const targets = nodes.filter(node => incoming.has(node.id) && !outgoing.has(node.id)); const middle = nodes.filter(node => !sources.includes(node) && !targets.includes(node)); const columns = [sources, middle, targets].filter(column => column.length); const positions = new Map();
            columns.forEach((column, columnIndex) => column.forEach((node, index) => positions.set(node.id, { x: 22 + columnIndex * (width - 92) / Math.max(columns.length - 1, 1), y: 18 + index * (height - 44) / Math.max(column.length, 1), width: 18, height: Math.max(24, (height - 52) / Math.max(column.length, 1) * .54), node })));
            links.forEach((link, index) => { const source = positions.get(link.sourceId); const target = positions.get(link.targetId); if (!source || !target) return; const thickness = clamp(Math.sqrt(number(link.value)) * 2.5, 3, 22); const path = svg("path"); setAttributes(path, { class: "z-native-chart-sankey-link", d: `M ${source.x + source.width} ${source.y + source.height / 2} C ${(source.x + target.x) / 2} ${source.y + source.height / 2}, ${(source.x + target.x) / 2} ${target.y + target.height / 2}, ${target.x} ${target.y + target.height / 2}` }); setPaint(path, { stroke: link.color || this.color(index, null), "stroke-width": thickness }); this.bindTooltip(path, `${link.label || `${source.node.label} → ${target.node.label}`}: ${withSuffix(number(link.value), this.configuration.valueSuffix)}`, { x: (source.x + target.x) / 2, y: (source.y + target.y) / 2 }); this.layers.data.append(path); });
            [...positions.values()].forEach((position, index) => { const node = svg("rect"); setAttributes(node, { class: "z-native-chart-sankey-node", x: position.x, y: position.y, width: position.width, height: position.height, rx: 4 }); setPaint(node, { fill: position.node.color || this.color(index, null) }); this.layers.data.append(node); appendText(this.layers.annotation, position.x + (position.x < width / 2 ? position.width + 6 : -6), position.y + position.height / 2 + 4, position.node.label, "z-native-chart-sankey-label", position.x < width / 2 ? "start" : "end"); });
        }

        renderGantt(width, height) {
            const items = this.configuration.ganttItems.filter(item => Date.parse(item.start) && Date.parse(item.end)); if (!items.length) return; const start = Math.min(...items.map(item => Date.parse(item.start))); const end = Math.max(...items.map(item => Date.parse(item.end))); const plot = { left: Math.min(130, width * .32), right: width - 16, top: 24, bottom: height - 14 }; const rowHeight = (plot.bottom - plot.top) / items.length; const toX = value => plot.left + (value - start) / Math.max(end - start, 1) * (plot.right - plot.left);
            ticks(start, end, 4).forEach(value => { const x = toX(value); const grid = svg("line"); setAttributes(grid, { class: "z-native-chart-grid", x1: x, x2: x, y1: plot.top, y2: plot.bottom }); this.layers.grid.append(grid); appendText(this.layers.axis, x, plot.top - 7, new Date(value).toLocaleDateString(undefined, { month: "short", day: "numeric" }), "z-native-chart-axis-label", "middle"); });
            items.forEach((item, index) => { const y = plot.top + index * rowHeight + Math.max(3, rowHeight * .18); const bar = svg("rect"); const x = toX(Date.parse(item.start)); const itemWidth = Math.max(2, toX(Date.parse(item.end)) - x); setAttributes(bar, { class: "z-native-chart-gantt", x, y, width: itemWidth, height: Math.max(8, rowHeight * .62), rx: 4 }); setPaint(bar, { fill: item.color || this.color(index, null) }); this.bindTooltip(bar, `${item.label}: ${new Date(item.start).toLocaleDateString()} – ${new Date(item.end).toLocaleDateString()} · ${format(number(item.progress))}%`, { x: x + itemWidth / 2, y }); this.layers.data.append(bar); if (number(item.progress) > 0) { const progress = svg("rect"); setAttributes(progress, { class: "z-native-chart-gantt-progress", x, y, width: itemWidth * clamp(number(item.progress), 0, 100) / 100, height: Math.max(8, rowHeight * .62), rx: 4 }); this.layers.data.append(progress); } appendText(this.layers.axis, plot.left - 9, y + rowHeight * .39, item.label, "z-native-chart-axis-label", "end"); });
        }

        async load() {
            try {
                this.root.setAttribute("aria-busy", "true"); this.setState("Refreshing…");
                const body = await window.ZoorceData.request({ source: this.configuration.binding?.source, signal: this.abortController.signal });
                this.render(body.series ?? body); this.setState("");
                this.root.dispatchEvent(new CustomEvent("zoorce:chart:loaded", { bubbles: true, detail: { series: body.series ?? body } }));
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.setState("Unable to refresh chart.");
                    this.root.dispatchEvent(new CustomEvent("zoorce:chart:error", { bubbles: true, detail: { error } }));
                }
            } finally { this.root.removeAttribute("aria-busy"); }
        }

        connectInvalidation() {
            if (!this.configuration.binding?.invalidationScope || !window.ZoorceData?.subscribe) return;
            this.unsubscribeRealtime = window.ZoorceData.subscribe(this.configuration.binding.invalidationScope, invalidation => {
                const update = invalidation?.change;
                if (update?.series) this.render(update.series);
                else this.load();
                this.root.dispatchEvent(new CustomEvent("zoorce:chart:realtime", { bubbles: true, detail: { update } }));
            });
        }

        setState(message) { if (this.state) this.state.textContent = message; }

        dispose() {
            this.abortController.abort(); this.resizeObserver?.disconnect(); this.unsubscribeRealtime?.();
            if (this.renderHandle) window.cancelAnimationFrame(this.renderHandle);
        }
    }

    const parseConfiguration = root => {
        try { return JSON.parse(root.querySelector("[data-z-chart-config]")?.textContent ?? "null"); }
        catch { return null; }
    };
    const enhancedMount = (scope = document) => {
        const roots = scope.matches?.("[data-z-chart]") ? [scope] : [...(scope.querySelectorAll?.("[data-z-chart]") ?? [])];
        roots.forEach(root => {
            if (controllers.has(root)) return;
            const configuration = parseConfiguration(root);
            if (!configuration) return;
            controllers.set(root, new NativeChartController(root, configuration));
        });
    };
    const enhancedDispose = root => {
        controllers.get(root)?.dispose(); controllers.delete(root);
    };

    enhancedMount();
    window.ZoorceCharts = Object.freeze({ mount: enhancedMount, dispose: enhancedDispose });
})();
