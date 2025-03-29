using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс контролирует жизни во время ответа на тест
/// </summary>
public class HeartContainer : MonoBehaviour
{
    [SerializeField] private GameObject heartPrefab;

    private List<GameObject> hearts = new();

    public int HeartCount => hearts.Count;

    /// <summary>
    /// Необходимо вызвать вместе с инициализацией окна вопросов. Инициализация системы
    /// </summary>
    /// <param name="heartNumber">Количество жизней, которые будут у игрока на старте</param>
    public void InitializeHearts(int heartNumber)
    {
        if (heartNumber <= 0) return;

        if (HeartCount != 0)
        {
            for (int i = 0; i < hearts.Count; i++)
            {
                TakeOneDamage();
            }
        }

        for (int i = 0; i < heartNumber; i++)
        {
            HealOneHeart();
        }
    }
    
    /// <summary>
    /// Снимает одно сердце
    /// </summary>
    public void TakeOneDamage()
    {
        if (HeartCount != 0)
        {
            Destroy(hearts[0]);
            hearts.RemoveAt(0);
        }
    }

    /// <summary>
    /// Добавляет одно сердце
    /// </summary>
    public void HealOneHeart()
    {
        hearts.Add(Instantiate(heartPrefab, transform));
    }
}
