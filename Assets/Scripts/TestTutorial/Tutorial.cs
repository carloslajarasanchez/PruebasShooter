using UnityEngine;

public class Tutorial : MonoBehaviour
{
    IEventService events;
    IGameState state;

    [SerializeField] private GameObject door;

    void Start()
    {
        state = AppContainer.Get<IGameState>();
        events = AppContainer.Get<IEventService>();

        state.SetFlag("tutorial_started", true);

        ShowMoveInstruction();

        events.Subscribe<OnFlagChangedEvent>(OnFlagChanged);
    }

    void OnDestroy()
    {
        events.Unsubscribe<OnFlagChangedEvent>(OnFlagChanged);
    }

    void OnFlagChanged(OwnEventBase e)
    {
        if (e is OnFlagChangedEvent evt)
        {
            if (evt.Key == "tutorial_weaponpicked" && evt.Value)
            {
                CompleteTutorial();
            }
        }
    }

    void ShowMoveInstruction()
    {
        Debug.Log("Usa WASD para moverte");
    }

    void CompleteTutorial()
    {
        door.GetComponent<DoorController>().MoveObject();

        state.SetFlag("tutorial_completed", true);
        state.SetFlag("tutorialdoor_opened", true);

        Debug.Log("Tutorial completado");
    }
}