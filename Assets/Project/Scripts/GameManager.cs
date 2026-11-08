using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Ready, Playing, Paused, Won, Lost }

/// Stan gry, odliczanie 2 minut, pauza, reset. Powieś na pustym obiekcie "GameManager".
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // static => ustawienia z menu przeżywają reset sceny
    public static int EnemyCount = 3;
    public static int MeteorsPerMinute = 12;

    public CastleHealth castle;
    public float winTime = 120f;   // ile sekund trzeba przetrwać

    public GameState State { get; private set; } = GameState.Ready;
    public float TimeLeft { get; private set; }
    public int BlockedCount { get; private set; }
    public bool IsPlaying => State == GameState.Playing;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        TimeLeft = winTime;
        if (castle == null) castle = FindFirstObjectByType<CastleHealth>();
    }

    void Start()
    {
        if (castle != null) castle.OnDestroyed += Lose;
    }

    void OnDestroy()
    {
        if (castle != null) castle.OnDestroyed -= Lose;
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (State != GameState.Playing) return;

        TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            State = GameState.Won;
        }
    }

    public void StartOrResume()
    {
        if (State == GameState.Ready || State == GameState.Paused)
        {
            State = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void TogglePause()
    {
        if (State == GameState.Playing) Pause();
        else if (State == GameState.Paused) StartOrResume();
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AddBlock() { BlockedCount++; }

    void Lose()
    {
        if (State == GameState.Playing) State = GameState.Lost;
    }
}
