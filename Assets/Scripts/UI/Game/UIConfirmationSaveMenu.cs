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
    private SaveMachine _currentMachine;

    private void Awake()
    {
        _inventoryService = AppContainer.Get<IInventoryService>();
        _saveService = AppContainer.Get<ISaveService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
        _panel.SetActive(false);
        _yesButton.onClick.AddListener(OnYesPressed);
        _noButton.onClick.AddListener(OnNoPressed);
    }

    public void Show(SaveMachine machine)
    {
        _currentMachine = machine;

        int tapeCount = 0;
        foreach (var item in _inventoryService.Items)
        {
            if (item is SaveTape) tapeCount++;
        }

        if (tapeCount == 0)
        {
            _confirmationText.text = "No tienes cintas para guardar";
            _yesButton.gameObject.SetActive(false);
        }
        else
        {
            _confirmationText.text = $"¿Guardar partida?\nCintas disponibles: {tapeCount}\n(Se consumirá una cinta)";
            _yesButton.gameObject.SetActive(true);
        }

        _panel.SetActive(true);
        _playerInput.SwitchControlMap(ControlMap.UI);
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
        _playerInput.SwitchControlMap(ControlMap.Player);
    }
}