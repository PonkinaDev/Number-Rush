using System.Collections;
using UnityEngine;

public class NumberSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private ClickableNumber numberPrefab; // Prefab used to spawn numbers

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval   = 1.2f;  // Time between spawn attempts
    [SerializeField] private float numberLifetime  = 2f;    // Lifetime of each number
    [SerializeField] private int   maxSimultaneous = 4;     // Maximum numbers active at the same time

    [Header("Spawn Area")]
    [SerializeField] private Vector2 minBounds = new Vector2(-7f, -3.5f); // Minimum spawn position
    [SerializeField] private Vector2 maxBounds = new Vector2( 7f,  3.5f); // Maximum spawn position

    private Coroutine _loop;   // Reference to the spawn coroutine
    private int       _active; // Current number of active spawned objects

    public void StartSpawning()
    {
        _active = 0;           // Reset active counter
        _loop   = StartCoroutine(SpawnLoop()); // Start spawn loop
    }

    public void StopSpawning()
    {
        // Stop spawn loop if running
        if (_loop != null) StopCoroutine(_loop);

        // Destroy all remaining spawned numbers
        foreach (var n in FindObjectsByType<ClickableNumber>(FindObjectsSortMode.None))
            Destroy(n.gameObject);
    }

    public void OnNumberDestroyed()
    {
        // Decrease active counter when a number is removed
        _active--;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Spawn only if below the maximum allowed
            if (_active < maxSimultaneous)
            {
                // Generate random position within bounds
                Vector2 pos = new Vector2(
                    Random.Range(minBounds.x, maxBounds.x),
                    Random.Range(minBounds.y, maxBounds.y));

                // Instantiate and initialize number
                var n = Instantiate(numberPrefab, pos, Quaternion.identity);
                n.Init(Random.Range(1, 10), numberLifetime, this);

                _active++;
            }

            // Wait before next spawn attempt
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}