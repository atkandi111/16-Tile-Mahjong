using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragToss : MonoBehaviour
{
    private GameObject draggedObject;

    Plane plane = new Plane(Vector3.up, Vector3.zero); // 0 instead of v3.zero
    private Camera camera;
    private Vector3 dragOffset;


    void Start()
    {
        camera = Camera.main;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && GameManager.TileToss.Contains(hit.collider.gameObject))
            {
                draggedObject = hit.collider.gameObject;
                dragOffset = hit.point - draggedObject.transform.position; // convert to rb position
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            draggedObject = null;
        }

        if (draggedObject != null)
        {
            float distance;
            float boundary = Perimeter.WallArea - GameManager.tileOffset.y;
            Vector3 mousePosition = new Vector3();
            Vector3 currentPosition;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Rigidbody rigidbody = draggedObject.GetComponent<Rigidbody>();

            if (plane.Raycast(ray, out distance))
            {
                mousePosition = ray.GetPoint(distance);
                if (mousePosition.x <= -boundary || mousePosition.x >= +boundary)
                {
                    if (rigidbody.position.x <= -boundary || rigidbody.position.x >= +boundary)
                    {
                        mousePosition.x = Mathf.Clamp(mousePosition.x, -boundary, +boundary);
                    }
                }
                
                if (mousePosition.z <= -boundary || mousePosition.z >= +boundary)
                {
                    if (rigidbody.position.z <= -boundary || rigidbody.position.z >= +boundary)
                    {
                        mousePosition.z = Mathf.Clamp(mousePosition.z, -boundary, +boundary);
                    }
                }
            }

            mousePosition = new Vector3(mousePosition.x, GameManager.tileOffset.z, mousePosition.z);

            rigidbody.velocity = (mousePosition - rigidbody.position) * 0.1f / Time.deltaTime;
            
            currentPosition = rigidbody.position;
            currentPosition.x = Mathf.Clamp(currentPosition.x, -boundary, +boundary);
            currentPosition.z = Mathf.Clamp(currentPosition.z, -boundary, +boundary);
            rigidbody.MovePosition(currentPosition);

            CheckCollision();
            
            
            /*float distance;
            Vector3 mousePosition = new Vector3();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out distance))
            {
                mousePosition = ray.GetPoint(distance);
            }

            mousePosition = new Vector3(mousePosition.x, GameManager.tileOffset.z, mousePosition.z);

            Vector3 currentVelocity = (mousePosition - draggedObject.GetComponent<Rigidbody>().position) / Time.deltaTime;
            draggedObject.GetComponent<Rigidbody>().MovePosition(draggedObject.GetComponent<Rigidbody>().position + currentVelocity * Time.deltaTime);  

            */
            // yyCheckCollision(); 
        }
    }

    private void CheckCollision()
    {
        BoxCollider boxCollider = draggedObject.GetComponent<BoxCollider>();

        Collider[] colliders = Physics.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.extents);
        foreach (Collider collider in colliders)
        {
            if (GameManager.TileToss.Contains(collider.gameObject) && collider.gameObject != draggedObject)
            {
                float boundary = Perimeter.WallArea - GameManager.tileOffset.y;
                Rigidbody rigidbody = collider.gameObject.GetComponent<Rigidbody>();
                Vector3 currentPosition = rigidbody.position;
                currentPosition.x = Mathf.Clamp(currentPosition.x, -boundary, +boundary);
                currentPosition.z = Mathf.Clamp(currentPosition.z, -boundary, +boundary);
                rigidbody.MovePosition(currentPosition);
                // if force > xxxx || velocity > xxxxx, do sound
                /*Debug.Log(collider.gameObject.name);
                AudioManager.Instance.soundCollision();*/
            }
        }
    }
    private void yyCheckCollision()
    {
        BoxCollider boxCollider = draggedObject.GetComponent<BoxCollider>();

        Collider[] colliders = Physics.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.extents);
        foreach (Collider collider in colliders)
        {
            if (GameManager.TileToss.Contains(collider.gameObject))
            {
                Vector3 forceDirection = collider.gameObject.transform.position - draggedObject.transform.position;
                forceDirection.Normalize();
                forceDirection.y = 0;

                Rigidbody collidedRigidbody = collider.gameObject.GetComponent<Rigidbody>();

                collidedRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
                // collidedRigidbody.AddForce(forceDirection * 0.1f, ForceMode.Impulse);
            }
        }
    }
    private void xxCheckCollision()
    {
        foreach (GameObject tile in GameManager.TileToss)
        {
            BoxCollider boxCollider = tile.GetComponent<BoxCollider>();

            Collider[] colliders = Physics.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.extents);
            foreach (Collider collider in colliders)
            {
                if (GameManager.TileToss.Contains(collider.gameObject) && collider.gameObject != tile)
                {
                    Vector3 forceDirection = collider.gameObject.transform.position - tile.transform.position;
                    forceDirection.Normalize();
                    forceDirection.y = 0;

                    Rigidbody colRigidbody = collider.gameObject.GetComponent<Rigidbody>();

                    colRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
                    colRigidbody.AddForce(forceDirection * 0.1f, ForceMode.Impulse);
                }
            }
        }
    }
}
public class xxDragToss : MonoBehaviour
{
    Plane plane = new Plane(Vector3.up, 0);
    private Camera camera;
    private Rigidbody rigidbody;

    void Start()
    {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {

    }

    void OnMouseDrag()
    {
        float distance, maxSpeed = 10f;
        Vector3 mousePosition = new Vector3();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            mousePosition = ray.GetPoint(distance);
        }

        mousePosition = new Vector3(mousePosition.x, GameManager.tileOffset.z, mousePosition.z);

        Vector3 currentVelocity = (mousePosition - rigidbody.position) / Time.deltaTime;
        currentVelocity = Vector3.ClampMagnitude(currentVelocity, maxSpeed);

        rigidbody.MovePosition(rigidbody.position + currentVelocity * Time.deltaTime);
        // rigidbody.MovePosition(mousePosition);
    }

    void OnCollisionEnter(Collision collision)
    {
        // it works but since no velocity, only moves very little

        // addforce, increase 0.1f

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


// what if only set rigidbody once reached tilearea

// SOLUTION: ADD INVISIBLE BOUNDARY PLANE