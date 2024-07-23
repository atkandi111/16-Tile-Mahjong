using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;
using System.Threading.Tasks;
// using GameManager.FacePreset

// Playerprefs Built-in class

public abstract class Player : MonoBehaviour
{
    public List<GameObject> Hand = new List<GameObject>();
    public List<GameObject> Open = new List<GameObject>();
    public Engine engine;

    public string playerName;
    public int playerID, coins = 0, wins = 0;


    // try private
    public abstract Quaternion quadrant { get; }
    public abstract Vector3 viewpoint { get; }
    public abstract float groundNegative { get; }

    public abstract bool WillWin(GameObject tile);
    public abstract bool WillChao(GameObject tile);
    public abstract bool WillPong(GameObject tile);
    public abstract bool WillKang(GameObject tile);

    public Player(int playerID = 0, string playerName = "")
    {
        this.playerID = playerID;
        this.playerName = playerName;
        this.engine = new Engine(this); // transfer to bot
    }

    public IEnumerator DelayAction(params Action[] postActions)
    {
        yield return new WaitForSeconds(0.3f);
        foreach (var postAction in postActions)
        {
            postAction();
        }
    }


    public void ChooseDiscard()
    {
        StartCoroutine(DelayAction(() => {
            string discardName = this.engine.ChooseDiscard();

            var _hand = Hand
                .Select(tile => tile.name)
                .OrderBy(name => name)
                .ToList();

            string handstr = "";
            foreach (string name in _hand)
            {
                handstr += name + " ";
            }
            Debug.Log(handstr + "-> " + discardName);

            GameObject discard = Hand.First(tile => tile.name == discardName);
            TossTile(discard);
        }));
    }
    public void DistributeHand()
    {
        IEnumerator<Vector3> row = Positioner.DistributeRow(Hand.Count, Perimeter.HandArea);
        foreach (GameObject tile in Hand)
        {
            row.MoveNext();
            Vector3 position = this.quadrant * (row.Current - new Vector3(0, groundNegative, 0));
            Quaternion rotation = tile.GetComponent<DragTile>().baseRotation;

            tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.1f);
        }
    }
    public void DistributeOpen()
    {
        IEnumerator<Vector3> row = Positioner.DistributeRow(Open.Count, Perimeter.OpenArea, upright: false);
        foreach (GameObject tile in Open)
        {
            row.MoveNext();
            Vector3 position = this.quadrant * row.Current;
            Quaternion rotation = this.quadrant * Quaternion.Euler(Positioner.FaceUp, 0, 180);
            
            tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.05f);
        }
    }

    public void DistributeScrt()
    {
        IEnumerator<Vector3> row = Positioner.DistributeRow(Open.Count, Perimeter.OpenArea, upright: false);
        foreach (GameObject tile in Open)
        {
            row.MoveNext();
            Vector3 position = this.quadrant * row.Current;
            Quaternion rotation = this.quadrant * Quaternion.Euler(Positioner.FaceUp, 0, 180); // FaceDown
            
            tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.05f);
        }
    }

    public virtual void GrabTile(GameObject tile) => GrabTile(new List<GameObject> { tile });
    public virtual void GrabTile(List<GameObject> tiles)
    {
        Hand.AddRange(tiles);
        GameManager.TileWalls.RemoveAll(tiles.Contains);
        foreach (GameObject tile in tiles)
        {
            this.engine.UpdateKB(tile.name);
        }

        foreach (GameObject tile in tiles)
        {
            Quaternion xRot = Quaternion.LookRotation(viewpoint);
            Quaternion zRot = Quaternion.Euler(0, 0, new Random().Next(2) * 180);

            // transfer to Human
            tile.GetComponent<DragTile>().baseRotation = xRot * zRot;
        }

        AudioManager.Instance.soundMovement();
        DistributeHand();
    }

    public virtual void OpenTile(GameObject tile) => OpenTile(new List<GameObject> { tile });
    public virtual void OpenTile(List<GameObject> tiles)
    {
        var newlyOpenedTiles = Hand.Where(obj1 => tiles.Contains(obj1, EqualityComparer<GameObject>.Default));

        Open.AddRange(tiles);
        Hand.RemoveAll(tiles.Contains);

        foreach (GameObject tile in newlyOpenedTiles)
        {
            foreach (Player p in GameManager.Players)
            {
                if (p == this)
                    continue;
                
                p.engine.UpdateKB(tile.name);
            }
        }
        foreach (GameObject tile in tiles) // opposite of newlyOpenedTiles
        {
            tile.GetComponent<BoxCollider>().isTrigger = true;
        }

        DistributeOpen();
        DistributeHand();
        StartCoroutine(DelayAction(() => 
        {
            string name = tiles[0].name;
            int flowerCount = tiles.Count(tile => tile.name.StartsWith("f"));
            if (flowerCount > 0)
            {
                GrabTile(GameManager.TileWalls.GetRange(GameManager.TileWalls.Count - flowerCount, flowerCount));
                GameManager.StackFlower();
            }
            else if (tiles.Count == 4 && tiles.All(tile => tile.name == name))
            {
                GrabTile(GameManager.TileWalls.Last());
                GameManager.StackFlower();
            }
            else if (Open.Count(tile => tile.name == name) == 4)
            {
                GrabTile(GameManager.TileWalls.Last());
                GameManager.StackFlower();
            }
        }));
    }

    public virtual void ScrtTile(GameObject tile) => ScrtTile(new List<GameObject> { tile });
    public virtual void ScrtTile(List<GameObject> tiles)
    {
        Open.AddRange(tiles);
        Hand.RemoveAll(tiles.Contains);

        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<BoxCollider>().isTrigger = true;
        }

        DistributeScrt();
        DistributeHand();
        StartCoroutine(DelayAction(() => 
        {
            GrabTile(GameManager.TileWalls.Last());
            GameManager.StackFlower();
        }));
    }

    public virtual void TossTile(GameObject tile)
    {
        Hand.Remove(tile);
        GameManager.TileToss.Add(tile);
        foreach (Player p in GameManager.Players)
        {
            if (p == this) // p == this
                continue;
            
            p.engine.UpdateKB(tile.name);
        }

        (Vector3 position, Quaternion rotation) = TossTiler.ComputeTossPosition();
        tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.1f);
        
        DistributeHand();
        tile.GetComponent<BoxCollider>().isTrigger = false;
        tile.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;

        StartCoroutine(GameManager.PostTossBuffer(tile));
    }   
}

