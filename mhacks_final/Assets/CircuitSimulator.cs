using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

// NOTE: only the x coordinate matters when checking for connectivity
// NOTE: all voltage pins should have an x coordinate of -1
// NOTE: all ground pins should have an x coordinate of -2
// pin locations (i.e. coordinates) represent nodes in graph
public class Node
{
    // member variables
    public int x; // matters a lot for connectivity
    public bool voltage;
    public bool ground;

    // Node equality operation
    public bool Equals(Node other)
    {
        return this.x == other.x && this.voltage == other.voltage && this.ground == other.ground;
    }
};

public enum ComponentType
{
    RESISTOR,
    LED,
    WIRE
}

// components (i.e. LED, resistor, wire) represent edges
public class Edge
{
    public Node source;
    public Node target;
    // the type of the component - either resistor or LED for the time being
    public ComponentType component;
    public int value;
    // TODO: do we need this? might be helpful when constructing the path
    //public int id;
};

class NodeComparer : IEqualityComparer<Node>
{
    public bool Equals(Node? n1, Node? n2)
    {
        if (ReferenceEquals(n1, n2))
            return true;

        if (n2 is null || n1 is null)
            return false;

        return n1.x == n2.x;
    }

    public int GetHashCode(Node node) => node.x + 2; // TODO: to change
};

public class CircuitSimulator : MonoBehaviour
{
    // Start is called before the first frame update

    List<GameObject> wires = new List<GameObject>();

    List<GameObject> resistors = new List<GameObject>();

    List<GameObject> leds = new List<GameObject>();

    public BreadBoardHandler boardHandler;

    // adjacent list to represent the breadboard connections
    static NodeComparer comparer = new();

    Dictionary<Node, List<Node>> graph = new(comparer);

    // a list of all components on the breadboard
    List<Edge> components = new List<Edge>();

    // function that updates the graph based on the edges
    void update_graph()
    {
        
        // TODO: reset the graph??? - or constantly update every time?
        // iterate through all of the edges and add connections for each node
        for (int i = 0; i < components.Count; ++i)
        {
            // access the component from components
            Edge component = components[i];


            // append component.target into graph[component.source] into the adjacent nodes list

            // if a list already exists for source node, then append the target node into the list
            if (graph.ContainsKey(component.source))
            {
                graph[component.source].Add(component.target);
            }
            else
            {
                // if it's a new list, then initialize it with the target node
                List<Node> holder = new List<Node>();
                holder.Add(component.target);  //potential error- with deep copies  
                graph.Add(component.source, holder);
            }
        }

        // print out the adjacency list
        foreach (KeyValuePair<Node, List<Node>> entry in graph)
        {
            Node source_node = entry.Key;
            Debug.Log("Source.x: " + source_node.x);
            Debug.Log("Source.voltage " + source_node.voltage);
            Debug.Log("Source.ground " + source_node.ground);
            foreach (Node target_node in entry.Value)
            {
                Debug.Log("Target.x: " + target_node.x);
            }
        }
    }
    // returns a list of all possible paths
    List<List<Edge>> get_valid_paths()
    {
        List<List<Edge>> all_paths = new List<List<Edge>>();
        // perform dfs on all possible paths starting at voltage
        foreach(KeyValuePair<Node, List<Node>> entry in graph)
        {
            Node start_node = entry.Key;
            //Debug.Log("Node started at (1): " + start_node.x);
            if (start_node.voltage)
            {
                //Debug.Log("Node started at (1.5): " + start_node.x);
                List<List<Edge>> paths = dfs(start_node);
         
                for (int i = 0; i < paths.Count; ++i)
                {
                    // TODO: determine if we need to deep copy this array
                    all_paths.Add(paths[i]);
                }
            }
        }

        return all_paths;
    }

