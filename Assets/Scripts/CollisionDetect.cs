using UnityEngine;

public class CollisionDetect : MonoBehaviour
{
    [SerializeField] GameObject thePlayer;
    void onTriggerEnter(Collider other)
    {
        thePlayer.GetComponent<InfinitePlayerMovement>().enabled = false;
    }
}
