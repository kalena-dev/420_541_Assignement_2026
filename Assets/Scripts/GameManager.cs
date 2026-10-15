using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int Score { get; private set; }
    public float RunTime { get; private set; }
    public float LevelTime { get; private set; }
    public bool Paused { get; private set; }
    public bool IsPlaying => Level != null && !Paused && !loading;
    public float Fade { get; private set; }
    public LevelInfo Level { get; private set; }
    public CharacterMovement Player { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerPowerups Powerups { get; private set; }
    public bool Finished => SceneManager.GetActiveScene().name == "EndScreen";
    int levelEntryScore;
    bool loading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null) new GameObject("Game Manager").AddComponent<GameManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<GameAudio>();
        gameObject.AddComponent<GameHud>();
        SceneManager.sceneLoaded += SceneLoaded;
        Application.targetFrameRate = 120;
    }
    void OnDestroy()
    {
        if (Instance == this) { SceneManager.sceneLoaded -= SceneLoaded; Instance = null; }
    }
    void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Level = FindAnyObjectByType<LevelInfo>();
        Player = FindAnyObjectByType<CharacterMovement>();
        Health = Player != null ? Player.GetComponent<PlayerHealth>() : null;
        Powerups = Player != null ? Player.GetComponent<PlayerPowerups>() : null;
        if (Level != null) { levelEntryScore = Score; LevelTime = 0f; }
        Paused = false;
        Time.timeScale = 1f;
        SetCursor(Level != null);
    }
    void Update()
    {
        if (Level != null && !loading && Input.GetKeyDown(KeyCode.Escape)) SetPaused(!Paused);
        if (IsPlaying) { RunTime += Time.deltaTime; LevelTime += Time.deltaTime; }
    }
    public void SetPaused(bool paused)
    {
        Paused = paused;
        Time.timeScale = paused ? 0f : 1f;
        if (Player != null) Player.CancelCharge();
        SetCursor(!paused && Level != null);
    }
    static void SetCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
    public void AddScore(int points) { Score += Mathf.Max(0, points); }
    public void StartRun()
    {
        if (loading) return;
        Score = 0;
        RunTime = 0f;
        Load("Level01");
    }
    public void RestartLevel()
    {
        if (loading || Level == null) return;
        // Keep completed levels' points, but prevent farming the same pickups on retries.
        Score = levelEntryScore;
        Load(SceneManager.GetActiveScene().name);
    }
    public void FinishLevel()
    {
        if (!IsPlaying) return;
        GameAudio.Play(Sound.Goal, Player.transform.position);
        Load(Level.levelNumber < 3 ? "Level0" + (Level.levelNumber + 1) : "EndScreen");
    }
    public void MainMenu() { if (!loading) Load("MainMenu"); }
    public void Quit()
    {
        Time.timeScale = 1f;
        SetCursor(false);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void Load(string scene) { StartCoroutine(ChangeScene(scene)); }
    IEnumerator ChangeScene(string scene)
    {
        loading = true;
        Time.timeScale = 1f;
        Paused = false;
        for (float t = 0; t < .18f; t += Time.unscaledDeltaTime)
        { Fade = t / .18f; yield return null; }
        Fade = 1f;
        yield return SceneManager.LoadSceneAsync(scene);
        for (float t = 0; t < .22f; t += Time.unscaledDeltaTime)
        { Fade = 1f - t / .22f; yield return null; }
        Fade = 0f;
        loading = false;
    }
}

