using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverCountDown : MonoBehaviour
{
    [Header("UI References")]
    public Text countdown_text;
    public Button continue_button;
    [Header("Settings")]
    [Tooltip("Seconds the player has to press Continue")]
    public int press_time_limit;
    [Tooltip("Next scene to play once the continue button is pressed")]
    public string next_scene_name;
    // Internal flag to track if the user has pressed the continue button.
    private bool continue_clicked = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {     
        // start the countdown coroutine
        StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        int time_left = press_time_limit;

        // loop until the counter reaches zero and then quit the game
        while (time_left > 0)
        {
            if (countdown_text != null)
            {
                // update the text with curr count
                countdown_text.text = time_left.ToString();
            }
            yield return new WaitForSeconds(1f);
            time_left--;
        }
        // show 0 on the screen:
        if (countdown_text != null)
            countdown_text.text = "0";

        // If the user has not clicked the button within the time limit, exit
        if (!continue_clicked)
        {
            Debug.Log("Time expired! Exiting application.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    public void OnContinueClicked()
    {
        Debug.Log("::DEBUG:: dipshit dippy and the three muskershits ::DEBUG::");
        continue_clicked = true;
        if (!string.IsNullOrEmpty(next_scene_name))
            SceneManager.LoadScene(next_scene_name);
    }
}
