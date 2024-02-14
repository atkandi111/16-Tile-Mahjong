using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class TileManager : MonoBehaviour // change to MoveTile // add HoverTile, SelectTile
{
    [System.Serializable]
    public class DestinationInfo
    {
        public (Vector3, Quaternion) destination;
        public float lerpDuration;
    }

    private Queue<DestinationInfo> destQueue = new Queue<DestinationInfo>();
    public DestinationInfo currentDestination; //set to private
    private Vector3 startPos, finalPos;
    private Quaternion startRot, finalRot;
    private float secondsTravelled, lerpFactor;

    public (Vector3, Quaternion) GetDestination()
    {
        return currentDestination.destination;
    }

    public void AddDestination(Vector3 position, Quaternion rotation, float LerpDuration)
    {
        DestinationInfo newDestination = new DestinationInfo
        {
            destination = (position, rotation),
            lerpDuration = LerpDuration
            //secondsTravelled = 0
        };
        destQueue.Enqueue(newDestination);

        currentDestination = destQueue.Peek();
    }

    public void xxChangeDestination(Vector3 position, Quaternion rotation, float LerpDuration)
    {
        DestinationInfo newDestination = new DestinationInfo
        {
            destination = (position, rotation),
            lerpDuration = LerpDuration
        };

        if (currentDestination == null)
        {
            destQueue.Enqueue(newDestination);
        }

        currentDestination = newDestination;
        secondsTravelled = 0f;
    }

    public void yyChangeDestination(Vector3 position, Quaternion rotation)
    {
        currentDestination.destination = (position, rotation);
        secondsTravelled = 0f;
    }

    public void ChangeDestination(Vector3 position, Quaternion rotation, float LerpDuration)
    {
        ClearDestination();
        AddDestination(position, rotation, LerpDuration); // mali since it adds to the end of the queue, go to yy
    }

    public void ClearDestination()
    {
        if (destQueue.Count > 0) // can be removed na
        { destQueue.Dequeue(); } 

        if (destQueue.Count > 0)
        { currentDestination = destQueue.Peek(); }
        else
        { currentDestination = null; }
    }

    void Update()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        finalPos = currentDestination.destination.Item1;
        finalRot = currentDestination.destination.Item2;
        
        secondsTravelled += Time.deltaTime; // try stopwatch and Time.time
        lerpFactor = secondsTravelled / currentDestination.lerpDuration;

        transform.position = Vector3.Lerp(startPos, finalPos, lerpFactor);
        transform.rotation = Quaternion.Lerp(startRot, finalRot, lerpFactor);

        if (transform.position == finalPos)// && transform.rotation == finalRot)
        {
            transform.position = finalPos;
            transform.rotation = finalRot;
            secondsTravelled = 0;
            ClearDestination();
            enabled = false;
        }
    }
}*/

public class TileManager : MonoBehaviour // change to MoveTile // add HoverTile, SelectTile
{
    private Vector3 currentPos, targetPos;
    private Quaternion currentRot, targetRot;
    private float lerpDuration, lerpFactor, secondsTravelled;

    public (Vector3, Quaternion) GetDestination()
    {
        return (targetPos, targetRot);
    }
    public void SetDestination(Vector3 position, Quaternion rotation, float LerpDuration = 3f)
    {
        currentPos = transform.position;
        currentRot = transform.rotation;
        targetPos = position;
        targetRot = rotation;

        lerpDuration = LerpDuration;
        secondsTravelled = 0f;
        enabled = true;
    }
    void Awake()
    {
        enabled = false;
    }
    void Update()
    {
        /*
        startPos = transform.position;
        startRot = transform.rotation;
        finalPos = currentDestination.destination.Item1;
        finalRot = currentDestination.destination.Item2;
        */

        currentPos = transform.position;
        currentRot = transform.rotation;
        secondsTravelled += Time.deltaTime;
        lerpFactor = secondsTravelled / lerpDuration;

        transform.position = Vector3.Lerp(currentPos, targetPos, lerpFactor);
        transform.rotation = Quaternion.Lerp(currentRot, targetRot * Quaternion.Euler(0, -1, 0), lerpFactor);

        if (transform.position == targetPos)// && transform.rotation == finalRot)
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
            enabled = false;
        }
    }
}