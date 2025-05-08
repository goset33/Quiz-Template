using System;
using UnityEngine;
using UnityEngine.UI;

public class QuizCardSetter : MonoBehaviour
{
    private QuizCard cardContent;

    private ChooseController controller;
    //[SerializeField] private Sprite favoriteSprite;
    //[SerializeField] private Sprite[] hardnessSprites = new Sprite[3];

    private Image imageComponent;
    private Slider sliderComponent;

    public static event Action<QuizCard> QuizChoosed;

    private void OnEnable()
    {
        if (imageComponent != null) return;
        
        imageComponent = GetComponent<Image>();
        sliderComponent = GetComponentInChildren<Slider>(true);
    }

    public void SetContent(QuizCard card, ChooseController controller)
    {
        cardContent = card;
        this.controller = controller;
        imageComponent.sprite = cardContent.image;

        UpdateContent();

        //int index = GameManager.GetQuizHardness(cardContent);
        //transform.GetChild(1).GetComponent<Image>().sprite = hardnessSprites[index];
    }

    public void UpdateContent()
    {
        sliderComponent.maxValue = cardContent.maxExp;
        sliderComponent.value = cardContent.exp;
    }

    public void QuizChooseButtonPressed()
    {
        QuizChoosed?.Invoke(cardContent);
    }

    [Obsolete]
    public void FavoritePressed()
    {
        controller.UpdateCardPos(cardContent);
    }
}
