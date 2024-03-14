using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations; //
using UnityEngine;
using UnityEditor;
using Random = System.Random;
using System.Runtime.CompilerServices; //
using System.Numerics;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2; //
using Vector3 = UnityEngine.Vector3;

using System.Diagnostics;
using Debug = UnityEngine.Debug;
public class GameManager : MonoBehaviour
{
    public static Player[] Players = new Player[4];
    public static List<GameObject> TileSet = new List<GameObject>();
    public static List<GameObject> TileHands = new List<GameObject>();
    public static List<GameObject>[] TileWalls = new List<GameObject>[4];
    public static List<GameObject>[] TileBlocks = new List<GameObject>[4];

    public static Vector3 tileSize, tileOffset;
    public enum FacePreset
    {
        Static, Stand = 180, Player = 220, Opened = 270, Closed = 90
    }

    void Awake()
    {   
        /* Create TileSet */
        string[] names = 
        {
            "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b9",
            "s1", "s2", "s3", "s4", "s5", "s6", "s7", "s8", "s9",
            "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9",
            "dN", "dS", "dW", "dE", "tG", "tR", "tW", "fR", "fB"
        };

        GameObject temp;
        foreach (string tileName in names)
        {
            for (int tileID = 1; tileID <= 4; tileID++)
            {
                /* if no PreFabs yet */
                // temp = Instantiate(Resources.Load("TileVariant")) as GameObject;

                /* if PreFabs exist */
                if (tileName.Contains('f')) 
                {
                    temp = Instantiate(Resources.Load(tileName + "-" + tileID)) as GameObject;
                } 
                else 
                {
                    temp = Instantiate(Resources.Load(tileName)) as GameObject;
                }

                //temp.name = tileName + "-" + tileID;
                temp.name = tileName;

                temp.transform.localScale = new Vector3(1600, 1600, 2000);
                temp.transform.rotation = Quaternion.Euler(0, 0, 0);

                temp.AddComponent<TileManager>();
                temp.AddComponent<DragTile>();

                temp.GetComponent<TileManager>().enabled = false;
                temp.GetComponent<DragTile>().enabled = false;
                temp.SetActive(false);

                TileSet.Add(temp);
            }
        }

        /* Set TileSize */
        Renderer renderer = TileSet[0].GetComponent<MeshRenderer>();
        tileSize = renderer.bounds.size;
        tileOffset = tileSize * 0.5f;

        //. size : (tileWidth, tileLength, tileDepth)
        //. orientation : upright


        /* Create Prefabs (if not existing) 
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
        } */

        /* Create Players */
        Players[0] = new Human(0, "Player 1");
        Players[1] = new Bot(1, "Player 2");
        Players[2] = new Bot(2, "Player 3");
        Players[3] = new Bot(3, "Player 4");
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

        /* Assign TileHands */
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                int quadIndex = (j / 4);
                int tileIndex = (2 * (4 - i - 1)) + (j % 2) + ((j % 4) / 2) * 8;
                TileHands.Add(TileBlocks[quadIndex][tileIndex]);

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
            }
        }

        /* Set Tile Positions */
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
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
                Vector3 position = quadrant * (row.Current + new Vector3(tileOffset.y, 0, 0));
                Quaternion rotation = quadrant * Quaternion.Euler((int) face, 0, random.Next(2) * 180);

                TileWalls[PlayerID][i].transform.position = position + new Vector3(0, 0.5f, 0);
                TileWalls[PlayerID][i].transform.rotation = rotation;
            }

            /* Assign Tile Blocks Layer 1 */
            row = DistributeRow(tileCount: 8, perimSize: 1.25f, face);
            for (int i = 0; i < 16; i++) // i < 8
            {
                row.MoveNext();
                Vector3 position = quadrant * row.Current;
                Quaternion rotation = quadrant * Quaternion.Euler((int) face, 0, random.Next(2) * 180);

                TileBlocks[PlayerID][i].transform.position = position + new Vector3(0, 0.5f, 0);
                TileBlocks[PlayerID][i].transform.rotation = rotation;
            }

            /* Assign Tile Blocks Layer 2 */
            row = DistributeRow(tileCount: 8, perimSize: 1.25f, face);
            for (int i = 8; i < 16; i++)
            {
                row.MoveNext();
                Vector3 position = quadrant * (row.Current + new Vector3(0, tileSize.z, 0));
                Quaternion rotation = quadrant * Quaternion.Euler((int) face, 0, random.Next(2) * 180);

                TileBlocks[PlayerID][i].transform.position = position + new Vector3(0, 0.5f, 0);
                TileBlocks[PlayerID][i].transform.rotation = rotation;
            }
        }

        stopwatch.Stop();
        TimeSpan elapsedTime = stopwatch.Elapsed;

        // Output the elapsed time
        Debug.Log("Elapsed Time: " + elapsedTime);

        StartCoroutine(SetupTiles());
    }
    public static IEnumerator<Vector3> DistributeRow(int tileCount, float perimSize, FacePreset face)
    {
        float centerOffset = tileOffset.x - (tileCount * tileSize.x) / 2;
        float perimOffset = -tileOffset.y - perimSize;

        float liftOffset;
        switch (face)
        {
            case FacePreset.Player: liftOffset = 0.11f; break;
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

    IEnumerator SetupTiles()
    { 
        for (int i = 0; i < TileSet.Count; i++)
        {
            yield return new WaitForSeconds(0.02f);
            TileSet[i].SetActive(true); 

            Vector3 position = TileSet[i].transform.position - new Vector3(0, 0.5f, 0);
            Quaternion rotation = TileSet[i].transform.rotation;

            TileSet[i].GetComponent<TileManager>().SetDestination(position, rotation, 0.2f);
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                yield return new WaitForSeconds(0.3f);
                Players[i].GrabTile(TileHands.GetRange((i * 16) + (j * 4), 4));
            }
        }

        /* Wait for setup to finish */
        foreach (GameObject tile in TileSet)
        {
            while (tile.GetComponent<TileManager>().enabled)
            {
                yield return null;
            }
        }

        // Players[0].AddComponent<SortTile>();
        yield return StartCoroutine(NextTurn());
    }

    IEnumerator NextTurn()
    {
        foreach (GameObject tile in Players[0].Hand)
        {
            tile.AddComponent<HoverTile>();
        }
        yield return null;
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

// instead of quadrant, use look at