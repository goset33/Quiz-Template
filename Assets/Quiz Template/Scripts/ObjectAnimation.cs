using UnityEngine;
using DG.Tweening;

public class ObjectAnimation : MonoBehaviour
{
    enum AnimationType
    {
        None,
        Scale,
        Shake
    }

    [SerializeField] private AnimationType type;
    [SerializeField] private float amplitude = 1f;

    private Sequence sequence;
    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        sequence = DOTween.Sequence();
        if (type == AnimationType.Scale)
        {
            sequence.Append(rect.DOScale(amplitude, 0.7f)).Append(rect.DOScale(1f, 0.7f)).SetLoops(-1, LoopType.Restart);
        }
        else if (type == AnimationType.Shake)
        {
            sequence.Append(rect.DOShakePosition(1f, amplitude)).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
        }
        sequence.Play();
    }

    private void OnDisable()
    {
        DOTween.Kill(sequence, true);
        sequence = null;
    }
}
