using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UnityEngine;

public class EmailManager : MonoBehaviour
{
    // ── Configuración SMTP ──────────────────────────────────────────────────
    private const string FromEmail = "ingmultimediausbbog@gmail.com";
    private const string AppPassword = "fsjq ioqf zsxs jrzf";
    private const string SmtpHost   = "smtp.gmail.com";
    private const int    SmtpPort   = 587;

    // Estados del envío
    public enum SendStatus { Idle, Sending, Success, Failed }
    public SendStatus CurrentStatus { get; private set; } = SendStatus.Idle;
    public string     LastError     { get; private set; } = "";

    // Evento para notificar a la UI cuando cambia el estado
    public event Action<SendStatus, string> OnStatusChanged;

    // ── API pública ─────────────────────────────────────────────────────────

    /// <summary>
    /// Envía el correo de resultado de partida de forma asíncrona.
    /// </summary>
    public async void SendGameResult(string toEmail, string playerName,
                                     int score, int totalClicks,
                                     float accuracy, int timeSeconds)
    {
        if (!IsValidEmail(toEmail))
        {
            SetStatus(SendStatus.Failed, "Correo destino inválido.");
            return;
        }

        SetStatus(SendStatus.Sending, "");

        string subject = $"[Number Rush] Resultado de partida – {score} pts";
        string body    = BuildEmailBody(playerName, score, totalClicks, accuracy, timeSeconds);

        await Task.Run(() => Send(toEmail, subject, body));
    }

    // ── Lógica interna ──────────────────────────────────────────────────────

    private void Send(string toEmail, string subject, string body)
    {
        try
        {
            using MailMessage mail = new MailMessage();
            mail.From    = new MailAddress(FromEmail, "Number Rush");
            mail.To.Add(toEmail);
            mail.Subject    = subject;
            mail.Body       = body;
            mail.IsBodyHtml = true;

            using SmtpClient smtp = new SmtpClient(SmtpHost)
            {
                Port        = SmtpPort,
                Credentials = new NetworkCredential(FromEmail, AppPassword),
                EnableSsl   = true
            };

            smtp.Send(mail);

            // Volver al hilo principal para actualizar UI
            UnityMainThreadDispatcher.Enqueue(() => SetStatus(SendStatus.Success, ""));
        }
        catch (Exception ex)
        {
            UnityMainThreadDispatcher.Enqueue(() => SetStatus(SendStatus.Failed, ex.Message));
        }
    }

    private string BuildEmailBody(string name, int score,
                                  int clicks, float accuracy, int seconds)
    {
        return $@"
        <html>
        <body style='font-family:Arial,sans-serif; background:#1a1a2e; color:#eee; padding:30px;'>
          <div style='max-width:480px; margin:auto; background:#16213e;
                      border-radius:12px; padding:30px; border:2px solid #0f3460;'>
            <h1 style='color:#e94560; text-align:center; margin-top:0;'>
              🎮 Number Rush
            </h1>
            <h2 style='color:#fff; text-align:center;'>Resumen de partida</h2>
            <hr style='border-color:#0f3460;'/>
            <table style='width:100%; border-collapse:collapse; margin-top:20px;'>
              <tr>
                <td style='padding:10px; color:#aaa;'>👤 Jugador</td>
                <td style='padding:10px; font-weight:bold; color:#fff;'>{(string.IsNullOrEmpty(name) ? "Anónimo" : name)}</td>
              </tr>
              <tr style='background:#0f3460;'>
                <td style='padding:10px; color:#aaa;'>⭐ Puntuación</td>
                <td style='padding:10px; font-weight:bold; color:#e94560; font-size:1.4em;'>{score} pts</td>
              </tr>
              <tr>
                <td style='padding:10px; color:#aaa;'>🖱️ Clics acertados</td>
                <td style='padding:10px; color:#fff;'>{clicks}</td>
              </tr>
              <tr style='background:#0f3460;'>
                <td style='padding:10px; color:#aaa;'>🎯 Precisión</td>
                <td style='padding:10px; color:#fff;'>{accuracy:F1}%</td>
              </tr>
              <tr>
                <td style='padding:10px; color:#aaa;'>⏱️ Tiempo jugado</td>
                <td style='padding:10px; color:#fff;'>{seconds} segundos</td>
              </tr>
            </table>
            <p style='text-align:center; margin-top:25px; color:#888; font-size:0.85em;'>
              Correo generado automáticamente por Number Rush · Unity 6
            </p>
          </div>
        </body>
        </html>";
    }

    private void SetStatus(SendStatus status, string error)
    {
        CurrentStatus = status;
        LastError     = error;
        OnStatusChanged?.Invoke(status, error);
    }

    private bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Contains(".");
}