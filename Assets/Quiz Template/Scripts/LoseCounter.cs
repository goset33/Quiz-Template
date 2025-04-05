using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Класс отвечает за круговой таймер, который показывается игроку при проигрыше
/// Объект со скриптом самоуничтожается по истечении таймера и вызывает функцию TimelessController.ButtonPressed()
/// </summary>
public class LoseCounter : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI counter;

    private float timer = 5f;

    private static event Action<int> OnTimeEnd;

    private void Start()
    {
        OnTimeEnd += transform.parent.parent.parent.GetComponent<TimelessController>().ButtonPressed;
    }

    private void OnDisable()
    {
        OnTimeEnd -= transform.parent.parent.parent.GetComponent<TimelessController>().ButtonPressed;
        Destroy(gameObject);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        counter.text = Mathf.RoundToInt(timer).ToString();
        if (timer <= 0)
        {
            OnTimeEnd.Invoke(0);
            slider.value = 0f;
            return;
        }

        slider.value = timer / 5f;        
    }
}
