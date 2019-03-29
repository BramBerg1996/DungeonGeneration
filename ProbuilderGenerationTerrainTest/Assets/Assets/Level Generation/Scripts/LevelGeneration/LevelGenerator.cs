using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    /*
     * TIPS AND NOTES:
     * 
     * Default plane size: 10x10
     * 
     * */

    //Parent of all chucks
    public Transform chuckCollection;

    private Transform levelCollection;
    private static int PLANE_SIZE = 10;
    private List<GameObject> chuckTypes;

    public GameObject ground;
    public int WidthX, WidthZ;

    void Start()
    {
        Initialize();

        generateLevel();

        Destroy(this.gameObject);
    }

    private void Initialize()
    {
        if (ground == null)
            print("Ground is not set");

        levelCollection = new GameObject().transform;
        levelCollection.name = "Level";

        chuckTypes = getChunkTypes();
    }

    private List<GameObject> getChunkTypes() {
        List<GameObject> l = new List<GameObject>();

        for (int i = 0; i < chuckCollection.childCount; i++) {
            l.Add(chuckCollection.GetChild(i).gameObject);
        }

        return l;
    }

    void generateLevel()
    {
        for (int x = 0; x < WidthX; x += PLANE_SIZE) {
            for (int z = 0; z < WidthZ; z += PLANE_SIZE) {
                GameObject created = Instantiate(selectRandomChunk(), new Vector3(x, 0, z), Quaternion.identity);
                created.transform.parent = levelCollection;
                Debug.Log(created.GetComponent<Renderer>().bounds.size);
            }
        }
    }


    GameObject selectRandomChunk() {
        return chuckTypes[Random.Range(0, chuckTypes.Count)];
    }
}
