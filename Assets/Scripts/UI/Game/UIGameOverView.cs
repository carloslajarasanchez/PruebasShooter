using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIGameOverView : MonoBehaviour
{
    [SerializeField] private GameObject _panelContainer;
    [SerializeField] private float _deathAnimationDuration = 1.5f;
    [SerializeField] private float _panelDelay = 2.5f;

    private IEventService _eventService;
    private IPlayerInput _playerInput;
    private IPauseService _pauseService;

    private void Awake()
    {
        _eventService = AppContainer.Get<IEventService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
        _pauseService = AppContainer.Get<IPauseService>();
        _panelContainer.SetActive(false);
    }

    private void OnEnable() => _eventService.Subscribe<OnGameOver>(OnGameOver);
    private void OnDisable() => _eventService.Unsubscribe<OnGameOver>(OnGameOver);

    private void OnGameOver(OwnEventBase e)
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        _playerInput.SwitchControlMap(ControlMap.UI);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float elapsed = 0f;
            Quaternion startRot = player.transform.rotation;
            Quaternion endRot = startRot * Quaternion.Euler(90f, 0f, 0f);

            while (elapsed < _deathAnimationDuration)
            {
                player.transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / _deathAnimationDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            player.transform.rotation = endRot;
        }

        yield return new WaitForSeconds(_panelDelay - _deathAnimationDuration);

        _panelContainer.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _pauseService.IsPauseBlocked = true;
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        _pauseService.IsPauseBlocked = false;
        _eventService.Clear();
        AppContainer.Get<IEquipService>().Clear();
        AppContainer.Get<IInventoryService>().Clear();
        AppContainer.Get<IGameState>().Clear();
        AppContainer.Get<IZoneService>().Clear();
        AppContainer.Get<ISaveService>().ClearStates();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitClicked() => Application.Quit();
}
