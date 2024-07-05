using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardListener : MonoBehaviour
{
    public static bool chaoRequested, pongRequested, kangRequested;
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
                            break;
                        }
                    }
                }
            }

            if (currentIndex == 4)
            {
                chaoRequested = false;
                pongRequested = false;
                kangRequested = false;

                /* accept latest request */
                if (currentListener == "CHAO")
                {
                    MeldText.Instance.PreChao(0);
                    chaoRequested = true;
                }

                if (currentListener == "PONG")
                {
                    MeldText.Instance.PrePong(0);
                    pongRequested = true;
                }
                
                if (currentListener == "KANG")
                {   
                    MeldText.Instance.PreKang(0);
                    kangRequested = true;
                }
                
                currentIndex = 0;
                currentListener = null;
            }   
        }
    }
}
