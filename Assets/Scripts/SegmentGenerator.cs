using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SegmentGenerator : MonoBehaviour
{
    public GameObject[] segment;
    [SerializeField] private int zPos = 50;
    [SerializeField] private bool creatingSegment = false;
    [SerializeField] private int segmentNum;

    public Transform player;                 // 👈 drag your player here
    public float deleteBuffer = 100f;        // how far behind before we delete

    private Queue<GameObject> activeSegments = new Queue<GameObject>();

    void Update()
    {
        if (!creatingSegment)
        {   
            creatingSegment = true;
            StartCoroutine(SegmentGeneratorCoroutine());
        }

        // cleanup check
        if (activeSegments.Count > 0)
        {
            GameObject oldest = activeSegments.Peek();
            if (player.position.z - oldest.transform.position.z > deleteBuffer)
            {
                Destroy(activeSegments.Dequeue());
            }
        }
    }

    IEnumerator SegmentGeneratorCoroutine() 
    {
        segmentNum = Random.Range(0, segment.Length);
        GameObject newSeg = Instantiate(segment[segmentNum], new Vector3(0, 0, zPos), Quaternion.identity);
        activeSegments.Enqueue(newSeg);

        zPos += 50;
        // Change how long segment takes to generate
        yield return new WaitForSeconds(5);
        creatingSegment = false;
    }
}
