using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour
{
    /*
     * TIPS AND NOTES:
     * 
     * Default plane size: 10x10
     * 
     * */

    private Transform levelCollection;
    private static int CHUNK_SIZE = 1;
    private List<GameObject> chuckTypes;
    private GameObject[,] grid;

    public int WidthX, WidthZ;

    private int startingX;
    private int startingZ;

    void Start()
    {
        Initialize();

        generateLevel();
        generatePath();

        Destroy(this.gameObject);
    }

    private void Initialize()
    {
        startingX = WidthX / 2;
        startingZ = WidthZ / 2;

        levelCollection = new GameObject().transform;
        levelCollection.name = "Level";

        grid = new GameObject[WidthX, WidthZ];
    }

    void generateLevel()
    {
        for (int x = 0; x < WidthX; x += CHUNK_SIZE)
        {
            for (int z = 0; z < WidthZ; z += CHUNK_SIZE)
            {
                GameObject created = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), new Vector3(x, 0, z), Quaternion.identity);
                created.AddComponent<PathNode>();
                grid[x, z] = created;
                created.transform.parent = levelCollection;
            }
        }
    }

    void generatePath()
    {
        grid[startingX, startingZ].GetComponent<MeshRenderer>().material.color = Color.yellow;
        //findDirection(startingX, startingZ, lastPos);
    }



    void findDirection(int x, int z, int lastX, int lastZ)
    {
        bool findingDirection = true;

        bool triedLeft = false;
        bool triedRight = false;
        bool triedDown = false;
        bool triedForward = false;


        while (findingDirection)
        {
            float direction = randomDirection();

            //forward
            if (direction == 1 && z + 1 < WidthZ)
            {
                GameObject piece = grid[x, z +1];
                triedForward = true;
                if (!piece.GetComponent<PathNode>().isPath)
                {
                    ++z;
                    piece.GetComponent<MeshRenderer>().material.color = Color.red;
                    piece.GetComponent<PathNode>().isPath = true;
                    findingDirection = false;
                }
            }
            //right
            else if (direction == 2 && x + 1 < WidthX)
            {
                GameObject piece = grid[x + 1, z];
                triedRight = true;
                if (!piece.GetComponent<PathNode>().isPath)
                {
                    ++x;
                    piece.GetComponent<MeshRenderer>().material.color = Color.blue;
                    piece.GetComponent<PathNode>().isPath = true;
                    findingDirection = false;
                }
            }
            //left
            else if (direction == 3 && x > 0)
            {
                GameObject piece = grid[x - 1, z];
                triedLeft = true;
                if (!piece.GetComponent<PathNode>().isPath)
                {
                    --x;
                    piece.GetComponent<MeshRenderer>().material.color = Color.green;
                    piece.GetComponent<PathNode>().isPath = true;
                    findingDirection = false;
                }
            }
            //down
            else if (direction == 4 && z > 0)
            {
                GameObject piece = grid[x, z - 1];
                triedDown = true;
                if (!piece.GetComponent<PathNode>().isPath)
                {
                    --z;
                    piece.GetComponent<MeshRenderer>().material.color = Color.black;
                    piece.GetComponent<PathNode>().isPath = true;
                    findingDirection = false;
                }
            }

            if (triedDown && triedForward && triedLeft && triedRight) {
                return;
            }
        }

        //Check out of bounds
        if ((z - 1) <= 0 || (x - 1) <= 0 || x + 1 > WidthX || z + 1 > WidthZ)
        {
            return;
        }

        findDirection(x, z);
    }
    float randomDirection()
    {
        return Random.Range(1, 4);
    }
}
