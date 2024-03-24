using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;
// using GameManager.FacePreset

// Playerprefs Built-in class

public abstract class Player
{
    public List<GameObject> Hand = new List<GameObject>();
    public List<GameObject> Open = new List<GameObject>();

    public string playerName;
    public int playerID, coins = 0, wins = 0;

    protected Quaternion quadrant;
    protected Vector3 viewpoint;
    protected GameManager.FacePreset state;

    public Player(int playerID, string playerName)
    {
        this.playerID = playerID;
        this.playerName = playerName;
    }

    void DistributeHand()
    {
        IEnumerator<Vector3> row = GameManager.DistributeRow(Hand.Count, 3.22f, this.state);
        foreach (GameObject tile in Hand)
        {
            row.MoveNext();
            Vector3 position = this.quadrant * row.Current;
            Quaternion rotation = tile.GetComponent<DragTile>().baseRotation;

            // tile.GetComponent<TileManager>().SetDestination(position, 0.1f);
            tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.1f);
        }
    }

    public virtual void GrabTile(GameObject tile)
    {
        GrabTile(new List<GameObject> { tile });
    }

    public virtual void GrabTile(List<GameObject> tiles)
    {
        Hand.AddRange(tiles);
        foreach(GameObject tile in tiles)
        {
            Quaternion xRot = Quaternion.LookRotation(this.viewpoint);
            Quaternion zRot = Quaternion.Euler(0, 0, new Random().Next(2) * 180);

            tile.GetComponent<DragTile>().baseRotation = xRot * zRot;
        }

        DistributeHand();
    }
    public virtual void OpenTile(GameObject tile)
    {
        OpenTile(new List<GameObject> { tile });
    }

    public virtual void OpenTile(List<GameObject> tiles)
    {
        Open.AddRange(tiles);
        Hand.RemoveAll(tiles.Contains);

        IEnumerator<Vector3> row = GameManager.DistributeRow(Open.Count, 2.75f, GameManager.FacePreset.Opened);
        foreach (GameObject tile in Open)
        {
            row.MoveNext();
            Vector3 position = this.quadrant * row.Current;
            Quaternion rotation = this.quadrant * Quaternion.Euler((int) GameManager.FacePreset.Opened, 0, 180);
            
            tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.05f);
        }

        DistributeHand();
    }

    public virtual void KangTile()
    {

    }

    public virtual void TossTile(GameObject tile)
    {
        Hand.Remove(tile);
        (Vector3 position, Quaternion rotation) = TossTiler.RandomTossPosition();
        GameManager.TileToss.Add(tile);
        
        // determine position and rotation

        //tile.GetComponent<TileManager>().SetDestination(new Vector3(0, 0, 0), Quaternion.Euler((int) GameManager.FacePreset.Opened, 0, 0), 0.05f);
        tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.05f);
        DistributeHand();

        //tile.GetComponent<Rigidbody>().isKinematic = true;
        tile.GetComponent<BoxCollider>().isTrigger = false;
        tile.AddComponent<DragToss>();
        GameManager.currentTurnRunning = false;
    }
}

public class Human : Player // implement as singleton so that e.g. Human.Hand()
{
    public Human(int playerID, string playerName) : base(playerID, playerName)
    {
        this.quadrant = Quaternion.Euler(0, -90 * playerID, 0);
        this.viewpoint = this.quadrant * Camera.main.transform.position;
        this.state = GameManager.FacePreset.Player;
    }

    public override void GrabTile(List<GameObject> tiles)
    {
        base.GrabTile(tiles);
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<DragTile>().enabled = true;
        }
    }
    
    public override void OpenTile(List<GameObject> tiles)
    {
        base.OpenTile(tiles);
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<DragTile>().enabled = false;
        }
    }
    
    /*
    public override void KangTile(List<GameObject> tiles)
    {
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<DragTile>().enabled = false;
        }
        base.OpenTile(tiles);
    }
    */
    public override void TossTile(GameObject tile)
    {
        tile.GetComponent<DragTile>().enabled = false;
        base.TossTile(tile);
    }
}

public class Bot : Player
{
    public Bot(int playerID, string playerName) : base(playerID, playerName)
    {
        this.quadrant = Quaternion.Euler(0, -90 * playerID, 0);
        // this.viewpoint = this.quadrant * new Vector3(0, 0, float.NegativeInfinity);
        this.viewpoint = this.quadrant * new Vector3(0, 0, -10);
        this.state = GameManager.FacePreset.Stand;
    }
}


// change overloading so that single grab is default rather than multi grab


// get rid of FacePreset
// get rid of orientation in TileManager since BaseRotation already exists


// convert dW dN dS dE -> to -> tW tN tS tE