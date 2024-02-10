using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;
using Random = System.Random;
using System.Runtime.CompilerServices;
using System.Numerics;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GameManager : MonoBehaviour
{
    public static Vector3 tileSize, tileOffset;
    public static Player[] Players = new Player[4];
    public static List<GameObject> TileSet = new List<GameObject>();
    public static List<GameObject>[] TileWalls = new List<GameObject>[4];
    public static List<GameObject>[] TileBlocks = new List<GameObject>[4];
    public static bool noRunningSchedules = true;
    private Random random = new Random();
    private List<V

    void Awake()
    {   
        Vector3 test = new Vector3(0, 0, -1);

        /* Create TileSet */
        Dictionary<string, IEnumerable<string>> constructor = new Dictionary<string, IEnumerable<string>>
        {
            { "b", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" } },
            { "s", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" } },
            { "c", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" } },
            { "d", new string[] { "N", "S", "W", "E" } },
            { "t", new string[] { "G", "R", "W" } },
            { "f", new string[] { "R", "B" } }
        };

        /*
        foreach (string suit in constructor.Keys)
        {
            foreach (string unit in constructor[suit])
            {
                for (int tileID = 1; tileID <= 4; tileID++)
                {
                    string tileName = suit + unit;
                    // if no PreFabs yet
                    // temp = Instantiate(Resources.Load("TileVariant")) as GameObject;

                    // if PreFabs exist 
                    GameObject visualRepr;
                    if (suit == "f") 
                    { 
                        visualRepr = Instantiate(Resources.Load(tileName + "-" + tileID)) as GameObject; 
                    }
                    else
                    { 
                        visualRepr = Instantiate(Resources.Load(tileName)) as GameObject;
                    }

                    visualRepr.name = "VisualRepr";
                    visualRepr.transform.localScale = new Vector3(1600, 1600, 2000);
                    visualRepr.transform.localRotation = Quaternion.Euler(0, 0, 0);

                    tileSize = visualRepr.GetComponent<MeshRenderer>().bounds.size;

                    GameObject temp = new GameObject(tileName + "-" + tileID);
                    temp.transform.localScale = transform.InverseTransformVector(tileSize);
                    visualRepr.transform.parent = temp.transform;

                    BoxCollider boxCollider = temp.AddComponent<BoxCollider>();
                    boxCollider.size = boxCollider.transform.InverseTransformVector(tileSize);
                    boxCollider.isTrigger = false;

                    //temp.transform.localScale = new Vector3(1600, 1600, 2000);
                    temp.AddComponent<TileManager>();
                    temp.GetComponent<TileManager>().enabled = false; // set in script
                    temp.SetActive(false);
                    TileSet.Add(temp);
                }
            }
        }
        */
        
        GameObject temp;
        foreach (string suit in constructor.Keys)
        {
            foreach (string unit in constructor[suit])
            {
                for (int tileID = 1; tileID <= 4; tileID++)
                {
                    string tileName = suit + unit;
                    // if no PreFabs yet
                    // temp = Instantiate(Resources.Load("TileVariant")) as GameObject;

                    // if PreFabs exist
                    if (suit == "f") 
                    { 
                        temp = Instantiate(Resources.Load(tileName + "-" + tileID)) as GameObject; 
                    }
                    else
                    { 
                        temp = Instantiate(Resources.Load(tileName)) as GameObject;
                    }

                    temp.name = tileName + "-" + tileID;
                    temp.transform.localScale = new Vector3(1600, 1600, 2000);
                    temp.transform.rotation = Quaternion.Euler(-90, 0, random.Next(2) * 180);
                    temp.AddComponent<TileManager>();
                    temp.GetComponent<TileManager>().enabled = false;
                    temp.SetActive(false);
                    TileSet.Add(temp);
                }
            }
        }

        /* Set TileSize */
        Renderer renderer = TileSet[0].GetComponent<MeshRenderer>();
        tileSize = renderer.bounds.size;
        tileOffset = tileSize * 0.5f;

        /* Create Prefabs (if not existing) */
        /*
        Material tileCrack = (Material) Resources.Load("Tile Crack");
        Material tileBack = (Material) Resources.Load("Tile Back");
        foreach (GameObject tile in TileSet)
        {
            string imagePath = tile.name;
            if (imagePath[0] != 'f')
            {
                imagePath = imagePath.Substring(0, 2);
            }
            
            Material tileFace = (Material) Resources.Load("Albedo Map/Materials/" + imagePath);
            Texture2D tileBump = (Texture2D) Resources.Load("Height Map/" + imagePath);
            GameObject prefabInstance = Instantiate(Resources.Load("TileVariant")) as GameObject;
            Renderer prefabRenderer = prefabInstance.GetComponent<Renderer>();

            Material[] materials = prefabRenderer.materials;
            materials[0] = tileFace;
            materials[0].SetTexture("_BumpMap", tileBump);
            materials[0].SetTextureScale("_MainTex", new Vector2(1.2f, 1f));
            materials[0].SetTextureOffset("_MainTex", new Vector2(0f, 0f));

            materials[1] = tileCrack;
            materials[2] = tileBack;
            prefabRenderer.materials = materials;
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, String.Format("Assets/Objects/Resources/{0}.prefab", imagePath));
            Destroy(prefabInstance);
        } 
        */

        /* Create Players */
        for (int i = 0; i < 4; i++)
        {
            Players[i] = ScriptableObject.CreateInstance<Player>();
            Players[i].Initialize(i, String.Format("Player {0}", i));
        }
    }
    void Start()
    {
        NewRound();
    }
    void NewRound()
    {        
        /* Shuffle TileSet using Fisher-Yates */
        Random random = new Random();
        for (int i = TileSet.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            GameObject temp = TileSet[i];
            TileSet[i] = TileSet[j];
            TileSet[j] = temp;
        }

        /* Assign TileWalls and TileBlocks */
        for (int i = 0; i < 4; i++)
        {
            TileWalls[i] = TileSet.GetRange(i * 20, 20);
            TileBlocks[i] = TileSet.GetRange(i * 16 + 80, 16);
        }

        List<GameObject> TileHands = new List<GameObject>();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                /*
                origIndex = 0.1.2.3.4.5.6.7.8.9............
                quadIndex = 0.0.0.0.1.1.1.1.2.2.2.2.3.3.3.3
                tileIndex = 0.1.8.9.0.1.8.9.0.1.8.9.0.1.8.9
                     sums
                     _%_2   0.1.0.1.0.1.0.1.0.1.0.1.0.1.0.1
                     _/_2   0.0.8.8.0.0.8.8.0.0.8.8.0.0.8.8
                  reverse   taking right-left, [i] => [4 - i]
                  cluster   taking lifted x8, taking adjacent x2
                */

                int quadIndex = (j / 4);
                int tileIndex = (2 * (4 - i - 1)) + (j % 2) + ((j % 4) / 2) * 8;
                TileHands.Add(TileBlocks[quadIndex][tileIndex]);
            }
        }

        /* Set Tile Positions
        for (int i = 0; i < 4; i++)
        {
            PositionManager.AssignPosition(TileWalls[i], PlayerID: i, tileState: "Closed", numTiles: 20, perimSize: 2.24f, startOffset: tileOffset.z);
            PositionManager.AssignPosition(TileBlocks[i], PlayerID: i, tileState: "Closed", numTiles: 8, perimSize: 1.25f);
        }

        for (int i = 0; i < 4; i++)
        {
            Players[i].GrabTile(TileHands.GetRange(i * 16, 16).ToArray());
        }

        foreach (GameObject tile in TileSet)
        {
            (Vector3, Quaternion) target = tile.GetComponent<TileManager>().GetDestination();
            tile.transform.position = target.Item1 + new Vector3(0, 0.5f, 0);
            tile.transform.rotation = target.Item2;
        }

        // Schedule Animation
        PositionManager.ScheduleEvent(duration: 0.02f, cluster: 1, tileArray: TileSet);
        PositionManager.ScheduleEvent(duration: 0.1f, cluster: 4, tileArray: TileHands);
        */

        for (int playerID = 0; playerID < 4; playerID++)
        {
            Quaternion quadrant = Quaternion.Euler(0, -90 * playerID, 0);

            /* Assign Tile Walls */

            Vector3 wallVector = quadrant * new Vector3(0, 0, -1) * (perimSize + tileOffset.z);
            Vector3 tileVector = quadrant * new Vector3(-1, 0, 0) * (tileSize.x * (numTiles / 2) - tileOffset.x);


        }

        StartCoroutine(NextTurn());
    }

    IEnumerator NextTurn()
    {
        yield return new WaitUntil(() => noRunningSchedules);
        foreach (GameObject tile in Players[0].Hand)
        {
            tile.AddComponent<HoverTile>(); // change to enable = true , and set void Awake() { enable = false; }
            tile.AddComponent<DragTile>();
            //tile.AddComponent<RotateTile>();
        }
    }
}

// create public class Tile that extends and inherits from GameObject