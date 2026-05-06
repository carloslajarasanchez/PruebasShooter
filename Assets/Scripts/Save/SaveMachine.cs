using UnityEngine;

public class SaveMachine : MonoBehaviour
{
    [SerializeField] private UIConfirmationSaveMenu _confirmationMenu;

    public void OpenMenu()
    {
        _confirmationMenu.Show(this);
    }
}
