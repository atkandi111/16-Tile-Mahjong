using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour // change to MoveTile // add HoverTile, SelectTile
{
    private Vector3 startPos, finalPos;
    private Quaternion startRot, finalRot;
    private float lerpFactor, velocity, duration;

    void Awake()
    {
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = false;
        rigidbody.useGravity = false;
        rigidbody.drag = 15f;
        rigidbody.angularDrag = 15f;
    }
    public (Vector3, Quaternion) GetDestination()
    {
        return (finalPos, finalRot);
    }
    public void SetDestination(Vector3? position = null, Quaternion? rotation = null, float smoothTime = 0f)
    {
        startPos = transform.position;
        startRot = transform.rotation;
        finalPos = position ?? transform.position;
        finalRot = rotation ?? transform.rotation;

        lerpFactor = 0f;
        velocity = 0f;
        duration = smoothTime;
        enabled = true;

        gameObject.GetComponent<DragTile>().UpdateBasePosition(finalPos, finalRot);
    }

    public IEnumerator MoveTile(float smoothTime, Vector3? position = null, Quaternion? rotation = null)
    {
        if (position == null && rotation == null)
        {
            throw new InvalidOperationException("Empty destination.");
        }

        Vector3 startPos = transform.position;
        Vector3 finalPos = position ?? transform.position;

        Quaternion startRot = transform.rotation;
        Quaternion finalRot = rotation ?? transform.rotation;

        float velocity = 0f;
        float lerpFactor = 0f;

        while (lerpFactor < 0.999f)
        {
            lerpFactor = Mathf.SmoothDamp(lerpFactor, 1f, ref velocity, smoothTime);

            transform.position = Vector3.Lerp(startPos, finalPos, lerpFactor);
            transform.rotation = Quaternion.Lerp(startRot, finalRot, lerpFactor);

            yield return null;
        }

        transform.position = finalPos;
        transform.rotation = finalRot;
    }
    
    void Update() // change to coroutine
    {
        lerpFactor = Mathf.SmoothDamp(lerpFactor, 1f, ref velocity, duration);
        
        transform.position = Vector3.Lerp(startPos, finalPos, lerpFactor);
        transform.rotation = Quaternion.Lerp(startRot, finalRot, lerpFactor);
        
        // if (transform.position == finalPos && transform.rotation == finalRot)
        // if (lerpFactor == 1f) // if (finalPos - transform.position < 0.2f)
        if (lerpFactor > 0.999f)
        {
            transform.position = finalPos;
            transform.rotation = finalRot;
            enabled = false;
        }
    }
}

// change update to coroutine
// relatively unnoticeable overhead
// coroutine can be stopped unlike update which needs boolean flag

// transfer baseposition, baserotatioin to tilemanager