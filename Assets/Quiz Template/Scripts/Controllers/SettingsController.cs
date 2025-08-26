using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using YG;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Slider musicSlider, vfxSlider;
    [SerializeField] private TextMeshProUGUI musicValue, vfxValue; 

    private void Awake()
    {
        musicSlider.onValueChanged.AddListener(MusicSliderValueChanged);
        vfxSlider.onValueChanged.AddListener(VfxSliderValueChanged);

        musicSlider.value = YG2.saves.musicVolume;
        vfxSlider.value = YG2.saves.vfxVolume;

        musicValue.text = Mathf.RoundToInt(YG2.saves.musicVolume * 100).ToString();
        vfxValue.text = Mathf.RoundToInt(YG2.saves.vfxVolume * 100).ToString();
    }

    private void MusicSliderValueChanged(float value)
    {
        YG2.saves.musicVolume = value;
        SoundManager.Instance.SetMusicVolume(value);

        musicValue.text = Mathf.RoundToInt(YG2.saves.musicVolume * 100).ToString();
    }

    private void VfxSliderValueChanged(float value)
    {
        YG2.saves.vfxVolume = value;
        SoundManager.Instance.SetVfxVolume(value);

        vfxValue.text = Mathf.RoundToInt(YG2.saves.vfxVolume * 100).ToString();
    }

    public void BackInMenu()
    {
        GameManager.Instance.ReturnToMenu(transform);
    }
}
