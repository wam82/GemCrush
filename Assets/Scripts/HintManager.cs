using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    private Board board;

    public float hintCooldown;

    private float hintCoolDownSeconds;

    public GameObject hintParticle;

    public GameObject currentHint;
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        hintCoolDownSeconds = hintCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        hintCoolDownSeconds -= Time.deltaTime;
        if (hintCoolDownSeconds <= 0 && currentHint == null && 
            board.currentState != GameState.lose && board.currentState != GameState.pause && board.currentState != GameState.win)
        {
            CreateHint();
            hintCoolDownSeconds = hintCooldown;
        }
    }
    
    // Find all possible matches
    List<GameObject> FindAllMatches()
    {
        List<GameObject> possibleMoves = new List<GameObject>();
        if (board != null)
        {
            for (int x = 0; x < board.width; x++)
            {
                for (int y = 0; y < board.height; y++)
                {
                    if (board.allCandies[x, y] != null)
                    {
                        if (x < board.width - 1)
                        {
                            if (board.SwitchCandyAndVerify(x, y, Vector2.right))
                            {
                                possibleMoves.Add(board.allCandies[x, y]);
                            }
                        }

                        if (y < board.height - 1)
                        {
                            if (board.SwitchCandyAndVerify(x, y, Vector2.up))
                            {
                                possibleMoves.Add(board.allCandies[x, y]);
                            }
                        }
                    }
                }
            }
        }
        return possibleMoves;
    }
    // Select one of the match randomly
    GameObject SelectMatch()
    {
        List<GameObject> possibleMoves = new List<GameObject>();
        possibleMoves = FindAllMatches();
        if (possibleMoves.Count() > 0)
        {
            int matchToSelect = Random.Range(0, possibleMoves.Count());
            return possibleMoves[matchToSelect];
        }
        return null;
    }
    // Create the hint on selected match
    private void CreateHint()
    {
        GameObject move = SelectMatch();
        if (move != null)
        {
            currentHint = Instantiate(hintParticle, move.transform.position, Quaternion.identity);
        }
    }
    // Destroy the hint
    public void DestroyHint()
    {
        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
            hintCoolDownSeconds = hintCooldown;
        }
    }
}
