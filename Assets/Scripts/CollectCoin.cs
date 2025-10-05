using UnityEngine;

public class CollectCoin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))   // optional but recommended
        {
            gameObject.SetActive(false);  // or: Destroy(gameObject);
        }
    }
}
