using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface player
public class DragTile : MonoBehaviour
{
    GameObject swapTile; 
    private bool leftClicked = false, rightClicked = false, activated = false;
    private float rightBound, leftBound;
    private float longPressTimer = 0f, longPressDuration = 0.25f;
    public Vector3 screenPoint, mouseOffset, hoverOffset = new Vector3(0, +0.06f, -0.05f);
    private Quaternion baseRotation;
    private Coroutine hoverRef;
    public Vector3 basePosition;
    public int index, handCount;
    public void UpdateBasePosition(Vector3 BasePosition)
    {
        handCount = GameManager.Players[0].Hand.Count;
        basePosition = BasePosition;
        baseRotation = transform.rotation;
    }
    void OnEnable()
    {
        UpdateBasePosition(transform.position);
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
            index = GameManager.Players[0].Hand.IndexOf(gameObject);
            longPressTimer = 0f;

            screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            mouseOffset = basePosition - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, screenPoint.z));

            leftBound = GameManager.Players[0].Hand[0].transform.position.x - 0.01f;
            rightBound = GameManager.Players[0].Hand[GameManager.Players[0].Hand.Count - 1].transform.position.x + 0.01f;

            if (Input.GetKey(KeyCode.R))
            {
                StartCoroutine(HoverRotate());
            }
            else
            {
                leftClicked = true;
                // hoverRef = StartCoroutine(Hover());
            }

        }
    }
    void OnMouseDrag()
    {
        if (activated && leftClicked)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, screenPoint.z));
            Vector3 resultPosition = mousePosition + mouseOffset;

            Vector3 position = transform.position;
            position.x = Mathf.Clamp(resultPosition.x, leftBound, rightBound);

            if (longPressTimer < longPressDuration)
            {
                position.y = Mathf.Lerp(basePosition.y, basePosition.y + hoverOffset.y, longPressTimer / longPressDuration);
                position.z = Mathf.Lerp(basePosition.z, basePosition.z + hoverOffset.z, longPressTimer / longPressDuration);

                longPressTimer += Time.deltaTime;
            }

            transform.position = position;

            DragLogic();
        }
    }

    void OnMouseUp()
    {
        /*if (activated && (leftClicked || rightClicked))
        {
            if (leftClicked && longPressTimer < longPressDuration)
            {
                // throw tile
                StopCoroutine(hoverRef);
                GameManager.Players[0].OpenTile(gameObject);
            }
            else
            {
                // return to basePosition
                StopCoroutine(hoverRef);
                GetComponent<TileManager>().SetDestination(basePosition, baseRotation, 0.05f);
                // PositionManager.ScheduleEvent(0.05f, 1, new List<GameObject> { gameObject });
            }
        }*/

        if (activated && leftClicked)
        {
            // StopCoroutine(hoverRef);
            if (longPressTimer < longPressDuration)
            {
                GameManager.Players[0].OpenTile(gameObject);
            }
            else
            {
                GetComponent<TileManager>().SetDestination(basePosition, baseRotation, 0.05f);
            }
        }

        leftClicked = false;
        rightClicked = false;
    }

    public void DragLogic()
    {   
        index = GameManager.Players[0].Hand.IndexOf(gameObject);
        while (index < handCount - 1 && transform.position.x >= GameManager.Players[0].Hand[index + 1].GetComponent<DragTile>().basePosition.x)
        {
            swapTile = GameManager.Players[0].Hand[index + 1];
            GameManager.Players[0].Hand[index] = swapTile;
            GameManager.Players[0].Hand[index + 1] = gameObject;

            swapTile.GetComponent<TileManager>().SetDestination(basePosition, swapTile.transform.rotation, 0.05f);

            Vector3 temp = swapTile.GetComponent<DragTile>().basePosition;
            swapTile.GetComponent<DragTile>().basePosition = basePosition;
            basePosition = temp;

            index = index + 1;
        }

        while (0 < index && transform.position.x <= GameManager.Players[0].Hand[index - 1].GetComponent<DragTile>().basePosition.x)
        {
            swapTile = GameManager.Players[0].Hand[index - 1];
            GameManager.Players[0].Hand[index] = swapTile;
            GameManager.Players[0].Hand[index - 1] = gameObject;

            swapTile.GetComponent<TileManager>().SetDestination(basePosition, swapTile.transform.rotation, 0.05f);

            Vector3 temp = swapTile.GetComponent<DragTile>().basePosition;
            swapTile.GetComponent<DragTile>().basePosition = basePosition;
            basePosition = temp;

            index = index - 1;
        }
    }

    IEnumerator Hover()
    {
        float secondsTravelled = 0f;
        while (secondsTravelled < 0.25f) // change to if secondsTravelled < 0.25f or LerpFactor < 1
        {
            Vector3 position = transform.position;
            position.y = Mathf.Lerp(basePosition.y, basePosition.y + hoverOffset.y, secondsTravelled / 0.25f);
            position.z = Mathf.Lerp(basePosition.z, basePosition.z + hoverOffset.z, secondsTravelled / 0.25f);
            transform.position = position;

            secondsTravelled += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator HoverRotate()
    {
        float secondsTravelled = 0f;
        Quaternion startRot = baseRotation;
        Quaternion finalRot = baseRotation * Quaternion.Euler(0, 0, 180);
        
        baseRotation = finalRot;

        while (secondsTravelled < 0.25f)
        {
            Vector3 position = transform.position;
            position.y = Mathf.Lerp(basePosition.y, basePosition.y + hoverOffset.y, secondsTravelled / 0.25f);
            position.z = Mathf.Lerp(basePosition.z, basePosition.z + hoverOffset.z, secondsTravelled / 0.25f);
            transform.position = position;

            transform.rotation = Quaternion.Lerp(startRot, finalRot, secondsTravelled / 0.25f);
            
            secondsTravelled += Time.deltaTime;
            yield return null;
        }

        GetComponent<TileManager>().SetDestination(basePosition, baseRotation, 0.05f);

        /*
        hoverrotate should be:
        1. hoverup
        2. rotate
        3. hoverdown

        not:
        1. hoverup and rotate
        2. hoverdown
        */
    }
}