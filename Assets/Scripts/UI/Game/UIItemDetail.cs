using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemDetail : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private RawImage _modelPreviewImage;
    [SerializeField] private Button _equipButton;

    [Header("3D Preview Setup")]
    [SerializeField] private Camera _previewCamera;          
    [SerializeField] private Transform _modelSpawnPoint;     
    [SerializeField] private float _rotationSpeed = 45f;     

    [Header("Preview Layer")]
    [SerializeField] private LayerMask _previewLayer; 

    private GameObject _detailPanel;
    private GameObject _currentModel;
    private Item _currentItem;
    private ILogService _logService;

    private void Awake()
    {
        _logService = AppContainer.Get<ILogService>();
        _detailPanel = this.gameObject;
        _detailPanel.SetActive(false);
        _equipButton.onClick.AddListener(OnEquipClicked);
    }

    private void Update()
    {
        if (_currentModel != null)
            _currentModel.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Muestra los detalles del item seleccionado en el panel derecho.
    /// </summary>
    public void ShowItem(Item item)
    {
        _currentItem = item;
        _detailPanel.SetActive(true);

        _nameText.text = item.Name;
        _descriptionText.text = item.Description;

        SpawnPreviewModel(item);
    }

    /// <summary>
    /// Oculta el panel derecho y limpia el modelo 3D.
    /// </summary>
    public void Hide()
    {
        _currentItem = null;
        _detailPanel.SetActive(false);
        DestroyCurrentModel();
    }

    private void SpawnPreviewModel(Item item)
    {
        DestroyCurrentModel();

        // Accedemos al prefab del modelo desde el ItemData a través del Item
        // Item expone su ModelPrefab a través de la propiedad (ver abajo)
        GameObject prefab = item.ModelPrefab;
        if (prefab == null)
        {
            _logService.Add<UIItemDetail>($"El item '{item.Name}' no tiene ModelPrefab asignado en su ItemData.");
            return;
        }

        _currentModel = Instantiate(prefab, _modelSpawnPoint.position, prefab.transform.rotation);

        // Mover el modelo al layer de preview para que solo lo vea la cámara de preview
        int layerIndex = (int)Mathf.Log(_previewLayer.value, 2);
        SetLayerRecursively(_currentModel, layerIndex);
    }

    private void DestroyCurrentModel()
    {
        if (_currentModel != null)
        {
            Destroy(_currentModel);
            _currentModel = null;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void OnEquipClicked()
    {
        _logService.Add<UIItemDetail>($"Pulsado boton equipar");
        if (_currentItem != null)
        {
            _currentItem.Equip();
            _logService.Add<UIItemDetail>($"Equipado: {_currentItem.Name}");
            // Opcional: cerrar el inventario o actualizar el grid tras equipar
            
        }
    }

    private void OnDestroy()
    {
        DestroyCurrentModel();
        _equipButton.onClick.RemoveListener(OnEquipClicked);
    }
}
