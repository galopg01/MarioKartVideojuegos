using java.io;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using weka.classifiers.trees;
using weka.core;
using weka.core.converters;

public class AprendizConduccionV1 : MonoBehaviour
{
    weka.classifiers.trees.M5P saberPredecirGiro;
    weka.core.Instances casosEntrenamiento;
    Text texto;
    private string ESTADO = "Sin conocimiento";
    public GameObject PuntoObjetivo;
    public float valorMaximoAceleracion, valorMaximoGiro, paso = 1, Velocidad_Simulacion = 1;
    float mejorGiro, giroActual, mejorAceleracion;
    private Vector3 posicionActual;
    private Vector3 rotacionActual;
    Rigidbody r;

    WheelController wheelController;
    private float time = 0;

    DotProduct script;

    private float DotInicial;
    private bool esperando = true;

    // Start is called before the first frame update
    void Start()
    {
        wheelController = GetComponent<WheelController>();
        script = GetComponent<DotProduct>();

        posicionActual = transform.position;
        rotacionActual = transform.eulerAngles;

        r = GetComponent<Rigidbody>();

        Time.timeScale = Velocidad_Simulacion;                                          //...opcional: hace que se vea más rápido (recomendable hasta 5)
        //texto = Canvas.FindObjectOfType<Text>();
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento     
    }
    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vacía:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Iniciales_Experiencias_GiroDotVNico.arff"));  //Lee fichero con variables. Sin instancias

        //Uso de una tabla con los datos del último entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_GiroDotVNico.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {
            // texto.text = "ENTRENAMIENTO: crea una tabla con las fuerzas Fx y Fy utilizadas y las distancias alcanzadas.";
            //print("Datos de entrada: valorMaximoAceleracion=" + valorMaximoAceleracion + " valorMaximoGiro=" + valorMaximoGiro + "  " + ((valorMaximoAceleracion == 0 || valorMaximoGiro == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));

            for (float acceleration = 100; acceleration <= 400; acceleration = acceleration + 5)
            {
                for (float giro = -1; giro <= valorMaximoGiro; giro = giro + paso)                    
                {
                    r.velocity = transform.forward * 0;
                    transform.position = posicionActual;
                    transform.eulerAngles = rotacionActual;

                    time = Time.time;
                    yield return new WaitUntil(() => Time.time - time >= 0.1);

                    DotInicial = script.dot;
                    wheelController.acceleration = acceleration;
                    wheelController.turn = giro;

                    time = Time.time;
                    yield return new WaitUntil(() => Time.time - time >= 0.5);      

                    Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                    print("ENTRENAMIENTO: con rotacion " + DotInicial + " y giro " + giro + " se alcanzó una rotacion de " + script.dot + " m");
                    casoAaprender.setDataset(casosEntrenamiento);                          //crea un registro de experiencia
                    casoAaprender.setValue(0, acceleration);                                        
                    casoAaprender.setValue(1, giro);
                    casoAaprender.setValue(2, script.dot);
                    casosEntrenamiento.add(casoAaprender);                                 //guarda el registro en la lista casosEntrenamiento
                }
            }


            File salida = new File("Assets/Finales_Experiencias_GiroDotVNico.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }

        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirGiro = new M5P();                                                
        casosEntrenamiento.setClassIndex(1);                                             
        saberPredecirGiro.buildClassifier(casosEntrenamiento);                        


        ESTADO = "Con conocimiento";
        print("uwu");

        /*print(casosEntrenamiento.numInstances() +" espers "+ saberPredecirDistancia.toString());

        //EVALUACION DEL CONOCIMIENTO APRENDIDO: 
        if (casosEntrenamiento.numInstances() >= 10){
            casosEntrenamiento.setClassIndex(0);
            Evaluation evaluador = new Evaluation(casosEntrenamiento);                   //...Opcional: si tien mas de 10 ejemplo, estima la posible precisión
            evaluador.crossValidateModel(saberPredecirFuerzaX, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Fx durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " N");
            casosEntrenamiento.setClassIndex(2);
            evaluador.crossValidateModel(saberPredecirDistancia, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Distancias durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " m");
        }

        //PRUEBA: Estimación de la distancia a la Canasta
        //distanciaObjetivo = leer_Distancia_de_la_canasta...  //...habría que implementar un metodo para leer la distancia objetivo;    

        //... o generacion aleatoria de una distancia dependiendo de sus límites:        
        AttributeStats estadisticasDistancia = casosEntrenamiento.attributeStats(2);        //Opcional: Inicializa las estadisticas de las distancias
        float maximaDistanciaAlcanzada = (float) estadisticasDistancia.numericStats.max;    //Opcional: Obtiene el valor máximo de las distancias alcanzadas
        distanciaObjetivo = UnityEngine.Random.Range(maximaDistanciaAlcanzada * 0.2f, maximaDistanciaAlcanzada * 0.8f);  //Opcional: calculo aleatorio de la distancia 

        /////////////////    SITUA LA CANASTA EN LA "distanciaObjetivo"  ESTIMADA   ///////////////////
        PuntoObjetivo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        PuntoObjetivo.transform.position = new Vector3(distanciaObjetivo, -1, 0);
        PuntoObjetivo.transform.localScale = new Vector3(1.1f, 1, 1.1f);
        PuntoObjetivo.GetComponent<Collider>().isTrigger = true;*/

        /////////////////////////////////////////////////////////////////////////////////////////////

    }
    // Update is called once per frame
    void FixedUpdate()                                   
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
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes()); 
                casoPrueba.setDataset(casosEntrenamiento);
                for(float acceleration = 100; acceleration < 400; acceleration ++)
                {
                    DotInicial = script.dot;
                    casoPrueba.setValue(0, acceleration);                               
                    casoPrueba.setValue(2, script.dot);
                    giroActual = (float)saberPredecirGiro.classifyInstance(casoPrueba);
                    if (giroActual > -0.05 && giroActual < 0.05) giroActual = 0;
                    if (giroActual < -1) giroActual = -1;
                    if (giroActual > 1) giroActual = 1;

                    if (ponderacion(acceleration, script.wall)>mejorAceleracion)
                    {
                        mejorAceleracion = acceleration;
                    }
                }
                wheelController.acceleration = mejorAceleracion;
                wheelController.turn = mejorGiro;

                print("DECISION REALIZADA: Se avanzó con giro =" + mejorGiro + "para dot = " + script.dot);
                esperando = false;
            }
        }
        
    }

    private float ponderacion(float a, GameObject wall)
    {
        float distancia = 4;
        if (wall != null)
        {
            distancia = Vector4.Distance(wall.transform.position, wall.transform.position);
        }
        return (float)((a * 0.3) + (distancia*100*0.7)); 
    }
}
