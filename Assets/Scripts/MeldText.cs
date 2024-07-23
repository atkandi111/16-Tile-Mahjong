using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeldText : MonoBehaviour
{
    private Sprite winText, chaoText, pongText, kangText;
    private Image[] imageArray = new Image[4];
    private static MeldText instance;
    public static  MeldText Instance
    {
        get { return instance; }
    }

    void Start()
    {
        instance = this;
        imageArray[0] = GameObject.Find("MeldCanvas/P0-Text").GetComponent<Image>();
        imageArray[1] = GameObject.Find("MeldCanvas/P1-Text").GetComponent<Image>();
        imageArray[2] = GameObject.Find("MeldCanvas/P2-Text").GetComponent<Image>();
        imageArray[3] = GameObject.Find("MeldCanvas/P3-Text").GetComponent<Image>();

        for (int i = 0; i < 4; i++)
        {
            Hide(i);
        }

        winText = Resources.Load<Sprite>("MeldText/Todas");
        chaoText = Resources.Load<Sprite>("MeldText/Chao");
        pongText = Resources.Load<Sprite>("MeldText/Pong");
        kangText = Resources.Load<Sprite>("MeldText/Kang");
    }

    public void OnWin(int playerIndex)
    {
        imageArray[0].enabled = false;

        Image image = imageArray[playerIndex];
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        image.sprite = winText;
        image.enabled = true;
    }

    public void OnChao(int playerIndex)
    {
        imageArray[0].enabled = false;

        Image image = imageArray[playerIndex];
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        image.sprite = chaoText;
        image.enabled = true;
    }

    public void OnPong(int playerIndex)
    {
        imageArray[0].enabled = false;

        Image image = imageArray[playerIndex];
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        image.sprite = pongText;
        image.enabled = true;
    }

    public void OnKang(int playerIndex)
    {
        imageArray[0].enabled = false;

        Image image = imageArray[playerIndex];
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        image.sprite = kangText;
        image.enabled = true;
    }
    public void Hide(int playerIndex)
    {
        imageArray[playerIndex].enabled = false;
    }

    /* for Player[0] only */
    public void PreWin(int playerIndex)
    {
        Image image = imageArray[playerIndex];
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.25f);
        image.sprite = winText;
        image.enabled = true;
    }
    public void PreChao(int playerIndex)
    {
        Image image = imageArray[playerIndex];
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.25f);
        image.sprite = chaoText;
        image.enabled = true;
    }
    public void PrePong(int playerIndex)
    {
        Image image = imageArray[playerIndex];
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.25f);
        image.sprite = pongText;
        image.enabled = true;
    }
    public void PreKang(int playerIndex)
    {
        Image image = imageArray[playerIndex];
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.25f);
        image.sprite = kangText;
        image.enabled = true;
    }
}
