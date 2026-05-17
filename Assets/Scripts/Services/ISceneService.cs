
//es el servicio encargado de cargar las escenas del juego, como el menú principal, el nivel de juego, etc.
public interface ISceneService
{
    void LoadScene(string sceneName);
    void LoadNextScene();
    void LoadPreviousScene();
    void QuitGame();
}
