using UnityEngine;

public static class Program
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Main()
    {
        AppContainer.Register<ISceneService>(() => new SceneService());
        // LogService se encarga de gestionar todos los logs de la aplicación
        AppContainer.Register<ILogService>(() => new LogService());

        // ConfigurationService se encarga de gestionar las settings de configuración (key => value)
        AppContainer.Register<IConfigurationService>(() => new ConfigurationService());

        AppContainer.Register<IEventService>(() => new EventService());

        // TranslationService se encarga de leer y servir las traducciones
        AppContainer.Register<ITranslationService>(() => new JsonTranslationService());

        AppContainer.Register<IPlayerInput>(() => new PlayerInputManager());

        AppContainer.Register<IInventoryService>(() => new InventoryService());

        AppContainer.Register<IPlayer>(() => new Player());

        AppContainer.Register<IEquipService>(() => new EquipService());

        AppContainer.Register<IZoneService>(() => new ZoneService());

        AppContainer.Register<IPoolService>(() => new PoolService());

        AppContainer.Register<IGameState>(() => new GameState());

        AppContainer.Register<ISaveService>(() => new SaveService());

        AppContainer.Register<IAlertService>(() => new AlertService());

        AppContainer.Register<IPauseService>(() => new PauseService());

        var library = Resources.Load<SoundLibrary>("SoundLibrary");
        library.Initialize();
        AppContainer.Register<ISoundLibrary>(() => library);

        AppContainer.Register<IAudioService>(() =>
        {
            var go = new GameObject("AudioService");
            Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<AudioService>();
            service.Initialize(library,go);
            return service;
        });
    }
}
