using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class DungeonGeneration : MonoBehaviour
{
    /**
     * Out of bounds on Z axes goes wrong, don't know why.
     * Sometimes the y goes out of bounds when its less then 0.
     */
    public int xBounds, zBounds, yBounds;

    public int ForwardChance;
    public int BackwardChance;
    public int LeftChance;
    public int RightChance;
    public int UpChance;
    public int DownChance;

    public bool UseSeed;
    public int seed;

    public bool setStepAmount;
    public int AmoutOfSteps;

    public GameObject CorridorPiece;
    public GameObject CornerPiece;
    public GameObject StartEndPiece;
    public GameObject UpPiece;
    public GameObject DownPiece;
    public GameObject player;

    private static int PIECE_SIZE = 10;

    private int[,,] dungeonLayout;
    private GameObject parent;

    private int x, z, y;
    private Directions nextDirection;
    private Directions prevDirection;

    private int currentRotation;
    private GameObject lastPiece;
    private Vector3 lastPos;
    private int chance;

    private int counter;

    private bool generatingLevel;
    private bool startPlaced;

    private enum Directions { NONE, FORWARD, RIGHT, LEFT, BACK, BLOCKED, UP, DOWN, };

    private List<GameObject> Dungeon = new List<GameObject>();

    public void Reload()
    {
        foreach (var piece in Dungeon)
        {
            Destroy(piece);
        }
        Start();
    }

    private void init()
    {
        Destroy(parent);
        startPlaced = false;
        counter = 0;
        currentRotation = 0;
        prevDirection = Directions.NONE;
        nextDirection = Directions.NONE;
        lastPos = Vector3.zero;
        generatingLevel = true;
        Dungeon.Clear();
        if (setStepAmount && AmoutOfSteps < 1)
        {
            AmoutOfSteps = 1;
        }
    }

    void Start()
    {
        init();
        if (xBounds == 0 || zBounds == 0)
        {
            Debug.LogError("Bounds are not set properly");
        }

        if (ForwardChance + BackwardChance + LeftChance + RightChance != 100)
        {
            Debug.LogError("Total chance not 100% " + (ForwardChance + BackwardChance + LeftChance + RightChance));//change to end game.
        }

        parent = new GameObject();
        parent.name = "Dungeon";

        dungeonLayout = new int[xBounds, zBounds, yBounds];

        if (UseSeed)
        {
            Random.InitState(seed);
        }
        else
        {
            seed = Random.Range(0, max: int.MaxValue);
            Random.InitState(seed);
        }

        x = Random.Range(xBounds/3, xBounds/3*2);
        z = Random.Range(zBounds/3, zBounds/3*2);
        y = Random.Range(yBounds/3, yBounds/3*2);

        // create start position
        createPart();

    }

    void Update()
    {
        if (generatingLevel)
        {
            //generate piece of the dungeon every update
            generateDungeon();
        }
    }

    //main function to generate the dungeon
    void generateDungeon()
    {
        //Creating a new position to place the next chunk.
        generateNewPosition();

        // create part of dungeon
        createPart();
    }

    //Updating the next position of the dungeon layout for chunk placement
    void generateNewPosition()
    {
        switch (nextDirection)
        {
            case Directions.BLOCKED:
                print("--------BLOCKED--------" + "X: " + x + "Z: " + z);
                generatingLevel = false;
                break;
            case Directions.FORWARD:
                z++;
                break;
            case Directions.RIGHT:
                x++;
                break;
            case Directions.LEFT:
                x--;
                break;
            case Directions.BACK:
                z--; ;
                break;
            default:
                break;
        }
    }

    //Generating a direction based on the chance fields controlled in the UI.
    private Directions GetChanceBasedDirections()
    {
        if (!setStepAmount || counter < AmoutOfSteps)
        {
            chance = Random.Range(1, 101);
            //forward
            if (chance >= 0 && chance <= ForwardChance)
            {
                return Directions.FORWARD;
            }

            //back
            if (chance >= ForwardChance && chance <= ForwardChance + BackwardChance)
            {
                return Directions.BACK;
            }

            //right
            if (chance >= ForwardChance + BackwardChance && chance <= ForwardChance + BackwardChance + RightChance)
            {
                return Directions.RIGHT;
            }

            //left
            if (chance >= ForwardChance + BackwardChance + RightChance && chance <= 100)
            {
                return Directions.LEFT;
            }
        }

        return Directions.BLOCKED;
    }


    // getting possible directions via this method and the blocked methode.
    Directions getNextDirection()
    {
        Directions dir = GetChanceBasedDirections();
        bool notBlocked = false;

        // can go forward
        if (z < zBounds - 1 && dungeonLayout[x, z + 1, y] == 0)
        {
            if (dir == Directions.FORWARD)
            {
                return Directions.FORWARD;
            }
            else if (ForwardChance > 0)
            {
                notBlocked = true;
            }
        }
        // can go back
        if (z - 1 > 0 && dungeonLayout[x, z - 1, y] == 0)
        {
            if (dir == Directions.BACK)
            {
                return Directions.BACK;
            }
            else if (BackwardChance > 0)
            {
                notBlocked = true;
            }
        }
        // can go right
        if (x < xBounds - 1 && dungeonLayout[x + 1, z, y] == 0)
        {
            if (dir == Directions.RIGHT)
            {
                return Directions.RIGHT;
            }
            else if (RightChance > 0)
            {
                notBlocked = true;
            }
        }
        // can go left
        if (x > 0 && dungeonLayout[x - 1, z, y] == 0)
        {
            if (dir == Directions.LEFT)
            {
                return Directions.LEFT;
            }
            else if (LeftChance > 0)
            {
                notBlocked = true;
            }
        }

        //Can build to a direction but the random given direction is not possible try generating an other direction
        if (notBlocked && dir != Directions.BLOCKED)
        {
            return getNextDirection();
        }

        return Directions.BLOCKED;
    }

    void createPart()
    {
        Directions placeDirection = nextDirection;
        nextDirection = getNextDirection();

        GameObject piece = null;

        // place start or end
        if (placeDirection == Directions.NONE || placeDirection == Directions.BLOCKED)
        {
            piece = Instantiate(StartEndPiece, new Vector3(x * PIECE_SIZE, y * 5, z * PIECE_SIZE), Quaternion.identity);
            //spawning start
            if (!startPlaced)
            {
                startPlaced = true;
                piece.name = "START";
                Component[] childs = piece.GetComponentsInChildren<Renderer>();
                //change color to green for starting node
                foreach (var child in childs)
                {
                    child.GetComponent<Renderer>().material.color = Color.green;
                }
                if (nextDirection == Directions.RIGHT)
                {
                    currentRotation = 180;
                }
                else if (nextDirection == Directions.FORWARD)
                {
                    currentRotation = 90;
                }
                else if (nextDirection == Directions.LEFT)
                {
                    currentRotation = 0;
                }
                else if (nextDirection == Directions.BACK)
                {
                    currentRotation = 270;
                }
                //uncomment if you want to spawn the player
                //spawnPlayer(piece.transform.position + new Vector3(0, 3, 0));
            }
            //spawning end
            else
            {
                piece.name = "END";
                Component[] childs = piece.GetComponentsInChildren<Renderer>();
                //adding color to end piece.
                foreach (var child in childs)
                {
                    child.GetComponent<Renderer>().material.color = Color.red;
                }

                //Rotation based on last piece
                if (lastPiece.name == "CORNER")
                {
                    if (lastPiece.transform.rotation.y >= 89 && lastPiece.transform.rotation.y <= 91)
                    {
                        currentRotation = 0;
                    }
                    else if (lastPiece.transform.rotation.y >= 179 && lastPiece.transform.rotation.y <= 181)
                    {
                        currentRotation = 180;
                    }
                    else if (lastPiece.transform.rotation.y >= -1 && lastPiece.transform.rotation.y <= 1)
                    {
                        currentRotation = 0;
                    }
                    else
                    {
                        currentRotation = 270;
                    }
                }
                //Rotation based on previous direction
                else if (prevDirection == Directions.RIGHT)
                {
                    currentRotation = 0;
                }
                else if (prevDirection == Directions.LEFT)
                {
                    currentRotation = 180;
                }
                else if (prevDirection == Directions.BACK)
                {
                    currentRotation = 90;
                }
                else
                {
                    currentRotation = 270;
                }
            }

        }
        // Place corner
        else if (mustPlaceCorner(placeDirection))
        {
            piece = Instantiate(CornerPiece, new Vector3(x * PIECE_SIZE, y * 5, z * PIECE_SIZE), Quaternion.identity);
            piece.name = "CORNER";

            if (placeDirection == Directions.FORWARD && nextDirection == Directions.RIGHT ||
                placeDirection == Directions.LEFT && nextDirection == Directions.BACK)
            {
                currentRotation = 90;
            }
            if (placeDirection == Directions.FORWARD && nextDirection == Directions.LEFT ||
               placeDirection == Directions.RIGHT && nextDirection == Directions.BACK)
            {
                currentRotation = 180;
            }

            if (placeDirection == Directions.RIGHT && nextDirection == Directions.FORWARD ||
                placeDirection == Directions.BACK && nextDirection == Directions.LEFT)
            {
                currentRotation = 270;
            }
            if (placeDirection == Directions.LEFT && nextDirection == Directions.FORWARD ||
                placeDirection == Directions.BACK && nextDirection == Directions.RIGHT)
            {
                currentRotation = 0;
            }
        }
        // place piece
        else
        {
            piece = Instantiate(CorridorPiece, new Vector3(x * PIECE_SIZE, y * 5, z * PIECE_SIZE), Quaternion.identity);
            piece.name = "CORRIDOR";
            if (placeDirection == Directions.RIGHT || placeDirection == Directions.LEFT)
            {
                currentRotation = 0;
            }
            else
            {
                currentRotation = 90;
            }
        }
        //rotate spawned piece
        piece.transform.Rotate(0, currentRotation, 0);

        //fil the dungeon position so there cannot be placed an other node.
        dungeonLayout[x, z, y] = 1;

        //hold the last position
        lastPos = new Vector3(x, y, z);

        //getting the last position
        prevDirection = placeDirection;

        //When the walker cannot go a direction place end piece and delete previos piece
        if (placeDirection == Directions.BLOCKED)
        {
            Destroy(lastPiece);
            placeDirection = Directions.NONE;
        }
        //add to parent
        piece.transform.parent = parent.transform;
        
        // keep track of all chunks to later delete for reload
        Dungeon.Add(piece);

        //setting last piece
        lastPiece = piece;

        //increase step counter.
        counter++;
    }


    //Checks if a corners needs to be placed
    bool mustPlaceCorner(Directions placeDirection)
    {

        if (placeDirection != Directions.NONE)
        {
            if (lastPos.x != getNextPos().x && lastPos.z != getNextPos().z)
            {
                return true;
            }
        }
        return false;
    }
    // look at next direction to calculate if it's possible
    Vector3 getNextPos()
    {
        Vector3 nextPos = new Vector3(x, y, z);

        // forward
        if (nextDirection == Directions.FORWARD)
        {
            nextPos.z++;
        }
        // right
        if (nextDirection == Directions.RIGHT)
        {
            nextPos.x++;
        }
        //left
        if (nextDirection == Directions.LEFT)
        {
            nextPos.x--;
        }
        //back
        if (nextDirection == Directions.BACK)
        {
            nextPos.z--;
        }

        return nextPos;
    }

    void spawnPlayer(Vector3 pos)
    {
        Instantiate(player, pos, Quaternion.identity);
    }
}

