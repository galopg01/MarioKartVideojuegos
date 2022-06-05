using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeRotation : MonoBehaviour
{
    Rigidbody rb;
    WheelController wheelController;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheelController = GetComponent<WheelController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag.Equals("Rampa") && transform.position.z > -20)
        {
            wheelController.maxSpeed = 9;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.AddForce(transform.forward * 230, ForceMode.VelocityChange);
        }
        else if (transform.position.z > -20)
        {
            rb.freezeRotation = false;
            if (!collision.transform.tag.Equals("Rampita")) {
                wheelController.maxSpeed = 7;
            }
        }
    }
}
