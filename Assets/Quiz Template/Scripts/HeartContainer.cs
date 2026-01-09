using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Класс контролирует жизни во время ответа на тест
/// </summary>
public class HeartContainer : MonoBehaviour
{
    private List<Heart> hearts = new();

    public int AliveHeartCount => hearts.Count(heart => !heart.IsBroken);

    /// <summary>
    /// Необходимо вызвать вместе с инициализацией окна вопросов. Инициализация системы
    /// </summary>
    public void InitializeHearts(IEnumerable<VisualElement> hearts)
    {
        foreach (Image heart in hearts.Cast<Image>())
        {
            this.hearts.Add(new Heart(heart, false));
        }
    }

    public void ResetHearts()
    {
        foreach (Heart heart in hearts)
        {
            heart.ChangeBrokeStatus(false);
        }
    }

    /// <summary>
    /// Снимает одно сердце
    /// </summary>
    public void TakeOneDamage()
    {
        if (AliveHeartCount != 0)
        {
            hearts.Last(heart => !heart.IsBroken).ChangeBrokeStatus(true);
        }
    }

    /// <summary>
    /// Добавляет одно сердце
    /// </summary>
    public void HealOneHeart()
    {
        var brokenHeart = hearts.FirstOrDefault(heart => heart.IsBroken);
        brokenHeart?.ChangeBrokeStatus(false);
    }

    private class Heart
    {
        public Image Image { get; private set; }
        public bool IsBroken { get; private set; }

        public Heart(Image image, bool isBroken)
        {
            Image = image;
            IsBroken = isBroken;
        }

        public void ChangeBrokeStatus(bool newStatus)
        {
            if (newStatus)
            {
                Image.AddToClassList("heart--broken");
            } 
            else
            {
                Image.RemoveFromClassList("heart--broken");
            }

            IsBroken = newStatus;
        }
    }
}
