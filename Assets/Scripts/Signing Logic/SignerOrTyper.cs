using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Engine;
using Common;
using UnityEngine.SceneManagement;

public class SignerOrTyper : MonoBehaviour
{
    [Header("SLRTK Overhead")]
    public SimpleExecutionEngine engine;

    [Space]

    [Header("Plug in below from editor")]
    public WordBank wordBank = null;
    public Text scoreText = null;
    public Text inferenceText = null;
    public Image background = null;

    [Header("Win vars")]
    [SerializeField] private string winSceneName = "";

    // Local vars
    private int score = 0;
    private string remainingWord = string.Empty;
    private string currentWord = string.Empty;
    private string signedWord = string.Empty;
    private int potentialPoints = 0;
    // variable for filters and callbacks to execute only once
    private bool hasExecuted = false;
    private SceneBindings bindings;
    List<string> filterWords = new List<string> { };

    private bool signingActive = false;

    private void Awake()
    {
        if (background) background.color = Color.black;
        if (engine) engine.Toggle();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // current word to be checked against for sign and typed letter
        currentWord = wordBank.GetWord();
        // for the purpose of having a starting word for type matching
        remainingWord = currentWord;
        //init points for first word
        potentialPoints = (currentWord.Length / 3) + 1;
        // set score to be "score: " initially on start up
        UpdateScoreText();
        // set up text fields of enemies
        StartCoroutine(AssignEnemyLabelsWhenReady());
        StartCoroutine(ForceEngineIdleAtLaunch());
    }

    // Update is called once per frame
    private void Update()
    {
        if (!hasExecuted && engine != null)
        {
            // where initialization goes
            engine.recognizer.AddCallback("check", OnSignRecognized);
            engine.recognizer.outputFilters.Clear();

            hasExecuted = true;
        }
        UserSigning();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bindings = FindFirstObjectByType<SceneBindings>(FindObjectsInactive.Include);

        if (!bindings)
        {
            // Not every scene will have bindings (e.g., defeat screen). That's OK.
            Debug.Log($"[SignerOrTyper] No SceneBindings found in scene '{scene.name}' (mode={mode}). Skipping rebind.");
            hasExecuted = false;        // so we re-init next time there *is* a scene with bindings
            filterWords.Clear();
            return;
        }

        wordBank = bindings.wordBank;
        engine = bindings.engine;
        scoreText = bindings.scoreText;
        inferenceText = bindings.inferenceText;
        background = bindings.background;

        hasExecuted = false;

        if (wordBank) wordBank.ResetWorkingWords(); // FRESH POOL OF WORDS FOR EACH SCENE

        // UI/engine initial state
        if (background) background.color = Color.black;
        if (engine && !engine.enabled) engine.enabled = false;

        InitializeHUDAndWord();
        StartCoroutine(AssignEnemyLabelsWhenReady());
        // StartCoroutine(ForceEngineIdleAtLaunch());
    }
    private void InitializeHUDAndWord()
    {
        currentWord = wordBank ? wordBank.GetWord() : "wordbank is missing";
        remainingWord = currentWord;
        potentialPoints = (currentWord.Length / 3) + 1;
        UpdateScoreText();
    }
    private IEnumerator AssignEnemyLabelsWhenReady()
    {
        // Wait a couple frames to let spawners finish
        yield return null;
        yield return null;

        // Grab whatever is in-scene right now
        var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);

        if (enemyLabels == null || enemyLabels.Length == 0)
        {
            Debug.LogWarning("No EnemyLabel found in scene.");
            yield break;
        }

