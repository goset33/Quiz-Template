using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public abstract class AbstractController : MonoBehaviour
{
    protected VisualElement root;

    public virtual void Init()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        var backInMenu = root.Q<Button>("Back");
        if (backInMenu != null)
        {
            backInMenu.clicked += BackInMenu;
        }
    }

    protected virtual void BackInMenu()
    {
        GameManager.Instance.OpenWindow<MenuController>();
    }

    public void ChangeVisibilityState()
    {
        ChangeVisibilityState(!root.visible);
    }

    public virtual void ChangeVisibilityState(bool newState)
    {
        if (root == null) return;

        root.visible = newState;
    }
}
