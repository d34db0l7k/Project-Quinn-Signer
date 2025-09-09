using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Tooltip("Optional explosion prefab to play when this enemy dies")]
    public GameObject explosionEffect;
    private AudioSource _audio;

    /// <summary>
    /// Call this to make the enemy blow up.
    /// </summary>

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }
    public void Explode()
    {
        if (_audio != null)
            _audio.Play(); // sounding

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
