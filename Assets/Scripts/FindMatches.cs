using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindMatches : MonoBehaviour
{
    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();
    public int numberOfCandiesAdded = 0;
    public string matchOrientation = "";
    
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCoroutine());
    }

    private IEnumerator FindAllMatchesCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        
        // ALGORITHM WILL ALWAYS FIND MATCH BASED ON THE CANDY IN THE MIDDLE OF THE MATCH
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                GameObject currentCandy = board.allCandies[x, y];
                if (currentCandy != null)
                {
                    if (x > 0 && x < board.width - 1)
                    {
                        GameObject leftCandy = board.allCandies[x - 1, y];
                        GameObject rightCandy = board.allCandies[x + 1, y];
                        if (leftCandy != null && rightCandy != null)
                        {
                            if (leftCandy != currentCandy && rightCandy != currentCandy &&
                                leftCandy.CompareTag(currentCandy.tag) && rightCandy.CompareTag(currentCandy.tag))
                            {
                                
                                if (!currentMatches.Contains(leftCandy))
                                {
                                    currentMatches.Add(leftCandy);
                                    numberOfCandiesAdded++;
                                }
                                leftCandy.GetComponent<Candy>().isMatched = true;
                                if (!currentMatches.Contains(rightCandy))
                                {
                                    currentMatches.Add(rightCandy);
                                    numberOfCandiesAdded++;
                                }
                                rightCandy.GetComponent<Candy>().isMatched = true;
                                if (!currentMatches.Contains(currentCandy))
                                {
                                    currentMatches.Add(currentCandy);
                                    numberOfCandiesAdded++;
                                }
                                currentCandy.GetComponent<Candy>().isMatched = true;
                                if (numberOfCandiesAdded > 1)
                                {
                                    if (board.goalManager != null)
                                    { 
                                        board.goalManager.CompareGoal(currentCandy.tag);
                                        matchOrientation = "Horizontal";
                                    }
                                    // Debug.Log("Found a horizontal match at candy (" + x + ", " + y + ") and added " + numberOfCandiesAdded +" candies.");
                                    numberOfCandiesAdded = 0;
                                }
                                // Debug.Log("Current Matches Size: " + currentMatches.Count + " at candy ("+ x +", "+ y +")");
                                // Debug.Log("Length Value: " + length);
                                // if (currentMatches.Count - length > 1)
                                // {
                                //     goalManager.blueCounter--;
                                //     length = currentMatches.Count;
                                //     // Check what colour the match is
                                //     // Decrease appropriate counter
                                //     // Set length equal to length of currentMatches list
                                //     // Repeat for up/down direction
                                // }
                                
                            }
                        }
                    }
                    if (y > 0 && y < board.height - 1)
                    {
                        GameObject upCandy = board.allCandies[x, y + 1];
                        GameObject downCandy = board.allCandies[x, y - 1];
                        if (upCandy != null && downCandy != null)
                        {
                            if (upCandy != currentCandy && downCandy != currentCandy &&
                                upCandy.CompareTag(currentCandy.tag) && downCandy.CompareTag(currentCandy.tag))
                            {
                                if (!currentMatches.Contains(upCandy))
                                {
                                    currentMatches.Add(upCandy);
                                    numberOfCandiesAdded++;
                                }
                                upCandy.GetComponent<Candy>().isMatched = true;
                                if (!currentMatches.Contains(downCandy))
                                {
                                    currentMatches.Add(downCandy);
                                    numberOfCandiesAdded++;
                                }
                                downCandy.GetComponent<Candy>().isMatched = true;
                                if (!currentMatches.Contains(currentCandy))
                                {
                                    currentMatches.Add(currentCandy);
                                    numberOfCandiesAdded++;
                                }
                                currentCandy.GetComponent<Candy>().isMatched = true;
                                
                                if (numberOfCandiesAdded > 1)
                                {
                                    if (board.goalManager != null)
                                    { 
                                        board.goalManager.CompareGoal(currentCandy.tag);
                                        matchOrientation = "Vertical";
                                    }
                                    // Debug.Log("Found a vertical match at candy (" + x + ", " + y + ") and added " + numberOfCandiesAdded +" candies.");
                                    numberOfCandiesAdded = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
