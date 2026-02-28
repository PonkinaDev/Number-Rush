using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Adjunta este script al prefab del número.
/// El prefab necesita: SpriteRenderer o Image de fondo + TextMeshPro + Collider2D.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ClickableNumber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private SpriteRenderer bg;

    [Header("Colores")]
    [SerializeField] private Color normalColor  = new Color(0.06f, 0.13f, 0.24f);
    [SerializeField] private Color hitColor     = new Color(0.91f, 0.27f, 0.37f);
    [SerializeField] private Color expiredColor = new Color(0.3f,  0.3f,  0.3f);

    private int            _value;
    private float          _lifetime;
    private NumberSpawner  _spawner;
    private bool           _clicked;
    private Coroutine      _lifeCoroutine;

    // ── Inicialización ──────────────────────────────────────────────────────

    public void Init(int value, float lifetime, NumberSpawner spawner)
    {
        _value    = value;
        _lifetime = lifetime;
        _spawner  = spawner;
        _clicked  = false;

        if (label != null) label.text = value.ToString();
        if (bg    != null) bg.color   = normalColor;

        _lifeCoroutine = StartCoroutine(LifetimeCoroutine());
    }

    // ── Input ───────────────────────────────────────────────────────────────

    private void OnMouseDown()
    {
        if (_clicked || GameManager.Instance?.State != GameManager.GameState.Playing) return;

        _clicked = true;
        StopCoroutine(_lifeCoroutine);

        GameManager.Instance.RegisterHit();
        StartCoroutine(HitFeedback());
    }

    // ── Coroutines ──────────────────────────────────────────────────────────

    private IEnumerator LifetimeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < _lifetime)
        {
            elapsed += Time.deltaTime;

            // El fondo se va aclarando conforme pasa el tiempo (feedback visual)
            float t = elapsed / _lifetime;
            if (bg != null) bg.color = Color.Lerp(normalColor, expiredColor, t);

            yield return null;
        }

        // Número expirado sin ser clicado
        _spawner?.OnNumberDestroyed();
        Destroy(gameObject);
    }

    private IEnumerator HitFeedback()
    {
        if (bg != null) bg.color = hitColor;

        // Animación de escala
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.4f, t / 0.2f);
            yield return null;
        }

        _spawner?.OnNumberDestroyed();
        Destroy(gameObject);
    }
}