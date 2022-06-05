//Programación de Videojuegos, Universidad de Málaga (Prof. M. Nuñez, mnunez@uma.es)
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

public class AprendizGiroDotYVelocidad : MonoBehaviour
{
    Classifier saberPredecirGiro, saberPredecirDotFinal;
    weka.core.Instances casosEntrenamiento;
    Text texto;
    private string ESTADO = "Sin conocimiento";
    public GameObject PuntoObjetivo;
    public float valorMaximoAceleracion, valorMaximoGiro, paso = 1, Velocidad_Simulacion = 1;
    float mejorGiro, mejorAceleracion, distanciaObjetivo;
    private Vector3 posicionActual;
    private Vector3 rotacionActual;
    Rigidbody r;

    WheelController wheelController;
    private float time = 0;

    DotProduct script;

    private float DotInicial;
    private bool esperando = false;

    void Start()
    {
        wheelController = GetComponent<WheelController>();
        script = GetComponent<DotProduct>();

        posicionActual = transform.position;
        rotacionActual = transform.eulerAngles;

        r = GetComponent<Rigidbody>();

        Time.timeScale = Velocidad_Simulacion;                                          //...opcional: hace que se vea más rápido (recomendable hasta 5)
        //texto = Canvas.FindObjectOfType<Text>();
        //if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento
                                                                                        //
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_GiroDotYVelocidad.arff"));
        saberPredecirGiro = (Classifier)SerializationHelper.read("Assets/saberPredecirGiroModelo");
        saberPredecirDotFinal = (Classifier)SerializationHelper.read("Assets/saberPredecirDotFinalModelo");
        ESTADO = "Con conocimiento";
    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vacía:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Iniciales_Experiencias_GiroDotYVelocidad.arff"));  //Lee fichero con variables. Sin instancias

        //Uso de una tabla con los datos del último entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_GiroDotYVelocidad.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {
            // texto.text = "ENTRENAMIENTO: crea una tabla con las fuerzas Fx y Fy utilizadas y las distancias alcanzadas.";
            //print("Datos de entrada: valorMaximoAceleracion=" + valorMaximoAceleracion + " valorMaximoGiro=" + valorMaximoGiro + "  " + ((valorMaximoAceleracion == 0 || valorMaximoGiro == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));
            for (float velocidad = 0; velocidad <= 7.1; velocidad = velocidad + 1)
            {
                for (float rotation = 0; rotation <= 181; rotation = rotation + 5)
                {
                    for (float aceleracion = -1; aceleracion <= 1.05; aceleracion = aceleracion + 0.2f)
                    {
                        for (float giro = -1; giro <= 1.05; giro = giro + 0.2f)                    //Bucle de planificación de la fuerza FY durante el entrenamiento
                        {
                            wheelController.speed = 0;
                            wheelController.turn = 0;
                            r.velocity = transform.forward * 0;
                            transform.position = posicionActual;
                            transform.eulerAngles = new Vector3(rotacionActual.x, rotation, rotacionActual.z);

                            time = Time.time;
                            yield return new WaitUntil(() => Time.time - time >= 0.1);

                            r.velocity = transform.forward * velocidad;
                            DotInicial = script.dot;
                            wheelController.speed = aceleracion;
                            wheelController.turn = giro;

                            time = Time.time;
                            yield return new WaitUntil(() => Time.time - time >= 0.25);       //... y espera a que la pelota llegue al suelo

                            Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                            print("ENTRENAMIENTO: con velocidad " + velocidad + " y dotInicial " + DotInicial + " y aceleración " + aceleracion + " y giro " + giro + " se alcanzó dotFinal de " + script.dot);
                            casoAaprender.setDataset(casosEntrenamiento);                          //crea un registro de experiencia
                            casoAaprender.setValue(0, velocidad);                                         //guarda los datos de las fuerzas Fx y Fy utilizadas
                            casoAaprender.setValue(1, DotInicial);
                            casoAaprender.setValue(2, aceleracion);
                            casoAaprender.setValue(3, giro);
                            casoAaprender.setValue(4, script.dot);
                            casosEntrenamiento.add(casoAaprender);                                 //guarda el registro en la lista casosEntrenamiento
                        }
                    }
                }
            }


            File salida = new File("Assets/Finales_Experiencias_GiroDotYVelocidad.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }


        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirGiro = new M5P();                                                //crea un algoritmo de aprendizaje M5P (árboles de regresión)
        casosEntrenamiento.setClassIndex(3);                                             //y hace que aprenda Fx dada la distancia y Fy
        saberPredecirGiro.buildClassifier(casosEntrenamiento);                        //REALIZA EL APRENDIZAJE DE FX A PARTIR DE LA DISTANCIA Y FY
        SerializationHelper.write("Assets/saberPredecirGiroModelo", saberPredecirGiro);

        saberPredecirDotFinal = new M5P();                                                //crea un algoritmo de aprendizaje M5P (árboles de regresión)
        casosEntrenamiento.setClassIndex(4);                                             //y hace que aprenda Fx dada la distancia y Fy
        saberPredecirDotFinal.buildClassifier(casosEntrenamiento);                        //REALIZA EL APRENDIZAJE DE FX A PARTIR DE LA DISTANCIA Y FY
        SerializationHelper.write("Assets/saberPredecirDotFinalModelo", saberPredecirDotFinal);

        distanciaObjetivo = 0;

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
    void FixedUpdate()                                                                                 //DURANTEL EL JUEGO: Aplica lo aprendido para lanzar a la canasta
    {
        if ((ESTADO == "Con conocimiento") && Time.time > 0.5)
        {
            float menorDistancia = 1e9f;

            if (!esperando)
            {
                time = Time.time;
                esperando = true;
            }

            if (esperando && Time.time - time >= 0.25)
            {
                for (float aceleracion = -1; aceleracion <= 1.05; aceleracion = aceleracion + 0.2f)                                           //Bucle FOR con fuerza Fy, deduce Fx = f (Fy, distancia) y escoge mejor combinacion         
                {
                    Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
                    casoPrueba.setDataset(casosEntrenamiento);
                    casoPrueba.setValue(0, r.velocity.magnitude);                                         //guarda los datos de las fuerzas Fx y Fy utilizadas
                    casoPrueba.setValue(1, script.dot);
                    casoPrueba.setValue(2, aceleracion);
                    casoPrueba.setValue(4, distanciaObjetivo);                                                  //y la distancia
                    float giro = (float)saberPredecirGiro.classifyInstance(casoPrueba);                          //Predice Fx a partir de la distancia y una Fy 

                    if (giro > -0.05 && giro < 0.05) giro = 0;
                    if (giro < -1) giro = -1;
                    if (giro > 1) giro = 1;

                    Instance casoPrueba2 = new Instance(casosEntrenamiento.numAttributes());
                    casoPrueba2.setDataset(casosEntrenamiento);                                                  //Utiliza el "modelo fisico aproximado" con Fx y Fy                 
                    casoPrueba2.setValue(0, r.velocity.magnitude);                                         //guarda los datos de las fuerzas Fx y Fy utilizadas
                    casoPrueba2.setValue(1, script.dot);
                    casoPrueba2.setValue(2, aceleracion);
                    casoPrueba2.setValue(3, giro);                                                             //Crea una registro con una Fy
                    float dotFinal = (float)saberPredecirDotFinal.classifyInstance(casoPrueba2);     //Predice la distancia dada Fx y Fy

                    //print("donFinal = " + dotFinal + " menorDistancia = " + menorDistancia);
                    if (Mathf.Abs(dotFinal - distanciaObjetivo) < menorDistancia || Mathf.Abs(dotFinal) - menorDistancia < 0.02)                     //Busca la Fy con una distancia más cercana al objetivo
                    {
                        menorDistancia = Mathf.Abs(dotFinal - distanciaObjetivo);                     //si encuentra una buena toma nota de esta distancia
                        mejorAceleracion = aceleracion;                                                                       //de la fuerzas que uso, Fx
                        mejorGiro = giro;                                                                       //tambien Fy
                        //print("RAZONAMIENTO: Para dotInicial" + script.dot + " y velocidad " + r.velocity.magnitude + " , una posible acción es ejercer una aceleración de = " + mejorAceleracion + " y giro = " + mejorGiro + " se alcanzaría una distancia de " + dotFinal);
                    }
                }

                wheelController.speed = mejorAceleracion;
                wheelController.turn = mejorGiro;

                //print("DECISION REALIZADA: Se ejerció aceleración " + mejorAceleracion + " y giro " + mejorGiro);
                esperando = false;
            }
        }
        /*if (ESTADO == "Acción realizada")
        {
            texto.text = "Para una canasta a " + distanciaObjetivo.ToString("0.000") + " m, las fuerzas Fx y Fy a utilizar será: " + mejorFuerzaX.ToString("0.000") + "N y " + mejorFuerzaY.ToString("0.000") + "N, respectivamente";
            if (r.transform.position.y < 0)                                            //cuando la pelota cae por debajo de 0 m
            {                                                                          //escribe la distancia en x alcanzada
                print("La canasta está a una distancia de " + distanciaObjetivo + " m");
                print("La pelota lanzada llegó a " + r.transform.position.x + ". El error fue de " + (r.transform.position.x - distanciaObjetivo).ToString("0.000000") + " m");
                r.isKinematic = true;
                ESTADO = "FIN";
            }
        }*/
    }
}