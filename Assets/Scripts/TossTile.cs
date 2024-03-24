using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class TossTiler : MonoBehaviour
{
    private Vector3[] tossArea = new Vector3[4];
    private static Vector3 centerPoint;
    private static float circleRadius = 0f;

    void GenerateTossArea()
    {
        // if TileHands.Length > 60, then if TileHands.Length > 40, then > 20
        tossArea[0] = new Vector3(-2.2f, 0, 0f);
        tossArea[1] = new Vector3(+2.2f, 0, 0f);
        tossArea[2] = new Vector3(+2.2f, 0, +2.2f);
        tossArea[3] = new Vector3(-2.2f, 0, +2.2f);
    }

    public static (Vector3, Quaternion) RandomTossPosition(int trial = 0)
    {
        if (trial == 2)
        {
            Debug.Log("layer 2");
        }
        if (trial > 2)
        {
            Debug.Log("throw exception hereeeeeee");
            return (centerPoint, Quaternion.Euler((int) GameManager.FacePreset.Opened, 0, 0));
        }
        centerPoint = new Vector3(0, GameManager.tileOffset.z, 0);

        int randomAngle = Random.Range(0, 360);
        Bounds bounds = GameManager.TileSet[0].GetComponent<MeshRenderer>().bounds;
        for (int i = 0; i < 360; i += 10)
        {
            float radian = (randomAngle + i) * Mathf.Deg2Rad;

            float x = Mathf.Cos(radian) * circleRadius;
            float z = Mathf.Sin(radian) * circleRadius;

            Vector3 position = centerPoint + new Vector3(x, 0, z);

            for (int j = 0; j < 360; j += 10)
            {
                Quaternion rotation = Quaternion.Euler((int) GameManager.FacePreset.Opened, randomAngle + j, 0);
                Collider[] colliders = Physics.OverlapBox(position, new Vector3(bounds.extents.x, bounds.extents.z, bounds.extents.y), rotation);
                if (colliders.Length == 0)
                {
                    return (position, rotation);
                }
            }
        }
        circleRadius = ComputeMinimumCircle() + GameManager.tileSize.y;
        return RandomTossPosition(trial + 1);
    }

    static float ComputeMinimumCircle()
    {
        float maxDistance = 0f;
        foreach (GameObject tile in GameManager.TileToss)
        {
            float distance = Vector3.Distance(centerPoint, tile.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }
        return maxDistance;
    }
}
