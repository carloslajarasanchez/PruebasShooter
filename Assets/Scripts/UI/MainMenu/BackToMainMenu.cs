using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackToMainMenu : MonoBehaviour
{
    private ISceneService _sceneService;

    private void Awake()
    {
        _sceneService = AppContainer.Get<ISceneService>();
        GetComponent<Button>().onClick.AddListener(_sceneService.LoadPreviousScene);
    }
}