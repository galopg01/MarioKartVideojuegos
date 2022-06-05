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
    private float alturaRelativa;
    private float fuerzaLevitacion;
    private float rapidezVertical = 3f;
    private float alturaDeseada = 0.5f;
    void Start()
    {
        force = (float)1.5;
        limSpeed = 2;
        rb = GetComponent<Rigidbody>();
        fuerza = transform.forward * force;
        fuerzaLevitacion = -(rb.mass * Physics.gravity.y);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        alturaRelativa = calcularAltura();
        float velocidad = Mathf.Sqrt( Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2));
        if (velocidad < limSpeed)
        {
            rb.AddForce(fuerza);
        }

        AlcanzarAltura(alturaDeseada, alturaRelativa);
        
    }

    private void AlcanzarAltura(float alturaObjetivo, float alturaActual)
    {
        float distancia = alturaObjetivo - alturaActual;
        if (rb.velocity.y >= 0f)
        {
            float factor = distancia * rapidezVertical;
            rb.AddForce(transform.up * fuerzaLevitacion * factor);
        }
        else
        {
            float factor = Mathf.Max(0, distancia * rapidezVertical * 5);
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
            
        }
        
        return distancia;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Pared" || other.transform.tag == "Shell")
        {
            Destroy(gameObject);
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Pared" || collision.transform.tag == "Shell")
        {
            //Destroy(gameObject);
        }
        
    }
}
