using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour // convert to singleton
{
    public static Player[] Players = new Player[4];
    public static string[] TileNames;
    public static GameObject[] TileSet = new GameObject[144]; // convert to private
    public static List<GameObject> TileWalls = new List<GameObject>();
    public static List<GameObject> TileToss = new List<GameObject>();

    public static Vector3 tileSize, tileOffset;

    public static Player currentPlayer;
    private int mano, diceRoll;

    void Awake()
    {   
        /* Create TileSet */
        TileNames = new string[]
        {
            "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b9",
            "s1", "s2", "s3", "s4", "s5", "s6", "s7", "s8", "s9",
            "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9",
            "tN", "tS", "tW", "tE", "tG", "tR", "tP", "fR", "fB"
        };

        GameObject temp;
        for (int i = 0; i < 144; i++)
        {
            string tileName = TileNames[i / 4];
            int tileID = (i % 4) + 1;
            if (tileName.Contains('f'))
            {
                temp = Instantiate(Resources.Load($"Tiles/{tileName}-{tileID}")) as GameObject;
            }
            else 
            {
                temp = Instantiate(Resources.Load($"Tiles/{tileName}")) as GameObject;
            }

            temp.name = tileName;
            temp.transform.localScale = new Vector3(1600, 1600, 2000);
            temp.transform.rotation = Quaternion.Euler(0, 0, 0);

            temp.AddComponent<TileManager>();
            temp.AddComponent<DragTile>();

            temp.GetComponent<TileManager>().enabled = false;
            temp.GetComponent<DragTile>().enabled = false;
            temp.SetActive(false);

            TileSet[i] = temp;
        }

        /* Set TileSize */
        Renderer renderer = TileSet[0].GetComponent<MeshRenderer>();
        tileSize = renderer.bounds.size;
        tileOffset = tileSize * 0.5f;

        Debug.Log(tileOffset);
        //. size : (tileWidth, tileLength, tileDepth)
        //. orientation : upright

        /* Create Players */
        GameObject table = GameObject.Find("Table Perimeter");

        /*Players[0] = new Player(0);
        Players[1] = new Player(1);
        Players[2] = new Player(2);
        Players[3] = new Player(3);*/

        Players[0] = table.AddComponent<Human>();
        Players[1] = table.AddComponent<Bot>();
        Players[2] = table.AddComponent<Bot>();
        Players[3] = table.AddComponent<Bot>();

        Players[0].playerID = 0;
        Players[1].playerID = 1;
        Players[2].playerID = 2;
        Players[3].playerID = 3;

        mano = 0;
        table.AddComponent<DragToss>();
        Camera.main.GetComponent<KeyboardListener>().enabled = false;
    }
    
    void Start()
    {
        NewRound();
    }

    void NewRound()
    {        
        /* Initialize Engine KnowledgeBase */
        foreach (Player p in Players)
        {
            p.engine.knowledgeBase = TileNames.ToDictionary(name => name, _ => 4);
        }

        /* Shuffle TileSet using Fisher-Yates */
        Random random = new Random();
        for (int i = TileSet.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            GameObject temp = TileSet[i];
            TileSet[i] = TileSet[j];
            TileSet[j] = temp;
        }

        // TileSet = TestRearrange(TileSet, new List<string>() {"tN", "tW", "tE", "tG"});

        // declare diceroll here

        /* Set Tile Positions */
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        IEnumerator<Vector3> row;
        for (int PlayerID = 0; PlayerID < 4; PlayerID++)
        {
            Quaternion quadrant = Quaternion.Euler(0, -90 * PlayerID, 0);

            /* Assign Tile Walls */
            row = Positioner.DistributeRow(tileCount: 20, perimSize: Perimeter.WallArea, upright: false);
            for (int i = 0; i < 20; i++)
            {
                row.MoveNext();
                Vector3 position = quadrant * (row.Current + new Vector3(tileOffset.y, 0, 0));
                Quaternion rotation = quadrant * Quaternion.Euler(Positioner.FaceDown, 0, random.Next(2) * 180);

                TileSet[i + PlayerID * 20].transform.position = position + new Vector3(0, 0.5f, 0);
                TileSet[i + PlayerID * 20].transform.rotation = rotation;
            }

            /* Assign Tile Blocks Layer 1 */
            row = Positioner.DistributeRow(tileCount: 8, perimSize: Perimeter.BlockArea, upright: false);
            for (int i = 80; i < 88; i++)
            {
                row.MoveNext();
                Vector3 position = quadrant * row.Current;
                Quaternion rotation = quadrant * Quaternion.Euler(Positioner.FaceDown, 0, random.Next(2) * 180);

                TileSet[i + PlayerID * 16].transform.position = position + new Vector3(0, 0.5f, 0);
                TileSet[i + PlayerID * 16].transform.rotation = rotation;
            }

            /* Assign Tile Blocks Layer 2 */
            row = Positioner.DistributeRow(tileCount: 8, perimSize: Perimeter.BlockArea, upright: false);
            for (int i = 88; i < 96; i++)
            {
                row.MoveNext();
                Vector3 position = quadrant * (row.Current + new Vector3(0, tileSize.z, 0));
                Quaternion rotation = quadrant * Quaternion.Euler(Positioner.FaceDown, 0, random.Next(2) * 180);

                TileSet[i + PlayerID * 16].transform.position = position + new Vector3(0, 0.5f, 0);
                TileSet[i + PlayerID * 16].transform.rotation = rotation;
            }
        }

        stopwatch.Stop();
        TimeSpan elapsedTime = stopwatch.Elapsed;
        Debug.Log("Elapsed Time: " + elapsedTime);

        StartCoroutine(SetupTiles());
    }
    IEnumerator SetupTiles()
    { 
        currentPlayer = Players[mano];
        diceRoll = 12;
        CompressWalls(diceRoll, mano);

        foreach (GameObject tile in TileSet)
        {
            // yield return new WaitForSeconds(0.02f);
            tile.SetActive(true);

            Vector3 position = tile.transform.position - new Vector3(0, 0.5f, 0);
            Quaternion rotation = tile.transform.rotation;

            tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.2f);
        }

        yield return new WaitForSeconds(0.02f);

        diceRoll = 12;
        for (int i = 0; i < 4; i++)
        {
            int playerID = (i + diceRoll - 1) % 4;
            for (int j = 0; j < 4; j++)
            {
                yield return new WaitForSeconds(0.3f);
                Players[playerID].GrabTile(new List<GameObject>() {
                    TileSet[80 + (i * 2) + (j * 16)],
                    TileSet[81 + (i * 2) + (j * 16)],
                    TileSet[88 + (i * 2) + (j * 16)],
                    TileSet[89 + (i * 2) + (j * 16)],
                });
            }

        }

        /* Wait for setup to finish */
        // use async OR unityEvent
        foreach (GameObject tile in TileSet)
        {
            while (tile.GetComponent<TileManager>().enabled)
            {
                yield return null;
            }
        }

        // yield return
        StackFlower();
        // yield return new WaitForSeconds(0.5f);

        currentPlayer.GrabTile(TileWalls[0]);
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(InitialFlowerRound());
        // call AnalyzeHand for each Player
    }

    public IEnumerator InitialFlowerRound()
    {
        List<GameObject>[] initialFlowers = new List<GameObject>[4];
        
        do 
        { 
            for (int i = mano; i < mano + 4; i++)
            {
                initialFlowers[i % 4] = Players[i % 4].Hand
                    .Where(tile => tile.name.StartsWith("f"))
                    .ToList();

                if (initialFlowers[i % 4].Count > 0)
                {
                    Players[i % 4].OpenTile(initialFlowers[i % 4]);
                    yield return new WaitForSeconds(0.7f); // 0.3 * 2 + 0.1 (allowance)
                }
            }
        } while (initialFlowers.Any(flwrList => flwrList.Count > 0));

        foreach (Player player in Players)
        {
            player.engine.ChooseDiscard(); // change to analyze hand
        }
    }

    public static IEnumerator PreTossBuffer()
    {
        GameObject nextTile = TileWalls[0];
        currentPlayer.GrabTile(nextTile);

        int currentIndex = Array.IndexOf(Players, currentPlayer);
        float elapsedTime = 0f, decisionTime = 3f;

        while (elapsedTime < decisionTime)
        {
            if (currentPlayer.WillWin(nextTile))
            {
                AudioManager.Instance.playWin();
                MeldText.Instance.OnWin(currentIndex);

                Debug.Log("BUNOT");
                yield break;
            }

            if (currentPlayer.WillKang(nextTile)) // && not round's first move
            {
                AudioManager.Instance.playKang();
                MeldText.Instance.OnKang(currentIndex);

                List<GameObject> kangBlock = currentPlayer.Hand.Where(go => go.name == nextTile.name).ToList();
                kangBlock.Add(nextTile);
                TileToss.Remove(nextTile);

                currentPlayer.ScrtTile(kangBlock);
                // currentPlayer.ChooseDiscard();
                elapsedTime = 0f;
                yield break;
            }

            PlayerLight.Instance.enabled = true;  

            if (elapsedTime - decisionTime >= 3f && elapsedTime - decisionTime <= 3.25f)
                PlayerLight.Instance.enabled = false;

            if (elapsedTime - decisionTime >= 2f && elapsedTime - decisionTime <= 2.25f)
                PlayerLight.Instance.enabled = false;
            
            if (elapsedTime - decisionTime >= 1f && elapsedTime - decisionTime <= 1.25f)
                PlayerLight.Instance.enabled = false;
        }
    }
    public static IEnumerator PostTossBuffer(GameObject tile) // postturn
    {
        float elapsedTime = 0f, decisionTime = 3f;

        Camera.main.GetComponent<KeyboardListener>().enabled = true;
        while (elapsedTime < decisionTime && !Input.GetKeyDown(KeyCode.RightArrow))
        {
            elapsedTime += Time.deltaTime;
            yield return null;

            // change to yield return WaitForSeconds
        }
        Camera.main.GetComponent<KeyboardListener>().enabled = false;

        for (int i = 0; i < 4; i++)
        {
            MeldText.Instance.Hide(i);
        }

        // measure time performance
        int currentIndex = Array.IndexOf(Players, currentPlayer);
        for (int i = currentIndex + 1; i < currentIndex + 4; i++)
        {
            if (Players[i % 4].WillWin(tile))
            {
                AudioManager.Instance.playWin();
                PlayerLight.Instance.UpdateLight(i % 4);
                MeldText.Instance.OnWin(i % 4);
                currentPlayer = Players[i % 4];
                Debug.Log("WINNER");
                yield break;
            }
        }

        for (int i = currentIndex + 1; i < currentIndex + 4; i++)
        {
            if (Players[i % 4].WillKang(tile))
            {
                AudioManager.Instance.playKang();
                PlayerLight.Instance.UpdateLight(i % 4);
                MeldText.Instance.OnKang(i % 4);
                currentPlayer = Players[i % 4];

                List<GameObject> kangBlock = currentPlayer.Hand.Where(go => go.name == tile.name).ToList();
                kangBlock.Add(tile);
                TileToss.Remove(tile);

                currentPlayer.OpenTile(kangBlock);
                // currentPlayer.ChooseDiscard();
                yield break;
            }

            if (Players[i % 4].WillPong(tile))
            {
                AudioManager.Instance.playPong();
                PlayerLight.Instance.UpdateLight(i % 4);
                MeldText.Instance.OnPong(i % 4);
                currentPlayer = Players[i % 4];

                List<GameObject> pongBlock = currentPlayer.Hand.Where(go => go.name == tile.name).ToList();
                pongBlock.Add(tile);
                TileToss.Remove(tile);

                currentPlayer.OpenTile(pongBlock);
                if (currentPlayer != Players[0]) // transfer to bot's grab tile
                    currentPlayer.ChooseDiscard();
                yield break;
            }
        }

        if (Players[(currentIndex + 1) % 4].WillChao(tile))
        {
            AudioManager.Instance.playChao();
            PlayerLight.Instance.UpdateLight((currentIndex + 1) % 4);
            MeldText.Instance.OnChao((currentIndex + 1) % 4);
            currentPlayer = Players[(currentIndex + 1) % 4];

            (string name1, string name2) = currentPlayer.engine.ChooseChao(tile.name);
            GameObject tile1 = currentPlayer.Hand.First(go => go.name == name1);
            GameObject tile2 = currentPlayer.Hand.First(go => go.name == name2);
            List<GameObject> chaoBlock = new List<GameObject>() { tile, tile1, tile2 };
            TileToss.Remove(tile);

            currentPlayer.OpenTile(chaoBlock);
            if (currentPlayer != Players[0])
                currentPlayer.ChooseDiscard();
            yield break;
        }

        NextTurn();
    }

    public static void NextTurn() // convert to coroutine
    {
        int currentIndex = (Array.IndexOf(Players, currentPlayer) + 1) % 4;

        PlayerLight.Instance.UpdateLight( currentIndex );
        currentPlayer = Players[currentIndex];
        currentPlayer.GrabTile(TileWalls[0]);
    }
    void CompressWalls(int diceRoll, int mano)
    {
        int wallIndex = (mano + (diceRoll % 4)) * 20;
        int tileIndex = (20 - diceRoll);

        int index = wallIndex + tileIndex + 1;

        TileWalls.AddRange(TileSet[0..index].Reverse());
        TileWalls.AddRange(TileSet[index..(80)].Reverse());
    }

    public static void StackFlower()
    {
        GameObject nextTile = TileWalls[TileWalls.Count - 2];
        GameObject thisTile = TileWalls[TileWalls.Count - 1];

        Vector3 position = nextTile.transform.position + new Vector3(0, tileSize.z, 0);
        Quaternion rotation = thisTile.transform.rotation;
        thisTile.GetComponent<TileManager>().SetDestination(position, rotation, 0.1f);
    }

    /*public static void UpdateAllKB(string tile)
    {
        foreach (Player p in Players)
        {
            p.engine.UpdateKB(tile);
        }
    }*/

    void Update()
    {
        // put this back
        // it's better if player will just press next arrow before next player grab tile
        // so that there's control on pong, eat

        /*if (Input.GetKeyDown(KeyCode.RightArrow) && currentTurnRunning != true)
        {
            currentPlayer = Players[(Array.IndexOf(Players, currentPlayer) + 1) % 4];
            // StartCoroutine(NextTurn());
            NextTurn();
        }*/
    }

    GameObject[] TestRearrange(GameObject[] tiles, List<string> targetName)
    {
        List<GameObject> tileList = new List<GameObject>(tiles);
        List<GameObject> tileMover = new List<GameObject>();

        foreach (string name in targetName)
        {
            for (int i = tileList.Count - 1; i >= 0; i--)
            {
                if (tileList[i].name == name)
                {
                    tileMover.Insert(0, tileList[i]);
                    tileList.RemoveAt(i);
                }
            }
        }

        tileList.Insert(82, tileMover[0]);
        tileList.Insert(82, tileMover[1]);
        tileList.Insert(90, tileMover[2]);
        tileList.Insert(90, tileMover[3]);

        tileList.Insert(98, tileMover[4]);
        tileList.Insert(98, tileMover[5]);
        tileList.Insert(106, tileMover[6]);
        tileList.Insert(106, tileMover[7]);

        tileList.Insert(114, tileMover[8]);
        tileList.Insert(114, tileMover[9]);
        tileList.Insert(122, tileMover[10]);
        tileList.Insert(122, tileMover[11]);

        tileList.Insert(130, tileMover[12]);
        tileList.Insert(130, tileMover[13]);
        tileList.Insert(138, tileMover[14]);
        tileList.Insert(138, tileMover[15]);
        
       
        return tileList.ToArray();
    }

}

