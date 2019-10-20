
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeatNetwork
{
    // public List<Node> nodes;
    public Dictionary<int, Node> nodes;
    public List<Connection> connections;
    // public Dictionary<int, Connection> connections;
    public int input_size;
    public int output_size;

    // private int _innov_counter;
    // public int innov_counter{
    //     get
    //     {
    //         _innov_counter++;
    //         return _innov_counter - 1;
    //     }
    // }

    public float fitness;
    public Species species;
    public float bias;


    public NeatNetwork(int input_size, int output_size, bool fill){
        this.input_size = input_size;
        this.output_size = output_size;
        this.nodes = new Dictionary<int, Node>();
        // this.connections = new Dictionary<int, Connection>();
        this.connections = new List<Connection>();

        if (fill){
            setupNetwork(input_size, output_size);
        }
    }

    public class Node
    {
        public enum Type
        {
            INPUT,
            HIDDEN,
            OUTPUT
        }
        public Type type;
        public int id;

        public float currentValue;

        public Node(Type type, int id){
            this.type = type;
            this.id = id;

            this.currentValue = 0.0f;
        }

    }

    public class Connection
    {
        public int in_node;
        public int out_node;
        public int innov;
        public float weight;
        public bool enabled;

        public Connection(int in_node, int out_node, int innov, float weight, bool enabled){
            this.in_node = in_node;
            this.out_node = out_node;
            this.innov = innov;
            this.weight = weight;
            this.enabled = enabled;
        }
    }

    // public int getInnov(){
    //     return _innov_counter;
    // }

    private void setupNetwork(int input_size, int output_size){
        List<Node> tmpInput = new List<Node>();
        this.nodes = new Dictionary<int, Node>();
        // this.connections = new Dictionary<int, Connection>();
        this.connections = new List<Connection>();
        for (int i = 0; i < input_size; i++){
            Node tmp = new Node(Node.Type.INPUT, i);
            tmpInput.Add(tmp);
            nodes.Add(i, tmp);
        }

        List<Node> tmpOutput = new List<Node>();
        for (int j = input_size; j < input_size + output_size; j++){ //Do output nodes first
            Node tmp = new Node(Node.Type.OUTPUT, j);
            tmpOutput.Add(tmp);
            nodes.Add(j, tmp);
        }

        foreach (Node n in tmpInput){
            foreach (Node n2 in tmpOutput){
                addConnection(n, n2);
            }
        }
        bias = Random.Range(-1.0f, 1.0f);
    }

    // public int getInnovConnectionIndex(int innov){
    //     for (int i = 0; i < connections.Count; i++){
    //         if (connections[i].innov_num == innov){
    //             return i;
    //         }
    //     }
    //     return -1;
    // }

    public bool containsNode(int id){
        foreach(var n in nodes){
            if (n.Value.id == id){
                return true;
            }
        }
        return false;
    }

    public void addConnection(Node node1, Node node2, bool pickRandom = false){
        bool reversed = false;
        if (pickRandom){
            node1 = nodes[Random.Range(0, nodes.Count)];
            node2 = nodes[Random.Range(0, nodes.Count)];
        }

        float weight = Random.Range(-2.0f, 2.0f);


        if (node1.type == Node.Type.HIDDEN && node2.type == Node.Type.INPUT)
            reversed = true;
        else if (node1.type == Node.Type.OUTPUT && node2.type == Node.Type.HIDDEN)
            reversed = true;
        else if (node1.type == Node.Type.OUTPUT && node2.type == Node.Type.INPUT)
            reversed = true;

        bool connection_exist = false;
        // foreach (Connection c in connections){
        foreach (Connection c in connections){
            if (c.in_node == node1.id && c.out_node == node2.id){
                connection_exist = true;
                // break;
                return;
            }
            else if (c.in_node == node2.id && c.out_node == node1.id){
                connection_exist = true;
                // break;
                return;
            }
        }

        int n1 = reversed ? node2.id : node1.id;
        int n2 = reversed ? node1.id : node2.id;

        // Connection newConnection = new Connection(node1.id, node2.id, weight, true, innov_counter);
        Connection newConnection = new Connection(n1, n2, GlobalVars.addPair(new Vector2(n1, n2)), weight, true);
        // connections.Add(innov_counter, newConnection);
        connections.Add(newConnection);

    }

    public void addNode(int connection_innov, bool pickRandom = false){
        Connection target_connection;
        if (pickRandom){
            target_connection = connections[Random.Range(0, connections.Count)];
        }
        else {
            target_connection = connections[connection_innov];
        }
        target_connection.enabled = false;

        Node newNode = new Node(Node.Type.HIDDEN, nodes.Count);
        Connection inToNew = new Connection(target_connection.in_node, newNode.id, GlobalVars.addPair(new Vector2(target_connection.in_node, newNode.id)), 1.0f, true);
        Connection newToOut = new Connection(newNode.id, target_connection.out_node, GlobalVars.addPair(new Vector2(newNode.id, target_connection.out_node)), target_connection.weight, true);

        nodes.Add(newNode.id, newNode);
        // connections.Add(GlobalVars.getInnov(), inToNew);
        // connections.Add(GlobalVars.getInnov(), newToOut);
        connections.Add(inToNew);
        connections.Add(newToOut);

    }

    public void mutate(){
        //90% Chance of perturbed. 10% of new weight
        foreach (Connection c in connections){
            float rand = Random.Range(0.0f, 1.0f);
            float rand_bias = Random.Range(0.0f, 1.0f);
            if (rand > 0.1f){ //Perturb
                float perturb_val = Random.Range(-0.5f, 0.5f);
                c.weight+=perturb_val;
            }
            else { //New weight
                c.weight = Random.Range(-2.0f, 2.0f);
            }
            if (rand_bias > 0.1f){
                float perturb_val = Random.Range(-0.5f, 0.5f);
                bias+=perturb_val;
            }
            else {
                bias = Random.Range(-1.0f, 1.0f);
            }
        }
    }

    private bool containsInnov(int innov){
        foreach (var c in connections){
            if (c.innov == innov){
                return true;
            }
        }
        return false;
    }

    public int getMaxInnov(){
        int max = -1;
        foreach(Connection c in connections){
            if (c.innov > max){
                max = c.innov;
            }
        }

        return max;
    }

    public static int getMaxFitness(NeatNetwork parent1, NeatNetwork parent2){
        if (parent1.fitness > parent2.fitness){
            return 0;
        }
        else if (parent1.fitness < parent2.fitness){
            return 1;
        }
        return -1;
    }

    //Returns Vector3(#matching, #disjoint, #Exess)
    public static Vector3 getGeneCompareCounts(NeatNetwork parent1, NeatNetwork parent2){
        Vector3 rtn = Vector3.zero;


        int p1_max_innov = parent1.getMaxInnov();
        int p2_max_innov = parent2.getMaxInnov();

        foreach (var p in GlobalVars.pairs){

            bool p1_contains = parent1.containsInnov(p.Key);
            bool p2_contains = parent2.containsInnov(p.Key);

            if (p1_contains && !p2_contains){
                if (p2_max_innov > p.Key){
                    rtn.y+=1;
                }
                else{
                    rtn.z+=1;
                }
            }
            else if (!p1_contains && p2_contains){
                if (p1_max_innov > p.Key){
                    rtn.y+=1;
                }
                else{
                    rtn.z+=1;
                }
            }
            else if (p1_contains && p2_contains){
                rtn.x+=1;
            }
            else { //Innov number error
            }
        }
        return rtn;
    }

    public static float getCompatDistance(NeatNetwork parent1, NeatNetwork parent2, float c1, float c2, float c3){
        Vector3 compareCounts = NeatNetwork.getGeneCompareCounts(parent1, parent2);
        float weightDiff = NeatNetwork.getAvgWeightDiff(parent1, parent2);

        return (int)compareCounts.z * c1 + (int)compareCounts.y * c2 + weightDiff * c3;
    }

    private static float getAvgWeightDiff(NeatNetwork parent1, NeatNetwork parent2){
        int p1_max_innov = parent1.getMaxInnov();
        int p2_max_innov = parent2.getMaxInnov();

        int max_innov = NeatNetwork.intMax(p1_max_innov, p2_max_innov);
        float diffSum = 0.0f;
        int matchingGenes = 0;

        for (int i = 0; i < max_innov; i++){
            if (parent1.containsInnov(i) && parent2.containsInnov(i)){
                matchingGenes++;
                diffSum += Mathf.Abs(parent1.getWeightFromInnov(i) - parent2.getWeightFromInnov(i));
            }
        }

        return diffSum / (float)matchingGenes;
    }

    public static int intMax(int n1, int n2){
        if (n1 >= n2){
            return n1;
        }
        else {
            return n2;
        }
    }
    public static float floatMax(float n1, float n2){
        if (n1 >= n2){
            return n1;
        }
        else {
            return n2;
        }
    }

    public Connection getConnectionFromInnov(int innov){
        for (int i = 0; i < connections.Count; i++){
            if (connections[i].innov == innov){
                return connections[i];
            }
        }
        return null;
    }

    public float getWeightFromInnov(int innov){
        for (int i = 0; i < connections.Count; i++){
            if (connections[i].innov == innov){
                return connections[i].weight;
            }
        }
        return 0;
    }

    public static NeatNetwork crossover(NeatNetwork parent1, NeatNetwork parent2){
        // List<Connection> newConnections = new List<Connection>();
        Dictionary<int, Node> newNodes = new Dictionary<int, Node>();
        List<Connection> newConnections = new List<Connection>();

        bool parent1_fitest;
        if (parent1.fitness > parent2.fitness){
            parent1_fitest = true;
        }
        else {
            parent1_fitest = false;
        }


        foreach (var p in GlobalVars.pairs){

            // int p1_index = parent1.connections.ContainsKey(i) ? i : -1;
            // int p2_index = parent2.connections.ContainsKey(i) ? i : -1;
            bool p1_contains = parent1.containsInnov(p.Key);
            bool p2_contains = parent2.containsInnov(p.Key);

            // if (p2_index == -1 && p1_index != -1){ //parent 1 has connection & not parent 2
            if (p1_contains && !p2_contains){
                if (parent1_fitest){
                    // newConnections.Add(parent1.connections[p1_index]);
                    Connection currCon = parent1.getConnectionFromInnov(p.Key);
                    newConnections.Add(currCon);
                    if (!newNodes.ContainsKey(currCon.in_node)){
                        newNodes.Add(currCon.in_node, parent1.nodes[currCon.in_node]);
                    }
                    if (!newNodes.ContainsKey(currCon.out_node)){
                        newNodes.Add(currCon.out_node, parent1.nodes[currCon.out_node]);
                    }
                }
            }
            // else if (p1_index == -1 && p2_index != -1){ //parent 2 has connection & not parent 1
            else if (!p1_contains && p2_contains){
                if (!parent1_fitest){
                    // newConnections.Add(parent2.connections[p2_index]);
                    // newConnections.Add(i, parent2.connections[i]);
                    Connection currCon = parent2.getConnectionFromInnov(p.Key);
                    newConnections.Add(currCon);
                    // newConnections.Add(parent2.getConnectionFromInnov(p.Key));
                    if (!newNodes.ContainsKey(currCon.in_node)){
                        newNodes.Add(currCon.in_node, parent2.nodes[currCon.in_node]);
                    }
                    if (!newNodes.ContainsKey(currCon.out_node)){
                        newNodes.Add(currCon.out_node, parent2.nodes[currCon.out_node]);
                    }

                }
            }
            // else if (p1_index != -1 && p2_index != -1){ //They both have it
            else if (p1_contains && p2_contains){
                int rand_parent = Random.Range(0, 2);
                // Connection currCon;
                if (rand_parent == 0){ //Taking connection from parent 1
                    // newConnections.Add(parent1.connections[p1_index]);
                    // newConnections.Add(i, parent1.connections[i]);
                    Connection currCon = parent1.getConnectionFromInnov(p.Key);
                    newConnections.Add(currCon);
                    // newConnections.Add(parent1.getConnectionFromInnov(p.Key));
                    if (!newNodes.ContainsKey(currCon.in_node)){
                        newNodes.Add(currCon.in_node, parent1.nodes[currCon.in_node]);
                    }
                    if (!newNodes.ContainsKey(currCon.out_node)){
                        newNodes.Add(currCon.out_node, parent1.nodes[currCon.out_node]);
                    }

                //     if (!newNodes.ContainsKey(parent1.connections[p1_index].in_node)){
                //         newNodes.Add(parent1.connections[p1_index].in_node, parent1.nodes[parent1.connections[p1_index].in_node]);
                //     }
                //     if (!newNodes.ContainsKey(parent1.connections[p1_index].out_node)){
                //         newNodes.Add(parent1.connections[p1_index].out_node, parent1.nodes[parent1.connections[p1_index].out_node]);
                //     }
                }
                else{ //Taking connection from parent 2
                    // newConnections.Add(parent2.connections[p2_index]);
                    // newConnections.Add(i, parent2.connections[i]);
                    Connection currCon = parent2.getConnectionFromInnov(p.Key);
                    newConnections.Add(currCon);
                    // newConnections.Add(parent2.getConnectionFromInnov(p.Key));
                    if (!newNodes.ContainsKey(currCon.in_node)){
                        newNodes.Add(currCon.in_node, parent2.nodes[currCon.in_node]);
                    }
                    if (!newNodes.ContainsKey(currCon.out_node)){
                        newNodes.Add(currCon.out_node, parent2.nodes[currCon.out_node]);
                    }

                    // if (!newNodes.ContainsKey(parent2.connections[p2_index].in_node)){
                    //     newNodes.Add(parent2.connections[p2_index].in_node, parent2.nodes[parent2.connections[p2_index].in_node]);
                    // }
                    // if (!newNodes.ContainsKey(parent2.connections[p2_index].out_node)){
                    //     newNodes.Add(parent2.connections[p2_index].out_node, parent2.nodes[parent2.connections[p1_index].out_node]);
                    // }
                }
            }
            else { //Innov number error

            }
        }

        NeatNetwork newNet = new NeatNetwork(parent1.input_size, parent1.output_size, false);
        newNet.nodes = newNodes;
        newNet.connections = newConnections;
        int randomParent = Random.Range(0, 2);
        if (randomParent == 1){
            newNet.bias = parent1.bias;
        }
        else {
            newNet.bias = parent2.bias;
        }

        // newNet.printNetwork("CHILD");


        return newNet;
    }

    public Matrix feedForward(List<float> inputs){
        List<int> to_check_queue = new List<int>();
        List<int> next_queue = new List<int>();
        List<int> all_checked_nodes = new List<int>();
        List<int> outputNodes = new List<int>();
        Dictionary<int, bool> biasTracker = new Dictionary<int, bool>(); //Holds whether a node has already summed bias


        int inputCounter = 0;
        foreach (var n in nodes){ //Get input nodes to start
            biasTracker[n.Key] = false;
            if (n.Value.type == Node.Type.INPUT){
                n.Value.currentValue = inputs[inputCounter];
                to_check_queue.Add(n.Value.id);
                inputCounter++;
            }
            else {
                n.Value.currentValue = 0.0f;
            }
            if (n.Value.type == Node.Type.OUTPUT){
                outputNodes.Add(n.Key);
            }
        }

        while (to_check_queue.Count > 0){
            for (int i = 0; i < to_check_queue.Count; i++){
                foreach (Connection c in connections){
                    if (c.in_node == to_check_queue[i]){
                        //TODO: Dont forget about bias
                        // printNode(c.out_node, "To node: ");
                        // printNode(c.in_node, " += ");
                        // Debug.Log("Times weights: " + c.weight);
                        nodes[c.out_node].currentValue += nodes[c.in_node].currentValue * c.weight;
                        if (!biasTracker[c.out_node]){
                            nodes[c.out_node].currentValue += bias;
                            biasTracker[c.out_node] = true;
                        }
                        if (nodes[c.out_node].type == Node.Type.OUTPUT){
                            continue;
                        }
                        if (!next_queue.Contains(c.out_node) && !all_checked_nodes.Contains(c.out_node)){
                            next_queue.Add(c.out_node);
                            all_checked_nodes.Add(c.out_node);
                        }
                    }
                }
            }
            to_check_queue = next_queue;
            next_queue.Clear();
        }
        outputNodes.Sort();
        Matrix outputs = new Matrix(outputNodes.Count, 1);
        for (int i = 0; i < outputNodes.Count; i++){
            outputs.insert(nodes[outputNodes[i]].currentValue, i);
        }

        // Debug.Log("OUTPUT 0: " + outputs.get(0));
        // Debug.Log("OUTPUT 1: " + outputs.get(1));
        // Debug.Log("OUTPUT 2: " + outputs.get(2));
        // Debug.Log("OUTPUT 3: " + outputs.get(3));
        return outputs.normalize();

        // Vector2 test = new Vector2(outputs.get(0), outputs.get(1)).normalized;
        // Debug.Log("TEST: " + test);

        // return outputs.sigmoid();

    }

    public void printNetwork(string tag = ""){
        if (tag != "")
            Debug.Log(tag);
        // foreach (var n in nodes){
        //     Debug.Log("Node: " + n.Value.id + " Type: " + n.Value.type + " Value: " + n.Value.currentValue);
        // }
        foreach (Connection c in connections){
            Debug.Log("Connection " + c.in_node + " to " + c.out_node + " innov: " + c.innov + " weight: " + c.weight + " enabled: " + c.enabled);
        }
    }

    public void printConnection(Connection c, int key, string tag = ""){
        if (tag != "")
            Debug.Log(tag);
        Debug.Log("Connection " + c.in_node + " to " + c.out_node + " innov: " + key + " weight: " + c.weight + " enabled: " + c.enabled);
    }

    public void printNode(int nodeid, string tag =""){
        if (tag != "") 
            Debug.Log(tag);
        Debug.Log("Node: " + nodeid + " Type: " + nodes[nodeid].type + " Value: " + nodes[nodeid].currentValue);
    }

}