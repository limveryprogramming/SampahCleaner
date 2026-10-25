using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] string gameScene = "SampleScene";

    public void Play() => SceneManager.LoadScene(gameScene);

    public void UnlimitedMode() => Debug.Log("Unlimited Mode: not built yet.");

    public void Upgrades() => Debug.Log("Upgrades: not built yet.");

    public void Settings() => Debug.Log("Settings: not built yet.");

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
