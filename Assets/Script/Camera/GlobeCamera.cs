using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.WorldGeneration;

public class GlobeCamera : MonoBehaviour
{
    public float cameraRadius = 300;
    public float scrollSpeed = 1000;
    public float speed = 100;

    public Lens lens;

    public enum Lens { plain, matte, winds, currents, biomes, temperature, precipitation, plates } // Add more later.

    void FixedUpdate()
    {
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

    Lens oldLens;
    void Update ()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cameraRadius += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
            this.gameObject.transform.position = this.gameObject.transform.position.normalized * cameraRadius;
        }

        if (lens != oldLens)
        {
            oldLens = lens;

            RaycastHit hit;
            Ray ray = new Ray(this.transform.position, this.transform.forward);

            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                GameObject objectHit = hit.transform.gameObject;
                objectHit.transform.parent.gameObject.GetComponent<HexSphereGenerator>().ChangeLenses();
            }
        }
    }

    public void ChangeLens(int i)
    {
        lens = (Lens)i;
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
