using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class TossTiler : MonoBehaviour // transfer to positioner
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

    public static (Vector3, Quaternion) ComputeTossPosition()
    {
        centerPoint = new Vector3(0, GameManager.tileOffset.z, 0);
        Bounds bounds = GameManager.TileSet[0].GetComponent<MeshRenderer>().bounds;
        Vector3 colliderBounds = new Vector3(bounds.extents.x, bounds.extents.z, bounds.extents.y);

        circleRadius = 0f;
        while (circleRadius < GameManager.tileSize.x * 10 * Mathf.Sqrt(2))
        {
            int randomAngle = Random.Range(0, 360);
            for (int i = 0; i < 360; i += 10)
            {
                float radian = (randomAngle + i) * Mathf.Deg2Rad;

                float x = Mathf.Cos(radian) * circleRadius;
                float z = Mathf.Sin(radian) * circleRadius;

                Vector3 position = centerPoint + new Vector3(x, 0, z);
                int minColliders = Physics.OverlapSphereNonAlloc(position, GameManager.tileOffset.x, new Collider[6]);
                if (minColliders > 0)
                {
                    continue;
                }

                for (int j = 0; j < 360; j += 10)
                {
                    Quaternion rotation = Quaternion.Euler(Positioner.FaceUp, randomAngle + j, 0);
                    int trueColliders = Physics.OverlapBoxNonAlloc(position, colliderBounds, new Collider[6], rotation);
                    if (trueColliders > 0)
                    {
                        continue;
                    }

                    return (position, rotation);
                }    
            }

            circleRadius += GameManager.tileSize.y;
        }

        Debug.Log("throw exception hereeeeeee");
        return (centerPoint, Quaternion.Euler(Positioner.FaceUp, 0, 0));
    }
}
