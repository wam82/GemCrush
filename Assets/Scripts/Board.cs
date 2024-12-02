using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum GameState
{
    wait,
    move,
    win,
    lose,
    pause,
    ready
}

public enum FillState
{
    destroy,
    refill,
    verify,
    none
}
public class Board : MonoBehaviour
{
    public int width; // Width should be 8

    public int height; // Height should be 8
    
    public int offSet;

    private BackgroundTile[,] allTiles;
    
    public GameObject[,] allCandies;

    public GameObject tilePrefab;
    
    public GameObject[] candies;

    public GameState currentState = GameState.move;

    private FindMatches findMatches;

    private HintManager hintManager;

    private int shuffleIterations = 0;

    private ScoreManagement scoreManager;

    public int baseValue;

    public int multiplier;

    public int[] scoreGoals;
    
    public GoalManagement goalManager;

    public int level;

    public int levelID;

    public World world;

    public bool firstPlaced = false;
    
    public bool firstPlaceState = false;
    
    public FillState fillState = FillState.none;

    public AudioSource audioSource;

    public Image backgroundImage;
    private void Awake()
    {
        if (PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if (world != null)
        {
            if (world.levels[level] != null)
            {
                levelID = world.levels[level].levelID;
                scoreGoals = world.levels[level].scoreGoals;
                backgroundImage.sprite = world.levels[level].backgroundImage;
                // Debug.Log(levelID);
            }
        }
    }
    

    // Start is called before the first frame update
    void Start()
    {
        allTiles = new BackgroundTile[width, height];
        allCandies = new GameObject[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        hintManager = FindObjectOfType<HintManager>();
        scoreManager = FindObjectOfType<ScoreManagement>();
        goalManager = FindObjectOfType<GoalManagement>();
        audioSource = GetComponent<AudioSource>();
        currentState = GameState.pause;
        SetUp();
    }

    private void SetUp()
    {
        audioSource.clip = world.levels[level].backgroundMusic;
        audioSource.loop = true;
        audioSource.Play();
        goalManager.updateGoals();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 startPosition = new Vector2(x, y);
                Vector2 offsetPosition = new Vector3(x, y + offSet);
                GameObject backgroundTile = Instantiate(tilePrefab, startPosition, Quaternion.identity);
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + x + ", " + y + " )";
                
                int colour = Random.Range(0, candies.Length);
                int maxIterations = 0;
                while (MatchesAtStart(x, y, candies[colour]) && maxIterations < 50)
                {
                    colour = Random.Range(0, candies.Length);
                    maxIterations++;
                }

                maxIterations = 0;
                GameObject dot = Instantiate(candies[colour], offsetPosition, Quaternion.identity);
                dot.GetComponent<Candy>().row = y;
                dot.GetComponent<Candy>().column = x;
                dot.transform.parent = this.transform;
                dot.name = "( " + x + ", " + y + " )";
                allCandies[x, y] = dot;
                Vector2 finalPosition = new Vector2(x, y);
            }
        }

        if (CheckDeadlocked())
        {
            // Debug.Log("Soft locked by game generation");
            Shuffle();
            // Debug.Log("Shuffle completed");
        }

        currentState = GameState.ready;
    }
    
    // Update is called once per frame
    // void Update()
    // {
    //     
    // }

    private bool MatchesAtStart(int column, int row, GameObject piece)
    {
        if (column > 1)
        {
            if (allCandies[column - 1, row].CompareTag(piece.tag) && allCandies[column - 2, row].CompareTag(piece.tag))
            {
                return true;
            }
        }
        if (row > 1)
        {
            if (allCandies[column, row - 1].CompareTag(piece.tag) && allCandies[column, row - 2].CompareTag(piece.tag))
            {
                return true;
            }
        }
        return false;
    }

    private void DestroyCandy(int column, int row)
    {
        if (allCandies[column, row].GetComponent<Candy>().isMatched)
        {
            // Debug.Log("Deleting Current Matches");
            findMatches.currentMatches.Remove(allCandies[column, row]);
            if (goalManager != null)
            {
                // goalManager.CompareGoal(allCandies[column, row].tag);
                goalManager.updateGoals();
            }
            Destroy(allCandies[column, row]);
            scoreManager.IncreaseScore(baseValue * multiplier);
            allCandies[column, row] = null;
        }
    }

