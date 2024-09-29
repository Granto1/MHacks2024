using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Unity.XR.CoreUtils;

public class ResistorManager : MonoBehaviour  //ENABLE CONTROLLER ROTATION
{
    // Start is called before the first frame update

    private XRGrabInteractable grabInteractable;
    public GameObject currentPinNeg;
    public GameObject currentPinPos;

    public BoxCollider neg;
    public BoxCollider pos;

    public GameObject NegLeg;
    public GameObject PosLeg;

    public Material black;
    public Material red;
    public Material good;

    public CircuitSimulator s;

    public bool positive;
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(onGrab);
        grabInteractable.selectExited.AddListener(onDrop);

        XROrigin g = FindAnyObjectByType<XROrigin>();
        s = g.GetComponent<CircuitSimulator>();
        s.addToResistors(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void onGrab(SelectEnterEventArgs args)
    {
        //wire.GetComponent<WireManager>().setMoving(true);
    }

    private float GetTotalHeight()
    {
        GameObject obj = gameObject;

        // Create a Bounds object to hold the combined bounds
        Bounds totalBounds = new Bounds(obj.transform.position, Vector3.zero);

        // Get all renderers in the GameObject and its children
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        // Expand the totalBounds to include the bounds of each renderer
        foreach (Renderer renderer in renderers)
        {
            totalBounds.Encapsulate(renderer.bounds);
        }

        // Return the Y size (height) of the total bounds
        return totalBounds.size.y;
    }

    private void onDrop(SelectExitEventArgs args)
    {
        if (currentPinNeg != null && currentPinPos != null)
        {
            // Calculate the midpoint between currentPinLeft and currentPinRight
            Vector3 leftPosition = currentPinNeg.transform.position;
            Vector3 rightPosition = currentPinPos.transform.position;
            Vector3 midpoint = (leftPosition + rightPosition) / 2;

            transform.position = new Vector3(midpoint.x, midpoint.y + GetTotalHeight()*2/3, midpoint.z);

            // Calculate the direction from left to right to align with the line
            Vector3 direction = (rightPosition - leftPosition).normalized;

            // Calculate the rotation to face up
            //Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            transform.up = Vector3.up;
            transform.right = direction;


        }
        //lock the legs, then lock the base
    }

    public void setPin(GameObject g, BoxCollider b)
    {
        if (b == neg)
        {
            if (g == null) NegLeg.GetComponent<MeshRenderer>().material = black;
            else NegLeg.GetComponent<MeshRenderer>().material = good;
            currentPinNeg = g;
        }
        else
        {
            if (g == null) PosLeg.GetComponent<MeshRenderer>().material = red;
            else PosLeg.GetComponent<MeshRenderer>().material = good;
            currentPinPos = g;
        }
    }
}
