using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerarGoomba : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject[] spots;
    private int randomSpot;
    private double timeUltLanzamiento;
    public GameObject goomba, goombaGenerado;
    private int numSpot;
    public int frecuenciaAparicion=10;
    public GameObject kart;
    
    void Start()
    {
        timeUltLanzamiento = 0;
        numSpot = gameObject.transform.childCount;
        spots = new GameObject[numSpot];
        randomSpot = 3;// Random.Range(0, numSpot);
        for (int i=0; i<numSpot; i++)
        {
            spots[i]=gameObject.transform.GetChild(i).gameObject;
        }
        Generar1Goomba();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time-timeUltLanzamiento > frecuenciaAparicion)
        {
            timeUltLanzamiento = Time.time;
            randomSpot = Random.Range(0, numSpot);
            Generar1Goomba();
            Destroy(goombaGenerado, frecuenciaAparicion*5);
        }
    }

    void Generar1Goomba()
    {
        Quaternion rotation = spots[randomSpot].transform.rotation;
        Vector3 rotationEuler = rotation.eulerAngles;
        rotationEuler += new Vector3(0, 45, 0);
        goombaGenerado = Instantiate(goomba, spots[randomSpot].transform.position, Quaternion.Euler(rotationEuler));
        kart.GetComponentInChildren<AprenderLanzar>().Goomba = goombaGenerado;
        kart.GetComponentInChildren<AprenderLanzar>().hayEnemigo = true;
    }
}
