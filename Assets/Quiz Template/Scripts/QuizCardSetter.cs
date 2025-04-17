using System;
using UnityEngine;
using UnityEngine.UI;

public class QuizCardSetter : MonoBehaviour
{
    [HideInInspector] public QuizCard cardContent;

    private ChooseController controller;
    [SerializeField] private Sprite favoriteSprite;

    public static event Action<int> QuizChoosed;

    public void SetContent(ChooseController controller, bool isFavorite)
    {
        this.controller = controller;
        GetComponent<Image>().sprite = cardContent.image;

        if (isFavorite)
        {
            transform.GetChild(0).GetComponent<Image>().sprite = favoriteSprite;
        }
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
