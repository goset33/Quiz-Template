using UnityEngine;
using UnityEngine.UIElements;

public class ChooseQuizViewer : MonoBehaviour
{
    [SerializeField] private UIDocument quizCard;

    private VisualElement root;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        ListView list = root.Q<ListView>();
        list.Clear();

        list.itemsChosen += OnQuizChosen;
    }

    private void OnQuizChosen(object obj)
    {
        if (obj is UIDocument uiDocument && uiDocument.Equals(quizCard))
        {
            
        }
    }
}
