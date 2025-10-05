using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SegmentGenerator : MonoBehaviour
{
    [Header("Segment Prefabs")]
    public GameObject[] segmentPrefabs;

    [Header("Layout")]
    [Tooltip("Exact world length of each segment along +Z.")]
    public float segmentLength = 50f;

    [Tooltip("How many segments should always be ahead of the player.")]
    public int segmentsAhead = 6;

    [Header("Player / Cleanup")]
    public Transform player;                  // drag your player here
    [Tooltip("Delete a segment after player is this far past the segment END.")]
    public float deleteBuffer = 100f;

    [Header("Hierarchy (optional)")]
    [Tooltip("Parent object that holds all spawned segments. Also scan its children at Start as pre-placed segments.")]
    public Transform segmentsParent;

    // --- Internal state ---
    private readonly Queue<GameObject> activeSegments = new Queue<GameObject>();
    private float nextSpawnZ = 0f;            // z where the next segment will spawn

    void Start()
    {
        if (segmentLength <= 0f) segmentLength = 50f;

        // 1) Ingest any pre-placed segments (e.g., you already have 2 ahead)
        if (segmentsParent != null)
        {
            var children = new List<Transform>();
            for (int i = 0; i < segmentsParent.childCount; i++)
                children.Add(segmentsParent.GetChild(i));

            // sort by startZ so cleanup order is correct
            foreach (var t in children.OrderBy(c => c.position.z))
                activeSegments.Enqueue(t.gameObject);

            if (children.Count > 0)
            {
                // continue spawning after the last pre-placed segment
                float lastStartZ = children.Max(c => c.position.z);
                nextSpawnZ = lastStartZ + segmentLength;
            }
        }

        // 2) If nothing pre-placed, start just ahead of the player
        if (activeSegments.Count == 0)
        {
            // align to segment grid just past player
            float playerZ = player != null ? player.position.z : 0f;
            nextSpawnZ = Mathf.Floor(playerZ / segmentLength + 1f) * segmentLength;
        }

        // 3) Prewarm: ensure we already have N segments ahead right now
        EnsureSegmentsAhead();
    }

    void Update()
    {
        if (player == null || segmentPrefabs == null || segmentPrefabs.Length == 0)
            return;

        // Keep enough segments ahead based on the player's current position
        EnsureSegmentsAhead();

        // Cleanup: remove ALL segments that are far behind (by segment END)
        while (activeSegments.Count > 0)
        {
            GameObject oldest = activeSegments.Peek();
            if (oldest == null) { activeSegments.Dequeue(); continue; }

            float startZ = oldest.transform.position.z;
            float endZ   = startZ + segmentLength;

            if (player.position.z - endZ > deleteBuffer)
                Destroy(activeSegments.Dequeue());
            else
                break; // the rest are newer
        }
    }

    private void EnsureSegmentsAhead()
    {
        // Make sure the last spawn position is at least 'segmentsAhead' segments ahead of the player
        float targetFrontZ = player.position.z + segmentsAhead * segmentLength;

        while (nextSpawnZ < targetFrontZ)
            SpawnNext();
    }

    private void SpawnNext()
    {
        int idx = Random.Range(0, segmentPrefabs.Length);
        Vector3 pos = new Vector3(0f, 0f, nextSpawnZ);
        GameObject seg = Instantiate(segmentPrefabs[idx], pos, Quaternion.identity,
                                     segmentsParent != null ? segmentsParent : null);
        activeSegments.Enqueue(seg);
        nextSpawnZ += segmentLength;
    }
}
