using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerOfScenes : MonoBehaviour
{
    public string levelToLoad;
    public int level;
    private AudioManagement audioManagement;

    void Start()
    {
        audioManagement = FindObjectOfType<AudioManagement>();
        if (audioManagement != null)
        {
            if (audioManagement.sourceOfAudio != null)
            {
                audioManagement.sourceOfAudio.Play();
            }
        }
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Current Level", level);
        SceneManager.LoadScene(levelToLoad);
    }

    public void StartLevel(int levelChosen)
    {
        level = levelChosen;
        Play();
        if (audioManagement != null)
        {
            audioManagement.sourceOfAudio.Stop();
        }
    }

    public void Retry()
    {
        StartLevel(level);
    }
}
