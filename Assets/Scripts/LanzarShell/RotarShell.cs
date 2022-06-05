using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RotarShell : MonoBehaviour
{
   
    private float alturaRelativa;
    private float fuerzaLevitacion;
    private float fuerzaRotacion;
    private float rapidezVertical = 3f;
    private float alturaDeseada = 0.5f;
    private Transform transformOriginal;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        transformOriginal = rb.transform;
        fuerzaLevitacion = -(rb.mass * Physics.gravity.y);
        fuerzaRotacion = 1f;
    }
    public Transform getTransformOriginal()
    {
        return transformOriginal;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        AlcanzarAltura(alturaDeseada, calcularAltura());
        rb.AddTorque(transform.up * fuerzaRotacion);
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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Enemy")
        {
            Destroy(gameObject);
        }

    }
}
