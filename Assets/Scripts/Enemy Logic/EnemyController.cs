using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject explosionEffect;
    private AudioSource _audio;

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