public class Human : Player
{
    public override Quaternion quadrant 
    {
        get { return Quaternion.Euler(0, -90 * playerID, 0); }
    } 
    public override float groundNegative 
    {
        get { return CameraPan.groundNegative; }
    }
    public override Vector3 viewpoint {
        get { return Camera.main.transform.position - new Vector3(0, 2f - groundNegative, - Perimeter.HandArea); }
    }

    public override void GrabTile(List<GameObject> tiles)
    {
        base.GrabTile(tiles);

        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<DragTile>().enabled = true;
        }
        Camera.main.GetComponent<KeyboardListener>().enabled = true;
    }

    public override void OpenTile(List<GameObject> tiles)
    {
        base.OpenTile(tiles);

        // move to inner func
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<DragTile>().enabled = false;
        }
    }
    
    public override void TossTile(GameObject tile)
    {
        base.TossTile(tile);

        tile.GetComponent<DragTile>().enabled = false;
        Camera.main.GetComponent<KeyboardListener>().enabled = false;
    }

    public override bool WillWin(GameObject tile)
    {
        if (KeyboardListener.winRequested == false)
            return false;

        KeyboardListener.winRequested = false;

        return this.engine.WillWin(tile.name);
    }

    public override bool WillChao(GameObject tile)
    {
        if (KeyboardListener.chaoRequested == false)
            return false;
        
        KeyboardListener.chaoRequested = false;

        char suit = tile.name[0];
        int unit = tile.name[1] - '0';

        string[] seqn = new string[] {
            suit + (unit - 2).ToString(),
            suit + (unit - 1).ToString(),
            suit + (unit + 1).ToString(),
            suit + (unit + 2).ToString(),
        };

        if (Hand.Any(_tile => _tile.name == seqn[0]) && Hand.Any(_tile => _tile.name == seqn[1]))
            return true;
        
        if (Hand.Any(_tile => _tile.name == seqn[1]) && Hand.Any(_tile => _tile.name == seqn[2]))
            return true;
        
        if (Hand.Any(_tile => _tile.name == seqn[2]) && Hand.Any(_tile => _tile.name == seqn[3]))
            return true;

        return false;
    }

    public override bool WillPong(GameObject tile)
    {
        if (KeyboardListener.pongRequested == false)
            return false;

        KeyboardListener.pongRequested = false;

        return Hand.Count(_tile => _tile.name == tile.name) >= 2;
    }

    public override bool WillKang(GameObject tile)
    {
        if (KeyboardListener.kangRequested == false)
            return false;
        
        KeyboardListener.kangRequested = false;

        return Hand.Count(_tile => _tile.name == tile.name) == 3;
    }
}

