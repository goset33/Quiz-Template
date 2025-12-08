using UnityEngine;
using UnityEngine.UIElements;

public abstract class AbstractController : MonoBehaviour
{
    protected VisualElement root;

    public virtual void Init()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }

    public void ChangeVisibilityState()
    {
        ChangeVisibilityState(!root.visible);
    }

    public void ChangeVisibilityState(bool newState)
    {
        if (root == null) return;

        root.visible = newState;
    }
}
