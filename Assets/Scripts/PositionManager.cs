/*using System;
using System.Collections;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PositionManager : MonoBehaviour
{
    private static Queue<Action> eventQueue = new Queue<Action>();
    private static List<GameObject> TileArray = new List<GameObject>();
    private static int Cluster, TileIndex = 0, liftRotation;
    private static float Duration, StopWatch, liftPosition;
    private static Vector3 position, wallVector, startVector, stepVector, liftVector;
    private static Vector3 tileSize, tileOffset;
    private static Quaternion rotation;
    private static Random random = new Random();

    public static void ScheduleEvent(float duration, int cluster, List<GameObject> tileArray)
    {
        eventQueue.Enqueue( () => { 
            Duration = duration; 
            Cluster = cluster;
            TileArray = tileArray;
        } );

        GameManager.noRunningSchedules = false;
    }

    public static void AssignPosition(List<GameObject> TileArray, int PlayerID, string tileState, int numTiles, float perimSize, float startOffset = 0f)
    {
        tileSize = GameManager.tileSize;
        tileOffset = GameManager.tileOffset;

        switch (tileState)
        {
            case "Opened":  liftPosition = tileSize.y; liftRotation = 270; break;
            case "Closed":  liftPosition = tileSize.y; liftRotation = 90; break;
            case "Stand":   liftPosition = tileSize.z; liftRotation = 180; break;
            case "UserPOV": liftPosition = 0.154f; liftRotation = 220; break;
        }

        switch (PlayerID)
        {
            case 0: wallVector = new Vector3(0, 0, -1); startVector = new Vector3(-1, 0, 0); stepVector = new Vector3(+1, 0, 0); break; // South 
            case 1: wallVector = new Vector3(+1, 0, 0); startVector = new Vector3(0, 0, -1); stepVector = new Vector3(0, 0, +1); break; // East
            case 2: wallVector = new Vector3(0, 0, +1); startVector = new Vector3(+1, 0, 0); stepVector = new Vector3(-1, 0, 0); break; // North
            case 3: wallVector = new Vector3(-1, 0, 0); startVector = new Vector3(0, 0, +1); stepVector = new Vector3(0, 0, -1); break; // West
        }

        wallVector  *= (perimSize + tileOffset.z);
        startVector *= (tileSize.x * (numTiles / 2) - tileOffset.x - startOffset); 
        stepVector  *= (tileSize.x);

        for (int i = 0; i < TileArray.Count; i++)
        {
            liftVector = ((i / numTiles) + 0.5f) * new Vector3(0, liftPosition, 0);
            position = wallVector + liftVector + startVector + stepVector * (i % numTiles);
            rotation = Quaternion.Euler(liftRotation, -PlayerID * 90, TileArray[i].transform.eulerAngles.z * 180);

            TileArray[i].GetComponent<TileManager>().AddDestination(position, rotation, 3f);
        }
    }

    void Update()
    { // only good for sequential animation
        if (TileIndex == 0)
        {
            if (eventQueue.Count > 0)
            {
                Action nextEvent = eventQueue.Dequeue();
                nextEvent();

                StopWatch = Time.time;
            }
            else
            {
                GameManager.noRunningSchedules = true;
                return;
            }
        }

        float timePassed = (Time.time - StopWatch) / Duration;
        float timeScheduled = TileIndex / Cluster;

        if (timePassed >= timeScheduled)
        {
            if (TileIndex < TileArray.Count)
            {
                for (int i = Cluster; i > 0; i--)
                {
                    TileArray[TileIndex].SetActive(true);
                    TileArray[TileIndex].GetComponent<TileManager>().enabled = true;
                    TileIndex++;
                }
            }
            else
            {
                GameObject lastTile = TileArray[TileArray.Count - 1];
                if (lastTile.GetComponent<TileManager>().enabled == false)
                {
                    TileIndex = 0;
                }
            }
        }
    }
}
*/