    public void DestroyMatch()
    {
        fillState = FillState.destroy;
        for (int x = 0; x < width; x++)
        {
            if (currentState == GameState.pause)
            {
                break;
            }
            for (int y = 0; y < height; y++)
            {
                if (currentState == GameState.pause)
                {
                    break;
                }
                if (allCandies[x, y] != null)
                {
                    DestroyCandy(x, y);
                }

                if (hintManager != null)
                {
                    hintManager.DestroyHint();
                }
            }
        }

        StartCoroutine(DropCoroutine());
    }

    private IEnumerator DropCoroutine()
    {
        int nullCount = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allCandies[x, y] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allCandies[x, y].GetComponent<Candy>().row -= nullCount;
                    allCandies[x, y] = null;
                }
            }

            nullCount = 0;
        }
        yield return new WaitForSeconds(0.5f / 2);
        StartCoroutine(RefillCoroutine());
    }

    
    // LVL 1 REFILL ALGORITHM
    private void RefillBoardLvl1()
    {
        for (int x = 0; x < width; x++)
        {
            if (currentState == GameState.pause)
            {
                firstPlaceState = firstPlaced;
                break;
            }
            for (int y = 0; y < height; y++)
            {
                if (currentState == GameState.pause)
                {
                    firstPlaceState = firstPlaced;
                    break;
                }
                if (allCandies[x, y] == null)
                {
                    Vector2 position = new Vector2(x, y + offSet);
                    int colour = Random.Range(0, candies.Length);
                    
                    if (findMatches.matchOrientation == "Vertical")
                    {
                        // Debug.Log("Refilling vertical match");
                        if (!firstPlaced)
                        {
                            if (y == 0)
                            {
                                // Debug.Log("Was at bottom (Vertical)");
                                colour = Random.Range(0, candies.Length);
                            }
                            else
                            {
                                if (Random.value <= 0.4f)
                                {
                                    // Debug.Log("Not at bottom (Vertical - 40% Passed)");
                                    for (int i = 0; i < candies.Length; i++)
                                    {
                                        if (candies[i].CompareTag(allCandies[x, y - 1].GetComponent<Candy>().tag))
                                        {
                                            colour = i;
                                        }
                                    }
                                }
                                else
                                {
                                    // Debug.Log("Not at bottom (Vertical - 40% Failed)");
                                    colour = Random.Range(0, candies.Length);
                                }
                            }
                            
                            firstPlaced = true;
                        }
                        else
                        {
                            
                            if (Random.value <= 0.6f && y > 0)
                            {
                                // Debug.Log("Post first match (Vertical - 60% Passed)");
                                for (int i = 0; i < candies.Length; i++)
                                {
                                    if (candies[i].CompareTag(allCandies[x, y - 1].GetComponent<Candy>().tag))
                                    {
                                        colour = i;
                                    }
                                }
                            }
                            else
                            {
                                // Debug.Log("Post first match (Vertical - 60% Failed)");
                                colour = Random.Range(0, candies.Length);
                            }
                        }
                    }
                    else if (findMatches.matchOrientation == "Horizontal")
                    {
                        // Debug.Log("Refilling horizontal match");
                        if (y == 0)
                        {
                            colour = Random.Range(0, candies.Length);
                        }
                        else
                        {
                            if (Random.value <= 0.6f)
                            {
                                for (int i = 0; i < candies.Length; i++)
                                {
                                    if (candies[i].CompareTag(allCandies[x, y - 1].GetComponent<Candy>().tag))
                                    {
                                        colour = i;
                                    }
                                }
                            }
                            else
                            {
                                colour = Random.Range(0, candies.Length);
                            } 
                        }
                    }
                    else
                    {
                        Debug.Log("Detected match but no direction");
                        colour = Random.Range(0, candies.Length);
                    }
                    
                    GameObject piece = Instantiate(candies[colour], position, Quaternion.identity);
                    allCandies[x, y] = piece;
                    piece.GetComponent<Candy>().row = y;
                    piece.GetComponent<Candy>().column = x;
                }
                else
                {
                    firstPlaced = false;
                }
            }
        }
    }
    
    // LVL 2 REFILL BOARD
    private void RefillBoardLvl2()
    {
        for (int x = 0; x < width; x++)
        {
            if (currentState == GameState.pause)
            {
                break;
            }
            for (int y = 0; y < height; y++)
            {
                if (currentState == GameState.pause)
                {
                    break;
                }
                if (allCandies[x, y] == null)
                {
                    Vector2 position = new Vector2(x, y + offSet);
                    int colour = SelectColour(x, y);
                    
                    GameObject piece = Instantiate(candies[colour], position, Quaternion.identity);
                    allCandies[x, y] = piece;
                    piece.GetComponent<Candy>().row = y;
                    piece.GetComponent<Candy>().column = x;
                }
            }
        }
    }

    private int SelectColour(int x, int y)
    {
        int colour = Random.Range(0, candies.Length);
        int[] surroundingColourCount = new int[candies.Length];

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                // Check neighbouring tiles
                if (dx == 0 && dy == 0)
                {
                    // Skip center tile itself
                    continue;
                }

                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < width && ny >= 0 && ny < height && allCandies[nx, ny] != null)
                {
                    // Check bounds to avoid out-of-bounds errors
                    string colourName = allCandies[nx, ny].tag;
                    int neighborColour = 0;
                    for (int i = 0; i < candies.Length; i++)
                    {
                        if (candies[i].CompareTag(colourName))
                        {
                            neighborColour = i;
                        }
                    }

                    surroundingColourCount[neighborColour]++;
                }
            }
        }

        // Set probabilities based on colour count
        float totalProbability = 0f;
        float[] probabilities = new float[candies.Length];
        for (int i = 0; i < candies.Length; i++)
        {
            probabilities[i] = 1f + surroundingColourCount[i]; // Base probability + count-based boost
            totalProbability += probabilities[i];
        }
        
        // Debug.Log($"Tile at ({x},{y}): Neighbor Color Counts:");
        // for (int i = 0; i < candies.Length; i++)
        // {
        //     float probabilityPercentage = (probabilities[i] / totalProbability) * 100f;
        //     Debug.Log($"Color {i} (Count: {surroundingColourCount[i]}): Probability {probabilityPercentage}%");
        // }
        
        // Select a colour based on the probabilities
        float randomValue = Random.Range(0f, totalProbability);
        float cumulative = 0f;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (randomValue <= cumulative)
            {
                return i;
            }
        }
        
        // Debug.Log($"Generated color: {colour} for tile at ({x},{y})");

        return colour;
    }

    private bool MatchOnBoardFromCascade()
    {
        fillState = FillState.verify;
        for (int x = 0; x < width; x++)
        {
            if (currentState == GameState.pause)
            {
                break;
            }
            for (int y = 0; y < height; y++)
            {
                if (currentState == GameState.pause)
                {
                    break;
                }
                if (allCandies[x, y] != null)
                {
                    if (allCandies[x, y].GetComponent<Candy>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void RefillBoard()
    {
        if (levelID == 1)
        {
            // Debug.Log("Level 1 Algorithm");
            RefillBoardLvl1();
        }
        else if(levelID == 2)
        {
            // Debug.Log("Level 2 Algorithm");
            RefillBoardLvl2();
        }
        else
        {
            Debug.Log("An error occured when fetching level");
        }
    }

    private IEnumerator RefillCoroutine()
    {
        fillState = FillState.refill;
        if (currentState != GameState.lose && currentState != GameState.pause && currentState != GameState.win)
        {
            RefillBoard();
            yield return new WaitForSeconds(2 * 0.5f);
            while (MatchOnBoardFromCascade())
            {
                if (currentState == GameState.pause)
                {
                    break;
                }
                multiplier++;
                DestroyMatch();
                yield return new WaitForSeconds(2 * 0.5f);
            }
        
            yield return new WaitForSeconds(0.5f);
            if (CheckDeadlocked())
            {
                yield return new WaitForSeconds(0.5f);
                Shuffle();
                // Debug.Log("Deadlocked from move");
            }
            currentState = GameState.move;
            fillState = FillState.none;
            multiplier = 1;
        }
    }

    private void SwitchCandiesForCheck(int column, int row, Vector2 direction)
    {
        (allCandies[column + (int)direction.x, row + (int)direction.y], allCandies[column, row]) = (allCandies[column, row], allCandies[column + (int)direction.x, row + (int)direction.y]);
        // GameObject holder = allCandies[column + (int) direction.x , row + (int) direction.y];
        // allCandies[column + (int)direction.x, row + (int)direction.y] = allCandies[column, row];
        // allCandies[column, row] = holder;
    }

    private bool CheckForMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allCandies[x, y] != null)
                {
                    if (x < width - 2)
                    {
                        if (allCandies[x + 1, y] != null & allCandies[x + 2, y] != null)
                        {
                            if (allCandies[x + 1, y].CompareTag(allCandies[x, y].tag) &&
                                allCandies[x + 2, y].CompareTag(allCandies[x, y].tag))
                            {
                                return true;
                            }
                        }
                    }

                    if (y < height - 2)
                    {
                        if (allCandies[x, y + 1] != null & allCandies[x, y + 2] != null)
                        {
                            if (allCandies[x, y + 1].CompareTag(allCandies[x, y].tag) &&
                                allCandies[x, y + 2].CompareTag(allCandies[x, y].tag))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    public bool SwitchCandyAndVerify(int column, int row, Vector2 direction)
    {
        SwitchCandiesForCheck(column, row, direction);
        if (CheckForMatches())
        {
            SwitchCandiesForCheck(column, row, direction);
            return true;
        }
        SwitchCandiesForCheck(column, row, direction);
        return false;
    }

    private bool CheckDeadlocked()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allCandies[x, y] != null)
                {
                    if (x < width - 1)
                    {
                        if (SwitchCandyAndVerify(x, y, Vector2.right))
                        {
                            return false;
                        }
                    }

                    if (y < height - 1)
                    {
                        if (SwitchCandyAndVerify(x, y, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private void Shuffle()
    {
        List<GameObject> secondBoard = new List<GameObject>();
        for (int x = 0; x < width; x++)
        {
            if (currentState == GameState.pause)
            {
                secondBoard.Clear();
                break;
            }
            for (int y = 0; y < height; y++)
            {
                if (currentState == GameState.pause)
                {
                    secondBoard.Clear();
                    break;
                }
                if (allCandies[x, y] != null)
                {
                    secondBoard.Add(allCandies[x, y]);
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            if (currentState == GameState.pause)
            {
                secondBoard.Clear();
                break;
            }
            for (int y = 0; y < height; y++)
            {
                if (currentState == GameState.pause)
                {
                    secondBoard.Clear();
                    break;
                }
                int candyToUse = Random.Range(0, secondBoard.Count());
                int maxIterations = 0;
                while (MatchesAtStart(x, y, secondBoard[candyToUse]) && maxIterations < 50)
                {
                    candyToUse = Random.Range(0, secondBoard.Count());
                    maxIterations++;
                }
                maxIterations = 0;
                Candy candy = secondBoard[candyToUse].GetComponent<Candy>();
                candy.column = x;
                candy.row = y;
                allCandies[x, y] = secondBoard[candyToUse];
                secondBoard.Remove(secondBoard[candyToUse]);
            }
        }

        if (CheckDeadlocked() && shuffleIterations < 10)
        {
            shuffleIterations++;
            Shuffle();
        }

        shuffleIterations = 0;
    }

    public void PickBackUp(FillState state)
    {
        Debug.Log("Arrived in PickBackUp() & recieved state " + state);
        if (state == FillState.destroy)
        {
            DestroyMatch();
        }
        else if (state == FillState.refill)
        {
            StartCoroutine(RefillCoroutine());
        }
        else if (state == FillState.verify)
        {
            MatchOnBoardFromCascade();
        }
        else
        {
            currentState = GameState.move;
        }
    }
}
