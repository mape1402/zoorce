(() => {
    "use strict";

    const namespace = "http://www.w3.org/2000/svg";
    const canvasWidth = 1400;
    const canvasHeight = 780;
    const nodeWidth = 176;
    const nodeHeight = 78;
    const instances = new WeakMap();
    const profiles = { Flowchart: "flowchart", MindMap: "mindmap", OrgChart: "orgchart", Architecture: "architecture" };
    const modes = { Editor: "editor", Viewer: "viewer", ReadOnly: "readonly" };
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
    const normalizeNode = (node, index) => ({
        id: text(node?.id) || "node-" + (index + 1),
        kind: key(node?.kind) || "process",
        title: text(node?.title) || "Untitled node",
        description: text(node?.description) || null,
        tone: enumValue(tones, node?.tone, "Neutral"),
        collapsed: Boolean(node?.collapsed),
        action: normalizeAction(node?.action),
        data: node?.data ?? null,
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
        kind: text(edge?.kind) || null
    });
    const normalizeDocument = document => {
        const nodes = Array.isArray(document?.nodes) ? document.nodes.map(normalizeNode) : [];
        const ids = new Set();
        const validNodes = nodes.filter(node => {
            const unique = !ids.has(key(node.id));
            ids.add(key(node.id));
            return unique;
        });
        const edges = (Array.isArray(document?.edges) ? document.edges.map(normalizeEdge) : [])
            .filter(edge => edge.fromNodeId && edge.toNodeId && edge.fromNodeId !== edge.toNodeId && validNodes.some(node => key(node.id) === key(edge.fromNodeId)) && validNodes.some(node => key(node.id) === key(edge.toNodeId)));
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
                gridSize: 24, snapToGrid: true, minimumZoom: .4, maximumZoom: 2.4, initialZoom: 1,
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
            this.status = root.querySelector("[data-z-diagram-status]");
            this.zoomOutput = root.querySelector("[data-z-diagram-zoom]");
            this.dirtyBadge = root.querySelector("[data-z-diagram-dirty]");
            this.announcer = root.querySelector("[data-z-diagram-announcer]");
            this.abortController = new AbortController();
            this.selection = new Set();
            this.selectedEdge = null;
            this.connectingFrom = null;
            this.history = [];
            this.future = [];
            this.dirty = false;
            this.view = { x: 0, y: 0, zoom: clamp(number(this.configuration.initialZoom, 1), number(this.configuration.minimumZoom, .4), number(this.configuration.maximumZoom, 2.4)) };
            this.drag = null;
            this.pan = null;
            this.pendingPointerAction = null;
            this.lastNodeTap = null;
            this.ignoreNativeDoubleClickUntil = 0;
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
            this.canvas?.addEventListener("pointerdown", event => this.onPointerDown(event), { signal });
            this.canvas?.addEventListener("pointermove", event => this.onPointerMove(event), { signal });
            this.canvas?.addEventListener("pointerup", event => this.onPointerUp(event), { signal });
            this.canvas?.addEventListener("pointercancel", event => this.onPointerUp(event), { signal });
            this.canvas?.addEventListener("wheel", event => this.onWheel(event), { signal, passive: false });
            this.canvas?.addEventListener("keydown", event => this.onKeyDown(event), { signal });
            this.root.addEventListener("zoerce:diagram:replace", event => this.replace(event.detail?.document ?? event.detail, "Diagram updated from the host."), { signal });
            this.root.addEventListener("zoerce:diagram:focus", event => this.focus(event.detail?.nodeId), { signal });
            this.root.addEventListener("zoerce:diagram:validate", () => this.validate(true), { signal });
            this.root.addEventListener("zoerce:diagram:save", () => this.save(), { signal });
        }

        onClick(event) {
            const command = event.target.closest("[data-z-diagram-command]")?.dataset.zDiagramCommand;
            if (command) {
                this.command(command);
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
                this.handlePort(port);
                return;
            }
            const node = event.target.closest("[data-z-diagram-node]");
            if (node) {
                const nodeId = node.dataset.zDiagramNodeId;
                const actionHandledOnPointerUp = this.pendingPointerAction === nodeId;
                this.pendingPointerAction = null;
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
            const nodeElement = event.target.closest("[data-z-diagram-node]");
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
            if (event.target === this.canvas || event.target === this.world || event.target === this.edgesLayer || event.button === 1) {
                this.pan = { x: event.clientX, y: event.clientY, viewX: this.view.x, viewY: this.view.y };
                this.canvas.setPointerCapture?.(event.pointerId);
                this.canvas.classList.add("is-panning");
                event.preventDefault();
            }
        }

        onPointerMove(event) {
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
                this.renderTransform();
            }
        }

        onPointerUp() {
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
            if (event.key === "Delete" || event.key === "Backspace") {
                if (this.editable && !event.target.matches("input, textarea, select")) {
                    event.preventDefault();
                    this.deleteSelection();
                }
                return;
            }
            if ((event.ctrlKey || event.metaKey) && key(event.key) === "z") {
                event.preventDefault();
                event.shiftKey ? this.redo() : this.undo();
                return;
            }
            if ((event.ctrlKey || event.metaKey) && key(event.key) === "d" && this.editable) {
                event.preventDefault();
                this.duplicateSelection();
                return;
            }
            if (event.key === "Escape") {
                this.connectingFrom = null;
                this.setStatus("Connection cancelled.");
                this.renderScene();
            }
        }

        command(command) {
            if (command === "zoom-in") this.setZoom(this.view.zoom + .15);
            if (command === "zoom-out") this.setZoom(this.view.zoom - .15);
            if (command === "fit") this.fit(true);
            if (command === "undo") this.undo();
            if (command === "redo") this.redo();
            if (command === "layout" && this.editable) this.applyLayout();
            if (command === "validate") this.validate(true);
            if (command === "save") this.save();
        }

        node(id) { return this.document.nodes.find(node => key(node.id) === key(id)); }
        edge(id) { return this.document.edges.find(edge => key(edge.id) === key(id)); }
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
            this.dirty = true;
            if (this.dirtyBadge) this.dirtyBadge.hidden = false;
            this.setStatus(message);
        }

        setStatus(message) {
            if (this.status) this.status.textContent = message;
            if (this.announcer) this.announcer.textContent = message;
        }

        emitChange(reason) {
            dispatch(this.root, "zoerce:diagram:changed", { document: this.snapshot(), reason });
        }

        selectNode(nodeId, append = false, invokeAction = true) {
            const node = this.node(nodeId);
            if (!node) return;
            if (!append) this.selection.clear();
            if (append && this.selection.has(node.id)) this.selection.delete(node.id);
            else this.selection.add(node.id);
            this.selectedEdge = null;
            this.render();
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

        handlePort(port) {
            const nodeId = port.closest("[data-z-diagram-node]")?.dataset.zDiagramNodeId;
            if (!nodeId) return;
            if (port.dataset.zDiagramPort === "out") {
                this.connectingFrom = nodeId;
                this.setStatus("Choose an input port to create the connection.");
                this.renderScene();
                return;
            }
            if (!this.connectingFrom) {
                this.setStatus("Choose an output port first.");
                return;
            }
            if (key(this.connectingFrom) === key(nodeId)) {
                this.setStatus("A node cannot connect to itself.");
                return;
            }
            if (this.document.edges.some(edge => key(edge.fromNodeId) === key(this.connectingFrom) && key(edge.toNodeId) === key(nodeId))) {
                this.connectingFrom = null;
                this.setStatus("That connection already exists.");
                this.renderScene();
                return;
            }
            const source = this.connectingFrom;
            this.connectingFrom = null;
            this.change("Connection added.", () => this.document.edges.push({
                id: idFor("edge", this.document.edges),
                fromNodeId: source,
                toNodeId: nodeId,
                label: null,
                tone: "Neutral",
                kind: null
            }));
        }

        addNode(kind) {
            const index = this.document.nodes.length;
            const offset = index % 5;
            this.change("New " + titleForKind(kind).toLowerCase() + " added.", () => {
                this.document.nodes.push({
                    id: idFor(key(kind) || "node", this.document.nodes),
                    kind: key(kind) || "process",
                    title: titleForKind(key(kind)),
                    description: null,
                    tone: "Neutral",
                    collapsed: false,
                    action: null,
                    data: null,
                    position: { x: this.snap(120 + offset * 190), y: this.snap(130 + Math.floor(index / 5) * 142) }
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
            if (!this.selection.size) return;
            const selected = this.document.nodes.filter(node => this.selection.has(node.id));
            this.change("Selected nodes duplicated.", () => {
                const copies = selected.map(node => ({ ...clone(node), id: idFor(node.kind || "node", this.document.nodes), title: node.title + " copy", position: { x: node.position.x + 32, y: node.position.y + 32 } }));
                this.document.nodes.push(...copies);
                this.selection = new Set(copies.map(node => node.id));
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
            this.renderTransform();
            this.renderScene();
            this.renderNavigator();
            this.renderInspector();
            this.renderMinimap();
        }

        renderTransform() {
            if (!this.world) return;
            this.world.style.setProperty("--z-diagram-x", this.view.x + "px");
            this.world.style.setProperty("--z-diagram-y", this.view.y + "px");
            this.world.style.setProperty("--z-diagram-zoom", String(this.view.zoom));
            if (this.zoomOutput) this.zoomOutput.textContent = Math.round(this.view.zoom * 100) + "%";
        }

        renderScene() {
            if (!this.world || !this.edgesLayer) return;
            const visible = this.visibleNodeIds();
            const nodes = this.document.nodes.filter(node => visible.has(node.id));
            this.edgesLayer.replaceChildren(this.marker());
            this.edgesLayer.setAttribute("viewBox", "0 0 " + canvasWidth + " " + canvasHeight);
            this.edgesLayer.setAttribute("width", String(canvasWidth));
            this.edgesLayer.setAttribute("height", String(canvasHeight));
            this.document.edges.filter(edge => visible.has(edge.fromNodeId) && visible.has(edge.toNodeId)).forEach(edge => this.renderEdge(edge));
            [...this.world.querySelectorAll("[data-z-diagram-node]")].forEach(element => element.remove());
            nodes.forEach(node => this.world.append(this.renderNode(node)));
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
            marker.append(path);
            defs.append(marker);
            return defs;
        }

        renderEdge(edge) {
            const from = this.node(edge.fromNodeId);
            const to = this.node(edge.toNodeId);
            if (!from || !to) return;
            const start = { x: from.position.x + nodeWidth, y: from.position.y + nodeHeight / 2 };
            const end = { x: to.position.x, y: to.position.y + nodeHeight / 2 };
            const line = svg("path", "z-diagram-edge z-diagram-edge--" + key(edge.tone));
            line.dataset.zDiagramEdge = "";
            line.dataset.zDiagramEdgeId = edge.id;
            const curve = this.profile === "mindmap" || this.profile === "orgchart"
                ? "M " + start.x + " " + start.y + " C " + (start.x + 70) + " " + start.y + ", " + (end.x - 70) + " " + end.y + ", " + end.x + " " + end.y
                : "M " + start.x + " " + start.y + " H " + ((start.x + end.x) / 2) + " V " + end.y + " H " + end.x;
            line.setAttribute("d", curve);
            line.setAttribute("marker-end", "url(#" + this.root.id + "-arrow)");
            if (edge.id === this.selectedEdge) line.classList.add("is-selected");
            this.edgesLayer.append(line);
            if (edge.label) {
                const label = svg("text", "z-diagram-edge-label");
                label.dataset.zDiagramEdge = "";
                label.dataset.zDiagramEdgeId = edge.id;
                label.setAttribute("x", String((start.x + end.x) / 2));
                label.setAttribute("y", String((start.y + end.y) / 2 - 8));
                label.setAttribute("text-anchor", "middle");
                label.textContent = edge.label;
                this.edgesLayer.append(label);
            }
        }

        renderNode(node) {
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
            if (this.connectingFrom === node.id) element.classList.add("is-connecting");
            const incoming = create("button", "z-diagram-port z-diagram-port--in");
            incoming.type = "button";
            incoming.dataset.zDiagramPort = "in";
            incoming.setAttribute("aria-label", "Connect into " + node.title);
            const icon = create("div", "z-diagram-node-icon");
            icon.textContent = iconForKind(node.kind);
            const copy = create("div", "z-diagram-node-copy");
            const title = create("strong");
            title.textContent = node.title;
            copy.append(title);
            if (node.description) {
                const description = create("small");
                description.textContent = node.description;
                copy.append(description);
            }
            const outgoing = create("button", "z-diagram-port z-diagram-port--out");
            outgoing.type = "button";
            outgoing.dataset.zDiagramPort = "out";
            outgoing.setAttribute("aria-label", "Connect from " + node.title);
            if (this.editable) element.append(incoming, icon, copy, outgoing);
            else element.append(icon, copy);
            return element;
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
            const apply = create("button", "z-button z-button-primary z-button-sm"); apply.type = "button"; apply.textContent = "Apply"; apply.dataset.zDiagramInspector = "apply-node";
            const duplicate = create("button", "z-button z-button-ghost z-button-sm"); duplicate.type = "button"; duplicate.textContent = "Duplicate"; duplicate.dataset.zDiagramInspector = "duplicate-node";
            const remove = create("button", "z-button z-button-danger z-button-sm"); remove.type = "button"; remove.textContent = "Delete"; remove.dataset.zDiagramInspector = "delete-node";
            actions.append(apply, duplicate, remove);
            if (this.profile === "mindmap" || this.profile === "orgchart") {
                const collapse = create("button", "z-button z-button-ghost z-button-sm"); collapse.type = "button"; collapse.textContent = node.collapsed ? "Expand branch" : "Collapse branch"; collapse.dataset.zDiagramInspector = "toggle-collapse"; actions.append(collapse);
            }
            form.append(titleLabel, descriptionLabel, toneLabel, actionLabel, targetLabel, actions);
            fragment.append(eyebrow, title, form);
            return fragment;
        }

        edgeInspector(edge) {
            const fragment = document.createDocumentFragment();
            const eyebrow = create("span", "z-section-kicker"); eyebrow.textContent = "Connection";
            const title = create("strong"); title.textContent = "Edge inspector";
            const form = create("div", "z-diagram-form");
            const label = create("label"); label.textContent = "Label";
            const input = create("input", "z-input"); input.value = edge.label ?? ""; input.dataset.zDiagramField = "edge-label"; label.append(input);
            const actions = create("div", "z-diagram-inspector-actions");
            const apply = create("button", "z-button z-button-primary z-button-sm"); apply.type = "button"; apply.textContent = "Apply"; apply.dataset.zDiagramInspector = "apply-edge";
            const remove = create("button", "z-button z-button-danger z-button-sm"); remove.type = "button"; remove.textContent = "Delete"; remove.dataset.zDiagramInspector = "delete-edge";
            actions.append(apply, remove);
            form.append(label, actions);
            fragment.append(eyebrow, title, form);
            return fragment;
        }

        handleInspector(action) {
            const selected = this.document.nodes.find(node => this.selection.has(node.id));
            const edge = this.selectedEdge ? this.edge(this.selectedEdge) : null;
            if (action === "apply-node" && selected) {
                const title = this.inspector.querySelector("[data-z-diagram-field='title']")?.value;
                const description = this.inspector.querySelector("[data-z-diagram-field='description']")?.value;
                const tone = this.inspector.querySelector("[data-z-diagram-field='tone']")?.value;
                const actionKind = this.inspector.querySelector("[data-z-diagram-field='action-kind']")?.value;
                const target = this.inspector.querySelector("[data-z-diagram-field='action-target']")?.value;
                this.change("Node details updated.", () => {
                    selected.title = text(title) || "Untitled node";
                    selected.description = text(description) || null;
                    selected.tone = tones.has(tone) ? tone : "Neutral";
                    selected.action = actionKinds.has(actionKind) && actionKind !== "None" ? { kind: actionKind, target: text(target) || null, payload: null } : null;
                });
            }
            if (action === "apply-edge" && edge) {
                const label = this.inspector.querySelector("[data-z-diagram-field='edge-label']")?.value;
                this.change("Connection label updated.", () => { edge.label = text(label) || null; });
            }
            if (action === "delete-node" && selected) this.deleteSelection();
            if (action === "delete-edge" && edge) this.deleteSelection();
            if (action === "duplicate-node" && selected) this.duplicateSelection();
            if (action === "toggle-collapse" && selected) this.toggleCollapsed(selected.id);
        }

        toggleCollapsed(nodeId) {
            this.change("Branch visibility changed.", () => {
                const target = this.node(nodeId);
                if (target) target.collapsed = !target.collapsed;
            });
        }

        renderMinimap() {
            if (!this.minimap) return;
            this.minimap.replaceChildren(...this.document.nodes.map(node => {
                const mark = create("i");
                mark.style.left = clamp(node.position.x / canvasWidth * 100, 0, 100) + "%";
                mark.style.top = clamp(node.position.y / canvasHeight * 100, 0, 100) + "%";
                if (this.selection.has(node.id)) mark.classList.add("is-selected");
                return mark;
            }));
        }

        focus(nodeId) {
            const node = this.node(nodeId);
            if (!node) return;
            this.selectNode(node.id, false, false);
            const width = this.canvas?.clientWidth ?? 700;
            const height = this.canvas?.clientHeight ?? 500;
            this.view.x = width / 2 - (node.position.x + nodeWidth / 2) * this.view.zoom;
            this.view.y = height / 2 - (node.position.y + nodeHeight / 2) * this.view.zoom;
            this.renderTransform();
            this.world?.querySelector("[data-z-diagram-node-id='" + CSS.escape(node.id) + "']")?.focus();
        }

        setZoom(value) {
            this.view.zoom = clamp(value, number(this.configuration.minimumZoom, .4), number(this.configuration.maximumZoom, 2.4));
            this.renderTransform();
        }

        fit(announce) {
            if (!this.document.nodes.length || !this.canvas) return;
            const xs = this.document.nodes.map(node => node.position.x);
            const ys = this.document.nodes.map(node => node.position.y);
            const minX = Math.min(...xs);
            const maxX = Math.max(...xs) + nodeWidth;
            const minY = Math.min(...ys);
            const maxY = Math.max(...ys) + nodeHeight;
            const zoom = clamp(Math.min((this.canvas.clientWidth - 80) / Math.max(maxX - minX, 160), (this.canvas.clientHeight - 80) / Math.max(maxY - minY, 120)), number(this.configuration.minimumZoom, .4), number(this.configuration.maximumZoom, 2.4));
            this.view.zoom = zoom;
            this.view.x = (this.canvas.clientWidth - (maxX + minX) * zoom) / 2;
            this.view.y = (this.canvas.clientHeight - (maxY + minY) * zoom) / 2;
            this.renderTransform();
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
            const levels = [[root.id]];
            const seen = new Set([root.id]);
            for (let depth = 0; depth < 10; depth += 1) {
                const next = levels.at(-1).flatMap(id => children.get(id) || []).filter(id => !seen.has(id));
                next.forEach(id => seen.add(id));
                if (!next.length) break;
                levels.push(next);
            }
            levels.forEach((level, depth) => level.forEach((id, index) => {
                const node = this.node(id);
                if (!node) return;
                node.position = horizontal
                    ? { x: 90 + depth * 245, y: 80 + index * 125 }
                    : { x: 90 + index * 225, y: 80 + depth * 140 };
            }));
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

        async save() {
            const validation = this.validate(false);
            if (!validation.valid) return;
            const detail = { document: this.snapshot(), source: "save" };
            if (!dispatch(this.root, "zoerce:diagram:save-request", detail, true)) return;
            if (!this.configuration.saveUrl) {
                this.dirty = false;
                if (this.dirtyBadge) this.dirtyBadge.hidden = true;
                this.setStatus("Saved locally for this diagram demo.");
                dispatch(this.root, "zoerce:diagram:saved", detail);
                return;
            }
            try {
                this.root.setAttribute("aria-busy", "true");
                const response = await fetch(this.configuration.saveUrl, { method: "PUT", headers: { "Content-Type": "application/json", Accept: "application/json" }, body: JSON.stringify(this.document), signal: this.abortController.signal });
                if (!response.ok) throw new Error("Save failed.");
                const body = await response.json().catch(() => null);
                if (body?.document || body?.nodes) this.document = normalizeDocument(body.document ?? body);
                this.dirty = false;
                if (this.dirtyBadge) this.dirtyBadge.hidden = true;
                this.setStatus("Diagram saved.");
                dispatch(this.root, "zoerce:diagram:saved", { document: this.snapshot() });
                this.render();
            } catch (error) {
                if (error.name !== "AbortError") {
                    this.setStatus("Diagram could not be saved.");
                    dispatch(this.root, "zoerce:diagram:error", { error });
                }
            } finally {
                this.root.removeAttribute("aria-busy");
            }
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
            this.abortController.abort();
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
