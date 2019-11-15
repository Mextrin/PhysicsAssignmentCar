using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    Rigidbody body;
    public Transform bodywork;

    [Header("Engine")]
    public float RPM;
    public float maxRPM;
    public AnimationCurve torqueCurve = new AnimationCurve(new Keyframe(1000, 280), new Keyframe(4500, 470), new Keyframe(6000, 280));

    enum DriveTrain { FrontWheelDrive, RearWheelDrive, AllWheelDrive };
    enum Gearbox { Manual, Automatic }
    [Header("Transmission")]

    [SerializeField] DriveTrain driveTrain;
    [SerializeField] Gearbox gearbox;
    [SerializeField] float[] gearRatios;
    [SerializeField] float finalDriveRatio;
    public int currentGear = 0;

    [Header("Braking")]
    public float brakePressure;

    [Header("Turning")]
    public float maxSteeringAngle = 30;
    
    [Header("Other")]
    public float frontArea;
    public float speed;

    Wheel[] wheels;


    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<Wheel>();
    }

    [ContextMenu("Push Car")]
    public void Push()
    {
        body.AddForce(transform.InverseTransformVector(new Vector3(-150 / 3.6f, 0, 0)), ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Input
        float throttleInput = Input.GetAxis("Throttle");
        float brakeInput = Input.GetAxis("Brake");
        float steeringInput = Input.GetAxis("Steering");

        Vector3 velocity = body.velocity;
        Vector3 angVelocity = body.angularVelocity;

        //Gearing
        if (Input.GetKeyDown(KeyCode.E) && currentGear < gearRatios.Length - 1) currentGear++;
        if (Input.GetKeyDown(KeyCode.Q) && currentGear > 0) currentGear--;

        if (gearbox == Gearbox.Automatic)
        {
            if (RPM >= maxRPM && currentGear < gearRatios.Length - 1) currentGear++;
            if (RPM <= 1500 && currentGear > 0) currentGear--;
        }

        if (Input.GetKeyDown(KeyCode.R)) transform.rotation = Quaternion.identity;

        //Aero
        float CDrag = (0.5f * 0.3f * frontArea * 1.29f);
        float CRoll = 30 * CDrag;
        Vector3 FDrag = -CDrag * velocity * velocity.magnitude;
        Vector3 FRoll = -CRoll * velocity;

        //Braking
        float TBrake = brakePressure * brakeInput * -Mathf.Sign(speed);

        //Engine
        float avgAngularVelocity = 0;
        for (int i = 0; i < wheels.Length; i++)
        {
            avgAngularVelocity += wheels[i].angularVelocity;
        }
        avgAngularVelocity /= wheels.Length;


        RPM = (avgAngularVelocity * gearRatios[currentGear] * finalDriveRatio * 60) / (2 * Mathf.PI);
        if (RPM < 1000) RPM = 1000;
        if (RPM > maxRPM) throttleInput = 0;

        float TMax = torqueCurve.Evaluate(RPM);
        float TEngine = 2 * TMax * throttleInput;
        float TDrive = TEngine * gearRatios[currentGear] * finalDriveRatio * 0.7f;
        print(TDrive);

        //Apply
        for (int i = 0; i < wheels.Length; i++)
        {
            float FDrive = TDrive * wheels[i].radius;

            if (i < 2)
            {
                wheels[i].turnAngle = steeringInput * maxSteeringAngle;
            }
            else
            {
                body.AddForceAtPosition(transform.forward * FDrive, wheels[i].transform.position);
            }

            float wheelLongVelocity = Vector3.Dot(wheels[i].transform.forward, wheels[i].velocity / Time.fixedDeltaTime);
            float wheelLatVelocity = Vector3.Dot(wheels[i].transform.right, wheels[i].velocity / Time.fixedDeltaTime);
            //print(wheels[i].gameObject.name + " | " + wheels[i].GetComponent<Rigidbody>().velocity);

            //Debug.DrawLine(transform.position, (wheels[i].velocity * 10) + transform.position, Color.blue);
            //Debug.DrawLine(wheels[i].transform.position, wheels[i].transform.position + (wheels[i].transform.right * 10), Color.red);
            body.AddForceAtPosition(-wheelLatVelocity * wheels[i].transform.right * 1000, wheels[i].transform.position);
            body.AddForceAtPosition(TBrake * wheels[i].transform.forward, wheels[i].transform.position);

            wheels[i].angularVelocity = Mathf.Rad2Deg * Vector3.Dot(wheels[i].transform.forward, wheels[i].velocity) * Mathf.PI;
        }

        speed = transform.InverseTransformVector(velocity).z * 3.6f;
    }
}
