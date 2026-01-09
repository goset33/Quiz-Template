using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UIElements;
using YG;

public class ResultController : AbstractController
{
	[SerializeField] private LocalizedString[] headerLocales, textLocales;

	private Label headerText, resultText;
	private Label expCountText, cashCountText;
	private Button doubleButton;

    private IntVariable rightAnswersLocale, allAnswersLocale;

	public override void Init()
	{
		base.Init();

		var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
		rightAnswersLocale = source["global"]["rightAnswers"] as IntVariable;
		allAnswersLocale = source["global"]["allAnswers"] as IntVariable;

		headerText = root.Q<Label>("HeaderText");
		resultText = root.Q<Label>("MainText");
		expCountText = root.Q<Label>("ExpText");
		cashCountText = root.Q<Label>("StarText");
		doubleButton = root.Q<Button>("DoubleButton");

		doubleButton.clicked += RewardButtonPressed;

		var doubleLabel = doubleButton.Q<VisualElement>("DoubleLabel");
		doubleLabel.schedule.Execute(() =>
		{
			if (doubleLabel.ClassListContains("double-reward--big"))
			{
				doubleLabel.RemoveFromClassList("double-reward--big");
			}
			else
			{
				doubleLabel.AddToClassList("double-reward--big");
			}
		}).Every(401);
	}

	public override void ChangeVisibilityState(bool newState)
	{
		base.ChangeVisibilityState(newState);
		SoundManager.Instance.ChangeMusicState();
		if (newState)
		{
			SoundManager.JingleEnded += ShowUI;
		}
		else
		{
			SoundManager.JingleEnded -= ShowUI;
		}
	}

	public void LoadResult(int rightAnswers, int allAnswers, int revives, bool isGood)
	{
		doubleButton.visible = true;

		int hardness = GameManager.Instance.GetQuizHardness();

		Dictionary<string, object> data = new() { 
			{ "Имя квиза", GameManager.Instance.chosenQuiz.GetName() }, 
			{ "Уровень сложности квиза", hardness }, 
			{ "Количество верных ответов", rightAnswers },
			{ "Количество возрождений", revives },
			{ "Игрок прошел квиз?", isGood } };
		YG2.MetricaSend("QuizEnded", data);

		int index = isGood ? 0 : 1;
		headerText.text = headerLocales[index].GetLocalizedString();    

		rightAnswersLocale.Value = rightAnswers;
		allAnswersLocale.Value = allAnswers;

		index = rightAnswers < 7 ? 0 : 1;
		resultText.text = textLocales[index].GetLocalizedString();

		if (hardness != 4)
		{
			expCountText.text = $"+{rightAnswers}";
		}
		else
		{
			expCountText.text = "+0";
		}

		cashCountText.text = $"+{rightAnswers * GameManager.Instance.config.cashAddCount[hardness - 1] * 2}";
		GameManager.ChangeCash(rightAnswers * GameManager.Instance.config.cashAddCount[hardness - 1]);

		bool isGotSmth = rightAnswers != 0;
		SoundManager.Instance.PlayJingle(isGood, isGotSmth);
	}

	private void ShowUI(int stage)
	{
		//if (rightAnswersLocale.Value == 0 && stage == 0)
		//{
		//    canvasGroups[3].DOFade(1f, 1f).OnComplete(() => canvasGroups[3].blocksRaycasts = true);
		//    return;
		//}

		//int startsFrom = stage == 0 ? 0 : 2;
		//for (int i = startsFrom; i < startsFrom + 2; i++)
		//{
		//    int index = i;
		//    canvasGroups[i].DOFade(1f, 1f).OnComplete(() => canvasGroups[index].blocksRaycasts = true);
		//}
	}

	public void RewardButtonPressed()
	{
		YG2.RewardedAdvShow("1", MultiplyReward);
	}

	private void MultiplyReward()
	{
        doubleButton.visible = false;

		Dictionary<string, object> data = new() {
			{ "Имя квиза", GameManager.Instance.chosenQuiz.GetName() },
			{ "Уровень сложности квиза", GameManager.Instance.GetQuizHardness() },
			{ "Количество звезд до удвоения", int.Parse(cashCountText.text) },
			{ "Количество опыта до удвоения", int.Parse(expCountText.text) } };
		YG2.MetricaSend("RewardDoubling", data);

        int rights = rightAnswersLocale.Value;
        if (GameManager.Instance.GetQuizHardness() != 4)
		{
			expCountText.text = $"+{rights * 2}";
			GameManager.Instance.AddExperience(rights);
		}

		GameManager.ChangeCash(int.Parse(cashCountText.text));
		cashCountText.text = $"+{int.Parse(cashCountText.text) * 2}";
	}
}
