using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Main Camera reference")]
    public GameObject mainCamera; // main camera

    [Header("GameObject to spawn")]
    public GameObject enemyPrefab;

    [Header("Words")]
    [SerializeField] private WordBank wordBank;

    [Header("Spawn Timing")]
    public float firstSpawnDelay = 1f;
    public float spawnInterval = 2f;

    [Header("Spawn Positions")]
    public float spawnDistance = 50f; // units in front of the camera
    public Vector2 lateralRangeX = new Vector2(-5, 5);
    public Vector2 lateralRangeY = new Vector2(-3, 3);

    [Header("Lock‑In Slots (camera‑local)")]
    public Vector3 slotTopLeft;
    public Vector3 slotCenter;
    public Vector3 slotTopRight;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private Vector3[] slotOffsets;

    void Start()
    {

        // auto-find WordBank
        if (!wordBank) wordBank = FindAnyObjectByType<WordBank>();
        // pack offsets
        slotOffsets = new[]{slotTopLeft, slotCenter, slotTopRight};
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner needs an enemyPrefab");
            enabled = false;
            return;
        }
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            // clear defeated enemies
            activeEnemies.RemoveAll(e => e == null);


            // if we have fewer than 3, spawn into the first free slot
            if (activeEnemies.Count < slotOffsets.Length)
            {
                // figure out which slots are taken
                bool[] used = new bool[slotOffsets.Length];
                foreach (var go in activeEnemies)
                {
                    var lockComp = go.GetComponent<EnemyLock>();
                    if (lockComp != null && lockComp.slotIndex >= 0 && lockComp.slotIndex < used.Length)
                        used[lockComp.slotIndex] = true;
                }

                // find first free slot index
                int slot = Array.FindIndex(used, taken => !taken);
                if (slot >= 0)
                {
                    if (!wordBank)
                    {
                        Debug.LogWarning("EnemySpawner: no WordBank, cannot assign words.");
                    }
                    else
                    {
                        var word = wordBank.PopRandomWord();
                        if (!string.IsNullOrEmpty(word) && word != "Out of Words!")
                        {
                            GameObject enemy = SpawnEnemy();
                            // assign its slot to lock into
                            var lockComp = enemy.GetComponent<EnemyLock>();
                            if (lockComp != null)
                            {
                                lockComp.slotIndex = slot;
                                lockComp.lockedLocalOffset = slotOffsets[slot];
                            }
                            var label = enemy.GetComponentInChildren<EnemyLabel>(true);
                            if (label) label.SetWord(word);

                            activeEnemies.Add(enemy);
                        }
                        else
                        {
                            // stop spawning if out of words
                            Debug.Log("WordBank is empty, skipping spawn.");
                        }
                    }
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private GameObject SpawnEnemy()
    {
        var camera = mainCamera;
        Vector3 forward = camera.transform.forward;
        Vector3 basePos = camera.transform.position + forward * spawnDistance;

        // add random lateral jitter (so they fly in with variety)
        basePos += camera.transform.right * UnityEngine.Random.Range(lateralRangeX.x, lateralRangeX.y);
        basePos += camera.transform.up * UnityEngine.Random.Range(lateralRangeY.x, lateralRangeY.y);

        return Instantiate(enemyPrefab, basePos, Quaternion.identity);
    }
}