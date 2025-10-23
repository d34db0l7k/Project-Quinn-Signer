using UnityEngine;

public class CollectCoin : MonoBehaviour
{
    [SerializeField] AudioClip coinClip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // play the coin sound if assigned
            if (coinClip != null)
                AudioSource.PlayClipAtPoint(coinClip, transform.position);

            // increase the global coin count
            MasterInfo.coinCount += 1;

            // hide or remove the coin
            gameObject.SetActive(false); // or Destroy(gameObject);
        }
    }
}
