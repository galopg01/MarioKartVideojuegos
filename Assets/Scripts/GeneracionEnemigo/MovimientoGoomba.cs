using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoGoomba : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;
    private float force;
    private float limSpeed;
    Vector3 fuerza;
    Quaternion orientacion;
    public bool isGrounded;
    private float alturaRelativa;
    private float fuerzaLevitacion;
    private float rapidezVertical = 3f;
    private float alturaDeseada = 0.5f;
    void Start()
    {
        force = (float)2;
        limSpeed = 1;
        isGrounded = false;
        rb = GetComponent<Rigidbody>();
        orientacion = transform.rotation;
        fuerza = transform.forward * force;
        fuerzaLevitacion = -(rb.mass * Physics.gravity.y);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        alturaRelativa = calcularAltura();
        //print(alturaRelativa);
        float velocidad = Mathf.Sqrt( Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2));
        if (velocidad < limSpeed)
        {
            //print("ENTRA");
            rb.AddForce(fuerza);
        }

        rb.rotation = new Quaternion(orientacion.x, transform.rotation.y, orientacion.z,0).normalized;

        float distancia = alturaDeseada - alturaRelativa;
        if (rb.velocity.y >= 0f)
        {
            float factor = distancia * rapidezVertical;
            rb.AddForce(transform.up * fuerzaLevitacion * factor);
        }
        else
        {
            float factor = Mathf.Max(0, distancia * rapidezVertical * 5 );
            rb.AddForce(Vector3.up * fuerzaLevitacion * factor);
        }
        
    }

    private float calcularAltura()
    {
        RaycastHit hit;
        float distancia = 0;
        if (Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity))
        {
            distancia = hit.distance;
            orientacion = Quaternion.EulerAngles(hit.normal);
            
        }
        
        return distancia;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.transform.tag == "Suelo")
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Suelo")
        {
            isGrounded = true;
        }else if(collision.transform.tag == "Pared")
        {
            Destroy(this);
        }
        
    }
}
