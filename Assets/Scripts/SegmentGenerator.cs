using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

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
    public Transform player;                  // drag your player here (optional; will auto-find)
    [Tooltip("Delete a segment after player is this far past the segment END.")]
    public float deleteBuffer = 100f;

    [Header("Hierarchy (optional)")]
    [Tooltip("Parent object that holds all spawned segments. If empty, this.transform will be used.")]
    public Transform segmentsParent;

    // --- Internal state ---
    private readonly Queue<GameObject> activeSegments = new Queue<GameObject>();
    private float nextSpawnZ = 0f;
    private bool initialized = false;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        InitializeIfNeeded();
    }

    void Update()
    {
        if (!player || segmentPrefabs == null || segmentPrefabs.Length == 0)
        {
            // Try to recover player mid-play if it was recreated
            if (!player) AutoFindPlayer();
            if (!player) return; // still nothing; skip this frame
        }
        EnsureSegmentsAhead();
        // Cleanup: remove segments far behind
        while (activeSegments.Count > 0)
        {
            GameObject oldest = activeSegments.Peek();
            if (!oldest) { activeSegments.Dequeue(); continue; }

            float startZ = oldest.transform.position.z;
            float endZ = startZ + segmentLength;

            if (player.position.z - endZ > deleteBuffer)
            {
                activeSegments.Dequeue();
                Destroy(oldest);
            }
            else break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fresh scene: rebind and reset generator state
        initialized = false;
        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (initialized) return;

        if (segmentLength <= 0f) segmentLength = 50f;
        if (!segmentsParent) segmentsParent = transform;           // safe default parent
        if (!player) AutoFindPlayer();

        // Clear old state (in case this object survived a scene change)
        ClearQueueAndDestroyChildrenNotInScene();

        // Ingest any pre-placed children under segmentsParent
        var children = new List<Transform>();
        for (int i = 0; i < segmentsParent.childCount; i++)
            children.Add(segmentsParent.GetChild(i));

        activeSegments.Clear();
        foreach (var t in children.OrderBy(c => c.position.z))
            activeSegments.Enqueue(t.gameObject);

        if (children.Count > 0)
        {
            float lastStartZ = children.Max(c => c.position.z);
            nextSpawnZ = lastStartZ + segmentLength;
        }
        else
        {
            float playerZ = player ? player.position.z : 0f;
            nextSpawnZ = Mathf.Floor(playerZ / segmentLength + 1f) * segmentLength;
        }

        EnsureSegmentsAhead();   // prewarm
        initialized = true;
    }

    private void AutoFindPlayer()
    {
        // Try by tag first
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go) { player = go.transform; return; }

        // Fallback: look for common movement script in the scene
        var mover = FindFirstObjectByType<InfinitePlayerMovement>();
        if (mover) player = mover.transform;
    }

    private void ClearQueueAndDestroyChildrenNotInScene()
    {
        // Clear the queue
        activeSegments.Clear();
    }

    private void EnsureSegmentsAhead()
    {
        if (!player) return;

        float targetFrontZ = player.position.z + segmentsAhead * segmentLength;
        while (nextSpawnZ < targetFrontZ)
            SpawnNext();
    }

    private void SpawnNext()
    {
        if (segmentPrefabs == null || segmentPrefabs.Length == 0) return;

        int idx = Random.Range(0, segmentPrefabs.Length);
        Vector3 pos = new Vector3(0f, 0f, nextSpawnZ);

        var seg = Instantiate(segmentPrefabs[idx], pos, Quaternion.identity, segmentsParent);
        activeSegments.Enqueue(seg);
        nextSpawnZ += segmentLength;
    }
}
