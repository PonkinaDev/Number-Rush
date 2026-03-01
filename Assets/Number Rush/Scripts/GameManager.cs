using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameDuration  = 30f;   // Total duration of the game in seconds
    [SerializeField] private int   pointsPerHit  = 10;    // Points awarded for each successful hit

    [Header("References")]
    [SerializeField] private NumberSpawner spawner;       // Reference to the number spawner
    [SerializeField] private UIManager     uiManager;     // Reference to the UI manager

    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Game states
    public enum GameState { MainMenu, Playing, GameOver }
    public GameState State { get; private set; } = GameState.MainMenu;

    // Public read-only properties
    public int   Score    { get; private set; }  // Current score
    public int   Hits     { get; private set; }  // Total successful hits
    public int   Misses   { get; private set; }  // Total missed attempts
    public float TimeLeft { get; private set; }  // Remaining time

    // Calculates accuracy percentage
    public float Accuracy => (Hits + Misses) == 0 
        ? 100f 
        : (float)Hits / (Hits + Misses) * 100f;

    // Calculates elapsed time since the game started
    public int ElapsedTime => Mathf.RoundToInt(gameDuration - TimeLeft);

    private void Awake()
    {
        // Ensure only one instance of GameManager exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        // Only update the timer if the game is currently playing
        if (State != GameState.Playing) return;

        TimeLeft -= Time.deltaTime;

        // Update UI timer display
        uiManager?.UpdateTimer(TimeLeft);

        // End the game if time runs out
        if (TimeLeft <= 0f)
            EndGame();
    }

    public void StartGame()
    {
        // Reset game values
        Score    = 0;
        Hits     = 0;
        Misses   = 0;
        TimeLeft = gameDuration;

        // Change state to Playing
        State = GameState.Playing;

        // Start spawning numbers
        spawner?.StartSpawning();

        // Notify UI that the game has started
        uiManager?.OnGameStarted();
    }

    public void RegisterHit()
    {
        // Increase hit count and score
        Hits++;
        Score += pointsPerHit;

        // Update score in UI
        uiManager?.UpdateScore(Score);
    }

    public void RegisterMiss()
    {
        // Increase miss count
        Misses++;
    }

    public void ReturnToMenu()
    {
        // Stop spawning and return to main menu
        spawner?.StopSpawning();
        State = GameState.MainMenu;

        // Show main menu UI
        uiManager?.ShowMainMenu();
    }

    private void EndGame()
    {
        // Change state to GameOver
        State = GameState.GameOver;

        // Stop spawning numbers
        spawner?.StopSpawning();

        // Send final results to UI
        uiManager?.OnGameOver(Score, Hits, Accuracy, ElapsedTime);
    }
}