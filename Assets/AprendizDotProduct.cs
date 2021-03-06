//Programaci?n de Videojuegos, Universidad de M?laga (Prof. M. Nu?ez, mnunez@uma.es)
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

public class AprendizDotProduct : MonoBehaviour
{
    weka.classifiers.trees.M5P saberPredecirDistancia, saberPredecirSpeed;
    weka.core.Instances casosEntrenamiento;
    Text texto;
    private string ESTADO = "Sin conocimiento";
    public GameObject PuntoObjetivo;
    public float valorMaximoAceleracion, valorMaximoGiro, paso = 1, Velocidad_Simulacion = 1;
    float mejorAceleracion, mejorGiro, distanciaObjetivo;
    private Vector3 posicionActual;
    private Vector3 rotacionActual;
    Rigidbody r;

    WheelController wheelController;
    private float time;

    DotProduct script;

    private float DotInicial;

    void Start()
    {
        wheelController = GetComponent<WheelController>();
        script = GetComponent<DotProduct>();

        posicionActual = transform.position;
        rotacionActual = transform.eulerAngles;

        r = GetComponent<Rigidbody>();

        Time.timeScale = Velocidad_Simulacion;                                          //...opcional: hace que se vea m?s r?pido (recomendable hasta 5)
        //texto = Canvas.FindObjectOfType<Text>();
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento                                                                                    
    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vac?a:
        // casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Iniciales_Experiencias_DotProduct.arff"));  //Lee fichero con variables. Sin instancias

        //Uso de una tabla con los datos del ?ltimo entrenamiento:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_DotProduct.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {
            // texto.text = "ENTRENAMIENTO: crea una tabla con las fuerzas Fx y Fy utilizadas y las distancias alcanzadas.";
            print("Datos de entrada: valorMaximoAceleracion=" + valorMaximoAceleracion + " valorMaximoGiro=" + valorMaximoGiro + "  " + ((valorMaximoAceleracion == 0 || valorMaximoGiro == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));

            for (float rotation = 0; rotation <= 180; rotation = rotation + 30)
            {
                for (float speed = 0; speed <= valorMaximoAceleracion; speed = speed + paso)                      //Bucle de planificaci?n de la fuerza FX durante el entrenamiento
                {
                    for (float giro = -1; giro <= valorMaximoGiro; giro = giro + paso)                    //Bucle de planificaci?n de la fuerza FY durante el entrenamiento
                    {
                        r.velocity = new Vector3(0, 0, 0);
                        transform.position = posicionActual;
                        transform.eulerAngles = new Vector3(rotacionActual.x, rotation, rotacionActual.z);

                        DotInicial = script.dot;
                        wheelController.speed = speed;
                        wheelController.turn = giro;

                        time = Time.time;
                        yield return new WaitUntil(() => Time.time - time >= 1);       //... y espera a que la pelota llegue al suelo

                        Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                        print("ENTRENAMIENTO: con rotacion " + DotInicial + "y speed " + speed + " y giro " + giro + " se alcanz? una rotacion de " + script.dot + " m");
                        casoAaprender.setDataset(casosEntrenamiento);                          //crea un registro de experiencia
                        casoAaprender.setValue(0, DotInicial);                                         //guarda los datos de las fuerzas Fx y Fy utilizadas
                        casoAaprender.setValue(1, speed);
                        casoAaprender.setValue(2, giro);                    //anota la distancia alcanzada
                        casoAaprender.setValue(3, script.dot);
                        casosEntrenamiento.add(casoAaprender);                                 //guarda el registro en la lista casosEntrenamiento
                    }                                                                          //FIN bucle de lanzamientos con diferentes de fuerzas
                }
            }


            File salida = new File("Assets/Finales_Experiencias_DotProduct.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }

        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirSpeed = new M5P();                                                //crea un algoritmo de aprendizaje M5P (?rboles de regresi?n)
        casosEntrenamiento.setClassIndex(1);                                             //y hace que aprenda Fx dada la distancia y Fy
        saberPredecirSpeed.buildClassifier(casosEntrenamiento);                        //REALIZA EL APRENDIZAJE DE FX A PARTIR DE LA DISTANCIA Y FY

        saberPredecirDistancia = new M5P();                                              //crea otro algoritmo de aprendizaje M5P (?rboles de regresi?n)  
        casosEntrenamiento.setClassIndex(3);                                             //La variable a aprender a calcular la distancia dada Fx e FY                                                                                         
        saberPredecirDistancia.buildClassifier(casosEntrenamiento);                      //este algoritmo aprende un "modelo fisico aproximado"

        distanciaObjetivo = 0;

        ESTADO = "Con conocimiento";
        print("uwu");

        /*print(casosEntrenamiento.numInstances() +" espers "+ saberPredecirDistancia.toString());

        //EVALUACION DEL CONOCIMIENTO APRENDIDO: 
        if (casosEntrenamiento.numInstances() >= 10){
            casosEntrenamiento.setClassIndex(0);
            Evaluation evaluador = new Evaluation(casosEntrenamiento);                   //...Opcional: si tien mas de 10 ejemplo, estima la posible precisi?n
            evaluador.crossValidateModel(saberPredecirFuerzaX, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Fx durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " N");
            casosEntrenamiento.setClassIndex(2);
            evaluador.crossValidateModel(saberPredecirDistancia, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Distancias durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " m");
        }

        //PRUEBA: Estimaci?n de la distancia a la Canasta
        //distanciaObjetivo = leer_Distancia_de_la_canasta...  //...habr?a que implementar un metodo para leer la distancia objetivo;    

        //... o generacion aleatoria de una distancia dependiendo de sus l?mites:        
        AttributeStats estadisticasDistancia = casosEntrenamiento.attributeStats(2);        //Opcional: Inicializa las estadisticas de las distancias
        float maximaDistanciaAlcanzada = (float) estadisticasDistancia.numericStats.max;    //Opcional: Obtiene el valor m?ximo de las distancias alcanzadas
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
        if ((ESTADO == "Con conocimiento"))
        {
            Time.timeScale = 1;                                                                               //Durante el juego, el NPC razona as?... (no juega a?n)   
            float menorDistancia = 1e9f;
            print("-- OBJETIVO: LANZAR LA PELOTA A UNA DISTANCIA DE " + distanciaObjetivo + " m.");

            //Si usa dos bucles Fx y Fy con "modelo fisico aproximado", complejidad n^2
            //Reduce la complejidad con un solo bucle FOR, as?

            for (float giro = -1; giro <= valorMaximoGiro; giro = giro + paso)                                             //Bucle FOR con fuerza Fy, deduce Fx = f (Fy, distancia) y escoge mejor combinacion         
            {
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
                casoPrueba.setDataset(casosEntrenamiento);
                casoPrueba.setValue(0, script.dot);
                casoPrueba.setValue(2, giro);                                                                   //crea un registro con una Fy
                casoPrueba.setValue(3, distanciaObjetivo);                                                    //y la distancia
                float speed = (float)saberPredecirSpeed.classifyInstance(casoPrueba);                          //Predice Fx a partir de la distancia y una Fy 
                if ((speed >= -5) && (speed <= 20))
                {
                    Instance casoPrueba2 = new Instance(casosEntrenamiento.numAttributes());
                    casoPrueba2.setDataset(casosEntrenamiento);                                                  //Utiliza el "modelo fisico aproximado" con Fx y Fy
                    casoPrueba.setValue(0, script.dot);
                    casoPrueba2.setValue(1, speed);                                                                 //Crea una registro con una Fx
                    casoPrueba2.setValue(2, giro);                                                                 //Crea una registro con una Fy
                    float prediccionDistancia = (float)saberPredecirDistancia.classifyInstance(casoPrueba2);     //Predice la distancia dada Fx y Fy
                    if (Mathf.Abs(prediccionDistancia - distanciaObjetivo) < menorDistancia)                     //Busca la Fy con una distancia m?s cercana al objetivo
                    {
                        menorDistancia = Mathf.Abs(prediccionDistancia - distanciaObjetivo);                     //si encuentra una buena toma nota de esta distancia
                        mejorAceleracion = speed;                                                                       //de la fuerzas que uso, Fx
                        mejorGiro = giro;                                                                       //tambien Fy
                        print("RAZONAMIENTO: Una posible acci?n es ejercer una speed=" + mejorAceleracion + " y giro= " + mejorGiro + " se alcanzar?a una distancia de " + prediccionDistancia);
                    }
                }
            }                                                                                                     //FIN DEL RAZONAMIENTO PREVIO
            if ((mejorAceleracion == 2000) && (mejorGiro == 2000))
            {
                texto.text = "NO SE LANZ? LA PELOTA: La distancia de " + distanciaObjetivo + " m no se ha alcanzado muchas veces.";
                print(texto.text);
            }
            else
            {
                //r.velocity = new Vector3(0, 0, 0);
                //transform.position = posicionActual;
                //transform.rotation = rotacionActual;

                wheelController.speed = mejorAceleracion;
                wheelController.turn = mejorGiro;

                print("DECISION REALIZADA: Se lanz? pelota con speed =" + mejorAceleracion + " y giro= " + mejorGiro);
                ESTADO = "Acci?n realizada";
            }
        }
        /*if (ESTADO == "Acci?n realizada")
        {
            texto.text = "Para una canasta a " + distanciaObjetivo.ToString("0.000") + " m, las fuerzas Fx y Fy a utilizar ser?: " + mejorFuerzaX.ToString("0.000") + "N y " + mejorFuerzaY.ToString("0.000") + "N, respectivamente";
            if (r.transform.position.y < 0)                                            //cuando la pelota cae por debajo de 0 m
            {                                                                          //escribe la distancia en x alcanzada
                print("La canasta est? a una distancia de " + distanciaObjetivo + " m");
                print("La pelota lanzada lleg? a " + r.transform.position.x + ". El error fue de " + (r.transform.position.x - distanciaObjetivo).ToString("0.000000") + " m");
                r.isKinematic = true;
                ESTADO = "FIN";
            }
        }*/
    }
}