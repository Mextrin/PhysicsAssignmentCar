using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bodywork : MonoBehaviour
{
    private void Awake()
    {
        Wheel[] wheels = transform.parent.GetComponentsInChildren<Wheel>();

        for (int i = 0; i < wheels.Length; i++)
        {
            Physics.IgnoreCollision(GetComponentInChildren<Collider>(), wheels[i].GetComponentInChildren<Collider>(), true);
        }
    }
}