public class Bot : Player
{
    public override Quaternion quadrant 
    { 
        get { return Quaternion.Euler(0, -90 * playerID, 0); } 
    }
    public override float groundNegative
    {
        get { return 0; }
    }
    public override Vector3 viewpoint 
    { 
        get { return quadrant * new Vector3(0, 0, -1); }
    }
    public override void GrabTile(List<GameObject> tiles)
    {
        base.GrabTile(tiles);

        /*if (tiles.Count > 1)
        {
            return;
        }*/
        StartCoroutine(DelayAction(() =>  // replicate in Human
        {
            int flowerCount = tiles.Count(tile => tile.name.StartsWith("f"));
            if (flowerCount > 0 && GameManager.currentPlayer == this)
            {
                OpenTile(tiles.Where(tile => tile.name.StartsWith("f")).ToList());
                // GameManager.StackFlower();
            }

            // if kang

            /*if (tiles[0].name.StartsWith("f"))
            {
                OpenTile(tiles[0]);
            }*/
            else if (tiles.Count == 1 && GameManager.currentPlayer == this)
            {
                if (Hand.Count(tile => tile.name == tiles[0].name) == 4)
                {
                    ScrtTile(Hand.Where(tile => tile.name == tiles[0].name).ToList());
                }
                else
                {
                    ChooseDiscard();
                }
            }
        }));
    }


    public override bool WillWin(GameObject tile)
    {
        return this.engine.WillWin(tile.name);
    }

    public override bool WillChao(GameObject tile)
    {
        return this.engine.WillChao(tile.name);
    }
    public override bool WillPong(GameObject tile)
    {
        return this.engine.WillPong(tile.name);
    }
    public override bool WillKang(GameObject tile)
    {
        return this.engine.WillKang(tile.name);
    }
}


// include open flower in sort
// auto sort function

// rename human, bot to controller, opponent

// press on tileWall before grabbing for Human Player


// change kangtile to chowtile
// find out where dragtile is enabled
// convert GrabFlower to IEnumerator

// for callback functions, to overcome bug where OnDisable happens before EventHandler gets asigned in the first place
// do: add cond in if statement: if (tile.enabled == true) {assign Handler}, else {execute Handler}

// FIINAL: MAYBE USE ASYNC, if not then coroutines, and use WaitForSeconds

// using async for I/O calls
        // TRY: remove auto get flower in bot's grab tile


// transfer anon coroutine to DistributeHand _end

// change update to async in DragTile

// separate tosstile for opening a chow


// bug if playerA throws playerB's pong
// if playerA picks up that same tile later, negative KB
// because OpenTiles redundantly updates playerA after TossTiles