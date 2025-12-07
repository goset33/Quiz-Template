using static UnityEngine.UIElements.VisualElement;
using System.Xml.Serialization;
using UnityEngine.UIElements;
using UnityEngine;

public enum GradientDirection
{
    Horizontal,
    Vertical
}

[UxmlElement]
public partial class GradientElement : VisualElement
{
    static readonly Vertex[] _vertices = new Vertex[4];
    static readonly ushort[] _indices = { 0, 1, 2, 2, 3, 0 };
    private VisualElement gradient;

    [UxmlAttribute]
    public GradientDirection GradientDirection { get; set; } = GradientDirection.Horizontal;

    [UxmlAttribute]
    public Color GradientFrom { get; set; }

    [UxmlAttribute]
    public Color GradientTo { get; set; }

    public GradientElement()
    {
        this.style.overflow = Overflow.Hidden;

        gradient = new VisualElement();
        gradient.name = "Gradient";
        gradient.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        gradient.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        gradient.generateVisualContent += GenerateVisualContent;

        hierarchy.Add(gradient);
    }

    void GenerateVisualContent(MeshGenerationContext meshGenerationContext)
    {
        var rect = gradient.contentRect;

        if (rect.width < 0.1f || rect.height < 0.1f)
        {
            return;
        }

        UpdateVerticesTint();
        UpdateVerticesPosition(rect);

        var meshWriteData = meshGenerationContext.Allocate(_vertices.Length, _indices.Length);
        meshWriteData.SetAllVertices(_vertices);
        meshWriteData.SetAllIndices(_indices);
    }

    static void UpdateVerticesPosition(Rect rect)
    {
        const float left = 0f;
        var right = rect.width;
        const float top = 0f;
        var bottom = rect.height;

        _vertices[0].position = new Vector3(left, bottom, Vertex.nearZ);
        _vertices[1].position = new Vector3(left, top, Vertex.nearZ);
        _vertices[2].position = new Vector3(right, top, Vertex.nearZ);
        _vertices[3].position = new Vector3(right, bottom, Vertex.nearZ);
    }

    void UpdateVerticesTint()
    {
        if (GradientDirection is GradientDirection.Horizontal)
        {
            _vertices[0].tint = GradientFrom;
            _vertices[1].tint = GradientFrom;
            _vertices[2].tint = GradientTo;
            _vertices[3].tint = GradientTo;
        }
        else
        {
            _vertices[0].tint = GradientTo;
            _vertices[1].tint = GradientFrom;
            _vertices[2].tint = GradientFrom;
            _vertices[3].tint = GradientTo;
        }
    }
}