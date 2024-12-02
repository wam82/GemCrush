using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalManagement : MonoBehaviour
{
    public int blueNeeded = 5;

    public int blueCollected = 0;
    
    public Text blueText;

    public int greenNeeded = 5;

    public int greenCollected = 0;
    
    public Text greenText;
    
    public int purpleNeeded = 5;

    public int purpleCollected = 0;
    
    public Text purpleText;
    
    public int redNeeded = 5;

    public int redCollected = 0;
    
    public Text redText;
    
    public int yellowNeeded = 5;

    public int yellowCollected = 0;
    
    public Text yellowText;

    private EndGameManagement endGameManager;
    // Start is called before the first frame update
    void Start()
    {
        endGameManager = FindObjectOfType<EndGameManagement>();
    }

    // Update is called once per frame
    void Update()
    {
        // blueText.text = (blueNeeded - blueCollected).ToString();
        // greenText.text = (greenNeeded - greenCollected).ToString();
        // purpleText.text = (purpleNeeded - purpleCollected).ToString();
        // redText.text = (redNeeded - redCollected).ToString();
        // yellowText.text = (yellowNeeded - yellowCollected).ToString();
    }

    public void updateGoals()
    {
        int goalsCompleted = 0;
        blueText.text = (blueNeeded - blueCollected).ToString();
        greenText.text = (greenNeeded - greenCollected).ToString();
        purpleText.text = (purpleNeeded - purpleCollected).ToString();
        redText.text = (redNeeded - redCollected).ToString();
        yellowText.text = (yellowNeeded - yellowCollected).ToString();

        if (blueCollected >= blueNeeded)
        {
            goalsCompleted++;
            blueText.text = "0";
        }
        if (greenCollected >= greenNeeded)
        {
            goalsCompleted++;
            greenText.text = "0";
        }
        if (redCollected >= redNeeded)
        {
            goalsCompleted++;
            redText.text = "0";
        }
        if (purpleCollected >= purpleNeeded)
        {
            goalsCompleted++;
            purpleText.text = "0";
        }
        if (yellowCollected >= yellowNeeded)
        {
            goalsCompleted++;
            yellowText.text = "0";
        }

        if (goalsCompleted >= 5)
        {
            if (endGameManager != null)
            {
                if (endGameManager.pauseButton != null)
                {
                    endGameManager.pauseButton.interactable = false;
                }

                StartCoroutine(CheckStatus());
            }
            // Debug.Log("Level Completed");
        }
    }

    private IEnumerator CheckStatus()
    {
        yield return new WaitUntil(() => endGameManager.board.currentState != GameState.wait);
        yield return new WaitUntil(() => endGameManager.board.fillState == FillState.none);
        endGameManager.WinGame();
    }

    public void CompareGoal(string goalToCompare)
    {
        if (goalToCompare == "Blue")
        {
            blueCollected++;
        }

        if (goalToCompare == "Green")
        {
            greenCollected++;
        }
        
        if (goalToCompare == "Red")
        {
            redCollected++;
        }
        
        if (goalToCompare == "Purple")
        {
            purpleCollected++;
        }
        
        if (goalToCompare == "Yellow")
        {
            yellowCollected++;
        }
    }
}
