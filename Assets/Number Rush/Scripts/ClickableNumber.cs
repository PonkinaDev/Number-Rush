using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClickableNumber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private SpriteRenderer bg;   // Background visual element

    private NumberSpawner _spawner; // Reference to the spawner that created this object
    private bool          _clicked; // Prevents multiple clicks

    public void Init(int value, float lifetime, NumberSpawner spawner)
    {
        _spawner = spawner;
        _clicked = false;

        // Set displayed number
        if (label != null)
            label.text = value.ToString();

        // Start lifetime countdown
        StartCoroutine(LifetimeRoutine(lifetime));
    }

    private void OnMouseDown()
    {
        // Ignore input if already clicked or game is not active
        if (_clicked || GameManager.Instance?.State != GameManager.GameState.Playing)
            return;

        _clicked = true;

        // Stop lifetime routine
        StopAllCoroutines();

        // Register successful hit
        GameManager.Instance.RegisterHit();

        // Play hit feedback animation
        StartCoroutine(HitAnimation());
    }

    private IEnumerator LifetimeRoutine(float lifetime)
    {
        // Gradually fade background color over lifetime
        Color start = bg != null ? bg.color : Color.white;
        Color end   = new Color(0.3f, 0.3f, 0.3f);

        float t = 0f;

        while (t < lifetime)
        {
            t += Time.deltaTime;

            if (bg != null)
                bg.color = Color.Lerp(start, end, t / lifetime);

            yield return null;
        }

        // Notify spawner and destroy object if time expires
        _spawner?.OnNumberDestroyed();
        Destroy(gameObject);
    }

    private IEnumerator HitAnimation()
    {
        // Change color to indicate successful click
        if (bg != null)
            bg.color = new Color(0.91f, 0.27f, 0.37f);

        float t = 0f;

        // Scale-up animation for visual feedback
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.4f, t / 0.2f);
            yield return null;
        }

        // Notify spawner and destroy object after animation
        _spawner?.OnNumberDestroyed();
        Destroy(gameObject);
    }
}