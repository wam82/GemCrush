using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Candy : MonoBehaviour
{
    private Vector2 firstTouchPosition;

    private Vector2 lastTouchPosition;

    private float swipeAngle;

    public int column;

    public int row;

    private GameObject otherCandy;

    private Board board;

    private int targetX;

    private int targetY;

    private Vector2 tempPosition;

    public bool isMatched = false;

    private int previousColumn;

    private int previousRow;

    private readonly float swipeResistance = 1f;

    private FindMatches findMatches;

    private HintManager hintManager;

    private EndGameManagement endGameManager;
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        hintManager = FindObjectOfType<HintManager>();
        endGameManager = FindObjectOfType<EndGameManagement>();
    }

    // Update is called once per frame
    void Update()
    {
        // FindMatch();
        if (isMatched)
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            sprite.color = new Color(1f, 1f, 1f, 0.2f);
        }
        targetX = column;
        targetY = row;
        // X-axis movement
        if (Mathf.Abs(targetX - transform.position.x) > 0.1)
        {
            // Move towards target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.2f);
            if (board.allCandies[column, row] != this.gameObject)
            {
                board.allCandies[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            // Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }
        
        // Y-axis movement
        if (Mathf.Abs(targetY - transform.position.y) > 0.1)
        {
            // Move towards target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.2f);
            if (board.allCandies[column, row] != this.gameObject)
            {
                board.allCandies[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            // Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    private IEnumerator CheckMoveCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        if (otherCandy != null)
        {
            if (!isMatched && !otherCandy.GetComponent<Candy>().isMatched)
            {
                // If no match, put the candies to their original places
                otherCandy.GetComponent<Candy>().row = row;
                otherCandy.GetComponent<Candy>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(0.5f);
                board.currentState = GameState.move;
            }
            else
            {
                if (endGameManager != null)
                {
                    endGameManager.DecreaseMove();
                }
                board.DestroyMatch();
            }
            
            otherCandy = null;
        }
        
    }
    private void OnMouseDown()
    {
        if (hintManager != null)
        {
            hintManager.DestroyHint();
        }
        
        if (board.currentState == GameState.move || board.currentState == GameState.ready)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move || board.currentState == GameState.ready)
        {
            lastTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(lastTouchPosition.y - firstTouchPosition.y) > swipeResistance ||
            Mathf.Abs(lastTouchPosition.x - firstTouchPosition.x) > swipeResistance)
        {
            // Making sure that there is actually a swipe. Without this condition, a simple click registers as a right swipe.
            swipeAngle = Mathf.Atan2(lastTouchPosition.y - firstTouchPosition.y,
                lastTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MoveCandies();
            board.currentState = GameState.wait;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MoveCandies()
    {
        if (swipeAngle is > -45 and <= 45 && column < board.width - 1)
        { //Right Swipe
            otherCandy = board.allCandies[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherCandy.GetComponent<Candy>().column -= 1;
            column += 1;
        }
        else if ((swipeAngle is > 135 or <= -135) && column > 0)
        { //Left Swipe
            otherCandy = board.allCandies[column - 1, row];
            previousRow = row;
            previousColumn = column;
            otherCandy.GetComponent<Candy>().column += 1;
            column -= 1;
        }
        else if (swipeAngle is > 45 and <= 135 && row < board.height - 1)
        { //Up Swipe
            otherCandy = board.allCandies[column, row + 1];
            previousRow = row;
            previousColumn = column;
            otherCandy.GetComponent<Candy>().row -= 1;
            row += 1;
        }
        else if (swipeAngle is < -45 and >= -135 && row > 0)
        { //Down Swipe
            otherCandy = board.allCandies[column, row - 1];
            previousRow = row;
            previousColumn = column;
            otherCandy.GetComponent<Candy>().row += 1;
            row -= 1;
        }
        StartCoroutine(CheckMoveCoroutine());
    }

/*
    void FindMatch()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftCandy1 = board.allCandies[column - 1, row];
            GameObject rightCandy1 = board.allCandies[column + 1, row];
            if (leftCandy1 != null && rightCandy1 != null)
            {
                if (leftCandy1.CompareTag(this.gameObject.tag) && rightCandy1.CompareTag(this.gameObject.tag) && leftCandy1 != this.gameObject && rightCandy1 != this.gameObject)
                {
                    leftCandy1.GetComponent<Candy>().isMatched = true;
                    rightCandy1.GetComponent<Candy>().isMatched = true;
                    isMatched = true;
                }
            }
            
        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upCandy1 = board.allCandies[column, row + 1];
            GameObject downCandy1 = board.allCandies[column, row - 1];
            if (upCandy1 != null && downCandy1 != null)
            {
                if (upCandy1.CompareTag(this.gameObject.tag) && downCandy1.CompareTag(this.gameObject.tag) && upCandy1 != this.gameObject && downCandy1 != this.gameObject)
                {
                    upCandy1.GetComponent<Candy>().isMatched = true;
                    downCandy1.GetComponent<Candy>().isMatched = true;
                    isMatched = true;
                }
            }
            
        }
    }
*/
}
