using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

/// <summary>
/// Кастомный элемент Image с поддержкой локализации спрайтов через Unity Localization.
/// Автоматически обновляет спрайт при изменении языка.
/// </summary>
[UxmlElement]
public partial class LocalizedImage : Image
{
    private LocalizedAsset<Sprite> localizedSprite;
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
                UpdateLocalizedSprite();
            }
        }
    }

    [UxmlAttribute("table-name")]
    public string TableName { get; set; } = "Asset Table";

    public LocalizedImage()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        UpdateSprite();
    }

    private void UpdateLocalizedSprite()
    {
        if (localizedSprite != null)
        {
            localizedSprite.AssetChanged -= OnSpriteChanged;
            localizedSprite = null;
        }

        if (string.IsNullOrEmpty(currentLocalizationKey))
        {
            sprite = null;
            return;
        }

        localizedSprite = new LocalizedAsset<Sprite>();
        localizedSprite.SetReference(TableName, currentLocalizationKey);
        localizedSprite.AssetChanged += OnSpriteChanged;
        UpdateSprite();
    }

    private void OnSpriteChanged(Sprite newSprite)
    {
        if (panel != null && currentLocalizationKey == localizedSprite?.TableEntryReference.Key)
        {
            sprite = newSprite;
        }
    }

    private async void UpdateSprite()
    {
        if (localizedSprite == null || string.IsNullOrEmpty(currentLocalizationKey))
        {
            return;
        }

        try
        {
            var operation = localizedSprite.LoadAssetAsync();
            await operation.Task;

            if (operation.IsDone && operation.Result != null && 
                currentLocalizationKey == localizedSprite.TableEntryReference.Key)
            {
                sprite = operation.Result;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to get localized sprite for key '{currentLocalizationKey}': {ex.Message}");
            if (panel != null)
            {
                sprite = null;
            }
        }
    }
}

