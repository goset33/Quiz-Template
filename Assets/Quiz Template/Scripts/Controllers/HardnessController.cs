using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using System;
using YG;

// Ассет UISoftMask технически тоже больше не нужен
[Obsolete("Раньше использовался для выбора уровня сложности. Неактуален.")]
public class HardnessController : MonoBehaviour
{
    public static GameManager gameManager;
    //public static event Action<int> LevelChoosed;

    private IntVariable buyCost;

    [Space]
    [SerializeField] private TextMeshProUGUI starCounter;
    [SerializeField] private Transform buttonContainer;

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
        starCounter.text = YG2.saves.cash.ToString();

        //bool isOpen = gameManager.IsLevelWasOpened(gameManager.chosenQuiz, 1);
        //buttonContainer.GetChild(1).GetChild(1).gameObject.SetActive(!isOpen);

        //isOpen = gameManager.IsLevelWasOpened(gameManager.chosenQuiz, 2);
        //buttonContainer.GetChild(2).GetChild(1).gameObject.SetActive(!isOpen);
    }

    /// <summary>
    /// Функция для обработки нажатия кнопки уровня сложности
    /// </summary>
    /// <param name="levelNumber">Номер уровня сложности начиная с 0</param>
    //public void LevelButtonPressed(int levelNumber)
    //{
    //    //if (!gameManager.IsLevelWasOpened(gameManager.chosenQuiz, levelNumber) && levelNumber != 0)
    //    {
    //        //int price = levelNumber == 1 ? gameManager.config.mediumPrice : gameManager.config.hardPrice;
    //        //buyCost.Value = price;

    //        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, buyLevelLocales));
    //        TimelessController.OnButtonPressed += BuyButtonPressed;
    //        return;
    //    }

    //    LevelChoosed?.Invoke(levelNumber);
    //}

    /// <summary>
    /// Вызывается при нажатии кнопки покупки уровня сложности
    /// </summary>
    //public void BuyButtonPressed(int buttonIndex)
    //{
    //    TimelessController.OnButtonPressed -= BuyButtonPressed;
    //    if (buttonIndex == 1)
    //    {
    //        int num = buyCost.Value;
    //        //int index = num == gameManager.config.mediumPrice ? 1 : 2;
    //        //if (index == 2 && !gameManager.IsLevelWasOpened(gameManager.chosenQuiz, 1))
    //        {
    //            gameManager.InvokeNotification(1);
    //            return;
    //        }

    //        if (GameManager.HaveEnoughCash(-num))
    //        {
    //            GameManager.ChangeCash(-num);
    //            //gameManager.OpenedLevels.Add(new DoubleInt(gameManager.chosenQuiz, index));
    //            YG2.SaveProgress();
    //        }
    //        else
    //        {
    //            gameManager.InvokeNotification(0);
    //        }

    //        OnEnable();
    //    }
    //}

    public void BackInMenuButtonPressed()
    {
        gameManager.ReturnToMenu(transform);
    }
}
