#Taiwanese Mahjong
#16-Tile Variant

from collections import Counter
from itertools import combinations, chain
from random import shuffle, choice
from time import time
import math

print("\033c", end = "")

def Group_Sets(hand):
    freq = Counter(hand)
    pair = [[x] * 2 for x in freq if freq[x] > 1]
    pong = [[x] * 3 for x in freq if freq[x] > 2]
    chow = []
    for item in freq:
        suit, unit = item[0], item[1]
        if unit.isalpha():
            continue

        seqn = [0, 1, 2]
        seqn = [x + int(unit) for x in seqn]
        seqn = [suit + str(x) for x in seqn]
        #tuple comprehension slower than list

        chow += [seqn] * min(freq[x] for x in seqn)
    return pair, pong, chow

def Near_Cards(hand, freq):
    near_count = {}
    for card in set(hand):
        suit, unit = card[0], card[1]
        if unit.isalpha():
            seqn = [card]
        else:
            seqn = [-2, -1, 0, 1, 2]
            seqn = [x + int(unit) for x in seqn]
            seqn = [suit + str(x) for x in seqn]

        near_count[card] = sum(freq[x] for x in seqn)
    
    min_count = min(near_count.values())
    return [x for x in hand if near_count[x] == min_count]

def Tiles_Needed(hand, freq):
    max_need = 0
    need_list = {}
    for test_card in set(hand):
        test_hand = hand.copy()
        test_hand.remove(test_card)
        test_freq = Counter(test_hand)

        tile_needed = {}
        for card in set(test_hand):
            suit, unit = card[0], card[1]
            if unit.isalpha():
                seqn = [card]

                completing_seqn = {
                    (seqn[0], seqn[0]) : (seqn[0])
                }
            else:
                seqn = [-2, -1, 0, 1, 2]
                seqn = [x + int(unit) for x in seqn]
                seqn = [suit + str(x) for x in seqn]

                completing_seqn = {
                    (seqn[0], seqn[2]) : (seqn[1], ),
                    (seqn[1], seqn[2]) : (seqn[0], seqn[3]),
                    (seqn[2], seqn[2]) : (seqn[2], ),
                    (seqn[2], seqn[3]) : (seqn[1], seqn[4]),
                    (seqn[2], seqn[4]) : (seqn[3], )
                }

            for pre_meld, card in completing_seqn.items():
                if (Counter(pre_meld) - test_freq):
                    continue
                for i in card:
                    tile_needed[i] = freq[i]

        if max_need < sum(tile_needed.values()):
            max_need = sum(tile_needed.values())
            need_list.clear()
        if max_need == sum(tile_needed.values()):
            need_list[test_card] = tile_needed.keys()
    
    return [x for x in hand if x in need_list]

def Decompose_Meld2(hand, freq):
    #include eyes
    #then try to merge with check_win
    pair, pong, chow = Group_Sets(hand)
    meldcount = {}

    eye_pool = pair

    for testcard in set(hand):
        testhand = hand.copy()
        testhand.remove(testcard)
        testfreq = Counter(testhand)

        numcount = len(hand) // 3
        while numcount > 0: # check if diff from >=
            for case in combinations(pong + chow, numcount):
                case = chain(*case)
                if (Counter(case) - testfreq):
                    continue

                #consider eye if theres only one pair
                #or if numcount == len(hand) // 3
                """if numcount == len(hand) // 3:
                    for eye in eye_pool"""
                meldcount[testcard] = numcount
                numcount = 0
                break
            numcount = numcount - 1

    maxcount = max(meldcount.values())
    return [x for x in hand if meldcount[x] == maxcount]

