using System;
using UnityEngine;

public class ChooseController : MonoBehaviour
{
    public static GameManager gameManager;

    public static event Action<int> QuizChoosed;

    public GameObject quizCardPrefab;
    public Transform cardsContainer;
    public LevelHandler levelHandler;

    private void OnEnable()
    {
        levelHandler.UpdateLevelUI();
    }

    public void OnGameStart(QuizCard[] quizCards)
    {
        foreach (QuizCard content in quizCards)
        {
            QuizCardSetter card = Instantiate(quizCardPrefab, cardsContainer).GetComponent<QuizCardSetter>();
            card.cardContent = content;
            card.SetContent(this);
        }
    }

    public void OnQuizChoosed(int quizIndex)
    {
        QuizChoosed?.Invoke(quizIndex);
    }
}
