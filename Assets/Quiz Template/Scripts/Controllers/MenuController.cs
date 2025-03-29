using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class MenuController : MonoBehaviour
{
    public static GameManager gameManager;

    private IntVariable buyCost;

    [Space]
    [SerializeField] TextMeshProUGUI starCounter;
    [SerializeField] Transform buttonContainer;

    [Space]
    public LocalizedString[] buyLevelLocales;

    private void Awake()
    {
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<UnityEngine.Localization.SmartFormat.Extensions.PersistentVariablesSource>();
        buyCost = source["global"]["levelCost"] as IntVariable;
    }

    // Инициализация меню после каждого включения
    private void OnEnable()
    {
        starCounter.GetComponentInChildren<Image>().sprite = gameManager.config.cashSprite;
        starCounter.text = YG.YandexGame.savesData.cash.ToString();

        bool isOpen = gameManager.IsLevelWasOpened(gameManager.chosenQuizIndex, 1);
        buttonContainer.GetChild(1).GetChild(1).gameObject.SetActive(!isOpen);

        isOpen = gameManager.IsLevelWasOpened(gameManager.chosenQuizIndex, 2);
        buttonContainer.GetChild(2).GetChild(1).gameObject.SetActive(!isOpen);
    }

    /// <summary>
    /// Функция для обработки нажатия кнопки уровня сложности
    /// </summary>
    /// <param name="levelNumber">Номер уровня сложности начиная с 0</param>
    public void LevelButtonPressed(int levelNumber)
    {
        if (!gameManager.IsLevelWasOpened(gameManager.chosenQuizIndex, levelNumber) && levelNumber != 0)
        {
            int price = levelNumber == 1 ? gameManager.config.mediumPrice : gameManager.config.hardPrice;
            buyCost.Value = price;

            gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, buyLevelLocales));
            TimelessController.OnButtonPressed += BuyButtonPressed;
            return;
        }

        gameManager.ChangeActiveWindow(transform, GameManager.GameState.SolvingQuestions, levelNumber);
    }

    /// <summary>
    /// Вызывается при нажатии кнопки покупки уровня сложности
    /// </summary>
    public void BuyButtonPressed(int buttonIndex)
    {
        TimelessController.OnButtonPressed -= BuyButtonPressed;
        if (buttonIndex == 1)
        {
            int num = buyCost.Value;
            int index = num == gameManager.config.mediumPrice ? 1 : 2;
            if (index == 2 && !gameManager.IsLevelWasOpened(gameManager.chosenQuizIndex, 1))
            {
                gameManager.InvokeNotification(1);
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
                gameManager.InvokeNotification(0);
            }

            OnEnable();
        }
    }

    public void BackInMenuButtonPressed()
    {
        gameManager.ChangeActiveWindow(transform, GameManager.GameState.ChoosingLevel, null);
    }
}
