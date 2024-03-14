using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class SortTile : MonoBehaviour
{
    public static bool busySorting = false;

    void Logger(List<string> list)
    {
        Debug.Log(string.Join(" ", list));
    }
    void Logger(List<int> list)
    {
        Debug.Log(string.Join(" ", list));
    }

    void Logger(List<GameObject> list)
    {
        Debug.Log(string.Join(" ", list.Select(tile => tile.name).ToList()));
    }

    IEnumerator SortTiles()
    {
        /*
        mainSubsequence 1 2 3 4 5 6 7
        but is just a subset of largerSequence 0 1 0 0 2 3 4 0 5 6 0 0 7

        case a: 1 6 2 3 4 5 7 ... mainSub : 1 2 3 4 5 7, toInsert : 6
        case b: 1 3 4 5 6 2 7 ... mainSub : 1 3 4 5 6 7, toInsert : 2
        case c: 2 3 4 1 5 6 7 ... mainSub : 2 3 4 5 6 7, toInsert : 1
        case d: 1 2 3 7 4 5 6 ... mainSub : 2 3 4 5 6 7, toInsert : 7

        Let Q be first number in mainSub larger than toInsert
        If Q exists, insert toInsert before Q
        Else, insert toInsert after mainSub
        */

        List<GameObject> Hand = new List<GameObject>(GameManager.Players[0].Hand);

        /* Sign tiles with (list_index / list_length) to preserve arrangement */
        List<string> tileNames = Hand
            .Select((tile, index) => $"{tile.name}.{100 * index / Hand.Count}")
            .ToList();

        /* Generate sorted list of hand's tile names */
        List<string> sortNames = tileNames
            .OrderBy(name => name, new TileComparer())
            .ToList();

        /* Rank each tile (acc to index after sorting) */
        List<int> tileRanks = tileNames
            .Select(name => sortNames.IndexOf(name))
            .ToList();

        // Proof of Optimality (Lemma 1): 
        // https://doi.org/10.1090/S0273-0979-99-00796-X

        /* Decompose targetIndices into increasing subsequences */
        List<int> mainSubsequence = new List<int>();
        List<List<int>> increasingSubsequences = new List<List<int>>();
        foreach (int rank in tileRanks)
        {
            bool numWasAdded = false;
            List<int> seqn = new List<int>();
            for (int i = 0; i < increasingSubsequences.Count - 1; i++)
            {
                seqn = increasingSubsequences[i];
                if (rank >= seqn[seqn.Count - 1])
                {
                    seqn.Add(rank);
                    numWasAdded = true;
                    break;
                }
            }

            if (numWasAdded == false)
            {
                seqn = new List<int>() {rank};
                increasingSubsequences.Add(seqn);
            }

            if (mainSubsequence.Count < seqn.Count)
            {
                mainSubsequence = seqn;
            }
        }

        /*List<List<int>> increasingSubsequences = tileRanks.Aggregate(new List<List<int>>(), (seqn, num) =>
        {
            if (seqn.Count == 0 || num <= seqn.Last().Last()) 
            {
                seqn.Add(new List<int>());
            }
            seqn.Last().Add(num);

            if (mainSubsequence.Count < seqn.Last().Count)
            {
                mainSubsequence = seqn.Last();
            }

            return seqn;
        });*/

        increasingSubsequences.Remove(mainSubsequence);
        foreach (List<int> seqnSample in increasingSubsequences)
        {
            //while (seqn.Any())
            if (true)
            {
                // List<int> seqnSample = seqn.GetRange(0, Math.Min(seqn.Count, 3));
                // seqn.RemoveRange(0, Math.Min(seqn.Count, 3));
                Logger(Hand);

                List<GameObject> tiles = new List<GameObject>() {};
                List<int> gotos = new List<int>() {};
                List<Vector3> tileTarget = new List<Vector3>() {};

                foreach (int rank in seqnSample)
                {
                    /*
                    In mainSubsequence, get the smallest larger number than the targetIndex
                    Insert targetIndex before that number

                    e.g.
                    list: [3, 5, 9] , num: 4
                    list: [3, 4, 5, 9]
                    */

                    int startIndex = tileRanks.IndexOf(rank);
                    GameObject currentTile = Hand[startIndex];

                    Hand.Remove(currentTile);
                    tileRanks.Remove(rank);

                    int finalIndex = 0;

                    for (int i = 0; i <= mainSubsequence.Count; i++)
                    {
                        if (i == mainSubsequence.Count)
                        {
                            int preceedingRank = mainSubsequence[i - 1];
                            mainSubsequence.Insert(i, rank);

                            finalIndex = tileRanks.IndexOf(preceedingRank) + 1;
                            break;
                            /*
                            mainSubsequence.Insert(i, rank);
                            finalIndex = tileRanks.Count;
                            */
                        }
                        if (mainSubsequence[i] > rank) // >=
                        {
                            int preceedingRank = mainSubsequence[i];
                            mainSubsequence.Insert(i, rank);

                            finalIndex = tileRanks.IndexOf(preceedingRank);
                            break;
                        } 
                    }

                    GameObject targetTile = null;
                    Debug.Log(finalIndex);
                    if (finalIndex > startIndex)
                    {
                        targetTile = Hand[finalIndex - 1];
                        Debug.Log(currentTile.name + " go to " + targetTile.name);
                    }
                    if (finalIndex < startIndex)
                    {
                        targetTile = Hand[finalIndex];
                        Debug.Log(currentTile.name + " go to " + targetTile.name);
                    }
                    Hand.Insert(finalIndex, currentTile);
                    tileRanks.Insert(finalIndex, rank);

                    if (finalIndex > startIndex)
                    {
                        tiles.Insert(0, currentTile);
                        gotos.Insert(0, finalIndex);
                    }
                    
                    if (finalIndex < startIndex)
                    {
                        tiles.Add(currentTile);
                        gotos.Add(finalIndex);
                    }
                }

                foreach (GameObject tile in tiles)
                {
                    int index = Hand.IndexOf(tile);
                    tileTarget.Add(GameManager.Players[0].BasePositions[index]);
                }                

                Vector3 position;
                float elapsedTime = 0f;
                while (elapsedTime < 0.25f)
                {
                    foreach (GameObject tile in tiles)
                    {
                        Vector3 basePosition = tile.GetComponent<DragTile>().basePosition;
                        Vector3 hoverOffset = tile.GetComponent<DragTile>().hoverOffset;

                        position = tile.transform.position;
                        position.y = Mathf.Lerp(basePosition.y, basePosition.y + hoverOffset.y, elapsedTime / 0.25f);
                        position.z = Mathf.Lerp(basePosition.z, basePosition.z + hoverOffset.z, elapsedTime / 0.25f);
                        tile.transform.position = position;
                    }

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                
                elapsedTime = 0f;
                while (elapsedTime < 0.5f)
                {
                    foreach (GameObject tile in tiles)
                    {
                        Vector3 startPos = tile.GetComponent<DragTile>().basePosition;
                        Vector3 finalPos = tileTarget[tiles.IndexOf(tile)];

                        position = tile.transform.position;
                        position.x = Mathf.Lerp(startPos.x, finalPos.x, elapsedTime / 0.5f);
                        tile.transform.position = position;
                        tile.GetComponent<DragTile>().DragLogic();
                    }

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                
                foreach (GameObject tile in tiles)
                {
                    Vector3 finalPos = tileTarget[tiles.IndexOf(tile)];
                    position = tile.transform.position;
                    position.x = finalPos.x;
                    tile.transform.position = position;
                    tile.GetComponent<DragTile>().DragLogic();
                }

                for (int i = 0; i < GameManager.Players[0].Hand.Count; i++)
                {
                    if (Hand[i] != GameManager.Players[0].Hand[i])
                    {
                        Debug.Log("-----/-----");
                        Logger(Hand);
                        Logger(GameManager.Players[0].Hand);
                        Debug.Log("-----/-----");
                        yield return new WaitForSeconds(100f);
                        break;
                    }
                }

                elapsedTime = 0f;
                while (elapsedTime < 0.25f)
                {
                    foreach (GameObject tile in tiles)
                    {
                        Vector3 basePosition = tile.GetComponent<DragTile>().basePosition;
                        Vector3 hoverOffset = tile.GetComponent<DragTile>().hoverOffset;

                        position = tile.transform.position;
                        position.y = Mathf.Lerp(basePosition.y + hoverOffset.y, basePosition.y, elapsedTime / 0.25f);
                        position.z = Mathf.Lerp(basePosition.z + hoverOffset.z, basePosition.z, elapsedTime / 0.25f);
                        tile.transform.position = position;
                    }

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }

        busySorting = false;
    }

    public class TileComparer : IComparer<string>
    {    
        string suitOrder = null;
        private string SuitOrder()
        {
            /* Create suit hierarchy */
            Dictionary<char, int> suitHierarchy = new Dictionary<char, int>
            {
                {'b', 0}, {'c', 0}, {'s', 0},   // Leftmost
                {'t', 1}, {'d', 1},             // Middle
                {'f', 2}                        // Rightmost
            };

            /* Calculate index density (the average index of all tiles of a suit) */
            var indexDensity = GameManager.Players[0].Hand
                .Select((tile, index) => (suit: tile.name[0], index))
                .GroupBy(pair => pair.suit)
                .ToDictionary(
                    grp => grp.Key, 
                    grp => grp.Average(pair => pair.index)
                );

            /* Sort suit based on (Hierarchy) and (Index Density) */
            var sortedLetters = indexDensity
                .OrderBy(kvp => suitHierarchy[kvp.Key])
                .ThenBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key);

            return string.Concat(sortedLetters);
        }
        public int Compare(string x, string y)
        {
            /* Extract suit and unit from tile name */
            if (suitOrder == null)
            {
                suitOrder = SuitOrder();
                // also create unitOrder
                // unitOrder automatically has 1-9, but also contains info for tR tG tW dN etc
            }

            int suitX = suitOrder.IndexOf(x[0]);
            int suitY = suitOrder.IndexOf(y[0]);
            string unitX = x.Substring(1);
            string unitY = y.Substring(1);

            /* Compare suits first */
            int suitComparison = suitX.CompareTo(suitY);
            if (suitComparison != 0)
            {
                return suitComparison;
            }

            /* If suits are the same, compare units (and signature) */
            return unitX.CompareTo(unitY);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && busySorting != true)
        {
            StartCoroutine(SortTiles());
            busySorting = true;
        }
    }
}


// suitOrder should be resetted to null after sorting
// busySorting should also be resetted to false