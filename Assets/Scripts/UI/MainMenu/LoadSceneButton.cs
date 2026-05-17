using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadSceneButton : MonoBehaviour
{
    [SerializeField] private string _sceneName;

    private ISceneService _sceneService;

    private void Awake()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        _sceneService = AppContainer.Get<ISceneService>();
        GetComponent<Button>().onClick.AddListener(() => _sceneService.LoadScene(_sceneName));
        // _sceneService.LoadScene(_sceneName));
    }
}