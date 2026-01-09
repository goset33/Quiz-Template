using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Расширенный элемент кругового прогресс-бара для UI Toolkit.
/// </summary>
[UxmlElement]
public partial class RadialProgressBar : VisualElement
{
    public static readonly string ussClassName = "radial-progress-bar";
    public static readonly string ussLabelClassName = "radial-progress-bar__label";

    // Mesh объекты
    EllipseMesh m_TrackMesh;
    EllipseMesh m_ProgressMesh;

    Label m_Label;

    const int k_NumSteps = 200;

    // Значения и параметры
    float m_MinValue = 0f;
    float m_MaxValue = 100f;
    float m_Value = 0f; // реальное значение в диапазоне [min, max]

    Color m_TrackColor = Color.gray;
    Color m_ProgressColor = Color.yellow;

    bool m_Clockwise = true;
    bool m_UseIntegerValues = true;
    bool m_CenterShowRounded = true;
    int m_FractionalDigits = 2;
    bool m_DisplayAsPercentage = true; // true - показываем %; false - показываем raw value

    float m_BorderSize = 10f;

    /// <summary>
    /// Минимальное значение шкалы (UXML).
    /// </summary>
    [UxmlAttribute]
    public float minValue
    {
        get => m_MinValue;
        set
        {
            if (Mathf.Approximately(m_MinValue, value)) return;
            m_MinValue = value;
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Максимальное значение шкалы (UXML).
    /// </summary>
    [UxmlAttribute]
    public float maxValue
    {
        get => m_MaxValue;
        set
        {
            if (Mathf.Approximately(m_MaxValue, value)) return;
            m_MaxValue = value;
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Текущее значение (UXML).
    /// </summary>
    [UxmlAttribute]
    public float value
    {
        get => m_Value;
        set
        {
            m_Value = value;
            UpdateLabelText();
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Цвет фона
    /// </summary>
    [UxmlAttribute]
    public Color TrackColor
    {
        get => m_TrackColor;
        set
        {
            m_TrackColor = value;
            UpdateCustomStyles();
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Цвет прогресса
    /// </summary>
    [UxmlAttribute]
    public Color ProgressColor
    {
        get => m_ProgressColor;
        set
        {
            m_ProgressColor = value;
            UpdateCustomStyles();
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Направление заполнения. true — по часовой, false — против часовой (UXML).
    /// </summary>
    [UxmlAttribute]
    public bool clockwise
    {
        get => m_Clockwise;
        set
        {
            if (m_Clockwise == value) return;
            m_Clockwise = value;
            if (m_ProgressMesh != null) m_ProgressMesh.Clockwise = m_Clockwise;
            if (m_TrackMesh != null) m_TrackMesh.Clockwise = m_Clockwise;
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Использовать целочисленные значения (true) или дробные (false) при отображении центрального текста (UXML).
    /// </summary>
    [UxmlAttribute]
    public bool useIntegerValues
    {
        get => m_UseIntegerValues;
        set
        {
            if (m_UseIntegerValues == value) return;
            m_UseIntegerValues = value;
            UpdateLabelText();
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Показать в центре округлённое значение (true) или "реальное" (false). Для целочисленного режима "реальное" будет усечено вниз. (UXML)
    /// </summary>
    [UxmlAttribute]
    public bool centerShowRounded
    {
        get => m_CenterShowRounded;
        set
        {
            if (m_CenterShowRounded == value) return;
            m_CenterShowRounded = value;
            UpdateLabelText();
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Количество дробных знаков при отображении дробных значений (UXML).
    /// </summary>
    [UxmlAttribute]
    public int fractionalDigits
    {
        get => m_FractionalDigits;
        set
        {
            m_FractionalDigits = Mathf.Max(0, value);
            UpdateLabelText();
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Если true - в центре показываем процент заполнения; иначе - реальное значение (UXML).
    /// </summary>
    [UxmlAttribute]
    public bool displayAsPercentage
    {
        get => m_DisplayAsPercentage;
        set
        {
            if (m_DisplayAsPercentage == value) return;
            m_DisplayAsPercentage = value;
            UpdateLabelText();
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Толщина бордера (внутренний радиус) кольца (UXML).
    /// </summary>
    [UxmlAttribute]
    public float borderSize
    {
        get => m_BorderSize;
        set
        {
            if (Mathf.Approximately(m_BorderSize, value)) return;
            m_BorderSize = Mathf.Max(0f, value);
            if (m_ProgressMesh != null) m_ProgressMesh.BorderSize = m_BorderSize;
            if (m_TrackMesh != null) m_TrackMesh.BorderSize = m_BorderSize;
            MarkDirtyRepaint();
        }
    }

    public RadialProgressBar()
    {
        m_Label = new Label();
        m_Label.AddToClassList(ussLabelClassName);
        Add(m_Label);

        m_ProgressMesh = new EllipseMesh(k_NumSteps) { Color = Color.green, BorderSize = m_BorderSize, Clockwise = m_Clockwise };
        m_TrackMesh = new EllipseMesh(k_NumSteps) { Color = Color.gray, BorderSize = m_BorderSize, Clockwise = m_Clockwise };

        AddToClassList(ussClassName);

        RegisterCallback<CustomStyleResolvedEvent>(evt => CustomStylesResolved(evt));
        generateVisualContent += context => GenerateVisualContent(context);

        value = m_MinValue;
    }

    static void CustomStylesResolved(CustomStyleResolvedEvent evt)
    {
        var element = (RadialProgressBar)evt.currentTarget;
        element.UpdateCustomStyles();
    }

    void UpdateCustomStyles()
    {
        m_ProgressMesh.Color = m_ProgressColor;
        m_TrackMesh.Color = m_TrackColor;

        MarkDirtyRepaint();
    }

    static void GenerateVisualContent(MeshGenerationContext context)
    {
        var element = (RadialProgressBar)context.visualElement;
        element.DrawMeshes(context);
    }

    void DrawMeshes(MeshGenerationContext context)
    {
        float halfWidth = contentRect.width * 0.5f;
        float halfHeight = contentRect.height * 0.5f;
        if (halfWidth < 2f || halfHeight < 2f) return;

        m_ProgressMesh.Width = halfWidth;
        m_ProgressMesh.Height = halfHeight;
        m_ProgressMesh.BorderSize = borderSize;
        m_ProgressMesh.UpdateMesh();

        m_TrackMesh.Width = halfWidth;
        m_TrackMesh.Height = halfHeight;
        m_TrackMesh.BorderSize = borderSize;
        m_TrackMesh.UpdateMesh();

        // Рисуем трек
        var trackMeshWriteData = context.Allocate(m_TrackMesh.Vertices.Length, m_TrackMesh.Indices.Length);
        trackMeshWriteData.SetAllVertices(m_TrackMesh.Vertices);
        trackMeshWriteData.SetAllIndices(m_TrackMesh.Indices);

        // Нормализуем значение в [0..1]
        float normalized;
        if (!Mathf.Approximately(m_MaxValue, m_MinValue))
            normalized = Mathf.Clamp01((m_Value - m_MinValue) / (m_MaxValue - m_MinValue));
        else
            normalized = 0f;

        // Определяем сколько шагов нужно заполнить
        int sliceSteps = Mathf.FloorToInt(k_NumSteps * normalized);
        if (sliceSteps <= 0) return;

        int sliceSize = sliceSteps * 6; // 6 индексов на шаг
        var progressMeshWriteData = context.Allocate(m_ProgressMesh.Vertices.Length, sliceSize);
        progressMeshWriteData.SetAllVertices(m_ProgressMesh.Vertices);

        var tempIndicesArray = new NativeArray<ushort>(m_ProgressMesh.Indices, Allocator.Temp);
        progressMeshWriteData.SetAllIndices(tempIndicesArray.Slice(0, sliceSize));
        tempIndicesArray.Dispose();
    }

    void UpdateLabelText()
    {
        float normalized;
        if (!Mathf.Approximately(m_MaxValue, m_MinValue))
            normalized = Mathf.Clamp01((m_Value - m_MinValue) / (m_MaxValue - m_MinValue));
        else
            normalized = 0f;

        float displayVal = m_DisplayAsPercentage ? normalized * 100f : m_Value;

        string text;
        if (m_UseIntegerValues)
        {
            if (m_CenterShowRounded)
                text = Mathf.RoundToInt(displayVal).ToString();
            else
                text = Mathf.FloorToInt(displayVal).ToString();
        }
        else
        {
            if (m_CenterShowRounded)
                text = displayVal.ToString("F" + m_FractionalDigits);
            else
                text = displayVal.ToString(); // "реальное" значение (стандартный float.ToString())
        }

        if (m_DisplayAsPercentage)
            text += "%";

        m_Label.text = text;
    }

    // Вспомогательный класс — mesh для кольца
    public class EllipseMesh
    {
        int m_NumSteps;
        float m_Width;
        float m_Height;
        Color m_Color = Color.white;
        float m_BorderSize;
        bool m_IsDirty;
        bool m_Clockwise = true;

        public Vertex[] Vertices { get; private set; }
        public ushort[] Indices { get; private set; }

        public EllipseMesh(int numSteps)
        {
            m_NumSteps = Mathf.Max(3, numSteps);
            m_IsDirty = true;
        }

        public void UpdateMesh()
        {
            if (!m_IsDirty) return;

            int numVertices = m_NumSteps * 2;
            int numIndices = numVertices * 3; // = m_NumSteps * 6

            if (Vertices == null || Vertices.Length != numVertices)
                Vertices = new Vertex[numVertices];

            if (Indices == null || Indices.Length != numIndices)
                Indices = new ushort[numIndices];

            float stepSize = 360.0f / (float)m_NumSteps;
            float angle = -180.0f;

            for (int i = 0; i < m_NumSteps; ++i)
            {
                // направление заполнения влияет на знак приращения угла
                angle += m_Clockwise ? -stepSize : stepSize;
                float radians = Mathf.Deg2Rad * angle;
                float outerX = Mathf.Sin(radians) * Width;
                float outerY = Mathf.Cos(radians) * Height;

                Vertex outerVertex = new Vertex
                {
                    position = new Vector3(Width + outerX, Height + outerY, Vertex.nearZ),
                    tint = Color
                };
                Vertices[i * 2] = outerVertex;

                float innerX = Mathf.Sin(radians) * (Width - BorderSize);
                float innerY = Mathf.Cos(radians) * (Height - BorderSize);

                Vertex innerVertex = new Vertex
                {
                    position = new Vector3(Width + innerX, Height + innerY, Vertex.nearZ),
                    tint = Color
                };
                Vertices[i * 2 + 1] = innerVertex;

                // Формируем индексы. Для корректного отображения при разном направлении обхода
                // необходимо менять порядок вершин треугольников (winding).
                if (m_Clockwise)
                {
                    // Обычный порядок (как было):
                    // tri A: previousOuter, currentOuter, currentInner
                    // tri B: previousOuter, currentInner, previousInner
                    Indices[i * 6] = (ushort)((i == 0) ? Vertices.Length - 2 : (i - 1) * 2); // previous outer
                    Indices[i * 6 + 1] = (ushort)(i * 2); // current outer
                    Indices[i * 6 + 2] = (ushort)(i * 2 + 1); // current inner
                    Indices[i * 6 + 3] = (ushort)((i == 0) ? Vertices.Length - 2 : (i - 1) * 2); // previous outer
                    Indices[i * 6 + 4] = (ushort)(i * 2 + 1); // current inner
                    Indices[i * 6 + 5] = (ushort)((i == 0) ? Vertices.Length - 1 : (i - 1) * 2 + 1); // previous inner
                }
                else
                {
                    // Инвертируем порядок вершин каждого треугольника, чтобы сохранить "front-facing" треугольников:
                    // tri A (reversed): previousOuter, currentInner, currentOuter
                    // tri B (reversed): previousOuter, previousInner, currentInner
                    Indices[i * 6] = (ushort)((i == 0) ? Vertices.Length - 2 : (i - 1) * 2); // previous outer
                    Indices[i * 6 + 1] = (ushort)(i * 2 + 1); // current inner
                    Indices[i * 6 + 2] = (ushort)(i * 2); // current outer
                    Indices[i * 6 + 3] = (ushort)((i == 0) ? Vertices.Length - 2 : (i - 1) * 2); // previous outer
                    Indices[i * 6 + 4] = (ushort)((i == 0) ? Vertices.Length - 1 : (i - 1) * 2 + 1); // previous inner
                    Indices[i * 6 + 5] = (ushort)(i * 2 + 1); // current inner
                }
            }

            m_IsDirty = false;
        }

        public bool IsDirty => m_IsDirty;

        void CompareAndWrite(ref float field, float newValue)
        {
            if (Mathf.Abs(field - newValue) > float.Epsilon)
            {
                m_IsDirty = true;
                field = newValue;
            }
        }

        public int NumSteps
        {
            get => m_NumSteps;
            set
            {
                m_IsDirty = value != m_NumSteps;
                m_NumSteps = Mathf.Max(3, value);
            }
        }

        public float Width
        {
            get => m_Width;
            set => CompareAndWrite(ref m_Width, value);
        }

        public float Height
        {
            get => m_Height;
            set => CompareAndWrite(ref m_Height, value);
        }

        public Color Color
        {
            get => m_Color;
            set
            {
                if (value != m_Color) m_IsDirty = true;
                m_Color = value;
            }
        }

        public float BorderSize
        {
            get => m_BorderSize;
            set => CompareAndWrite(ref m_BorderSize, value);
        }

        /// <summary>
        /// Если true — вершины генерируются по часовой стрелке, иначе — против часовой.
        /// </summary>
        public bool Clockwise
        {
            get => m_Clockwise;
            set
            {
                if (m_Clockwise != value)
                {
                    m_IsDirty = true;
                    m_Clockwise = value;
                }
            }
        }
    }
}
