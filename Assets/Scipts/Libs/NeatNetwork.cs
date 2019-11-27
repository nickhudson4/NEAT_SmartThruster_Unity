
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class NeatNetwork
{
    public NeatManager manager;

    public Dictionary<int, Node> nodes;
    public List<Connection> connections;
    public int input_size;
    public int output_size;

    public float fitness;
    public float bias;

    public int tmp;


    public NeatNetwork(int input_size, int output_size, bool fill, NeatManager manager){
        this.manager = manager;

        this.input_size = input_size;
        this.output_size = output_size;
        this.nodes = new Dictionary<int, Node>();
        this.connections = new List<Connection>();

        if (fill){
            setupNetwork(input_size, output_size);
        }
    }

    [System.Serializable]
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
        public float bias;

        public Node(Type type, int id){
            this.type = type;
            this.id = id;

            this.currentValue = 0.0f;
        }

    }

    [System.Serializable]
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
        bias = Random.Range(-3.0f, 3.0f);
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

        if ((node1.type == Node.Type.INPUT && node2.type == Node.Type.INPUT) || (node1.type == Node.Type.OUTPUT && node2.type == Node.Type.OUTPUT) || (node1.id == node2.id)){
            return;
        }

        float weight = Random.Range(-2.0f, 2.0f);


        if (node1.type == Node.Type.HIDDEN && node2.type == Node.Type.INPUT)
            reversed = true;
        else if (node1.type == Node.Type.OUTPUT && node2.type == Node.Type.HIDDEN)
            reversed = true;
        else if (node1.type == Node.Type.OUTPUT && node2.type == Node.Type.INPUT)
            reversed = true;

        foreach (Connection c in connections){
            if (c.in_node == node1.id && c.out_node == node2.id){
                return;
            }
            else if (c.in_node == node2.id && c.out_node == node1.id){
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
        if (!target_connection.enabled){
            return;
        }
        target_connection.enabled = false;

        Node newNode = new Node(Node.Type.HIDDEN, nodes.Count);
        Connection inToNew = new Connection(target_connection.in_node, newNode.id, GlobalVars.addPair(new Vector2(target_connection.in_node, newNode.id)), 1.0f, true);
        Connection newToOut = new Connection(newNode.id, target_connection.out_node, GlobalVars.addPair(new Vector2(newNode.id, target_connection.out_node)), target_connection.weight, true);

        nodes.Add(newNode.id, newNode);
        connections.Add(inToNew);
        connections.Add(newToOut);

    }

    private void testForLoop(){

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
                bias = Random.Range(-3.0f, 3.0f);
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

    public static bool getRandBool(){
        int randNum = Random.Range(0, 2);

        if (randNum == 0){
            return true;
        }
        else {
            return false;
        }
    }

    public static NeatNetwork crossover(NeatNetwork parent1, NeatNetwork parent2){
        Dictionary<int, Node> newNodes = new Dictionary<int, Node>();
        List<Connection> newConnections = new List<Connection>();

        bool useWeightAvg = false;
        if (Random.Range(0.0f, 1.0f) <= 0.3f){ //% of crossovers will use average weights from both parents instead of picking one at random
            useWeightAvg = true;
        }
        // useWeightAvg = false;

        bool parent1_fitest;
        if (parent1.fitness > parent2.fitness){
            parent1_fitest = true;
        }
        else if (parent1.fitness < parent2.fitness) {
            parent1_fitest = false;
        }
        else {
            parent1_fitest = NeatNetwork.getRandBool();
        }

        //TODO: Cleanup below code

        foreach (var p in GlobalVars.pairs){
            bool p1_contains = parent1.containsInnov(p.Key);
            bool p2_contains = parent2.containsInnov(p.Key);

            if (!p1_contains && !p2_contains){ continue; }

            if (p1_contains && !p2_contains){
                if (parent1_fitest){
                    Connection currCon = parent1.getConnectionFromInnov(p.Key);
                    newConnections.Add(new Connection(currCon.in_node, currCon.out_node, currCon.innov, currCon.weight, currCon.enabled));
                    if (!newNodes.ContainsKey(currCon.in_node)){
                        Node tmpNode = parent1.nodes[currCon.in_node];
                        newNodes.Add(currCon.in_node, new Node(tmpNode.type, tmpNode.id));
                    }
                    if (!newNodes.ContainsKey(currCon.out_node)){
                        Node tmpNode = parent1.nodes[currCon.out_node];
                        newNodes.Add(currCon.out_node, new Node(tmpNode.type, tmpNode.id));
                    }
                }
            }
            else if (!p1_contains && p2_contains){
                if (!parent1_fitest){
                    Connection currCon = parent2.getConnectionFromInnov(p.Key);
                    newConnections.Add(new Connection(currCon.in_node, currCon.out_node, currCon.innov, currCon.weight, currCon.enabled));
                    if (!newNodes.ContainsKey(currCon.in_node)){
                        Node tmpNode = parent2.nodes[currCon.in_node];
                        newNodes.Add(currCon.in_node, new Node(tmpNode.type, tmpNode.id));
                    }
                    if (!newNodes.ContainsKey(currCon.out_node)){
                        Node tmpNode = parent2.nodes[currCon.out_node];
                        newNodes.Add(currCon.out_node, new Node(tmpNode.type, tmpNode.id));
                    }

                }
            }
            else if (p1_contains && p2_contains){
                if (useWeightAvg){
                    Connection p1_connection = parent1.getConnectionFromInnov(p.Key);
                    Connection p2_connection = parent2.getConnectionFromInnov(p.Key);
                    float average_weight = (p1_connection.weight + p2_connection.weight) / 2.0f;
                    newConnections.Add(new Connection(p1_connection.in_node, p1_connection.out_node, p1_connection.innov, average_weight, Random.Range(0, 2) == 1 ? p1_connection.enabled : p2_connection.enabled));
                    if (!newNodes.ContainsKey(p1_connection.in_node)){
                        Node tmpNode = parent1.nodes[p1_connection.in_node];
                        newNodes.Add(p1_connection.in_node, new Node(tmpNode.type, tmpNode.id));
                    }
                    if (!newNodes.ContainsKey(p1_connection.out_node)){
                        Node tmpNode = parent1.nodes[p1_connection.out_node];
                        newNodes.Add(p1_connection.out_node, new Node(tmpNode.type, tmpNode.id));
                    }
                }
                else {
                    int rand_parent = Random.Range(0, 2);
                    if (rand_parent == 0){ //Taking connection from parent 1
                        Connection currCon = parent1.getConnectionFromInnov(p.Key);
                        newConnections.Add(new Connection(currCon.in_node, currCon.out_node, currCon.innov, currCon.weight, currCon.enabled));
                        if (!newNodes.ContainsKey(currCon.in_node)){
                            Node tmpNode = parent1.nodes[currCon.in_node];
                            newNodes.Add(currCon.in_node, new Node(tmpNode.type, tmpNode.id));
                        }
                        if (!newNodes.ContainsKey(currCon.out_node)){
                            Node tmpNode = parent1.nodes[currCon.out_node];
                            newNodes.Add(currCon.out_node, new Node(tmpNode.type, tmpNode.id));
                        }

                    }
                    else{ //Taking connection from parent 2
                        Connection currCon = parent2.getConnectionFromInnov(p.Key);
                        newConnections.Add(new Connection(currCon.in_node, currCon.out_node, currCon.innov, currCon.weight, currCon.enabled));
                        if (!newNodes.ContainsKey(currCon.in_node)){
                            Node tmpNode = parent2.nodes[currCon.in_node];
                            newNodes.Add(currCon.in_node, new Node(tmpNode.type, tmpNode.id));
                        }
                        if (!newNodes.ContainsKey(currCon.out_node)){
                            Node tmpNode = parent2.nodes[currCon.out_node];
                            newNodes.Add(currCon.out_node, new Node(tmpNode.type, tmpNode.id));
                        }

                    }
                }
            }
        }

        NeatNetwork newNet = new NeatNetwork(parent1.input_size, parent1.output_size, false, parent1.manager);
        newNet.nodes = newNodes;
        newNet.connections = newConnections;
        int randomParent = Random.Range(0, 2);
        if (randomParent == 1){
            newNet.bias = parent1.bias;
        }
        else {
            newNet.bias = parent2.bias;
        }

        // newNet.checkForDeadEnds();


        return newNet;
    }

    private List<int> copyIntList(List<int> orig){
        List<int> newList = new List<int>();
        foreach (int i in orig){
            newList.Add(i);
        }

        return newList;
    }


    private float backRecurse(int focusNode, List<int> myPathNodes){
        if (nodes[focusNode].type == Node.Type.INPUT){
            // Debug.Log("****** PATH: ******");
            // foreach (int i in myPathNodes){
            //     Debug.Log(i);
            // }
            return nodes[focusNode].currentValue;
        }
        float value = 0;
        foreach (Connection c in connections){
            if (nodes[focusNode].type == Node.Type.OUTPUT){
            }
            if (c.out_node == focusNode && c.enabled){
                List<int> myNestedPathNodes = copyIntList(myPathNodes);
                if (myPathNodes.Contains(c.in_node)){
                    // Debug.Log("^^^^^^LOOP DETECTED^^^^^^");
                    // printNetwork("FEED FORWARDING NETWORK: ");
                    return 0.0f;
                }
                myNestedPathNodes.Add(c.in_node);
                float backRec = backRecurse(c.in_node, myNestedPathNodes);
                value += backRec * c.weight;

            }
        }
        return value + bias;
    }

    public Matrix feedForward2(List<float> inputs){
        int i = 0;
        foreach (var n in nodes){
            if (n.Value.type == Node.Type.INPUT){
                n.Value.currentValue = inputs[i];
                i++;
            }
            else {
                n.Value.currentValue = 0.0f;
            }
        }


        List<int> out_nodes = new List<int>();
        foreach (var n in nodes){
            if (n.Value.type == Node.Type.OUTPUT){
                // inCurrentRecursion.Add(n.Key);
                List<int> myPathNodes = new List<int>();
                myPathNodes.Add(n.Key);
                float output = backRecurse(n.Key, myPathNodes);
                n.Value.currentValue = output;
                out_nodes.Add(n.Key);
            }
        }
        out_nodes.Sort();
        Matrix outputs = new Matrix(out_nodes.Count, 1);
        for (int j = 0; j < out_nodes.Count; j++){
            outputs.insert(nodes[out_nodes[j]].currentValue, j);
        }

        return outputs;
    }

    private void checkForDeadEnds(){
        foreach (var n in nodes){
            if (n.Value.type != Node.Type.INPUT){
                bool foundConnection = false;
                foreach (Connection c in connections){
                    if (c.out_node == n.Key){
                        foundConnection = true;
                        break;
                    }
                }
                if (!foundConnection){
                    Debug.Log("&&&&&&&&&&&&&&&&&& FOUND DEAD END &&&&&&&&&&&&&&&&&&&&");
                }
            }
        }
    }

    public int getNumActiveConnections(){
        int counter = 0;
        foreach (Connection c in connections){
            if (c.enabled){
                counter++;
            }
        }
        return counter;
    }

    public void saveNetwork(){

        // if(!Directory.Exists()){
        //     Debug.Log("HERERERE");
        //     Directory.CreateDirectory("C:/Users/" + Environment.UserName + "/AppData/Local/PipeEstimating/PDF_Layers/TiledImages");
        // }

        string root_path = Application.dataPath + "/StreamingAssets/modelsaves/";
        string child_path;
        DirectoryInfo di = new DirectoryInfo(root_path);
        int numFolders = di.GetDirectories().Length;
        child_path = root_path + "save" + numFolders;
        Directory.CreateDirectory(child_path);
        Directory.CreateDirectory(child_path + "/nodes");
        Directory.CreateDirectory(child_path + "/connections");

        int i = 0;
        foreach (var n in nodes){
            n.Value.bias = bias;
            string json = JsonUtility.ToJson(n.Value);
            Debug.Log("JSON: " + json);

            using (StreamWriter w = File.AppendText(child_path + "/nodes/node" + i + ".json"))
            {
                w.WriteLine(json);
            }
            i++;
        }

        int j = 0;
        foreach (Connection c in connections){
            string json = JsonUtility.ToJson(c);

            using (StreamWriter w = File.AppendText(child_path + "/connections/connection" + j + ".json"))
            {
                w.WriteLine(json);
            }
            j++;
        }

        NeatNetwork tmp = new NeatNetwork(10, 2, false, GameObject.Find("NeatManager").GetComponent<NeatManager>());

    }

    public static NeatNetwork createNetworkFromSave(int save_num, NeatManager manager){
        string root_path = Application.dataPath + "/StreamingAssets/modelsaves/";
        string child_path = root_path + "save" + save_num;

        DirectoryInfo nodes_di = new DirectoryInfo(child_path + "/nodes");
        int num_nodes = Directory.GetFiles(child_path + "/nodes", "*", SearchOption.TopDirectoryOnly).Where(name => !name.EndsWith(".meta")).Count();

        DirectoryInfo connections_di = new DirectoryInfo(child_path + "/connections");
        int num_connections = Directory.GetFiles(child_path + "/connections", "*", SearchOption.TopDirectoryOnly).Where(name => !name.EndsWith(".meta")).Count();

        Dictionary<int, Node> my_nodes = new Dictionary<int, Node>();
        List<Connection> my_connections = new List<Connection>();
        for (int i = 0; i < num_nodes; i++){
            string json_string = File.ReadAllText(child_path + "/nodes" + "/node" + i + ".json");
            Node n = JsonUtility.FromJson<Node>(json_string);
            my_nodes.Add(n.id, n);
        }
        for (int i = 0; i < num_connections; i++){
            string json_string = File.ReadAllText(child_path + "/connections" + "/connection" + i + ".json");
            Connection c = JsonUtility.FromJson<Connection>(json_string);
            my_connections.Add(c);
        }
        NeatNetwork tmp_net = new NeatNetwork(manager.input_size, manager.output_size, false, manager);
        tmp_net.nodes = my_nodes;
        tmp_net.connections = my_connections;
        tmp_net.bias = my_nodes[0].bias;



        return tmp_net;
    }

    public void printNetwork(string tag = ""){
        if (tag != "")
            Debug.Log(tag);
        Debug.Log("Bias: " + bias);
        foreach (var n in nodes){
            Debug.Log("Node: " + n.Value.id + " Type: " + n.Value.type + " Value: " + n.Value.currentValue);
        }
        foreach (Connection c in connections){
            Debug.Log("Connection " + c.in_node + " to " + c.out_node + " innov: " + c.innov + " weight: " + c.weight + " enabled: " + c.enabled);
        }
    }

    public void printConnection(Connection c, string tag = ""){
        if (tag != "")
            Debug.Log(tag);
        Debug.Log("Connection " + c.in_node + " to " + c.out_node + " innov: " + c.innov + " weight: " + c.weight + " enabled: " + c.enabled);
    }

    public void printNode(int nodeid, string tag =""){
        if (tag != "")
            Debug.Log(tag);
        Debug.Log("Node: " + nodeid + " Type: " + nodes[nodeid].type + " Value: " + nodes[nodeid].currentValue);
    }

}