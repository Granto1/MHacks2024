using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// for the vector data structure
using System.Numerics;

// NOTE: only the x coordinate matters when checking for connectivity
// NOTE: all voltage pins should have an x coordinate of -1
// NOTE: all ground pins should have an x coordinate of -2
// pin locations (i.e. coordinates) represent nodes in graph
public class Node
{
    // TODO: member variables
    int x; // matters a lot for connectivity
    bool voltage;
    bool ground;

    // TODO: Node copy constructor
    // Node equality operation
    public override bool Equals(Node lhs, Node rhs)
    {
        return lhs.x == rhs.x && lhs.voltage == rhs.voltage && lhs.ground == rhs.ground;
    }
};

enum ComponentType
{
    RESISTOR,
    LED,
    WIRE
}

// components (i.e. LED, resistor, wire) represent edges
public class Edge
{
    Node source;
    Node target;
    // the type of the component - either resistor or LED for the time being
    ComponentType component;
    int value;
    // TODO: do we need this? might be helpful when constructing the path
    int id;

    // TODO: Edge copy constructor
};

// TODO: error checking if both ends of one component is in the same row

public class Breadboard : MonoBehaviour
{
    // adjacent list to represent the breadboard connections
    Dictionary<Node, List<Node>> graph = new Dictionary<Node, List<Node>>();

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
            if (graph.ContainsValue(component.source))
            {
                graph[component.source].Add(component.target);
            }
            else
            {
                // if it's a new list, then initialize it with the target node
                graph.Add(component.source, new List<Node>(component.target));
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ;
    }

    // Update is called once per frame
    void Update()
    {
        ;
    }

    // returns a list of all possible paths
    List<List<Edge>> get_valid_paths()
    {
        List<List<Edge>> all_paths = new List<List<Edge>>();
        // perform dfs on all possible paths starting at voltage
        // TODO: iterate through all nodes in graph
        for (int index = 0; index < graph.Count; ++index)
        {
            Node node = graph[index];
            if (node.voltage)
            {
                List<List<Edge>> paths = dfs(node);
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
            if (visited.ContainsValue(node))
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
                while (!previous_node.voltage)
                {
                    reversed_path.Add(previous_node);

                    // move the previous node back
                    previous_node = child_parent[node];
                }

                // add the first node (the voltage path) now
                reversed_path.Add(previous_node);

                // reverse the path and append it to paths
                reversed_path.Reverse();

                // convert the path of Nodes to a path of Edges (i.e. pins to components)
                List<Edge> path_of_components = pins_to_components(reversed_path);

                // TODO: determine if this deep copies the path
                paths.Add(path_of_components);

                // continue search for remaining nodes in queue
                continue;
            }

            // add all the children (based on graph) into the queue
            for (int index = 0; index < graph[node].Count; ++index)
            {
                Node n = graph[node][index];
                nodes.Enqueue(n);
                // update the child -> parent map
                child_parent.Add(n, node);
            }
        }

        // TODO: make the following check a function
        // at the end, remove all paths that are just from voltage to ground (which represents shorted circuit)
    }

    // helper function for dfs()
    List<Edge> nodes_to_edges(List<Node> nodes)
    {
        List<Edge> components = new List<Edge>();

        Node source = nodes[0];
        Node target = nodes[1];

        // TODO: compute the first edge for [source, target]
        // find the edge whose nodes are equal to source and target
        for (int index = 0; index < components.Count; ++index)
        {
            Edge component = components[index];

            if (component.source.Equals(component.target))
            {
                components.Add(component);
                break;
            }
        }

        for (int i = 1; i < nodes.Count; ++i)
        {
            // update source & target nodes for next iteration
            source = nodes[i];
            target = nodes[i + 1];

            // find the edge whose nodes are equal to source and target
            for (int index = 0; index < components.Count; ++index)
            {
                Edge component = components[index];

                if (component.source.Equals(component.target))
                {
                    components.Add(component);
                    break;
                }
            }
        }

        return components;
    }
}
