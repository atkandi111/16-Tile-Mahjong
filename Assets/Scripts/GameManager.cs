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
    public static Player[] Players = new Player[4];
    public static List<GameObject> TileSet = new List<GameObject>();
    public static List<GameObject> TileHands = new List<GameObject>();
    public static List<GameObject>[] TileWalls = new List<GameObject>[4];
    public static List<GameObject>[] TileBlocks = new List<GameObject>[4];

    public static Vector3 tileSize, tileOffset;
    private (Vector3, Quaternion)[] initialLoc = new (Vector3, Quaternion)[144];
    public enum FacePreset
    {
        Static, Stand = 180, Player = 220, Opened = 270, Closed = 90
    }

    void Awake()
    {   
        /* Create TileSet */
        /*string[] Names = new string[144]
        {
            "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b9",
            "s1", "s2", "s3", "s4", "s5", "s6", "s7", "s8", "s9",
            "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9",
            "dN", "dS", "dW", "dE", "tG", "tR", "tW", "fR", "fB"
        };*/

        Dictionary<string, IEnumerable<string>> constructor = new Dictionary<string, IEnumerable<string>>
        {
            { "b", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" } },
            { "s", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" } },
            { "c", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" } },
            { "d", new string[] { "N", "S", "W", "E" } },
            { "t", new string[] { "G", "R", "W" } },
            { "f", new string[] { "R", "B" } }
        };

        /* Create Prefabs
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
                    { temp = Instantiate(Resources.Load(tileName + "-" + tileID)) as GameObject; }
                    else
                    { temp = Instantiate(Resources.Load(tileName)) as GameObject; }

                    temp.name = tileName + "-" + tileID;
                    temp.transform.localScale = new Vector3(1600, 1600, 2000);
                    temp.transform.rotation = Quaternion.Euler(0, 0, 0);
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

        Vector3 position;
        Quaternion rotation;
        IEnumerator<Vector3> row;
        for (int PlayerID = 0; PlayerID < 4; PlayerID++)
        {
            FacePreset face = FacePreset.Closed;
            Quaternion quadrant = Quaternion.Euler(0, -90 * PlayerID, 0);

            /* Assign Tile Walls */
            row = DistributeRow(tileCount: 20, perimSize: 2.24f, face);
            for (int i = 0; i < 20; i++)
            {
                row.MoveNext();
                position = quadrant * (row.Current + new Vector3(tileOffset.y, 0, 0));
                rotation = quadrant * Quaternion.Euler((int) face, 0, random.Next(2) * 180);

                TileWalls[PlayerID][i].transform.position = position + new Vector3(0, 0.5f, 0);
                TileWalls[PlayerID][i].transform.rotation = rotation;

                // remove the transform.position above and separate it
                // instead initialize tilewalls here using same index as initial loc
                // do same in tileblocks

                initialLoc[PlayerID * 20 + i] = (position, rotation);
            }

            /* Assign Tile Blocks Layer 1 */
            row = DistributeRow(tileCount: 8, perimSize: 1.25f, face);
            for (int i = 0; i < 16; i++)
            {
                row.MoveNext();
                position = quadrant * row.Current;
                rotation = quadrant * Quaternion.Euler((int) face, 0, random.Next(2) * 180);

                TileBlocks[PlayerID][i].transform.position = position + new Vector3(0, 0.5f, 0);
                TileBlocks[PlayerID][i].transform.rotation = rotation;

                initialLoc[PlayerID * 16 + 80 + i] = (position, rotation);
            }

            /* Assign Tile Blocks Layer 2 */
            row = DistributeRow(tileCount: 8, perimSize: 1.25f, face);
            for (int i = 8; i < 16; i++)
            {
                row.MoveNext();
                position = quadrant * (row.Current + new Vector3(0, tileSize.z, 0));
                rotation = quadrant * Quaternion.Euler((int) face, 0, random.Next(2) * 180);

                TileBlocks[PlayerID][i].transform.position = position + new Vector3(0, 0.5f, 0);
                TileBlocks[PlayerID][i].transform.rotation = rotation;

                initialLoc[PlayerID * 16 + 80 + i] = (position, rotation);
            }
        }

        StartCoroutine(SetupTiles());
    }
    public static IEnumerator<Vector3> DistributeRow(int tileCount, float perimSize, FacePreset face)
    {
        float centerOffset = tileOffset.x - (tileCount * tileSize.x) / 2;
        float perimOffset = -tileOffset.y - perimSize;

        float liftOffset;
        switch (face)
        {
            case FacePreset.Player: liftOffset = 0.078f; break;
            case FacePreset.Stand:  liftOffset = tileOffset.y; break;
            case FacePreset.Opened: liftOffset = tileOffset.z; break;
            case FacePreset.Closed: liftOffset = tileOffset.z; break;
            default: liftOffset = tileOffset.z; break;
        }

        Vector3 position = new Vector3(centerOffset, liftOffset, perimOffset);
        Vector3 spacing = new Vector3(tileSize.x, 0, 0);

        for (int i = 0; i < tileCount; i++)
        {
            yield return position + (spacing * i);
        }
    }

    /*public static IEnumerator<Vector3> DistributeRow(int rowCount, int tileCount, float perimSize, Quaternion xRot)
    {
        Vector3 rotTileSize = xRot * tileSize;
        rotTileSize = new Vector3(Mathf.Abs(rotTileSize.x), Mathf.Abs(rotTileSize.y), Mathf.Abs(rotTileSize.z));
        Vector3 rotTileOffset = rotTileSize / 2;

        float centerOffset = (rowCount * rotTileSize.x) / 2;
        float perimOffset = perimSize + rotTileSize.z;

        Vector3 position = new Vector3(-centerOffset, 0, -perimOffset) + rotTileOffset;
        Vector3 spacing = new Vector3(rotTileSize.x, 0, 0);
        Vector3 lifting = new Vector3(0, rotTileSize.y, 0);
        
        for (int i = 0; i < tileCount; i++)
        {
            yield return position + (spacing * (i % rowCount)) + (lifting * (i / rowCount));
        }
    }*/

    IEnumerator SetupTiles()
    {
        for (int i = 0; i < TileSet.Count; i++)
        {
            yield return new WaitForSeconds(0.02f);
            TileSet[i].SetActive(true); 
            TileSet[i].GetComponent<TileManager>().SetDestination(initialLoc[i].Item1, initialLoc[i].Item2);
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                yield return new WaitForSeconds(0.4f);
                Players[i].GrabTile(TileHands.GetRange(i * 16 + j * 4, 4).ToArray());
            }
        }
        yield return StartCoroutine(NextTurn());
    }

    IEnumerator NextTurn()
    {
        //yield return new WaitUntil(() => noRunningSchedules);
        yield return null;
        foreach (GameObject tile in Players[0].Hand)
        {
            tile.AddComponent<HoverTile>(); // change to enable = true , and set void Awake() { enable = false; }
            tile.AddComponent<DragTile>();
            //tile.AddComponent<RotateTile>();
        }
    }
}

// create public class Tile that extends and inherits from GameObject
// compress list_tilewalls[] to list_tilewalls

/*
enum FacePresets
{
    Stand,
    Player,
    Opened,
    Closed,
    Static
}
*/

/*
for two-sided preset
hand must be on the left half and
open must be on the right half
because stepVector is positive, so that it's easier to left-align
*/