using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public string sceneToLoad;
    public GameObject panelToLoad;
    public Canvas background;
    private Board board;
    private EndGameManagement endGame;
    private GameState previousState;
    private FillState previousFillState;

    private void Start()
    {
        board = FindObjectOfType<Board>();
        endGame = FindObjectOfType<EndGameManagement>();
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoadPausePanel()
    {
        panelToLoad.SetActive(true);
        previousState = board.currentState;
        previousFillState = board.fillState;
        board.audioSource.Pause();
        board.currentState = GameState.pause;
        endGame.StopTime();
        if (panelToLoad.activeInHierarchy)
        {
            if (background != null)
            {
                background.sortingOrder = 2;
            }
        }
        // Debug.Log(board.fillState);
    }

    public void ResumeGame()
    {
        panelToLoad.SetActive(false);
        board.audioSource.Play();
        if (!panelToLoad.activeInHierarchy)
        {
            if (background != null)
            {
                background.sortingOrder = -3;
            }
        }
        board.currentState = previousState;
        board.firstPlaced = board.firstPlaceState;
        endGame.StartTime();
        if (previousState == GameState.wait)
        {
            board.PickBackUp(previousFillState);
        }
    }
}
