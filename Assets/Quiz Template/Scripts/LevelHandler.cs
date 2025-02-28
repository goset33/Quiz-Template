using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class LevelHandler : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI levelCounter;

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

    public void UpdateLevelUI()
    {
        slider.maxValue = YandexGame.savesData.requiredExp;
        slider.value = YandexGame.savesData.experience;
        levelCounter.text = YandexGame.savesData.level.ToString();
    }
}
