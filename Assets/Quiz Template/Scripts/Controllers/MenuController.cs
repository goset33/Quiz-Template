using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using DG.Tweening;

public class MenuController : MonoBehaviour
{
    public static GameManager gameManager;

    public MenuConfig config; // Конфиг нужно назначать из инспектора

    private IntVariable buyCost;
    private LocalizedString[] errorTexts;

    [Space]
    public GameObject buyWindow;
    public TextMeshProUGUI buyError, starCounter;
    public Transform buttonContainer;

    private void Awake()
    {
        if (config == null)
        {
            throw new NullReferenceException("No menu config!");
        }

        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<UnityEngine.Localization.SmartFormat.Extensions.PersistentVariablesSource>();
        buyCost = source["global"]["levelCost"] as IntVariable;
        errorTexts = new[] { source["global"]["buyError1"] as LocalizedString, source["global"]["buyError2"] as LocalizedString };
    }

    public void Init()
    {
        starCounter.GetComponentInChildren<Image>().sprite = config.cashSprite;
        starCounter.text = YG.YandexGame.savesData.cash.ToString();

        if (config.easyButtonImage != null)
        {
            buttonContainer.GetChild(0).GetComponent<Image>().sprite = config.easyButtonImage;
            buttonContainer.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().color = config.easyTextColor;
        }

        bool isOpen = gameManager.IsLevelWasOpened(gameManager.chosenQuizIndex, 1);
        Sprite sprite = isOpen ? config.mediumButtonImage : config.lockImage;
        Color color = isOpen ? config.mediumTextColor : config.lockColor;
        buttonContainer.GetChild(1).GetComponent<Image>().sprite = sprite;
        buttonContainer.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().color = color;

        isOpen = gameManager.IsLevelWasOpened(gameManager.chosenQuizIndex, 2);
        sprite = isOpen ? config.hardButtonImage : config.lockImage;
        color = isOpen ? config.hardTextColor : config.lockColor;
        buttonContainer.GetChild(2).GetComponent<Image>().sprite = sprite;
        buttonContainer.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().color = color;
    }

    // Функция для обработки нажатия кнопки уровня. На вход принимает номер уровня начиная с 0
    // Метод отбрасывает нажатия кнопок уровней, которые не должны запускаться
    public void LevelButtonPressed(int levelNumber)
    {
        if (!gameManager.IsLevelWasOpened(gameManager.chosenQuizIndex, levelNumber) && levelNumber != 0)
        {
            buyWindow.SetActive(true);
            int price = levelNumber == 1 ? config.mediumOpenPrice : config.hardOpenPrice;
            buyCost.Value = price;
            return;
        }

        gameManager.NextStep(levelNumber, transform);
    }

    public void BuyButtonPressed()
    {
        int num = buyCost.Value;
        int index = num == config.mediumOpenPrice ? 1 : 2;
        if (index == 2 && !gameManager.IsLevelWasOpened(gameManager.chosenQuizIndex, 1))
        {
            DoBuyError(0);
            buyWindow.SetActive(false);
            return;
        }

        if (GameManager.HaveEnoughCash(-num))
        {
            GameManager.ChangeCash(-num);
            gameManager.OpenedLevels.Add(new DoubleInt(gameManager.chosenQuizIndex, index));
            YG.YandexGame.SaveProgress();
        }
        else
        {
            DoBuyError(1);
        }

        buyWindow.SetActive(false);
        Init();
    }

    private void DoBuyError(int textIndex)
    {
        DOTween.Kill(0);
        buyError.text = errorTexts[textIndex].GetLocalizedString();
        buyError.GetComponent<RectTransform>().position = new Vector2(Screen.width / 2f, Screen.height / 2.5f);
        buyError.color = Color.white;
        DOTween.Sequence()
            .Append(buyError.GetComponent<RectTransform>().DOAnchorPosY(buyError.transform.position.y + 1f, 2f))    
            .Join(buyError.DOFade(0f, 2f))
            .SetId(0);
    }

    public void BackButtonPressed()
    {
        buyWindow.SetActive(false);
    }

    public void BackInMenuButtonPressed()
    {
        gameManager.BackInMenu(transform);
    }
}
