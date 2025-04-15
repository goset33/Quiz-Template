using System;
using UnityEngine;
using UnityEngine.UI;

public class QuizCardSetter : MonoBehaviour
{
    [HideInInspector] public QuizCard cardContent;

    private ChooseController controller;

    public static event Action<int> QuizChoosed;

    public void SetContent(ChooseController controller)
    {
        this.controller = controller;
        GetComponent<Image>().sprite = cardContent.image;
    }

    public void FavoritePressed()
    {
        controller.UpdateCardPos(cardContent);
    }

    public void QuizChooseButtonPressed()
    {
        QuizChoosed?.Invoke(transform.GetSiblingIndex());
    }
}
