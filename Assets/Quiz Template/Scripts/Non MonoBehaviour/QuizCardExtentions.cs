using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Расширения для массивов и объектов, для конвертации сохранений из строки в карточку квиза
/// </summary>
public static class QuizCardExtentions
{
    public static string[] ConvertToNames(this IList<QuizCard> array)
    {
        string[] names = new string[array.Count];
        for (int i = 0; i < array.Count; i++)
        {
            names[i] = array[i].GetName();
        }
        return names;
    }

    public static QuizCard[] ConvertToCards(this IList<string> names, IList<QuizCard> target)
    {
        QuizCard[] cards = new QuizCard[names.Count];
        for (int i = 0; i < names.Count; i++)
        {
            if (string.IsNullOrEmpty(names[i])) continue;

            for (int j = 0; j < target.Count; j++)
            {
                if (names[i] == target[j].GetName())
                {
                    cards[i] = target[j];
                    break;
                }
            }
        }

        return cards;
    }

    public static QuizCard ConvertToCard(this string name, IList<QuizCard> target)
    {
        if (string.IsNullOrEmpty(name)) return null;

        for (int j = 0; j < target.Count; j++)
        {
            if (target[j].GetName() == name)
            {
                return target[j];
            }
        }
        return null;
    }

    public static bool ContainsThatName(this IList<QuizCard> array, string s)
    {
        return array.Any(item => item.GetName() == s);
    }

    public static bool ContainsThatQuizCard(this IList<string> array, QuizCard card)
    {
        string s = card.GetName();
        return array.Any(item => item == s);
    }

    public static QuizCardSaveData GetSaveDataByQuizCard(this IList<QuizCardSaveData> array, QuizCard quizCard)
    {
        string id = quizCard.GetName();
        foreach (QuizCardSaveData data in array)
        {
            if (data.cardId == id)
            {
                return data;
            }
        }
        return null;
    }

    public static void MultiplyArray(this IList<int> array, int multiplier)
    {
        for (int i = 0; i < array.Count; i++)
        {
            array[i] *= multiplier;
        }
    }

    public static QuizCard GetQuizCardByQuizUIData(this IList<QuizCard> quizzes, QuizUIData data)
    {
        Sprite image = data.image;
        return quizzes.FirstOrDefault(quizCard => quizCard.image == data.image);
    }

    public static Color ConvertHexToColor(string hex)
    {
        string pureHex = hex;
        if (hex.StartsWith("#"))
        {
            pureHex = hex.Substring(1);
        }
        else if (hex.StartsWith("0x"))
        {
            pureHex= hex.Substring(2);
        }

        if (string.IsNullOrEmpty(pureHex) || pureHex.Length != 6) return Color.clear;

        int r = int.Parse(pureHex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        int g = int.Parse(pureHex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        int b = int.Parse(pureHex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color(r, g, b);
    }
}
