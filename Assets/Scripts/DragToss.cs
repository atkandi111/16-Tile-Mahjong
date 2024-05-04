using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragToss : MonoBehaviour
{
    Plane plane = new Plane(Vector3.up, 0);
    private Camera camera;
    private Rigidbody rigidbody;

    void Start()
    {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody>();
    }

    void OnMouseDown()
    {
    }

    void OnMouseDrag()
    {
        float distance;
        Vector3 mousePosition = new Vector3();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            mousePosition = ray.GetPoint(distance);
        }

        mousePosition = new Vector3(mousePosition.x, GameManager.tileOffset.z, mousePosition.z);
        rigidbody.MovePosition(mousePosition);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Apply force when colliding with other objects
        foreach (ContactPoint contact in collision.contacts)
        {
            //Vector3 forceDirection = contact.point - transform.position;
            Vector3 forceDirection = new Vector3(collision.contacts[0].normal.x, 0, collision.contacts[0].normal.z).normalized;
            Rigidbody colRigidbody = collision.gameObject.GetComponent<Rigidbody>();

            colRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
            colRigidbody.AddForce(forceDirection * 0.1f, ForceMode.Impulse);
        }
    }
}
