using System;
using System.Collections;
using System.Collections.Generic;
using java.io;
using UnityEngine;
using weka.classifiers;
using weka.classifiers.trees;
using weka.core;
using weka.core.converters;

public class AprenderLanzar : MonoBehaviour
{

    Classifier saberPredecirFy, saberPredecirDistanciaFinal;
    weka.core.Instances casosEntrenamiento;
    private string ESTADO = "Sin conocimiento";
    
    Rigidbody rb;

    public GameObject ShellPrefab, GoombaPrefab;
    GameObject ShellInstance, GoombaInstance;
    public float Fx=1;
    public float Velocidad_Simulacion = 5;

    private float distanciaAnterior;
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Time.timeScale = Velocidad_Simulacion;   
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento

        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_LanzarShell.arff"));
        
    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vac?a:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Iniciales_Experiencias_LanzarShell.arff"));  //Lee fichero con variables. Sin instancias

        //Uso de una tabla con los datos del ?ltimo entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_GiroDotYVelocidad.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {

            for (float Fy = 0; Fy <= 2;Fy = Fy + 0.05f)
            {
                for (float disGoombaX = -6; disGoombaX <= 6; disGoombaX += 0.5f)
                {
                    for (float disGoombaZ = 5 ; disGoombaZ <= 20; disGoombaZ += 0.5f)
                    {
                        GoombaInstance = Instantiate(GoombaPrefab, new Vector3(disGoombaX,0.5f, disGoombaZ),Quaternion.Euler(0,45,0));
                        ShellInstance = Instantiate(ShellPrefab, transform.position, transform.rotation);

                        Rigidbody rbShell = ShellInstance.GetComponent<Rigidbody>();
                        
                        rbShell.AddForce(transform.right * Fy);
                        float time = Time.time;
                        distanciaAnterior = Vector3.Distance(ShellInstance.transform.position, GoombaInstance.transform.position);
                        yield return new WaitUntil(() => Time.time - time > 0.1f);
                        
                        yield return new WaitUntil(() => seAlejen(ShellInstance, GoombaInstance));

                        float finalDistanceToGoomba = Vector3.Distance(ShellInstance.transform.position, GoombaInstance.transform.position);
                        Destroy(GoombaInstance);
                        Destroy(ShellInstance);

                        Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                        print("ENTRENAMIENTO: con Fy " + Fy + " y distancia a enemigo X " + disGoombaX + " y distancia a enemigo Z " + disGoombaZ +" se alcanzo distancia a objetivo de " + finalDistanceToGoomba);
                        casoAaprender.setDataset(casosEntrenamiento);                          
                        casoAaprender.setValue(0, Fy);                                         
                        casoAaprender.setValue(1, disGoombaX);
                        casoAaprender.setValue(2, disGoombaZ);
                        casoAaprender.setValue(3, finalDistanceToGoomba);
                        casosEntrenamiento.add(casoAaprender);                                 
                        
                    }
                }
            }


            File salida = new File("Assets/Finales_Experiencias_LanzarShell.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }


        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirFy = new M5P();                                                
        casosEntrenamiento.setClassIndex(0);                                             
        saberPredecirFy.buildClassifier(casosEntrenamiento);                        
        SerializationHelper.write("Assets/saberPredecirFyLanzarShellModelo", saberPredecirFy);

        saberPredecirDistanciaFinal = new M5P();
        casosEntrenamiento.setClassIndex(0);
        saberPredecirDistanciaFinal.buildClassifier(casosEntrenamiento);
        SerializationHelper.write("Assets/saberPredecirFyLanzarShellModelo", saberPredecirDistanciaFinal);

        ESTADO = "Con conocimiento";
        print("uwu");

        
    }

    private bool seAlejen(GameObject shellInstance, GameObject goombaInstance)
    {

        bool seAlejan = false;
        float distanciaActual = Vector3.Distance(ShellInstance.transform.position, GoombaInstance.transform.position);
        if (distanciaActual > distanciaAnterior) { 
            seAlejan = true;
        }
        distanciaAnterior = distanciaActual;
        return seAlejan;
    }

    void FixedUpdate()                                                                                 
    {
        if ((ESTADO == "Con conocimiento") && Time.time > 0.5)
        {
            float mejorDistanciaFinal=300;
            float mejorFy=0;
            for (float Fy =0; Fy<2;Fy+=0.1f)
            {
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
                casoPrueba.setDataset(casosEntrenamiento);
                casoPrueba.setValue(0, Fy);
                casoPrueba.setValue(1, GoombaInstance.transform.position.x - transform.position.x);
                casoPrueba.setValue(2, GoombaInstance.transform.position.z - transform.position.z);
                float distanciaFinal = (float)saberPredecirDistanciaFinal.classifyInstance(casoPrueba);

                if (distanciaFinal < mejorDistanciaFinal)
                {
                    mejorDistanciaFinal = distanciaFinal;
                    mejorFy = Fy;
                }
            }
            

            ShellInstance = Instantiate(ShellPrefab);
            Rigidbody rbShell = ShellInstance.GetComponent<Rigidbody>();
            rbShell.AddForce(transform.right * mejorFy);

            print("DECISION REALIZADA: Fy " + mejorFy);
            
        }


       
            
        
    }
}
