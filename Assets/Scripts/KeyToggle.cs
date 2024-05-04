using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyToggle : MonoBehaviour
{
    private Sprite R_Down, S_Down, O_Down, T_Down;
    private Sprite R_Up, S_Up, O_Up, T_Up;
    private Image R_Image, S_Image, O_Image, T_Image;

    void Start()
    {
        R_Image = GameObject.Find("Canvas/R-Key").GetComponent<Image>();
        S_Image = GameObject.Find("Canvas/S-Key").GetComponent<Image>();
        O_Image = GameObject.Find("Canvas/O-Key").GetComponent<Image>();
        T_Image = GameObject.Find("Canvas/T-Key").GetComponent<Image>();

        R_Down = Resources.Load<Sprite>("Icons/R-Down2");
        S_Down = Resources.Load<Sprite>("Icons/S-Down2");
        O_Down = Resources.Load<Sprite>("Icons/O-Down2");
        T_Down = Resources.Load<Sprite>("Icons/T-Down2");

        R_Up = Resources.Load<Sprite>("Icons/R-Up");
        S_Up = Resources.Load<Sprite>("Icons/S-Up");
        O_Up = Resources.Load<Sprite>("Icons/O-Up");
        T_Up = Resources.Load<Sprite>("Icons/T-Up");
    }

    void Update()
    {
        if (Input.GetKeyDown (KeyCode.R))
        {
            R_Image.sprite = R_Down;
        }
        if (Input.GetKeyUp (KeyCode.R))
        {
            R_Image.sprite = R_Up;
        }

        if (Input.GetKeyDown (KeyCode.S))
        {
            S_Image.sprite = S_Down;
        }
        if (Input.GetKeyUp (KeyCode.S))
        {
            S_Image.sprite = S_Up;
        }

        if (Input.GetKeyDown (KeyCode.O))
        {
            O_Image.sprite = O_Down;
        }
        if (Input.GetKeyUp (KeyCode.O))
        {
            O_Image.sprite = O_Up;
        }

        if (Input.GetKeyDown (KeyCode.T))
        {
            T_Image.sprite = T_Down;
        }
        if (Input.GetKeyUp (KeyCode.T))
        {
            T_Image.sprite = T_Up;
        }
        
    }
}
