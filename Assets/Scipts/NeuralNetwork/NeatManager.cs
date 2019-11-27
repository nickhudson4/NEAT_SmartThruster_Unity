using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;


public class NeatManager : MonoBehaviour
{

    public enum Mode {
        TRAINING,
        TESTING
    }
    public Mode mode;

    //POPULATION VARS
    public GameObject playerPrefab;
    private Vector3 startPos = new Vector3(0.0f, 0.8f, -45.0f);
    public int populationSize = 100;
    public List<Player> players;
    private int isDeadCount = 0;
    private float maxRuntime = 75.0f;
    private Counter global_counter;
    private int gen_counter = 0;
    private bool renderPlayers = true;

    //SPECIES VARS
    public List<Species> species;

    //NETWORK VARS
    public int input_size;
    // public int input_size = 9;
    public int output_size;
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
    public GameObject pairs;
    private List<Vector3> gatePositions;

    //MISC
    public TextMeshProUGUI current_gen_tmp;
    public string tag;
    public TextMeshProUGUI tag_tmp;

    //FILE WRITING VARS
    private int numGensToWrite = 150;
    private string line_tag = "";
    private string all_fitnesses = "";

    //MODELING SAVING VARS
    private bool saveNextGen = false;
    public TMP_InputField whichSave;

    //PAUSING VARS
    private bool pauseNextGen = false;
    private bool pause = false;

    //NETWORK DRAWING VARS
    NetworkDraw networkDraw;
    private int indexToDraw;

    //MAZE GEN VARS
    private int newMazeAfter = 150;

    private float speed = 100.0f;

    void Start(){
        this.species = new List<Species>();
        this.players = new List<Player>();
        this.global_counter = new Counter(maxRuntime);
        this.current_gen_tmp.text = "Gen: " + gen_counter;

        this.mode = Mode.TRAINING;
        this.pause = true;

        this.networkDraw = GameObject.Find("NetworkDrawer").GetComponent<NetworkDraw>();

        // spawnPopulation();
        getGatePositions();


        this.tag_tmp.text = "" + tag;
    }

    void Update(){
        if (pause){ return; }
        run();
        // Debug.Log(calc_score(players[0]));


    }

    private void run(){
        for (int i = 0; i < players.Count; i++){
            if (players[i].isDead || players[i].player_GO == null){ continue; }
            if (!isInBounds(players[i].player_GO.transform.position)){
                Debug.Log("Player: " + players[i].player_GO.name + " Out of bounds! ", players[i].player_GO);
                onDeath(i);
            }
            players[i].network.tmp = i;

            // List<float> dists = getRaycastDistances(players[i]);
            // if (dists.Count != 8){ continue; }
            // if (dists.Count != 5){ continue; }
            // List<float> inputs = createInputList(dists, players[i]);
            List<float> inputs = createInputList2(players[i]);
            Matrix outputs = players[i].network.feedForward2(inputs);
            outputs = outputs.normalize();
            if (i == indexToDraw){
                networkDraw.drawUpdate(inputs, outputs.toList());
            }
            // players[i].controller.applyForceOnAxis(outputs.get(0), outputs.get(1), true);
            players[i].controller.applyTranslateOnAxis(outputs.get(0), outputs.get(1), speed);
            // players[i].controller.moveForwardWithRot(outputs.get(0));
            // players[i].controller.moveForwardWithRot(outputs.getVectorMaxIndex() == 0 ? -1.0f : 1.0f);
            // players[i].controller.applyStrictForce(outputs);

            if (global_counter.isOver()){
                onDeath(i);
            }
            // players[i].checkIfStuck(i);
            players[i].checkIfInLoop(i);
            players[i].lastPos = players[i].player_GO.transform.position;

        }
        global_counter.incriment();

        if (isDeadCount >= players.Count && mode == Mode.TRAINING){
            startEvaluation();
            global_counter.reset();
        }
    }

