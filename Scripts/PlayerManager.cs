using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class Player : ScriptableObject
{
    public List<GameObject> Hand = new List<GameObject>();
    public List<GameObject> Open = new List<GameObject>();

    //[Header("Player Information")]
    public string playerName, state;
    private int playerID, coins = 0, wins = 0;

    public void Initialize(int PlayerID, string PlayerName)
    {
        this.playerID = PlayerID;
        this.playerName = PlayerName;
        
        if (playerID == 0)
        {
            this.state = "UserPOV";
        }
        else
        {
            this.state = "Stand";
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
        PositionManager.AssignPosition(Hand, PlayerID: this.playerID, tileState: this.state, numTiles: Hand.Count, perimSize: 3.22f);

        /*foreach(GameObject tile in Hand)
        {
            tile.GetComponent<DragTile>().enabled = true;
        }*/
    }

    public void GrabTile(GameObject tile)
    {
        Hand.Add(tile);
        PositionManager.AssignPosition(Hand, PlayerID: this.playerID, tileState: this.state, numTiles: Hand.Count, perimSize: 3.22f);
    }

    public void OpenTile(GameObject tile)
    {
        Open.Add(tile);
        Hand.Remove(tile);

        PositionManager.AssignPosition(Open, PlayerID: this.playerID, tileState: "Opened", numTiles: Open.Count, perimSize: 2.75f);
        PositionManager.AssignPosition(Hand, PlayerID: this.playerID, tileState: this.state, numTiles: Hand.Count, perimSize: 3.22f);
        
        foreach (GameObject _tile in Open)
        {
            _tile.GetComponent<DragTile>().enabled = false;
            _tile.GetComponent<TileManager>().enabled = true;
        }

        foreach (GameObject _tile in Hand)
        {
            Vector3 currentDestination = _tile.GetComponent<TileManager>().currentDestination.destination.Item1;
            _tile.GetComponent<DragTile>().UpdateBasePosition(currentDestination);
            _tile.GetComponent<TileManager>().enabled = true;
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
