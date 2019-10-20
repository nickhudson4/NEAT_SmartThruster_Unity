using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeatManager : MonoBehaviour
{
    //POPULATION VARS
    public GameObject playerPrefab;
    private Vector3 startPos = new Vector3(0.0f, 0.8f, -11.0f);
    public int populationSize = 100;
    public List<Player> players;
    private int isDeadCount = 0;
    private float maxRuntime = 10.0f;
    private Counter global_counter;

    //SPECIES VARS
    public List<Species> species;

    //NETWORK VARS
    int input_size = 10;
    int output_size = 2;
    public float MUTATION_RATE;
    public float ADD_CONNECTION_RATE;
    public float ADD_NODE_RATE;
    float c1 = 1.0f;
    float c2 = 1.0f;
    float c3 = 3.0f;
    float DT = 4.0f;

    //MAZE VARS
    public GameObject target_GO;
    int layerMask = 1 << 9;


    void Start(){
        this.species = new List<Species>();
        this.players = new List<Player>();
        this.global_counter = new Counter(maxRuntime);


        spawnPopulation();



        // NeatNetwork n1 = new NeatNetwork(3, 1, true);

        // NeatNetwork n2 = new NeatNetwork(3, 1, true);
        // // n1.addNode(1);
        // // n1.addConnection(n1.nodes[0], n1.nodes[4]);
        // n2.addNode(1);
        // n2.addNode(3);
        // n2.addConnection(n2.nodes[0], n2.nodes[4]);
        // n2.addConnection(n2.nodes[2], n2.nodes[5]);
        // n1.addNode(0);
        // n2.addConnection(n2.nodes[2], n2.nodes[4]);
        // // n1.addConnection(n1.nodes[2], n1.nodes[5], GlobalVars.getInnov());
        // n2.fitness = 0.5f;

        // n1.printNetwork("NETWORK 1");
        // // n1.mutate();
        // // n1.printNetwork("NETWORK 1 MUTATED");
        // n2.printNetwork("NETWORK 2");
        // Vector3 comp = NeatNetwork.getGeneCompareCounts(n1, n2);
        // Debug.Log("MATHCNIONG: " + comp);
        // // n2.mutate();
        // // n2.printNetwork("NETWORK 2 MUTATED");

        // NeatNetwork.crossover(n1, n2);
        // run();
    }

    void Update(){
        // Debug.Log("Current Count: " + global_counter.currentCount());
        run();
    }

    private void run(){

        for (int i = 0; i < players.Count; i++){
            // Player tmp = players[i];
            if (players[i].isDead || players[i].player_GO == null){ continue; }
            // players[i].network.printNetwork("Player " + i + " Network: ");

            List<float> dists = getRaycastDistances(players[i]);
            if (dists.Count != 8){ continue; }
            List<float> inputs = createInputList(dists, players[i]);
            // printFloatList(inputs);
            Matrix tmp = players[i].network.feedForward(inputs);
            // tmp.printMatrix("OUPUTS");
            players[i].controller.applyForceOnAxis(tmp.get(0), tmp.get(1));
            // players[i].controller.applyForces(tmp);
            // List<float> outputs = tmp.toList();
            // printFloatList(outputs);

            // float horiz = changeRange(outputs[0]);
            // Debug.Log(horiz);
            // players[i].controller.moveForwardWithRot(horiz);

            if (global_counter.isOver()){
                onDeath(i);
            }

        }
        global_counter.incriment();

        if (isDeadCount >= players.Count){
            startEvaluation();
            global_counter.reset();
        }
    }

    private void spawnPopulation(){

        List<Player> tmp = new List<Player>();
        for (int i = 0; i < populationSize; i++){
            GameObject player = Instantiate(playerPrefab, startPos, Quaternion.identity);
            player.name = "Player_" + i;
            NeatNetwork net = new NeatNetwork(input_size, output_size, true);
            Player newPlayer = new Player(net, player);
            tmp.Add(newPlayer);
        }
        players = tmp;
    }

    private void startEvaluation(){
        foreach (Species s in species){
            s.reset();
        }
        fillSpecies();
        removeEmptySpecies();
        evaluatePlayers();
        createNextGen();
        isDeadCount = 0;
    }

    private void fillSpecies(){
        foreach (Player p in players){
            bool foundSpecies = false;
            foreach (Species s in species){
                // Debug.Log("SPEC DIST: " + NeatNetwork.getCompatDistance(p.network, s.mascot.network, c1, c2, c3));
                if (NeatNetwork.getCompatDistance(p.network, s.mascot.network, c1, c2, c3) < DT){
                    s.members.Add(p);
                    p.species = s;
                    foundSpecies = true;
                    break;
                }
            }

            if (!foundSpecies){
                Species newSpecies = new Species(p);
                newSpecies.color = new Color(
                    Random.Range(0f, 1f), 
                    Random.Range(0f, 1f), 
                    Random.Range(0f, 1f)
                );
                species.Add(newSpecies);
                p.species = newSpecies;
            }
        }
        Debug.Log("Species: " + species.Count);
    }

    private void removeEmptySpecies(){
        List<Species> tmp = new List<Species>();
        foreach (Species s in species){
            if (s.members.Count > 0){
                tmp.Add(s);
            }
        }
        species = tmp;
    }

    private void evaluatePlayers(){
        foreach (Player p in players){
            float adjustedScore = p.score / p.species.members.Count;

            p.species.setSpeciesFitness(adjustedScore);
            p.fitness = adjustedScore;
        }
    }

    private void createNextGen(){
        List<Player> fittest = new List<Player>();
        foreach (Species s in species){
            float best = -1.0f;
            NeatNetwork bestPlayer = null;
            foreach (Player p in s.members){
                if (p.fitness > best){
                    best = p.fitness;
                    bestPlayer = p.network;
                }
            }
            if (bestPlayer != null){
                GameObject player = Instantiate(playerPrefab, startPos, Quaternion.identity);
                player.name = "Player_";
                Player bestP = new Player(bestPlayer, player);
                bestP.species = s;
                player.GetComponent<Renderer>().material.SetColor("_Color", s.color);
                fittest.Add(bestP);
            }
        }
        while (fittest.Count < populationSize){
            Species s = getRandSpeciesWithProbability();
            Player p1 = getRandPlayerWithProbability(s);
            Player p2 = getRandPlayerWithProbability(s);

            NeatNetwork childNet = NeatNetwork.crossover(p1.network, p2.network);
            float r1 = Random.Range(0.0f, 1.0f);
            float r2 = Random.Range(0.0f, 1.0f);
            float r3 = Random.Range(0.0f, 1.0f);
            if (r1 < MUTATION_RATE){
                childNet.mutate();
            }
            if (r2 < ADD_CONNECTION_RATE){
                childNet.addConnection(null, null, true);
            }
            if (r3 < ADD_NODE_RATE){
                childNet.addNode(-1, true);
            }

            GameObject player = Instantiate(playerPrefab, startPos, Quaternion.identity);
            player.name = "Player_";
            Player newPlayer = new Player(childNet, player);
            newPlayer.species = s;
            player.GetComponent<Renderer>().material.SetColor("_Color", s.color);

            fittest.Add(newPlayer);

        }
        destroyPopulation();
        players = fittest;
    }


    private Species getRandSpeciesWithProbability(){
        float completeWeight = 0.0f;
        foreach (Species s in species){
            completeWeight += s.speciesFitness;
        }
        float r = Random.Range(0.0f, completeWeight);
        float countWeight = 0.0f;

        foreach (Species s in species){
            countWeight += s.speciesFitness;
            if (countWeight >= r){
                return s;
            }
        }
        return null;
    }

    private Player getRandPlayerWithProbability(Species s){
        float completeWeight = 0.0f;
        foreach (Player p in s.members){
            completeWeight += p.fitness;
        }
        float r = Random.Range(0.0f, completeWeight);
        float countWeight = 0.0f;

        foreach (Player p in s.members){
            countWeight += p.fitness;
            if (countWeight >= r){
                return p;
            }
        }
        return null;
    }

    private void destroyPopulation(){
        foreach (Player p in players){
            Destroy(p.player_GO);
        }

        players.Clear();
    }

    private List<float> createInputList(List<float> ray_dists, Player p){
        List<float> inputs = new List<float>();
        for (int i = 0; i < ray_dists.Count; i++){
            inputs.Add(ray_dists[i]);
        }
        inputs.Add(target_GO.transform.position.x);
        inputs.Add(target_GO.transform.position.z);

        return inputs;
    }

    public void OnPlayerWallHitHelper(GameObject p_obj, GameObject otherObject){
        int player_index = getPlayerIndexFromObj(p_obj);
        if (otherObject.tag == "MazeWall"){
            onDeath(player_index);
        }
        else if (otherObject.tag == "MazeTarget"){
            onMazeCompletion(player_index);
        }
    }

    private void onDeath(int playerIndex){
        players[playerIndex].isDead = true;
        players[playerIndex].score = calc_score(players[playerIndex]);
        // players[playerIndex].player_GO.SetActive(false);
        players[playerIndex].controller.rb.isKinematic = true;
        isDeadCount++;
    }


    private float calc_score(Player p){
        float dist_from_targ = Vector3.Distance(p.player_GO.transform.position, target_GO.transform.position);
        float total_dist = Vector3.Distance(startPos, target_GO.transform.position);
        float change = total_dist - dist_from_targ;
        if (change < 0){ change = 0; }

        return change;
    }

    private void onMazeCompletion(int playerIndex){
        // Player tmp = players[playerIndex];
        onDeath(playerIndex);
    }

    public int getPlayerIndexFromObj(GameObject p_obj){
        for (int i = 0; i < players.Count; i++){
            if (players[i].player_GO == p_obj){
                return i;
            }
        }
        return -1;
    }

    private float changeRange(float val){
        return (val - 0.5f) * 2;
    }

    private Vector3 getDirBetween(Vector3 dir1, Vector3 dir2){ //Dir between right angle formed by dir1 and dir2
        Vector3 newDir = Vector3.zero;
        newDir += dir1/2;
        newDir += dir2/2;

        return newDir;
    }

    public Vector2 vector3_to_vector2(Vector3 orig, string dropAxis){
        if (dropAxis == "y"){
            return new Vector2(orig.x, orig.z);
        }
        if (dropAxis== "x"){
            return new Vector2(orig.y, orig.z);
        }
        else {
            return new Vector2(orig.x, orig.y);
        }
    }

    public Vector3 vector2_to_vector3(Vector2 orig, string dropAxis){
        if (dropAxis == "y"){
            return new Vector3(orig.x, 0, orig.y);
        }
        if (dropAxis== "x"){
            return new Vector3(0, orig.x, orig.y);
        }
        else {
            return new Vector3(orig.x, orig.y, 0);
        }
    }

    public void printFloatList(List<float> l){
        string concat = "";
        for (int i = 0; i < l.Count; i++){
            concat += l[i] + ", ";
        }
        Debug.Log(concat);
    }

    private List<float> getRaycastDistances(Player p){
         RaycastHit hit;
         Vector3 forward_right = getDirBetween(p.player_GO.transform.forward, p.player_GO.transform.right);
         Vector3 forward_left = getDirBetween(p.player_GO.transform.forward, -p.player_GO.transform.right);
         Vector3 back_right = getDirBetween(-p.player_GO.transform.forward, p.player_GO.transform.right);
         Vector3 back_left = getDirBetween(-p.player_GO.transform.forward, -p.player_GO.transform.right);

        List<float> distances = new List<float>();

        if (Physics.Raycast(p.player_GO.transform.position, p.player_GO.transform.forward, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, p.player_GO.transform.forward, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, forward_right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, forward_right, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, p.player_GO.transform.right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, p.player_GO.transform.right, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, back_right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, back_right, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, -p.player_GO.transform.forward, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, -p.player_GO.transform.forward, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, back_left, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, back_left, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, -p.player_GO.transform.right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, -p.player_GO.transform.right, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, forward_left, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, forward_left, Color.green);
        }
        return distances;
    }
}
