using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Collections;
using TMPro;


public class LocalizationLenguageSelect : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private bool isChangingLanguage = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.value = LocalizationSettings.SelectedLocale == null ? 0 : LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        //ChangeLanguage();

    }

    public void ChangeLanguage()
    {
        if (isChangingLanguage) return;

        StartCoroutine(SetLocale(dropdown.value));
    }

    private IEnumerator SetLocale(int id)
    {
        //Debug.Log($"Changing language to: {LocalizationSettings.AvailableLocales.Locales[id].name}");
        isChangingLanguage = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[id];
        isChangingLanguage = false;
    }
}
