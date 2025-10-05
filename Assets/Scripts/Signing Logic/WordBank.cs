using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class WordBank : MonoBehaviour
{
    [SerializeField] private TextAsset signsList; // assign within the inspector
    public List<string> WorkingWords { get; private set; } = new();
    private const string NoWordsMessage = "Out of Words!";
    private List<string> allWords = new();

    private void Awake()
    {
        CreateVocabList();
        DeduplicateInPlace(allWords);
        ToLowerInPlace(allWords);
        ResetWorkingWords();
    }

    public void CreateVocabList()
    {
        if (signsList == null) { Debug.LogError("Assign a TextAsset in the Inspector"); return; }
        allWords.Clear();
        ParseLines(signsList.text, allWords);
    }

    public void ResetWorkingWords()
    {
        WorkingWords = new List<string>(allWords);
        ShuffleInPlace(WorkingWords);
    }
    
    private static void ParseLines(string text, List<string> outputList)
    {
        outputList.Clear();
        var lines = text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var rawDoginWord in lines)
        {
            string word = rawDoginWord.Trim();
            if (word.Length == 0 || word.StartsWith("#")) continue; // this allows for commented lines
            outputList.Add(word);
        }
    }
    private void ToLowerInPlace(List<string> list)
    {
        for (int i = 0; i < list.Count; ++i)
            list[i] = list[i].ToLowerInvariant();
    }
    public string GetWord()
    {
        if (WorkingWords.Count == 0) return NoWordsMessage;
        return WorkingWords[^1]; // grabs last without removing
    }
    public string RemoveWord(string currentWord)
    {
        WorkingWords.Remove(currentWord);
        return GetWord();
    }
    public List<string> GetWordList() => new List<string>(WorkingWords);

    /// <summary>
    /// Shuffle list in place
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    private static void ShuffleInPlace<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Remove duplicates while preserving original order
    /// </summary>
    /// <param name="words"></param>
    private static void DeduplicateInPlace(List<string> words)
    {
        var seen = new HashSet<string>();
        for (int i = words.Count - 1; i >= 0; i--)
            if (!seen.Add(words[i])) words.RemoveAt(i);
    }

    /// <summary>
    /// Returns a NEW list with words starting with the given letter (case-insensitive)
    /// </summary>
    /// <param name="letter"></param>
    /// <returns></returns>
    public List<string> FilterByStartingLetter(char letter) =>
        FilterByPrefix(letter.ToString());

    /// <summary>
    /// Returns a NEW list with words that start with the given prefix (case-insensitive)
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public List<string> FilterByPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return new List<string>(WorkingWords);
        return WorkingWords
            .Where(w => w.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Returns a NEW list with words whose lengths are within [minLen, maxLen] inclusive
    /// </summary>
    /// <param name="minLen"></param>
    /// <param name="maxLen"></param>
    /// <returns></returns>
    public List<string> FilterByLength(int minLen, int maxLen)
    {
        if (minLen < 0) minLen = 0;
        if (maxLen < minLen) (minLen, maxLen) = (maxLen, minLen);
        return WorkingWords.Where(w => w.Length >= minLen && w.Length <= maxLen).ToList();
    }

    /// <summary>
    /// Pop and return a random word from WorkingWords (removes it)
    /// </summary>
    /// <returns></returns>
    public string PopRandomWord()
    {
        if (WorkingWords.Count == 0) return NoWordsMessage;
        int index = Random.Range(0, WorkingWords.Count);
        string w = WorkingWords[index];
        WorkingWords.RemoveAt(index);
        return w;
    }

    /// <summary>
    /// Get N random words. If unique=true then pick without replacement; else with replacement
    /// </summary>
    /// <param name="count"></param>
    /// <param name="unique"></param>
    /// <returns></returns>
    public List<string> GetRandomWords(int count, bool unique = true)
    {
        count = Mathf.Max(0, count);
        if (count == 0 || WorkingWords.Count == 0) return new List<string>();

        if (unique)
        {
            var copy = new List<string>(WorkingWords);
            ShuffleInPlace(copy);
            if (count >= copy.Count) return copy;
            return copy.GetRange(0, count);
        }
        else
        {
            var picks = new List<string>(count);
            for (int i = 0; i < count; i++)
                picks.Add(WorkingWords[Random.Range(0, WorkingWords.Count)]);
            return picks;
        }
    }

    /// <summary>
    /// Retrieve single random word. Returns "Out of Words!" if none
    /// </summary>
    /// <param name="unique"></param>
    /// <returns></returns>
    public string GetRandomWord(bool unique = true)
    {
        var list = GetRandomWords(1, unique);
        return list.Count == 0 ? NoWordsMessage : list[0];
    }
}
