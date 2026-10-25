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

        GUI.Box(new Rect(10, 10, 330, 180), GUIContent.none);
        GUI.Label(new Rect(20, 155, 320, 30), $"Coins: {GameState.Coins}", label);
        GUI.Label(new Rect(20, 15, 320, 30), $"Dead animals: {GameState.DeadAnimals}/{GameState.DeadAnimalLimit}", label);
        GUI.Label(new Rect(20, 45, 320, 30), $"False reports: {GameState.FalseReports}/{GameState.FalseReportLimit}", label);
        GUI.Label(new Rect(20, 75, 320, 30), $"Trash collected: {GameState.TrashCollected}", label);
        GUI.Label(new Rect(20, 105, 320, 30), $"Litterbugs reported: {GameState.LitterbugsReported}", label);
        GUI.Label(new Rect(20, 130, 320, 30), $"Trash on map: {Trash.All.Count}", label);

        bool camMode = player != null && player.InCameraMode;
        string hint = camMode
            ? "CAMERA MODE - Mouse: look | Left click: photo | F: exit"
            : "Mouse: look | WASD: move | Walk into trash to collect | F: camera mode | R: shop";
        GUI.Label(new Rect(10, h - 40, w, 30), hint, label);

        if (camMode)
        {
            GUI.Box(new Rect(w / 2 - 2, h / 2 - 20, 4, 40), GUIContent.none);
            GUI.Box(new Rect(w / 2 - 20, h / 2 - 2, 40, 4), GUIContent.none);
        }

        if (Time.unscaledTime < messageUntil)
            GUI.Label(new Rect(0, 200, w, 40), message, center);

        if (Time.unscaledTime < cameraFlashUntil)
        {
            var old = GUI.color;
            GUI.color = new Color(1, 1, 1, 0.8f);
            GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
            GUI.color = old;
        }

        if (GameState.Ending != EndingType.None) DrawEnding(w, h);
    }

    const int EndingSlideCount = 3;
    EndingType slidesFor;
    int slideIndex;
    Texture2D[] slides;

    void LoadSlides()
    {
        slidesFor = GameState.Ending;
        slideIndex = 0;
        slides = new Texture2D[EndingSlideCount];
        int n = (int)GameState.Ending;
        for (int i = 0; i < EndingSlideCount; i++)
            slides[i] = Resources.Load<Texture2D>($"Endings/ending_{n}_ending_{n}_slide_{i + 1}");
    }

    void DrawEnding(float w, float h)
    {
        if (slides == null || slidesFor != GameState.Ending) LoadSlides();

        var old = GUI.color;
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = old;

        var tex = slides[slideIndex];
        if (tex != null)
        {
            float scale = Mathf.Min(w / tex.width, h / tex.height);
            float tw = tex.width * scale, th = tex.height * scale;
            GUI.DrawTexture(new Rect((w - tw) / 2f, (h - th) / 2f, tw, th), tex, ScaleMode.ScaleToFit);
        }
        else
        {
            GUI.Label(new Rect(w / 2 - 450, h / 2 - 180, 900, 60), GameState.EndingTitle(GameState.Ending), big);
            GUI.Label(new Rect(w / 2 - 400, h / 2 - 100, 800, 120), "\"" + GameState.EndingText(GameState.Ending) + "\"", center);
        }

        float by = h - 80;
        bool last = slideIndex >= EndingSlideCount - 1;
        if (!last)
        {
            if (GUI.Button(new Rect(w / 2 - 75, by, 150, 50), "Next")) slideIndex++;
        }
        else
        {
            if (GUI.Button(new Rect(w / 2 - 160, by, 150, 50), "Restart"))
            {
                GameState.Reset();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if (GUI.Button(new Rect(w / 2 + 10, by, 150, 50), "Main Menu"))
            {
                GameState.Reset();
                SceneManager.LoadScene("MainMenuScene");
            }
        }
    }
}
