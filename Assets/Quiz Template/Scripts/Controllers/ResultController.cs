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
    private IntVariable rightAnswersLocale, allAnswersLocale;

    [SerializeField] TextMeshProUGUI header, resultText;
    [SerializeField] Transform cashObject, expObject;
    [SerializeField] GameObject rewardButton;

    public LocalizedString[] headerLocales;

    private void Awake()
    {
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
        rightAnswersLocale = source["global"]["rightAnswers"] as IntVariable;
        allAnswersLocale = source["global"]["allAnswers"] as IntVariable;

        expObject.GetComponentInChildren<Image>().sprite = GameManager.Instance.config.expSprite;
        cashObject.GetComponentInChildren<Image>().sprite = GameManager.Instance.config.cashSprite;
    }

    public void Init(int rightAnswers, int allAnswers, bool isGood)
    {
        int index = isGood ? 0 : 1;
        header.text = headerLocales[index].GetLocalizedString();

        rightAnswersLocale.Value = rightAnswers;
        allAnswersLocale.Value = allAnswers;

        expObject.GetComponent<TextMeshProUGUI>().text = $"+{rightAnswers}";

        cashObject.GetComponent<TextMeshProUGUI>().text = $"+{rightAnswers * 2}";
        GameManager.ChangeCash(rightAnswers);
    }

    public void RewardButtonPressed()
    {
        YG2.RewardedAdvShow("1", MultiplyReward);
    }

    private void MultiplyReward()
    {
        int rights = rightAnswersLocale.Value;

        expObject.GetComponent<TextMeshProUGUI>().text = $"+{rights * 2}";
        GameManager.Instance.AddExperience(rights);

        cashObject.GetComponent<TextMeshProUGUI>().text = $"+{rights * 4}";
        GameManager.ChangeCash(rights * 2);

        rewardButton.SetActive(false);
    }

    public void BackButtonPressed()
    {
        rewardButton.SetActive(true);
        GameManager.Instance.ReturnToMenu(transform);
    }
}
