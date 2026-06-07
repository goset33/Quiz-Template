using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using YG;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private static Dictionary<Button, List<Action>> subscribes = new Dictionary<Button, List<Action>>();

    [SerializeField] private AudioSource vfxSource, clickSource, musicSource;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioClip[] buttonSounds, endJingles;

    public static event Action<int> JingleEnded; // Передает 0 если первая стадия и 1 если вторая стадия

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        StartCoroutine(PitchChanger());

        VisualElement root = FindFirstObjectByType<UIDocument>().rootVisualElement;

        List<Button> buttons = root.parent.Query<Button>().ToList();
        foreach (Button button in buttons)
        {
            if (button.ClassListContains("variant-button")) continue;

            Action action = () => PlayButtonSound(0);
            button.clicked += action;
            if (!subscribes.ContainsKey(button))
                subscribes[button] = new List<Action>();

            subscribes[button].Add(action);
        }

        YG2.saves.musicVolume.AsObservable().Subscribe(value => SetMusicVolume(value / 100f)).AddTo(GameManager.disposables);
        YG2.saves.vfxVolume.AsObservable().Subscribe(value => SetVfxVolume(value / 100f)).AddTo(GameManager.disposables);
        musicSource.Play();
    }

    /// <summary>
    /// Устанавливает громкость музыки
    /// </summary>
    /// <param name="volume">Значение громкости музыки в диапазоне 0-1</param>
    public void SetMusicVolume(float volume)
    {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat("MusicVolume", dB);
    }

    /// <summary>
    /// Устанавливает громкость спецэффектов
    /// </summary>
    /// <param name="volume">Значение громкости спецэффектов в диапазоне 0-1</param>
    public void SetVfxVolume(float volume)
    {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat("VFXVolume", dB);
    }
    
    /// <summary>
    /// Ставит и снимает музыку с паузы
    /// </summary>
    public void ChangeMusicState(bool? newState = null)
    {
        if (newState == null)
        {
            if (musicSource.isPlaying)
            {
                musicSource.Pause();
            }
            else
            {
                musicSource.UnPause();
            }
        }
        else if (newState == false)
        {
            musicSource.Pause();
        }
        else
        {
            musicSource.UnPause();
        }
    }

    /// <summary>
    /// Запускает корутину проигрывания финальных джинглов
    /// </summary>
    /// <param name="isGood">Победил ли игрок</param>
    /// <param name="isFull">Нужно ли проигрывать вторую часть джингла</param>
    public void PlayJingle(bool isGood, bool isFull)
    {
        StartCoroutine(JingleSequence(isGood, isFull));
    }

    private IEnumerator JingleSequence(bool isGood, bool isFull)
    {
        int index = isGood ? 0 : 1;
        vfxSource.PlayOneShot(endJingles[index], 1f);

        yield return new WaitForSeconds(endJingles[index].length);
        JingleEnded?.Invoke(0);

        if (isFull)
        {
            vfxSource.PlayOneShot(endJingles[2], 1f);

            yield return new WaitForSeconds(endJingles[2].length);
            JingleEnded?.Invoke(1);
        }
    }

    private void PlayButtonSound(int index)
    {
        if (index == 0)
        {
            clickSource.PlayOneShot(buttonSounds[index], 1f);
            return;
        }

        vfxSource.PlayOneShot(buttonSounds[index], 1f);
    }

    /// <summary>
    /// Метод подписывает вопспроизведение звука на ивент нажатия кнопки
    /// </summary>
    /// <param name="button">Кнопка</param>
    /// <param name="indexRights">0 - классический звук нажатия. 1 - звук правильного ответа. 2 - звук неправильного ответа.</param>
    public void AddUniqueSoundToButton(Button button, int indexRights)
    {
        if (indexRights < 0 || indexRights > 3)
        {
            Debug.LogError("Индекс не 0-3");
            return;
        }

        if (button != null)
        {
            Action action = () => PlayButtonSound(indexRights);
            button.clicked += action;
            if (!subscribes.ContainsKey(button))
                subscribes[button] = new List<Action>();

            subscribes[button].Add(action);
        }
    }

    public void UnsubscribeSoundFromButton(Button button)
    {
        if (button == null) return;

        if (subscribes.TryGetValue(button, out var actions))
        {
            foreach (var act in actions)
            {
                button.clicked -= act;
            }
            subscribes.Remove(button);
        }
    }

    /// <summary>
    /// Раз в секунду меняет pitch звука нажатия кнопок
    /// </summary>
    IEnumerator PitchChanger()
    {
        YieldInstruction waiter = new WaitForSeconds(1f);
        while (true)
        {
            clickSource.pitch = Random.Range(0.7f, 1.3f);
            yield return waiter;
        }
    }
}