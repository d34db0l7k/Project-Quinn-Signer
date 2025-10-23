using UnityEngine;

public class SpaceshipCollision : MonoBehaviour
{
    [Header("Explosion FX Prefabs")]
    public GameObject explosionEffect;      // prefab with particle effect
    public GameObject bigExplosionEffect;   // prefab as a big explosion
    public AudioClip explosionSound;

    [Header("End the Game")]
    public string nextSceneName; // next scene name to load (end game)
    public float sceneDelay;

    private bool hasCollided = false;

    void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided && collision.gameObject.CompareTag("Enemy"))
        {
            ExplodePlayer();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasCollided && other.CompareTag("Wall"))
        {
            ExplodePlayer();
        }
    }

    void ExplodePlayer()
    {
        hasCollided = true; //runs once

        // 1) initial big short explosion
        if (bigExplosionEffect != null)
            Instantiate(bigExplosionEffect, transform.position, Quaternion.identity);

        // 2) debris or smoke effect
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        SceneSwitcher.Instance?.SwitchSceneAfterDelay(nextSceneName, sceneDelay);
        Destroy(gameObject);
    }
}