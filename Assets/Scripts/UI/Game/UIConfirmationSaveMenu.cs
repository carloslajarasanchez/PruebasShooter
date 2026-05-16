using UnityEngine;
using UnityEngine.UI;

public class UIConfirmationSaveMenu : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMPro.TextMeshProUGUI _confirmationText;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    private IInventoryService _inventoryService;
    private ISaveService _saveService;
    private IPlayerInput _playerInput;
    private IPauseService _pauseService;
    private ITranslationService _translationService;
    private IEventService _eventService;
    private SaveMachine _currentMachine;
    private int _cachedTapeCount;

    private void Awake()
    {
        _inventoryService = AppContainer.Get<IInventoryService>();
        _saveService = AppContainer.Get<ISaveService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
        _pauseService = AppContainer.Get<IPauseService>();
        _translationService = AppContainer.Get<ITranslationService>();
        _eventService = AppContainer.Get<IEventService>();
        _panel.SetActive(false);
        _yesButton.onClick.AddListener(OnYesPressed);
        _noButton.onClick.AddListener(OnNoPressed);
    }

    private void OnEnable()
    {
        _eventService.Subscribe<OnLanguageChanged>(OnLanguageChanged);
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe<OnLanguageChanged>(OnLanguageChanged);
    }

    public void Show(SaveMachine machine)
    {
        _currentMachine = machine;

        int tapeCount = 0;
        foreach (var item in _inventoryService.Items)
        {
            if (item is SaveTape) tapeCount++;
        }

        _cachedTapeCount = tapeCount;

        if (tapeCount == 0)
        {
            _confirmationText.text = _translationService.Get("_noTapes");
            _yesButton.gameObject.SetActive(false);
        }
        else
        {
            _confirmationText.text = string.Format(_translationService.Get("_saveConfirm"), tapeCount);
            _yesButton.gameObject.SetActive(true);
        }

        _panel.SetActive(true);
        _pauseService.IsPauseBlocked = true;
        _playerInput.SwitchControlMap(ControlMap.UI);
    }

    private void OnLanguageChanged(OwnEventBase e)
    {
        if (!_panel.activeSelf) return;

        if (_cachedTapeCount == 0)
        {
            _confirmationText.text = _translationService.Get("_noTapes");
        }
        else
        {
            _confirmationText.text = string.Format(_translationService.Get("_saveConfirm"), _cachedTapeCount);
        }
    }

    public void OnYesPressed()
    {
        SaveTape tape = _inventoryService.GetItem<SaveTape>(t => true);
        if (tape != null)
        {
            tape.Consume();
            _saveService.Save();
        }
        Hide();
    }

    public void OnNoPressed()
    {
        Hide();
    }

    private void Hide()
    {
        _panel.SetActive(false);
        _currentMachine = null;
        _pauseService.IsPauseBlocked = false;
        _playerInput.SwitchControlMap(ControlMap.Player);
    }
}