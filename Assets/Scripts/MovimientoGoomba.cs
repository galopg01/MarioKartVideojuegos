using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoGoomba : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;
    float power=(float)0.5;
    public float speed = 2;
    Vector3 fuerza;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fuerza = Vector3.forward * speed;
        rb.velocity = new Vector3(0, 0, power);
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.z < speed)
        {
            rb.AddForce(fuerza);
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
    }
}
