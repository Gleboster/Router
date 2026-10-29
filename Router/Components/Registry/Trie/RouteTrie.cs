using Router.Models.Registrations;
using Router.Models.Routes;
using Router.Models.Segments;
using Router.Utils;

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
            var templateSegment = segment;
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
            if (routeSegment.MatchTemplateSegment(child.Segment))
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

    private SegmentNode FindOrCreateChild(SegmentNode parent, TemplateSegment segment)
    {
        foreach (var child in parent.Children)
        {
            if (child.Segment.Equal(segment))
            {
                return child;
            }
        }

        var newNode = new SegmentNode(segment);
        parent.AddChild(newNode);
        return newNode;
    }
}
