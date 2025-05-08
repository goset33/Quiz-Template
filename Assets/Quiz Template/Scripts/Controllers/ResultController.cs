using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;
using YG;

public class ResultController : MonoBehaviour
{
    public static GameManager gameManager;

    private IntVariable rightAnswersLocale, allAnswersLocale;

    [SerializeField] TextMeshProUGUI header, resultText;
    [SerializeField] Transform addingObject;
    [SerializeField] GameObject rewardButton;

    public LocalizedString[] headerLocales;

    private void Awake()
    {
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
        rightAnswersLocale = source["global"]["rightAnswers"] as IntVariable;
        allAnswersLocale = source["global"]["allAnswers"] as IntVariable;
    }

    public void Init(int rightAnswers, int allAnswers, bool isGood)
    {
        int index = isGood ? 0 : 1;
        header.text = headerLocales[index].GetLocalizedString();

        rightAnswersLocale.Value = rightAnswers;
        allAnswersLocale.Value = allAnswers;
        addingObject.GetComponentInChildren<Image>().sprite = gameManager.config.cashSprite;
        addingObject.GetComponent<TextMeshProUGUI>().text = $"+{rightAnswers * 2}";
        GameManager.ChangeCash(rightAnswers);
        gameManager.AddExperience(rightAnswers * 2);
    }

    public void RewardButtonPressed()
    {
        YG2.RewardedAdvShow("1", MultiplyReward);
    }

    private void MultiplyReward()
    {
        int rights = rightAnswersLocale.Value;
        GameManager.ChangeCash(rights * 2);
        addingObject.GetComponent<TextMeshProUGUI>().text = $"+{rights * 4}";
        rewardButton.SetActive(false);
    }

    public void BackButtonPressed()
    {
        rewardButton.SetActive(true);
        gameManager.ReturnToMenu(transform);
    }
}
