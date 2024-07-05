using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand
{
    private List<GameObject> hand;
    public List<GameObject> GetHand()
    {
        return new List<GameObject>(hand);
    }
    public Hand()
    {
        hand = new List<GameObject>();
    }

    public void AddTile(GameObject tile)
    {
        hand.Add(tile);
    }

    public void AddTile(List<GameObject> tiles)
    {
        hand.AddRange(tiles);
    }

    public void RemoveTile(GameObject tile)
    {
        hand.Remove(tile);
    }

    public void RemoveTile()
    {

    }
}