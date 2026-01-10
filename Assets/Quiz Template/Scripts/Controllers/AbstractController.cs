using System.Threading.Tasks;
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

    public virtual Task ChangeVisibilityStateAsync(bool newState) { return null; }

    public void ChangeVisibilityState()
    {
        ChangeVisibilityState(root.ClassListContains("hided"));
    }

    public virtual void ChangeVisibilityState(bool newState)
    {
        if (root == null) return;

        if (newState)
        {
            root.RemoveFromClassList("hided");
        } else
        {
            root.AddToClassList("hided");
        }
    }
}
