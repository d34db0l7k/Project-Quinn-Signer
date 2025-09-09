using UnityEngine;
using UnityEngine.UI;
public class EnemyLabel : MonoBehaviour
{
    public Text label;            // legacy UI.Text above the ship
    [HideInInspector] 
    public string targetWord;     // the word they must sign/type

    public void SetWord(string w)
    {
        Debug.Log("This word was just added" + w);
        targetWord = w;
        label.text  = w;
    }
}
