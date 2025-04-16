using System.Collections.Generic;
using UnityEngine;
using YG;

public class ChooseController : MonoBehaviour
{
    public static GameManager gameManager;

    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private Transform cardsParent;
    public LevelHandler levelHandler;

    private void OnEnable()
    {
        levelHandler.UpdateLevelUI();
    }

    private void OnGameStart()
    {
        foreach (QuizCard content in YandexGame.savesData.currentQuizSeqence)
        {
            QuizCardSetter card = Instantiate(quizCardPrefab, cardsParent).GetComponent<QuizCardSetter>();
            card.cardContent = content;
            card.SetContent(this);
        }
    }

    public void RedrawOrder()
    {
        for (int i = 0; i < cardsParent.childCount; i++)
        {
            Destroy(cardsParent.GetChild(i).gameObject);
        }
        OnGameStart();
    }

    public void UpdateCardPos(QuizCard card)
    {
        gameManager.UpdateQuizCardPosition(card);
        RedrawOrder();
    }
}
