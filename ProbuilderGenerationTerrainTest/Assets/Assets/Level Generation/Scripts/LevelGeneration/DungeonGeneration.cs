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
        // create part of dungeon
        createPart();

        //direction
        int moveDirection = direction();

        // blocked
        if (moveDirection == 0) {
            print("--------BLOCKED--------" + "X: " + x + "Z: " + z);
            generatingLevel = false;
        }
        // forward
        if (moveDirection == 1)
        {
            z++;
        }
        // right
        if (moveDirection == 2)
        {
            x++;
        }
        //left
        if (moveDirection == 3)
        {
            x--;
        }
        //down
        if (moveDirection == 4)
        {
            z--;
        }

        //for debugging and testing purposes
        curDirection = moveDirection;

        if (outOfBounds())
        {
            generatingLevel = false;
            print("--------OUT OF BOUNDS--------" + "X: " + x + "Z: " + z);
            return;
        }

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
        //print("created: X: " + x + "Z: " + z);
        //print("current direction: " + curDirection);

        GameObject created = GameObject.CreatePrimitive(PrimitiveType.Cube);
        created.transform.position = new Vector3(x, 0, z);
        dungeonLayout[x, z] = 1;


        //// start
        //if (curDirection == 0)
        //{

        //    created.GetComponent<MeshRenderer>().material.color = Color.red;
        //}
        //// forward
        //if (curDirection == 1)
        //{

        //    created.GetComponent<MeshRenderer>().material.color = Color.black;
        //}
        //// right
        //if (curDirection == 2)
        //{

        //    created.GetComponent<MeshRenderer>().material.color = Color.blue;
        //}
        //// left
        //if (curDirection == 3)
        //{

        //    created.GetComponent<MeshRenderer>().material.color = Color.green;
        //}
        //// down
        //if (curDirection == 4)
        //{

        //    created.GetComponent<MeshRenderer>().material.color = Color.yellow;
        //}
    }
}


