using System.Collections.Generic;
using UnityEngine;

public class DevKillKey : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode killKey = KeyCode.K;
    public bool addScore = true;

    [Tooltip("Optional: assign your SignerOrTyper to award points & win check; will auto-find if null.")]
    public SignerOrTyper signerOrTyper;

    void Awake()
    {
        if (!signerOrTyper) signerOrTyper = FindFirstObjectByType<SignerOrTyper>(FindObjectsInactive.Include);
    }

    void Update()
    {
        if (Input.GetKeyDown(killKey))
            KillOneEnemy();
    }

    void KillOneEnemy()
    {
        // Grab all enemy labels
        EnemyLabel[] labels;

        labels = FindObjectsByType<EnemyLabel>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (labels == null || labels.Length == 0)
        {
            Debug.Log("[DevKillKey] No enemies found.");
            return;
        }

        // Pick the first non-null label
        EnemyLabel target = null;
        foreach (var l in labels)
        {
            if (l != null) { target = l; break; }
        }
        if (!target)
        {
            Debug.Log("[DevKillKey] All labels were null.");
            return;
        }

        // scoring using same rule as signing
        if (addScore && signerOrTyper && !string.IsNullOrEmpty(target.targetWord))
        {
            int pts = Mathf.Max(1, (target.targetWord.Length / 3) + 1);
            signerOrTyper.AddScore(pts);
        }

        // Explode / destroy
        var controller = target.GetComponentInParent<EnemyController>() ?? target.GetComponent<EnemyController>();
        if (controller) controller.Explode();
        else Destroy(target.gameObject);

        // remove word from filters and check win
        if (signerOrTyper)
            signerOrTyper.HandleEnemyKilled(target);
        else
            Debug.LogWarning("[DevKillKey] signerOrTyper not set—win check/word removal skipped.");
    }
}
