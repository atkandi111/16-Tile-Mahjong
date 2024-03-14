using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;
// using GameManager.FacePreset

// Playerprefs Built-in class

public abstract class Player
{
    public List<Vector3> BasePositions = new List<Vector3>();
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

    public virtual void GrabTile(GameObject tile)
    {
        GrabTile(new List<GameObject> { tile });
    }

    public virtual void GrabTile(List<GameObject> tiles)
    {
        Hand.AddRange(tiles);
        foreach(GameObject tile in tiles)
        {
            tile.GetComponent<TileManager>().SetOrientation(new Random().Next(2) * 180);
        }

        BasePositions = new List<Vector3>();
        IEnumerator<Vector3> row = GameManager.DistributeRow(Hand.Count, 3.22f, this.state);
        foreach (GameObject tile in Hand)
        {
            row.MoveNext();
            BasePositions.Add(row.Current);
            Vector3 position = this.quadrant * row.Current;
            Quaternion rotation = Quaternion.LookRotation(this.viewpoint);

            /*Debug.Log(this.quadrant.eulerAngles);
            Debug.Log(rotation.eulerAngles);
            Debug.Log("//");

            localrotation is -180 so that initial prefab is facing south
            */

            rotation *= Quaternion.Euler(0, 0, tile.GetComponent<TileManager>().GetOrientation());

            tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.1f);
        }
    }
    public virtual void OpenTile(GameObject tile)
    {
        OpenTile(new List<GameObject> { tile });
    }

    public virtual void OpenTile(List<GameObject> tiles)
    {
        Open.AddRange(tiles);
        Hand.RemoveAll(tiles.Contains);

        IEnumerator<Vector3> row;

        row = GameManager.DistributeRow(Open.Count, 2.75f, GameManager.FacePreset.Opened);
        foreach (GameObject tile in Open)
        {
            row.MoveNext();
            Vector3 position = this.quadrant * row.Current;
            Quaternion rotation = this.quadrant * Quaternion.Euler((int) GameManager.FacePreset.Opened, 0, 180);
            
            tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.05f);
            tile.GetComponent<DragTile>().enabled = false;
        }

        BasePositions = new List<Vector3>();
        row = GameManager.DistributeRow(Hand.Count, 3.22f, this.state);
        foreach (GameObject tile in Hand)
        {
            row.MoveNext();
            BasePositions.Add(row.Current);
            Vector3 position = this.quadrant * row.Current;
            Quaternion rotation = tile.transform.rotation;
            
            tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.05f);
            tile.GetComponent<DragTile>().UpdateBasePosition(position);
        }
    }

    public virtual void KangTile()
    {

    }

    public virtual void TossTile(GameObject tile)
    {
        tile.GetComponent<DragTile>().enabled = false;
    }
}

public class Human : Player
{
    public Human(int playerID, string playerName) : base(playerID, playerName)
    {
        this.quadrant = Quaternion.Euler(0, -90 * playerID, 0);
        this.viewpoint = this.quadrant * Camera.main.transform.position;
        this.state = GameManager.FacePreset.Player;
    }

    public override void GrabTile(List<GameObject> tiles)
    {
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<DragTile>().enabled = true;
        }
        base.GrabTile(tiles);
    }
    
    public override void OpenTile(List<GameObject> tiles)
    {
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<DragTile>().enabled = false;
        }
        base.OpenTile(tiles);
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
    
    public override void TossTile(List<GameObject> tiles)
    {
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<DragTile>().enabled = false;
        }
        base.OpenTile(tiles);
    }
    */
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

// BasePositions array not necessary, just sub with tile at spec index