using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BallManager : MonoBehaviour
{
    // Start is called before the first frame update

    private XRGrabInteractable grabInteractable;
    private GameObject wire;
    private GameObject currentPin;

    public Material good;
    public Material bad;

    public bool positive;

    // public FUCKING DEFINE IN TERMS OF RED+BLACK, RED IS POSTIVE
    void Start()
    {
        bad = GetComponent<MeshRenderer>().material;
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(onGrab);
        grabInteractable.selectExited.AddListener(onDrop);

        wire = transform.parent.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void onGrab(SelectEnterEventArgs args)
    {
        wire.GetComponent<WireManager>().setMoving(true);
    }

    private void onDrop(SelectExitEventArgs args)
    {
        wire.GetComponent<WireManager>().move();
        /*if (currentPin != null)
        {
            transform.position = new Vector3
            (
                transform.position.x,
                transform.position.y+0.4f,
                transform.position.z
            );
        }*/
        wire.GetComponent<WireManager>().setMoving(false);
    }

    public void setPin(GameObject g)
    {
        if (g != null)
        {
            GetComponent<MeshRenderer>().material = good;
        }
        else GetComponent<MeshRenderer>().material = bad;
        wire.GetComponent<WireManager>().setPin(g, positive); // to do: prevent fake exiting
        currentPin = g;
    }
}
