using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class SortTile : MonoBehaviour
{
    public static bool busySorting = false;
    private static string suitOrder, unitOrder;

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
            .Select((tile, index) => $"{tile.name}.{(100 * index / Hand.Count).ToString("00")}")
            .ToList();

        /* Generate sorted list of hand's tile names */
        List<string> sortNames = tileNames
            .OrderBy(name => name, new TileComparer())
            .ToList();

        /* Rank each tile (acc to index after sorting) */
        List<int> tileRanks = tileNames
            .Select(name => sortNames.IndexOf(name))
            .ToList();

        /* Decompose targetIndices into increasing subsequences */
        List<int> mainSubsequence = new List<int>();
        List<List<int>> increasingSubsequences = new List<List<int>>();

        foreach (int num in tileRanks)
        {
            for (int i = 0; i <= increasingSubsequences.Count; i++)
            {
                if (i == increasingSubsequences.Count)
                {
                    increasingSubsequences.Add(new List<int>() { num });
                    break;
                }

                if (num > increasingSubsequences[i].Last())
                {
                    increasingSubsequences[i].Add(num);
                    break;
                }
            }
        };

        foreach (List<int> seqn in increasingSubsequences)
        {
            if (mainSubsequence.Count <= seqn.Count)
            {
                mainSubsequence = seqn;
            }
        }

        increasingSubsequences.Remove(mainSubsequence);
        foreach (List<int> seqn in increasingSubsequences)
        {
            List<GameObject> tiles = new List<GameObject>() {};
            List<Vector3> tileTarget = new List<Vector3>() {};

            /*
            bool leftToRight = true;
            foreach (int rank in seqn)
            {
                int startIndex = tileRanks.BinarySearch(rank);

                tileRanks.Remove(rank);
                int mainIndex = mainSubsequence.BinarySearch(rank);
                int finalIndex = tileRanks.BinarySearch(mainSubsequence[mainIndex]);

                mainSubsequence.Insert(mainIndex, rank);
                tileRanks.Insert(finalIndex, rank);

                leftToRight = (startIndex < finalIndex);
            }

            foreach (int rank in seqn)
            {
                int index = tileRanks.IndexOf(rank);
                
                if (leftToRight)
                {
                    tiles.Insert(0, currentTile);
                    tileTarget.Insert(0, GameManager.Players[0].Hand[index].GetComponent<DragTile>().basePosition);
                }
                else
                {
                    tiles.Add(currentTile);
                    tileTarget.Add(GameManager.Players[0].Hand[index].GetComponent<DragTile>().basePosition);
                }
            }
            */

            foreach (int rank in seqn)
            {
                /*
                In mainSubsequence, get the smallest rank larger than current rank
                Insert rank before that number in Hand

                e.g.
                mainSub: [3, 5, 9] , num: 4
                Hand: [15, 3, 16, 4, 5, 17, 9]
                */

                int startIndex = tileRanks.IndexOf(rank);
                int finalIndex = 0;

                GameObject currentTile = Hand[startIndex];
                Hand.Remove(currentTile);
                tileRanks.Remove(rank);

                for (int i = 0; i <= mainSubsequence.Count; i++)
                {
                    if (i == mainSubsequence.Count)
                    {
                        finalIndex = tileRanks.Count;
                        mainSubsequence.Insert(i, rank);
                        break;
                    }
                    if (mainSubsequence[i] > rank)
                    {
                        finalIndex = tileRanks.IndexOf(mainSubsequence[i]);
                        mainSubsequence.Insert(i, rank);
                        break;
                    } 
                }

                /*GameObject targetTile = null;
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
                }*/

                Hand.Insert(finalIndex, currentTile);
                tileRanks.Insert(finalIndex, rank);

                /* NOTE: order in tiles matter because of swapTile */
                if (finalIndex > startIndex)
                {
                    tiles.Insert(0, currentTile);
                }
                
                if (finalIndex < startIndex)
                {
                    tiles.Add(currentTile);
                }
            }

            /*foreach (int rank in seqn)
            {
                int index = tileRanks.IndexOf(rank);
                tileTarget.Add(GameManager.Players[0].Hand[index].GetComponent<DragTile>().basePosition);

                // maybe a BasePosition array would be best
            }*/

            foreach (GameObject tile in tiles)
            {
                int index = Hand.IndexOf(tile);
                tileTarget.Add(GameManager.Players[0].Hand[index].GetComponent<DragTile>().basePosition);
            }   

            foreach (GameObject tile in tiles)
            {
                StartCoroutine(tile.GetComponent<DragTile>().Hover());
            }
            yield return new WaitForSeconds(0.15f);
            
            float elapsedTime = 0f;
            while (elapsedTime < 0.5f)
            {
                foreach (GameObject tile in tiles)
                {
                    Vector3 startPos = tile.GetComponent<DragTile>().basePosition;
                    Vector3 finalPos = tileTarget[tiles.IndexOf(tile)];

                    Vector3 position = tile.transform.position;
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
                Vector3 position = tile.transform.position;
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

            foreach (GameObject tile in tiles)
            {
                StartCoroutine(tile.GetComponent<DragTile>().HoverDown());
            }
            yield return new WaitForSeconds(0.15f);
        }

        busySorting = false;
    }

    public class TileComparer : IComparer<string>
    {    
        private string SuitOrder()
        {
            /* Create suit hierarchy */
            Dictionary<char, int> suitHierarchy = new Dictionary<char, int>
            {
                {'b', 0}, {'c', 0}, {'s', 0},   // Leftmost
                {'t', 1}, {'f', 2}              // Rightmost
            };

            /* Calculate index density (the median index of all tiles of a suit) */
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

        private string UnitOrder()
        {
            /* Calculate index density (the median index of all tiles of a unit) */
            var indexDensity = GameManager.Players[0].Hand
                .Select((tile, index) => (unit: tile.name[1], index))
                .Where(pair => Char.IsLetter(pair.unit))
                .GroupBy(pair => pair.unit)
                .ToDictionary(
                    grp => grp.Key, 
                    grp => grp.Average(pair => pair.index)
                );

            /* Sort unit based on (Hierarchy) and (Index Density) */
            var sortedLetters = indexDensity
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key);

            return string.Concat(sortedLetters) + "123456789";
        }

        public int Compare(string x, string y)
        {
            if (suitOrder == null)
            {
                suitOrder = SuitOrder();
                unitOrder = UnitOrder();
            }

            int suitX = suitOrder.IndexOf(x[0]);
            int suitY = suitOrder.IndexOf(y[0]);
            int suitComparison = suitX.CompareTo(suitY);
            if (suitComparison != 0)
            {
                return suitComparison;
            }

            int unitX = unitOrder.IndexOf(x[1]);
            int unitY = unitOrder.IndexOf(y[1]);
            int unitComparison = unitX.CompareTo(unitY);
            if (unitComparison != 0)
            {
                return unitComparison;
            }

            string signX = x.Substring(3);
            string signY = y.Substring(3);
            return signX.CompareTo(signY);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && busySorting != true)
        {
            busySorting = true;

            suitOrder = null;
            unitOrder = null;
            StartCoroutine(SortTiles());
        }
    }
}


// 12 x 13 14 15 x x x
// becomes 15 14 13 x 12
