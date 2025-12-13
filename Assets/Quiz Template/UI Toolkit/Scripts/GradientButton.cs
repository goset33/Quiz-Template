using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class GradientButton : Button
{
    private static readonly int TextureSize = 2;
    private Texture2D _gradientTexture;

    private GradientDirection _gradientDirection = GradientDirection.Horizontal;
    private Color _gradientFrom = Color.blue;
    private Color _gradientTo = Color.cyan;

    [UxmlAttribute]
    public GradientDirection GradientDirection
    {
        get => _gradientDirection;
        set
        {
            if (_gradientDirection == value) return;
            _gradientDirection = value;
            UpdateGradient();
        }
    }

    [UxmlAttribute]
    public Color GradientFrom
    {
        get => _gradientFrom;
        set
        {
            if (_gradientFrom == value) return;
            _gradientFrom = value;
            UpdateGradient();
        }
    }

    [UxmlAttribute]
    public Color GradientTo
    {
        get => _gradientTo;
        set
        {
            if (_gradientTo == value) return;
            _gradientTo = value;
            UpdateGradient();
        }
    }

    public GradientButton()
    {
        RegisterCallback<AttachToPanelEvent>(_ => UpdateGradient());
        RegisterCallback<DetachFromPanelEvent>(_ => DestroyTexture());
    }

    private void DestroyTexture()
    {
        if (_gradientTexture == null) return;

        if (Application.isPlaying)
            Object.Destroy(_gradientTexture);
        else
            Object.DestroyImmediate(_gradientTexture);

        _gradientTexture = null;
    }

    private void UpdateGradient()
    {
        if (panel == null) return;

        bool isHorizontal = _gradientDirection == GradientDirection.Horizontal;
        int width = isHorizontal ? TextureSize : 1;
        int height = isHorizontal ? 1 : TextureSize;

        if (_gradientTexture == null ||
            _gradientTexture.width != width ||
            _gradientTexture.height != height)
        {
            DestroyTexture();
            _gradientTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        _gradientTexture.SetPixel(0, 0, _gradientTo);
        _gradientTexture.SetPixel(isHorizontal ? 1 : 0, isHorizontal ? 0 : 1, _gradientFrom);
        _gradientTexture.Apply();

        style.backgroundImage = new StyleBackground(_gradientTexture);
    }
}