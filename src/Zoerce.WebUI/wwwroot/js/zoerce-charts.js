(() => {
    "use strict";

    const legacyComponents = window.ZoorceComponents;
    if (!legacyComponents) return;

    const svgNamespace = "http://www.w3.org/2000/svg";
    const palette = ["var(--z-data-primary)", "var(--z-data-secondary)", "var(--z-data-tertiary)", "var(--z-data-quaternary)", "var(--z-data-quinary)"];
    const controllers = new WeakMap();
    const chartKindsWithoutAxes = new Set(["Donut", "Gauge", "Funnel", "Heatmap", "Bullet"]);
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
    const pathFor = (points, curve) => {
        if (!points.length) return "";
        if (points.length === 1) return `M ${points[0].x} ${points[0].y}`;
        if (curve === "Step") {
            return points.reduce((path, point, index) => index === 0
                ? `M ${point.x} ${point.y}`
                : `${path} H ${point.x} V ${point.y}`, "");
        }
        if (curve !== "Smooth") return points.map((point, index) => `${index ? "L" : "M"} ${point.x} ${point.y}`).join(" ");
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
                group: point.group == null ? null : String(point.group)
            }))
        }))
        : [];

    class NativeChartController {
        constructor(root, configuration) {
            this.root = root;
            this.configuration = { curve: "Smooth", barMode: "Grouped", showLegend: true, showGrid: true, showAxes: true, showTooltip: true, ...configuration };
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
            this.root.addEventListener("zoerce:chart:replace", event => {
                this.hiddenSeries.clear();
                this.render(event.detail?.series ?? this.series);
                this.root.dispatchEvent(new CustomEvent("zoerce:chart:updated", { bubbles: true, detail: { series: this.series } }));
            }, { signal: this.abortController.signal });
            this.root.addEventListener("zoerce:chart:refresh", () => this.load(), { signal: this.abortController.signal });
            this.initializeCanvas();
            this.render(this.series);
            this.observeSize();
            this.connectRealtime();
            if (configuration.sourceUrl) this.load();
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
            this.updateLegend();
            this.updateMetadata(visible);
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
            const values = visible.flatMap(seriesItem => seriesItem.points.map(point => point.value));
            const kind = this.configuration.kind || "Line";
            if (kind === "Donut") return this.renderDonut(visible, width, height);
            if (kind === "Gauge") return this.renderGauge(visible, width, height);
            if (kind === "Funnel") return this.renderFunnel(visible, width, height);
            if (kind === "Heatmap") return this.renderHeatmap(visible, width, height);
            if (kind === "Scatter" || kind === "Bubble") return this.renderScatter(visible, width, height, kind === "Bubble");
            if (kind === "Bullet") return this.renderBullet(visible, width, height);
            if (kind === "Histogram") return this.renderHistogram(visible, width, height);
            if (kind === "Waterfall") return this.renderWaterfall(visible, width, height);
            if (kind === "HorizontalBar") return this.renderHorizontalBars(visible, width, height);
            if (kind === "Bar") return this.renderBars(visible, width, height);
            this.renderTrend(visible, width, height, kind === "Area", kind === "Sparkline");
        }

        updateMetadata(series) {
            const primary = series[0];
            const points = primary?.points ?? [];
            const kind = this.configuration.kind || "Line";
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
            this.legend.hidden = !this.configuration.showLegend || this.series.length < 2;
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
                button.addEventListener("click", () => {
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
            if (!this.configuration.showTooltip) return;
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
            const toX = (index, count) => count <= 1 ? (plot.left + plot.right) / 2 : plot.left + index * (plot.right - plot.left) / (count - 1);
            const toY = value => plot.bottom - ((value - domain.minimum) / (domain.maximum - domain.minimum)) * (plot.bottom - plot.top);
            if (!sparkline) this.renderFrame(labels, domain, plot, toX, toY);
            series.forEach((seriesItem, seriesIndex) => {
                const color = this.color(seriesIndex, seriesItem);
                const points = seriesItem.points.map((point, index) => ({ x: toX(index, seriesItem.points.length), y: toY(point.value), point }));
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
            const centerX = width / 2; const centerY = height / 2; const radius = Math.max(48, Math.min(width, height) * .29);
            const track = svg("path");
            setAttributes(track, { class: "z-native-chart-ring-track", d: ringPath(centerX, centerY, radius) });
            this.layers.data.append(track);
            const gap = Math.min(3.2, Math.max(1.3, 10 / Math.max(slices.length, 1)));
            let angle = 0;
            slices.forEach((slice, index) => {
                const share = slice.value / total * 100;
                const sweep = share * 3.6;
                const arc = svg("path");
                setAttributes(arc, { class: "z-native-chart-ring", d: arcPath(centerX, centerY, radius, angle + gap / 2, Math.max(0, sweep - gap)) });
                setPaint(arc, { stroke: slice.color });
                this.bindTooltip(arc, `${slice.name}: ${format(slice.value)} (${format(share)}%)`, { x: centerX, y: centerY - radius });
                this.layers.data.append(arc); angle += sweep;
            });
            const leading = slices.reduce((result, slice) => !result || slice.value > result.value ? slice : result, null);
            appendText(this.layers.annotation, centerX, centerY - 3, withSuffix(leading?.value ?? 0, this.configuration.valueSuffix), "z-native-chart-ring-value", "middle");
            appendText(this.layers.annotation, centerX, centerY + 17, leading?.name ?? "No data", "z-native-chart-ring-label", "middle");
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

        async load() {
            try {
                this.root.setAttribute("aria-busy", "true"); this.setState("Refreshing…");
                const response = await fetch(this.configuration.sourceUrl, { headers: { Accept: "application/json" }, signal: this.abortController.signal });
                if (!response.ok) throw new Error(`Request failed with ${response.status}`);
                const body = await response.json();
                this.render(body.series ?? body); this.setState("");
                this.root.dispatchEvent(new CustomEvent("zoerce:chart:loaded", { bubbles: true, detail: { series: body.series ?? body } }));
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.setState("Unable to refresh chart.");
                    this.root.dispatchEvent(new CustomEvent("zoerce:chart:error", { bubbles: true, detail: { error } }));
                }
            } finally { this.root.removeAttribute("aria-busy"); }
        }

        connectRealtime() {
            if (!this.configuration.realtimeHubUrl || !this.configuration.realtimeEventName) return;
            if (!window.ZoorceSignalR?.subscribe) {
                document.addEventListener("zoerce:signalr-ready", () => this.connectRealtime(), { once: true, signal: this.abortController.signal }); return;
            }
            this.unsubscribeRealtime = window.ZoorceSignalR.subscribe({ hubUrl: this.configuration.realtimeHubUrl, eventName: this.configuration.realtimeEventName }, update => {
                this.render(update?.series ?? update);
                this.root.dispatchEvent(new CustomEvent("zoerce:chart:realtime", { bubbles: true, detail: { update } }));
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

    document.querySelectorAll("[data-z-chart]").forEach(root => {
        legacyComponents.dispose(root);
        enhancedMount(root);
    });
    window.ZoorceComponents = Object.freeze({
        mount(scope = document) {
            legacyComponents.mount(scope);
            const roots = scope.matches?.("[data-z-chart]") ? [scope] : [...(scope.querySelectorAll?.("[data-z-chart]") ?? [])];
            roots.forEach(root => { legacyComponents.dispose(root); enhancedMount(root); });
        },
        dispose(root) { enhancedDispose(root); legacyComponents.dispose(root); }
    });
    window.ZoorceCharts = Object.freeze({ mount: enhancedMount, dispose: enhancedDispose });
})();
