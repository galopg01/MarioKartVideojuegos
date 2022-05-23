using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ComportamientoCoche : MonoBehaviour{
    public Transform modeloRuedaFrontalDerecha, modeloRuedaIzquierda;
    public float fuerzaFrontal, velocGiro;
    float maxAnguloGiroVolante = 40;
    public bool colisionandoSuelo;
    Rigidbody rb;
    void Start(){
        rb = GetComponent<Rigidbody>();
        velocGiro     = rb.mass * 10 * 1.2f;
        fuerzaFrontal = rb.mass * 10 * 1.8f;
        colisionandoSuelo = true;
    }
    void FixedUpdate() {
        if (colisionandoSuelo)
        {   //El coche_gira cinemáticamente
            transform.Rotate(transform.up * Input.GetAxis("Horizontal") * velocGiro * Time.deltaTime * 0.03f);

            //Sus ruedas delanteras giran 
            modeloRuedaIzquierda.transform.localEulerAngles      = new Vector3(0, Input.GetAxis("Horizontal") * maxAnguloGiroVolante, 0);
            modeloRuedaFrontalDerecha.transform.localEulerAngles = new Vector3(0, Input.GetAxis("Horizontal") * maxAnguloGiroVolante, 0);

            //El coche NPC aplica una fuerza hacia su Z
            rb.AddRelativeForce(0, 0, Input.GetAxis("Vertical") * fuerzaFrontal);

            //Podría avazar cinemáticamente con: float velocFrontal = 10; rb.velocity= transform.forward*velocFrontal;
        }else
            print("Coche NPC no está colisionando con suelo. No se están aplicando fuerzas");
        colisionandoSuelo = false;
    }
    private void OnTriggerStay(Collider other) {
        if (other.gameObject.tag == "Suelo")
            colisionandoSuelo = true;
}}
