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
        YG2.saves.experience += addedExp;
        if (YG2.saves.experience >= YG2.saves.requiredExp)
        {
            YG2.saves.level++;
            YG2.saves.experience -= YG2.saves.requiredExp;
            YG2.saves.requiredExp = YG2.saves.level * 50 + 50;
        }
        YG2.SaveProgress();
    }

    /// <summary>
    /// Отрисовывает любые изменения переменных уровня и опыта 
    /// </summary>
    public void UpdateLevelUI()
    {
        slider.maxValue = YG2.saves.requiredExp;
        slider.value = YG2.saves.experience;
        levelCounter.text = YG2.saves.level.ToString();
    }
}
