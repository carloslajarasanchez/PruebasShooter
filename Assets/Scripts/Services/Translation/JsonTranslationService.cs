using System;
using System.Collections.Generic;
using UnityEngine;

public class JsonTranslationService : ITranslationService
{
    private Dictionary<string, string> translationsDictionary = new Dictionary<string, string>();

    private readonly IConfigurationService _configurationService;
    private readonly ILogService _logService;
    private readonly IEventService _eventService;

    public JsonTranslationService()
    {
        this._configurationService = AppContainer.Get<IConfigurationService>();
        this._logService = AppContainer.Get<ILogService>();
        this._eventService = AppContainer.Get<IEventService>();
        // Cargar el idioma por defecto al iniciar el servicio
        this.ChangeLanguage(this._configurationService.Get("language", "es"));
    }
    public void ChangeLanguage(string language)
    {
        // Cargamos el idioma
        try
        {
            TextAsset contentJson = Resources.Load<TextAsset>($"i18n/{language}");
            TranslationsDTO translationsDTO = JsonUtility.FromJson<TranslationsDTO>(contentJson.text);

            this.translationsDictionary = this.ConvertToDictionary(translationsDTO.translations);

            this._logService.Add<JsonTranslationService>($"Cambiamos el language a `{language}`");

            //Invocamos evento de cambio de lenguage
            OnLanguageChanged onLanguageChanged = new OnLanguageChanged() { Language = language };
            _eventService.Publish(onLanguageChanged);
        }
        catch (Exception ex)
        {
            this._logService.Add<JsonTranslationService>($"Error al cargar el resource {language} en `/i18n`");
            this._logService.Add<JsonTranslationService>(ex.Message + ex.InnerException + ex.StackTrace);
        }
    }

    private Dictionary<string, string> ConvertToDictionary(List<TranslateItem> translations)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        foreach (var translation in translations)
        {
            if (!result.TryAdd(translation.key, translation.value))
            {
                this._logService.Add<JsonTranslationService>($"`{translation.key}` está duplicada");
            }
        }

        return result;
    }

    public string Get(string key)
    {
        if (!this.translationsDictionary.TryGetValue(key, out string value))
            return key;

        return value;
    }
}
