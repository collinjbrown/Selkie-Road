using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.WorldGeneration;

public class GlobeCamera : MonoBehaviour
{
    public float cameraRadius = 300;
    public float scrollSpeed = 1000;
    public float speed = 100;

    public float lookMod = 1; // Between 0 and 1, how much we should tilt towards relative up.

    public Lens lens;

    public enum Lens { plain, matte, winds, currents, biomes, temperature, precipitation, plates } // Add more later.

    public GameObject target;

    void FixedUpdate()
    {
        Vector3 relativeUp = FindRelativeAxes(this.transform.position)[1];

        if (target == null)
        {

            RaycastHit hit;
            Ray ray = new Ray(this.transform.position, this.transform.forward);

            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                target = hit.transform.gameObject;
            }
        }

        if (Input.GetAxis("Horizontal") != 0 && target != null)
        {
            Vector3 currentUp = FindRelativeAxes(this.transform.position)[1];

            Vector3 potentialPosition = FindRelativeAxes((this.gameObject.transform.position + -Input.GetAxis("Horizontal") * currentUp * speed * cameraRadius * Time.deltaTime).normalized * cameraRadius)[1];

            float tempMod = lookMod;
            float camMod = cameraRadius;

            if (potentialPosition.y - 1 > -2f && this.transform.position.y < target.transform.position.y)
            {
                float distFromPole = -(potentialPosition.y - 1) - 1; // This should range from 0.8 at the edge to 0 at the center.
                                                                     // We want the tempMod to be 0 at the pole and equal to lookMod...
                                                                     // at the edge of our radius from the pole.

                tempMod = Mathf.Lerp(0, lookMod, distFromPole);

                if (camMod < 150)
                {
                    camMod = Mathf.Lerp(150, cameraRadius, distFromPole);
                }
            }

            this.gameObject.transform.position += Input.GetAxis("Horizontal") * FindRelativeAxes(this.transform.position)[2] * speed * cameraRadius * Time.deltaTime;
            this.gameObject.transform.position = this.gameObject.transform.position.normalized * camMod;
            this.gameObject.transform.LookAt(target.transform.position + (relativeUp * tempMod));

        }
        if (Input.GetAxis("Vertical") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                lookMod += -Input.GetAxis("Vertical") * speed * cameraRadius * Time.deltaTime;

                if (lookMod > 0)
                {
                    lookMod = 0;
                }

                this.gameObject.transform.LookAt(target.transform.position + (relativeUp * lookMod));
            }
            else
            {
                Vector3 currentUp = FindRelativeAxes(this.transform.position)[1];

                Vector3 potentialPosition = FindRelativeAxes((this.gameObject.transform.position + -Input.GetAxis("Vertical") * currentUp * speed * cameraRadius * Time.deltaTime).normalized * cameraRadius)[1];

                float tempMod = lookMod;
                float camMod = cameraRadius;

                if (potentialPosition.y - 1 > -2f && this.transform.position.y < target.transform.position.y)
                {
                    float distFromPole = -(potentialPosition.y - 1) - 1; // This should range from 0.8 at the edge to 0 at the center.
                                                                        // We want the tempMod to be 0 at the pole and equal to lookMod...
                                                                        // at the edge of our radius from the pole.

                    tempMod = Mathf.Lerp(0, lookMod, distFromPole);

                    if (camMod < 150)
                    {
                        camMod = Mathf.Lerp(150, cameraRadius, distFromPole);
                    }
                }

                if (1 - potentialPosition.y > 1.05f && -Input.GetAxis("Vertical") < 0)
                {
                    this.gameObject.transform.position += -Input.GetAxis("Vertical") * currentUp * speed * cameraRadius * Time.deltaTime;
                    this.gameObject.transform.position = this.gameObject.transform.position.normalized * camMod;
                    this.gameObject.transform.LookAt(target.transform.position + (relativeUp * tempMod));
                }
                else if (potentialPosition.y - 1 < -1.05f && -Input.GetAxis("Vertical") > 0)
                {
                    this.gameObject.transform.position += -Input.GetAxis("Vertical") * currentUp * speed * cameraRadius * Time.deltaTime;
                    this.gameObject.transform.position = this.gameObject.transform.position.normalized * camMod;
                    this.gameObject.transform.LookAt(target.transform.position + (relativeUp * tempMod));
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
