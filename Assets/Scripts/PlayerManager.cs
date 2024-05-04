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

    public string playerName;
    public int playerID, coins = 0, wins = 0;


    /*private string m_Value;

    protected A(string value) {
        m_Value = value;
    }

    public Vector3 viewpoint {
        get { return Camera.main.transform.position - new Vector3(0, 2f, 0); }
    }*/

    //protected Quaternion quadrant;
    //protected Vector3 viewpoint;

    // try private
    public abstract Quaternion quadrant { get; }
    public abstract Vector3 viewpoint { get; }
    public abstract float groundNegative { get; }

    // public abstract IEnumerator SyncTurn(GameObject tile);

    /*public Player(int playerID, string playerName)
    {
        this.playerID = playerID;
        this.playerName = playerName;
    }*/

    public virtual void DistributeHand()
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
    public IEnumerator GrabFlower(List<GameObject> tiles) // GameObject tile (single)
    {
        foreach (GameObject tile in tiles)
        {
            while (tile.GetComponent<TileManager>().enabled)
            {
                yield return null;
            }
        }

        OpenTile(tiles);
        GrabTile(GameManager.TileWalls.GetRange(GameManager.TileWalls.Count - tiles.Count, tiles.Count));

        GameManager.StackFlower();

    }
    public virtual void GrabTile(GameObject tile)
    {
        GrabTile(new List<GameObject> { tile });
    }

    public virtual void GrabTile(List<GameObject> tiles)
    {
        Hand.AddRange(tiles);
        GameManager.TileWalls.RemoveAll(tiles.Contains);
        foreach (GameObject tile in tiles)
        {
            Quaternion xRot = Quaternion.LookRotation(viewpoint);
            Quaternion zRot = Quaternion.Euler(0, 0, new Random().Next(2) * 180);

            // transfer to Human
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

        IEnumerator<Vector3> row = Positioner.DistributeRow(Open.Count, Perimeter.OpenArea, upright: false);
        foreach (GameObject tile in Open)
        {
            row.MoveNext();
            Vector3 position = this.quadrant * row.Current;
            Quaternion rotation = this.quadrant * Quaternion.Euler(Positioner.FaceUp, 0, 180);
            
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
        (Vector3 position, Quaternion rotation) = TossTiler.ComputeTossPosition();
        GameManager.TileToss.Add(tile);

        tile.GetComponent<TileManager>().SetDestination(position, rotation, 0.1f);
        DistributeHand();

        tile.GetComponent<BoxCollider>().isTrigger = false;
        tile.AddComponent<DragToss>();
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

    public IEnumerator SyncTurn(GameObject tile) // transfer to grabtile
    {
        // use unityevents to subscribe NextTurn to TileManager.completion instead
        while (tile.GetComponent<TileManager>().enabled)
        {
            yield return null;
        }

        GameManager.NextTurn();
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

    public override void OpenTile(GameObject tile)
    {
        if (tile.name.StartsWith("f"))
        {
            StartCoroutine(GrabFlower(new List<GameObject>() { tile }));
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

        StartCoroutine(SyncTurn(tile));
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

    public IEnumerator SyncTurn(List<GameObject> tiles)
    {
        // Choose Discard //
        foreach (GameObject tile in tiles)
        {
            while (tile.GetComponent<TileManager>().enabled)
            {
                yield return null;
            }
        }

        string discardName = Engine.ChooseDiscard(
            Hand.Select(tile => tile.name).ToList()
        );

        GameObject discard = Hand.First(tile => tile.name == discardName);
        TossTile(discard);

        // Delay Throw //
        while (discard.GetComponent<TileManager>().enabled)
        {
            yield return null;
        }

        float elapsedTime = 0f, decisionTime = 3f;
        while (elapsedTime < decisionTime && !Input.GetKeyDown(KeyCode.RightArrow))
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        GameManager.NextTurn();
    }
    public override void GrabTile(List<GameObject> tiles)
    {
        base.GrabTile(tiles);
        
        List<GameObject> flowers = tiles
            .Where(tile => tile.name.StartsWith("f"))
            .ToList();

        if (flowers.Count > 0)
        {
            StartCoroutine(GrabFlower(flowers));
        }
        else
        {
            if (GameManager.currentPlayer == this)
            {
                StartCoroutine(SyncTurn(tiles));
            }
        }
    }


}


// include open flower in sort


// rename human, bot to controller, opponent


// syncturn WaitFor should be in Grab, Open, and Toss methods instead
// prevent openFlower if not current turn
// press on tileWall before grabbing for Human Player