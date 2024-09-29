using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WireManager : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    private bool moving = false;
    private bool held = false;

    public GameObject firstPin;
    public GameObject secondPin;

    private GameObject top;
    private GameObject bottom;
    private GameObject cylinder;
    private GameObject topBall;
    private GameObject botBall;

    public CircuitSimulator c;


    void Start()
    {
        XROrigin g = FindAnyObjectByType<XROrigin>();
        c = g.GetComponent<CircuitSimulator>();
        c.addToWires(gameObject);

        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(onGrab);
        grabInteractable.selectExited.AddListener(onDrop);

        cylinder = gameObject;
        topBall = transform.parent.transform.GetChild(1).gameObject;
        botBall = transform.parent.transform.GetChild(2).gameObject;
        top = transform.GetChild(0).gameObject;
        bottom = transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (held)
        {
            topBall.transform.position = new Vector3(top.transform.position.x, top.transform.position.y, top.transform.position.z);
            botBall.transform.position = new Vector3(bottom.transform.position.x, bottom.transform.position.y, bottom.transform.position.z);
        }
        if (moving)
        {
            move();
        }
    }

    public void move()
    {
        Vector3 topPos = topBall.transform.position;
        Vector3 botPos = botBall.transform.position;


        if (firstPin != null)
        {
            topPos = new Vector3(topPos.x, topPos.y + 0.02f, topPos.z);
        }
        if (secondPin != null) 
        {
            botPos = new Vector3(botPos.x, botPos.y + 0.02f, botPos.z);
        }
        // Calculate the direction from the bottom ball to the top ball
        Vector3 direction = (topPos - botPos).normalized;

        // Set the position of the cylinder to the midpoint
        cylinder.transform.position = (topPos + botPos) / 2;

        // Set the cylinder's up direction
        cylinder.transform.up = direction;

        // Calculate the distance between the two balls
        float distance = Vector3.Distance(topPos, botPos);

        // Adjust the cylinder's scale to match the distance
        // Assuming the cylinder's original height is 1 unit
        cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x, 10f*distance, cylinder.transform.localScale.z);
    }

    private void onGrab(SelectEnterEventArgs args)
    {
        held = true;
          

        if (firstPin != null && secondPin != null) 
        {
            /*if (Vector3.Distance(hand.transform.position, top.transform.position) < Vector3.Distance(hand.transform.position, bottom.transform.position))
            {
                secondPin = null;
            }
            else
            {
                firstPin = secondPin;
                secondPin = null;
            }*/
        }
    }

    private void onDrop(SelectExitEventArgs args)
    {
        held = false;
        move();
    }

    public void setPin(GameObject g, Boolean positive)
    {
        if (positive)
        {
            firstPin = g;
        }
        else secondPin = g;
    } 

    public void setMoving(bool move)
    {
        moving = move;
    }
}
