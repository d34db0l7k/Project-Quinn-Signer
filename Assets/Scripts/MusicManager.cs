using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Assign your tracks here")]
    public AudioClip titleTheme;
    public AudioClip gameplayTheme;
    public AudioClip gameOverTheme;

    private AudioSource audioSource;

    void Awake()
    {
        // enforce singleton & persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // swap based on scene name (or buildIndex, tag, etc)
        if (scene.name == "MainMenu")
            PlayTheme(titleTheme);
        else if (scene.name == "MainScene")
            PlayTheme(gameplayTheme);
        else
            PlayTheme(gameOverTheme);
    }

    void PlayTheme(AudioClip clip)
    {
        if (clip == null) return;
        // if already playing this clip, do nothing
        if (audioSource.clip == clip && audioSource.isPlaying) return;
        audioSource.clip = clip;
        audioSource.Play();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
