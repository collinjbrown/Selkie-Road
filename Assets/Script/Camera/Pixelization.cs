using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pixelization : MonoBehaviour
{
    bool pixelated = true;
    public GameObject imageObject;
    public RenderTexture rawImage;

    public void Pixelize ()
    {
        if (pixelated)
        {
            pixelated = false;
            Camera.main.targetTexture = null;
            imageObject.SetActive(false);
        }
        else
        {
            pixelated = true;
            Camera.main.targetTexture = rawImage;
            imageObject.SetActive(true);
        }
    }


    public List<GameObject> buttons;
    public void HideButtons()
    {
        foreach (GameObject g in buttons)
        {
            if (g.activeInHierarchy)
            {
                g.SetActive(false);
            }
            else
            {
                g.SetActive(true);
            }
        }
    }
}
