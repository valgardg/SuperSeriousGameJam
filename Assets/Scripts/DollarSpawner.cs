using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollarSpawner : MonoBehaviour
{
    public event System.Action<int> DollarReachedTarget;

    public GameObject dollarPrefab;
    public Transform dollarTarget;

    [Header("Audio")]
    [SerializeField] private AudioSource dollarSpawnSound;

    [Header("Timing")]
    [Min(0f)]
    public float delayBetweenSpawns = 0.1f;

    private readonly Queue<SpawnRequest> spawnQueue = new Queue<SpawnRequest>();
    private Coroutine spawnQueueCoroutine;
    private int activeDollarCount;

    public bool IsBusy => spawnQueueCoroutine != null
        || spawnQueue.Count > 0
        || activeDollarCount > 0;

    private struct SpawnRequest
    {
        public int Count;
        public Vector3 Origin;
        public bool IsNegative;

        public SpawnRequest(int count, Vector3 origin, bool isNegative)
        {
            Count = count;
            Origin = origin;
            IsNegative = isNegative;
        }
    }

    public void SpawnDollars(int value, Transform origin)
    {
        if (value == 0 || origin == null)
            return;

        SpawnDollars(value, origin.position);
    }

    public void SpawnDollars(int value, Vector3 origin)
    {
        if (value == 0)
            return;

        int count = Mathf.Abs(value);
        spawnQueue.Enqueue(new SpawnRequest(count, origin, value < 0));

        if (spawnQueueCoroutine == null)
            spawnQueueCoroutine = StartCoroutine(ProcessSpawnQueue());
    }

    private IEnumerator ProcessSpawnQueue()
    {
        while (spawnQueue.Count > 0)
        {
            SpawnRequest request = spawnQueue.Dequeue();

            for (int i = 0; i < request.Count; i++)
            {
                GameObject dollar = Instantiate(
                    dollarPrefab,
                    request.Origin,
                    Quaternion.identity
                );

                Dollar dollarScript = dollar.GetComponent<Dollar>();
                activeDollarCount++;
                dollarScript.Launch(
                    dollarTarget,
                    request.IsNegative,
                    HandleDollarReachedTarget,
                    HandleDollarDespawned
                );
                PlayDollarSpawnSound();

                if (delayBetweenSpawns > 0f)
                    yield return new WaitForSeconds(delayBetweenSpawns);
                else
                    yield return null;
            }
        }

        spawnQueueCoroutine = null;
    }

    public IEnumerator WaitUntilComplete()
    {
        yield return new WaitUntil(() => !IsBusy);
    }

    private void HandleDollarDespawned()
    {
        activeDollarCount = Mathf.Max(0, activeDollarCount - 1);
    }

    private void HandleDollarReachedTarget(int value)
    {
        DollarReachedTarget?.Invoke(value);
    }

    private void PlayDollarSpawnSound()
    {
        if (dollarSpawnSound == null || dollarSpawnSound.clip == null)
            return;

        dollarSpawnSound.PlayOneShot(dollarSpawnSound.clip);
    }

    private void OnDisable()
    {
        if (spawnQueueCoroutine != null)
            StopCoroutine(spawnQueueCoroutine);

        spawnQueue.Clear();
        spawnQueueCoroutine = null;
    }
}
