# Migration guide

## Naming

The public family is `Zoorce.Web.UI.*`. Earlier package and namespace paths are corrected. The sample is `Zoorce.Sample` and is not a distributable library.

## Data and realtime

New code uses `Binding` rather than assigning a component to a specific browser transport. Configure a source and optional scope:

```csharp
Binding = new ZoorceDataBindingOptions
{
    Source = "/api/releases/current",
    InvalidationScope = "release-current"
}
```

Install `Zoorce.Web.UI.Realtime.SignalR` only when the host uses SignalR, then translate its events to `ZoorceDataInvalidation`. A component reacts to the invalidation scope, not to SignalR itself.

## Forms

Prefer named controls and options—for example `ZoorceTextInput` with `ZoorceTextInputOptions`, `ZoorceQuantityStepper` with `ZoorceQuantityStepperOptions`, and `ZoorceAvatarPicker` with `ZoorceAvatarPickerOptions`. This keeps options discoverable, lets components evolve independently, and avoids a form-level discriminator in host pages.
