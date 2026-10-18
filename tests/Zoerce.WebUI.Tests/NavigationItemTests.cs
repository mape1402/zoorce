using Zoerce.WebUI.Abstractions.Navigation;

namespace Zoerce.WebUI.Tests;

public sealed class NavigationItemTests
{
    [Fact]
    public void HasChildren_returns_true_when_children_are_supplied()
    {
        var item = new NavigationItem(
            "Catalog",
            Icon: "collection",
            Children: [new NavigationItem("Packages", "/packages")]);

        Assert.True(item.HasChildren);
    }

    [Fact]
    public void HasChildren_returns_false_for_a_link()
    {
        var item = new NavigationItem("Home", "/");

        Assert.False(item.HasChildren);
    }
}
