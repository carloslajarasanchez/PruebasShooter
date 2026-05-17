using TMPro;
using UnityEngine;

/// <summary>
/// Componente para textos de TextMeshPro que se actualizan automáticamente
/// cuando cambia el idioma del sistema.
/// </summary>

public class TranslatableItemTextMesh : MonoBehaviour
{
    private TextMeshPro _text;

    [Tooltip("Clave técnica que coincide con el JSON (ej: '_title')")]
    [SerializeField] private string _key;

    private IEventService _eventService; // Servicio de eventos para suscribirse a cambios de idioma.


    private void Awake()
    {
        _eventService = AppContainer.Get<IEventService>(); // Obtenemos el servicio de eventos del contenedor de servicios.

        this._text = GetComponent<TextMeshPro>();

    }

    private void Start()
    {
        // Forzamos la primera actualización al inicio para mostrar el idioma actual.
        this.UpdateText(null);
    }


    private void OnEnable()
    {
        // Nos suscribimos al evento global de cambio de idioma para actualizar el texto automáticamente.
        // le tenemos que pasar la función UpdateText para que se ejecute cada vez que se dispare el evento.
        _eventService.Subscribe<OnLanguageChanged>(UpdateText);
    }

    private void OnDisable()
    {
        // Nos desuscribimos del evento global de cambio de idioma para evitar actualizaciones innecesarias cuando el objeto no está activo.
        _eventService.Unsubscribe<OnLanguageChanged>(UpdateText);
    }


    /// <summary>
    /// Consulta el diccionario global y actualiza el componente visual.
    /// </summary>
    private void UpdateText(OwnEventBase parameters)
    {
        if (this._text == null)
            return;

        // Obtenemos la traducción. Si la clave no existe
        //OnLanguageChanged eventLanguage =(OnLanguageChanged)parameters;

        // esto es para que se actualice el texto cada vez que se dispare el evento, aunque no se use el parámetro del evento.
        this._text.text = AppContainer.Get<ITranslationService>().Get(this._key);
    }


}
