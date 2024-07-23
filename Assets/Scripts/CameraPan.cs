using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour // static
{
    const float minRadians = 0f, maxRadians = Mathf.PI / 2;
    const float deltaRadians = 0.02f;

    public static float percentArc, groundNegative;
    private float radius, radians;
    private float x, y;

    void Start()
    {
        x = 9f; // 9.5f
        y = 7f; // 7.15f
        radians = Mathf.Atan2(y, x);
        percentArc = radians / maxRadians;
        groundNegative = percentArc * GameManager.tileOffset.y;

        radius = Mathf.Sqrt(Mathf.Pow(y, 2) + Mathf.Pow(x, 2));

        MoveCamera();
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            radians = Mathf.Clamp(radians + deltaRadians, minRadians, maxRadians); // convert to 0-100 perc
            percentArc = radians / maxRadians;
            groundNegative = percentArc * GameManager.tileOffset.y;
            MoveCamera();
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            radians = Mathf.Clamp(radians - deltaRadians, minRadians, maxRadians);
            percentArc = radians / maxRadians;
            groundNegative = percentArc * GameManager.tileOffset.y;
            MoveCamera();
        }
    }


    void MoveCamera()
    {
        x = radius * Mathf.Cos(radians);
        y = radius * Mathf.Sin(radians) + 3f;
        transform.position = new Vector3(0, y, -x);

        float angle = 0.75f - (percentArc * 3f);
        transform.LookAt(new Vector3(0, angle, 0));

        foreach (GameObject tile in GameManager.Players[0].Hand)
        {
            Vector3 tilePos = tile.GetComponent<DragTile>().basePosition;
            Quaternion tileRot = tile.GetComponent<DragTile>().baseRotation;

            Quaternion rotation = Quaternion.LookRotation(
                transform.position - new Vector3(0, 2f + GameManager.tileOffset.y - groundNegative, - Perimeter.HandArea), 
                tileRot * Vector3.up
            );

            Vector3 position = tile.GetComponent<DragTile>().basePosition;
            position.y = GameManager.tileOffset.y - groundNegative;

            if (tile.GetComponent<TileManager>().enabled)
            {
                tile.GetComponent<TileManager>().finalPos = position;
                tile.GetComponent<TileManager>().finalRot = rotation;
            }
            else
            {
                tile.transform.position = position;
                tile.transform.rotation = rotation;
            }

            tile.GetComponent<DragTile>().UpdateBasePosition(position, rotation);
        }
    }
}


// weird yung pan during sort