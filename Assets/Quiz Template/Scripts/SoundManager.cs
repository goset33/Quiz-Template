using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource vfxSource, clickSource;
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

    public void SetMusicVolume(float volume)
    {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat("MusicVolume", dB);
    }

    public void SetVfxVolume(float volume)
    {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat("VFXVolume", dB);
    }

    public void PlayJingle(bool isGood)
    {
        StartCoroutine(JingleSequence(isGood));
    }

    private IEnumerator JingleSequence(bool isGood)
    {
        int index = isGood ? 0 : 1;
        vfxSource.PlayOneShot(endJingles[index], 1f);

        yield return new WaitForSeconds(endJingles[index].length);
        JingleEnded?.Invoke(0);

        vfxSource.PlayOneShot(endJingles[2], 1f);

        yield return new WaitForSeconds(endJingles[2].length);
        JingleEnded?.Invoke(1);
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