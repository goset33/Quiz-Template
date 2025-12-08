using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class TimelessController : AbstractController
{
    [SerializeField] private RectTransform popupWindow, notificationText;

   public static event Action<int> OnPopupButtonPressed;

    /// <summary>
    /// Создает окно в соответствии с настройками
    /// </summary>
    /// <param name="settings">Настройки окна в виде специального класса</param>
    public void CreatePopup(PopupSettings settings)
    {
        SoundManager.Instance.ChangeMusicState();

        popupWindow.gameObject.SetActive(true);
        popupWindow.sizeDelta = new Vector2(popupWindow.rect.width, (int) settings.size);

        RectTransform textTransform = popupWindow.GetChild(3).GetComponent<RectTransform>();

        popupWindow.GetChild(2).GetComponent<TextMeshProUGUI>().text = settings.title.GetLocalizedString();
        textTransform.GetComponent<TextMeshProUGUI>().text = settings.text.GetLocalizedString();
        popupWindow.GetChild(5).GetComponentInChildren<TextMeshProUGUI>().text = settings.button1.GetLocalizedString();
        popupWindow.GetChild(6).GetComponentInChildren<TextMeshProUGUI>().text = settings.button2.GetLocalizedString();

        if (settings.objectPrefab == null || (int)settings.size < 1000)
        {
            textTransform.offsetMin = new Vector2(textTransform.offsetMin.x, 145f);
        }
        else
        {
            textTransform.offsetMin = new Vector2(textTransform.offsetMin.x, 510f);
            Instantiate(settings.objectPrefab, popupWindow.GetChild(4));
        }
    }

    public void ButtonPressed(int index)
    {
        popupWindow.gameObject.SetActive(false);
        SoundManager.Instance.ChangeMusicState();
        OnPopupButtonPressed?.Invoke(index);
    }

    /// <summary>
    /// Создает всплывающий текст-уведомление
    /// </summary>
    /// <param name="localizedString">Локализованная строка текста, который будет показан</param>
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
