using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioClip[] sounds;

    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
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

    private void PlayButtonSound(int index)
    {
        audioSource.PlayOneShot(sounds[index], 1f);
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
            audioSource.pitch = Random.Range(0.7f, 1.3f);
            yield return waiter;
        }
    }
}