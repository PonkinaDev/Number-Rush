# Number Rush

Mini juego en Unity 6 donde el jugador hace clic en numeros que aparecen en pantalla antes de que desaparezcan. Al terminar la partida, el resultado se envia automaticamente por correo usando SMTP.

---

## Sistema de notificaciones por email

### Evento que dispara el envio

El correo se manda una sola vez, cuando se acaba el tiempo de la partida.

El flujo empieza en `GameManager.cs`, cuando el timer llega a cero:

```csharp
private void EndGame()
{
    State = GameState.GameOver;
    spawner?.StopSpawning();
    uiManager?.OnGameOver(Score, Hits, Accuracy, ElapsedTime);
}
```

Eso llama a `UIManager.OnGameOver()`, que es quien tiene el correo del jugador (ingresado al inicio) y llama directamente al metodo de envio:

```csharp
emailManager?.SendGameResult(_email, _playerName, score, hits, accuracy, elapsed);
```

---

### Flujo basico de envio SMTP

El envio usa `System.Net.Mail`, que ya viene con .NET

Para no bloquear el juego mientras se conecta server, el envio corre en un hilo separado con `Task.Run`. Una vez que termina, el resultado vuelve al hilo principal de Unity
```
Fin de partida
      |
      v
SendGameResult()  <-- construye asunto y cuerpo del correo
      |
      v
Task.Run(() => SendEmail())  <-- se ejecuta en hilo secundario
      |
      v
SmtpClient.Send()  <-- conexion al servidor de Gmail
      |
      v
Enqueue(resultado)  <-- devuelve success o error al hilo de Unity
      |
      v
OnStatusChanged  <-- la UI actualiza 
```

El metodo `SendEmail` usa estructura del codigo SMTP proporcionado por el profe:

```csharp
MailMessage mail = new MailMessage(); //object to create the email
mail.From = new MailAddress(fromEmail); //sender address
mail.To.Add(toEmail);
mail.Subject = subject; //email subject
mail.Body    = body;    //email body

SmtpClient smtp = new SmtpClient("smtp.gmail.com") //gmail smtp server
{
    Port        = 587, //extended SMTP port
    Credentials = new NetworkCredential(fromEmail, password),
    EnableSsl   = true
};
```

El puerto 587 con `EnableSsl = true` corresponde a STARTTLS, que es el protocolo que usa Gmail

---

### Manejo de respuestas del servidor

El bloque `try/catch` dentro de `SendEmail` cubre los dos casos posibles:

**Envio exitoso**

```csharp
smtp.Send(mail);
Debug.Log("Email sended succesfuly");
Enqueue(() => OnStatusChanged?.Invoke(SendStatus.Success, ""));
```

La UI muesta: `Email sent to [correo].`

**Error en el envio**

```csharp
catch (Exception ex)
{
    Debug.Log("Error: " + ex.Message);
    Enqueue(() => OnStatusChanged?.Invoke(SendStatus.Failed, ex.Message));
}
```

La UI muestra el mensaje de error directamente, por ejemplo:

- `550 5.4.5 Daily sending quota exceeded` — la cuenta de Gmail alcanzo el limite diario
- `535 Bad credentials` — contrasena de aplicacion incorrecta
- `A connection attempt failed` — sin conexion a internet

En todos los casos el juego no se congela ni crashea porque el error ocurre en el hilo secundario y tiene mensajes de error.

---

## Estructura del proyecto

```
Assets/
  Number Rush/
    Assets/
     HUD, Game Over, Main Menu -- imagenes de fondo
    Prefabs/
     Numberprefab -- numeros q sale en la pantalla
    Scene/
      Game -- unica escena 
    Scripts/
      GameManager.cs       -- logica principal, timer, puntaje
      NumberSpawner.cs     -- genera los numeros en pantalla
      ClickableNumber.cs   -- comportamiento de cada numero
      UIManager.cs         -- maneja los tres paneles de la UI
      EmailManager.cs      -- envio SMTP y contenido del correo

```

## Requisitos

- Unity 6000.0.34f1
- .NET Framework (no .NET Standard) en Player Settings > Api Compatibility Level
- Cuenta de Gmail con contrasena de aplicacion habilitada