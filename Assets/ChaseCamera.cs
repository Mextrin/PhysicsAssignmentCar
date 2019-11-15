using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;

    private void Start()
    {
        transform.position = target.position + offset;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = target.position + target.forward * offset.z + transform.up * offset.y;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * Vector3.Distance(transform.position, targetPosition));

        transform.LookAt(target.position);
    }
}
