using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizeLanguage : MonoBehaviour
{
    private void Start()
    {
        SetEnglish();
    }

    public void SetNewLocale(string language)
    {
        LocalizationSettings.SelectedLocale = language switch
        {
            "en" => LocalizationSettings.AvailableLocales.Locales[0],
            "hi" => LocalizationSettings.AvailableLocales.Locales[1],
            _ => LocalizationSettings.AvailableLocales.Locales[0],
        };
    }

    public void SetEnglish()
    {
        SetNewLocale("en");
    }

    public void SetHindi()
    {
        SetNewLocale("hi");
    }
}