    // perform dfs based on starting node and only return paths that reach the ground node
    List<List<Edge>> dfs(Node start_node)
    {

        //Debug.Log("Node started at (2): " + start_node.x);
        List<List<Edge>> paths = new List<List<Edge>>();

        Queue<Node> nodes = new Queue<Node>();
        nodes.Enqueue(start_node);

        Dictionary<Node, bool> visited = new Dictionary<Node, bool>();

        // maps child node -> parent node. Important for constructing the path back
        Dictionary<Node, Node> child_parent = new Dictionary<Node, Node>();

        // while there's still more nodes to search through
        while (nodes.Count != 0)
        {
            Node node = nodes.Dequeue();

            // if it has already been visited, check out another node
            if (visited.ContainsKey(node))
            {
                continue;
            }
            // add it to the set of visited nodes
            visited.Add(node, true);

            // if the node is equal to the ground node, then it's a valid path
            if (node.ground)
            {
                // this is a valid path! insert into paths to be returned
                List<Node> reversed_path = new List<Node>();

                // add the ground node as the last node
                reversed_path.Add(node);

                // node = last node
                Node previous_node = child_parent[node];

                // while it's not reached the start of the path
                while (previous_node.voltage == false)
                {
                    reversed_path.Add(previous_node);

                    // move the previous node back
                    previous_node = child_parent[previous_node];
                }

                // add the first node (the voltage path) now
                reversed_path.Add(previous_node);

                // reverse the path and append it to paths
                reversed_path.Reverse();

                // convert the path of Nodes to a path of Edges (i.e. pins to components)
                List<Edge> path_of_components = nodes_to_edges(reversed_path);

                // TODO: debugging print the size of the components
                Debug.Log(path_of_components.Count);

                foreach (Edge e in path_of_components)
                {
                    Debug.Log("Source.x: " + e.source.x);
                    Debug.Log("Target.x: " + e.target.x);
                }

                // TODO: determine if this deep copies the path
                paths.Add(path_of_components);

                // continue search for remaining nodes in queue
                continue;
            }

            // add all the children (based on graph) into the queue
            if (graph.ContainsKey(node))
            {
                for (int index = 0; index < graph[node].Count; ++index)
                {
                 
                    Node n = graph[node][index];

                    // reduce size of the queue when possible
                    if (visited.ContainsKey(n))
                    {
                        continue;
                    }
                    nodes.Enqueue(n);
                    // update the child -> parent map
                    child_parent.Add(n, node);
                }
            }
        }

        // TODO: make the following check a function
        // at the end, remove all paths that are just from voltage to ground (which represents shorted circuit)

        return paths;
    }

    // helper function for dfs()
    List<Edge> nodes_to_edges(List<Node> nodes)
    {
        List<Edge> path_components = new List<Edge>();

        // find the edge whose nodes are equal to source and target
        for (int i = 0; i < nodes.Count - 1; ++i)
        {
            // update source & target nodes for next iteration
            Node source = nodes[i];
            Node target = nodes[i + 1];

            Debug.Log("Trying to find: ");
            Debug.Log("Source.x: " + source.x);
            Debug.Log("Target.x: " + target.x);

            // find the edge whose nodes are equal to source and target
            for (int index = 0; index < components.Count; ++index)
            {
                Edge component = components[index];

                Debug.Log("One edge");
                Debug.Log("source: " + component.source.x);
                Debug.Log("target: " + component.target.x);


                if (component.source.Equals(source) && component.target.Equals(target))
                {
                    path_components.Add(component);
                    Debug.Log("Found edge source: " + component.source.x);
                    Debug.Log("Found edge target: " + component.target.x);
                    break;
                }
            }

        }

        return path_components;
    }

    public void addToWires(GameObject g)
    {
        wires.Add(g);
        Debug.Log(g.name);
    }

    public void addToLEDs(GameObject g)
    {
        leds.Add(g);
        Debug.Log(g.name);
    }

    public void addToResistors(GameObject g)
    {
        resistors.Add(g);
        Debug.Log(g.name);
    }

    private int nameTranslation(string s)
    {
        s = s.Substring(s.Length - 2, 2);
        return Int32.Parse(s);
    }

