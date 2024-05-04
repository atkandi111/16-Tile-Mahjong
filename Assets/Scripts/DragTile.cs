using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface player
public class DragTile : MonoBehaviour
{
    private Camera camera;
    private bool clicked = false, activated = false;
    private float rightBound, leftBound;
    private float longPressTimer = 0f, longPressDuration = 0.25f;
    private Vector3 screenPoint, mouseOffset, hoverOffset = new Vector3(0, +0.1f, -0.05f);
    private Coroutine hoverRef;
    public Quaternion baseRotation;
    public Vector3 basePosition;
    
    public void UpdateBasePosition(Vector3 BasePosition, Quaternion BaseRotation)
    {
        basePosition = BasePosition;
        baseRotation = BaseRotation;
        /*hoverOffset = BaseRotation * new Vector3(0, 0.05f, 0.05f);*/

        hoverOffset = Quaternion.Euler(-90f - BaseRotation.eulerAngles.x, 0, 0) * new Vector3(0, 0.1f, 0.05f);
    }
    void OnEnable()
    {
        camera = Camera.main;
        activated = true;
    }
    void OnDisable()
    {
        activated = false;
    }
    void OnMouseDown()
    {        
        if (activated && SortTile.busySorting != true)
        {
            longPressTimer = 0f;

            screenPoint = camera.WorldToScreenPoint(transform.position);
            mouseOffset = basePosition - camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, screenPoint.z));

            leftBound = GameManager.Players[0].Hand[0].transform.position.x - 0.01f;
            rightBound = GameManager.Players[0].Hand[GameManager.Players[0].Hand.Count - 1].transform.position.x + 0.01f;

            if (isRotatable())
            {
                StartCoroutine(HoverRotate());
            }
            else if (isOpenable())
            {
                GameManager.Players[0].OpenTile(gameObject);
            }
            else if (isTossable())
            {
                GameManager.Players[0].TossTile(gameObject);
            }
            else if (isDraggable())
            {
                clicked = true;
                hoverRef = StartCoroutine(Hover());
            }
        }
    }
    void OnMouseDrag()
    {
        if (activated && clicked)
        {
            Vector3 mousePosition = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, screenPoint.z));
            Vector3 resultPosition = mousePosition + mouseOffset;

            Vector3 position = transform.position;
            position.x = Mathf.Clamp(resultPosition.x, leftBound, rightBound); // change leftBound rightBound to vector
            transform.position = position;

            DragLogic();
        }
    }

    void OnMouseUp()
    {
        if (activated && clicked)
        {
            clicked = false;
            if (longPressTimer < longPressDuration)
            {
                // highlight tile
            }
            else
            {
                
            }
            StopCoroutine(hoverRef);
            StartCoroutine(HoverDown());
        }
    }

    public bool isRotatable()
    {
        if (Input.GetKey(KeyCode.R))
        {
            return true;
        }
        return false;
    }
    public bool isOpenable()
    {
        if (Input.GetKey(KeyCode.O) && gameObject.name[0] == 'f')
        {
            return true;
        }
        return false;
    }
    public bool isTossable()
    {
        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.O) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.T))
        {
            return false;
        }
        if (GameManager.currentPlayer == GameManager.Players[0] && gameObject.name[0] != 'f') // and no tiles moving
        {
            return true;
        }
        return false;
    }
    public bool isDraggable()
    {
        if (Input.GetKey(KeyCode.T) && SortTile.busySorting != true)
        {
            return true;
        }
        return false;
    }

    public void DragLogic()
    {   
        int index = GameManager.Players[0].Hand.IndexOf(gameObject);
        int handCount = GameManager.Players[0].Hand.Count;

        while (index < handCount - 1 && transform.position.x >= GameManager.Players[0].Hand[index + 1].GetComponent<DragTile>().basePosition.x)
        {
            GameObject swapTile = GameManager.Players[0].Hand[index + 1];
            GameManager.Players[0].Hand[index] = swapTile;
            GameManager.Players[0].Hand[index + 1] = gameObject;

            Vector3 temp = swapTile.GetComponent<DragTile>().basePosition;
            swapTile.GetComponent<TileManager>().SetDestination(basePosition, swapTile.transform.rotation, 0.05f);
            basePosition = temp;

            index = index + 1;
        }

        while (0 < index && transform.position.x <= GameManager.Players[0].Hand[index - 1].GetComponent<DragTile>().basePosition.x)
        {
            GameObject swapTile = GameManager.Players[0].Hand[index - 1];
            GameManager.Players[0].Hand[index] = swapTile;
            GameManager.Players[0].Hand[index - 1] = gameObject;

            Vector3 temp = swapTile.GetComponent<DragTile>().basePosition;
            swapTile.GetComponent<TileManager>().SetDestination(basePosition, swapTile.transform.rotation, 0.05f);
            basePosition = temp;

            index = index - 1;
        }
    }

    #region Hover Coroutines
    float hoverDuration = 0.15f;
    float rotateDuration = 0.25f;

    public IEnumerator Hover()
    {
        float secondsTravelled = 0f;
        while (secondsTravelled < hoverDuration)
        {
            Vector3 position = transform.position;
            position.y = Mathf.Lerp(basePosition.y, basePosition.y + hoverOffset.y, secondsTravelled / hoverDuration);
            position.z = Mathf.Lerp(basePosition.z, basePosition.z + hoverOffset.z, secondsTravelled / hoverDuration); 
            transform.position = position;

            secondsTravelled += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator HoverDown()
    {
        float secondsTravelled = 0f;
        while (secondsTravelled < hoverDuration)
        {
            transform.position = Vector3.Lerp(basePosition + hoverOffset, basePosition, secondsTravelled / hoverDuration);

            secondsTravelled += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator HoverRotate()
    {
        StartCoroutine(Hover());
        yield return new WaitForSeconds(hoverDuration);

        Quaternion startRot = baseRotation;
        Quaternion finalRot = baseRotation * Quaternion.Euler(0, 0, 180);
        
        baseRotation = finalRot;

        float secondsTravelled = 0f;
        while (secondsTravelled < rotateDuration)
        {
            transform.rotation = Quaternion.Lerp(startRot, finalRot, secondsTravelled / 0.25f);
            
            secondsTravelled += Time.deltaTime;
            yield return null;
        }

        transform.rotation = baseRotation;

        StartCoroutine(HoverDown());
    }
    #endregion
}


// what if busySorting will just disable (deactivate) dragTile
// changed (activated && clicked) to just (clicked)
// 


// should only work when currentPlayer = Player[0]