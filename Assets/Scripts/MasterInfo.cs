using UnityEngine;
using UnityEngine.UI;

public class MasterInfo : MonoBehaviour
{
    public static int coinCount = 0;
    public static int score = 0;

    [SerializeField] Text coinDisplay;
    [SerializeField] Text scoreDisplay;
    [SerializeField] Transform player;          // assign your Player here
    [SerializeField] float pointsPerMeter = 1f; // how many points per unit

    float startZ;

    void Start()
    {
        if (player != null)
            startZ = player.position.z; // assumes forward is +Z

        UpdateUI();
    }

    void Update()
    {
        if (player == null) return;

        // calculate distance since start
        float distance = Mathf.Max(0f, player.position.z - startZ);
        int newScore = Mathf.FloorToInt(distance * pointsPerMeter);

        if (newScore != score)
        {
            score = newScore;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (coinDisplay)
            coinDisplay.text = "CRYSTALS: " + coinCount;

        if (scoreDisplay)
            scoreDisplay.text = "DISTANCE: " + score;
    }
}
