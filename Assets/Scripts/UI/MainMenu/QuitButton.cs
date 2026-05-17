using UnityEngine;
using UnityEngine.UI;

//Se encarga de cerrar el juego, se le asigna a un botón en el menú principal

[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour
{
    private ISceneService _sceneService;

    private void Awake()
    {
        _sceneService = AppContainer.Get<ISceneService>();
        GetComponent<Button>().onClick.AddListener(_sceneService.QuitGame);
    }
}
