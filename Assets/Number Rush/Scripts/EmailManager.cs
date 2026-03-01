using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UnityEngine;

public class EmailManager : MonoBehaviour
{
    /////// Credentials (required) //////////////////////////////////////////////////////////////
    string fromEmail = "ingmultimediausbbog@gmail.com"; //sender email address
    string password  = "fsjq ioqf zsxs jrzf";          //app password

    ///// Send status enum (used to notify the UI) ////////////////////////////////////////////////////////////
    public enum SendStatus { Idle, Sending, Success, Failed }
    public event Action<SendStatus, string> OnStatusChanged;

    // Queue to return results to the Unity main thread
    private readonly Queue<Action> _mainThreadQueue = new Queue<Action>();

    private void Update()
    {
        lock (_mainThreadQueue)
            while (_mainThreadQueue.Count > 0)
                _mainThreadQueue.Dequeue()?.Invoke();
    }

    //////// Entry point: called by UIManager when the game ends ////////

    public async void SendGameResult(string toEmail, string playerName,
                                     int score, int hits, float accuracy, int seconds)
    {
        OnStatusChanged?.Invoke(SendStatus.Sending, "");

        string subject = $"[Number Rush] Game result - {score} pts";
        string body    = BuildBody(playerName, score, hits, accuracy, seconds);

        await Task.Run(() => SendEmail(toEmail, subject, body));
    }

    ///////// Send method -- same structure as the original SMTP file provided /////////

    void SendEmail(string toEmail, string subject, string body)
    {
        MailMessage mail = new MailMessage(); //object to create the email
        mail.From = new MailAddress(fromEmail); //sender address
        mail.To.Add(toEmail);
        mail.Subject    = subject; //email subject
        mail.Body       = body;   //email body
        mail.IsBodyHtml = true;

        SmtpClient smtp = new SmtpClient("smtp.gmail.com") //gmail smtp server
        {
            Port        = 587, //extended SMTP port
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl   = true
        };

        try
        {
            smtp.Send(mail);
            Debug.Log("Email sended succesfuly");
            Enqueue(() => OnStatusChanged?.Invoke(SendStatus.Success, ""));
        }
        catch (Exception ex)
        {
            Debug.Log("Error: " + ex.Message);
            Enqueue(() => OnStatusChanged?.Invoke(SendStatus.Failed, ex.Message));
        }
    }

    //////// Returns the action to the main thread to update the UI ///////////////////

    private void Enqueue(Action action)
    {
        lock (_mainThreadQueue) { _mainThreadQueue.Enqueue(action); }
    }

    //////// Dynamic email content ////////

    private (string rank, string title, string color) GetRank(int score)
    {
        if (score >= 200) return ("[S]", "Absolute legend",  "#FFD700");
        if (score >= 150) return ("[A]", "Elite expert",     "#C0C0C0");
        if (score >= 100) return ("[B]", "Great performance","#4FC3F7");
        if (score >= 60)  return ("[C]", "Good attempt",     "#81C784");
        if (score >= 30)  return ("[D]", "Keep improving",   "#FFB74D");
                          return ("[E]", "Keep practicing",  "#EF9A9A");
    }

    private string GetAccuracyMessage(float accuracy)
    {
        if (accuracy >= 95) return "Your accuracy is nearly perfect. Outstanding reflexes.";
        if (accuracy >= 80) return "Very good accuracy. You clearly know what you are doing.";
        if (accuracy >= 60) return "Acceptable accuracy, but there is still room to improve.";
        if (accuracy >= 40) return "Watch out for random clicks, quality matters more than quantity.";
                            return "Looks like the mouse has a mind of its own. Keep training.";
    }

    private string GetTip(int score, int hits)
    {
        if (hits == 0)    return "<em>Tip:</em> Click on the numbers before they disappear.";
        if (score < 50)   return "<em>Tip:</em> Focus on numbers near the center, they are easier to reach.";
        if (score >= 150) return "<em>Pro tip:</em> Try to anticipate where the next number will appear.";
                          return "<em>Tip:</em> Keep your eyes at the center of the screen to react faster.";
    }

    //////// HTML email body ///////////

    private string BuildBody(string nombre, int score, int hits, float accuracy, int seconds)
    {
        string name = string.IsNullOrEmpty(nombre) ? "Player" : nombre;
        var (rank, title, color) = GetRank(score);

        return $@"<html>
<body style='font-family:Arial,sans-serif;color:#222;padding:30px;'>

  <h2>Number Rush - Game Result</h2>
  <p>Hi {name}, here is your result.</p>

  <p><b>Score:</b> {score} pts ({title})<br>
     <b>Hits:</b> {hits}<br>
     <b>Accuracy:</b> {accuracy:F1}%<br>
     <b>Time:</b> {seconds}s</p>

  <p>{GetAccuracyMessage(accuracy)}</p>
  <p>{GetTip(score, hits)}</p>

</body>
</html>";
    }
}