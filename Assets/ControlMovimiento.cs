using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlMovimiento : MonoBehaviour{
    private Rigidbody rb;
    private Rigidbody rbKart;
    public GameObject ObjetoPerseguido;
    public float fuerzaLevitacion;
    public float RapidezHorizontal = 60f;
    public float RapidezVertical = 0.4f;
    public float SeparacionConObjetivo = 0;

    void Start() { 
        rb = GetComponent<Rigidbody>(); 
        rbKart = GameObject.Find("Kart").GetComponent<Rigidbody>();
        fuerzaLevitacion= -(rbKart.mass * Physics.gravity.y);
    }

    void FixedUpdate() {
        
        AlcanzarPosicion( ObjetoPerseguido.transform.position - SeparacionConObjetivo * ObjetoPerseguido.transform.forward, RapidezHorizontal, 1);

        if(gameObject.name.Contains("Paracaidas")){
            rbKart.AddForce(Vector3.up * fuerzaLevitacion * 0.8f);
        }else{
            AlcanzarAltura( ObjetoPerseguido.transform.position.y, RapidezVertical);
            
        }

        if(Vector3.Distance(transform.position,ObjetoPerseguido.transform.position)<=3f || (ObjetoPerseguido.name.Equals("o2") && transform.position.y-ObjetoPerseguido.transform.position.y<=1f)){              
                GameObject.Find("Kart").GetComponent<WheelController>().estado="Normal";
                Destroy(gameObject);
    
        }
    
    }

    private void AlcanzarAltura(float alturaObjetivo, float rapidezVertical){
        float distancia = (alturaObjetivo - transform.position.y);
        if(rbKart.velocity.y>=0f){
            float factor= distancia * rapidezVertical;
            rbKart.AddForce(Vector3.up * fuerzaLevitacion * factor);
        }else{
            float factor= Mathf.Max(0, distancia * rapidezVertical * 5 );
            rbKart.AddForce(Vector3.up * fuerzaLevitacion * factor);
        }
    }
    
    private void AlcanzarPosicion(Vector3 posObjetivo, float rapidezHorizontal, float propulsionFrontal){
        Vector3 vectorHaciaObjetivo = posObjetivo - transform.position;
        float velocidadRelativa= ObjetoPerseguido.GetComponent<Rigidbody>().velocity.magnitude - rbKart.velocity.magnitude;
        float angulo = Vector3.Angle(vectorHaciaObjetivo, GetComponent<Rigidbody>().velocity);
        
        if ((velocidadRelativa>0) || (angulo < 70)){
            float factor = vectorHaciaObjetivo.magnitude * rapidezHorizontal;
            rbKart.AddForce(vectorHaciaObjetivo * propulsionFrontal * factor);
            rbKart.transform.LookAt(new Vector3(posObjetivo.x, rbKart.transform.position.y, posObjetivo.z));
        } 
    }
}