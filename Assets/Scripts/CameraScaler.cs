using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    private Board board;

    public float cameraOffset;

    public float aspectRatio; //Is a precalculated value based on aspect ratio: width/height

    public float padding;
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>(); // Finds the object that is of type Board. If more than one object of type Board exists than this doesn't work anymore.
        if (board != null)
        {
            Reposition(board.width, board.height);
        }
    }

    void Reposition(float x, float y)
    {
        Vector3 newPosition = new Vector3((x - 1) / 2, (y - 1) / 2, cameraOffset); // Camera position set to be in the middle of the the game board (both in x & y axis)
        transform.position = newPosition;
        if (board.width >= board.height)
        {
            Camera.main.orthographicSize = (board.width / 2 + padding) / aspectRatio;
        }
        else
        {
            Camera.main.orthographicSize = (board.height / 2 + padding);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
