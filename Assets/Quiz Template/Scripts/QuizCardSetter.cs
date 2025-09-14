using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

/// <summary>
/// Устанавливает содержимое карточки квиза при инициализации в соответствии с переданным cardContent
/// </summary>
public class QuizCardSetter : MonoBehaviour
{
    private QuizCard cardContent;

    private ChooseController controller;
    //[SerializeField] private Sprite favoriteSprite;
    //[SerializeField] private Sprite[] hardnessSprites = new Sprite[3];

    private QuizCardSaveData saveData;
    private Image imageComponent;
    private Slider sliderComponent;
    private TextMeshProUGUI levelComponent;

    public static event Action<QuizCard> QuizChoosed;

    private void OnEnable()
    {
        if (imageComponent == null)
        {
            imageComponent = GetComponent<Image>();
            sliderComponent = GetComponentInChildren<Slider>(true);
            levelComponent = sliderComponent.GetComponentInChildren<TextMeshProUGUI>(true);
            return;
        }

        UpdateContent();
    }

    public void SetContent(QuizCard card, ChooseController controller)
    {
        cardContent = card;
        saveData = YG2.saves.quizCards.GetSaveDataByQuizCard(cardContent);
        this.controller = controller;
        imageComponent.sprite = cardContent.image;

        SoundManager.Instance.AddUniqueSoundToButton(GetComponent<Button>(), 0);

        UpdateContent();

        //int index = GameManager.GetQuizHardness(cardContent);
        //transform.GetChild(1).GetComponent<Image>().sprite = hardnessSprites[index];
    }

    private void UpdateContent()
    {
        levelComponent.text = saveData.level.ToString();
        sliderComponent.maxValue = saveData.maxExp;
        sliderComponent.value = saveData.exp;
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
