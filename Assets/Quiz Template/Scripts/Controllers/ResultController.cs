using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class ResultController : MonoBehaviour
{
    public static GameManager gameManager;

    private IntVariable rightAnswersLocale;

    public TextMeshProUGUI resultText;

    private void Awake()
    {
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<UnityEngine.Localization.SmartFormat.Extensions.PersistentVariablesSource>();
        rightAnswersLocale = source["global"]["rightAnswers"] as IntVariable;

    }

    public void Init(int rightAnswers)
    {
        rightAnswersLocale.Value = rightAnswers;

        resultText.transform.localScale = new Vector3(7f, 7f, 7f);
        resultText.transform.rotation = Quaternion.Euler(0f, 0f, -10f);

        resultText.transform.DOScale(10f, .75f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        resultText.transform.DOLocalRotate(new Vector3(0f, 0f, 10f), 5f).SetEase(Ease.Flash).SetLoops(-1, LoopType.Yoyo).SetDelay(0.1f);
    }

    public void RestartButtonPressed()
    {
        DOTween.KillAll();
        gameManager.RestartLastQuiz(transform);
    }

    public void BackButtonPressed()
    {
        DOTween.KillAll();
        gameManager.BackInMenu(transform);
    }
}
