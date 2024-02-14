using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragTile : MonoBehaviour
{
    GameObject swapTile; 
    private bool clicked = false, activated = false;
    private float rightBound, leftBound;
    private float longPressTimer = 0f, longPressDuration = 0.25f;
    private Vector3 screenPoint, mouseOffset, hoverOffset = new Vector3(0, +0.06f, -0.05f);
    private Quaternion baseRotation;
    public Vector3 basePosition;
    public int index, handCount;
    public void UpdateBasePosition(Vector3 BasePosition)
    {
        handCount = GameManager.Players[0].Hand.Count;
        basePosition = BasePosition;
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
        //if (GameManager.noRunningSchedules == true)
        if (activated)
        {
            index = GameManager.Players[0].Hand.IndexOf(gameObject);
            longPressTimer = 0f;
            clicked = true;


            screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            mouseOffset = basePosition - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, screenPoint.z));

            rightBound = ((handCount / 2) * GameManager.tileSize.x - GameManager.tileOffset.x);
            leftBound = -((handCount / 2) * GameManager.tileSize.x - GameManager.tileOffset.x);

            if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl))
            {
                baseRotation = transform.rotation * Quaternion.Euler(0, 0, 180);
            }
            else
            {
                baseRotation = transform.rotation;
            }

            //GetComponent<TileManager>().AddDestination(basePosition + hoverOffset, baseRotation, 1);

            //GetComponent<TileManager>().enabled = true;
            //PositionManager.ScheduleEvent(0.05f, 1, new List<GameObject> { gameObject });
            StartCoroutine(Hover());
        }
    }
    void OnMouseDrag()
    {
        if (activated && clicked)
        {
            if (longPressTimer < longPressDuration)
            {
                longPressTimer += Time.deltaTime;
                return;
            }
            
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, screenPoint.z));
            Vector3 resultPosition = mousePosition + mouseOffset;
            float boundedPosition = Mathf.Clamp(resultPosition.x, leftBound, rightBound);
            transform.position = new Vector3(boundedPosition, transform.position.y, transform.position.z);

            while (index < handCount - 1 && transform.position.x > GameManager.Players[0].Hand[index + 1].GetComponent<DragTile>().basePosition.x)
            {
                swapTile = GameManager.Players[0].Hand[index + 1];
                GameManager.Players[0].Hand[index] = swapTile;
                GameManager.Players[0].Hand[index + 1] = gameObject;

                swapTile.GetComponent<TileManager>().SetDestination(basePosition, swapTile.transform.rotation, 0.05f);
                // swapTile.GetComponent<TileManager>().enabled = true;

                Vector3 temp = swapTile.GetComponent<DragTile>().basePosition;
                swapTile.GetComponent<DragTile>().basePosition = basePosition;
                basePosition = temp;

                index = index + 1;
            }

            while (0 < index && transform.position.x < GameManager.Players[0].Hand[index - 1].GetComponent<DragTile>().basePosition.x)
            {
                swapTile = GameManager.Players[0].Hand[index - 1];
                GameManager.Players[0].Hand[index] = swapTile;
                GameManager.Players[0].Hand[index - 1] = gameObject;

                swapTile.GetComponent<TileManager>().SetDestination(basePosition, swapTile.transform.rotation, 0.05f);
                // swapTile.GetComponent<TileManager>().enabled = true;

                Vector3 temp = swapTile.GetComponent<DragTile>().basePosition;
                swapTile.GetComponent<DragTile>().basePosition = basePosition;
                basePosition = temp;

                index = index - 1;

                //GameManager.noRunningSchedules = false;
            }
        }
    }

    void OnMouseUp()
    {
        if (activated && clicked)
        {
            if (longPressTimer < longPressDuration) // and if cmd is not pressed
            {
                // throw tile
                GameManager.Players[0].OpenTile(gameObject);
            }
            else
            {
                // return to basePosition
                GetComponent<TileManager>().SetDestination(basePosition, baseRotation, 1);
                // PositionManager.ScheduleEvent(0.05f, 1, new List<GameObject> { gameObject });
            }
        }

        clicked = false;
    }

    IEnumerator Hover()
    {
        float elapsedTime = 0f;

        while (elapsedTime < 0.25f)
        {
            transform.position = Vector3.Lerp(basePosition, basePosition + hoverOffset, elapsedTime / 0.25f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    void Update()
    {

    }
}


// enable every time hand length changes