using System;
using UnityEngine.UIElements;

public abstract class AbstractObjectController : IDisposable
{
    public abstract void Init(float t, TimelessController controller, VisualElement element);

    public abstract void Dispose();
}
