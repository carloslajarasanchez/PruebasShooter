using System.Collections;
using UnityEngine;

public class SplashController : MonoBehaviour
{
    [SerializeField] private float _duration = 3f;

    private ISceneService _sceneService;

    private void Awake()
    {
        _sceneService = AppContainer.Get<ISceneService>();
    }

    private void Start()
    {
        StartCoroutine(AutoAdvance());
    }

    private IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(_duration);
        _sceneService.LoadNextScene();
    }
}