/*
 * 
 * UI CLASS: to handle UI elements for all parameters
 * 
 * */
[CustomEditor(typeof(DungeonGeneration))]
public class DungeonGenerationEditor : Editor
{
    private bool chanceFold;
    private bool objectsFold;

    public override void OnInspectorGUI()
    {
        var dg = target as DungeonGeneration;

        dg.xBounds = EditorGUILayout.IntField("X Boundary", dg.xBounds);
        dg.yBounds = EditorGUILayout.IntField("Y Boundary", dg.yBounds);
        dg.zBounds = EditorGUILayout.IntField("Z Boundary", dg.zBounds);

        EditorGUILayout.Space();

        dg.UseSeed = EditorGUILayout.Toggle("Use Seed", dg.UseSeed);

        if (dg.UseSeed)
        {
            dg.seed = EditorGUILayout.IntField("Seed", dg.seed);
        }
        EditorGUILayout.Space();

        dg.setStepAmount = EditorGUILayout.Toggle("Set step amount", dg.setStepAmount);
        if(dg.setStepAmount) dg.AmoutOfSteps = EditorGUILayout.IntField("Amount of Steps", dg.AmoutOfSteps);

        EditorGUILayout.Space();
        chanceFold = EditorGUILayout.Foldout(chanceFold, "Chances for pieces");
        if (chanceFold)
        {
            dg.ForwardChance = EditorGUILayout.IntField("Forward Chance", dg.ForwardChance);
            dg.BackwardChance = EditorGUILayout.IntField("Backward Chance", dg.BackwardChance);
            dg.LeftChance = EditorGUILayout.IntField("Left Chance", dg.LeftChance);
            dg.RightChance = EditorGUILayout.IntField("Right Chance", dg.RightChance);
            dg.DownChance = EditorGUILayout.IntField("Down Chance", dg.DownChance);
            dg.UpChance = EditorGUILayout.IntField("Up Chance", dg.UpChance);
        }

        EditorGUILayout.Space();
        objectsFold = EditorGUILayout.Foldout(objectsFold, "Pieces");
        if (objectsFold)
        {
            dg.CorridorPiece = (GameObject) EditorGUILayout.ObjectField("Corridor Piece", dg.CorridorPiece, typeof(Object), true);
            dg.CornerPiece = (GameObject) EditorGUILayout.ObjectField("Corner Piece", dg.CornerPiece, typeof(Object), true);
            dg.StartEndPiece = (GameObject) EditorGUILayout.ObjectField("Start/End Piece", dg.StartEndPiece, typeof(Object), true);
            dg.UpPiece = (GameObject) EditorGUILayout.ObjectField("Up Piece", dg.UpPiece, typeof(Object), true);
            dg.DownPiece = (GameObject) EditorGUILayout.ObjectField("Down Piece", dg.DownPiece, typeof(Object), true);
            dg.player = (GameObject) EditorGUILayout.ObjectField("Player", dg.player, typeof(Object), true);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Reload"))
        {
            dg.Reload();
        }
    }
}