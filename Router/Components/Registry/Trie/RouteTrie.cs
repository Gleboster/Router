using Router.Models.Registrations;
using Router.Models.Routes;
using Router.Models.Segments;

namespace Router.Components.Registry.Trie;

public sealed class RouteTrie
{
    private readonly SegmentNode rootNode;

    public RouteTrie()
    {
        rootNode = new SegmentNode();
    }

    public void Add(RouteRegistration routeRegistration)
    {
        var currentNode = rootNode;
        var segments = routeRegistration.Template.Segments;

        foreach (var segment in segments)
        {
            var templateSegment = (TemplateSegment)segment;
            var childNode = FindOrCreateChild(currentNode, templateSegment);
            currentNode = childNode;
        }

        currentNode.SetRegistration(routeRegistration);
    }

    public RouteRegistration ResolveRoute(Route route)
    {
        return ResolveNode(rootNode, route.Segments, 0);
    }

    private RouteRegistration ResolveNode(
        SegmentNode node,
        IReadOnlyList<RouteSegment> routeSegments,
        int segmentIndex)
    {
        if (segmentIndex == routeSegments.Count)
        {
            return node.IsEndpoint ? node.Registration : null;
        }

        var routeSegment = routeSegments[segmentIndex];

        foreach (var child in node.Children)
        {
            if (IsMatch(child.Segment, routeSegment))
            {
                var result = ResolveNode(child, routeSegments, segmentIndex + 1);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }

    private bool IsMatch(TemplateSegment templateSegment, RouteSegment routeSegment)
    {
        return templateSegment switch
        {
            StaticTemplateSegment staticSegment =>
                string.Equals(staticSegment.Value, routeSegment.Value, StringComparison.OrdinalIgnoreCase),
            DynamicTemplateSegment => true,
            _ => false
        };
    }

    private SegmentNode FindOrCreateChild(SegmentNode parent, TemplateSegment segment)
    {
        foreach (var child in parent.Children)
        {
            if (AreSegmentsEqual(child.Segment, segment))
            {
                return child;
            }
        }

        var newNode = new SegmentNode(segment);
        parent.AddChild(newNode);
        return newNode;
    }

    private bool AreSegmentsEqual(TemplateSegment first, TemplateSegment second)
    {
        if (first == null || second == null)
            return first == second;

        return (first, second) switch
        {
            (StaticTemplateSegment s1, StaticTemplateSegment s2) =>
                string.Equals(s1.Value, s2.Value, StringComparison.OrdinalIgnoreCase),
            (DynamicTemplateSegment d1, DynamicTemplateSegment d2) =>
                d1.RouteVariable.Name == d2.RouteVariable.Name &&
                d1.RouteVariable.Type == d2.RouteVariable.Type,
            _ => false
        };
    }
}
