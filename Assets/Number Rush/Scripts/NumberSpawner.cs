using System.Collections;
using UnityEngine;

public class NumberSpawner : MonoBehaviour
{
    [Header("Prefab del número clicable")]
    [SerializeField] private ClickableNumber numberPrefab;

    [Header("Tiempo entre spawns (segundos)")]
    [SerializeField] private float spawnInterval = 1.2f;

    [Header("Tiempo de vida de cada número")]
    [SerializeField] private float numberLifetime = 2f;

    [Header("Máximos simultáneos en pantalla")]
    [SerializeField] private int maxSimultaneous = 4;

    [Header("Límites de la zona de juego (en unidades mundo)")]
    [SerializeField] private Vector2 minBounds = new Vector2(-7f, -3.5f);
    [SerializeField] private Vector2 maxBounds = new Vector2( 7f,  3.5f);

    private Coroutine _spawnCoroutine;
    private int       _activeCount;

    public void StartSpawning()
    {
        _activeCount = 0;
        _spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);

        // Destruir números que queden en pantalla
        foreach (var n in FindObjectsByType<ClickableNumber>(FindObjectsSortMode.None))
            Destroy(n.gameObject);

        _activeCount = 0;
    }

    public void OnNumberDestroyed() => _activeCount--;

    // ── Privados ────────────────────────────────────────────────────────────

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (_activeCount < maxSimultaneous)
                SpawnNumber();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnNumber()
    {
        Vector2 pos = new Vector2(
            Random.Range(minBounds.x, maxBounds.x),
            Random.Range(minBounds.y, maxBounds.y)
        );

        int value = Random.Range(1, 10);    // número a mostrar (1-9)

        ClickableNumber instance = Instantiate(numberPrefab, pos, Quaternion.identity);
        instance.Init(value, numberLifetime, this);
        _activeCount++;
    }
}