    private List<float> createInputList2(Player player){
        List<float> tmp = new List<float>();
        Transform p_trans = player.player_GO.transform;
        tmp.Add(p_trans.position.x);
        tmp.Add(p_trans.position.z);

        Vector3 next_gate = Vector3.zero;
        for (int i = 0; i < gatePositions.Count; i++){
            if (p_trans.position.z > gatePositions[i].z){
                if (i == gatePositions.Count - 1){
                    next_gate = target_GO.transform.position;
                    break;
                }
                else {
                    next_gate = gatePositions[i+1];
                }
            }
        }
        if (next_gate == Vector3.zero){
            next_gate = gatePositions[0];
        }

        tmp.Add(next_gate.x);
        tmp.Add(next_gate.z);

        return tmp;
    }

    private bool isInBounds(Vector3 pos){
        // Debug.Log("POS: " + pos);
        if (pos.z < -50.0f || pos.z > 50.0f || pos.x < -50.0f || pos.x > 50.0f){
            Debug.Log("Return false");
            return false;
        }
        return true;
    }


    private void spawnPopulation(){
        List<Player> tmp = new List<Player>();
        for (int i = 0; i < populationSize; i++){
            GameObject player = Instantiate(playerPrefab, startPos, Quaternion.identity);
            if (!renderPlayers){
                player.GetComponent<Renderer>().enabled = false;
            }
            player.name = "Player_" + i;
            NeatNetwork net = new NeatNetwork(input_size, output_size, true, this);
            Player newPlayer = new Player(net, player, this);
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
        gen_counter++;
        current_gen_tmp.text = "Gen: " + gen_counter;
        if (pauseNextGen){
            pause = true;
        }
    }

    private void fillSpecies(){
        if (species.Count <= populationSize/10){
            DT-=0.3f;
        }
        else {
            DT+=0.3f;
        }

        if (gen_counter > 0 && gen_counter % 20 == 0){ //after number of gens, remove lowest species
            Debug.Log("===== REMOVING WORST SPECIES =====");
            int worst_species = getWorstSpecies();
            species.RemoveAt(worst_species);
        }

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
        // Debug.Log("Num Species: " + species.Count);
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
            p.network.fitness = adjustedScore;
        }
        // Debug.Log("GLOBAL FITNESS: " + calc_global_fitness() + " At gen: " + gen_counter);

        // if (saveNextGen || (gen_counter % 300 == 0 && gen_counter > 0)){
        if (saveNextGen){
            saveBestModel();
        }

        // if (gen_counter % newMazeAfter == 0 && gen_counter > 0){
        //     GameObject.Find("MazeGenerator").GetComponent<MazeGenerator>().OnClickGenerateMaze();
        // }

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
                Player bestP = spawnPlayer(bestPlayer, startPos);
                bestP.species = s;
                s.setMascot(bestP);
                bestP.player_GO.GetComponent<Renderer>().material.SetColor("_Color", s.color);
                fittest.Add(bestP);
            }
        }

        indexToDraw = getBestPlayerIndex(fittest);
        networkDraw.draw(fittest[indexToDraw].network);
        // foreach (Player p in players){ //% of players are mutated without crossover
        //     float rand = Random.Range(0.0f, 1.0f);
        //     if (rand <= .2f){
        //         NeatNetwork newNet = p.network;
        //         newNet.mutate();
        //         Player newP = spawnPlayer(newNet, startPos);
        //         newP.species = p.species;
        //         newP.player_GO.GetComponent<Renderer>().material.SetColor("_Color", p.species.color);
        //         fittest.Add(newP);
        //     }
        // }
        int loopCounter = 0;
        while (fittest.Count < populationSize){
            Species s = getRandSpeciesWithProbability(); //Get high fitness species
            Player p1 = getRandPlayerWithProbability(s); //Get high fitness player1 from species
            Player p2 = getRandPlayerWithProbability(s); //Get high fitness player2 from species
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

            Player newPlayer = spawnPlayer(childNet, startPos);
            newPlayer.species = s;
            newPlayer.player_GO.GetComponent<Renderer>().material.SetColor("_Color", s.color);

            fittest.Add(newPlayer);

            if (loopCounter >= 500){
                Debug.Log("INFINITE WHILE LOOP DETECTED!!!!");
                break;
            }

            loopCounter++;
        }
        destroyPopulation();
        players = fittest;
    }

    private int getBestPlayerIndex(List<Player> players){
        float bestFitness = -1.0f;
        int bestP = 0;
        for (int i = 0; i < players.Count; i++){
            if (players[i].fitness > bestFitness){
                bestFitness = players[i].fitness;
                bestP = i;
            }
        }

        return bestP;
    }

    public void getGatePositions(){
        List<Vector3> tmp_rtn = new List<Vector3>();
        for (int i = 0; i < pairs.transform.childCount; i+=2){
            Transform p1 = pairs.transform.GetChild(i);
            Transform p2 = pairs.transform.GetChild(i+1);

            Vector3 tmp_pos_1 = p1.position;
            tmp_pos_1.x += p1.localScale.x / 2;
            Vector3 tmp_pos_2 = p2.position;
            tmp_pos_2.x -= p2.localScale.x / 2;
            Vector3 newPos = (tmp_pos_1 + tmp_pos_2) / 2;
            // GameObject prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // prim.transform.localScale = new Vector3(2,2,2);
            // prim.transform.position = newPos;
            tmp_rtn.Add(newPos);
        }
        gatePositions = tmp_rtn;
    }


    private void saveBestModel(){
        float bestFitness = -1.0f;
        Player bestP = null;
        foreach (Player p in players){
            if (p.fitness > bestFitness){
                bestFitness = p.fitness;
                bestP = p;
            }
        }
        bestP.network.saveNetwork();

        saveNextGen = false;
    }

    private Player spawnPlayer(NeatNetwork network, Vector3 pos){
        GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
        if (!renderPlayers){
            player.GetComponent<Renderer>().enabled = false;
        }
        player.name = "Player_" + Random.Range(0, 10000);

        Player newPlayer = new Player(network, player, this);
        return newPlayer;
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

    private int getWorstSpecies(){
        float worst_fit = 1000.0f;
        int worst = 0;
        for (int i = 0; i < species.Count; i++){
            if (species[i].speciesFitness < worst_fit){
                worst_fit = species[i].speciesFitness;
                worst = i;
            }
        }

        return worst;
    }

    private void destroyPopulation(){
        foreach (Player p in players){
            Destroy(p.player_GO);
        }

        players.Clear();
    }


    private List<float> createInputList(List<float> ray_dists, Player p){
        List<float> inputs = new List<float>();
        Vector2 velocity = vector3_to_vector2(p.controller.rb.velocity, "y");

        for (int i = 0; i < input_size; i++){
            if (i < ray_dists.Count){
                inputs.Add(ray_dists[i]);
            }
            else if (i == 8){
                inputs.Add(target_GO.transform.position.x);
            }
            else if (i == 9){
                inputs.Add(target_GO.transform.position.z);
            }
            else if (i == 10){
                inputs.Add(p.player_GO.transform.position.x);
            }
            else if (i == 11){
                inputs.Add(p.player_GO.transform.position.z);
            }
            else if (i == 12){
                inputs.Add(velocity.x);
            }
            else if (i == 13){
                inputs.Add(velocity.y);
            }
            // if (i < ray_dists.Count){
            //     inputs.Add(ray_dists[i]);
            // }
            // else if (i == 5){
            //     inputs.Add(target_GO.transform.position.x);
            // }
            // else if (i == 6){
            //     inputs.Add(target_GO.transform.position.z);
            // }
            // else if (i == 7){
            //     inputs.Add(p.player_GO.transform.position.x);
            // }
            // else if (i == 8){
            //     inputs.Add(p.player_GO.transform.position.z);
            // }
        }

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

    private void onMazeCompletion(int playerIndex){
        // Player tmp = players[playerIndex];
        onDeath(playerIndex);
    }

    public void onDeath(int playerIndex){
        players[playerIndex].player_GO.name = players[playerIndex].player_GO.name + " (DEAD)";
        players[playerIndex].isDead = true;
        players[playerIndex].score = calc_score(players[playerIndex]);
        // players[playerIndex].player_GO.SetActive(false);
        players[playerIndex].controller.rb.isKinematic = true;
        isDeadCount++;
    }

    private float calc_global_fitness(){
        float sum = 0;
        foreach (Species s in species){
            sum += s.speciesFitness;
        }
        return sum/species.Count;
    }

    private float calc_score(Player p){
        // float dist_from_targ = Vector3.Distance(p.player_GO.transform.position, target_GO.transform.position);
        // float total_dist = Vector3.Distance(startPos, target_GO.transform.position);
        // float change = total_dist - dist_from_targ;
        // if (change < 0){ change = 0; }

        // return change;

        Transform p_transform = p.player_GO.transform;
        Vector3 current_gate = Vector3.zero;
        Vector3 next_gate = Vector3.zero;
        int current_gate_index = 0;
        for (int i = 0; i < gatePositions.Count; i++){
            if (p_transform.position.z > gatePositions[i].z){
                current_gate = gatePositions[i];
                current_gate_index = i + 1;
                if (i == gatePositions.Count - 1){ //current_gate == last gate
                    next_gate = target_GO.transform.position; //next_gate = target
                }
                else {
                    next_gate = gatePositions[i+1];
                }
            }
        }

        if (current_gate == Vector3.zero){
            next_gate = gatePositions[0];
        }

        float already_completed = current_gate_index * 1.5f;
        float cur_full = Vector3.Distance(current_gate, next_gate);
        float cur_completed = Vector3.Distance(current_gate, p_transform.position);
        float cur_toGo = Vector3.Distance(p_transform.position, next_gate);
        float cur_full_with_error = cur_completed + cur_toGo;
        float percent_with_error = cur_completed / cur_full_with_error;
        float error = 1 - (cur_full / cur_full_with_error);
        float percent_with_errorsubbed = percent_with_error - error;

        float final_completed = already_completed + percent_with_errorsubbed;

        // if (final_completed < already_completed){ final_completed = already_completed; }
        if (final_completed < 0.0f){ final_completed = 0.0f; }

        return final_completed;


        // if (score < 0){ score = 0; }

        // return score;
    }


    public void OnClickPause(){
        pauseNextGen = true;
    }
    public void OnClickPlay(){
        pause = false;
        pauseNextGen = false;
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

    public Vector3 vector2_to_vector3(Vector2 orig, string addAxis, float fillWith){
        if (addAxis == "y"){
            return new Vector3(orig.x, fillWith, orig.y);
        }
        if (addAxis== "x"){
            return new Vector3(fillWith, orig.x, orig.y);
        }
        else {
            return new Vector3(orig.x, orig.y, fillWith);
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
        // Debug.Log("player : " + p.player_GO.name + " vel: " + p.controller.rb.velocity, gameObject);
         RaycastHit hit;
         Vector3 forward_right = getDirBetween(p.player_GO.transform.forward, p.player_GO.transform.right);
         Vector3 forward_left = getDirBetween(p.player_GO.transform.forward, -p.player_GO.transform.right);
         Vector3 back_right = getDirBetween(-p.player_GO.transform.forward, p.player_GO.transform.right);
         Vector3 back_left = getDirBetween(-p.player_GO.transform.forward, -p.player_GO.transform.right);

        List<float> distances = new List<float>();

        if (Physics.Raycast(p.player_GO.transform.position, p.player_GO.transform.forward, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, p.player_GO.transform.forward, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, forward_right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, forward_right, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, p.player_GO.transform.right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, p.player_GO.transform.right, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, back_right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, back_right, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, -p.player_GO.transform.forward, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, -p.player_GO.transform.forward, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, back_left, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, back_left, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, -p.player_GO.transform.right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, -p.player_GO.transform.right, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, forward_left, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, forward_left, Color.green);
        }
        return distances;
    }

    private List<float> getRaycastDistances2(Player p) {

         RaycastHit hit;
         Vector3 forward_right2 = getDirBetween(p.player_GO.transform.forward, p.player_GO.transform.right);
         Vector3 forward_left2 = getDirBetween(p.player_GO.transform.forward, -p.player_GO.transform.right);
         Vector3 back_right = getDirBetween(-p.player_GO.transform.forward, p.player_GO.transform.right);
         Vector3 back_left = getDirBetween(-p.player_GO.transform.forward, -p.player_GO.transform.right);

        List<float> distances = new List<float>();

        Vector3 player_forward = p.controller.rb.velocity.normalized;
        Vector3 forward_right = getDirBetween(-getPerpendicularVector(player_forward), player_forward);
        Vector3 forward_left = getDirBetween(getPerpendicularVector(player_forward), player_forward);
        // Debug.Log("player : " + p.player_GO.name + " vel: " + player_forward, gameObject);

        if (Physics.Raycast(p.player_GO.transform.position, player_forward, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, player_forward, Color.green);
        }
        else if (Physics.Raycast(p.player_GO.transform.position, p.player_GO.transform.forward, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, p.player_GO.transform.forward, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, getPerpendicularVector(player_forward), out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, getPerpendicularVector(player_forward), Color.green);
        }
        else if (Physics.Raycast(p.player_GO.transform.position, -p.player_GO.transform.right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, p.player_GO.transform.forward, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, -getPerpendicularVector(player_forward), out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, -getPerpendicularVector(player_forward), Color.green);
        }
        else if (Physics.Raycast(p.player_GO.transform.position, p.player_GO.transform.right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, p.player_GO.transform.forward, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, forward_right, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, forward_right, Color.green);
        }
        else if (Physics.Raycast(p.player_GO.transform.position, forward_right2, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, p.player_GO.transform.forward, Color.green);
        }
        if (Physics.Raycast(p.player_GO.transform.position, forward_left, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            Debug.DrawRay(p.player_GO.transform.position, forward_left, Color.green);
        }
        else if (Physics.Raycast(p.player_GO.transform.position, forward_left2, out hit, Mathf.Infinity, layerMask)){
            distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
            // Debug.DrawRay(p.player_GO.transform.position, p.player_GO.transform.forward, Color.green);
        }
        // if (Physics.Raycast(p.player_GO.transform.position, back_right, out hit, Mathf.Infinity, layerMask)){
        //     distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
        //     // Debug.DrawRay(p.player_GO.transform.position, back_right, Color.green);
        // }
        // if (Physics.Raycast(p.player_GO.transform.position, -p.player_GO.transform.forward, out hit, Mathf.Infinity, layerMask)){
        //     distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
        //     // Debug.DrawRay(p.player_GO.transform.position, -p.player_GO.transform.forward, Color.green);
        // }
        // if (Physics.Raycast(p.player_GO.transform.position, back_left, out hit, Mathf.Infinity, layerMask)){
        //     distances.Add(Vector3.Distance(p.player_GO.transform.position, hit.transform.position));
        //     // Debug.DrawRay(p.player_GO.transform.position, back_left, Color.green);
        // }

        return distances;
    }

    private Vector3 getPerpendicularVector(Vector3 orig){
        Vector2 tmp = vector3_to_vector2(orig, "y");
        tmp = Vector2.Perpendicular(tmp);
        return vector2_to_vector3(tmp, "y", 0.0f);
    }

    public void OnRenderToggleChange(Toggle change){
        renderPlayers = change.isOn;
    }

    public void OnClickSave(){
        saveNextGen = true;
    }

    public void OnClickTrain(){
        mode = Mode.TRAINING;
        pause = false;

        spawnPopulation();
    }

    public void OnClickTest(){
        // mode = Mode.TESTING;
        // int save_num;
        // Debug.Log("WHICH SAVE: " + whichSave.text);
        // try {
        //     save_num = int.Parse(whichSave.text);
        // }
        // catch {
        //     Debug.LogError("Failed To Get Save Num. Loading Save 0");
        //     save_num = 0;
        // }

        // NeatNetwork network = NeatNetwork.createNetworkFromSave(save_num, this);
        // Player p = spawnPlayer(network, startPos);
        // players.Add(p);
        // populationSize = 1;
        // pause = false;
    }
}
