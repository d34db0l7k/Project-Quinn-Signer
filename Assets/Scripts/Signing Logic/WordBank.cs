using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class WordBank : MonoBehaviour
{
    protected List<string> workingWords = new List<string>();

    private void Awake()
    {
        workingWords = new List<string>{
            "yellow", "yucky", "dance",
            "yes", "bird", "wolf",
            "later", "better"
        };
        Shuffle(workingWords);
        ConvertToLower(workingWords);
    }

    private void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            int random = Random.Range(i, list.Count);
            string temp = list[i];

            list[i] = list[random];
            list[random] = temp;
        }
    }
    private void ConvertToLower(List<string> list)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            list[i] = list[i].ToLower();
        }
    }
    public string GetWord()
    {
        string newWord = "Out of Words!";

        if (workingWords.Count != 0)
        {
            newWord = workingWords.Last();
        }
        return newWord;
    }
    public string RemoveWord(string currentWord)
    {
        workingWords.Remove(currentWord);
        return GetWord();
    }
    public List<string> GetWordList()
    {
        List<string> wordList = new List<string>();
        for (int i = 0; i < workingWords.Count; ++i) {
            wordList.Add(workingWords[i]);
        }
        return wordList;
    }
}
