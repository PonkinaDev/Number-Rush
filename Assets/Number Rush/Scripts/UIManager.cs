using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona los tres paneles: MainMenu, GameHUD y GameOver.
/// Todos los campos se asignan desde el Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ── Paneles ─────────────────────────────────────────────────────────────
    [Header("Paneles")]
    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelHUD;
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private GameObject panelEmailStatus;   // panel pequeño de estado

    // ── Main Menu ───────────────────────────────────────────────────────────
    [Header("Main Menu")]
    [SerializeField] private TMP_InputField inputPlayerName;
    [SerializeField] private Button         btnPlay;

    // ── HUD ─────────────────────────────────────────────────────────────────
    [Header("HUD (durante partida)")]
    [SerializeField] private TextMeshProUGUI txtScore;
    [SerializeField] private TextMeshProUGUI txtTimer;
    [SerializeField] private Slider          timerSlider;

    // ── Game Over ───────────────────────────────────────────────────────────
    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI txtFinalScore;
    [SerializeField] private TextMeshProUGUI txtHits;
    [SerializeField] private TextMeshProUGUI txtAccuracy;
    [SerializeField] private TextMeshProUGUI txtTime;
    [SerializeField] private TMP_InputField  inputEmail;
    [SerializeField] private Button          btnSendEmail;
    [SerializeField] private Button          btnPlayAgain;
    [SerializeField] private Button          btnMenu;

    // ── Panel de estado email ────────────────────────────────────────────────
    [Header("Estado del correo")]
    [SerializeField] private TextMeshProUGUI txtEmailStatus;
    [SerializeField] private Image           imgStatusIcon;
    [SerializeField] private Color           colorSending = new Color(1f, 0.8f, 0f);
    [SerializeField] private Color           colorSuccess = new Color(0.2f, 0.9f, 0.4f);
    [SerializeField] private Color           colorError   = new Color(0.9f, 0.2f, 0.2f);

    // ── Referencias ─────────────────────────────────────────────────────────
    [Header("Dependencias")]
    [SerializeField] private EmailManager emailManager;
    [SerializeField] private float        gameDuration = 30f;  // debe coincidir con GameManager

    // ── Variables privadas ──────────────────────────────────────────────────
    private int   _lastScore;
    private int   _lastHits;
    private float _lastAccuracy;
    private int   _lastTime;

    // ── Unity ───────────────────────────────────────────────────────────────

    private void Start()
    {
        // Botones
        btnPlay?.onClick.AddListener(OnClickPlay);
        btnSendEmail?.onClick.AddListener(OnClickSendEmail);
        btnPlayAgain?.onClick.AddListener(OnClickPlayAgain);
        btnMenu?.onClick.AddListener(OnClickMenu);

        // Escuchar estados del email
        if (emailManager != null)
            emailManager.OnStatusChanged += HandleEmailStatus;

        ShowMainMenu();
    }

    // ── Llamados por GameManager ─────────────────────────────────────────────

    public void OnGameStarted()
    {
        panelMainMenu?.SetActive(false);
        panelHUD?.SetActive(true);
        panelGameOver?.SetActive(false);
        panelEmailStatus?.SetActive(false);

        UpdateScore(0);
        UpdateTimer(gameDuration);
    }

    public void UpdateScore(int score)
    {
        if (txtScore != null) txtScore.text = $"⭐ {score}";
    }

    public void UpdateTimer(float timeLeft)
    {
        if (txtTimer    != null) txtTimer.text          = $"⏱ {Mathf.CeilToInt(timeLeft)}s";
        if (timerSlider != null) timerSlider.value      = timeLeft / gameDuration;
    }

    public void OnGameOver(int score, int hits, float accuracy, int elapsed)
    {
        _lastScore    = score;
        _lastHits     = hits;
        _lastAccuracy = accuracy;
        _lastTime     = elapsed;

        panelHUD?.SetActive(false);
        panelGameOver?.SetActive(true);
        panelEmailStatus?.SetActive(false);

        if (txtFinalScore != null) txtFinalScore.text = $"{score}";
        if (txtHits       != null) txtHits.text       = $"{hits} aciertos";
        if (txtAccuracy   != null) txtAccuracy.text   = $"{accuracy:F1}%";
        if (txtTime       != null) txtTime.text       = $"{elapsed}s";

        // Pre-rellenar email si quedó guardado
        if (inputEmail != null && inputEmail.text == "") inputEmail.text = "";
        SetSendButtonInteractable(true);
    }

    public void ShowMainMenu()
    {
        panelMainMenu?.SetActive(true);
        panelHUD?.SetActive(false);
        panelGameOver?.SetActive(false);
        panelEmailStatus?.SetActive(false);
    }

    // ── Botones ─────────────────────────────────────────────────────────────

    private void OnClickPlay()
    {
        GameManager.Instance?.StartGame();
    }

    private void OnClickSendEmail()
    {
        string email      = inputEmail?.text?.Trim() ?? "";
        string playerName = inputPlayerName?.text?.Trim() ?? "Anónimo";

        if (string.IsNullOrEmpty(email))
        {
            ShowStatus("⚠ Ingresa un correo destino.", colorError);
            return;
        }

        SetSendButtonInteractable(false);
        emailManager?.SendGameResult(email, playerName, _lastScore,
                                     _lastHits, _lastAccuracy, _lastTime);
    }

    private void OnClickPlayAgain() => GameManager.Instance?.StartGame();
    private void OnClickMenu()      => GameManager.Instance?.ReturnToMenu();

    // ── Estado del email ─────────────────────────────────────────────────────

    private void HandleEmailStatus(EmailManager.SendStatus status, string error)
    {
        switch (status)
        {
            case EmailManager.SendStatus.Sending:
                ShowStatus("📤 Enviando correo...", colorSending);
                break;

            case EmailManager.SendStatus.Success:
                ShowStatus("✅ ¡Correo enviado correctamente!", colorSuccess);
                SetSendButtonInteractable(false);   // ya enviado, evitar duplicados
                break;

            case EmailManager.SendStatus.Failed:
                ShowStatus($"❌ Error: {error}", colorError);
                SetSendButtonInteractable(true);    // permite reintentar
                break;
        }
    }

    private void ShowStatus(string message, Color color)
    {
        panelEmailStatus?.SetActive(true);

        if (txtEmailStatus != null)
        {
            txtEmailStatus.text  = message;
            txtEmailStatus.color = color;
        }
    }

    private void SetSendButtonInteractable(bool value)
    {
        if (btnSendEmail != null) btnSendEmail.interactable = value;
    }
}