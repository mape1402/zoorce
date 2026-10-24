(() => {
    "use strict";

    const states = ["Pending", "Active", "Completed", "Attention", "Error", "Disabled"];
    const tones = ["Auto", "Neutral", "Primary", "Success", "Warning", "Danger", "Info"];
    const stateText = {
        Pending: "Pending",
        Active: "In progress",
        Completed: "Completed",
        Attention: "Needs review",
        Error: "Action required",
        Disabled: "Unavailable"
    };
    const normalizeEnum = (value, values, fallback) => Number.isInteger(value) && value >= 0 && value < values.length
        ? values[value]
        : values.find(candidate => candidate.toLowerCase() === String(value ?? "").toLowerCase()) ?? fallback;
    const normalizeState = value => normalizeEnum(value, states, "Pending");
    const normalizeTone = value => normalizeEnum(value, tones, "Auto");

    const evaluateTransition = ({ items, currentIndex, targetIndex, reason, interaction = "Wizard", allowCompletedNavigation = true, allowFutureNavigation = false, transitionRules = {} }) => {
        const target = items[targetIndex];
        const current = items[currentIndex];
        if (!target || !current || targetIndex === currentIndex) return { allowed: false, message: "This step is already active." };
        if (target.navigable === false || normalizeState(target.state) === "Disabled") return { allowed: false, message: "This step is unavailable." };

        if (targetIndex < currentIndex) {
            const backward = transitionRules.backward ?? "Auto";
            if (backward === "Block") return { allowed: false, message: "Going back is not available in this workflow." };
            if (backward === "CompletedOnly") return normalizeState(target.state) === "Completed"
                ? { allowed: true, message: "" }
                : { allowed: false, message: "Only completed steps can be revisited." };
            if (backward === "AnyEligible" || reason === "previous") return { allowed: true, message: "" };
            return allowCompletedNavigation || normalizeState(target.state) === "Completed"
                ? { allowed: true, message: "" }
                : { allowed: false, message: "Completed steps cannot be revisited." };
        }

        const currentState = normalizeState(current.state);
        const errorRule = transitionRules.error ?? "Block";
        const warningRule = transitionRules.warning ?? "Allow";
        if (currentState === "Error" && errorRule === "Block") return { allowed: false, message: "Resolve the current step before continuing." };
        if (currentState === "Attention" && warningRule === "Block") return { allowed: false, message: "Resolve the warning before continuing." };
        const requiresAuthorization = (currentState === "Error" && errorRule === "RequireAuthorization") || (currentState === "Attention" && warningRule === "RequireAuthorization");
        const forward = transitionRules.forward ?? "Auto";
        if (forward === "Block") return { allowed: false, message: "Forward progress is controlled by this workflow." };
        if (forward === "AdjacentOnly") return targetIndex === currentIndex + 1
            ? { allowed: true, message: "", requiresAuthorization }
            : { allowed: false, message: "Complete the current step before skipping ahead." };
        if (forward === "AnyEligible") return { allowed: true, message: "", requiresAuthorization };
        if (interaction === "Wizard") return targetIndex === currentIndex + 1
            ? { allowed: true, message: "", requiresAuthorization }
            : { allowed: false, message: "Complete the current step before skipping ahead." };
        return allowFutureNavigation
            ? { allowed: true, message: "", requiresAuthorization }
            : { allowed: false, message: "Future steps are not available yet." };
    };

    const getLocalTransitionPlan = ({ currentIndex, targetIndex, reason, transitionRules = {}, interaction = "Wizard" }) => ({
        resetFollowing: targetIndex < currentIndex && (transitionRules.onBackward === "ResetFollowing" || (transitionRules.onBackward ?? "Auto") === "Auto" && interaction === "Wizard"),
        completePrevious: reason === "next"
    });

    window.ZoorceWorkflow = Object.freeze({
        states,
        tones,
        stateText,
        normalizeState,
        normalizeTone,
        evaluateTransition,
        getLocalTransitionPlan
    });
})();
