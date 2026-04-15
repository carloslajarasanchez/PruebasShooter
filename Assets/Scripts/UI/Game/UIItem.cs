using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _selectionHighlight;

    private Button _button;
    private Item _item;
    private UIInventory _uiInventory;

    public void SetItem(Item item, UIInventory uiInventory)
    {
        _item = item;
        _uiInventory = uiInventory;
        _nameText.text = item.Name;
        _icon.sprite = item.Icon;
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        SetSelected(false);
    }

    private void OnClick()
    {
        _uiInventory.SelectItem(_item);
    }

    public void SetSelected(bool selected)
    {
        if(_selectionHighlight != null)
        {
            _selectionHighlight.enabled = selected;
        }
    }
}
