using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGeneration : MonoBehaviour
{
    public int xBounds, zBounds;

    public int ForwardChance;
    public int BackwardChance;
    public int LeftChance;
    public int RightChance;

    public bool UseSeed = false;
    public int seed;

    public int AmoutOfSteps = 0;

    public GameObject CorridorPiece;
    public GameObject CornerPiece;
    public GameObject StartEndPiece;
    public GameObject player;

    private static int PIECE_SIZE = 10;

    private int[,] dungeonLayout;
    private GameObject parent;

    private int x, z;
    private Directions nextDirection = Directions.NONE;
    private Directions prevDirection = 0;

    private int currentRotation = 0;
    private GameObject lastPiece;
    private Vector3 lastPos = Vector3.zero;
    private int chance;

    private int counter = 0;

    private bool generatingLevel = true;
    private bool startPlaced = false;

    private enum Directions { NONE, FORWARD, RIGHT, LEFT, BACK, BLOCKED, UP, DOWN,};

    void Start()
    {
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

        dungeonLayout = new int[xBounds, zBounds];

        if (UseSeed)
        {
            Random.InitState(seed);
        }
        else
        {
            seed = Random.Range(0,max: int.MaxValue);
            Random.InitState(seed);
        }

        if (AmoutOfSteps == 0)
        {
            AmoutOfSteps = int.MaxValue;
        }

        x = Random.Range(0, xBounds);
        z = Random.Range(0, zBounds / 2);

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
            default:
                break;
        }

        if (outOfBounds())
        {
            generatingLevel = false;
            print("--------OUT OF BOUNDS--------" + "X: " + x + "Z: " + z);
            nextDirection = Directions.NONE;
        }
    }

    private Directions GetChanceBasedDirections()
    {
        if (counter < AmoutOfSteps)
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
        


        // can forward
        if (z < zBounds - 1 && dungeonLayout[x, z + 1] == 0)
        {
            if (dir == Directions.FORWARD)
            {
                return Directions.FORWARD;
            }else if (ForwardChance > 0)
            {
                notBlocked = true;
            }
        }
        // can back
        if (z - 1 > 0 && dungeonLayout[x, z - 1] == 0)
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
        if (x < xBounds - 1 && dungeonLayout[x + 1, z] == 0)
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
        if (x > 0 && dungeonLayout[x - 1, z] == 0)
        {
            if (dir == Directions.LEFT)
            {
                return Directions.LEFT;
            }else if (LeftChance > 0)
            {
                notBlocked = true;
            }
        }

        if (notBlocked && dir != Directions.BLOCKED) {
            return getNextDirection();
        }

        return Directions.BLOCKED;
    }

    //if out of bounds of the dungeon generation area return true;
    bool outOfBounds()
    {
        return x == xBounds - 1 || z == zBounds - 1 || x < 0 || z < 0;
    }

    void createPart()
    {
        Directions placeDirection = nextDirection;
        nextDirection = getNextDirection();

        GameObject piece = null;

        // place end
        if (placeDirection == Directions.NONE || placeDirection == Directions.BLOCKED)
        {
            piece = Instantiate(StartEndPiece, new Vector3(x * PIECE_SIZE, 0, z * PIECE_SIZE), Quaternion.identity);
            //spawning start
            if (!startPlaced)
            {
                startPlaced = true;
                //spawnPlayer(piece.transform.position);
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

            }
            //spawning end
            else
            {
                if (lastPiece.name == "corner")
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
            piece = Instantiate(CornerPiece, new Vector3(x * PIECE_SIZE, 0, z * PIECE_SIZE), Quaternion.identity);
            piece.name = "corner";

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
        else
        {
            piece = Instantiate(CorridorPiece, new Vector3(x * PIECE_SIZE, 0, z * PIECE_SIZE), Quaternion.identity);
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
        dungeonLayout[x, z] = 1;
        lastPos = new Vector3(x, 0, z);
        prevDirection = placeDirection;

        if (placeDirection == Directions.BLOCKED)
        {
            Destroy(lastPiece);
            placeDirection = Directions.NONE;
        }
        piece.transform.parent = parent.transform;

        lastPiece = piece;
        counter++;
    }

    bool mustPlaceCorner(Directions placeDirection)
    {

        if (placeDirection != Directions.NONE)
        {
            if (lastPos.x != getNextPos(nextDirection).x && lastPos.z != getNextPos(nextDirection).z)
            {
                return true;
            }
        }
        return false;
    }
    Vector3 getNextPos(Directions nextDir)
    {
        Vector3 nextPos = new Vector3(x, 0, z);

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
        //down
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


