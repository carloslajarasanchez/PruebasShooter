using TMPro;
using UnityEngine;

/// <summary>
/// Componente para textos de TextMeshPro que se actualizan automáticamente
/// cuando cambia el idioma del sistema.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TranslatableItem : MonoBehaviour
{
    [Tooltip("Clave técnica que coincide con el JSON (ej: '_title')")]
    [SerializeField] private string _key;
    
    public string Key { get => _key; set => _key = value; }

    private TextMeshProUGUI _text;
    private IEventService _eventService; // Servicio de eventos para suscribirse a cambios de idioma.

    private void Awake()
    {
        _eventService = AppContainer.Get<IEventService>(); 
        this._text = GetComponent<TextMeshProUGUI>();  
    }

    private void Start()
    {
        this.UpdateText(null);
    }

    private void OnEnable()
    {
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

        this._text.text = AppContainer.Get<ITranslationService>().Get(this._key);
    } 
}
