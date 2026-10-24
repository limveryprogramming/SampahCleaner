using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHud : MonoBehaviour
{
    static GameHud instance;
    string message;
    float messageUntil;
    float cameraFlashUntil;
    PlayerController player;
    GUIStyle label, big, center;

    void Awake() => instance = this;

    public static void Flash(string msg)
    {
        if (instance == null) return;
        instance.message = msg;
        instance.messageUntil = Time.unscaledTime + 3f;
    }

    public static void FlashCamera()
    {
        if (instance != null) instance.cameraFlashUntil = Time.unscaledTime + 0.12f;
    }

    void InitStyles()
    {
        if (label != null) return;
        label = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        label.normal.textColor = Color.white;
        big = new GUIStyle(label) { fontSize = 34, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        center = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
    }

    void OnGUI()
    {
        InitStyles();
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        float w = Screen.width, h = Screen.height;

        GUI.Box(new Rect(10, 10, 330, 150), GUIContent.none);
        GUI.Label(new Rect(20, 15, 320, 30), $"Dead animals: {GameState.DeadAnimals}/{GameState.DeadAnimalLimit}", label);
        GUI.Label(new Rect(20, 45, 320, 30), $"False reports: {GameState.FalseReports}/{GameState.FalseReportLimit}", label);
        GUI.Label(new Rect(20, 75, 320, 30), $"Trash collected: {GameState.TrashCollected}", label);
        GUI.Label(new Rect(20, 105, 320, 30), $"Litterbugs reported: {GameState.LitterbugsReported}", label);
        GUI.Label(new Rect(20, 130, 320, 30), $"Trash on map: {Trash.All.Count}", label);

        bool camMode = player != null && player.InCameraMode;
        string hint = camMode
            ? "CAMERA MODE - Mouse: look | Space/Click: photo | F: exit"
            : "WASD: move | E: collect trash | F: camera mode | R: shop";
        GUI.Label(new Rect(10, h - 40, w, 30), hint, label);

        if (camMode)
        {
            GUI.Box(new Rect(w / 2 - 2, h / 2 - 20, 4, 40), GUIContent.none);
            GUI.Box(new Rect(w / 2 - 20, h / 2 - 2, 40, 4), GUIContent.none);
        }

        if (player != null && player.ShopOpen)
        {
            GUI.Box(new Rect(w / 2 - 200, h / 2 - 100, 400, 200), GUIContent.none);
            GUI.Label(new Rect(w / 2 - 200, h / 2 - 90, 400, 40), "SHOP", big);
            GUI.Label(new Rect(w / 2 - 190, h / 2 - 30, 380, 80), "Nothing for sale yet.\nPress R to close.", center);
        }

        if (Time.unscaledTime < messageUntil)
            GUI.Label(new Rect(0, 170, w, 40), message, center);

        if (Time.unscaledTime < cameraFlashUntil)
        {
            var old = GUI.color;
            GUI.color = new Color(1, 1, 1, 0.8f);
            GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
            GUI.color = old;
        }

        if (GameState.Ending != EndingType.None) DrawEnding(w, h);
    }

    void DrawEnding(float w, float h)
    {
        var old = GUI.color;
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = old;

        GUI.Label(new Rect(w / 2 - 450, h / 2 - 180, 900, 60), GameState.EndingTitle(GameState.Ending), big);
        GUI.Label(new Rect(w / 2 - 400, h / 2 - 100, 800, 120), "\"" + GameState.EndingText(GameState.Ending) + "\"", center);

        if (GUI.Button(new Rect(w / 2 - 160, h / 2 + 50, 150, 50), "Restart"))
        {
            GameState.Reset();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (GUI.Button(new Rect(w / 2 + 10, h / 2 + 50, 150, 50), "Main Menu"))
        {
            GameState.Reset();
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
