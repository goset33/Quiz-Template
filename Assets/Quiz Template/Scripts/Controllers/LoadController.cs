using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadController : AbstractController
{
    private ProgressBar loadBar;
    private VisualElement starsBackground;

    private BackgroundPosition currentOffsetX = new(BackgroundPositionKeyword.Right, 20);
    private BackgroundPosition currentOffsetY = new(BackgroundPositionKeyword.Top, -10);
    private StyleBackgroundPosition cachedStylePositionX = new();
    private StyleBackgroundPosition cachedStylePositionY = new();

    public override void Init()
    {
        base.Init();

        loadBar = root.Q<ProgressBar>();
        starsBackground = root.Q<VisualElement>("BackgroundStars");

        loadBar.value = 33;

        new Delayer(this).IntervalForOneFrame(() =>
        {
            if (!root.visible) return;

            currentOffsetX.offset.value -= 10f * Time.deltaTime;
            currentOffsetY.offset.value += 10f * Time.deltaTime;

            cachedStylePositionX.value = currentOffsetX;
            cachedStylePositionY.value = currentOffsetY;

            starsBackground.style.backgroundPositionX = cachedStylePositionX;
            starsBackground.style.backgroundPositionY = cachedStylePositionY;
        });
    }

    public void StartLoad(Action callback = null)
    {
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

        if (targetAlpha > 0 && !root.visible)
        {
            root.visible = true;
            root.style.opacity = 0f;
            loadBar.value = 0;
        }
        else if (targetAlpha == 0 && root.visible)
        {
            root.style.opacity = 1f;

            fadeSequence.Append(DOTween.To(() => loadBar.value, x => loadBar.value = x, 66f, 0.3f));
            fadeSequence.AppendInterval(0.3f);
            fadeSequence.Append(DOTween.To(() => loadBar.value, x => loadBar.value = x, 100f, 0.3f));
        }

        fadeSequence.Append(DOTween.To(() => root.resolvedStyle.opacity, x => root.style.opacity = x, targetAlpha, duration));

        if (targetAlpha > 0)
        {
            fadeSequence.Append(DOTween.To(() => loadBar.value, x => loadBar.value = x, 33f, 0.3f));
        }

        fadeSequence.OnComplete(() =>
        {
            if (targetAlpha == 0)
            {
                root.visible = false;
            }
            callback?.Invoke();
        });

        fadeSequence.Play();
        yield return fadeSequence.WaitForCompletion();
    }
}