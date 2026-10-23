using Zoorce.Web.UI.Navigation.Stepper;

namespace Zoorce.Sample.Infrastructure;

/// <summary>Small transport-neutral snapshots used by the isolated stepper sample.</summary>
public static class SampleStepperData
{
    public static object Create(int revision = 0)
    {
        var phase = Math.Clamp(revision, 0, 2);
        var current = phase switch
        {
            0 => "release-validation",
            1 => "release-handoff",
            _ => "release-production"
        };

        return new
        {
            currentStepId = current,
            steps = new[]
            {
                new { id = "release-brief", state = "Completed", statusText = "Approved" },
                new { id = "release-validation", state = phase == 0 ? "Active" : "Completed", statusText = phase == 0 ? "In review" : "Validated" },
                new { id = "release-handoff", state = phase == 0 ? "Pending" : phase == 1 ? "Active" : "Completed", statusText = phase == 1 ? "Preparing" : phase == 2 ? "Handed off" : "Queued" },
                new { id = "release-production", state = phase == 2 ? "Active" : "Pending", statusText = phase == 2 ? "Monitoring" : "Waiting" }
            }
        };
    }

    public static ZoorceStepperTransitionDecision Authorize(ZoorceStepperTransitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var current = request.StepId?.Trim().ToLowerInvariant();
        var previous = request.PreviousStepId?.Trim().ToLowerInvariant();
        if (current is not ("scope" or "risk" or "approval" or "release"))
        {
            return new ZoorceStepperTransitionDecision(false, "The requested release-gate step does not exist.");
        }

        if (current == "approval" && previous != "risk")
        {
            return new ZoorceStepperTransitionDecision(false, "Approval can only follow the risk review.");
        }

        if (current == "release" && previous != "approval")
        {
            return new ZoorceStepperTransitionDecision(false, "Release can only follow an approved gate.");
        }

        var currentIndex = PolicyIndex(current);
        var previousIndex = PolicyIndex(previous);
        if (previousIndex >= 0 && request.IsBackward != (currentIndex < previousIndex))
        {
            return new ZoorceStepperTransitionDecision(false, "The transition direction does not match the requested release-gate step.");
        }
        var steps = new[]
        {
            PolicyStep("scope", "Scope", "Acceptance criteria agreed", "✓", currentIndex, 0, ZoorceStepperTone.Success),
            PolicyStep("risk", "Risk review", "One decision needs approval", "!", currentIndex, 1, ZoorceStepperTone.Warning),
            PolicyStep("approval", "Approval", "Release owner confirms", "⌁", currentIndex, 2, ZoorceStepperTone.Info),
            PolicyStep("release", "Release", "Production handoff", "↗", currentIndex, 3, ZoorceStepperTone.Auto)
        };

        return new ZoorceStepperTransitionDecision(true, "Transition authorized.", new ZoorceStepperSnapshot(current, steps));
    }

    private static ZoorceStepperStepSnapshot PolicyStep(string id, string title, string supportingText, string icon, int currentIndex, int index, ZoorceStepperTone tone)
    {
        var state = index < currentIndex
            ? ZoorceStepperState.Completed
            : index == currentIndex
                ? ZoorceStepperState.Active
                : ZoorceStepperState.Pending;
        var status = state switch
        {
            ZoorceStepperState.Completed => "Completed",
            ZoorceStepperState.Active => "In progress",
            _ => "Waiting"
        };

        return new ZoorceStepperStepSnapshot(id, state, tone, title, supportingText, status, icon, true);
    }

    private static int PolicyIndex(string? id) => id switch
    {
        "scope" => 0,
        "risk" => 1,
        "approval" => 2,
        "release" => 3,
        _ => -1
    };
}