def Decompose_Meld(hand, freq):
    pair, pong, chow = Group_Sets(hand)
    meldcount = {}
    maxsuit = {
        "b" : (len([i for i in hand if i[0] == "b"]) - 1) // 3,
        "c" : (len([i for i in hand if i[0] == "c"]) - 1) // 3,
        "s" : (len([i for i in hand if i[0] == "s"]) - 1) // 3
    }

    eye_pool = pair

    for testcard in set(hand):
        testhand = hand.copy()

        if testcard[1].isdigit():
            suithand = [i for i in testhand if i[0] == testcard[0]]
            meldcount[testcard] = 0
            numcount = len(suithand) // 3

            suithand.remove(testcard)
            suitfreq = Counter(suithand)
            suitpair, suitpong, suitchow = Group_Sets(suithand)
            print(suitpong, suitchow)
            while numcount > 0:
                for case in combinations(suitpong + suitchow, numcount):
                    if (Counter(chain(*case)) - suitfreq):
                        continue

                    meldcount[testcard] = (len(suithand) // 3) - numcount
                numcount = numcount - 1
        else:
            meldcount[testcard] = testhand.count(testcard) // 3
    
    maxcount = max(meldcount.values())
    return [x for x in hand if meldcount[x] == maxcount]





#rename variables
#merge dm with check_win
# consider one eye(pair) for hand to win
# if return is empty, it means win
# if meld_count is empty, win

def Suggest_Discard(hand, opened, discard):
    print("/////")
    discard_candidates = hand.copy()
    freq = Counter(reference) - Counter(hand + opened + discard)
    # set freq = 4 - Counter(kb) 
    # so that counter(reference) wouldnt be called anymore

    weights = [Decompose_Meld, Tiles_Needed, Near_Cards]

    global timer
    for func in weights:
        start = time()
        discard_candidates = func(discard_candidates, freq)
        timer[weights.index(func)] += time() - start
        print(Sorter(discard_candidates))
    print("/////")
    return choice(discard_candidates)

    #1. Decompose hand maximizing true-melds
    #2. Decompose hand into pre-melds maximizing tiles_needed
    #3. Find remaining tiles with least number of near_cards

def Check_Win(hand):
    pair, pong, chow = Group_Sets(hand)
    winning_case = False
    class_of_win = "Standard"

    if len(pair) < 7:
        case_pool = combinations(pong + chow, len(hand) // 3)
        eye_pool = pair
    else: #7 Pairs
        case_pool = combinations(pair, 7)
        eye_pool = pong + chow

    for item in case_pool:
        case = [x for meld in item for x in meld]
        for eye in eye_pool:
            if Counter(case + eye) != Counter(hand):
                continue
            
            winning_case = True
            if all(x in pair for x in item):
                class_of_win = "7 Pairs"
            if all(x in pong for x in item): 
                class_of_win = "Todo Pong"
        
    return winning_case, class_of_win

def Sorter(hand):
    suit = [x for x in hand if x[1].isdigit()]
    null = [x for x in hand if x[1].isalpha()]

    return sorted(suit) + sorted(null)

def Solo_Game():
    tiles = reference.copy()

    """
    naming-scheme: class + value
    ----- classes: ball, stick, char, 
    -------------: direction, trash, flower
    """

    shuffle(tiles)
    hand, tiles = tiles[:16], tiles[16:]
    thand, ttiles = hand.copy(), tiles.copy()
    opened, discard = [], []

    while len(tiles) > 13:
        print("\033c", end = "")
        print("Open:", *opened, sep = " ")
        print("Discard:", *discard, sep = " ")

        print("- - - - - - - - - - - - - -")

        print("Grab:", tiles[0])
        hand.append(tiles.pop(0))

        for idx, card in enumerate(hand):
            while card in ["fR", "fB"] and tiles:
                opened.append(card)
                card = tiles.pop()
            else:
                hand[idx] = card

        hand = Sorter(hand)
        print(*hand, sep = " ")
        if Check_Win(hand)[0]:
            print("You Won!")
            break

        print("Suggested:", Suggest_Discard(hand, opened, discard))

        toss = hand.index(input("Throw: "))
        discard.append(hand.pop(toss))
        #add try-except for incorrect input

        #toss = Suggest_Discard(hand, opened, discard)
        #discard.append(toss)
        #hand.remove(toss)
    else:
        print("Draw!")
        global draw
        draw = draw + 1
    total_discard.append(len(discard))

reference = [
        "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b9",
        "s1", "s2", "s3", "s4", "s5", "s6", "s7", "s8", "s9",
        "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9",
        "dN", "dE", "dW", "dS", "tG", "tR", "tW", "fR", "fB"
    ] * 4

start = time()
draw = 0
timer = [0, 0, 0]
total_discard = []
for _ in range(250):
    Solo_Game()
    print("\033c", end = "")
    print(total_discard)
print(timer)
print("{} secs".format(time() - start))
print("Draws", draw)


#problems:
#doesn't weight pong vs chow
#doesn't know if hand is already waiting
#   if waiting, P(pong) = P(chow)


#when everything is goods
#do two-sample t-test
    #check mean of discards when bot is deciding
    #check mean of discards when I am deciding
#if significantly different,
#or if mean_bot << mean_me, then
#bot-decision-making is flawed/inefficient