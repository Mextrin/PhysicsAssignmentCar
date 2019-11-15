using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Wheel : MonoBehaviour
{
    [Header("Wheel information")]
    public float radius;
    public float frictionCoefficient;

    [Header("Suspsnsion")]
    [SerializeField, Range(0, 1)] float offset = 0f;

    [SerializeField] float stiffness = 10f;
    [SerializeField] float damping = 10f;

    Rigidbody rigidbody;
    Rigidbody rigidbodyCar;

    Vector3 hingePoint;
    Vector3 prevPos;
    public Vector3 velocity;

    [Header("Debug data")]
    public float angularVelocity;   //rad/s
    public float turnAngle;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbodyCar = transform.parent.GetComponent<Rigidbody>();

        hingePoint = transform.localPosition;
        prevPos = transform.parent.TransformPoint(hingePoint);
    }

    void FixedUpdate()
    {
        velocity = transform.parent.TransformPoint(hingePoint) - prevPos;

        transform.localEulerAngles = new Vector3(0, turnAngle, 0);

        foreach (Transform child in transform)
        {
            child.Rotate(Vector3.right, Time.deltaTime * angularVelocity * Mathf.Rad2Deg);
        }

        //Suspension
        Vector3 hingeWorldPos = transform.parent.TransformPoint(hingePoint);
        Vector3 fixedHingeVelocity = (hingeWorldPos - prevPos) / Time.fixedDeltaTime;
        float fixedDeltaLength = Vector3.Dot(transform.up, transform.position - hingeWorldPos) + offset;
        float fixedHingeDeltaVelocity = Vector3.Dot(transform.up, fixedHingeVelocity - rigidbody.velocity);

        float hingeVelocity = (hingeWorldPos.y - prevPos.y) / Time.fixedDeltaTime;
        float deltaLength = (transform.position.y - hingeWorldPos.y);
        float blockDeltaVelocity = hingeVelocity - rigidbody.velocity.y;

        float spring = (fixedDeltaLength * stiffness);
        float damper = (fixedHingeDeltaVelocity * damping);
        float force = spring - damper;

        Debug.DrawLine(transform.position, transform.position + fixedHingeVelocity, Color.red);

        //float force = (deltaLength * stiffness) - (blockDeltaVelocity * damping);
        //print(blockDeltaVelocity - blockVelocity);
        //rigidbody.AddForce(transform.up * force * stiffness);
        //

        rigidbodyCar.AddForceAtPosition(transform.up * force, hingeWorldPos);
        GetComponent<Rigidbody>().AddForce(-transform.up * force / 100);

        transform.localPosition = new Vector3(hingePoint.x, transform.localPosition.y, hingePoint.z);
        prevPos = hingeWorldPos;
    }
}