public static class Positioner // convert to monobehavior and merge w tosstile
{
    public const int Stand = 180;
    public const int FaceUp = 270;
    public const int FaceDown = 90;

    public static IEnumerator<Vector3> DistributeRow(int tileCount, float perimSize, bool upright = true)
    {
        float centerOffset = GameManager.tileOffset.x - (tileCount * GameManager.tileSize.x) / 2;
        float perimOffset = -GameManager.tileOffset.y - perimSize;
        float liftOffset = (upright) ? (GameManager.tileOffset.y) : (GameManager.tileOffset.z);

        Vector3 position = new Vector3(centerOffset, liftOffset, perimOffset);
        Vector3 spacing = new Vector3(GameManager.tileSize.x, 0, 0);

        for (int i = 0; i < tileCount; i++)
        {
            yield return position + (spacing * i);
        }
    }
}

public static class Perimeter
{
    public const float BlockArea = 1.25f;
    public const float WallArea = 2.24f;
    public const float OpenArea = 2.75f;
    public const float HandArea = 3.22f;
}



// create public class Tile that extends and inherits from GameObject
// compress list_tilewalls[] to list_tilewalls

/*
for two-sided preset
hand must be on the left half and
open must be on the right half
because stepVector is positive, so that it's easier to left-align
*/

// instead of quadrant, use look at

// replace TileManager with public class Tile


// maybe findMeld is unnecessary

// remove dragtoss when opening tile

// BUG when dragging ponged tile as it is being taken


// BUG when clicking right arrow when ponged tile is being taken

// bake light on table perimeter and then just rotate tableperim

// BUG when engine has a pair, breaks the pair, but doesn't realize that pair doesn't exist anymore
// because engine still pongs the pair even after breaking it
// because prePong and preChow are not updated after choosing a toss


// convert currentplayer to int


// CONVERT TO EVENTS:
/*
OnKang(), OnPong(), OnChao()
*/

// bug if chao an earlier pong.
// e.g. existing openchao == 5 6 7.
// if pong 7, bot will think it's kang

// doesn't kang an existing pong