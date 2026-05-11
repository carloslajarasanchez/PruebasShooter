using TMPro;
using UnityEngine;

public class UIAlertView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private GameObject _panelContainer;

    private IEventService _eventService;
    private ILogService _logService;
    private JsonTranslationService _translationService;
    private TranslatableItem _translatableTitle;
    private TranslatableItem _translatableDescription;

    private void Awake()
    {
        _eventService = AppContainer.Get<IEventService>();
        _logService = AppContainer.Get<ILogService>();
        _translationService = AppContainer.Get<JsonTranslationService>();
        _translatableTitle = _titleText.GetComponent<TranslatableItem>();
        _translatableDescription = _descriptionText.GetComponent<TranslatableItem>();
        _panelContainer.SetActive(false);
    }

    private void OnEnable()
    {
        _eventService.Subscribe<OnAlertMessageReceived>(OnAlertReceived);
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe<OnAlertMessageReceived>(OnAlertReceived);
    }

    private void OnAlertReceived(OwnEventBase parameters)
    {
        if (parameters is OnAlertMessageReceived alertMessage)
        {
            // Si recibimos un mensaje vacío, cerramos el panel
            if (string.IsNullOrEmpty(alertMessage.Description))
            {
                ClosePanel();
                return;
            }

            _panelContainer.SetActive(true);
            _logService.Add<UIAlertView>($"Recibido mensaje de alerta: {alertMessage.Description}");

            //_descriptionText.text = alertMessage.Description;
            _descriptionText.text = _translationService.Get(alertMessage.Description);
            if (alertMessage.ShowTitle)
            {
                _titleText.gameObject.SetActive(true);
                //_titleText.text = alertMessage.Title;
                _titleText.text = _translationService.Get(alertMessage.Title);
            }
            else
            {
                _titleText.gameObject.SetActive(false);
            }
        }
    }

    public void ClosePanel()
    {
        _panelContainer.SetActive(false);
    }
}