    private Edge determineEdge(GameObject firstPin, GameObject secondPin, bool reversible, ComponentType c)
    {
        Edge e = new Edge();
        e.component = c;
        Debug.Log("Component type: " + c);

        e.value = 0;
        if (e.component == ComponentType.LED)
        {
            e.value = 50;
            //e.g = 
        }
        else if (e.component == ComponentType.RESISTOR) e.value = 100;



        Node target = new Node();
        Node source = new Node();

        source.voltage = false;
        source.ground = false;
        target.voltage = false;
        target.ground = false;

        target.x = nameTranslation(secondPin.name);
        source.x = nameTranslation(firstPin.name);


        string name = firstPin.name;
        if (name.Substring(1, 1) == "+")
        {
            source.voltage = true;
            source.x = -1;
        }
        else if (name.Substring(1, 1) == "-")
        {
            source.ground = true;
            source.x = -2;
        }

        name = secondPin.name;
        if (name.Substring(1, 1) == "+")
        {
            target.voltage = true;
            target.x = -1;
        }
        else if (name.Substring(1, 1) == "-")
        {
            target.ground = true;
            target.x = -2;
        }

   

        if (!reversible)
        {
            e.target = target;
            e.source = source;
        }
        else
        {
            e.source = target;
            e.target = source;
        }

        return e;
    }

    // TODO: error checking if both ends of one component is in the same row
    public void simulate()
    {
        //firstpin should be positive, secondpin should be negative, when polarity is assumed
        components.Clear();
        graph.Clear();


        foreach (GameObject g in wires)
        {
            if (g.GetComponent<WireManager>().firstPin != null && g.GetComponent<WireManager>().secondPin != null)
            {
                GameObject firstPin = g.GetComponent<WireManager>().firstPin;
                GameObject secondPin = g.GetComponent<WireManager>().secondPin;

                Edge e = determineEdge(firstPin, secondPin, false, ComponentType.WIRE);
                components.Add(e);
                Edge e2 = determineEdge(firstPin, secondPin, true, ComponentType.WIRE); //"reversed"
                components.Add(e2);
            }
        }

        foreach (GameObject g in leds)
        {
            if (g.GetComponent<LEDManager>().currentPinNeg != null && g.GetComponent<LEDManager>().currentPinPos != null)
            {
                GameObject firstPin = g.GetComponent<LEDManager>().currentPinPos;
                GameObject secondPin = g.GetComponent<LEDManager>().currentPinNeg;

             

                Edge e = determineEdge(firstPin, secondPin, false, ComponentType.LED);
                components.Add(e);
            }
        }

        foreach (GameObject g in resistors)
        {
            if (g.GetComponent<ResistorManager>().currentPinNeg != null && g.GetComponent<ResistorManager>().currentPinPos != null)
            {
                GameObject firstPin = g.GetComponent<ResistorManager>().currentPinPos;
                GameObject secondPin = g.GetComponent<ResistorManager>().currentPinNeg;

                Edge e = determineEdge(firstPin, secondPin, false, ComponentType.RESISTOR);
                components.Add(e);
                Edge e2 = determineEdge(firstPin, secondPin, true, ComponentType.RESISTOR); //"reversed"
                components.Add(e2);
            }
        }

        update_graph();
        List<List<Edge>> overall  = get_valid_paths();

        // print all possible paths
        foreach(List<Edge> path in overall)
        {
            Debug.Log("Path start");
            foreach (Edge e in path)
            {
                Debug.Log("Source.x: " + e.source.x);
                Debug.Log("Target.x: " + e.target.x);
            
                foreach (GameObject g in leds)
                {
                    if (g.GetComponent<LEDManager>().currentPinNeg != null && g.GetComponent<LEDManager>().currentPinPos != null)
                    {
                        // check if it exists in the path
                        g.GetComponent<LEDManager>().changeLight(true);
                    }
                }
            }
            Debug.Log("Path end");
        }
    }

    public void turnOff()
    {
        foreach (GameObject g in leds)
        {
            g.GetComponent<LEDManager>().changeLight(false);
        }
    }
    // external function to interface with Breadboard handler
    double get_component_current(List<List<Edge>> valid_paths, Edge component)
    {
        // the path that the component exists in
        List<Edge> path = null;

        // find the component in the path
        foreach (List<Edge> valid_path in valid_paths)
        {
            bool component_exists = false;
            foreach (Edge edge in valid_path)
            {
                if (component.Equals(edge))
                {
                    component_exists = true;
                }
            }
            // TODO: check if this deep copies
            if (component_exists)
            {
                path = valid_path;
            }
        }

        // call function from breadboard handler that calculates the path's current
        double current = boardHandler.get_path_current(path);
        return current;
    }
}
