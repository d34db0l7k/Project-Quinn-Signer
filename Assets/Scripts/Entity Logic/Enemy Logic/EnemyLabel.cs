using UnityEngine;
using UnityEngine.UI;
public class EnemyLabel : MonoBehaviour
{
    public Text label;
    [HideInInspector] public string targetWord;

    [SerializeField] private string defaultText = "Word to Sign";
    private bool explicitlySet;

    private void Start()
    {
        // Only set default if nothing explicit yet
        if (!explicitlySet && label) label.text = defaultText;
    }

    public void SetWord(string word)
    {
        targetWord = word;
        explicitlySet = true;
        if (label) label.text = word;
        else Debug.LogWarning($"Missing Text on {name}");
    }
}
