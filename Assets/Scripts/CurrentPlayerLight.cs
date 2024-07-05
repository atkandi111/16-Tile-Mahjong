using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLight : MonoBehaviour // rename to player light
{
    private static PlayerLight instance;
    public static PlayerLight Instance
    {
        get { return instance; }
    }
    public Light playerLight;

    void Start()
    {
        instance = this;
        playerLight = gameObject.GetComponent<Light>();
        UpdateLight();
    }
    
    public void UpdateLight(int playerIndex = 0)
    {
        playerLight.transform.position = Quaternion.Euler(0, -90f * playerIndex, 0) * new Vector3(0f, 0.4f, -4.5f);
        /*switch (playerIndex)
        {
            case 0:
                playerLight.transform.position = new Vector3(0f, 0.4f, -4.5f);
                break;
            case 1:
                playerLight.transform.position = new Vector3(4.75f, 0.4f, 0f);
                break;
            case 2:
                playerLight.transform.position = new Vector3(0f, 0.4f, 5.5f);
                break;
            case 3:
                playerLight.transform.position = new Vector3(-4.75f, 0.4f, 0f);
                break;
            default:
                playerLight.transform.position = new Vector3(0f, -1f, 0f);
                break;
        }*/

        // use quaternion multiplication
    }
}
