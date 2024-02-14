using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using GameManager.FacePreset

//[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class Player : ScriptableObject
{
    public List<GameObject> Hand = new List<GameObject>();
    public List<GameObject> Open = new List<GameObject>();

    //[Header("Player Information")]
    public string playerName;
    public GameManager.FacePreset state;
    private Quaternion quadrant;
    private int playerID, coins = 0, wins = 0;

    public void Initialize(int PlayerID, string PlayerName)
    {
        this.playerID = PlayerID;
        this.playerName = PlayerName;
        this.quadrant = Quaternion.Euler(0, -90 * PlayerID, 0);
        
        if (playerID == 0)
        {
            this.state = GameManager.FacePreset.Player;
        }
        else
        {
            this.state = GameManager.FacePreset.Stand;
        }

        // take 16 tiles as starting hand
        // PositionWalls (1) so that they arrive to hand face down
        // PositionWalls (2) so that face down hand is standed
    }
    private Vector3 position;
    private Quaternion rotation;

    public void GrabTile(GameObject[] tiles)
    {
        Hand.AddRange(tiles);

        IEnumerator<Vector3> row = GameManager.DistributeRow(Hand.Count, 3.22f, this.state);
        foreach (GameObject tile in Hand)
        {
            row.MoveNext();
            position = this.quadrant * row.Current;
            rotation = this.quadrant * Quaternion.Euler((int) this.state, 0, 0);
            
            tile.GetComponent<TileManager>().SetDestination(position, rotation, 1.5f);
        }

        //PositionManager.AssignPosition(Hand, PlayerID: this.playerID, tileState: this.state, numTiles: Hand.Count, perimSize: 3.22f);

        /*foreach(GameObject tile in Hand)
        {
            tile.GetComponent<DragTile>().enabled = true;
        }*/
    }

    public void GrabTile(GameObject tile)
    {
        Hand.Add(tile);

        IEnumerator<Vector3> row = GameManager.DistributeRow(Hand.Count, 3.22f, this.state);
        foreach (GameObject _tile in Hand)
        {
            row.MoveNext();
            position = this.quadrant * row.Current;
            rotation = this.quadrant * Quaternion.Euler((int) this.state, 0, 0);
            
            _tile.GetComponent<TileManager>().SetDestination(position, rotation, 2f);
        }
    }

    public void OpenTile(GameObject tile)
    {
        Open.Add(tile);
        Hand.Remove(tile);

        IEnumerator<Vector3> row = GameManager.DistributeRow(Open.Count, 2.75f, this.state);
        foreach (GameObject _tile in Open)
        {
            row.MoveNext();
            position = this.quadrant * row.Current;
            rotation = this.quadrant * Quaternion.Euler((int) GameManager.FacePreset.Opened, 0, 0);
            
            _tile.GetComponent<TileManager>().SetDestination(position, rotation);
        }

        row = GameManager.DistributeRow(Hand.Count, 3.22f, this.state);
        foreach (GameObject _tile in Open)
        {
            row.MoveNext();
            position = this.quadrant * row.Current;
            rotation = this.quadrant * Quaternion.Euler((int) this.state, 0, 0);
            
            _tile.GetComponent<TileManager>().SetDestination(position, rotation);
        }

        //PositionManager.AssignPosition(Open, PlayerID: this.playerID, tileState: "Opened", numTiles: Open.Count, perimSize: 2.75f);
        //PositionManager.AssignPosition(Hand, PlayerID: this.playerID, tileState: this.state, numTiles: Hand.Count, perimSize: 3.22f);
        
        foreach (GameObject _tile in Open)
        {
            _tile.GetComponent<DragTile>().enabled = false;
            // _tile.GetComponent<TileManager>().enabled = true;
        }

        foreach (GameObject _tile in Hand)
        {
            Vector3 currentDestination = _tile.GetComponent<TileManager>().GetDestination().Item1;
            _tile.GetComponent<DragTile>().UpdateBasePosition(currentDestination);
            // _tile.GetComponent<TileManager>().enabled = true;
        }

        // PositionManager.ScheduleEvent(duration: 0.02f, cluster: Open.Count + Hand.Count, tileArray: Hand.Concat(Open).ToList());
    }
    public void KangTile()
    {

    }

    public void TossTile(GameObject tile)
    {
        tile.GetComponent<DragTile>().enabled = false;
    }
}
