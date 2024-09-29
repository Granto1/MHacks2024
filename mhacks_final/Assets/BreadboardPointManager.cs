using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadboardPointManager : MonoBehaviour
{

    private SphereCollider s;
    // Start is called before the first frame update
    void Start()
    {
        s = GetComponent<SphereCollider>();
        s.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.name);
        Debug.Log(other.gameObject.tag);

        if (other.gameObject.tag == "Wire")
        {
            BallManager c = other.GetComponent<BallManager>();
            c.setPin(gameObject);
        }
        else if (other.gameObject.tag == "LED")
        {
            LEDManager led = other.transform.parent.parent.GetComponent<LEDManager>();
            led.setPin(gameObject, (BoxCollider)other);
        }
        else if (other.gameObject.tag == "Resistor")
        {
            ResistorManager res = other.transform.parent.parent.GetComponent<ResistorManager>();
            res.setPin(gameObject, (BoxCollider)other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log(gameObject.name);
        //Debug.Log(other.gameObject.tag);

        if (other.gameObject.tag == "Wire")
        {
            BallManager c = other.GetComponent<BallManager>();
            c.setPin(null);
        }
        else if (other.gameObject.tag == "LED")
        {
            LEDManager led = other.transform.parent.parent.GetComponent<LEDManager>();
            led.setPin(null, (BoxCollider)other);
        }
        else if (other.gameObject.tag == "Resistor")
        {
            ResistorManager res = other.transform.parent.parent.GetComponent<ResistorManager>();
            res.setPin(null, (BoxCollider)other);
        }
    }
}
