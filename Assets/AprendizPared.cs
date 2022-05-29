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

public class AprendizPared : MonoBehaviour
{
    weka.classifiers.trees.M5P saberPredecirDistancia, saberPredecirSpeed;
    weka.core.Instances casosEntrenamiento;
    Text texto;
    private string ESTADO = "Sin conocimiento";
    public GameObject PuntoObjetivo;
    public float valorMaximoAceleracion, valorMaximoGiro, paso=1, Velocidad_Simulacion=1;
    float mejorAceleracion, mejorGiro, distanciaObjetivo;
    private Vector3 posicionActual;
    private Quaternion rotacionActual;
    Rigidbody r;

    WheelController wheelController;
    private float time;
 
    void Start()
    {
        wheelController = GetComponent<WheelController>();

        posicionActual = transform.position;
        rotacionActual = transform.rotation;

        r = GetComponent<Rigidbody>();

        Time.timeScale = Velocidad_Simulacion;                                          //...opcional: hace que se vea más rápido (recomendable hasta 5)
        //texto = Canvas.FindObjectOfType<Text>();
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento                                                                                    
    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vacía:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/ExperienciasParedIniciales.arff"));  //Lee fichero con variables. Sin instancias
        
        //Uso de una tabla con los datos del último entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {
            // texto.text = "ENTRENAMIENTO: crea una tabla con las fuerzas Fx y Fy utilizadas y las distancias alcanzadas.";
            print("Datos de entrada: valorMaximoAceleracion=" + valorMaximoAceleracion + " valorMaximoGiro=" + valorMaximoGiro + "  " + ((valorMaximoAceleracion == 0 || valorMaximoGiro == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));
            for (float speed = 0; speed <= valorMaximoAceleracion; speed = speed +  0.1f)                      //Bucle de planificación de la fuerza FX durante el entrenamiento
            {
                for (float giro = -1; giro <= valorMaximoGiro; giro = giro + paso)                    //Bucle de planificación de la fuerza FY durante el entrenamiento
                {
                    r.velocity = new Vector3(0, 0, 0);
                    transform.position = posicionActual;
                    transform.rotation = rotacionActual;

                    wheelController.speed = speed;
                    wheelController.turn = giro;

                    time = Time.time;
                    yield return new WaitUntil(() => Time.time - time >= 2);       //... y espera a que la pelota llegue al suelo

                    Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                    print("ENTRENAMIENTO: con speed " + speed + " y giro " + giro + " se alcanzó una distancia de " + transform.position.z + " m");
                    casoAaprender.setDataset(casosEntrenamiento);                          //crea un registro de experiencia
                    casoAaprender.setValue(0, speed);                                         //guarda los datos de las fuerzas Fx y Fy utilizadas
                    casoAaprender.setValue(1, giro);
                    casoAaprender.setValue(2, transform.position.z);                    //anota la distancia alcanzada
                    casosEntrenamiento.add(casoAaprender);                                 //guarda el registro en la lista casosEntrenamiento
                }                                                                          //FIN bucle de lanzamientos con diferentes de fuerzas
            }


            File salida = new File("Assets/ExperienciasParedFinales.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }

        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirSpeed = new M5P();                                                //crea un algoritmo de aprendizaje M5P (árboles de regresión)
        casosEntrenamiento.setClassIndex(0);                                             //y hace que aprenda Fx dada la distancia y Fy
        saberPredecirSpeed.buildClassifier(casosEntrenamiento);                        //REALIZA EL APRENDIZAJE DE FX A PARTIR DE LA DISTANCIA Y FY

        saberPredecirDistancia = new M5P();                                              //crea otro algoritmo de aprendizaje M5P (árboles de regresión)  
        casosEntrenamiento.setClassIndex(2);                                             //La variable a aprender a calcular la distancia dada Fx e FY                                                                                         
        saberPredecirDistancia.buildClassifier(casosEntrenamiento);                      //este algoritmo aprende un "modelo fisico aproximado"

        distanciaObjetivo = PuntoObjetivo.transform.position.z;

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
        if ((ESTADO == "Con conocimiento"))
        {
            Time.timeScale = 1;                                                                               //Durante el juego, el NPC razona así... (no juega aún)   
            float menorDistancia = 1e9f;
            print("-- OBJETIVO: LANZAR LA PELOTA A UNA DISTANCIA DE " + distanciaObjetivo + " m.");

            //Si usa dos bucles Fx y Fy con "modelo fisico aproximado", complejidad n^2
            //Reduce la complejidad con un solo bucle FOR, así

            for (float giro = -1; giro <= valorMaximoGiro; giro = giro + paso)                                             //Bucle FOR con fuerza Fy, deduce Fx = f (Fy, distancia) y escoge mejor combinacion         
            {
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
                casoPrueba.setDataset(casosEntrenamiento);
                casoPrueba.setValue(1, giro);                                                                   //crea un registro con una Fy
                casoPrueba.setValue(2, distanciaObjetivo);                                                    //y la distancia
                float speed = (float)saberPredecirSpeed.classifyInstance(casoPrueba);                          //Predice Fx a partir de la distancia y una Fy 
                if ((speed >= -5) && (speed <= 20))
                {
                    Instance casoPrueba2 = new Instance(casosEntrenamiento.numAttributes());
                    casoPrueba2.setDataset(casosEntrenamiento);                                                  //Utiliza el "modelo fisico aproximado" con Fx y Fy                 
                    casoPrueba2.setValue(0, speed);                                                                 //Crea una registro con una Fx
                    casoPrueba2.setValue(1, giro);                                                                 //Crea una registro con una Fy
                    float prediccionDistancia = (float)saberPredecirDistancia.classifyInstance(casoPrueba2);     //Predice la distancia dada Fx y Fy
                    if (Mathf.Abs(prediccionDistancia - distanciaObjetivo) < menorDistancia)                     //Busca la Fy con una distancia más cercana al objetivo
                    {
                        menorDistancia = Mathf.Abs(prediccionDistancia - distanciaObjetivo);                     //si encuentra una buena toma nota de esta distancia
                        mejorAceleracion = speed;                                                                       //de la fuerzas que uso, Fx
                        mejorGiro = giro;                                                                       //tambien Fy
                        print("RAZONAMIENTO: Una posible acción es ejercer una speed=" + mejorAceleracion + " y giro= " + mejorGiro + " se alcanzaría una distancia de " + prediccionDistancia);
                    }
                }
            }                                                                                                     //FIN DEL RAZONAMIENTO PREVIO
            if ((mejorAceleracion == 2000) && (mejorGiro == 2000)) { 
                texto.text = "NO SE LANZÓ LA PELOTA: La distancia de "+distanciaObjetivo+" m no se ha alcanzado muchas veces.";
                print(texto.text);
            }
            else
            {
                r.velocity = new Vector3(0, 0, 0);
                transform.position = posicionActual;
                transform.rotation = rotacionActual;

                wheelController.speed = mejorAceleracion;
                wheelController.turn = mejorGiro;

                print("DECISION REALIZADA: Se lanzó pelota con speed =" + mejorAceleracion + " y giro= " + mejorGiro);
                ESTADO = "Acción realizada";
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
