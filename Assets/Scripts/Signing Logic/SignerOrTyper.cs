using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Engine;
using Common;
using UnityEngine.Rendering;

public class SignerOrTyper : MonoBehaviour
{
    [Header("On = typed, Off = signed")]
    public bool typed;

    [Header("SLRTK Overhead")]
    public SimpleExecutionEngine engine;

    [Space]

    [Header("Imports")]
    public WordBank wordBank = null;
    public Text wordOutput = null;
    public List<GameObject> enemies;
    public Text scoreText = null;
    public Text inferenceText = null;
    public Image background = null;

    // Local vars
    private int score = 0;
    private string remainingWord = string.Empty;
    private string currentWord = string.Empty;
    private string signedWord = string.Empty;
    private int potentialPoints = 0;
    // variable for filters and callbacks to execute only once
    private bool hasExecuted = false;
    List<string> filterWords = new List<string> { };

    private bool signingActive = false;

    private void Awake()
    {
        background.color = Color.black;
        engine.Toggle();
    }
    private IEnumerator ForceEngineIdleAtLaunch()
    {
        yield return null;                 // let SimpleExecutionEngine.Start() run
        if (engine == null) yield break;

        engine.Toggle();                   // hide preview (engine showed it in Start)
        signingActive = false;
        if (background) background.color = Color.black;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // current word to be checked against for sign and typed letter
        currentWord = wordBank.GetWord();
        // for the purpose of having a starting word for type matching
        remainingWord = currentWord;
        // what is shown to the user on the HUD as the word to sign/type
        wordOutput.text = "Target: " + currentWord;
        //init points for first word
        potentialPoints = (currentWord.Length / 3) + 1;
        // set score to be "score: " initially on start up
        UpdateScoreText();
        // set up text fields of enemies
        if (enemies.Count != wordBank.GetWordList().Count)
        {
            Debug.LogWarning("Number of enemies does not match the number of words");
        }
        int i = 0;
        List<string> words = wordBank.GetWordList();
        words.Reverse();
        foreach (string word in words)
        {
            if (i == enemies.Count)
                break;
            enemies[i].GetComponent<EnemyLabel>().SetWord(word);
            ++i;
        }
        StartCoroutine(ForceEngineIdleAtLaunch());
    }

    // Update is called once per frame
    private void Update()
    {
        if (!hasExecuted)
        {
            // where initialization goes
            engine.recognizer.AddCallback("check", sign => signedWord = sign);
            engine.recognizer.outputFilters.Clear();

            // Deep copy of word list for non stale FocusSublistFilter
            List<string> wordList = wordBank.GetWordList();
            for (int i = 0; i < wordList.Count; ++i)
            {
                filterWords.Add(wordList[i]);
            }

            engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(filterWords));
            hasExecuted = true;
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            string output = "";
            foreach (string word in filterWords)
            {
                output += word + "|**|";
            }
            Debug.Log("Hello i am working");
            Debug.Log(output);
        }
        if (typed)
        {
            // get user's key input
            CheckTypedInput();
        }
        else
        {
            // get user's signing attempt
            UserSigning();
        }
    }

    private void SetCurrentWord(string newWord)
    {
        // keep the switching between typing and signing consistent
        remainingWord = newWord;

        currentWord = newWord;
        signedWord = string.Empty;
        potentialPoints = (currentWord.Length / 3) + 1;
        wordOutput.text = "Target: " + currentWord;
    }
    private void SetCurrentWord()
    {
        SetRemainingWord(wordBank.RemoveWord(currentWord));
        currentWord = remainingWord;
    }

    // Functions to handle typed behavior
    private void CheckTypedInput()
    {
        if (Input.anyKeyDown)
        {
            string keyPressed = Input.inputString;

            if (keyPressed.Length == 1)
                EnterLetter(keyPressed);
        }
    }
    private void EnterLetter(string typedLetter)
    {
        if (IsCorrectLetter(typedLetter))
        {
            RemoveLetter();
            if (IsWordComplete())
            {
                AddScore(potentialPoints);
                SetCurrentWord();
            }
        }
    }
    private bool IsCorrectLetter(string letter)
    {
        return remainingWord.IndexOf(letter) == 0;
    }
    private void RemoveLetter()
    {
        string newString = remainingWord.Remove(0, 1);
        SetRemainingWord(newString);
    }
    private bool IsWordComplete()
    {
        return remainingWord.Length == 0;
    }
    private void SetRemainingWord(string newString)
    {
        remainingWord = newString;
        wordOutput.text = remainingWord;
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
    // Sign Language Words
    private void CheckCorrectSign()
    {
        if (signedWord == currentWord)
        {
            Debug.Log("Signed correct word: " + signedWord);
            inferenceText.text = signedWord;
            inferenceText.color = Color.green;
            OnWordMatched(signedWord);
        }
        else
        {
            Debug.Log("Signed incorrect word: " + signedWord);
            inferenceText.text = signedWord;
            inferenceText.color = Color.red;
            signedWord = "Try again";
        }
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

            // check if the user's sign matches the current word
            CheckCorrectSign();
        }
    }

    private void OnWordMatched(string word)
    {
        AddScore(potentialPoints);
        if (typed)
            SetCurrentWord();
        else
            SetCurrentWord(wordBank.RemoveWord(signedWord));

        enemies.RemoveAll(e => e == null);
        // find enemy with the matching word
        foreach (GameObject enemy in enemies)
        {
            if (enemy.GetComponent<EnemyLabel>().targetWord == word)
            {
                var controller = enemy.GetComponent<EnemyController>();
                if (controller != null)
                    controller.Explode();
                break;
            }
        }
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

    public void EndMobileSign() {
        engine.buffer.TriggerCallbacks();
        engine.Toggle();
        background.color = Color.black;

        if (engine != null && signingActive)
        {
            signingActive = false;
            engine.enabled = false;
        }

        CheckCorrectSign();
    }
}
