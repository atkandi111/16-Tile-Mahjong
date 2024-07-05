using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Engine
{
    private Player player;
    /*private static string[] tileNames = new string[]
    {
        "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b9",
        "s1", "s2", "s3", "s4", "s5", "s6", "s7", "s8", "s9",
        "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9",
        "tN", "tS", "tW", "tE", "tG", "tR", "tP", "fR", "fB"
    };
    private Dictionary<string, int> reference = tileNames.ToDictionary(name => name, _ => 4);*/
    public Engine(Player player)
    {
        this.player = player;
    }
    public Dictionary<string, int> knowledgeBase = new Dictionary<string, int>(); 
    public void UpdateKB(string tile)
    {
        this.knowledgeBase[tile] -= 1;
        if (this.knowledgeBase[tile] < 0)
        {
            Debug.Log(tile);
            Debug.Log(this.player.playerID);
            throw new ArgumentException("negative KB");
        }
    }

    HashSet<string> preKang = new HashSet<string>();
    HashSet<string> prePong = new HashSet<string>();
    HashSet<string> preChow = new HashSet<string>();

    public string ChooseDiscard()
    {
        /*this.knowledgeBase = new Dictionary<string, int>(this.reference);
        var tossed = GameManager.TileToss
            .Select(tile => tile.name)
            .GroupBy(name => name)
            .ToDictionary(group => group.Key, group => group.Count());

        var opened = 


        return this.knowledgeBase.ToDictionary(
            tile => tile.Key,
            tile => tile.Value - dict2.GetValue(tile.Key)
        );*/
        this.prePong.Clear();
        this.preChow.Clear();

        // this.knowledgeBase()

        return AnalyzeHand(player.Hand.Select(tile => tile.name).ToList())[0];
    }

    public List<string> AnalyzeHand(List<string> candidates)
    {
        /*foreach (string tile in candidates)
        {
            this.knowledgeBase[tile] -= 1;
            if (knowledgeBase[tile] < 0)
            {
                throw new ArgumentException("negative KB");
            }
        }*/

        // rename Frequency to Count
        candidates = DecomposeMeld(candidates);
        candidates = DecomposeNeed(candidates);
        candidates = DecomposeNear(candidates);

        return candidates;
    }
    private List<string> newDecomposeMeld(List<string> candidates)
    {
        Dictionary<string, int> handFrequency = candidates
            .GroupBy(tile => tile)
            .ToDictionary(group => group.Key, group => group.Count());

        Dictionary<string, float> meldFrequency = candidates
            .GroupBy(tile => tile)
            .ToDictionary(group => group.Key, _ => 0f);

        List<string[]> melds = new List<string[]>();
        foreach (string tile in handFrequency.Keys)
        {
            if (handFrequency[tile] > 2) 
                melds.Add(new string[] {tile, tile, tile});
            
            if (char.IsDigit(tile[1])) 
            {
                char suit = tile[0];
                int unit = tile[1] - '0';
                
                string[] chow = new string[] {
                    $"{suit}{unit}", 
                    $"{suit}{unit + 1}", 
                    $"{suit}{unit + 2}"
                };

                if (chow.All(item => handFrequency.ContainsKey(item)))
                {
                    for (int i = 0; i < chow.Min(tile => handFrequency[tile]); i++)
                    {
                        melds.Add(chow);

                    }
                }
            }
        }
        
        for (int meldCount = candidates.Count / 3; meldCount >= 0; meldCount--)
        {
            // DCMP = decompositon
            //var combinations = Grouper.Combinations(melds, meldCount); 
            // Debug.Log(combinations.Length);

            foreach (List<string[]> meldDCMP in Grouper.Combinations(melds, meldCount)) // convert to IEnumerator
            {
                Dictionary<string, int> dcmpFrequency = meldDCMP
                    .SelectMany(array => array)
                    .GroupBy(tile => tile)
                    .ToDictionary(group => group.Key, group => group.Count());

                bool invalidDCMP = dcmpFrequency.Any(tile => tile.Value > handFrequency[tile.Key]);
                if (invalidDCMP)
                    continue;

                foreach (var tile in dcmpFrequency)
                {
                    if (meldFrequency.ContainsKey(tile.Key))
                    {
                        meldFrequency[tile.Key] += tile.Value;
                    }
                    else
                    {
                        meldFrequency[tile.Key] = tile.Value;
                    }
                }

                meldCount = -1; // = 0 and sentinel should be meldCount > 0
            }
        }

        // meldFrequency is the number of melds lost if the tile is discarded //
        int maxCounter = 0;
        List<string> meldKeys = meldFrequency.Keys.ToList();
        foreach (string tile in meldKeys)
        {
            meldFrequency[tile] = (int) Math.Round((double) meldFrequency[tile] / handFrequency[tile], 2);
            if (maxCounter < meldFrequency[tile])
            {
                maxCounter = (int) meldFrequency[tile];
            }
        }

        List<string> shortlist = new List<string>();
        foreach (string tile in meldKeys)
        {
            for (int i = 0; i < handFrequency[tile]; i++)
            {
                if ((i + 1) * meldFrequency[tile] >= maxCounter)
                    break;

                shortlist.Add(tile);
            }
        }

        return shortlist.Count > 0 ? shortlist : candidates;
    }

    private List<string> DecomposeMeld(List<string> candidates)
    {
        Dictionary<string, int> handFrequency = candidates
            .GroupBy(tile => tile)
            .ToDictionary(group => group.Key, group => group.Count());

        Dictionary<string, float> meldFrequency = candidates
            .GroupBy(tile => tile)
            .ToDictionary(group => group.Key, _ => 0f);

        List<string[]> melds = new List<string[]>();
        foreach (string tile in handFrequency.Keys)
        {
            if (handFrequency[tile] > 2) 
            {
                melds.Add(new string[] {tile, tile, tile});
                preKang.Add(tile);
            }
            
            if (char.IsDigit(tile[1])) 
            {
                char suit = tile[0];
                int unit = tile[1] - '0';
                
                string[] chow = new string[] {
                    $"{suit}{unit}", 
                    $"{suit}{unit + 1}", 
                    $"{suit}{unit + 2}"
                };

                if (chow.All(item => handFrequency.ContainsKey(item)))
                {
                    for (int i = 0; i < chow.Min(tile => handFrequency[tile]); i++)
                    {
                        melds.Add(chow);
                    }
                }
            }
        }
        
        for (int meldCount = candidates.Count / 3; meldCount >= 0; meldCount--)
        {
            // DCMP = decompositon
            var combinations = Grouper.Combinations(melds, meldCount);
            // Debug.Log(combinations.Length);

            foreach (List<string[]> meldDCMP in combinations)
            {
                Dictionary<string, int> dcmpFrequency = meldDCMP
                    .SelectMany(array => array)
                    .GroupBy(tile => tile)
                    .ToDictionary(group => group.Key, group => group.Count());

                bool invalidDCMP = dcmpFrequency.Any(tile => tile.Value > handFrequency[tile.Key]);
                if (invalidDCMP)
                    continue;

                foreach (var tile in dcmpFrequency)
                {
                    if (meldFrequency.ContainsKey(tile.Key))
                    {
                        meldFrequency[tile.Key] += tile.Value;
                    }
                    else
                    {
                        meldFrequency[tile.Key] = tile.Value;
                    }
                }

                meldCount = -1; // = 0 and sentinel should be meldCount > 0
            }
        }

        // meldFrequency is the number of melds lost if the tile is discarded //
        int maxCounter = 0;
        List<string> meldKeys = meldFrequency.Keys.ToList();
        foreach (string tile in meldKeys)
        {
            meldFrequency[tile] = (int) Math.Round((double) meldFrequency[tile] / handFrequency[tile], 2);
            if (maxCounter < meldFrequency[tile])
            {
                maxCounter = (int) meldFrequency[tile];
            }
        }

        List<string> shortlist = new List<string>();
        foreach (string tile in meldKeys)
        {
            for (int i = 0; i < handFrequency[tile]; i++)
            {
                if ((i + 1) * meldFrequency[tile] >= maxCounter)
                    break;

                shortlist.Add(tile);
            }
        }

        return shortlist.Count > 0 ? shortlist : candidates;
    }

    private List<string> DecomposeNeed(List<string> candidates)
    {
        /* 
        degree of T in pmeld = num times T appears in other pmelds
        T with lower degree is less valuable than higher degree

        (because Ts with shared needs have less effect on deficiency if removed)

        to respect the degree of a pmeld,
        the value of a need is inversely proportional to its count in needList

        (because degree of T = number of pmelds that have its need)
        */

        Dictionary<string, int> handFrequency = candidates
            .GroupBy(tile => tile)
            .ToDictionary(group => group.Key, group => group.Count());

        Dictionary<string, float> needFrequency = candidates
            .GroupBy(tile => tile)
            .ToDictionary(group => group.Key, _ => 0f);

        List<string> handKeys = handFrequency.Keys.ToList();
        foreach (string tile in handKeys)
        { 
            if (handFrequency[tile] >= 2)
            {
                needFrequency[tile] += knowledgeBase[tile] * handFrequency[tile];
                // adds weight (preference for kangable)

                prePong.Add(tile);
            }

            if (char.IsNumber(tile[1]))
            {
                char suit = tile[0];
                int unit = int.Parse(tile[1].ToString());

                string[] seqn = new string[] {
                    $"{suit}{unit - 1}", 
                    $"{suit}{unit + 0}", 
                    $"{suit}{unit + 1}", 
                    $"{suit}{unit + 2}"
                };

                if (handFrequency.ContainsKey(seqn[2])) // __ tB tC __ (outer chow)
                {
                    int cmpCount = AvailableCompletors(seqn[1], seqn[2]);

                    needFrequency[seqn[1]] += cmpCount / handFrequency[seqn[1]];
                    needFrequency[seqn[2]] += cmpCount / handFrequency[seqn[2]];

                    preChow.Add(seqn[0]);
                    preChow.Add(seqn[3]);
                }

                if (handFrequency.ContainsKey(seqn[3])) // tB __ tD (inner chow)
                {
                    int cmpCount = AvailableCompletors(seqn[1], seqn[3]);

                    needFrequency[seqn[1]] += cmpCount / handFrequency[seqn[1]];
                    needFrequency[seqn[3]] += cmpCount / handFrequency[seqn[3]];

                    preChow.Add(seqn[2]);
                }
            }
        }

        float minCount = needFrequency.Values.Min();
        return needFrequency
            .Where(tile => tile.Value == minCount)
            .Select(tile => tile.Key)
            .ToList();
    }

    private List<string> DecomposeNear(List<string> candidates)
    {
        Dictionary<string, int> nearFrequency = candidates
            .GroupBy(tile => tile)
            .ToDictionary(group => group.Key, _ => 0);

        foreach (string tile in candidates)
        {
            nearFrequency[tile] += knowledgeBase[tile];

            if (char.IsNumber(tile[1]))
            {
                char suit = tile[0];
                int unit = tile[1] - '0';

                string[] near = new string[] {
                    $"{suit}{unit - 2}", 
                    $"{suit}{unit - 1}",
                    $"{suit}{unit + 0}", 
                    $"{suit}{unit + 1}", 
                    $"{suit}{unit + 2}"
                };

                if (knowledgeBase.ContainsKey(near[1]) && knowledgeBase[near[1]] > 0)
                {
                    nearFrequency[tile] += knowledgeBase[near[1]];
                    if (knowledgeBase.ContainsKey(near[0]))
                    {
                        nearFrequency[tile] += knowledgeBase[near[0]];
                    }
                }
                if (knowledgeBase.ContainsKey(near[3]) && knowledgeBase[near[3]] > 0)
                {
                    nearFrequency[tile] += knowledgeBase[near[3]];
                    if (knowledgeBase.ContainsKey(near[4]))
                    {
                        nearFrequency[tile] += knowledgeBase[near[4]];
                    }
                }
            }
        }

        int minCount = nearFrequency.Values.Min();
        return nearFrequency
            .Where(tile => tile.Value == minCount)
            .Select(tile => tile.Key)
            .ToList();
    }
    public bool WillWin(string tile)
    {
        return false;
    }
    public bool WillKang(string tile)
    {
        return preKang.Contains(tile); // && preKang is a disjoint meld
    }

    public bool WillPong(string tile)
    {
        // remove prePong.Count > 1 and do the same as WillChao
        return prePong.Contains(tile) && prePong.Count > 1; // remove >1 if engine is of human
    }
    public bool WillChao(string tile)
    {
        if (preChow.Contains(tile))
        {
            var oldHandFrequency = player.Hand.Select(tile => tile.name)
                .GroupBy(tile => tile)
                .ToDictionary(group => group.Key, group => group.Count());
            //.where (suit = tile_suit)

            var newHandFrequency = new Dictionary<string, int>(oldHandFrequency);
            if (newHandFrequency.ContainsKey(tile))
            {
                newHandFrequency[tile] += 1;
            }
            else
            {
                newHandFrequency.Add(tile, 1);
            }

            int oldDCMP = GroupMelds(oldHandFrequency).Count + GroupPairs(oldHandFrequency).Count;
            int newDCMP = GroupMelds(newHandFrequency).Count + GroupPairs(newHandFrequency).Count;
             
            // if (newDCMP > oldDCMP)
            if (GroupMelds(newHandFrequency).Count > GroupMelds(oldHandFrequency).Count)
            {
                return true;
            }
        }

        return false;
    }

    // include one pair as eye ,, try pair / numpairs = 1 meld
    List<string> GroupPairs(Dictionary<string, int> handFrequency)
    {
        return handFrequency
            .Where(tile => tile.Value > 2)
            .Select(tile => tile.Key)
            .ToList();
    }
    List<string[]> GroupMelds(Dictionary<string, int> handFrequency)
    {
        List<string[]> melds = new List<string[]>();
        foreach (string tile in handFrequency.Keys)
        {
            if (handFrequency[tile] > 2) 
                melds.Add(new string[] {tile, tile, tile});
            
            if (char.IsDigit(tile[1])) 
            {
                char suit = tile[0];
                int unit = tile[1] - '0';
                
                string[] chow = new string[] {
                    $"{suit}{unit}", 
                    $"{suit}{unit + 1}", 
                    $"{suit}{unit + 2}"
                };

                if (chow.All(item => handFrequency.ContainsKey(item)))
                {
                    for (int i = 0; i < chow.Min(tile => handFrequency[tile]); i++)
                    {
                        melds.Add(chow);
                    }
                }
            }
        }

        return melds;
    }
    
    //remove willchao and direct getchao
    //return "" if getchao is empty
    public (string, string) ChooseChao(string tile) 
    {
        Dictionary<string, int> handFrequency = player.Hand.Select(tile => tile.name)
            .GroupBy(tile => tile)
            .ToDictionary(group => group.Key, group => group.Count());

        if (preChow.Contains(tile))
        {
            char suit = tile[0];
            int unit = tile[1] - '0';

            string[] seqn = new string[] {
                $"{suit}{unit - 2}", 
                $"{suit}{unit - 1}",
                $"{suit}{unit + 0}", 
                $"{suit}{unit + 1}", 
                $"{suit}{unit + 2}"
            };

            int leftChao = 0, midChao = 0, rightChao = 0;
            if (handFrequency.ContainsKey(seqn[0]) && handFrequency.ContainsKey(seqn[1]))
            {
                var tempFrequency = new Dictionary<string, int>(handFrequency);
                tempFrequency[seqn[0]] -= 1;
                tempFrequency[seqn[1]] -= 1;
                leftChao = GroupMelds(tempFrequency).Count + 1;
            }

            if (handFrequency.ContainsKey(seqn[1]) && handFrequency.ContainsKey(seqn[3]))
            {
                var tempFrequency = new Dictionary<string, int>(handFrequency);
                tempFrequency[seqn[1]] -= 1;
                tempFrequency[seqn[3]] -= 1;
                midChao = GroupMelds(tempFrequency).Count + 1;
            }

            if (handFrequency.ContainsKey(seqn[3]) && handFrequency.ContainsKey(seqn[4]))
            {
                var tempFrequency = new Dictionary<string, int>(handFrequency);
                tempFrequency[seqn[3]] -= 1;
                tempFrequency[seqn[4]] -= 1;
                rightChao = GroupMelds(tempFrequency).Count + 1;
            }

            // if equal max, choose min available completors
            // so that min opportunity cost
            int max = Math.Max(midChao, Math.Max(leftChao, rightChao));
            if (max == 0)
            {
                return (null, null);
            }
            if (leftChao == max)
                return (seqn[0], seqn[1]);

            if (rightChao == max)
                return (seqn[3], seqn[4]);

            if (midChao == max)
                return (seqn[1], seqn[3]);
        }

        return (null, null);
    }

    /*(string, string) Chao(string tile)
    {
        (string, string) minGroup = (null, null);
        int minCount = int.MaxValue;

        if (preChow.Contains(tile))
        {
            foreach ((string, string) grp in preChow[tile])
            {
                (string tile1, string tile2) = grp;
                int cmpCount = AvailableCompletors(tile1, tile2);

                if (cmpCount < minCount)
                {
                    minCount = cmpCount;
                    minGroup = grp;
                }
            }
        }
        
        return minGroup;
    }*/

    int AvailableCompletors(string tile1, string tile2) // useless
    {
        char suit = tile1[0];

        int unit1 = tile1[1] - '0';
        int unit2 = tile2[1] - '0';

        /* ------pong------ */
        if (unit2 - unit1 == 0)
        {
            return knowledgeBase[tile1];
        } 

        /* ---outer-chow--- */
        else if (unit2 - unit1 == 1)
        {
            string completor1 = suit.ToString() + (unit1 - 1).ToString();
            string completor2 = suit.ToString() + (unit2 + 1).ToString(); 

            int count1 = knowledgeBase.ContainsKey(completor1) ? knowledgeBase[completor1] : default(int);
            int count2 = knowledgeBase.ContainsKey(completor2) ? knowledgeBase[completor2] : default(int);
            return count1 + count2;           
        }

        /* ---inner-chow--- */
        else if (unit2 - unit1 == 2) 
        {
            string completor = suit.ToString() + (unit1 + 1).ToString();
            return knowledgeBase[completor];
        }

        // throw new ArgumentException

        return 0;
    }
}
public static class Grouper
{
    public static List<string[]> GroupMelds()
    {
        return new List<string[]>();
    }

