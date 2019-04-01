using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour
{
    public int xBounds, zBounds;

    private int[,] dungeonLayout;

    private float nextUpdate = 0.5f;

    private static int PIECE_SIZE = 10;

    private int x, z;
    public GameObject CorridorPiece;
    public GameObject CornerPiece;
    public GameObject StartEndPiece;

    private Vector3 lastPos = Vector3.zero;

    bool generatingLevel = true;

    int curDirection = 0;

    void Start()
    {
        if (xBounds == 0 || zBounds == 0)
        {
            Debug.LogError("Bounds are not set properly");
        }

        dungeonLayout = new int[xBounds, zBounds];

        x = Random.Range(0, xBounds);
        z = Random.Range(0, zBounds);

        // create start position
        createPart();
    }

    void Update()
    {
        //if (Time.time >= nextUpdate && generatingLevel)
        //{
        //    nextUpdate = Mathf.FloorToInt(Time.time) + 0.5f;

        placePiece();
        //}

        // if generation is done destroy self
        if (!generatingLevel)
        {
            Destroy(gameObject);
        }
    }


    void placePiece()
    {
        // blocked
        if (curDirection == 0) {
            print("--------BLOCKED--------" + "X: " + x + "Z: " + z);
            generatingLevel = false;
        }
        // forward
        if (curDirection == 1)
        {
            z++;
        }
        // right
        if (curDirection == 2)
        {
            x++;
        }
        //left
        if (curDirection == 3)
        {
            x--;
        }
        ////down
        //if (curDirection == 4)
        //{
        //    z--;
        //}

        if (outOfBounds())
        {
            generatingLevel = false;
            print("--------OUT OF BOUNDS--------" + "X: " + x + "Z: " + z);
            curDirection = 0;

        }

        // create part of dungeon
        createPart();
    }

    // getting possible directions via this method and the blocked methode.
    int direction()
    {
        List<int> possibleDirections = new List<int>();
        print("created: X: " + x + "Z: " + z);
        // can forward
        if (z < zBounds - 1 && dungeonLayout[x, z + 1] == 0)
        {
            print("FORWARD");
            possibleDirections.Add(1);
        }
        //// can back
        //if (z - 1 > 0 && dungeonLayout[x, z - 1] == 0)
        //{
        //    possibleDirections.Add(4);
        //}
        // can right
        if (x < xBounds + 1 && dungeonLayout[x + 1, z] == 0)
        {
            possibleDirections.Add(2);
        }
        // can left
        if (x > 0 && dungeonLayout[x - 1, z] == 0)
        {
            possibleDirections.Add(3);
        }

        if (possibleDirections.Count > 0)
        {
            print("possible directions " + possibleDirections);
            return possibleDirections[Random.Range(0, possibleDirections.Count)];
        }

        return 0;
    }

    //if out of bounds of the dungeon generation area return true;
    bool outOfBounds()
    {
        return x == xBounds - 1 || z == zBounds - 1 || x < 0 || z < 0;
    }

    void createPart()
    {
        int placeDirection = curDirection;
        curDirection = direction();
        // place end
        if (placeDirection == 0)
        {
            Instantiate(StartEndPiece, new Vector3(x * PIECE_SIZE, 0, z * PIECE_SIZE), Quaternion.identity);
        }
        else if (mustPlaceCorner(placeDirection)) {
            Instantiate(CornerPiece, new Vector3(x * PIECE_SIZE, 0, z * PIECE_SIZE), Quaternion.identity);
        }
        else {
            Instantiate(CorridorPiece, new Vector3(x * PIECE_SIZE, 0, z * PIECE_SIZE), Quaternion.identity);
        }

        dungeonLayout[x, z] = 1;
        lastPos = new Vector3(x, 0, z);
    }

    bool mustPlaceCorner(int placeDirection) {

        if (placeDirection != 0) {
            if (lastPos.x != getNextPos(curDirection).x && lastPos.z != getNextPos(curDirection).z) {
                return true;
            }
        }
        return false;
    }
    Vector3 getNextPos(int nextDir) {
        Vector3 nextPos = new Vector3(x, 0, z);

        // forward
        if (curDirection == 1)
        {
            nextPos.z++;
        }
        // right
        if (curDirection == 2)
        {
            nextPos.x++;
        }
        //left
        if (curDirection == 3)
        {
            nextPos.x--;
        }

        return nextPos;
    }

    Quaternion getPieceRotation()
    {
        return new Quaternion();
    }
}


