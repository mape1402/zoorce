using System.Reflection;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Zoorce.Web.UI.Forms;

namespace Zoorce.Web.UI.Tests;

/// <summary>Verifies that every public form field owns a named, independently invokable component boundary.</summary>
public sealed class FormComponentSurfaceTests
{
    [Fact]
    public void Every_public_field_option_has_a_matching_view_component_and_normalizes_independently()
    {
        var assembly = typeof(ZoorceTextInputOptions).Assembly;
        var optionsTypes = assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(ZoorceFieldOptions).IsAssignableFrom(type))
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(optionsTypes);

        foreach (var optionsType in optionsTypes)
        {
            var options = Assert.IsAssignableFrom<ZoorceFieldOptions>(Activator.CreateInstance(optionsType));
            optionsType.GetProperty(nameof(ZoorceFieldOptions.Id))!.SetValue(options, ToId(optionsType.Name));

            var model = options.ToRenderOptions();
            Assert.Equal(ToId(optionsType.Name), model.Id);

            var componentName = optionsType.Name.Replace("Options", "ViewComponent", StringComparison.Ordinal);
            var componentType = assembly.GetType($"Zoorce.Web.UI.Forms.{componentName}");
            Assert.NotNull(componentType);

            var component = Activator.CreateInstance(componentType!);
            var invoke = componentType!.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(invoke);

            var result = invoke!.Invoke(component, [options]);
            Assert.IsType<ViewViewComponentResult>(result);
        }
    }

    private static string ToId(string typeName)
        => string.Concat(typeName
            .Replace("Zoorce", string.Empty, StringComparison.Ordinal)
            .Replace("Options", string.Empty, StringComparison.Ordinal)
            .Select((character, index) => index > 0 && char.IsUpper(character) ? $"-{char.ToLowerInvariant(character)}" : char.ToLowerInvariant(character).ToString()));
}
