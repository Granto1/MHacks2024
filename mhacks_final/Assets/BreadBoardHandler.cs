using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class element
{
    public enum el 
    {
        resistor,   // 0
        wire,       // 1
        led         // 2
    }

    public el type;

    // node connections
    public int node1;
    public int node2;

    // element value
    public double value;

    public element(int type, int n1, int n2, double value)
    {
        this.type = (el) type;
        node1 = n1;
        node2 = n2;
        this.value = value;
    }

    public bool EQUALS(element other)
    {
        if (other == null)
        {
            return false;
        }
        return parallel(other.node1, other.node2) && (other.type == type) && (other.value == value);
    }

    // TODO: add support for parallel circuits in the future
    public bool parallel(int n1, int n2)
    {
        return ((n1 == node1) && (n2 == node2)) || ((n1 == node2) && (n2 == node1));
    }

    // values through the element
    public double voltDrop = 0;
    public double current = 0;

    public void setVoltDrop(double v)
    {
        voltDrop = v;
    }

    public void setCurrent(double c)
    {
        current = c;
    }

}

// TODO: make the following check a function
// at the end, remove all paths that are just from voltage to ground (which represents shorted circuit) - could just move to circuit handler
public class BreadBoardHandler : MonoBehaviour
{
    // VOLTAGE HANDLING
    public double voltage = 5;
    public double volts = 5;
    public bool onOff = false;

    // TODO: triggered by a physical button press on the environment - link it to the other class!
    // toggle on/off the power generator
    public void toggleVoltage()
    {
        onOff = !onOff;
        voltage = (onOff) ? (volts) : (0);
    }

    // TODO: triggered by physical movement in the environment - link it to the other class!
    // change the volts to be supplied
    public void setVoltage(double v)
    {
        volts = v;
    }

    // breadboard - technically the list of viable circuits
    public List<List<element>> breadboard = new List<List<element>>();

    // power strip
    public List<List<element>> power = new List<List<element>>();
    
    // BREADBOARD HANDLERS

    //// add the element to each of the connection nodes
    //public void addElement(int type, int n1, int n2, double value)
    //{
    //    element newElement = new element((el) type, n1, n2, value);
    //    breadboard[n1].Add(newElement);
    //    breadboard[n2].Add(newElement);
    //}

    //// ensure that the element exists on both ends and then delete it
    //public void removeElement(int type, int n1, int n2, double value)
    //{
    //    int row1 = -1, row2 = -1;
    //    element deletable = new element((el) type, n1, n2, value);
    //    for (int a = 0; a < breadboard[n1].Count; a++)
    //    {
    //        if (breadboard[n1][a].EQUAL(deletable))
    //        {
    //            row1 = a;
    //            break;
    //        }
    //    }
    //    for (int b = 0; b < breadboard[n2].Count; b++)
    //    {
    //        if (breadboard[n2][b].EQUAL(deletable))
    //        {
    //            row2 = b;
    //            break;
    //        }
    //    }
    //    if (row1 != -1 && row2 != -1)
    //    {
    //        breadboard[n1].DeleteAt(row1);
    //        breadboard[n2].DeleteAt(row2);
    //    }
    //}

    // LED HANDLER
    public int LEDHandler(int type, int n1, int n2, double value)
    {
        element tempLED = new element(type, n1, n2, value);
        for (int a = 0; a < breadboard[n1].Count; a++)
        {
            if (tempLED.Equals(breadboard[n1][a]))
            {
                double curr = breadboard[n1][a].current; 
                /*
                    TO-DO
                    Display a message that there is too much current or the diode is the wrong way
                */
                return (int)((curr > 0 && curr <= 0.02) ? (curr / 0.02 * 100) : (0));
            }
        }
        return 0;
    }
    public double get_path_current(List<Edge> path)
    {
        double current = 0;
        double resistance = 0;

        // calculate the total resistance of the circuit
        foreach(Edge element in path)
        {
            resistance += element.value;
        }

        current = voltage / resistance;

        return current;
    }

    public void solveParallel()
    {
        ;
    }


    // Start is called before the first frame update
    void Start()
    {
        ;
    }



    // Update is called once per frame
    void Update()
    {
        // if the oscilloscope is on
      
    }

    // external function to interact with the user - interact with VR component
    // returns { current, resistance } of the component
    /*List<int> get_current_and_resistance(Edge component)
    {
        // external call to different file
        List<List<Edge>> overall = get_valid_paths();
        // another external call to diff file
        double current = get_component_current(overall, component);

        return { current, component.value };
    }*/
}
