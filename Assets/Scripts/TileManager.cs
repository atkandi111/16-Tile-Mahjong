using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileManager : MonoBehaviour // change to MoveTile // add HoverTile, SelectTile
{
    public Vector3 startPos, finalPos;
    public Quaternion startRot, finalRot;
    private float lerpFactor, velocity, duration;

    public Vector3 basePosition;
    public Quaternion baseRotation;

    public delegate void OnMovementStopped();
    public event OnMovementStopped OnStop;

    void Awake()
    {
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        // rigidbody.mass = 1000;
        rigidbody.isKinematic = false;
        rigidbody.useGravity = false;
        rigidbody.drag = 15f;
        rigidbody.angularDrag = 15f;
    }

    void OnDisable()
    {
        OnStop?.Invoke();
    }

    public (Vector3, Quaternion) GetDestination()
    {
        return (finalPos, finalRot);
    }

    /*
    void OnEnable()
    {
        lerpFactor = 0f;
        velocity = 0f;
    }
    */

    public void UpdateBase(Vector3 BasePosition, Quaternion BaseRotation)
    {
        basePosition = BasePosition;
        baseRotation = BaseRotation;
    }

    public void SetDestination(Vector3 position, Quaternion rotation, float smoothTime)
    {
        startPos = transform.position;
        startRot = transform.rotation;
        finalPos = position;
        finalRot = rotation;

        lerpFactor = 0f;
        velocity = 0f;
        duration = smoothTime;

        // tilesMoving += 1;
        enabled = true;

        gameObject.GetComponent<DragTile>().UpdateBasePosition(finalPos, finalRot);
    }

    void Update()
    {
        lerpFactor = Mathf.SmoothDamp(lerpFactor, 1f, ref velocity, duration);
        
        transform.position = Vector3.Lerp(startPos, finalPos, lerpFactor);
        transform.rotation = Quaternion.Lerp(startRot, finalRot, lerpFactor);
        
        if (lerpFactor > 0.999f)
        {
            transform.position = finalPos;
            transform.rotation = finalRot;

            enabled = false;
        }
    }
}

// transfer baseposition, baserotatioin to tilemanager