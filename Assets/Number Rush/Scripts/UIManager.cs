using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // UI Panels
    [Header("Paneles")]
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private GameObject panelHUD;
    [SerializeField] private GameObject panelGameOver;

    // Main Menu references
    [Header("Menú principal")]
    [SerializeField] private TMP_InputField  inputName;
    [SerializeField] private TMP_InputField  inputEmail;
    [SerializeField] private TextMeshProUGUI txtMenuError;
    [SerializeField] private Button          btnPlay;

    // In-game HUD references
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI txtScore;
    [SerializeField] private TextMeshProUGUI txtTimer;
    [SerializeField] private Slider          timerSlider;

    // Game Over screen statistics and actions
    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI txtFinalScore;
    [SerializeField] private TextMeshProUGUI txtHits;
    [SerializeField] private TextMeshProUGUI txtAccuracy;
    [SerializeField] private TextMeshProUGUI txtTime;
    [SerializeField] private TextMeshProUGUI txtEmailStatus;
    [SerializeField] private Button          btnPlayAgain;
    [SerializeField] private Button          btnMenu;

    // External dependencies and configurable settings
    [Header("Dependencias")]
    [SerializeField] private EmailManager emailManager;
    [SerializeField] private float        gameDuration = 30f;

    // Cached player data collected from the main menu
    private string _nombre = "";
    private string _email  = "";

    // Visual feedback colors for email status
    private readonly Color colorSending = new Color(1f,  0.8f, 0f);
    private readonly Color colorSuccess = new Color(0.2f,0.9f, 0.4f);
    private readonly Color colorError   = new Color(0.9f,0.2f, 0.2f);

    private void Start()
    {
        // Bind UI buttons to their respective actions
        btnPlay?.onClick.AddListener(OnPlay);
        btnPlayAgain?.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());
        btnMenu?.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());

        // Subscribe to email sending status updates
        if (emailManager != null)
            emailManager.OnStatusChanged += OnEmailStatus;

        // Ensure menu error is hidden on startup
        txtMenuError?.gameObject.SetActive(false);

        // Initialize in main menu state
        ShowMainMenu();
    }

    /////////////// Main Menu Logic ///////////////////////////////////////////

    private void OnPlay()
    {
        // Capture and sanitize user input
        _nombre = inputName?.text?.Trim()  ?? "";
        _email  = inputEmail?.text?.Trim() ?? "";

        // Basic validation before starting the game
        if (string.IsNullOrEmpty(_nombre))
        { 
            SetMenuError("Ingresa tu nombre."); 
            return; 
        }

        if (!_email.Contains("@") || !_email.Contains("."))
        { 
            SetMenuError("Ingresa un correo válido."); 
            return; 
        }

        // Clear error state and notify GameManager to start
        txtMenuError?.gameObject.SetActive(false);
        GameManager.Instance?.StartGame();
    }

    private void SetMenuError(string msg)
    {
        // Displays validation feedback in the main menu
        if (txtMenuError == null) return;
        txtMenuError.text = msg;
        txtMenuError.gameObject.SetActive(true);
    }

    ////////////////// Methods Invoked by GameManager ////////////////////////////

    public void OnGameStarted()
    {
        // Transition UI to gameplay state
        panelMenu?.SetActive(false);
        panelHUD?.SetActive(true);
        panelGameOver?.SetActive(false);

        UpdateScore(0);
        UpdateTimer(gameDuration);
    }

    public void UpdateScore(int score)
    {
        // Updates live score display
        if (txtScore != null) 
            txtScore.text = $" {score}";
    }

    public void UpdateTimer(float t)
    {
        // Updates both numeric timer and visual progress bar
        if (txtTimer != null)    
            txtTimer.text  = $" {Mathf.CeilToInt(t)}s";

        if (timerSlider != null) 
            timerSlider.value = t / gameDuration;
    }

    public void OnGameOver(int score, int hits, float accuracy, int elapsed)
    {
        // Transition UI to Game Over state
        panelHUD?.SetActive(false);
        panelGameOver?.SetActive(true);

        // Display final statistics
        if (txtFinalScore != null) txtFinalScore.text = $"{score}";
        if (txtHits       != null) txtHits.text       = $"{hits} aciertos";
        if (txtAccuracy   != null) txtAccuracy.text   = $"{accuracy:F1}%";
        if (txtTime       != null) txtTime.text       = $"{elapsed}s";

        // Trigger email result delivery
        SetEmailStatus(" Enviando resultado a tu correo...", colorSending);
        emailManager?.SendGameResult(_email, _nombre, score, hits, accuracy, elapsed);
    }

    public void ShowMainMenu()
    {
        // Restores UI to initial menu state
        panelMenu?.SetActive(true);
        panelHUD?.SetActive(false);
        panelGameOver?.SetActive(false);
    }

    /////////////// Email Status Handling ////////////////////////////////////////

    private void OnEmailStatus(EmailManager.SendStatus status, string error)
    {
        // Reacts to asynchronous email sending state changes
        switch (status)
        {
            case EmailManager.SendStatus.Sending:
                SetEmailStatus("Enviando resultado a el correo...", colorSending); 
                break;

            case EmailManager.SendStatus.Success:
                SetEmailStatus($"Correo enviado a {_email}", colorSuccess);      
                break;

            case EmailManager.SendStatus.Failed:
                SetEmailStatus($"Error al enviar: {error}", colorError);           
                break;
        }
    }

    private void SetEmailStatus(string msg, Color color)
    {
        // Updates Game Over email feedback text and color
        if (txtEmailStatus == null) return;

        txtEmailStatus.text  = msg;
        txtEmailStatus.color = color;
        txtEmailStatus.gameObject.SetActive(true);
    }
}