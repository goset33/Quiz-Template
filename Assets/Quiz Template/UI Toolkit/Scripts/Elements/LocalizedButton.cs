using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

/// <summary>
/// Кастомный элемент Button с поддержкой локализации через Unity Localization.
/// Автоматически обновляет текст кнопки при изменении языка.
/// </summary>
[UxmlElement]
public partial class LocalizedButton : Button
{
    private LocalizedString localizedString;
    private string currentLocalizationKey;

    [UxmlAttribute("localization-key")]
    public string LocalizationKey
    {
        get => currentLocalizationKey;
        set
        {
            if (currentLocalizationKey != value)
            {
                currentLocalizationKey = value;
                UpdateLocalizedString();
            }
        }
    }

    [UxmlAttribute("table-name")]
    public string TableName { get; set; } = "Main Table";

    public LocalizedButton()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        UpdateText();
    }

    private void UpdateLocalizedString()
    {
        if (string.IsNullOrEmpty(currentLocalizationKey))
        {
            localizedString = null;
            text = string.Empty;
            return;
        }

        localizedString = new LocalizedString(TableName, currentLocalizationKey);
        UpdateText();
    }

    private async void UpdateText()
    {
        if (localizedString == null || string.IsNullOrEmpty(currentLocalizationKey))
        {
            return;
        }

        try
        {
            var operation = localizedString.GetLocalizedStringAsync();
            await operation.Task;
            
            if (operation.IsDone && operation.Result != null && 
                currentLocalizationKey == localizedString.TableEntryReference.Key)
            {
                text = operation.Result;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to get localized string for key '{currentLocalizationKey}': {ex.Message}");

            if (panel != null)
            {
                text = currentLocalizationKey;
            }
        }
    }
}

