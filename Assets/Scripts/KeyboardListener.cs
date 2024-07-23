using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardListener : MonoBehaviour
{
    public static bool winRequested, chaoRequested, pongRequested, kangRequested;
    private int currentIndex;
    private string currentListener;
    Dictionary<char, KeyCode> keyMap = new Dictionary<char, KeyCode>()
    {
        {'A', KeyCode.A}, {'B', KeyCode.B}, {'C', KeyCode.C}, {'D', KeyCode.D},
        {'E', KeyCode.E}, {'F', KeyCode.F}, {'G', KeyCode.G}, {'H', KeyCode.H},
        {'I', KeyCode.I}, {'J', KeyCode.J}, {'K', KeyCode.K}, {'L', KeyCode.L},
        {'M', KeyCode.M}, {'N', KeyCode.N}, {'O', KeyCode.O}, {'P', KeyCode.P},
        {'Q', KeyCode.Q}, {'R', KeyCode.R}, {'S', KeyCode.S}, {'T', KeyCode.T}, 
        {'U', KeyCode.U}, {'V', KeyCode.V}, {'W', KeyCode.W}, {'X', KeyCode.X}, 
        {'Y', KeyCode.Y}, {'Z', KeyCode.Z},
    };
    
    void OnEnable()
    {
        winRequested = false;
        chaoRequested = false;
        pongRequested = false;
        kangRequested = false;
        
        currentIndex = 0;
        currentListener = null;
    }

    void Update() // convert to coroutine
    {
        if (Input.anyKey)
        {
            if (currentListener == null)
            {
                if (Input.GetKeyDown (KeyCode.W))
                    currentListener = "WIN";

                if (Input.GetKeyDown (KeyCode.C))
                    currentListener = "CHAO";

                if (Input.GetKeyDown (KeyCode.P))
                    currentListener = "PONG";

                if (Input.GetKeyDown (KeyCode.K))
                    currentListener = "KANG";
            }
            
            if (currentListener != null)
            {
                char targetChar = currentListener[currentIndex];
                if (Input.GetKeyDown (keyMap[targetChar]))
                {
                    Debug.Log(currentIndex);
                    currentIndex += 1;
                }
                else
                {
                    foreach (KeyCode keyCode in keyMap.Values)
                    {
                        if (Input.GetKeyDown (keyCode))
                        {
                            currentIndex = 0;
                            currentListener = null;
                            return;
                        }
                    }
                }

                if (currentIndex == currentListener.Length)
                {
                    winRequested = false;
                    chaoRequested = false;
                    pongRequested = false;
                    kangRequested = false;

                    /* accept latest request */
                    if (currentListener == "WIN")
                    {
                        MeldText.Instance.PreWin(0);
                        winRequested = true;

                        if (GameManager.currentPlayer == GameManager.Players[0])
                        {
                            if (GameManager.Players[0].engine.WillWin())
                            {
                                AudioManager.Instance.playWin();
                                MeldText.Instance.OnWin(0);
                                Debug.Log("todas");
                            }
                        }
                    }

                    if (currentListener == "CHAO" && GameManager.currentPlayer != GameManager.Players[0])
                    {
                        MeldText.Instance.PreChao(0);
                        chaoRequested = true;
                    }

                    if (currentListener == "PONG" && GameManager.currentPlayer != GameManager.Players[0])
                    {
                        MeldText.Instance.PrePong(0);
                        pongRequested = true;
                    }
                    
                    if (currentListener == "KANG")
                    {   
                        MeldText.Instance.PreKang(0);
                        kangRequested = true;

                        if (GameManager.currentPlayer == GameManager.Players[0])
                        {
                            List<GameObject> kangBlock = GameManager.currentPlayer.Hand
                                .Where(go => GameManager.currentPlayer.Open.Count(xo => xo.name == go.name) == 3)
                                .ToList();
                            
                            List<GameObject> scrtBlock = GameManager.currentPlayer.Hand
                                .GroupBy(go => go.name)
                                .Where(group => group.Count() == 4)
                                .SelectMany(group => group)
                                .ToList();

                            if (kangBlock.Count() + scrtBlock.Count() > 0)
                            {
                                AudioManager.Instance.playKang();
                                MeldText.Instance.OnKang(0);

                                if (kangBlock.Count() > 0)
                                    GameManager.currentPlayer.OpenTile(kangBlock);
                                if (scrtBlock.Count() > 0)
                                    GameManager.Players[0].ScrtTile(scrtBlock);
                            }
                        }
                    }
                    
                    currentIndex = 0;
                    currentListener = null;
                }   
            }
        }
    }
}
