using Zoorce.Web.UI.Abstractions.Navigation;

namespace Zoorce.Web.UI.Tests;

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

    [Fact]
    public void Children_can_describe_a_navigation_tree_with_multiple_levels()
    {
        var leaf = new NavigationItem("Tokens", "/library/tokens");
        var section = new NavigationItem("Foundations", Icon: "F", Children: [leaf]);
        var root = new NavigationItem("Library", Icon: "L", Children: [section]);

        Assert.True(root.HasChildren);
        Assert.True(root.Children!.Single().HasChildren);
        Assert.Same(leaf, root.Children!.Single().Children!.Single());
    }

    [Fact]
    public void Groups_can_supply_a_stable_identity_and_initial_expansion_override()
    {
        var item = new NavigationItem(
            "Workspace",
            Icon: "02",
            Id: "workspace",
            GroupExpansion: NavigationGroupExpansion.Expanded,
            Children: [new NavigationItem("Dashboard", "/dashboard")]);

        Assert.Equal("workspace", item.Id);
        Assert.Equal(NavigationGroupExpansion.Expanded, item.GroupExpansion);
    }
}
