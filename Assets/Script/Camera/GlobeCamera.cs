using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeCamera : MonoBehaviour
{
    public float speed = 100;

    void Update ()
    {
        this.transform.LookAt(new Vector3(0, 0, 0));

        if (Input.GetMouseButton(2))
        {
            this.transform.RotateAround(new Vector3(0,0,0), Vector3.up, Input.GetAxis("Mouse X") * speed * Time.deltaTime);
            this.transform.RotateAround(new Vector3(0, 0, 0), Vector3.forward, Input.GetAxis("Mouse Y") * speed * Time.deltaTime);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            this.gameObject.transform.position += Input.GetAxis("Mouse ScrollWheel") * this.gameObject.transform.position * Time.deltaTime * (speed / 100);
        }
    }
}
