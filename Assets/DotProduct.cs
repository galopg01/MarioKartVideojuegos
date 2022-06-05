using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotProduct : MonoBehaviour
{
    private Vector3 WallNormal;
    public float dot;
    private float dotAux;
    public GameObject wall;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        WallNormal = getWallNormal();

        if (Vector3.Dot(WallNormal, transform.forward) > -0.01 && Vector3.Dot(WallNormal, transform.forward) < 0.01)
        {
            dotAux = 0;
        }
        else
        {
            dotAux = Vector3.Dot(WallNormal, transform.forward);
        }

        Debug.DrawRay(transform.position, transform.right * 10, Color.green);
        Debug.DrawRay(transform.position, -transform.right * 10, Color.green);
        if (dotAux <= 0)
        {
            if (Physics.Raycast(transform.position, transform.right * 10, out RaycastHit hit, 14, 1 << 3))
            {
                dot = Vector3.Dot(-hit.transform.forward, transform.right) > 0 ? dotAux : -2 - dotAux;
            }
            else
            {
                dot = dotAux;
            }
        }
        else if (dotAux > 0)
        {
            if (Physics.Raycast(transform.position, -transform.right * 10, out RaycastHit hit, 14, 1 << 3))
            {
                dot = Vector3.Dot(-hit.transform.forward, -transform.right) < 0 ? dotAux : 2 - dotAux;
            }
            else
            {
                dot = dotAux;
            }
        }
    }

    private Vector3 getWallNormal()
    {
        if (Physics.Raycast(transform.position, transform.forward * 10, out RaycastHit hit, 10, 1 << 3))
        {
            Debug.DrawRay(transform.position, transform.forward * 10 + transform.up * 2, Color.red);
            wall = hit.transform.gameObject;
            return -hit.transform.forward;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * 10, Color.green);
            return transform.right;
        }
    }
}