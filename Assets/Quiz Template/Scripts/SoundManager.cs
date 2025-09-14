using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource vfxSource, clickSource, musicSource;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioClip[] buttonSounds, endJingles;

    public static event Action<int> JingleEnded; // Передает 0 если первая стадия и 1 если вторая стадия

    void Awake()
    {
        Instance = this;
        StartCoroutine(PitchChanger());

        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => PlayButtonSound(0));
        }
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
    public void ChangeMusicState()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        } else
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
            button.onClick.AddListener(() => PlayButtonSound(indexRights));
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