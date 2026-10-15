using UnityEngine;

// Immediate-mode UI keeps the interface small and independent of scene setup.
public class GameHud : MonoBehaviour
{
    static readonly Color Ink = new Color(.075f, .08f, .09f);
    static readonly Color Red = new Color(.88f, .055f, .09f);
    static readonly Color Paper = new Color(.95f, .95f, .93f);
    GUIStyle menuTitle, menuText, right, score, button, pauseTitle, pauseScore;

    void Styles()
    {
        if (menuTitle != null) return;
        menuTitle = Label(44, FontStyle.Bold); menuTitle.alignment = TextAnchor.MiddleCenter;
        menuText = Label(24); menuText.alignment = TextAnchor.MiddleCenter;
        right = Label(16); right.alignment = TextAnchor.UpperRight;
        score = Label(42, FontStyle.Bold); score.alignment = TextAnchor.UpperRight;
        button = new GUIStyle(GUI.skin.button) { fontSize = 22 };
        pauseTitle = Label(32);
        pauseTitle.alignment = TextAnchor.MiddleCenter;
        pauseScore = Label(22);
        pauseScore.alignment = TextAnchor.UpperRight;
    }
    static GUIStyle Label(int size, FontStyle weight = FontStyle.Normal)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight };
        style.normal.textColor = Ink;
        return style;
    }
    static void Rect(float x, float y, float w, float h, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = old;
    }
    bool Button(float x, float y, string label)
    {
        bool clicked = GUI.Button(new Rect(x, y, 320, 48), label, button);
        if (clicked) GameAudio.Play(Sound.Click, Vector3.zero);
        return clicked;
    }
    void OnGUI()
    {
        GameManager game = GameManager.Instance;
        if (game == null) return;
        Styles();
        float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
        Matrix4x4 previous = GUI.matrix;
        if (game.Level == null || game.Paused)
        {
            GUI.matrix = Matrix4x4.identity;
            Rect(0, 0, Screen.width, Screen.height, Color.white);
        }
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1600f * scale) * .5f, (Screen.height - 900f * scale) * .5f, 0f), Quaternion.identity, Vector3.one * scale);
        if (game.Level == null) Menu(game);
        else if (game.Paused) PauseMenu(game);
        else Hud(game);
        if (game.Fade > 0f) Rect(0, 0, 1600, 900, new Color(.04f, .04f, .05f, game.Fade));
        GUI.matrix = previous;
    }
    void Menu(GameManager game)
    {
        if (game.Finished)
        {
            GUI.Label(new Rect(500, 220, 600, 70), "Complete", menuTitle);
            GUI.Label(new Rect(500, 320, 600, 45), "Final score: " + game.Score, menuText);
            GUI.Label(new Rect(500, 375, 600, 45), "Time: " + Clock(game.RunTime), menuText);
            if (Button(640, 470, "Run again")) game.StartRun();
            if (Button(640, 530, "Quit")) game.Quit();
        }
        else
        {
            GUI.Label(new Rect(500, 240, 600, 80), "REDLINE", menuTitle);
            if (Button(640, 380, "Play")) game.StartRun();
            if (Button(640, 440, "Quit")) game.Quit();
        }
    }
    void PauseMenu(GameManager game)
    {
        GUI.Label(new Rect(640, 285, 320, 55), "Paused", pauseTitle);
        GUI.Label(new Rect(1250, 42, 290, 35), "Score: " + game.Score, pauseScore);
        if (Button(640, 365, "Resume")) game.SetPaused(false);
        if (Button(640, 425, "Restart")) game.RestartLevel();
        if (Button(640, 485, "Main menu")) game.MainMenu();
        if (Button(640, 545, "Quit")) game.Quit();
    }
    void Hud(GameManager game)
    {
        // Keep the numbers readable against the platforms and background.
        Rect(36, 30, 215, 84, new Color(Paper.r, Paper.g, Paper.b, .88f));
        DrawScore(game);
        int health = game.Health != null ? game.Health.Health : 3;
        for (int i = 0; i < 3; i++) Rect(58 + i * 42, 58, 28, 28, i < health ? Ink : new Color(.75f, .75f, .74f));
        GUI.Label(new Rect(1270, 810, 275, 30), Clock(game.RunTime), right);
        if (game.Player != null && game.Player.Charge > 0f)
        {
            Rect(680, 800, 240, 5, new Color(.1f, .1f, .1f, .18f));
            Rect(680, 800, 240 * game.Player.Charge, 5, Red);
        }
        if (game.Powerups == null) return;
        int y = 150;
        Buff("SPEED", game.Powerups.SpeedRemaining, 5f, new Color(1f, .4f, .06f), ref y);
        Buff("SCORE x2", game.Powerups.ScoreRemaining, 10f, new Color(.62f, .22f, .86f), ref y);
        Buff("DOUBLE JUMP", game.Powerups.JumpRemaining, 30f, new Color(.05f, .48f, .95f), ref y);
    }
    void DrawScore(GameManager game)
    {
        Rect(1320, 30, 244, 103, new Color(Paper.r, Paper.g, Paper.b, .88f));
        GUI.Label(new Rect(1360, 42, 180, 28), "SCORE", right);
        GUI.Label(new Rect(1300, 65, 240, 62), game.Score.ToString("D6"), score);
    }
    void Buff(string label, float seconds, float duration, Color color, ref int y)
    {
        if (seconds <= 0f) return;
        GUI.Label(new Rect(1270, y, 270, 30), label + "  " + Mathf.CeilToInt(seconds) + "s", right);
        Rect(1330, y + 30, 210, 4, new Color(.1f, .1f, .1f, .15f));
        Rect(1330, y + 30, 210 * Mathf.Clamp01(seconds / duration), 4, color);
        y += 58;
    }
    static string Clock(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        return minutes.ToString("D2") + ":" + (time % 60f).ToString("00.00");
    }
}

