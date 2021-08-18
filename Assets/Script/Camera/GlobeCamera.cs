using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeCamera : MonoBehaviour
{
    public float cameraRadius = 300;
    public float scrollSpeed = 1000;
    public float speed = 100;

    void FixedUpdate()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cameraRadius += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
            this.gameObject.transform.position = this.gameObject.transform.position.normalized * cameraRadius;
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            RaycastHit hit;
            Ray ray = new Ray(this.transform.position, this.transform.forward);

            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                GameObject objectHit = hit.transform.gameObject;

                this.gameObject.transform.position += Input.GetAxis("Horizontal") * FindRelativeAxes(this.transform.position)[2] * speed * Time.deltaTime;
                this.gameObject.transform.position = this.gameObject.transform.position.normalized * cameraRadius;
                this.gameObject.transform.LookAt(objectHit.transform);
            }
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            RaycastHit hit;
            Ray ray = new Ray(this.transform.position, this.transform.forward);

            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                Vector3 currentUp = FindRelativeAxes(this.transform.position)[1];

                Vector3 potentialPosition = FindRelativeAxes((this.gameObject.transform.position + -Input.GetAxis("Vertical") * currentUp * speed * Time.deltaTime).normalized * cameraRadius)[1];

                if (1 - potentialPosition.y > 1.05f && -Input.GetAxis("Vertical") < 0)
                {
                    GameObject objectHit = hit.transform.gameObject;

                    this.gameObject.transform.position += -Input.GetAxis("Vertical") * currentUp * speed * Time.deltaTime;
                    this.gameObject.transform.position = this.gameObject.transform.position.normalized * cameraRadius;
                    this.gameObject.transform.LookAt(objectHit.transform);
                }
                else if (potentialPosition.y - 1 < -1.05f && -Input.GetAxis("Vertical") > 0)
                {
                    GameObject objectHit = hit.transform.gameObject;

                    this.gameObject.transform.position += -Input.GetAxis("Vertical") * currentUp * speed * Time.deltaTime;
                    this.gameObject.transform.position = this.gameObject.transform.position.normalized * cameraRadius;
                    this.gameObject.transform.LookAt(objectHit.transform);
                }
            }
        }
    }

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

                    h.neighbors[0].color = Color.red;
                    h.neighbors[1].color = Color.yellow;
                    h.neighbors[2].color = Color.green;
                    h.neighbors[3].color = Color.blue;
                    h.neighbors[4].color = Color.cyan;

                    if (!h.pent)
                    {
                        h.neighbors[5].color = Color.white;
                    }

                    objectHit.GetComponent<HexChunk>().UpdateColors(true);
                }
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000.0f, Color.red, 10.0f);
                Debug.Log("Ray didn't hit anything.");
            }
        }
    }

    public static Vector3[] FindRelativeAxes(Vector3 v)
    {
        Vector3 forward = v.normalized;

        Vector3 up;

        if (forward == Vector3.forward || forward == Vector3.right)
        {
            up = Vector3.up;
        }
        else if (forward == Vector3.back || forward == Vector3.left)
        {
            up = Vector3.down;
        }
        else if (forward == Vector3.down)
        {
            up = Vector3.right;
        }
        else if (forward == Vector3.up)
        {
            up = Vector3.left;
        }
        else
        {
            up = Vector3.Project(Vector3.up, forward);
            up -= Vector3.up;
            up = up.normalized;
        }

        Vector3 right = -Vector3.Cross(forward, up).normalized;

        return new Vector3[] { forward, up, right };
    }
}
