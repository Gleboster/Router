using Router.Models.Registrations;
using Router.Models.Segments;

namespace Router.Components.Registry.Trie;

public sealed class SegmentNode
{
    private readonly TemplateSegment segment;
    private readonly List<SegmentNode> children;
    private RouteRegistration registration;

    public SegmentNode(TemplateSegment segment = null)
    {
        this.segment = segment;
        children = new List<SegmentNode>();
    }

    public IReadOnlyList<SegmentNode> Children => children;

    public TemplateSegment Segment => segment;

    public RouteRegistration Registration => registration;

    public bool IsEndpoint => registration != null;

    public void AddChild(SegmentNode node)
    {
        children.Add(node);
    }

    public void SetRegistration(RouteRegistration registration)
    {
        this.registration = registration;
    }
}
