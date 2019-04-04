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
        Directions prevDirection = 0;
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

        if (ForwardChance + BackwardChance + LeftChance + RightChance + UpChance + DownChance != 100)
        {
            Debug.LogError("Total chance not 100% " + (ForwardChance + BackwardChance + LeftChance + RightChance + UpChance + DownChance));//change to end game.
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

    void generateDungeon()
    {
        generateNewPosition();

        // create part of dungeon
        createPart();
    }

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
            case Directions.UP:
                y++;
                break;
            case Directions.DOWN:
                y--;
                break;
            default:
                break;
        }

        /*if (outOfBounds())
        {
            generatingLevel = false;
            print("--------OUT OF BOUNDS--------" + "X: " + x + "Z: " + z);
            nextDirection = Directions.NONE;
        }*/
    }

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
            if (chance >= ForwardChance + BackwardChance + RightChance && chance <= ForwardChance + BackwardChance + RightChance + LeftChance)
            {
                return Directions.LEFT;
            }

            //up
            if (chance >= ForwardChance + BackwardChance + RightChance + LeftChance && chance <= ForwardChance + BackwardChance + RightChance + LeftChance + UpChance)
            {
                return Directions.UP;
            }

            //down
            if (chance >= ForwardChance + BackwardChance + RightChance + LeftChance + UpChance && chance <= ForwardChance + BackwardChance + RightChance + LeftChance + UpChance + DownChance)
            {
                return Directions.DOWN;
            }
        }

        return Directions.BLOCKED;
    }


    // getting possible directions via this method and the blocked methode.
    Directions getNextDirection()
    {
        Directions dir = GetChanceBasedDirections();
        bool notBlocked = false;



        // can forward
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
        // can back
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
        // can right
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
        // can left
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

        // can up
        if (x > 0 && dungeonLayout[x, z, y + 1] == 0 && lastPiece != null && lastPiece.name != "START" && lastPiece.name != "UP")
        {
            if (dir == Directions.UP)
            {
                return Directions.UP;
            }
            else if (UpChance > 0)
            {
                notBlocked = true;
            }
        }
        //can down
        if (x > 0 && dungeonLayout[x, z, y - 1] == 0 && lastPiece != null && lastPiece.name != "START" && lastPiece.name != "DOWN")
        {
            if (dir == Directions.DOWN)
            {
                return Directions.DOWN;
            }
            else if (DownChance > 0)
            {
                notBlocked = true;
            }
        }

        if (notBlocked && dir != Directions.BLOCKED)
        {
            return getNextDirection();
        }

        return Directions.BLOCKED;
    }

    //if out of bounds of the dungeon generation area return true;
    bool outOfBounds()
    {
        return x == xBounds - 1 || z == zBounds - 1 || y == yBounds || y < 0 || x < 0 || z < 0;
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
                //spawnPlayer(piece.transform.position + new Vector3(0, 3, 0));
            }
            //spawning end
            else
            {
                piece.name = "END";
                Component[] childs = piece.GetComponentsInChildren<Renderer>();
                foreach (var child in childs)
                {
                    child.GetComponent<Renderer>().material.color = Color.red;
                }
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
        else if (nextDirection == Directions.UP || nextDirection == Directions.DOWN)
        {
            if (nextDirection == Directions.UP)
            {
                piece = Instantiate(UpPiece, new Vector3(x * PIECE_SIZE, y * 5, z * PIECE_SIZE), Quaternion.identity);
                piece.name = "UP";
            }
            else
            {
                piece = Instantiate(DownPiece, new Vector3(x * PIECE_SIZE, y * 5, z * PIECE_SIZE), Quaternion.identity);
                piece.name = "DOWN";
            }

            if (placeDirection == Directions.RIGHT)
            {
                currentRotation = 0;
            }
            else if (placeDirection == Directions.LEFT)
            {
                currentRotation = 180;
            }
            else if (placeDirection == Directions.FORWARD)
            {
                currentRotation = 270;
            }
            else
            {
                currentRotation = 90;
            }
        }
        else if (placeDirection == Directions.DOWN || placeDirection == Directions.UP)
        {
            if (placeDirection == Directions.DOWN)
            {
                piece = Instantiate(UpPiece, new Vector3(x * PIECE_SIZE, y * 5, z * PIECE_SIZE), Quaternion.identity);
                piece.name = "UP";
            }
            else
            {
                piece = Instantiate(DownPiece, new Vector3(x * PIECE_SIZE, y * 5, z * PIECE_SIZE), Quaternion.identity);
                piece.name = "DOWN";
            }

            if (nextDirection == Directions.BACK)
            {
                currentRotation = 270;
            }
            else if (nextDirection == Directions.RIGHT)
            {
                currentRotation = 180;
            }
            else if (nextDirection == Directions.FORWARD)
            {
                currentRotation = 90;
            }
            else
            {
                currentRotation = 0;
            }
        }
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
        piece.transform.Rotate(0, currentRotation, 0);
        dungeonLayout[x, z, y] = 1;
        lastPos = new Vector3(x, 0, z);
        prevDirection = placeDirection;

        if (placeDirection == Directions.BLOCKED)
        {
            Destroy(lastPiece);
            placeDirection = Directions.NONE;
        }
        piece.transform.parent = parent.transform;
        Dungeon.Add(piece);
        lastPiece = piece;
        counter++;
    }

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
        //down
        if (nextDirection == Directions.UP)
        {
            nextPos.y--;
        }
        //up
        if (nextDirection == Directions.UP)
        {
            nextPos.y++;
        }

        return nextPos;
    }

    void spawnPlayer(Vector3 pos)
    {
        Instantiate(player, pos, Quaternion.identity);
    }
}

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