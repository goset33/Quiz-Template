using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using YG;

public class TimelessController : MonoBehaviour
{
    [Header("Music Settings")]
    public GameObject musicButton;
    public Sprite[] musicSprites = new Sprite[2];

    [Space]
    public RectTransform popupWindow;
    public RectTransform notificationText;

    public delegate void onButtonPressed(int buttonIndex);
    public static event onButtonPressed OnButtonPressed;

    private void Awake()
    {
        if (!YandexGame.savesData.isMusicPlaying)
        {
            GetComponent<AudioSource>().Stop();
            musicButton.GetComponent<Image>().sprite = musicSprites[0];
            musicButton.transform.localScale = new Vector3(0.94f, 0.94f, 0.94f);
        }
        else
        {
            musicButton.GetComponent<Image>().sprite = musicSprites[1];
            musicButton.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Функция для обработки нажатия кнопки включения/выключения музыки
    /// </summary>
    public void MusicButtonPressed()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource.isPlaying && YandexGame.savesData.isMusicPlaying)
        {
            YandexGame.savesData.isMusicPlaying = false;

            audioSource.Stop();
            musicButton.GetComponent<Image>().sprite = musicSprites[0];
            musicButton.transform.localScale = new Vector3(0.94f, 0.94f, 0.94f);
        }
        else
        {
            YandexGame.savesData.isMusicPlaying = true;

            audioSource.Play();
            musicButton.GetComponent<Image>().sprite = musicSprites[1];
            musicButton.transform.localScale = Vector3.one;
        }
        YandexGame.SaveProgress();
    }

    /// <summary>
    /// Создает окно в соответствии с настройками
    /// </summary>
    /// <param name="settings">Настройки окна в виде специального класса</param>
    public void CreatePopup(PopupSettings settings)
    {
        popupWindow.gameObject.SetActive(true);
        popupWindow.sizeDelta = new Vector2(popupWindow.rect.width, (int) settings.size);

        popupWindow.GetChild(2).GetComponent<TextMeshProUGUI>().text = settings.title.GetLocalizedString();
        popupWindow.GetChild(3).GetComponent<TextMeshProUGUI>().text = settings.text.GetLocalizedString();
        popupWindow.GetChild(4).GetComponentInChildren<TextMeshProUGUI>().text = settings.button1.GetLocalizedString();
        popupWindow.GetChild(5).GetComponentInChildren<TextMeshProUGUI>().text = settings.button2.GetLocalizedString();
    }

    public void ButtonPressed(int index)
    {
        popupWindow.gameObject.SetActive(false);
        OnButtonPressed?.Invoke(index);
    }

    public void CreateNotification(LocalizedString localizedString)
    {
        DOTween.Kill(0);
        notificationText.GetComponent<TextMeshProUGUI>().text = localizedString.GetLocalizedString();
        notificationText.position = new Vector2(Screen.width / 2f, Screen.height / 2.5f);
        notificationText.GetComponent<TextMeshProUGUI>().color = Color.white;
        DOTween.Sequence()
            .Append(notificationText.DOAnchorPosY(notificationText.position.y + 1f, 2f))
            .Join(notificationText.GetComponent<TextMeshProUGUI>().DOFade(0f, 2f))
            .SetId(0);
    }
}
