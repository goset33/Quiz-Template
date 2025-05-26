using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadController : MonoBehaviour
{
    [SerializeField] private Transform canvas;

    private List<Image> images = new();
    private List<TextMeshProUGUI> texts = new();

    public void Awake()
    {
        for (int i = 0; i < canvas.childCount; i++)
        {
            Transform child = canvas.GetChild(i);

            if (child.TryGetComponent(out Image image))
            {
                images.Add(image);
            }
            else if (child.TryGetComponent(out TextMeshProUGUI text))
            {
                texts.Add(text);
            }
        }
    }

    public void StartLoad(Action callback = null)
    {
        canvas.gameObject.SetActive(true);
        StartCoroutine(FadeVisuals(1f, 0.5f, callback));
    }

    public void EndLoad(Action callback = null)
    {
        StartCoroutine(FadeVisuals(0f, 0.5f, callback));
    }

    /// <summary>
    /// Основной метод для плавной анимации экрана загрузки
    /// </summary>
    /// <param name="targetAlpha">Целевое значение прозрачности, от 0 до 1</param>
    /// <param name="duration">Время анимации</param>
    /// <param name="callback">Ивент, вызывающийся по окончанию анимации</param>
    IEnumerator FadeVisuals(float targetAlpha, float duration, Action callback = null)
    {
        Sequence fadeSequence = DOTween.Sequence();

        foreach (Image img in images)
        {
            if (img != null)
            {
                fadeSequence.Join(img.DOFade(targetAlpha, duration));
            }
        }

        foreach (TextMeshProUGUI txt in texts)
        {
            if (txt != null)
            {
                fadeSequence.Join(txt.DOFade(targetAlpha, duration));
            }
        }

        fadeSequence.OnComplete(() => {
            callback?.Invoke();
            if (targetAlpha == 0f)
            {
                canvas.gameObject.SetActive(false);
            }
        });

        fadeSequence.Play();

        yield return fadeSequence.WaitForCompletion();
    }
}