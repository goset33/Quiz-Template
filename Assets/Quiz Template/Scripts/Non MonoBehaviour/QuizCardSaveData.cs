using UnityEngine;

[System.Serializable]
public class QuizCardSaveData
{
    public string cardId;

    public int level = 1;
    public int maxExp = QuizCard.MIN_EXP;
    public int exp = 0;

    private const int MAX_QUIZ_LEVEL = 4;

    public QuizCardSaveData() { }

    public QuizCardSaveData(string identifier)
    {
        cardId = identifier;
        level = 1;
        exp = 0;
        maxExp = QuizCard.MIN_EXP * level;
    }

    /// <summary>
    /// Добавляет очки опыта и при необходимости обновляет уровень
    /// </summary>
    /// <param name="amount">Количество опыта, которое нужно добавить</param>
    /// <returns>True, если квиз повысил уровень сложности, false в противном случае</returns>
    public bool AddExperience(int amount)
    {
        if (level >= MAX_QUIZ_LEVEL)
        {
            maxExp = QuizCard.MIN_EXP * MAX_QUIZ_LEVEL; 
            exp = maxExp;
            return false;
        }

        exp += amount;
        return UpdateExp();
    }

    /// <summary>
    /// Обновляет опыт и уровень. Обрабатывает многократное повышение уровня, если набрано достаточно опыта
    /// </summary>
    /// <returns>True, если произошло повышение уровня, false в противном случае</returns>
    private bool UpdateExp()
    {
        bool leveledUp = false;
        while (exp >= maxExp && level < MAX_QUIZ_LEVEL)
        {
            exp -= maxExp;
            level++;
            leveledUp = true;

            maxExp = QuizCard.MIN_EXP * level;

            if (level >= MAX_QUIZ_LEVEL)
            {
                level = MAX_QUIZ_LEVEL;
                exp = maxExp;
                break;
            }
        }

        if (level < MAX_QUIZ_LEVEL)
        {
            if (exp < 0)
            {
                exp = 0;
            }
            if (exp > maxExp)
            {
                Debug.LogWarning($"Experience {exp} exceeds maxExp {maxExp} for level {level} without leveling up. Capping exp.");
                exp = maxExp;
            }
        }
        return leveledUp;
    }
}
