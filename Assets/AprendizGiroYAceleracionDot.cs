//Programaci�n de Videojuegos, Universidad de M�laga (Prof. M. Nu�ez, mnunez@uma.es)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using weka.classifiers.trees;
using weka.classifiers.evaluation;
using weka.core;
using java.io;
using java.lang;
using java.util;
using weka.classifiers.functions;
using weka.classifiers;
using weka.core.converters;
using System;

public class AprendizGiroYAceleracionDot : MonoBehaviour
{
    weka.classifiers.trees.M5P saberPredecirGiro, saberPredecirVelocidad, saberPredecirDot;
    weka.core.Instances casosEntrenamiento;
    Text texto;
    private string ESTADO = "Sin conocimiento";
    public GameObject PuntoObjetivo;
    public float valorMaximoAceleracion, valorMaximoGiro, paso = 1, Velocidad_Simulacion = 1;
    float mejorGiro, mejorSpeed, menorDot, mejorPonderacion=0;
    private Vector3 posicionActual;
    private Vector3 rotacionActual;
    Rigidbody r;

    WheelControllerV2 wheelController;
    private float time = 0;

    DotProduct script;

    private float DotInicial;
    private bool esperando = false;

    void Start()
    {
        wheelController = GetComponent<WheelControllerV2>();
        script = GetComponent<DotProduct>();

        posicionActual = transform.position;
        rotacionActual = transform.eulerAngles;

        r = GetComponent<Rigidbody>();

        Time.timeScale = Velocidad_Simulacion;                                          //...opcional: hace que se vea m�s r�pido (recomendable hasta 5)
        //texto = Canvas.FindObjectOfType<Text>();
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento                                                                                    
    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vac�a:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Iniciales_Experiencias_GiroYAceleracion_Dot.arff"));  //Lee fichero con variables. Sin instancias

        //Uso de una tabla con los datos del �ltimo entrenamiento:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_GiroYAceleracion_Dot.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {
            // texto.text = "ENTRENAMIENTO: crea una tabla con las fuerzas Fx y Fy utilizadas y las distancias alcanzadas.";
            //print("Datos de entrada: valorMaximoAceleracion=" + valorMaximoAceleracion + " valorMaximoGiro=" + valorMaximoGiro + "  " + ((valorMaximoAceleracion == 0 || valorMaximoGiro == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));

            for(float speed=0; speed<=1; speed+=paso){
                for (float rotation = 0; rotation <= 180; rotation = rotation + 1)
                {
                    for (float giro = -1; giro <= valorMaximoGiro; giro = giro + paso)                    //Bucle de planificaci�n de la fuerza FY durante el entrenamiento
                    {
                        r.velocity = transform.forward * 0;
                        transform.position = posicionActual;
                        transform.eulerAngles = new Vector3(rotacionActual.x, rotation, rotacionActual.z);

                        time = Time.time;
                        yield return new WaitUntil(() => Time.time - time >= 0.1);

                        DotInicial = script.dot;
                        r.velocity = transform.forward * 1;
                        wheelController.speed = speed;
                        wheelController.turn = giro;

                        time = Time.time;
                        yield return new WaitUntil(() => Time.time - time >= 0.5);       //... y espera a que la pelota llegue al suelo

                        Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                        print("ENTRENAMIENTO: con velocidad " + speed + " y rotacion " + DotInicial + " y giro " + giro + " se alcanzo una rotacion de " + script.dot + " m");
                        casoAaprender.setDataset(casosEntrenamiento);                          //crea un registro de experiencia
                        casoAaprender.setValue(0, DotInicial);                                         //guarda los datos de las fuerzas Fx y Fy utilizadas
                        casoAaprender.setValue(1, giro);
                        casoAaprender.setValue(2, script.dot);
                        casoAaprender.setValue(3, speed);
                        casosEntrenamiento.add(casoAaprender);                                 //guarda el registro en la lista casosEntrenamiento
                    }
                }
            }


            File salida = new File("Assets/Finales_Experiencias_GiroYAceleracion_Dot.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }

        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirGiro = new M5P();                                                //crea un algoritmo de aprendizaje M5P (�rboles de regresi�n)
        casosEntrenamiento.setClassIndex(1);                                             //y hace que aprenda Fx dada la distancia y Fy
        saberPredecirGiro.buildClassifier(casosEntrenamiento);                        //REALIZA EL APRENDIZAJE DE FX A PARTIR DE LA DISTANCIA Y FY

        saberPredecirVelocidad = new M5P();
        casosEntrenamiento.setClassIndex(3);                                             //y hace que aprenda Fx dada la distancia y Fy
        saberPredecirVelocidad.buildClassifier(casosEntrenamiento);                        //REALIZA EL APRENDIZAJE DE FX A PARTIR DE LA DISTANCIA Y FY

        saberPredecirDot = new M5P();
        casosEntrenamiento.setClassIndex(2);                                             //y hace que aprenda Fx dada la distancia y Fy
        saberPredecirDot.buildClassifier(casosEntrenamiento);                        //REALIZA EL APRENDIZAJE DE FX A PARTIR DE LA DISTANCIA Y FY

        ESTADO = "Con conocimiento";
        print("uwu");

    }
    void FixedUpdate()                                                                                 //DURANTEL EL JUEGO: Aplica lo aprendido para lanzar a la canasta
    {
        if ((ESTADO == "Con conocimiento") && Time.time > 0.5)
        {
            if (!esperando)
            {
                time = Time.time;
                esperando = true;
            }

            if (esperando && Time.time - time >= 0.5)
            {
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());  //Crea un registro de experiencia durante el juego
                
                for (float speed = 0; speed<=1; speed+=paso)
                {
                    for(float giro = -1; giro<=valorMaximoGiro; giro += paso)
                    {
                        casoPrueba.setDataset(casosEntrenamiento);
                        casoPrueba.setValue(0, script.dot);   
                        casoPrueba.setValue(1, giro);
                        casoPrueba.setValue(3, speed);
                        float dot = (float)saberPredecirDot.classifyInstance(casoPrueba);
                        float ponderacion = calcularPonderacion(speed, dot);
                        if (ponderacion>mejorPonderacion)
                        {
                            mejorGiro = giro;
                            mejorSpeed = speed;
                            mejorPonderacion = ponderacion;
                            
                        }
                    }
                    
                }
                
                if (mejorGiro > -0.05 && mejorGiro < 0.05) mejorGiro = 0;
                if (mejorGiro < -1) mejorGiro = -1;
                if (mejorGiro > 1) mejorGiro = 1;

                wheelController.turn = mejorGiro;
                wheelController.speed = mejorSpeed;

                print("DECISION REALIZADA: Se avanza con giro =" + mejorGiro + "y velocidad = "+ mejorSpeed + "para dot = " + script.dot);
                esperando = false;
            }
        }
        
    }

    private float calcularPonderacion(float speed, float dot)
    {
        return (float)(( (speed*80) + (1 / System.Math.Pow(10, 12)*dot)) /100);
    }
}