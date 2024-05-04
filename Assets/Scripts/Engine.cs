using System;
using System.Collections;
using System.Collections.Generic;
public static class Engine
{
    static Random random = new Random();
    public static string ChooseDiscard(List<string> hand)
    {
        int randomIndex = random.Next(0, hand.Count);
        return hand[randomIndex];
    }

    public static bool ShouldEatTile(List<string> hand, string tile)
    {
        return true;
    }
}
