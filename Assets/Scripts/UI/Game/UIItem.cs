using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Image _icon;

    public void SetItem(Item item)
    {
        _nameText.text = item.Name;
       // _descriptionText.text = item.Description;
        _icon.sprite = item.Icon;
    }

    private void SelectedItem()
    {
        // TODO: Implement item selection logic, such as equipping the item or using it and updating the UI accordingly
    }
}
