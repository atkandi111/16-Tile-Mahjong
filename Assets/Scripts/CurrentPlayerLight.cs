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
    }
}
