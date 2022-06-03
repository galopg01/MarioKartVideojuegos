using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider backRight;
    [SerializeField] WheelCollider backLeft;

    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform backRightTransform;
    [SerializeField] Transform backLeftTransform;

    public GameObject prefabAlaDelta;
    public GameObject prefabParacaidas;

    public float acceleration = 500f;
    public float breakingForce = 300f;
    public float maxTurnAngle = 15f;
    public string estado;

    private float currentAcceleration = 0f;
    private float currentBreakForce = 0f;
    private float currentTurnAngle = 0f;

    public float maxSpeed = 8f;

    public float speed = 0;
    public float turn = 0;

    private Rigidbody rb;
    AprendizGiroDotYVelocidad script;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        script = GetComponent<AprendizGiroDotYVelocidad>();
        estado="Normal";
    }

    private void Update()
    {
        
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }

    private void FixedUpdate()
    {
        if(estado.Equals("Volando")){

        }else{
            if (Input.GetKey(KeyCode.X))
            {
                rb.AddForce(2,0,0, ForceMode.VelocityChange);
            }

            currentAcceleration = acceleration * (script.enabled ? speed : Input.GetAxis("Vertical"));
            //currentAcceleration = acceleration * 1;

            if (Input.GetKeyUp(KeyCode.Space))
                currentBreakForce = breakingForce;
            else
                currentBreakForce = 0f;

            frontRight.motorTorque = currentAcceleration;
            frontLeft.motorTorque = currentAcceleration;

            frontRight.brakeTorque = currentBreakForce;
            frontLeft.brakeTorque = currentBreakForce;
            backLeft.brakeTorque = currentBreakForce;
            backRight.brakeTorque = currentBreakForce;

            currentTurnAngle = maxTurnAngle * (script.enabled ? turn : Input.GetAxis("Horizontal"));
            frontLeft.steerAngle = currentTurnAngle;
            frontRight.steerAngle = currentTurnAngle;

            UpdateWheel(frontRight, frontRightTransform);
            UpdateWheel(frontLeft, frontLeftTransform);
            UpdateWheel(backRight, backRightTransform);
            UpdateWheel(backLeft, backLeftTransform);
        }
    }

    void UpdateWheel(WheelCollider col, Transform trans) 
    {
         Vector3 position;
         Quaternion rotation;
         col.GetWorldPose(out position, out rotation);

         trans.position = position;
         trans.rotation = rotation;
    }

    private void OnTriggerEnter(Collider obj){ 

        if (obj.gameObject.name.Equals("Rampa1")){ 
            if(!estado.Equals("Volando")){
                GameObject alaDelta = Instantiate(prefabAlaDelta, new Vector3(transform.position.x+0.1f,transform.position.y+0.1f,transform.position.z), transform.rotation);
                alaDelta.AddComponent<FixedJoint>();
                alaDelta.GetComponent<FixedJoint>().connectedBody= rb;
                GameObject gm = GameObject.Find("o1");
                alaDelta.GetComponent<ControlMovimiento>().ObjetoPerseguido=GameObject.Find("o1");

                estado="Volando";
            }
        }

        if (obj.gameObject.name.Equals("Rampa2")){ 
            if(!estado.Equals("Volando")){
                GameObject alaDelta = Instantiate(prefabParacaidas, new Vector3(transform.position.x,transform.position.y+0.1f,transform.position.z-0.1f), transform.rotation);
                alaDelta.AddComponent<FixedJoint>();
                alaDelta.GetComponent<FixedJoint>().connectedBody= rb;
                alaDelta.GetComponent<ControlMovimiento>().ObjetoPerseguido=GameObject.Find("o2");

                estado="Volando";
            }
        }
    }
}
