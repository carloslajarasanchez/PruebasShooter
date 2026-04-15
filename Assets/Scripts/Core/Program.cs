using UnityEngine;
using UnityEngine.Playables;

public static class Program
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Main()
    {
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
    }
}
