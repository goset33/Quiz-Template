using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using YG;

public class ChooseController : MonoBehaviour
{
    public static GameManager gameManager;

    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform cardsParent;
    public LevelHandler levelHandler;

    private void OnEnable()
    {
        inputField.onValueChanged.AddListener(RedrawOrder);

        levelHandler.UpdateLevelUI();
    }

    private void OnDisable()
    {
        inputField.onValueChanged.RemoveListener(RedrawOrder);
    }

    /// <summary>
    /// Заново отрисовывает порядок квизов
    /// </summary>
    public void RedrawOrder(string _)
    {
        for (int i = 0; i < cardsParent.childCount; i++)
        {
            Destroy(cardsParent.GetChild(i).gameObject);
        }

        QuizCard[] order;
        if (!string.IsNullOrEmpty(inputField.text))
        {
            order = SearchQuizzesByName(inputField.text);
        }
        else
        {
            string[] arr = YG2.saves.favoriteCards.Concat(YG2.saves.otherCards).ToArray();
            order = arr.ConvertToCards(gameManager.quizzes);
        }

        OnGameStart(order);
    }

    private void OnGameStart(QuizCard[] order)
    {
        foreach (QuizCard content in order)
        {
            if (content == null) continue;

            QuizCardSetter card = Instantiate(quizCardPrefab, cardsParent).GetComponent<QuizCardSetter>();
            card.cardContent = content;
            bool isFav = YG2.saves.favoriteCards.ContainsThatQuizCard(content);
            card.SetContent(this, isFav);
        }
    }

    private QuizCard[] SearchQuizzesByName(string name)
    {
        string[] arr = YG2.saves.favoriteCards.Concat(YG2.saves.otherCards).ToArray();
        QuizCard[] allCards = arr.ConvertToCards(gameManager.quizzes);
        List<QuizCard> result = new();

        foreach (QuizCard card in allCards)
        {
            if (card != null && card.names != null && card.names.Any(n => n != null && n.Contains(name, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(card);
            }
        }
        return result.ToArray();
    }

    public void UpdateCardPos(QuizCard card)
    {
        gameManager.SetAsFavorite(card);
        RedrawOrder(null);
    }
}
