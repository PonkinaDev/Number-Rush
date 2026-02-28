using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // ── Configuración ───────────────────────────────────────────────────────
    [Header("Duración de partida (segundos)")]
    [SerializeField] private float gameDuration = 30f;

    [Header("Puntos por número clicado")]
    [SerializeField] private int pointsPerHit = 10;

    // ── Referencias ─────────────────────────────────────────────────────────
    [Header("Referencias")]
    [SerializeField] private NumberSpawner spawner;
    [SerializeField] private UIManager     uiManager;

    // ── Estado del juego ────────────────────────────────────────────────────
    public enum GameState { MainMenu, Playing, GameOver }
    public GameState State { get; private set; } = GameState.MainMenu;

    public int   Score        { get; private set; }
    public int   HitCount     { get; private set; }   // clics acertados
    public int   MissCount    { get; private set; }   // clics fallidos
    public float TimeLeft     { get; private set; }
    public float Accuracy     => (HitCount + MissCount) == 0 ? 100f
                                 : (float)HitCount / (HitCount + MissCount) * 100f;
    public int   ElapsedTime  => Mathf.RoundToInt(gameDuration - TimeLeft);

    // ── Eventos Unity ───────────────────────────────────────────────────────
    public UnityEvent OnGameStarted  = new UnityEvent();
    public UnityEvent OnGameOver     = new UnityEvent();
    public UnityEvent OnScoreChanged = new UnityEvent();

    // ── Singleton sencillo ──────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (State != GameState.Playing) return;

        TimeLeft -= Time.deltaTime;
        uiManager?.UpdateTimer(TimeLeft);

        if (TimeLeft <= 0f)
            EndGame();
    }

    // ── API pública ─────────────────────────────────────────────────────────

    public void StartGame()
    {
        Score     = 0;
        HitCount  = 0;
        MissCount = 0;
        TimeLeft  = gameDuration;
        State     = GameState.Playing;

        OnGameStarted.Invoke();
        spawner?.StartSpawning();
        uiManager?.OnGameStarted();
    }

    /// <summary>Llamado por ClickableNumber cuando el jugador acierta.</summary>
    public void RegisterHit()
    {
        HitCount++;
        Score += pointsPerHit;
        OnScoreChanged.Invoke();
        uiManager?.UpdateScore(Score);
    }

    /// <summary>Llamado por la UI o área de juego cuando el jugador falla un clic.</summary>
    public void RegisterMiss()
    {
        MissCount++;
    }

    public void ReturnToMenu()
    {
        spawner?.StopSpawning();
        State = GameState.MainMenu;
        uiManager?.ShowMainMenu();
    }

    // ── Privados ────────────────────────────────────────────────────────────

    private void EndGame()
    {
        State = GameState.GameOver;
        spawner?.StopSpawning();
        OnGameOver.Invoke();
        uiManager?.OnGameOver(Score, HitCount, Accuracy, ElapsedTime);
    }
}