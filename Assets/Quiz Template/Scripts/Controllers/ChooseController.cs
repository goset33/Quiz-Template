using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class ChooseController : MonoBehaviour
{
    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private Transform cardsParent;
    [SerializeField] private TextMeshProUGUI cashCounter;
    [SerializeField] private TMP_InputField inputField;

    private void OnEnable()
    {
        //inputField.onValueChanged.AddListener(RedrawOrder);

        OnGameStart();
    }

    private void OnGameStart()
    {
        cashCounter.text = YG2.saves.cash.ToString();
        Image image = cashCounter.GetComponentInChildren<Image>();
        if (image.sprite == null)
        {
            image.sprite = GameManager.Instance.config.cashSprite;
        }

        if (cardsParent.childCount != 0) return;

        foreach (QuizCard content in GameManager.Instance.quizzes)
        {
            if (content == null) continue;

            QuizCardSetter card = Instantiate(quizCardPrefab, cardsParent).GetComponent<QuizCardSetter>();
            //bool isFav = YG2.saves.favoriteCards.ContainsThatQuizCard(content);
            card.SetContent(content, this);
        }
    }

    public void BackInMenu()
    {
        GameManager.Instance.ReturnToMenu(transform);
    }

    // --- Все что ниже - устарело ---

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
            order = arr.ConvertToCards(GameManager.Instance.quizzes);
        }

        OnGameStart();
    }

    [Obsolete]
    private QuizCard[] SearchQuizzesByName(string name)
    {
        string[] arr = YG2.saves.favoriteCards.Concat(YG2.saves.otherCards).ToArray();
        QuizCard[] allCards = arr.ConvertToCards(GameManager.Instance.quizzes);
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
        GameManager.Instance.SetAsFavorite(card);
        RedrawOrder(null);
    }
}
