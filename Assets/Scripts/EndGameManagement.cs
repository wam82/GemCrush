using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum RequirementType
{
    Moves,
    Time
}

[Serializable]
public class EndGameRequirements
{
    public RequirementType type;
    public int counterValue;
}
public class EndGameManagement : MonoBehaviour
{
    public EndGameRequirements moveRequirement;

    public EndGameRequirements timeRequirement;

    public Text movesCounter;

    public Text timeCounter;

    private int timeCounterValue;

    private int movesCounterValue;

    private float timer;

    public Board board;

    public GameObject winPanel;

    public GameObject losePanel;

    public bool stopTime = false;

    public Button pauseButton;

    public Text loseScore;

    public Text winScore;

    private ScoreManagement scoreManager;

    public GameObject winStar1;
    
    public GameObject winStar2;

    public GameObject winStar3;
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        scoreManager = FindObjectOfType<ScoreManagement>();
        SetupGame();
    }

    void SetupGame()
    {
        timeCounterValue = timeRequirement.counterValue;
        movesCounterValue = moveRequirement.counterValue;
        movesCounter.text = movesCounterValue.ToString();
        timeCounter.text = timeCounterValue.ToString();
        timer = 1;
    }

    void DecreaseTime()
    {
        // Debug.Log("Called when gameState = " + board.currentState);
        if ((board.currentState == GameState.move || board.currentState == GameState.wait))
        {
            if (timeCounterValue >= 1)
            {
                timeCounterValue--;
                timeCounter.text = timeCounterValue.ToString();
            }
            else
            {
                LoseGameByTime();
            }
        }
    }

    public void DecreaseMove()
    {
        movesCounterValue--;
        movesCounter.text = movesCounterValue.ToString();
        if (movesCounterValue <= 0 && board.fillState == FillState.none)
        {
            LoseGameByMoves();
        }
    }

    public void WinGame()
    {
        winScore.text = scoreManager.score.ToString();
        if (scoreManager.GetStars() == 1)
        {
            winStar1.SetActive(true);
        }
        else if (scoreManager.GetStars() == 2)
        {
            winStar1.SetActive(true);
            winStar2.SetActive(true);
        }
        else if (scoreManager.GetStars() == 0)
        {
            
        }
        else
        {
            winStar1.SetActive(true);
            winStar2.SetActive(true);
            winStar3.SetActive(true);
        }
        winPanel.SetActive(true);
        board.currentState = GameState.win;
    }

    public void LoseGameByTime()
    {
        if (pauseButton != null)
        {
            pauseButton.interactable = false;
        }

        StartCoroutine(CheckStatusTime());
        // Debug.Log("Lost to time");
    }
    public void LoseGameByMoves()
    {
        if (pauseButton != null)
        {
            pauseButton.interactable = false;
        }
        movesCounterValue = 0;
        movesCounter.text = movesCounterValue.ToString();
        StartCoroutine(CheckStatusMoves());
        // Debug.Log("Lost to moves");
    }

    public void StopTime()
    {
        stopTime = true;
    }

    public void StartTime()
    {
        stopTime = false;
    }

    private IEnumerator CheckStatusMoves()
    {
        yield return new WaitUntil(() => board.currentState != GameState.wait);
        yield return new WaitUntil(() => board.fillState == FillState.none);
        board.goalManager.updateGoals();
        if (board.currentState != GameState.win)
        {
            loseScore.text = scoreManager.score.ToString();
            losePanel.SetActive(true);
            board.currentState = GameState.lose;
            StopTime();
        }
    }

    private IEnumerator CheckStatusTime()
    {
        yield return new WaitUntil(() => board.currentState != GameState.wait);
        yield return new WaitUntil(() => board.fillState == FillState.none);
        board.goalManager.updateGoals();
        if (board.currentState != GameState.win)
        {
            loseScore.text = scoreManager.score.ToString();
            losePanel.SetActive(true);
            board.currentState = GameState.lose;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!stopTime)
        {
            timer -= Time.deltaTime;
            if (timer <= 0 )
            {
                DecreaseTime();
                timer = 1;
            } 
        }
    }
}
