(() => {
    "use strict";

    const namespace = "http://www.w3.org/2000/svg";
    const canvasWidth = 1400;
    const canvasHeight = 780;
    const nodeWidth = 176;
    const nodeHeight = 86;
    const ellipseKinds = new Set(["start", "end", "topic"]);
    const instances = new WeakMap();
    const profiles = { Flowchart: "flowchart", MindMap: "mindmap", OrgChart: "orgchart", Architecture: "architecture" };
    const modes = { Editor: "editor", Viewer: "viewer", ReadOnly: "readonly" };
    const panModes = { Background: "background", SpaceKey: "spacekey", MiddleButton: "middlebutton", Disabled: "disabled" };
    const portSides = new Set(["Auto", "Top", "Right", "Bottom", "Left"]);
    const portOrder = ["top", "right", "bottom", "left"];
    const nodeKinds = new Set([
        "start", "end", "process", "decision", "input-output", "subprocess", "document", "data-store", "delay", "connector", "annotation",
        "root", "branch", "topic", "note", "person", "team", "vacancy", "user", "service", "database", "queue", "external", "boundary"
    ]);
    const tones = new Set(["Neutral", "Primary", "Success", "Warning", "Danger", "Info"]);
    const actionKinds = new Set(["None", "Navigate", "Panel", "Modal", "Event", "Command", "Endpoint"]);
    const enumValue = (values, value, fallback) => [...values].find(candidate => key(candidate) === key(value)) ?? fallback;

    const create = (name, className) => {
        const element = document.createElement(name);
        if (className) element.className = className;
        return element;
    };
    const svg = (name, className) => {
        const element = document.createElementNS(namespace, name);
        if (className) element.setAttribute("class", className);
        return element;
    };
    const clone = value => JSON.parse(JSON.stringify(value));
    const text = value => String(value ?? "").trim();
    const key = value => text(value).toLowerCase();
    const clamp = (value, min, max) => Math.min(max, Math.max(min, value));
    const dispatch = (root, name, detail, cancelable = false) => root.dispatchEvent(new CustomEvent(name, { bubbles: true, cancelable, detail }));
    const number = (value, fallback = 0) => Number.isFinite(Number(value)) ? Number(value) : fallback;
    const optionalNumber = (value, minimum, maximum) => value === null || value === undefined || value === "" || !Number.isFinite(Number(value))
        ? null
        : clamp(Number(value), minimum, maximum);
    const color = value => {
        const candidate = text(value);
        if (!candidate) return null;
        if (globalThis.CSS?.supports) return globalThis.CSS.supports("color", candidate) ? candidate : null;
        return /^#[0-9a-f]{3,8}$/i.test(candidate) ? candidate : null;
    };
    const colorForInput = (value, fallback) => /^#[0-9a-f]{6}$/i.test(text(value)) ? text(value) : fallback;
    const idFor = (prefix, values) => {
        let index = values.length + 1;
        let id = prefix + "-" + index;
        while (values.some(item => key(item.id) === key(id))) {
            index += 1;
            id = prefix + "-" + index;
        }
        return id;
    };
    const titleForKind = kind => ({
        start: "Start", end: "End", process: "New process", decision: "Decision",
        "input-output": "Input / output", subprocess: "Subprocess", document: "Document",
        "data-store": "Data store", delay: "Delay", connector: "Connector", annotation: "Annotation",
        root: "Central topic", branch: "New branch", topic: "New topic", note: "Note",
        person: "Team member", team: "Team", vacancy: "Open role", user: "User",
        service: "Service", database: "Database", queue: "Event queue", external: "External system", boundary: "Domain boundary"
    })[kind] ?? "New node";
    const iconForKind = kind => ({
        start: "●", end: "■", process: "▭", decision: "◇", "input-output": "▱",
        subprocess: "▣", document: "▤", "data-store": "◒", delay: "◔", connector: "○", annotation: "⌁",
        root: "✦", branch: "↗", topic: "◌", note: "⌁", person: "●", team: "◇", vacancy: "+",
        user: "◉", service: "▤", database: "◒", queue: "≈", external: "↗", boundary: "□"
    })[kind] ?? "◌";

    const normalizeAction = action => {
        const kind = enumValue(actionKinds, action?.kind, "None");
        if (!action || kind === "None") return null;
        return { kind, target: text(action.target) || null, payload: action.payload ?? null };
    };
    const normalizeNodeStyle = style => {
        if (!style || typeof style !== "object") return null;
        const gradientStart = color(style.gradient?.startColor);
        const gradientEnd = color(style.gradient?.endColor);
        const normalized = {
            fillColor: color(style.fillColor),
            borderColor: color(style.borderColor),
            borderWidth: optionalNumber(style.borderWidth, .5, 12),
            textColor: color(style.textColor),
            fontFamily: ["system", "rounded", "mono", "serif"].includes(key(style.fontFamily)) ? key(style.fontFamily) : null,
            fontSize: optionalNumber(style.fontSize, 9, 42),
            fontWeight: optionalNumber(style.fontWeight, 300, 900),
            cornerRadius: optionalNumber(style.cornerRadius, 0, 64),
            opacity: optionalNumber(style.opacity, .15, 1),
            gradient: gradientStart && gradientEnd ? { startColor: gradientStart, endColor: gradientEnd, angle: Math.round(optionalNumber(style.gradient?.angle, 0, 360) ?? 135) } : null
        };
        return Object.values(normalized).some(value => value !== null) ? normalized : null;
    };
    const normalizeEdgeStyle = style => {
        if (!style || typeof style !== "object") return null;
        const normalized = { strokeColor: color(style.strokeColor), strokeWidth: optionalNumber(style.strokeWidth, 1, 8), dashed: Boolean(style.dashed) };
        return normalized.strokeColor || normalized.strokeWidth || normalized.dashed ? normalized : null;
    };
    const normalizeNode = (node, index) => ({
        id: text(node?.id) || "node-" + (index + 1),
        kind: key(node?.kind) || "process",
        title: text(node?.title) || "Untitled node",
        description: text(node?.description) || null,
        tone: enumValue(tones, node?.tone, "Neutral"),
        collapsed: Boolean(node?.collapsed),
        action: normalizeAction(node?.action),
        data: node?.data ?? null,
        style: normalizeNodeStyle(node?.style),
        position: {
            x: clamp(number(node?.position?.x, 120 + index * 220), -600, 3000),
            y: clamp(number(node?.position?.y, 140 + (index % 3) * 140), -600, 2400)
        }
    });
    const normalizeEdge = (edge, index) => ({
        id: text(edge?.id) || "edge-" + (index + 1),
        fromNodeId: text(edge?.fromNodeId),
        toNodeId: text(edge?.toNodeId),
        label: text(edge?.label) || null,
        tone: enumValue(tones, edge?.tone, "Neutral"),
        kind: text(edge?.kind) || null,
        style: normalizeEdgeStyle(edge?.style),
        fromPort: enumValue(portSides, edge?.fromPort, "Auto"),
        toPort: enumValue(portSides, edge?.toPort, "Auto")
    });
    const normalizeDocument = document => {
        const nodes = Array.isArray(document?.nodes) ? document.nodes.map(normalizeNode) : [];
        const ids = new Set();
        const validNodes = nodes.filter(node => {
            const unique = !ids.has(key(node.id));
            ids.add(key(node.id));
            return unique;
        });
        const paths = new Set();
        const edges = (Array.isArray(document?.edges) ? document.edges.map(normalizeEdge) : [])
            .filter(edge => {
                const valid = edge.fromNodeId && edge.toNodeId && edge.fromNodeId !== edge.toNodeId && validNodes.some(node => key(node.id) === key(edge.fromNodeId)) && validNodes.some(node => key(node.id) === key(edge.toNodeId));
                const unique = !paths.has(key(edge.fromNodeId) + "→" + key(edge.toNodeId));
                if (valid && unique) paths.add(key(edge.fromNodeId) + "→" + key(edge.toNodeId));
                return valid && unique;
            });
        return {
            schemaVersion: Math.max(1, Math.floor(number(document?.schemaVersion, 1))),
            nodes: validNodes,
            edges,
            version: text(document?.version) || null
        };
    };

    class DiagramController {
        constructor(root, configuration) {
            this.root = root;
            this.configuration = {
                profile: "Flowchart", mode: "Editor", layout: "Manual", document: {},
                gridSize: 24, snapToGrid: true, minimumZoom: .4, maximumZoom: 2.4, initialZoom: 1, panMode: "Background",
                ...configuration
            };
            this.profile = profiles[this.configuration.profile] ?? key(this.configuration.profile);
            this.mode = modes[this.configuration.mode] ?? key(this.configuration.mode);
            this.document = normalizeDocument(this.configuration.document);
            this.canvas = root.querySelector("[data-z-diagram-canvas]");
            this.world = root.querySelector("[data-z-diagram-world]");
            this.edgesLayer = root.querySelector("[data-z-diagram-edges]");
            this.navigator = root.querySelector("[data-z-diagram-navigator]");
            this.inspector = root.querySelector("[data-z-diagram-inspector]");
            this.minimap = root.querySelector("[data-z-diagram-minimap]");
            this.overview = root.querySelector("[data-z-diagram-overview]");
            this.overviewToggle = root.querySelector("[data-z-diagram-overview-toggle]");
            this.panHelp = root.querySelector("[data-z-diagram-pan-help]");
            this.status = root.querySelector("[data-z-diagram-status]");
            this.zoomOutput = root.querySelector("[data-z-diagram-zoom]");
            this.dirtyBadge = root.querySelector("[data-z-diagram-dirty]");
            this.announcer = root.querySelector("[data-z-diagram-announcer]");
            this.abortController = new AbortController();
            this.selection = new Set();
            this.selectedEdge = null;
            this.connectingFrom = null;
            this.connectionMode = false;
            this.portDrag = null;
            this.connectionPreview = null;
            this.ignoreNextPortClick = false;
            this.connectToggle = root.querySelector("[data-z-diagram-connect-toggle]");
            this.focusToggle = root.querySelector("[data-z-diagram-focus-toggle]");
            this.history = [];
            this.future = [];
            this.dirty = false;
            this.revision = 0;
            this.saveInFlight = false;
            this.saveQueued = false;
            this.autoSaveTimer = null;
            this.focusMode = false;
            this.focusReturn = null;
            this.focusPlaceholder = null;
            this.inspectorEditTarget = null;
            this.inspectorEditCaptured = false;
            this.inspectorTab = "content";
            this.overviewVisible = false;
            const requestedPanMode = panModes[this.configuration.panMode] ?? key(this.configuration.panMode);
            this.panMode = Object.values(panModes).includes(requestedPanMode) ? requestedPanMode : "background";
            if (this.panHelp) this.panHelp.textContent = this.panMode === "spacekey"
                ? "Hold space and drag unused canvas space to pan the viewport."
                : this.panMode === "middlebutton"
                    ? "Use the middle mouse button to pan the viewport."
                    : this.panMode === "disabled"
                        ? "Viewport panning is disabled for this diagram."
                        : "Drag unused canvas space to pan the viewport.";
            this.view = { x: 0, y: 0, zoom: clamp(number(this.configuration.initialZoom, 1), number(this.configuration.minimumZoom, .4), number(this.configuration.maximumZoom, 2.4)) };
            this.drag = null;
            this.pan = null;
            this.paletteDragKind = null;
            this.pendingPointerAction = null;
            this.lastNodeTap = null;
            this.ignoreNativeDoubleClickUntil = 0;
            this.resizeObserver = globalThis.ResizeObserver ? new ResizeObserver(() => this.renderViewport()) : null;
            this.resizeObserver?.observe(this.canvas);
            this.bind();
            this.render();
            this.fit(false);
            this.load();
            this.connectRealtime();
        }

        get editable() { return this.mode === "editor"; }

        bind() {
            const signal = this.abortController.signal;
            this.root.addEventListener("click", event => this.onClick(event), { signal });
            this.root.addEventListener("dblclick", event => this.onDoubleClick(event), { signal });
            this.root.addEventListener("dragstart", event => this.onPaletteDragStart(event), { signal });
            this.root.addEventListener("dragend", () => {
                this.paletteDragKind = null;
                this.canvas?.classList.remove("is-drop-ready", "is-drop-target");
            }, { signal });
            this.canvas?.addEventListener("pointerdown", event => this.onPointerDown(event), { signal });
            this.canvas?.addEventListener("pointermove", event => this.onPointerMove(event), { signal });
            this.canvas?.addEventListener("pointerup", event => this.onPointerUp(event), { signal });
            this.canvas?.addEventListener("pointercancel", event => this.onPointerUp(event), { signal });
            this.canvas?.addEventListener("wheel", event => this.onWheel(event), { signal, passive: false });
            this.canvas?.addEventListener("keydown", event => this.onKeyDown(event), { signal });
            this.canvas?.addEventListener("keyup", event => this.onKeyUp(event), { signal });
            this.canvas?.addEventListener("dragover", event => this.onCanvasDragOver(event), { signal });
            this.canvas?.addEventListener("dragleave", event => this.onCanvasDragLeave(event), { signal });
            this.canvas?.addEventListener("drop", event => this.onCanvasDrop(event), { signal });
            this.canvas?.addEventListener("blur", () => { this.spacePan = false; }, { signal });
            this.minimap?.addEventListener("pointerdown", event => this.onOverviewPointerDown(event), { signal });
            this.minimap?.addEventListener("pointermove", event => this.onOverviewPointerMove(event), { signal });
            this.minimap?.addEventListener("pointerup", () => { this.overviewDrag = false; }, { signal });
            this.minimap?.addEventListener("pointercancel", () => { this.overviewDrag = false; }, { signal });
            this.root.addEventListener("zoerce:diagram:replace", event => this.replace(event.detail?.document ?? event.detail, "Diagram updated from the host."), { signal });
            this.root.addEventListener("zoerce:diagram:focus", event => this.focus(event.detail?.nodeId), { signal });
            this.root.addEventListener("zoerce:diagram:validate", () => this.validate(true), { signal });
            this.root.addEventListener("zoerce:diagram:save", () => this.save(), { signal });
            this.root.addEventListener("focusin", event => {
                if (event.target.matches("[data-z-diagram-field], [data-z-diagram-style-field]")) this.beginInspectorEdit(event.target);
            }, { signal });
            this.root.addEventListener("input", event => {
                this.onInspectorFieldChanged(event.target);
            }, { signal });
            this.root.addEventListener("change", event => {
                this.onInspectorFieldChanged(event.target);
            }, { signal });
        }

        onClick(event) {
            const command = event.target.closest("[data-z-diagram-command]")?.dataset.zDiagramCommand;
            if (command) {
                this.command(command);
                return;
            }
            const tab = event.target.closest("[data-z-diagram-inspector-tab]")?.dataset.zDiagramInspectorTab;
            if (tab) {
                this.inspectorTab = tab;
                this.renderInspector();
                return;
            }
            const swatch = event.target.closest("[data-z-diagram-style-swatch]");
            if (swatch) {
                const field = swatch.dataset.zDiagramStyleSwatch;
                const input = this.inspector?.querySelector("[data-z-diagram-style-field='" + CSS.escape(field) + "']");
                if (input) {
                    input.value = swatch.dataset.zDiagramStyleValue;
                    this.beginInspectorEdit(swatch);
                    this.commitInspectorAppearance();
                }
                return;
            }
            const inspectorAction = event.target.closest("[data-z-diagram-inspector]")?.dataset.zDiagramInspector;
            if (inspectorAction) {
                this.handleInspector(inspectorAction);
                return;
            }
            const add = event.target.closest("[data-z-diagram-add]")?.dataset.zDiagramAdd;
            if (add && this.editable) {
                this.addNode(add);
                return;
            }
            const navigate = event.target.closest("[data-z-diagram-navigate]")?.dataset.zDiagramNavigate;
            if (navigate) {
                this.focus(navigate);
                return;
            }
            const port = event.target.closest("[data-z-diagram-port]");
            if (port && this.editable) {
                event.stopPropagation();
                if (this.ignoreNextPortClick) {
                    this.ignoreNextPortClick = false;
                    return;
                }
                this.handlePort(port);
                return;
            }
            const node = event.target.closest("[data-z-diagram-node]");
            const pointerNode = this.pendingPointerAction;
            this.pendingPointerAction = null;
            // A node selection refreshes its inspector during pointerdown. Some browsers
            // subsequently retarget the synthetic click to the canvas, which must not
            // immediately clear that just-established selection.
            if (!node && pointerNode && this.node(pointerNode)) {
                this.selectNode(pointerNode, false, false);
                return;
            }
            if (node) {
                const nodeId = node.dataset.zDiagramNodeId;
                if (this.connectionMode) {
                    if (!this.connectingFrom) {
                        if (!this.canConnectFrom(nodeId)) {
                            this.setStatus("An end node cannot start a connection.");
                            return;
                        }
                        this.connectingFrom = { nodeId, side: "auto" };
                        this.setStatus("Source selected. Click any target node to finish the connection.");
                        this.renderScene();
                        return;
                    }
                    if (key(this.connectingFrom.nodeId) !== key(nodeId)) {
                        const source = this.node(this.connectingFrom.nodeId);
                        const target = this.node(nodeId);
                        this.connectNodes(this.connectingFrom.nodeId, this.connectingFrom.side, nodeId, this.autoPort(source, target, false));
                        return;
                    }
                }
                const actionHandledOnPointerUp = pointerNode === nodeId;
                this.selectNode(nodeId, event.ctrlKey || event.metaKey, !actionHandledOnPointerUp);
                return;
            }
            const edge = event.target.closest("[data-z-diagram-edge]");
            if (edge) {
                this.selectEdge(edge.dataset.zDiagramEdgeId);
                return;
            }
            if (event.target === this.canvas || event.target === this.world || event.target === this.edgesLayer) this.clearSelection();
        }

        onDoubleClick(event) {
            if (!this.editable || !(this.profile === "mindmap" || this.profile === "orgchart")) return;
            if (Date.now() < this.ignoreNativeDoubleClickUntil) return;
            const node = event.target.closest("[data-z-diagram-node]");
            if (!node) return;
            this.toggleCollapsed(node.dataset.zDiagramNodeId);
        }

        onPointerDown(event) {
            if (event.button !== 0 && event.button !== 1) return;
            const port = event.target.closest("[data-z-diagram-port]");
            if (port && this.editable && event.button === 0) {
                const nodeId = port.closest("[data-z-diagram-node]")?.dataset.zDiagramNodeId;
                if (!nodeId) return;
                this.portDrag = { nodeId, side: port.dataset.zDiagramPortSide || "right", x: event.clientX, y: event.clientY, moved: false };
                this.canvas.setPointerCapture?.(event.pointerId);
                event.preventDefault();
                return;
            }
            const nodeElement = event.target.closest("[data-z-diagram-node]");
            if (nodeElement && this.editable && this.connectionMode && !event.target.closest("[data-z-diagram-port]")) {
                // Do not select and re-render a node before its click reaches the connection handler.
                // Replacing it during pointerdown would detach the eventual click target.
                event.preventDefault();
                return;
            }
            if (nodeElement && this.editable && !event.target.closest("[data-z-diagram-port]")) {
                const nodeId = nodeElement.dataset.zDiagramNodeId;
                if (!this.selection.has(nodeId)) this.selectNode(nodeId, event.ctrlKey || event.metaKey, false);
                this.drag = {
                    x: event.clientX,
                    y: event.clientY,
                    primaryId: nodeId,
                    before: this.snapshot(),
                    positions: [...this.selection].map(id => ({ id, position: { ...this.node(id)?.position } })),
                    moved: false
                };
                this.canvas.setPointerCapture?.(event.pointerId);
                return;
            }
            if (this.canPan(event) && (event.target === this.canvas || event.target === this.world || event.target === this.edgesLayer || event.button === 1)) {
                this.pan = { x: event.clientX, y: event.clientY, viewX: this.view.x, viewY: this.view.y };
                this.canvas.setPointerCapture?.(event.pointerId);
                this.canvas.classList.add("is-panning");
                event.preventDefault();
            }
        }

        onPointerMove(event) {
            if (this.portDrag) {
                const distance = Math.abs(event.clientX - this.portDrag.x) + Math.abs(event.clientY - this.portDrag.y);
                if (distance > 4) {
                    this.portDrag.moved = true;
                    const targetId = document.elementFromPoint(event.clientX, event.clientY)?.closest("[data-z-diagram-node]")?.dataset.zDiagramNodeId;
                    const target = this.node(targetId);
                    const source = this.node(this.portDrag.nodeId);
                    this.connectionPreview = { ...this.portDrag, x: event.clientX, y: event.clientY, targetId: target?.id ?? null, targetSide: target ? this.autoPort(source, target, false) : null };
                    this.renderScene();
                }
                return;
            }
            if (this.drag) {
                const deltaX = (event.clientX - this.drag.x) / this.view.zoom;
                const deltaY = (event.clientY - this.drag.y) / this.view.zoom;
                if (Math.abs(deltaX) + Math.abs(deltaY) > 3) this.drag.moved = true;
                this.drag.positions.forEach(entry => {
                    const node = this.node(entry.id);
                    if (node) {
                        node.position.x = clamp(entry.position.x + deltaX, -400, canvasWidth - 30);
                        node.position.y = clamp(entry.position.y + deltaY, -300, canvasHeight - 30);
                    }
                });
                this.renderScene();
                return;
            }
            if (this.pan) {
                this.view.x = this.pan.viewX + event.clientX - this.pan.x;
                this.view.y = this.pan.viewY + event.clientY - this.pan.y;
                this.renderViewport();
            }
        }

        onPointerUp(event) {
            if (this.portDrag) {
                const drag = this.portDrag;
                this.portDrag = null;
                this.connectionPreview = null;
                if (drag.moved) {
                    const targetElement = document.elementFromPoint(event.clientX, event.clientY)?.closest("[data-z-diagram-node]");
                    if (targetElement) {
                        const targetId = targetElement.dataset.zDiagramNodeId;
                        const targetPort = document.elementFromPoint(event.clientX, event.clientY)?.closest("[data-z-diagram-port]");
                        const source = this.node(drag.nodeId);
                        const target = this.node(targetId);
                        this.connectNodes(drag.nodeId, drag.side, targetId, targetPort?.dataset.zDiagramPortSide || this.autoPort(source, target, false));
                    } else {
                        this.setStatus("Connection drag cancelled.");
                    }
                    this.ignoreNextPortClick = true;
                }
                this.renderScene();
                return;
            }
            if (this.drag) {
                const drag = this.drag;
                const moved = drag.moved;
                if (moved) {
                    if (this.configuration.snapToGrid) this.document.nodes.forEach(node => {
                        node.position.x = this.snap(node.position.x);
                        node.position.y = this.snap(node.position.y);
                    });
                    this.history.push(drag.before);
                    this.future = [];
                    this.markDirty("Node position updated.");
                    this.emitChange("move");
                    this.render();
                } else {
                    this.pendingPointerAction = drag.primaryId;
                    const node = this.node(drag.primaryId);
                    if (node?.action) this.invokeAction(node);
                    if (this.profile === "mindmap" || this.profile === "orgchart") {
                        const now = Date.now();
                        if (this.lastNodeTap?.id === drag.primaryId && now - this.lastNodeTap.time < 400) {
                            this.lastNodeTap = null;
                            this.ignoreNativeDoubleClickUntil = now + 250;
                            this.toggleCollapsed(drag.primaryId);
                        } else {
                            this.lastNodeTap = { id: drag.primaryId, time: now };
                        }
                    }
                }
                this.drag = null;
            }
            if (this.pan) {
                this.pan = null;
                this.canvas?.classList.remove("is-panning");
            }
        }

        onWheel(event) {
            if (!event.ctrlKey && !event.metaKey) return;
            event.preventDefault();
            this.setZoom(this.view.zoom + (event.deltaY < 0 ? .1 : -.1));
        }

        onKeyDown(event) {
            if (event.code === "Space" && this.panMode === "spacekey") {
                this.spacePan = true;
                event.preventDefault();
                this.canvas?.classList.add("is-pan-ready");
                return;
            }
            if (event.key === "Delete" || event.key === "Backspace") {
                if (this.editable && !event.target.matches("input, textarea, select")) {
                    event.preventDefault();
                    this.deleteSelection();
                }
                return;
            }
            if ((event.ctrlKey || event.metaKey) && key(event.key) === "c" && this.editable && !event.target.matches("input, textarea, select")) {
                event.preventDefault();
                this.copySelection();
                return;
            }
            if ((event.ctrlKey || event.metaKey) && key(event.key) === "v" && this.editable && !event.target.matches("input, textarea, select")) {
                event.preventDefault();
                this.pasteSelection();
                return;
            }
            if ((event.ctrlKey || event.metaKey) && key(event.key) === "z") {
                event.preventDefault();
                event.shiftKey ? this.redo() : this.undo();
                return;
            }
            if (event.key === "Escape") {
                if (this.connectingFrom) {
                    this.connectingFrom = null;
                    this.setStatus("Connection source cleared.");
                } else if (this.connectionMode) {
                    this.connectionMode = false;
                    this.setStatus("Connection mode closed.");
                } else if (this.focusMode) {
                    this.toggleFocusMode(false);
                    return;
                }
                this.renderScene();
            }
        }

        onKeyUp(event) {
            if (event.code !== "Space") return;
            this.spacePan = false;
            this.canvas?.classList.remove("is-pan-ready");
        }

        paletteKind(event) {
            const transferred = event.dataTransfer?.getData("application/x-zoorce-diagram-node")
                || event.dataTransfer?.getData("text/plain")
                || this.paletteDragKind;
            const kind = key(transferred);
            return nodeKinds.has(kind) ? kind : null;
        }

        onPaletteDragStart(event) {
            const kind = key(event.target.closest("[data-z-diagram-add]")?.dataset.zDiagramAdd);
            if (!this.editable || !nodeKinds.has(kind) || !event.dataTransfer) return;
            this.paletteDragKind = kind;
            event.dataTransfer.effectAllowed = "copy";
            event.dataTransfer.setData("application/x-zoorce-diagram-node", kind);
            event.dataTransfer.setData("text/plain", kind);
            this.canvas?.classList.add("is-drop-ready");
        }

        onCanvasDragOver(event) {
            if (!this.editable || !this.paletteKind(event)) return;
            event.preventDefault();
            event.dataTransfer.dropEffect = "copy";
            this.canvas?.classList.add("is-drop-target");
        }

        onCanvasDragLeave(event) {
            if (!event.relatedTarget || !this.canvas?.contains(event.relatedTarget)) this.canvas?.classList.remove("is-drop-target");
        }

        onCanvasDrop(event) {
            const kind = this.paletteKind(event);
            this.paletteDragKind = null;
            this.canvas?.classList.remove("is-drop-ready", "is-drop-target");
            if (!this.editable || !kind || !this.canvas) return;
            event.preventDefault();
            const bounds = this.canvas.getBoundingClientRect();
            this.addNode(kind, {
                x: (event.clientX - bounds.left - this.view.x) / this.view.zoom,
                y: (event.clientY - bounds.top - this.view.y) / this.view.zoom
            });
        }

        command(command) {
            if (command === "zoom-in") this.setZoom(this.view.zoom + .15);
            if (command === "zoom-out") this.setZoom(this.view.zoom - .15);
            if (command === "fit") this.fit(true);
            if (command === "undo") this.undo();
            if (command === "redo") this.redo();
            if (command === "layout" && this.editable) this.applyLayout();
            if (command === "connect" && this.editable) this.toggleConnectionMode();
            if (command === "validate") this.validate(true);
            if (command === "save") this.save();
            if (command === "toggle-overview") this.toggleOverview();
            if (command === "focus") this.toggleFocusMode();
        }

        node(id) { return this.document.nodes.find(node => key(node.id) === key(id)); }
        edge(id) { return this.document.edges.find(edge => key(edge.id) === key(id)); }
        canPan(event) {
            if (event.button === 1) return this.panMode !== "disabled";
            if (this.panMode === "background") return true;
            if (this.panMode === "spacekey") return Boolean(this.spacePan);
            return false;
        }
        snap(value) { return Math.round(value / number(this.configuration.gridSize, 24)) * number(this.configuration.gridSize, 24); }
        snapshot() { return clone(this.document); }

        change(message, mutation) {
            this.history.push(this.snapshot());
            this.future = [];
            mutation();
            this.markDirty(message);
            this.emitChange("edit");
            this.render();
        }

        markDirty(message) {
            this.revision += 1;
            this.dirty = true;
            if (this.dirtyBadge) this.dirtyBadge.hidden = false;
            this.setStatus(message);
            this.scheduleAutoSave();
        }

        scheduleAutoSave(delay = 480) {
            if (!this.editable) return;
            if (this.autoSaveTimer) globalThis.clearTimeout(this.autoSaveTimer);
            this.autoSaveTimer = globalThis.setTimeout(() => {
                this.autoSaveTimer = null;
                this.save(true);
            }, delay);
        }

        setStatus(message) {
            if (this.status) this.status.textContent = message;
            if (this.announcer) this.announcer.textContent = message;
        }

        emitChange(reason) {
            dispatch(this.root, "zoerce:diagram:changed", { document: this.snapshot(), reason });
        }

        beginInspectorEdit(target) {
            if (this.inspectorEditTarget === target) return;
            this.inspectorEditTarget = target;
            this.inspectorEditCaptured = false;
        }

        prepareInspectorChange() {
            if (this.inspectorEditCaptured) return;
            this.history.push(this.snapshot());
            this.future = [];
            this.inspectorEditCaptured = true;
        }

        onInspectorFieldChanged(target) {
            if (!this.editable) return;
            if (target.matches("[data-z-diagram-field]")) this.commitInspectorContent();
            if (target.matches("[data-z-diagram-style-field]")) {
                const edge = this.selectedEdge ? this.edge(this.selectedEdge) : null;
                if (edge) this.commitInspectorEdge();
                else this.commitInspectorAppearance();
            }
        }

        commitInspectorContent() {
            const selected = this.document.nodes.find(node => this.selection.has(node.id));
            if (!selected || !this.inspector) return;
            const title = this.inspector.querySelector("[data-z-diagram-field='title']")?.value;
            const description = this.inspector.querySelector("[data-z-diagram-field='description']")?.value;
            const tone = this.inspector.querySelector("[data-z-diagram-field='tone']")?.value;
            const actionKind = this.inspector.querySelector("[data-z-diagram-field='action-kind']")?.value;
            const target = this.inspector.querySelector("[data-z-diagram-field='action-target']")?.value;
            this.prepareInspectorChange();
            selected.title = text(title) || "Untitled node";
            selected.description = text(description) || null;
            selected.tone = tones.has(tone) ? tone : "Neutral";
            selected.action = actionKinds.has(actionKind) && actionKind !== "None" ? { kind: actionKind, target: text(target) || null, payload: null } : null;
            this.markDirty("Node details updated.");
            this.emitChange("edit");
            this.renderScene();
            this.renderNavigator();
        }

        commitInspectorAppearance() {
            const selected = this.document.nodes.find(node => this.selection.has(node.id));
            if (!selected) return;
            this.prepareInspectorChange();
            selected.style = this.styleFromInspector();
            this.markDirty("Node appearance updated.");
            this.emitChange("edit");
            this.renderScene();
            const advice = this.inspector?.querySelector("[data-z-diagram-contrast]");
            if (advice) advice.textContent = this.contrastAdvice(selected.style ?? {});
        }

        commitInspectorEdge() {
            const edge = this.selectedEdge ? this.edge(this.selectedEdge) : null;
            if (!edge || !this.inspector) return;
            const label = this.inspector.querySelector("[data-z-diagram-field='edge-label']")?.value;
            this.prepareInspectorChange();
            edge.label = text(label) || null;
            edge.style = normalizeEdgeStyle({ strokeColor: this.styleValue("edgeStroke"), strokeWidth: this.styleValue("edgeWidth"), dashed: this.styleChecked("edgeDashed") });
            this.markDirty("Connection style updated.");
            this.emitChange("edit");
            this.renderScene();
        }

        selectNode(nodeId, append = false, invokeAction = true) {
            const node = this.node(nodeId);
            if (!node) return;
            if (!append) this.selection.clear();
            if (append && this.selection.has(node.id)) this.selection.delete(node.id);
            else this.selection.add(node.id);
            this.selectedEdge = null;
            this.render();
            // Rendering replaces the node that was clicked. Return focus to the
            // persistent canvas so Delete / Ctrl+C / Ctrl+V keep working after
            // a selection instead of leaving focus on the detached node.
            this.canvas?.focus({ preventScroll: true });
            dispatch(this.root, "zoerce:diagram:selectionchanged", { nodeIds: [...this.selection], edgeId: null });
            dispatch(this.root, "zoerce:diagram:nodeclick", { node, document: this.snapshot() });
            if (invokeAction && node.action) this.invokeAction(node);
        }

        selectEdge(edgeId) {
            const edge = this.edge(edgeId);
            if (!edge) return;
            this.selection.clear();
            this.selectedEdge = edge.id;
            this.render();
            this.canvas?.focus({ preventScroll: true });
            dispatch(this.root, "zoerce:diagram:selectionchanged", { nodeIds: [], edgeId: edge.id });
            dispatch(this.root, "zoerce:diagram:edgeclick", { edge, document: this.snapshot() });
        }

        clearSelection() {
            if (!this.selection.size && !this.selectedEdge && !this.connectingFrom) return;
            this.selection.clear();
            this.selectedEdge = null;
            this.connectingFrom = null;
            this.render();
            dispatch(this.root, "zoerce:diagram:selectionchanged", { nodeIds: [], edgeId: null });
        }

        invokeAction(node) {
            const action = node.action;
            if (!action) return;
            const detail = { node, action, document: this.snapshot() };
            if (dispatch(this.root, "zoerce:diagram:action", detail, true)) {
                if (action.kind === "Navigate" && action.target && action.target.startsWith("/")) window.location.assign(action.target);
                if (action.kind === "Event") dispatch(this.root, action.target || "zoerce:diagram:custom-action", detail);
                if (action.kind === "Endpoint" && action.target && action.target.startsWith("/")) {
                    fetch(action.target, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(detail) })
                        .then(response => this.setStatus(response.ok ? "Node action completed." : "Node action could not be completed."))
                        .catch(() => this.setStatus("Node action could not be completed."));
                }
            }
        }

        toggleConnectionMode() {
            this.connectionMode = !this.connectionMode;
            this.connectingFrom = null;
            this.setStatus(this.connectionMode ? "Connection mode is on. Choose a source anchor, then a target anchor." : "Connection mode is off.");
            this.renderScene();
        }

        preferredPort(nodeId, source = true) {
            const node = this.node(nodeId);
            if (!node) return source ? "right" : "left";
            if (this.profile === "orgchart") return source ? "bottom" : "top";
            return source ? "right" : "left";
        }

        canConnectFrom(nodeId) {
            const node = this.node(nodeId);
            if (!node) return false;
            if (this.profile === "flowchart" && node.kind === "end") return false;
            return true;
        }

        canConnectTo(nodeId) {
            const node = this.node(nodeId);
            if (!node) return false;
            if (this.profile === "flowchart" && node.kind === "start") return false;
            return true;
        }

        handlePort(port) {
            const nodeId = port.closest("[data-z-diagram-node]")?.dataset.zDiagramNodeId;
            const side = key(port.dataset.zDiagramPortSide) || "right";
            if (!nodeId) return;
            if (!this.connectingFrom) {
                if (!this.canConnectFrom(nodeId)) {
                    this.setStatus("An end node cannot start a connection.");
                    return;
                }
                this.connectingFrom = { nodeId, side };
                this.connectionMode = true;
                this.setStatus("Source selected. Choose any anchor on the target node.");
                this.renderScene();
                return;
            }
            this.connectNodes(this.connectingFrom.nodeId, this.connectingFrom.side, nodeId, side);
        }

        connectNodes(sourceId, sourceSide, targetId, targetSide) {
            if (!sourceId || !targetId) return;
            if (key(sourceId) === key(targetId)) {
                this.setStatus("A node cannot connect to itself.");
                return;
            }
            if (!this.canConnectFrom(sourceId) || !this.canConnectTo(targetId)) {
                this.setStatus("That connection is not allowed for this diagram.");
                return;
            }
            if (this.document.edges.some(edge => key(edge.fromNodeId) === key(sourceId) && key(edge.toNodeId) === key(targetId))) {
                this.connectingFrom = null;
                this.setStatus("That connection already exists.");
                this.renderScene();
                return;
            }
            if ((this.profile === "mindmap" || this.profile === "orgchart") && this.document.edges.some(edge => key(edge.toNodeId) === key(targetId))) {
                this.setStatus("Tree nodes can only have one parent.");
                return;
            }
            this.connectingFrom = null;
            this.change("Connection added.", () => this.document.edges.push({
                id: idFor("edge", this.document.edges),
                fromNodeId: sourceId,
                toNodeId: targetId,
                label: null,
                tone: "Neutral",
                kind: null,
                fromPort: (sourceSide || this.preferredPort(sourceId, true)).replace(/^./, value => value.toUpperCase()),
                toPort: (targetSide || this.preferredPort(targetId, false)).replace(/^./, value => value.toUpperCase())
            }));
        }

        addNode(kind, droppedAt = null) {
            const normalizedKind = nodeKinds.has(key(kind)) ? key(kind) : "process";
            const index = this.document.nodes.length;
            const offset = index % 5;
            const size = this.nodeSize({ kind: normalizedKind });
            const snap = value => this.configuration.snapToGrid ? this.snap(value) : Math.round(value);
            const position = droppedAt
                ? {
                    x: clamp(snap(number(droppedAt.x) - size.width / 2), 0, canvasWidth - size.width),
                    y: clamp(snap(number(droppedAt.y) - size.height / 2), 0, canvasHeight - size.height)
                }
                : { x: this.snap(120 + offset * 190), y: this.snap(130 + Math.floor(index / 5) * 142) };
            this.change("New " + titleForKind(normalizedKind).toLowerCase() + " added.", () => {
                this.document.nodes.push({
                    id: idFor(normalizedKind, this.document.nodes),
                    kind: normalizedKind,
                    title: titleForKind(normalizedKind),
                    description: null,
                    tone: "Neutral",
                    collapsed: false,
                    action: null,
                    data: null,
                    position
                });
            });
            const node = this.document.nodes.at(-1);
            if (node) this.selectNode(node.id, false, false);
        }

        deleteSelection() {
            const selected = new Set(this.selection);
            const edgeId = this.selectedEdge;
            if (!selected.size && !edgeId) return;
            this.change("Selected diagram element removed.", () => {
                if (edgeId) this.document.edges = this.document.edges.filter(edge => edge.id !== edgeId);
                if (selected.size) {
                    this.document.nodes = this.document.nodes.filter(node => !selected.has(node.id));
                    this.document.edges = this.document.edges.filter(edge => !selected.has(edge.fromNodeId) && !selected.has(edge.toNodeId));
                }
            });
            this.selection.clear();
            this.selectedEdge = null;
        }

        duplicateSelection() {
            this.copySelection();
            this.pasteSelection();
        }

        copySelection() {
            const selected = this.document.nodes.filter(node => this.selection.has(node.id));
            if (!selected.length) {
                this.setStatus("Select one or more nodes to copy.");
                return;
            }
            const ids = new Set(selected.map(node => key(node.id)));
            this.nodeClipboard = {
                nodes: clone(selected),
                edges: clone(this.document.edges.filter(edge => ids.has(key(edge.fromNodeId)) && ids.has(key(edge.toNodeId))))
            };
            this.pasteSequence = 0;
            this.setStatus(selected.length === 1 ? "Node copied. Use Ctrl+V to paste." : selected.length + " nodes copied. Use Ctrl+V to paste.");
        }

        pasteSelection() {
            const clipboard = this.nodeClipboard;
            if (!clipboard?.nodes?.length) {
                this.setStatus("Copy nodes before pasting.");
                return;
            }
            this.pasteSequence = (this.pasteSequence || 0) + 1;
            const offset = this.pasteSequence * 32;
            this.change("Nodes pasted.", () => {
                const copies = [];
                const idMap = new Map();
                clipboard.nodes.forEach(node => {
                    const id = idFor(node.kind || "node", [...this.document.nodes, ...copies]);
                    idMap.set(node.id, id);
                    copies.push({ ...clone(node), id, title: node.title + " copy", position: { x: node.position.x + offset, y: node.position.y + offset } });
                });
                const copiedEdges = [];
                clipboard.edges
                    .filter(edge => idMap.has(edge.fromNodeId) && idMap.has(edge.toNodeId))
                    .forEach(edge => copiedEdges.push({ ...clone(edge), id: idFor("edge", [...this.document.edges, ...copiedEdges]), fromNodeId: idMap.get(edge.fromNodeId), toNodeId: idMap.get(edge.toNodeId) }));
                this.document.nodes.push(...copies);
                this.document.edges.push(...copiedEdges);
                this.selection = new Set(copies.map(node => node.id));
                this.selectedEdge = null;
            });
        }

        undo() {
            const previous = this.history.pop();
            if (!previous) {
                this.setStatus("Nothing to undo.");
                return;
            }
            this.future.push(this.snapshot());
            this.document = previous;
            this.selection.clear();
            this.selectedEdge = null;
            this.markDirty("Undid the latest edit.");
            this.emitChange("undo");
            this.render();
        }

        redo() {
            const next = this.future.pop();
            if (!next) {
                this.setStatus("Nothing to redo.");
                return;
            }
            this.history.push(this.snapshot());
            this.document = next;
            this.selection.clear();
            this.selectedEdge = null;
            this.markDirty("Restored the latest edit.");
            this.emitChange("redo");
            this.render();
        }

        visibleNodeIds() {
            if (!(this.profile === "mindmap" || this.profile === "orgchart")) return new Set(this.document.nodes.map(node => node.id));
            const hidden = new Set();
            const children = new Map();
            this.document.edges.forEach(edge => {
                if (!children.has(edge.fromNodeId)) children.set(edge.fromNodeId, []);
                children.get(edge.fromNodeId).push(edge.toNodeId);
            });
            const hideChildren = id => (children.get(id) || []).forEach(child => {
                hidden.add(child);
                hideChildren(child);
            });
            this.document.nodes.filter(node => node.collapsed).forEach(node => hideChildren(node.id));
            return new Set(this.document.nodes.filter(node => !hidden.has(node.id)).map(node => node.id));
        }

        render() {
            this.renderViewport();
            this.renderNavigator();
            this.renderInspector();
        }

        renderViewport() {
            this.renderTransform();
            this.renderScene();
            this.renderMinimap();
        }

        renderTransform() {
            if (!this.canvas) return;
            const scale = this.view.zoom;
            const grid = Math.max(8, number(this.configuration.gridSize, 24) * scale);
            const x = this.pixel(this.view.x);
            const y = this.pixel(this.view.y);
            this.canvas.style.setProperty("--z-diagram-grid-size", grid + "px");
            this.canvas.style.setProperty("--z-diagram-grid-x", (x - 1) + "px");
            this.canvas.style.setProperty("--z-diagram-grid-y", (y - 1) + "px");
            if (this.zoomOutput) this.zoomOutput.textContent = Math.round(this.view.zoom * 100) + "%";
        }

        pixel(value) {
            const density = Math.max(1, number(globalThis.devicePixelRatio, 1));
            return Math.round(value * density) / density;
        }

        project(point) {
            return {
                x: this.pixel(this.view.x + point.x * this.view.zoom),
                y: this.pixel(this.view.y + point.y * this.view.zoom)
            };
        }

        renderScene() {
            if (!this.world || !this.edgesLayer) return;
            this.canvas?.classList.toggle("is-connection-mode", this.connectionMode);
            if (this.connectToggle) {
                this.connectToggle.classList.toggle("is-active", this.connectionMode);
                this.connectToggle.setAttribute("aria-pressed", String(this.connectionMode));
                this.connectToggle.textContent = this.connectionMode ? "Connecting…" : "Connect";
            }
            const visible = this.visibleNodeIds();
            const nodes = this.document.nodes.filter(node => visible.has(node.id));
            const edges = this.document.edges.filter(edge => visible.has(edge.fromNodeId) && visible.has(edge.toNodeId));
            const width = Math.max(1, this.canvas?.clientWidth ?? canvasWidth);
            const height = Math.max(1, this.canvas?.clientHeight ?? canvasHeight);
            this.edgesLayer.replaceChildren(this.marker());
            this.edgesLayer.setAttribute("viewBox", "0 0 " + width + " " + height);
            this.edgesLayer.setAttribute("width", String(width));
            this.edgesLayer.setAttribute("height", String(height));
            if (this.profile === "orgchart") this.renderOrgEdges(edges);
            else edges.forEach(edge => this.renderEdge(edge, edges));
            this.renderConnectionPreview();
            const existing = new Map([...this.world.querySelectorAll("[data-z-diagram-node]")].map(element => [element.dataset.zDiagramNodeId, element]));
            nodes.forEach(node => {
                let element = existing.get(node.id);
                if (!element?.querySelector("[data-z-diagram-node-title]")) {
                    element?.remove();
                    element = this.renderNode(node);
                }
                this.updateNode(element, node);
                if (!element.parentElement) this.world.append(element);
                existing.delete(node.id);
            });
            existing.forEach(element => element.remove());
        }

        marker() {
            const defs = svg("defs");
            const marker = svg("marker");
            marker.id = this.root.id + "-arrow";
            marker.setAttribute("viewBox", "0 0 10 10");
            marker.setAttribute("refX", "8");
            marker.setAttribute("refY", "5");
            marker.setAttribute("markerWidth", "6");
            marker.setAttribute("markerHeight", "6");
            marker.setAttribute("orient", "auto-start-reverse");
            const path = svg("path");
            path.setAttribute("d", "M 0 0 L 10 5 L 0 10 z");
            path.setAttribute("fill", "context-stroke");
            marker.append(path);
            defs.append(marker);
            return defs;
        }

        autoPort(from, to, source) {
            if (this.profile === "orgchart") return source ? "bottom" : "top";
            const horizontal = Math.abs(to.position.x - from.position.x) >= Math.abs(to.position.y - from.position.y);
            if (horizontal) return source ? (to.position.x >= from.position.x ? "right" : "left") : (to.position.x >= from.position.x ? "left" : "right");
            return source ? (to.position.y >= from.position.y ? "bottom" : "top") : (to.position.y >= from.position.y ? "top" : "bottom");
        }

        portPoint(node, side, offset = 0) {
            const size = this.nodeSize(node);
            const centerX = node.position.x + size.width / 2;
            const centerY = node.position.y + size.height / 2;
            const logical = side === "top" ? { x: centerX + offset, y: node.position.y }
                : side === "bottom" ? { x: centerX + offset, y: node.position.y + size.height }
                    : side === "left" ? { x: node.position.x, y: centerY + offset }
                        : { x: node.position.x + size.width, y: centerY + offset };
            return this.project(logical);
        }

        nodeSize(node) {
            if (node?.kind === "decision") return { width: 216, height: 122 };
            // Elliptical nodes need a broader interior than a rectangular node:
            // their curved shoulders visually consume the available copy space.
            if (ellipseKinds.has(node?.kind)) return { width: 226, height: 104 };
            return { width: nodeWidth, height: nodeHeight };
        }

        edgeAnchors(edge, edges) {
            const from = this.node(edge.fromNodeId);
            const to = this.node(edge.toNodeId);
            if (!from || !to) return null;
            const outgoing = edges.filter(candidate => key(candidate.fromNodeId) === key(edge.fromNodeId)).sort((left, right) => left.id.localeCompare(right.id));
            const incoming = edges.filter(candidate => key(candidate.toNodeId) === key(edge.toNodeId)).sort((left, right) => left.id.localeCompare(right.id));
            const offsetFor = collection => clamp((collection.findIndex(candidate => candidate.id === edge.id) - (collection.length - 1) / 2) * 14, -22, 22);
            const fromSide = key(edge.fromPort) === "auto" || !edge.fromPort ? this.autoPort(from, to, true) : key(edge.fromPort);
            const toSide = key(edge.toPort) === "auto" || !edge.toPort ? this.autoPort(from, to, false) : key(edge.toPort);
            const sourceOffset = ["left", "right"].includes(fromSide) ? offsetFor(outgoing) : 0;
            const targetOffset = ["left", "right"].includes(toSide) ? offsetFor(incoming) : 0;
            return {
                start: this.portPoint(from, fromSide, sourceOffset),
                end: this.portPoint(to, toSide, targetOffset),
                fromSide,
                toSide,
                index: outgoing.findIndex(candidate => candidate.id === edge.id),
                count: outgoing.length
            };
        }

        renderEdge(edge, edges) {
            const anchors = this.edgeAnchors(edge, edges);
            if (!anchors) return;
            const { start, end, index, count, fromSide, toSide } = anchors;
            const curve = this.profile === "mindmap"
                ? this.curvePath(start, end, fromSide, toSide)
                : this.orthogonalPath(start, end, fromSide, toSide, index, count);
            this.renderEdgePath(edge, curve, this.edgeLabelPoint(start, end, index, count));
        }

        renderEdgePath(edge, curve, labelPoint, marker = true) {
            const line = svg("path", "z-diagram-edge z-diagram-edge--" + key(edge.tone));
            line.dataset.zDiagramEdge = "";
            line.dataset.zDiagramEdgeId = edge.id;
            const hitArea = svg("path", "z-diagram-edge-hit");
            hitArea.dataset.zDiagramEdge = "";
            hitArea.dataset.zDiagramEdgeId = edge.id;
            hitArea.setAttribute("d", curve);
            this.edgesLayer.append(hitArea);
            line.setAttribute("d", curve);
            if (marker) line.setAttribute("marker-end", "url(#" + this.root.id + "-arrow)");
            if (edge.style?.strokeColor) line.style.setProperty("--z-edge-stroke", edge.style.strokeColor);
            if (edge.style?.strokeWidth) line.style.setProperty("--z-edge-width", String(edge.style.strokeWidth));
            if (edge.style?.dashed) line.classList.add("is-dashed");
            if (edge.id === this.selectedEdge) line.classList.add("is-selected");
            this.edgesLayer.append(line);
            if (edge.label) {
                const label = svg("text", "z-diagram-edge-label");
                label.dataset.zDiagramEdge = "";
                label.dataset.zDiagramEdgeId = edge.id;
                label.setAttribute("x", String(labelPoint.x));
                label.setAttribute("y", String(labelPoint.y));
                label.setAttribute("text-anchor", "middle");
                if (edge.style?.strokeColor) label.style.setProperty("--z-edge-stroke", edge.style.strokeColor);
                label.textContent = edge.label;
                this.edgesLayer.append(label);
            }
        }

        curvePath(start, end, fromSide, toSide) {
            const handle = Math.max(46, Math.min(110, Math.abs(end.x - start.x) + Math.abs(end.y - start.y)) / 2);
            const vector = side => side === "top" ? [0, -1] : side === "bottom" ? [0, 1] : side === "left" ? [-1, 0] : [1, 0];
            const [sx, sy] = vector(fromSide);
            const [ex, ey] = vector(toSide);
            return "M " + start.x + " " + start.y + " C " + (start.x + sx * handle) + " " + (start.y + sy * handle) + ", " + (end.x + ex * handle) + " " + (end.y + ey * handle) + ", " + end.x + " " + end.y;
        }

        renderOrgEdges(edges) {
            const grouped = new Map();
            edges.forEach(edge => {
                const explicit = key(edge.fromPort) !== "auto" || key(edge.toPort) !== "auto";
                if (explicit) {
                    this.renderEdge(edge, edges);
                    return;
                }
                const keyId = key(edge.fromNodeId);
                if (!grouped.has(keyId)) grouped.set(keyId, []);
                grouped.get(keyId).push(edge);
            });
            grouped.forEach(group => {
                const anchors = group.map(edge => ({ edge, anchors: this.edgeAnchors(edge, edges) })).filter(item => item.anchors);
                if (anchors.length < 2) {
                    anchors.forEach(item => this.renderEdge(item.edge, edges));
                    return;
                }
                const start = anchors[0].anchors.start;
                const junctionY = (start.y + Math.min(...anchors.map(item => item.anchors.end.y))) / 2;
                const busStart = Math.min(start.x, ...anchors.map(item => item.anchors.end.x));
                const busEnd = Math.max(start.x, ...anchors.map(item => item.anchors.end.x));
                const trunk = "M " + start.x + " " + start.y + " V " + junctionY + " H " + busStart + " M " + start.x + " " + junctionY + " H " + busEnd;
                const trunkEdge = { ...anchors[0].edge, id: anchors[0].edge.id + "-bus", label: null };
                this.renderEdgePath(trunkEdge, trunk, { x: start.x, y: junctionY }, false);
                anchors.forEach(item => {
                    const end = item.anchors.end;
                    const branch = "M " + end.x + " " + junctionY + " V " + end.y;
                    this.renderEdgePath(item.edge, branch, { x: end.x, y: (junctionY + end.y) / 2 - 8 }, false);
                });
            });
        }

        renderConnectionPreview() {
            if (!this.connectionPreview || !this.canvas) return;
            const node = this.node(this.connectionPreview.nodeId);
            if (!node) return;
            const rect = this.canvas.getBoundingClientRect();
            const start = this.portPoint(node, this.connectionPreview.side);
            const target = this.node(this.connectionPreview.targetId);
            const end = target
                ? this.portPoint(target, this.connectionPreview.targetSide)
                : { x: this.connectionPreview.x - rect.left, y: this.connectionPreview.y - rect.top };
            const preview = svg("path", "z-diagram-edge z-diagram-edge--preview");
            preview.setAttribute("d", this.orthogonalPath(start, end, this.connectionPreview.side, this.connectionPreview.targetSide || "left", 0, 1));
            this.edgesLayer.append(preview);
        }

        orthogonalPath(start, end, fromSide, toSide, index, count) {
            const horizontalStart = ["left", "right"].includes(fromSide);
            const horizontalEnd = ["left", "right"].includes(toSide);
            const lane = (index - (count - 1) / 2) * 20;
            if (horizontalStart && horizontalEnd) {
                const middle = (start.x + end.x) / 2 + lane;
                return "M " + start.x + " " + start.y + " H " + middle + " V " + end.y + " H " + end.x;
            }
            if (!horizontalStart && !horizontalEnd) {
                const middle = (start.y + end.y) / 2 + lane;
                return "M " + start.x + " " + start.y + " V " + middle + " H " + end.x + " V " + end.y;
            }
            const outward = 42 + Math.abs(lane);
            const sourceBend = horizontalStart
                ? { x: start.x + (fromSide === "right" ? outward : -outward), y: start.y }
                : { x: start.x, y: start.y + (fromSide === "bottom" ? outward : -outward) };
            const targetBend = horizontalEnd
                ? { x: end.x + (toSide === "right" ? outward : -outward), y: end.y }
                : { x: end.x, y: end.y + (toSide === "bottom" ? outward : -outward) };
            return "M " + start.x + " " + start.y + " L " + sourceBend.x + " " + sourceBend.y + " L " + targetBend.x + " " + targetBend.y + " L " + end.x + " " + end.y;
        }

        edgeLabelPoint(start, end, index, count) {
            if (this.profile === "mindmap" || this.profile === "orgchart") return { x: (start.x + end.x) / 2, y: (start.y + end.y) / 2 - 8 };
            const lane = (index - (count - 1) / 2) * 22;
            return { x: (start.x + end.x) / 2 + lane, y: (start.y + end.y) / 2 - 8 };
        }

        renderNode(node) {
            const size = this.nodeSize(node);
            const element = create("article", "z-diagram-node z-diagram-node--" + node.kind + " z-diagram-node--" + key(node.tone));
            element.dataset.zDiagramNode = "";
            element.dataset.zDiagramNodeId = node.id;
            element.dataset.zDiagramNodeKind = node.kind;
            element.style.setProperty("--z-node-x", node.position.x + "px");
            element.style.setProperty("--z-node-y", node.position.y + "px");
            element.tabIndex = 0;
            element.setAttribute("role", "button");
            element.setAttribute("aria-label", node.title);
            if (this.selection.has(node.id)) element.classList.add("is-selected");
            if (this.connectingFrom?.nodeId === node.id) element.classList.add("is-connecting");
            if (this.connectionPreview?.targetId === node.id) element.classList.add("is-connection-target");
            const surface = svg("svg", "z-diagram-node-surface");
            surface.setAttribute("viewBox", "0 0 " + size.width + " " + size.height);
            surface.setAttribute("aria-hidden", "true");
            const compact = node.kind === "connector";
            const icon = create("div", "z-diagram-node-icon");
            icon.textContent = iconForKind(node.kind);
            const copy = create("div", "z-diagram-node-copy");
            if (compact) copy.classList.add("is-compact");
            const title = create("strong");
            title.dataset.zDiagramNodeTitle = "";
            title.textContent = node.title;
            copy.append(title);
            const description = create("small");
            description.dataset.zDiagramNodeDescription = "";
            description.textContent = node.description ?? "";
            description.hidden = node.kind === "decision" || compact || !node.description;
            copy.append(description);
            element.append(surface);
            if (!compact) element.append(icon);
            element.append(copy);
            if (this.editable) portOrder.forEach(side => {
                const port = create("button", "z-diagram-port z-diagram-port--" + side);
                port.type = "button";
                port.dataset.zDiagramPort = "";
                port.dataset.zDiagramPortSide = side;
                port.title = "Connect from or to the " + side + " side";
                port.setAttribute("aria-label", "Connection anchor on the " + side + " side of " + node.title);
                element.append(port);
            });
            return element;
        }

        updateNode(element, node) {
            element.className = "z-diagram-node z-diagram-node--" + node.kind + " z-diagram-node--" + key(node.tone);
            element.dataset.zDiagramNodeId = node.id;
            element.dataset.zDiagramNodeKind = node.kind;
            element.setAttribute("aria-label", node.title);
            if (this.selection.has(node.id)) element.classList.add("is-selected");
            if (this.connectingFrom?.nodeId === node.id) element.classList.add("is-connecting");
            if (this.connectionPreview?.targetId === node.id) element.classList.add("is-connection-target");
            const title = element.querySelector("[data-z-diagram-node-title]");
            const description = element.querySelector("[data-z-diagram-node-description]");
            if (title) title.textContent = node.title;
            if (description) {
                description.textContent = node.description ?? "";
                description.hidden = node.kind === "decision" || node.kind === "connector" || !node.description;
            }
            this.applyNodePresentation(element, node, node.style);
        }

        applyNodePresentation(element, node, style) {
            const point = this.project(node.position);
            const scale = this.view.zoom;
            const size = this.nodeSize(node);
            element.style.setProperty("--z-node-x", point.x + "px");
            element.style.setProperty("--z-node-y", point.y + "px");
            element.style.setProperty("--z-node-scale", String(scale));
            element.style.setProperty("--z-node-width", size.width * scale + "px");
            element.style.setProperty("--z-node-height", size.height * scale + "px");
            const values = [
                ["--z-node-border-color", style?.borderColor],
                ["--z-node-text-color", style?.textColor],
                ["--z-node-title-size", style?.fontSize ? style.fontSize + "px" : null],
                ["--z-node-font-weight", style?.fontWeight ? String(style.fontWeight) : null],
                ["--z-node-font-family", this.fontFamily(style?.fontFamily)]
            ];
            values.forEach(([name, value]) => value ? element.style.setProperty(name, value) : element.style.removeProperty(name));
            if (style?.opacity) element.style.opacity = String(style.opacity);
            else element.style.removeProperty("opacity");
            this.renderNodeSurface(element, node, style);
        }

        shapePath(kind, radius, width = nodeWidth, height = nodeHeight) {
            const nodeWidth = width;
            const nodeHeight = height;
            const rounded = "M " + radius + " 2 H " + (nodeWidth - radius) + " Q " + (nodeWidth - 2) + " 2 " + (nodeWidth - 2) + " " + radius + " V " + (nodeHeight - radius) + " Q " + (nodeWidth - 2) + " " + (nodeHeight - 2) + " " + (nodeWidth - radius) + " " + (nodeHeight - 2) + " H " + radius + " Q 2 " + (nodeHeight - 2) + " 2 " + (nodeHeight - radius) + " V " + radius + " Q 2 2 " + radius + " 2 Z";
            const capsuleRadius = Math.max(24, Math.min(nodeHeight / 2 - 2, 52));
            const capsule = "M " + capsuleRadius + " 2 H " + (nodeWidth - capsuleRadius) + " A " + capsuleRadius + " " + capsuleRadius + " 0 0 1 " + (nodeWidth - capsuleRadius) + " " + (nodeHeight - 2) + " H " + capsuleRadius + " A " + capsuleRadius + " " + capsuleRadius + " 0 0 1 " + capsuleRadius + " 2 Z";
            const shapes = {
                start: capsule,
                end: capsule,
                decision: "M " + (nodeWidth / 2) + " 2 L " + (nodeWidth - 2) + " " + (nodeHeight / 2) + " L " + (nodeWidth / 2) + " " + (nodeHeight - 2) + " L 2 " + (nodeHeight / 2) + " Z",
                "input-output": "M 21 2 H " + (nodeWidth - 2) + " L " + (nodeWidth - 21) + " " + (nodeHeight - 2) + " H 2 Z",
                subprocess: rounded,
                document: "M 3 2 H " + (nodeWidth - 3) + " V " + (nodeHeight - 21) + " C " + (nodeWidth - 43) + " " + (nodeHeight - 1) + " " + (nodeWidth - 72) + " " + (nodeHeight + 4) + " " + (nodeWidth - 112) + " " + (nodeHeight - 2) + " H 3 Z",
                "data-store": "M 3 13 C 3 -2 " + (nodeWidth - 3) + " -2 " + (nodeWidth - 3) + " 13 V " + (nodeHeight - 13) + " C " + (nodeWidth - 3) + " " + (nodeHeight + 2) + " 3 " + (nodeHeight + 2) + " 3 " + (nodeHeight - 13) + " Z",
                database: "M 3 13 C 3 -2 " + (nodeWidth - 3) + " -2 " + (nodeWidth - 3) + " 13 V " + (nodeHeight - 13) + " C " + (nodeWidth - 3) + " " + (nodeHeight + 2) + " 3 " + (nodeHeight + 2) + " 3 " + (nodeHeight - 13) + " Z",
                delay: "M 3 2 H " + (nodeWidth - 48) + " A 42 42 0 0 1 " + (nodeWidth - 48) + " " + (nodeHeight - 2) + " H 3 Z",
                connector: "M " + (nodeWidth / 2) + " 2 A " + (nodeHeight / 2 - 2) + " " + (nodeHeight / 2 - 2) + " 0 1 1 " + (nodeWidth / 2 - .1) + " 2 Z",
                annotation: "M 26 2 H " + (nodeWidth - 2) + " V " + (nodeHeight - 2) + " H 26 M 26 2 V " + (nodeHeight - 2),
                root: "M 24 2 H " + (nodeWidth - 24) + " Q " + (nodeWidth - 2) + " 2 " + (nodeWidth - 2) + " 24 V " + (nodeHeight - 24) + " Q " + (nodeWidth - 2) + " " + (nodeHeight - 2) + " " + (nodeWidth - 24) + " " + (nodeHeight - 2) + " H 24 Q 2 " + (nodeHeight - 2) + " 2 " + (nodeHeight - 24) + " V 24 Q 2 2 24 2 Z",
                branch: "M 23 2 H " + (nodeWidth - 23) + " L " + (nodeWidth - 2) + " " + (nodeHeight / 2) + " L " + (nodeWidth - 23) + " " + (nodeHeight - 2) + " H 23 L 2 " + (nodeHeight / 2) + " Z",
                topic: capsule,
                note: "M 3 2 H " + (nodeWidth - 29) + " L " + (nodeWidth - 2) + " 29 V " + (nodeHeight - 2) + " H 3 Z M " + (nodeWidth - 29) + " 2 V 29 H " + (nodeWidth - 2),
                person: rounded,
                team: "M 19 2 H " + (nodeWidth - 19) + " L " + (nodeWidth - 2) + " 20 V " + (nodeHeight - 2) + " H 2 V 20 Z",
                vacancy: "M 23 2 H " + (nodeWidth - 23) + " L " + (nodeWidth - 2) + " " + (nodeHeight / 2) + " L " + (nodeWidth - 23) + " " + (nodeHeight - 2) + " H 23 L 2 " + (nodeHeight / 2) + " Z",
                user: "M 20 2 H " + (nodeWidth - 20) + " Q " + (nodeWidth - 2) + " 2 " + (nodeWidth - 2) + " 20 V " + (nodeHeight - 2) + " H 2 V 20 Q 2 2 20 2 Z",
                service: "M 23 2 H " + (nodeWidth - 23) + " L " + (nodeWidth - 2) + " " + (nodeHeight / 2) + " L " + (nodeWidth - 23) + " " + (nodeHeight - 2) + " H 23 L 2 " + (nodeHeight / 2) + " Z",
                queue: "M 18 2 H " + (nodeWidth - 18) + " L " + (nodeWidth - 2) + " 17 V " + (nodeHeight - 17) + " L " + (nodeWidth - 18) + " " + (nodeHeight - 2) + " H 18 L 2 " + (nodeHeight - 17) + " V 17 Z",
                external: "M 23 2 H " + (nodeWidth - 2) + " V " + (nodeHeight - 24) + " L " + (nodeWidth - 24) + " " + (nodeHeight - 2) + " H 2 V 23 Z",
                boundary: rounded
            };
            return shapes[kind] ?? rounded;
        }

        renderNodeSurface(element, node, style) {
            const surface = element.querySelector(".z-diagram-node-surface");
            if (!surface) return;
            const size = this.nodeSize(node);
            surface.setAttribute("viewBox", "0 0 " + size.width + " " + size.height);
            const defs = svg("defs");
            const path = svg("path", "z-diagram-node-shape");
            path.setAttribute("d", this.shapePath(node.kind, clamp(number(style?.cornerRadius, 13), 0, 38), size.width, size.height));
            const fill = style?.fillColor;
            const stroke = style?.borderColor;
            if (style?.gradient) {
                const gradient = svg("linearGradient");
                const id = this.root.id + "-gradient-" + node.id.replace(/[^a-z0-9_-]/gi, "-");
                gradient.id = id;
                gradient.setAttribute("x1", "0%"); gradient.setAttribute("y1", "0%");
                gradient.setAttribute("x2", "100%"); gradient.setAttribute("y2", "100%");
                const first = svg("stop"); first.setAttribute("offset", "0%"); first.setAttribute("stop-color", style.gradient.startColor);
                const last = svg("stop"); last.setAttribute("offset", "100%"); last.setAttribute("stop-color", style.gradient.endColor);
                gradient.append(first, last); defs.append(gradient); path.setAttribute("fill", "url(#" + id + ")");
            } else if (fill) path.setAttribute("fill", fill);
            if (stroke) path.setAttribute("stroke", stroke);
            if (style?.borderWidth) path.setAttribute("stroke-width", String(style.borderWidth));
            if (node.kind === "vacancy" || node.kind === "boundary") path.setAttribute("stroke-dasharray", "6 4");
            surface.replaceChildren(defs, path);
        }

        fontFamily(value) {
            return ({
                rounded: 'ui-rounded, "Nunito", "Segoe UI", sans-serif',
                mono: 'ui-monospace, "Cascadia Code", Consolas, monospace',
                serif: 'Iowan Old Style, Georgia, serif',
                system: 'Inter, "Segoe UI Variable Text", "Segoe UI", sans-serif'
            })[key(value)] ?? null;
        }

        renderNavigator() {
            if (!this.navigator) return;
            this.navigator.replaceChildren(...this.document.nodes.map(node => {
                const item = create("li");
                const button = create("button");
                button.type = "button";
                button.dataset.zDiagramNavigate = node.id;
                button.textContent = node.title;
                if (this.selection.has(node.id)) button.classList.add("is-current");
                item.append(button);
                return item;
            }));
        }

        renderInspector() {
            if (!this.inspector) return;
            const selected = this.document.nodes.find(node => this.selection.has(node.id));
            const edge = this.selectedEdge ? this.edge(this.selectedEdge) : null;
            this.inspector.replaceChildren();
            if (!selected && !edge) {
                const title = create("strong"); title.textContent = "Inspector";
                const copy = create("p"); copy.textContent = "Select a node or connection to edit its details.";
                this.inspector.append(title, copy);
                return;
            }
            if (selected) this.inspector.append(this.nodeInspector(selected));
            if (edge) this.inspector.append(this.edgeInspector(edge));
        }

        nodeInspector(node) {
            const fragment = document.createDocumentFragment();
            const eyebrow = create("span", "z-section-kicker"); eyebrow.textContent = node.kind;
            const title = create("strong"); title.textContent = "Node inspector";
            const tabs = create("div", "z-diagram-inspector-tabs"); tabs.setAttribute("role", "tablist");
            [
                ["content", "Content"],
                ["appearance", "Appearance"]
            ].forEach(([value, label]) => {
                const button = create("button"); button.type = "button"; button.textContent = label; button.dataset.zDiagramInspectorTab = value; button.setAttribute("role", "tab");
                button.setAttribute("aria-selected", String(this.inspectorTab === value));
                if (this.inspectorTab === value) button.classList.add("is-active");
                tabs.append(button);
            });
            fragment.append(eyebrow, title, tabs, this.inspectorTab === "appearance" ? this.nodeAppearanceInspector(node) : this.nodeContentInspector(node));
            return fragment;
        }

        nodeContentInspector(node) {
            const form = create("div", "z-diagram-form");
            const titleLabel = create("label"); titleLabel.textContent = "Title";
            const titleInput = create("input", "z-input"); titleInput.value = node.title; titleInput.dataset.zDiagramField = "title";
            titleLabel.append(titleInput);
            const descriptionLabel = create("label"); descriptionLabel.textContent = "Description";
            const descriptionInput = create("textarea", "z-textarea"); descriptionInput.value = node.description ?? ""; descriptionInput.dataset.zDiagramField = "description";
            descriptionLabel.append(descriptionInput);
            const toneLabel = create("label"); toneLabel.textContent = "Tone";
            const toneSelect = create("select", "z-select"); toneSelect.dataset.zDiagramField = "tone";
            ["Neutral", "Primary", "Success", "Warning", "Danger", "Info"].forEach(tone => {
                const option = create("option"); option.value = tone; option.textContent = tone; option.selected = node.tone === tone; toneSelect.append(option);
            });
            toneLabel.append(toneSelect);
            const actionLabel = create("label"); actionLabel.textContent = "Click action";
            const actionSelect = create("select", "z-select"); actionSelect.dataset.zDiagramField = "action-kind";
            ["None", "Panel", "Modal", "Event", "Command", "Navigate", "Endpoint"].forEach(kind => {
                const option = create("option"); option.value = kind; option.textContent = kind; option.selected = (node.action?.kind ?? "None") === kind; actionSelect.append(option);
            });
            actionLabel.append(actionSelect);
            const targetLabel = create("label"); targetLabel.textContent = "Action target";
            const targetInput = create("input", "z-input"); targetInput.value = node.action?.target ?? ""; targetInput.dataset.zDiagramField = "action-target"; targetInput.placeholder = "e.g. details-panel";
            targetLabel.append(targetInput);
            const actions = create("div", "z-diagram-inspector-actions");
            if (this.profile === "mindmap" || this.profile === "orgchart") {
                const collapse = create("button", "z-button z-button-ghost z-button-sm"); collapse.type = "button"; collapse.textContent = node.collapsed ? "Expand branch" : "Collapse branch"; collapse.dataset.zDiagramInspector = "toggle-collapse"; actions.append(collapse);
            }
            form.append(titleLabel, descriptionLabel, toneLabel, actionLabel, targetLabel);
            if (actions.childElementCount) form.append(actions);
            return form;
        }

        nodeAppearanceInspector(node) {
            const style = node.style ?? {};
            const form = create("div", "z-diagram-form z-diagram-appearance-form");
            const surface = create("strong", "z-diagram-form-heading"); surface.textContent = "Surface";
            const fill = this.colorField("Fill", "fillColor", colorForInput(style.fillColor, "#7c5cff"));
            const gradient = create("label", "z-diagram-switch"); gradient.textContent = "Use a gradient";
            const gradientToggle = create("input"); gradientToggle.type = "checkbox"; gradientToggle.checked = Boolean(style.gradient); gradientToggle.dataset.zDiagramStyleField = "gradientEnabled"; gradient.append(gradientToggle);
            const gradientStart = this.colorField("Gradient start", "gradientStart", colorForInput(style.gradient?.startColor, "#7c5cff"));
            const gradientEnd = this.colorField("Gradient end", "gradientEnd", colorForInput(style.gradient?.endColor, "#22c8a2"));
            const angle = this.rangeField("Gradient angle", "gradientAngle", style.gradient?.angle ?? 135, 0, 360, "°");
            const borderHeading = create("strong", "z-diagram-form-heading"); borderHeading.textContent = "Border & shape";
            const border = this.colorField("Border", "borderColor", colorForInput(style.borderColor, "#8f7bff"));
            const borderWidth = this.rangeField("Width", "borderWidth", style.borderWidth ?? 1, 1, 8, "px");
            const radius = this.rangeField("Corner radius", "cornerRadius", style.cornerRadius ?? 13, 0, 40, "px");
            const typography = create("strong", "z-diagram-form-heading"); typography.textContent = "Typography";
            const textColor = this.colorField("Text", "textColor", colorForInput(style.textColor, "#f8f7ff"));
            const familyLabel = create("label"); familyLabel.textContent = "Family";
            const family = create("select", "z-select"); family.dataset.zDiagramStyleField = "fontFamily";
            [["system", "System"], ["rounded", "Rounded"], ["mono", "Mono"], ["serif", "Serif"]].forEach(([value, label]) => { const option = create("option"); option.value = value; option.textContent = label; option.selected = (style.fontFamily ?? "system") === value; family.append(option); });
            familyLabel.append(family);
            const fontSize = this.rangeField("Title size", "fontSize", style.fontSize ?? 11, 10, 24, "px");
            const weightLabel = create("label"); weightLabel.textContent = "Weight";
            const weight = create("select", "z-select"); weight.dataset.zDiagramStyleField = "fontWeight";
            [500, 600, 700, 800, 900].forEach(value => { const option = create("option"); option.value = String(value); option.textContent = String(value); option.selected = (style.fontWeight ?? 800) === value; weight.append(option); });
            weightLabel.append(weight);
            const opacity = this.rangeField("Opacity", "opacity", Math.round((style.opacity ?? 1) * 100), 15, 100, "%");
            const palette = this.stylePalette();
            const advice = create("p", "z-diagram-contrast-advice"); advice.dataset.zDiagramContrast = ""; advice.textContent = this.contrastAdvice(style);
            form.append(surface, fill, gradient, gradientStart, gradientEnd, angle, palette, borderHeading, border, borderWidth, radius, typography, textColor, familyLabel, fontSize, weightLabel, opacity, advice);
            return form;
        }

        colorField(label, field, value) {
            const wrapper = create("label", "z-diagram-color-field"); wrapper.textContent = label;
            const input = create("input"); input.type = "color"; input.value = value; input.dataset.zDiagramStyleField = field;
            wrapper.append(input);
            return wrapper;
        }

        rangeField(label, field, value, minimum, maximum, suffix) {
            const wrapper = create("label", "z-diagram-range-field");
            const copy = create("span"); copy.textContent = label;
            const output = create("output"); output.textContent = String(value) + suffix;
            const input = create("input"); input.type = "range"; input.min = String(minimum); input.max = String(maximum); input.value = String(value); input.dataset.zDiagramStyleField = field;
            input.addEventListener("input", () => { output.textContent = input.value + suffix; }, { signal: this.abortController.signal });
            wrapper.append(copy, output, input);
            return wrapper;
        }

        stylePalette() {
            const wrapper = create("div", "z-diagram-style-palette");
            const label = create("span"); label.textContent = "Theme palette";
            const colors = ["#7c5cff", "#22c8a2", "#3b82f6", "#f59e0b", "#ef476f", "#f8f7ff"];
            const choices = create("div");
            colors.forEach(value => {
                const button = create("button"); button.type = "button"; button.title = value; button.style.setProperty("--z-swatch", value); button.dataset.zDiagramStyleSwatch = "fillColor"; button.dataset.zDiagramStyleValue = value; choices.append(button);
            });
            wrapper.append(label, choices);
            return wrapper;
        }

        styleValue(field) {
            return this.inspector?.querySelector("[data-z-diagram-style-field='" + CSS.escape(field) + "']")?.value;
        }

        styleChecked(field) {
            return Boolean(this.inspector?.querySelector("[data-z-diagram-style-field='" + CSS.escape(field) + "']")?.checked);
        }

        styleFromInspector() {
            const gradient = this.styleChecked("gradientEnabled")
                ? { startColor: this.styleValue("gradientStart"), endColor: this.styleValue("gradientEnd"), angle: this.styleValue("gradientAngle") }
                : null;
            return normalizeNodeStyle({
                fillColor: this.styleValue("fillColor"), borderColor: this.styleValue("borderColor"), borderWidth: this.styleValue("borderWidth"),
                textColor: this.styleValue("textColor"), fontFamily: this.styleValue("fontFamily"), fontSize: this.styleValue("fontSize"),
                fontWeight: this.styleValue("fontWeight"), cornerRadius: this.styleValue("cornerRadius"), opacity: number(this.styleValue("opacity"), 100) / 100, gradient
            });
        }

        previewNodeStyle() {
            const selected = this.document.nodes.find(node => this.selection.has(node.id));
            if (!selected) return;
            const style = this.styleFromInspector();
            const element = this.world?.querySelector("[data-z-diagram-node-id='" + CSS.escape(selected.id) + "']");
            if (element) this.applyNodePresentation(element, selected, style);
            const advice = this.inspector?.querySelector("[data-z-diagram-contrast]");
            if (advice) advice.textContent = this.contrastAdvice(style ?? {});
        }

        edgeInspector(edge) {
            const fragment = document.createDocumentFragment();
            const eyebrow = create("span", "z-section-kicker"); eyebrow.textContent = "Connection";
            const title = create("strong"); title.textContent = "Edge inspector";
            const form = create("div", "z-diagram-form");
            const label = create("label"); label.textContent = "Label";
            const input = create("input", "z-input"); input.value = edge.label ?? ""; input.dataset.zDiagramField = "edge-label"; label.append(input);
            const stroke = this.colorField("Stroke", "edgeStroke", colorForInput(edge.style?.strokeColor, "#7c5cff"));
            const width = this.rangeField("Stroke width", "edgeWidth", edge.style?.strokeWidth ?? 2, 1, 6, "px");
            const dashed = create("label", "z-diagram-switch"); dashed.textContent = "Dashed";
            const dashedInput = create("input"); dashedInput.type = "checkbox"; dashedInput.checked = Boolean(edge.style?.dashed); dashedInput.dataset.zDiagramStyleField = "edgeDashed"; dashed.append(dashedInput);
            form.append(label, stroke, width, dashed);
            fragment.append(eyebrow, title, form);
            return fragment;
        }

        handleInspector(action) {
            const selected = this.document.nodes.find(node => this.selection.has(node.id));
            if (action === "toggle-collapse" && selected) this.toggleCollapsed(selected.id);
        }

        contrastAdvice(style) {
            if (style.gradient) return "Gradient preview is active. Verify text contrast at both ends before saving.";
            const foreground = this.hexRgb(style.textColor);
            const background = this.hexRgb(style.fillColor);
            if (!foreground || !background) return "Theme-aware colors will be used when no custom color is supplied.";
            const ratio = this.contrastRatio(foreground, background);
            return ratio >= 4.5
                ? "Contrast " + ratio.toFixed(1) + ":1 — readable for normal text."
                : "Contrast " + ratio.toFixed(1) + ":1 — choose a stronger text or fill color.";
        }

        hexRgb(value) {
            const match = /^#([0-9a-f]{6})$/i.exec(text(value));
            return match ? [parseInt(match[1].slice(0, 2), 16), parseInt(match[1].slice(2, 4), 16), parseInt(match[1].slice(4, 6), 16)] : null;
        }

        contrastRatio(foreground, background) {
            const luminance = rgb => {
                const values = rgb.map(value => value / 255).map(value => value <= .03928 ? value / 12.92 : Math.pow((value + .055) / 1.055, 2.4));
                return values[0] * .2126 + values[1] * .7152 + values[2] * .0722;
            };
            const first = luminance(foreground);
            const second = luminance(background);
            return (Math.max(first, second) + .05) / (Math.min(first, second) + .05);
        }

        toggleCollapsed(nodeId) {
            this.change("Branch visibility changed.", () => {
                const target = this.node(nodeId);
                if (target) target.collapsed = !target.collapsed;
            });
        }

        renderMinimap() {
            if (!this.minimap || !this.overview) return;
            const bounds = this.documentBounds();
            const width = Math.max(1, this.minimap.clientWidth || 130);
            const height = Math.max(1, this.minimap.clientHeight || 74);
            const inset = 7;
            const scale = Math.min((width - inset * 2) / Math.max(bounds.width, nodeWidth), (height - inset * 2) / Math.max(bounds.height, nodeHeight));
            const useful = Boolean(this.configuration.showMinimap) && (this.document.nodes.length > 4 || bounds.width * this.view.zoom > (this.canvas?.clientWidth ?? 0) * .92 || bounds.height * this.view.zoom > (this.canvas?.clientHeight ?? 0) * .92);
            if (!this.overviewInitialized) {
                this.overviewVisible = useful;
                this.overviewInitialized = true;
            }
            if (!useful) this.overviewVisible = false;
            this.overview.hidden = !this.overviewVisible;
            if (this.overviewToggle) this.overviewToggle.hidden = !useful;
            const marks = this.document.nodes.map(node => {
                const mark = create("i");
                mark.style.left = (inset + (node.position.x - bounds.minX) * scale) + "px";
                mark.style.top = (inset + (node.position.y - bounds.minY) * scale) + "px";
                if (this.selection.has(node.id)) mark.classList.add("is-selected");
                return mark;
            });
            const frame = create("span", "z-diagram-overview-frame");
            const documentX = -this.view.x / this.view.zoom;
            const documentY = -this.view.y / this.view.zoom;
            frame.style.left = (inset + (documentX - bounds.minX) * scale) + "px";
            frame.style.top = (inset + (documentY - bounds.minY) * scale) + "px";
            frame.style.width = Math.max(18, ((this.canvas?.clientWidth ?? 1) / this.view.zoom) * scale) + "px";
            frame.style.height = Math.max(14, ((this.canvas?.clientHeight ?? 1) / this.view.zoom) * scale) + "px";
            this.minimap.replaceChildren(...marks, frame);
            this.overviewLayout = { bounds, scale, inset };
        }

        documentBounds() {
            if (!this.document.nodes.length) return { minX: 0, minY: 0, width: nodeWidth, height: nodeHeight };
            const padding = 120;
            const xs = this.document.nodes.map(node => node.position.x);
            const ys = this.document.nodes.map(node => node.position.y);
            const minX = Math.min(...xs) - padding;
            const minY = Math.min(...ys) - padding;
            const maxX = Math.max(...this.document.nodes.map(node => node.position.x + this.nodeSize(node).width)) + padding;
            const maxY = Math.max(...this.document.nodes.map(node => node.position.y + this.nodeSize(node).height)) + padding;
            return { minX, minY, width: maxX - minX, height: maxY - minY };
        }

        toggleOverview() {
            if (!this.overview || this.overviewToggle?.hidden) return;
            this.overviewVisible = !this.overviewVisible;
            this.renderMinimap();
            this.setStatus(this.overviewVisible ? "Diagram overview shown." : "Diagram overview hidden.");
        }

        toggleFocusMode(active = !this.focusMode) {
            const next = Boolean(active);
            if (this.focusMode === next) return;
            this.focusMode = next;
            if (this.focusToggle) {
                this.focusToggle.classList.toggle("is-active", next);
                this.focusToggle.setAttribute("aria-pressed", String(next));
                this.focusToggle.setAttribute("aria-label", next ? "Exit focus mode" : "Enter focus mode");
                this.focusToggle.title = next ? "Exit focus mode" : "Focus canvas";
            }
            if (next) {
                this.focusReturn = document.activeElement;
                // A transformed app shell creates a containing block for fixed
                // descendants. Portal the diagram while focused so it can use the
                // actual viewport, then restore it exactly where it came from.
                const parent = this.root.parentNode;
                if (parent && !this.focusPlaceholder) {
                    this.focusPlaceholder = document.createComment("zoorce-diagram-focus-placeholder");
                    parent.insertBefore(this.focusPlaceholder, this.root);
                    document.body.append(this.root);
                }
                this.root.classList.add("is-focus-mode");
                document.documentElement.classList.add("z-diagram-focus-active");
                document.body.classList.add("z-diagram-focus-active");
                this.setStatus("Focus mode enabled. Press Escape to return.");
                globalThis.requestAnimationFrame(() => {
                    // Focus intentionally preserves the user's zoom and viewport.
                    // It provides more available canvas; it must not reframe work.
                    this.renderViewport();
                    this.canvas?.focus({ preventScroll: true });
                });
                return;
            }
            this.root.classList.remove("is-focus-mode");
            if (this.focusPlaceholder?.parentNode) this.focusPlaceholder.parentNode.replaceChild(this.root, this.focusPlaceholder);
            this.focusPlaceholder = null;
            document.documentElement.classList.remove("z-diagram-focus-active");
            document.body.classList.remove("z-diagram-focus-active");
            this.setStatus("Focus mode closed.");
            globalThis.requestAnimationFrame(() => this.renderViewport());
            this.focusReturn?.focus?.({ preventScroll: true });
            this.focusReturn = null;
        }

        onOverviewPointerDown(event) {
            if (!this.overviewLayout) return;
            this.overviewDrag = true;
            this.minimap?.setPointerCapture?.(event.pointerId);
            this.navigateOverview(event);
            event.preventDefault();
        }

        onOverviewPointerMove(event) {
            if (this.overviewDrag) this.navigateOverview(event);
        }

        navigateOverview(event) {
            if (!this.minimap || !this.canvas || !this.overviewLayout) return;
            const rect = this.minimap.getBoundingClientRect();
            const { bounds, scale, inset } = this.overviewLayout;
            const x = bounds.minX + clamp((event.clientX - rect.left - inset) / scale, 0, bounds.width);
            const y = bounds.minY + clamp((event.clientY - rect.top - inset) / scale, 0, bounds.height);
            this.view.x = this.canvas.clientWidth / 2 - x * this.view.zoom;
            this.view.y = this.canvas.clientHeight / 2 - y * this.view.zoom;
            this.renderViewport();
        }

        focus(nodeId) {
            const node = this.node(nodeId);
            if (!node) return;
            this.selectNode(node.id, false, false);
            const width = this.canvas?.clientWidth ?? 700;
            const height = this.canvas?.clientHeight ?? 500;
            const size = this.nodeSize(node);
            this.view.x = width / 2 - (node.position.x + size.width / 2) * this.view.zoom;
            this.view.y = height / 2 - (node.position.y + size.height / 2) * this.view.zoom;
            this.renderViewport();
            this.world?.querySelector("[data-z-diagram-node-id='" + CSS.escape(node.id) + "']")?.focus();
        }

        setZoom(value) {
            this.view.zoom = clamp(value, number(this.configuration.minimumZoom, .4), number(this.configuration.maximumZoom, 2.4));
            this.renderViewport();
        }

        fit(announce) {
            if (!this.document.nodes.length || !this.canvas) return;
            const xs = this.document.nodes.map(node => node.position.x);
            const ys = this.document.nodes.map(node => node.position.y);
            const minX = Math.min(...xs);
            const maxX = Math.max(...this.document.nodes.map(node => node.position.x + this.nodeSize(node).width));
            const minY = Math.min(...ys);
            const maxY = Math.max(...this.document.nodes.map(node => node.position.y + this.nodeSize(node).height));
            const zoom = clamp(Math.min((this.canvas.clientWidth - 80) / Math.max(maxX - minX, 160), (this.canvas.clientHeight - 80) / Math.max(maxY - minY, 120)), number(this.configuration.minimumZoom, .4), number(this.configuration.maximumZoom, 2.4));
            this.view.zoom = zoom;
            this.view.x = (this.canvas.clientWidth - (maxX + minX) * zoom) / 2;
            this.view.y = (this.canvas.clientHeight - (maxY + minY) * zoom) / 2;
            this.renderViewport();
            if (announce) this.setStatus("Canvas fitted to the current diagram.");
        }

        applyLayout() {
            this.change("Diagram arranged.", () => {
                if (this.profile === "mindmap") this.radialLayout();
                else if (this.profile === "orgchart") this.treeLayout(false);
                else if (this.profile === "flowchart") this.flowLayout();
                else this.gridLayout();
            });
            this.fit(false);
        }

        childrenMap() {
            const map = new Map();
            this.document.edges.forEach(edge => {
                if (!map.has(edge.fromNodeId)) map.set(edge.fromNodeId, []);
                map.get(edge.fromNodeId).push(edge.toNodeId);
            });
            return map;
        }

        roots() {
            const targets = new Set(this.document.edges.map(edge => key(edge.toNodeId)));
            return this.document.nodes.filter(node => !targets.has(key(node.id)));
        }

        flowLayout() {
            const children = this.childrenMap();
            const depth = new Map();
            const queue = this.roots();
            queue.forEach(node => depth.set(node.id, 0));
            for (let index = 0; index < queue.length; index += 1) {
                const node = queue[index];
                (children.get(node.id) || []).forEach(childId => {
                    if (!depth.has(childId)) {
                        depth.set(childId, (depth.get(node.id) || 0) + 1);
                        const child = this.node(childId);
                        if (child) queue.push(child);
                    }
                });
            }
            this.document.nodes.forEach(node => { if (!depth.has(node.id)) depth.set(node.id, 0); });
            const columns = new Map();
            this.document.nodes.forEach(node => {
                const value = depth.get(node.id) || 0;
                if (!columns.has(value)) columns.set(value, []);
                columns.get(value).push(node);
            });
            [...columns.entries()].forEach(([depthValue, nodes]) => nodes.forEach((node, index) => {
                node.position = { x: 70 + depthValue * 240, y: 100 + index * 142 };
            }));
        }

        radialLayout() {
            const root = this.roots()[0] ?? this.document.nodes[0];
            if (!root) return;
            const children = this.childrenMap();
            const levels = [[root.id]];
            const seen = new Set([root.id]);
            for (let depth = 0; depth < 6; depth += 1) {
                const next = levels.at(-1).flatMap(id => children.get(id) || []).filter(id => !seen.has(id));
                next.forEach(id => seen.add(id));
                if (!next.length) break;
                levels.push(next);
            }
            root.position = { x: 620, y: 330 };
            levels.slice(1).forEach((level, depth) => level.forEach((id, index) => {
                const angle = (Math.PI * 2 * index / level.length) - Math.PI / 2;
                const radius = 180 + depth * 145;
                const node = this.node(id);
                if (node) node.position = { x: 620 + Math.cos(angle) * radius, y: 330 + Math.sin(angle) * radius };
            }));
        }

        treeLayout(horizontal) {
            const root = this.roots()[0] ?? this.document.nodes[0];
            if (!root) return;
            const children = this.childrenMap();
            const coordinates = new Map();
            const seen = new Set();
            let leaf = 0;
            const walk = (id, depth) => {
                if (seen.has(id)) return leaf++;
                seen.add(id);
                const childIds = (children.get(id) || []).filter(child => !seen.has(child));
                const order = childIds.length
                    ? childIds.map(child => walk(child, depth + 1)).reduce((sum, value) => sum + value, 0) / childIds.length
                    : leaf++;
                coordinates.set(id, { depth, order });
                return order;
            };
            walk(root.id, 0);
            this.document.nodes.filter(node => !coordinates.has(node.id)).forEach(node => coordinates.set(node.id, { depth: 0, order: leaf++ }));
            this.document.nodes.forEach(node => {
                const position = coordinates.get(node.id);
                node.position = horizontal
                    ? { x: 90 + position.depth * 255, y: 80 + position.order * 142 }
                    : { x: 90 + position.order * 230, y: 80 + position.depth * 165 };
            });
        }

        gridLayout() {
            this.document.nodes.forEach((node, index) => {
                node.position = { x: 88 + (index % 4) * 260, y: 96 + Math.floor(index / 4) * 170 };
            });
        }

        validate(announce) {
            const errors = [];
            const ids = new Set();
            this.document.nodes.forEach(node => {
                if (!node.id || ids.has(key(node.id))) errors.push("Every node requires a unique id.");
                ids.add(key(node.id));
            });
            this.document.edges.forEach(edge => {
                if (!this.node(edge.fromNodeId) || !this.node(edge.toNodeId)) errors.push("A connection refers to a missing node.");
                if (key(edge.fromNodeId) === key(edge.toNodeId)) errors.push("A node cannot connect to itself.");
            });
            if (this.profile === "flowchart") {
                if (this.document.nodes.filter(node => node.kind === "start").length > 1) errors.push("A flowchart can only have one start node.");
                this.document.nodes.filter(node => node.kind === "end").forEach(node => {
                    if (this.document.edges.some(edge => key(edge.fromNodeId) === key(node.id))) errors.push("End nodes cannot have outgoing connections.");
                });
            }
            if (this.profile === "mindmap" || this.profile === "orgchart") {
                const incoming = new Map();
                this.document.edges.forEach(edge => incoming.set(edge.toNodeId, (incoming.get(edge.toNodeId) || 0) + 1));
                if (this.document.nodes.length && this.document.nodes.filter(node => !incoming.has(node.id)).length !== 1) errors.push("This tree requires one root node.");
                if ([...incoming.values()].some(count => count > 1)) errors.push("A tree node can only have one parent.");
            }
            const valid = errors.length === 0;
            this.root.classList.toggle("has-validation-errors", !valid);
            this.setStatus(valid ? "Diagram validation passed." : errors[0]);
            dispatch(this.root, "zoerce:diagram:validated", { valid, errors, document: this.snapshot() });
            return { valid, errors };
        }

        async save(automatic = false) {
            if (this.saveInFlight) {
                this.saveQueued = true;
                return;
            }
            if (this.autoSaveTimer) {
                globalThis.clearTimeout(this.autoSaveTimer);
                this.autoSaveTimer = null;
            }
            const validation = this.validate(false);
            if (!validation.valid) return;
            const revision = this.revision;
            const detail = { document: this.snapshot(), source: automatic ? "autosave" : "save" };
            if (!dispatch(this.root, "zoerce:diagram:save-request", detail, true)) return;
            this.saveInFlight = true;
            if (!this.configuration.saveUrl) {
                this.completeSave(detail, revision, null, automatic);
                this.saveInFlight = false;
                return;
            }
            try {
                this.root.setAttribute("aria-busy", "true");
                const response = await fetch(this.configuration.saveUrl, { method: "PUT", headers: { "Content-Type": "application/json", Accept: "application/json" }, body: JSON.stringify(this.document), signal: this.abortController.signal });
                if (!response.ok) throw new Error("Save failed.");
                const body = await response.json().catch(() => null);
                this.completeSave(detail, revision, body?.document || body?.nodes ? body.document ?? body : null, automatic);
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.setStatus("Diagram could not be saved.");
                    dispatch(this.root, "zoerce:diagram:error", { error });
                }
            } finally {
                this.saveInFlight = false;
                this.root.removeAttribute("aria-busy");
                if (this.saveQueued || this.revision > revision) {
                    this.saveQueued = false;
                    this.scheduleAutoSave(80);
                }
            }
        }

        completeSave(detail, revision, document, automatic) {
            if (this.revision !== revision) return;
            if (document) this.document = normalizeDocument(document);
            this.dirty = false;
            if (this.dirtyBadge) this.dirtyBadge.hidden = true;
            this.setStatus(automatic ? "Changes saved." : "Diagram saved.");
            dispatch(this.root, "zoerce:diagram:saved", { ...detail, document: this.snapshot() });
            this.renderScene();
            this.renderNavigator();
        }

        async load() {
            if (!this.configuration.sourceUrl) return;
            try {
                const response = await fetch(this.configuration.sourceUrl, { headers: { Accept: "application/json" }, signal: this.abortController.signal });
                if (!response.ok) throw new Error("Load failed.");
                const body = await response.json();
                this.replace(body.document ?? body, "Diagram loaded.");
            } catch (error) {
                if (error.name !== "AbortError") this.setStatus("Using the server-rendered diagram.");
            }
        }

        replace(document, message) {
            this.document = normalizeDocument(document);
            this.selection.clear();
            this.selectedEdge = null;
            this.history = [];
            this.future = [];
            this.dirty = false;
            this.revision = 0;
            this.inspectorEditTarget = null;
            this.inspectorEditCaptured = false;
            if (this.dirtyBadge) this.dirtyBadge.hidden = true;
            this.render();
            this.setStatus(message);
            dispatch(this.root, "zoerce:diagram:replaced", { document: this.snapshot() });
        }

        connectRealtime() {
            if (!this.configuration.realtimeHubUrl || !this.configuration.realtimeEventName) return;
            if (!window.ZoorceSignalR?.subscribe) {
                document.addEventListener("zoerce:signalr-ready", () => this.connectRealtime(), { once: true, signal: this.abortController.signal });
                return;
            }
            this.unsubscribeRealtime = window.ZoorceSignalR.subscribe({
                hubUrl: this.configuration.realtimeHubUrl,
                eventName: this.configuration.realtimeEventName
            }, update => this.replace(update?.document ?? update, "Diagram updated in real time."));
        }

        dispose() {
            if (this.autoSaveTimer) globalThis.clearTimeout(this.autoSaveTimer);
            if (this.focusMode) {
                this.root.classList.remove("is-focus-mode");
                if (this.focusPlaceholder?.parentNode) this.focusPlaceholder.parentNode.replaceChild(this.root, this.focusPlaceholder);
                this.focusPlaceholder = null;
                document.documentElement.classList.remove("z-diagram-focus-active");
                document.body.classList.remove("z-diagram-focus-active");
            }
            this.abortController.abort();
            this.resizeObserver?.disconnect();
            this.unsubscribeRealtime?.();
        }
    }

    const mount = scope => {
        const roots = scope?.matches?.("[data-z-diagram]") ? [scope] : [...(scope?.querySelectorAll?.("[data-z-diagram]") ?? document.querySelectorAll("[data-z-diagram]"))];
        roots.forEach(root => {
            if (instances.has(root)) return;
            try {
                const configuration = JSON.parse(root.querySelector("[data-z-diagram-config]")?.textContent ?? "{}");
                instances.set(root, new DiagramController(root, configuration));
            } catch {
                root.querySelector("[data-z-diagram-status]")?.replaceChildren("Diagram configuration could not be loaded.");
            }
        });
    };
    const dispose = root => {
        instances.get(root)?.dispose();
        instances.delete(root);
    };
    window.ZoorceDiagrams = Object.freeze({ mount, dispose });
    mount(document);
})();
