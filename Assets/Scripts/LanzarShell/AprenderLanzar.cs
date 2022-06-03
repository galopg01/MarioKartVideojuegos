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

    Classifier saberPredecirFx, saberPredecirDistanciaFinal;
    weka.core.Instances casosEntrenamiento;
    private string ESTADO = "Sin conocimiento";
    
    Rigidbody rb;

    public GameObject ShellPrefab, GoombaPrefab;
    GameObject ShellInstance, GoombaInstance;
    public GameObject Goomba;
    public float Fz=10;
    public float Velocidad_Simulacion = 5;
    public bool lanzado = false;
    private float distanciaAnterior;
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Time.timeScale = Velocidad_Simulacion;   
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento

        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_LanzarShell.arff"));
        //saberPredecirFx = (Classifier)SerializationHelper.read("Assets/saberPredecirFxLanzarShellModelo");
        //saberPredecirDistanciaFinal = (Classifier)SerializationHelper.read("Assets/saberPredecirDistanciaFinalLanzarShellModelo");
        //ESTADO = "Con conocimiento";

    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vac?a:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Iniciales_Experiencias_LanzarShell.arff"));  //Lee fichero con variables. Sin instancias

        //Uso de una tabla con los datos del ?ltimo entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_GiroDotYVelocidad.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {

            for (float Fx = -20f; Fx <= 20;Fx = Fx + 0.25f)
            {
                for (float disGoombaX = -5; disGoombaX <= 5; disGoombaX += 0.25f)
                {
                    for (float disGoombaZ = 5 ; disGoombaZ <= 15; disGoombaZ += 0.5f)
                    {
                        
                        ShellInstance = Instantiate(ShellPrefab, transform.position, transform.rotation);
                        GoombaInstance = Instantiate(GoombaPrefab, new Vector3(disGoombaX, 0.5f, disGoombaZ), Quaternion.Euler(0, 45, 0));
                        transform.LookAt(GoombaInstance.transform);
                        Rigidbody rbShell = ShellInstance.GetComponent<Rigidbody>();
                        Vector3 fuerzaZ = transform.forward * Fz;
                        Vector3 fuerzaX = transform.right * Fx;
                        rbShell.AddForce(fuerzaX + fuerzaZ);
                        
                        float time = Time.time;
                        distanciaAnterior = Vector3.Distance(ShellInstance.transform.position, GoombaInstance.transform.position);
                        yield return new WaitUntil(() => Time.time - time > 0.01f);
                        
                        yield return new WaitUntil(() => seAlejen(ShellInstance, GoombaInstance));

                        float finalDistanceToGoomba = Vector3.Distance(ShellInstance.transform.position, GoombaInstance.transform.position);
                        Destroy(GoombaInstance);
                        Destroy(ShellInstance);

                        Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                        print("ENTRENAMIENTO: con Fx " + Fx + " y distancia a enemigo X " + disGoombaX + " y distancia a enemigo Z " + disGoombaZ +" se alcanzo distancia a objetivo de " + finalDistanceToGoomba);
                        casoAaprender.setDataset(casosEntrenamiento);                          
                        casoAaprender.setValue(0, Fx);                                         
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
        saberPredecirFx = new M5P();                                                
        casosEntrenamiento.setClassIndex(0);                                             
        saberPredecirFx.buildClassifier(casosEntrenamiento);                        
        SerializationHelper.write("Assets/saberPredecirFxLanzarShellModelo", saberPredecirFx);

        saberPredecirDistanciaFinal = new M5P();
        casosEntrenamiento.setClassIndex(3);
        saberPredecirDistanciaFinal.buildClassifier(casosEntrenamiento);
        SerializationHelper.write("Assets/saberPredecirDistanciaFinalLanzarShellModelo", saberPredecirDistanciaFinal);

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
        if ((ESTADO == "Con conocimiento") && !lanzado && Time.time > 0.5)
        {
            float mejorDistanciaFinal=300;
            float mejorFx=0;
            transform.LookAt(Goomba.transform);
            for (float Fx =-20; Fx<20;Fx+=1f)
            {
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
                casoPrueba.setDataset(casosEntrenamiento);
                casoPrueba.setValue(0, Fx);
                casoPrueba.setValue(1, Goomba.transform.position.x - transform.parent.position.x );
                casoPrueba.setValue(2, Goomba.transform.position.z - transform.parent.position.z );
                float distanciaFinal = (float)saberPredecirDistanciaFinal.classifyInstance(casoPrueba);

                if (distanciaFinal < mejorDistanciaFinal)
                {
                    mejorDistanciaFinal = distanciaFinal;
                    mejorFx = Fx;
                }

            }
            

            ShellInstance = Instantiate(ShellPrefab, transform.position, transform.rotation);
            Rigidbody rbShell = ShellInstance.GetComponent<Rigidbody>();
            Vector3 fuerzaZ = transform.forward * Fz;
            Vector3 fuerzaX = transform.right * mejorFx;
            rbShell.AddForce(fuerzaX + fuerzaZ);
            lanzado = true;
            print("DECISION REALIZADA: Fx " + mejorFx);
            
        }


       
            
        
    }
}
