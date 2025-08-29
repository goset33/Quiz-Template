using DG.Tweening;
using System.Collections.Generic;
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
    [SerializeField] private TextMeshProUGUI header, resultText;
    [SerializeField] private Transform cashObject, expObject;
    [SerializeField] private GameObject rewardButton, backButton;

    [Space]
    [SerializeField] private LocalizedString[] headerLocales;

    private CanvasGroup[] canvasGroups;
    private IntVariable rightAnswersLocale, allAnswersLocale;

    private void Awake()
    {
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
        rightAnswersLocale = source["global"]["rightAnswers"] as IntVariable;
        allAnswersLocale = source["global"]["allAnswers"] as IntVariable;

        canvasGroups = new CanvasGroup[4] { 
            cashObject.GetComponent<CanvasGroup>(), 
            expObject.GetComponent<CanvasGroup>(), 
            rewardButton.GetComponent<CanvasGroup>(), 
            backButton.GetComponent<CanvasGroup>() };

        expObject.GetComponentInChildren<Image>().sprite = GameManager.Instance.config.expSprite;
        cashObject.GetComponentInChildren<Image>().sprite = GameManager.Instance.config.cashSprite;
    }

    private void OnEnable()
    {
        SoundManager.JingleEnded += ShowUI;
    }

    private void OnDisable()
    {
        SoundManager.JingleEnded -= ShowUI;

        foreach (CanvasGroup group in canvasGroups)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }
    }

    public void Init(int rightAnswers, int allAnswers, int revives, bool isGood)
    {
        Dictionary<string, object> data = new() { 
            { "Имя квиза", GameManager.Instance.chosenQuiz.GetName() }, 
            { "Уровень сложности квиза", GameManager.Instance.GetQuizHardness() }, 
            { "Количество верных ответов", rightAnswers },
            { "Количество возрождений", revives },
            { "Игрок прошел квиз?", isGood } };
        YG2.MetricaSend("QuizEnded", data);

        int index = isGood ? 0 : 1;
        header.text = headerLocales[index].GetLocalizedString();

        rightAnswersLocale.Value = rightAnswers;
        allAnswersLocale.Value = allAnswers;

        expObject.GetComponent<TextMeshProUGUI>().text = $"+{rightAnswers}";

        cashObject.GetComponent<TextMeshProUGUI>().text = $"+{rightAnswers * 2}";
        GameManager.ChangeCash(rightAnswers);

        SoundManager.Instance.PlayJingle(isGood);
    }

    private void ShowUI(int stage)
    {
        int startsFrom = stage == 0 ? 0 : 2;
        for (int i = startsFrom; i < startsFrom + 2; i++)
        {
            int index = i;
            canvasGroups[i].DOFade(1f, 1f).OnComplete(() => canvasGroups[index].blocksRaycasts = true);
        }
    }

    public void RewardButtonPressed()
    {
        YG2.RewardedAdvShow("1", MultiplyReward);
    }

    private void MultiplyReward()
    {
        int rights = rightAnswersLocale.Value;

        Dictionary<string, object> data = new() {
            { "Имя квиза", GameManager.Instance.chosenQuiz.GetName() },
            { "Уровень сложности квиза", GameManager.Instance.GetQuizHardness() },
            { "Количество звезд до удвоения", rights * 2 },
            { "Количество опыта до удвоения", rights } };
        YG2.MetricaSend("RewardDoubling", data);

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
