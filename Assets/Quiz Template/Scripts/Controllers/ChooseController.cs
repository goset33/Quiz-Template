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
    //public LevelHandler levelHandler;

    private void OnEnable()
    {
        //inputField.onValueChanged.AddListener(RedrawOrder);
        //levelHandler.UpdateLevelUI();

        OnGameStart();
    }

    private void OnGameStart()
    {
        if (cardsParent.childCount != 0) return;

        foreach (QuizCard content in gameManager.quizzes)
        {
            if (content == null) continue;

            QuizCardSetter card = Instantiate(quizCardPrefab, cardsParent).GetComponent<QuizCardSetter>();
            //bool isFav = YG2.saves.favoriteCards.ContainsThatQuizCard(content);
            card.SetContent(content, this);
        }
    }

    public void BackInMenu()
    {
        gameManager.ReturnToMenu(transform);
    }

    // Все что ниже - устарело

    //private void OnDisable()
    //{
    //    inputField.onValueChanged.RemoveListener(RedrawOrder);
    //}

    /// <summary>
    /// Заново отрисовывает порядок квизов. Нужен только если существует поиск и избранные
    /// </summary>
    [Obsolete]
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

        OnGameStart();
    }

    [Obsolete]
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

    [Obsolete]
    public void UpdateCardPos(QuizCard card)
    {
        gameManager.SetAsFavorite(card);
        RedrawOrder(null);
    }
}
