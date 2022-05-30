using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotProduct : MonoBehaviour
{
    private Vector3 WallNormal;
    public float dot;
    public GameObject wall;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        WallNormal = getWallNormal();
        dot = Vector3.Dot(WallNormal, transform.forward);
    }

    private Vector3 getWallNormal()
    {
        if (Physics.Raycast(transform.position, transform.forward * 10 + transform.up * 2, out RaycastHit hit, 15, 1 << 3))
        {
            Debug.DrawRay(transform.position, transform.forward * 10 + transform.up * 2, Color.red);
            wall = hit.transform.gameObject;
            return -hit.transform.forward;
        } else
        {
            Debug.DrawRay(transform.position, transform.forward * 10 + transform.up * 2, Color.green);
            return transform.right;
        }
    }
}
