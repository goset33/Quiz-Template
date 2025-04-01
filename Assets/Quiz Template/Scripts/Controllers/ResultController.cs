using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class ResultController : MonoBehaviour
{
    public static GameManager gameManager;

    private IntVariable rightAnswersLocale, allAnswersLocale;

    public TextMeshProUGUI header, resultText;
    public Transform addingObject;

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

        //resultText.transform.localScale = new Vector3(7f, 7f, 7f);
        //resultText.transform.rotation = Quaternion.Euler(0f, 0f, -10f);

        //resultText.transform.DOScale(10f, .75f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        //resultText.transform.DOLocalRotate(new Vector3(0f, 0f, 10f), 5f).SetEase(Ease.Flash).SetLoops(-1, LoopType.Yoyo).SetDelay(0.1f);
    }

    public void MultiplyReward()
    {

    }

    public void BackButtonPressed()
    {
        gameManager.ReturnToMenu(transform);
    }
}
