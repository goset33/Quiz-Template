using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

/// <summary>
/// Отвечает за изменение и отображение уровня и опыта игрока
/// </summary>
public class LevelHandler : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI levelCounter;

    /// <summary>
    /// Добавляет опыт игрока.
    /// Не отрисовывает изменения в UI. Для отрисовки смотреть <c>UpdateLevelUI</c>
    /// </summary>
    /// <param name="addedExp">Количество добавляемого опыта</param>
    public void AddExp(int addedExp)
    {
        YandexGame.savesData.experience += addedExp;
        if (YandexGame.savesData.experience >= YandexGame.savesData.requiredExp)
        {
            YandexGame.savesData.level++;
            YandexGame.savesData.experience -= YandexGame.savesData.requiredExp;
            YandexGame.savesData.requiredExp = YandexGame.savesData.level * 50 + 50;
        }
        YandexGame.SaveProgress();
    }

    /// <summary>
    /// Отрисовывает любые изменения переменных уровня и опыта 
    /// </summary>
    public void UpdateLevelUI()
    {
        slider.maxValue = YandexGame.savesData.requiredExp;
        slider.value = YandexGame.savesData.experience;
        levelCounter.text = YandexGame.savesData.level.ToString();
    }
}