        // Pull a set of unique words (or allow repeats if you prefer)
        filterWords = wordBank.GetRandomWords(enemyLabels.Length, unique: true);
        engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(filterWords));
        for (int i = 0; i < enemyLabels.Length && i < filterWords.Count; i++)
        {
            SafeSetEnemyWord(enemyLabels[i], filterWords[i]);
        }

        if (engine != null)
        {
            engine.recognizer.outputFilters.Clear();
            engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(filterWords));
        }
    }
    private static void SafeSetEnemyWord(EnemyLabel label, string word)
    {
        if (!label) return;
        if (!label.label)
        {
            Debug.LogWarning($"EnemyLabel on {label.gameObject.name} has no UI Text assigned.");
            return;
        }
        label.SetWord(word);
    }
    private IEnumerator ForceEngineIdleAtLaunch()
    {
        yield return null;                 // let SimpleExecutionEngine.Start() run
        if (engine == null) yield break;

        engine.Toggle();                   // hide preview (engine showed it in Start)
        signingActive = false;
        if (background) background.color = Color.black;

    }
    // Functions to handle scoring behavior
    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
    }
    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
    private void UserSigning()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (engine != null && !signingActive)
            {
                engine.enabled = true;
                signingActive = true;
            }
            background.color = Color.white;
            engine.Toggle();
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            engine.buffer.TriggerCallbacks();
            engine.Toggle();
            background.color = Color.black;

            if (engine != null && signingActive)
            {
                signingActive = false;
                engine.enabled = false;
            }
        }
    }
    private void OnSignRecognized(string rawInput)
    {
        Debug.Log($"[OnSignRecognized] raw='{rawInput}' cleaned='{(rawInput ?? "").Trim().ToLowerInvariant()}'");
        string signed = (rawInput ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(signed))
        {
            if (inferenceText) { inferenceText.text = rawInput; inferenceText.color = Color.red; }
            return;
        }

        // find a matching enemy label by word (case-insensitive)
        var labels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
        EnemyLabel match = null;
        foreach (var label in labels)
        {
            if (!label) continue;
            if (string.Equals(label.targetWord, signed, System.StringComparison.OrdinalIgnoreCase))
            {
                match = label;
                break;
            }
        }

        if (!match)
        {
            if (inferenceText) { inferenceText.text = signed; inferenceText.color = Color.red; }
            return;
        }

        // explode that enemy + award points based on the matched word length
        int pts = Mathf.Max(1, (match.targetWord.Length / 3) + 1);
        AddScore(pts);

        var controller = match.GetComponentInParent<EnemyController>() ?? match.GetComponent<EnemyController>();
        if (controller) controller.Explode();
        else Destroy(match.gameObject);

        if (inferenceText) { inferenceText.text = signed; inferenceText.color = Color.green; }

        // remove signed word
        RemoveWordFromList(signed);
        StartCoroutine(CheckForWinNextFrame());
    }
    /*for mobile signing button usage*/
    public void BeginMobileSign()
    {
        if (engine != null && !signingActive)
        {
            engine.enabled = true;
            signingActive = true;
        }

        background.color = Color.white;
        engine.Toggle();
    }
    public void EndMobileSign()
    {
        engine.buffer.TriggerCallbacks();
        engine.Toggle();
        background.color = Color.black;

        if (engine != null && signingActive)
        {
            signingActive = false;
            engine.enabled = false;
        }
    }

    /*------------------------Win State--------------------------------*/
    private void RemoveWordFromList(string word)
    {
        if (string.IsNullOrEmpty(word)) return;
        string key = word.Trim().ToLowerInvariant();

        // Remove one instance of the word from filterWords (they were unique anyway)
        for (int i = 0; i < filterWords.Count; i++)
        {
            if (filterWords[i] == key)
            {
                filterWords.RemoveAt(i);
                break;
            }
        }

        // Rebuild recognizer filter with the remaining words
        if (engine != null)
        {
            engine.recognizer.outputFilters.Clear();
            if (filterWords.Count > 0)
                engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(filterWords));
        }
    }

    private IEnumerator CheckForWinNextFrame()
    {
        // wait one frame so destroyed enemies are actually gone
        yield return null;

        // If nothing left to check (filterWords empty OR wordBank out) AND no EnemyLabels remain → win
        bool noWordsLeft = (filterWords == null || filterWords.Count == 0) || (wordBank && wordBank.GetWordList().Count == 0);

        var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
        bool noEnemiesLeft = enemyLabels == null || enemyLabels.Length == 0;

        if (noWordsLeft && noEnemiesLeft)
            TriggerWin();
    }

    private void TriggerWin()
    {
        if (!string.IsNullOrEmpty(winSceneName))
        {
            SceneManager.LoadScene(winSceneName, LoadSceneMode.Single);
            return;
        }
        else
        {
            Debug.Log("[Win] All ships destroyed and no words left!");
        }
    }

}
