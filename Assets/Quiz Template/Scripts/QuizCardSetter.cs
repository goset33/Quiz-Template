using System;
using UnityEngine;
using UnityEngine.UI;

public class QuizCardSetter : MonoBehaviour
{
    [HideInInspector] public QuizCard cardContent;

    private ChooseController controller;
    [SerializeField] private Sprite favoriteSprite;
    [SerializeField] private Sprite[] hardnessSprites = new Sprite[3];

    public static event Action<QuizCard> QuizChoosed;

    public void SetContent(ChooseController controller, bool isFavorite)
    {
        this.controller = controller;
        GetComponent<Image>().sprite = cardContent.image;

        int index = GameManager.GetQuizHardness(cardContent);
        transform.GetChild(1).GetComponent<Image>().sprite = hardnessSprites[index];

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
        QuizChoosed?.Invoke(cardContent);
    }
}