    private static IEnumerable<int[]> CombinationIndex(int m, int n)
    {
        int[] result = new int[m];
        Stack<int> stack = new Stack<int>(m);
        stack.Push(0);
        while (stack.Count > 0)
        {
            int index = stack.Count - 1;
            int value = stack.Pop();
            while (value < n)
            {
                result[index++] = value++;
                stack.Push(value);
                if (index != m) 
                { 
                    continue;
                }

                yield return result;
                break;
            }
        }
    }

    public static IEnumerable<List<string[]>> Combinations(List<string[]> hand, int m)
    {
        string[][] result = new string[m][];
        if (m < 1)
            yield return result.ToList();
        else
        {
            foreach (int[] j in CombinationIndex(m, hand.Count))
            {
                for (int i = 0; i < m; i++)
                {
                    result[i] = hand[j[i]];
                }
                // yield return (int[]) result.Clone();
                yield return result.ToList();
            }
        }
    }
}

// Engine shouldn't be static, each player has own instances of Engine 


// threshold for:
// 7-pair preference -> try (> 5) pairs and one set
// todopong preference -> try (> 3) pongs and triplets
// escalera preference -> try one suit has (> 7) different units

// threshold flowchart:
// if pref 7-pair, else if pref todopong, else if pref escalera, else normal



// for typing pong and chow and win, do a dota-like (or terminal-like) chat on bottomleft


//groupmelds .where (suit = tile_suit)

//esc to cancel pong

//do initial handanalysis