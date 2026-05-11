using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente para cambiar el idioma del juego al hacer clic en un botón.
/// </summary>
[RequireComponent(typeof(Button))] // Debe estar en el mismo GameObject que un componente Button de Unity UI.
public class ChangeLanguage : MonoBehaviour
{
    private Button _button;

    [Tooltip("Nombre del archivo JSON en Resources/i18n/ (ej: 'en', 'es')")]
    [SerializeField]
    private string _language;

    
    private ITranslationService _translationService; //es el servicio de traducciones para cambiar el idioma en el sistema global.

    private void Awake()
    {
        _translationService = AppContainer.Get<ITranslationService>(); // Obtenemos el servicio de traducciones del contenedor de servicios.

        // Obtenemos la referencia al componente botón
        this._button = GetComponent<Button>();

        // Suscribimos la acción al evento onClick. 
        // Se hace por código para que el botón funcione "solo" al arrastrar el script.
        if (this._button != null)
        {
            this._button.onClick.AddListener(MakeChange);
        }


    }

    /// <summary>
    /// Invoca el cambio de idioma en el sistema global.
    /// </summary>
    private void MakeChange()
    {
       

        if (string.IsNullOrEmpty(this._language))
        {
            Debug.LogWarning($"[ChangeLanguage] El campo '_language' en {gameObject.name} está vacío.");
            return;
        }       

        //tenemos que cambiar el idioma a través del servicio de eventos para que otros componentes que lo necesiten puedan reaccionar al cambio.
        _translationService.ChangeLanguage(this._language);
    }
}
