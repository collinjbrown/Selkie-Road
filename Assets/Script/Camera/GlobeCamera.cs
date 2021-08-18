using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeCamera : MonoBehaviour
{
    public float speed = 100;

    void Update ()
    {
        this.transform.LookAt(new Vector3(0, 0, 0));

        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000.0f, Color.red, 10.0f);
                GameObject objectHit = hit.transform.gameObject;

                if (objectHit.GetComponent<HexChunk>() != null)
                {
                    Hex h = objectHit.GetComponent<HexChunk>().ListNearestHex(hit.point);

                    this.gameObject.transform.GetChild(0).gameObject.GetComponent<LineRenderer>().SetPosition(0, objectHit.transform.position);
                    this.gameObject.transform.GetChild(0).gameObject.GetComponent<LineRenderer>().SetPosition(1, h.center.pos * 2);

                    UnityEngine.Debug.DrawRay(objectHit.transform.position, h.neighbors[0].center.pos * 2, Color.red, Mathf.Infinity, true);
                    UnityEngine.Debug.DrawRay(objectHit.transform.position, h.neighbors[1].center.pos * 2, Color.yellow, Mathf.Infinity, true);
                    UnityEngine.Debug.DrawRay(objectHit.transform.position, h.neighbors[2].center.pos * 2, Color.green, Mathf.Infinity, true);
                    UnityEngine.Debug.DrawRay(objectHit.transform.position, h.neighbors[3].center.pos * 2, Color.blue, Mathf.Infinity, true);
                    UnityEngine.Debug.DrawRay(objectHit.transform.position, h.neighbors[4].center.pos * 2, Color.cyan, Mathf.Infinity, true);
                    UnityEngine.Debug.DrawRay(objectHit.transform.position, h.neighbors[5].center.pos * 2, Random.ColorHSV(), Mathf.Infinity, true);

                    Debug.Log($"Hex Coordinates :: {h.x} / {h.y}.");
                }
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000.0f, Color.red, 10.0f);
                Debug.Log("Ray didn't hit anything.");
            }
        }

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
