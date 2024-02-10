using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveerTile : MonoBehaviour
{
    private Color onColor = Color.grey;
    private Color offColor = Color.white;
}
public class HoverTile : MonoBehaviour
{
    private BoxCollider boxCollider;
    private Vector3 hoverHeight = new Vector3(0, 0.05f, 0);
    void Awake()
    {
        boxCollider = gameObject.AddComponent<BoxCollider>(); // was get
        boxCollider.isTrigger = true;

        //boxCollider.size = boxCollider.transform.InverseTransformVector(GameManager.tileSize + hoverHeight);
        //boxCollider.center = boxCollider.transform.InverseTransformPoint(transform.position - (hoverHeight / 2));
    }
    void OnMouseEnter()
    {
        //transform.GetChild(0).transform.position += hoverHeight;
    }
    void OnMouseExit()
    {
        //transform.GetChild(0).transform.position -= hoverHeight;
    }
    public void RotateHoverOffset()
    {
        boxCollider.center -= boxCollider.transform.InverseTransformPoint(hoverHeight);
    }
}

public class SelectTile : MonoBehaviour
{
    void Update()
    {

    }
}

// create HumanPlayer Script and add to it HoverTile and RotateTile