using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private Vector3 startPos = new Vector3(0.0f, 0.8f, -47.0f);
    public int populationSize = 100;
    public List<Player> players;
    private int isDeadCount = 0;
    private float maxRuntime = 75.0f;
    private Counter global_counter;
    private int gen_counter = 0;

    //SPECIES VARS
    public List<Species> species;

    //NETWORK VARS
    public int input_size = 10;
    public int output_size = 2;
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
    private NeatNetwork network_to_draw;
    public GameObject networkParentHolder;

    //MAZE GEN VARS
    private int newMazeAfter = 50;


    void Start(){
        this.species = new List<Species>();
        this.players = new List<Player>();
        this.global_counter = new Counter(maxRuntime);
        this.current_gen_tmp.text = "Gen: " + gen_counter;

        this.mode = Mode.TRAINING;
        this.pause = true;

        // spawnPopulation();
        gatePositions = getGatePositions();



    }

    void Update(){
        if (pause){ return; }
        run();
        network_to_draw.drawNetwork(networkParentHolder);

    }

    private void run(){
        for (int i = 0; i < players.Count; i++){
            if (players[i].isDead || players[i].player_GO == null){ continue; }
            players[i].network.tmp = i;
            float distFromStartPos = Vector3.Distance(startPos, players[i].player_GO.transform.position);
            // Debug.Log("DIST: " + distFromStartPos, players[i].player_GO);
            if (global_counter.currentCount() >= 10.0f && distFromStartPos <= 10.0f){
                Debug.Log("KILLED");
                onDeath(i);
            }

            List<float> dists = getRaycastDistances(players[i]);
            if (dists.Count != 8){ continue; }
            List<float> inputs = createInputList(dists, players[i]);
            Matrix outputs = players[i].network.feedForward2(inputs);
            outputs = outputs.normalize();
            players[i].controller.applyForceOnAxis(outputs.get(0), outputs.get(1), true);
            // players[i].controller.moveForwardWithRot(outputs.get(0));

            if (global_counter.isOver()){
                onDeath(i);
            }
            players[i].checkIfStuck(i);
            players[i].lastPos = players[i].player_GO.transform.position;

        }
        global_counter.incriment();

        if (isDeadCount >= players.Count && mode == Mode.TRAINING){
            startEvaluation();
            global_counter.reset();
        }
    }


    private void spawnPopulation(){
        int rand_net_to_draw = Random.Range(0, populationSize);

        List<Player> tmp = new List<Player>();
        for (int i = 0; i < populationSize; i++){
            GameObject player = Instantiate(playerPrefab, startPos, Quaternion.identity);
            player.name = "Player_" + i;
            NeatNetwork net = new NeatNetwork(input_size, output_size, true);
            Player newPlayer = new Player(net, player, this);
            tmp.Add(newPlayer);

            if (rand_net_to_draw == i){
                network_to_draw = net;
            }
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
        Debug.Log("Num Species: " + species.Count);
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
        Debug.Log("GLOBAL FITNESS: " + calc_global_fitness() + " At gen: " + gen_counter);
        // string tmp = calc_global_fitness:
        float global_fit = calc_global_fitness();
        string tmp = System.String.Format("{0:0.00}", global_fit);
        if (global_fit < 10.0f){
            tmp = "0"  + tmp;
        }
        all_fitnesses += tmp + ",";


        // if (gen_counter == numGensToWrite){
        //     using (StreamWriter w = File.AppendText(Application.dataPath + "/StreamingAssets/test.txt"))
        //     {
        //         w.WriteLine("" + line_tag + " Pop Size: " + populationSize + " Mut Rate: " + MUTATION_RATE + " Add Conn Rate: " + ADD_CONNECTION_RATE + " Add Node Rate: " + ADD_NODE_RATE + " C1: " + c1 + " C2: " + c2 + " C3: " + c3 + " DT: " + DT);
        //         w.WriteLine(line_tag + all_fitnesses);
        //     }
        // }

        if (saveNextGen || (gen_counter % 300 == 0 && gen_counter > 0)){
            saveBestModel();
        }

        // if (gen_counter % newMazeAfter == 0 && gen_counter > 0){
        //     GameObject.Find("MazeGenerator").GetComponent<MazeGenerator>().OnClickGenerateMaze();
        // }

    }

    private void createNextGen(){
        network_to_draw = getBestNetwork();

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
            // p1.network.printNetwork("======== PARENT1 " + p1.player_GO.name + " with fitness: " + p1.fitness + " ========");
            // p2.network.printNetwork("======== PARENT2 " + p2.player_GO.name + " with fitness: " + p2.fitness + " ========");
            NeatNetwork childNet = NeatNetwork.crossover(p1.network, p2.network);
            // childNet.printNetwork("======= CHILD =======");
            float r1 = Random.Range(0.0f, 1.0f);
            float r2 = Random.Range(0.0f, 1.0f);
            float r3 = Random.Range(0.0f, 1.0f);
            if (r1 < MUTATION_RATE){
                // Debug.Log("MUTATED");
                childNet.mutate();
            }
            if (r2 < ADD_CONNECTION_RATE){
                // Debug.Log("ADDED CONNECTION");
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

    private List<Vector3> getGatePositions(){
        List<Vector3> tmp_rtn = new List<Vector3>();
        for (int i = 0; i < pairs.transform.childCount; i+=2){
            Transform p1 = pairs.transform.GetChild(i);
            Transform p2 = pairs.transform.GetChild(i+1);

            Vector3 tmp_pos_1 = p1.position;
            tmp_pos_1.x += p1.localScale.x / 2;
            Vector3 tmp_pos_2 = p2.position;
            tmp_pos_2.x -= p2.localScale.x / 2;
            Vector3 newPos = (tmp_pos_1 + tmp_pos_2) / 2;
            GameObject prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prim.transform.localScale = new Vector3(2,2,2);
            prim.transform.position = newPos;
            tmp_rtn.Add(newPos);
        }
        return tmp_rtn;
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

    private NeatNetwork getBestNetwork(){
        float bestFitness = -1.0f;
        NeatNetwork bestP = null;
        foreach (Player p in players){
            if (p.fitness > bestFitness){
                bestFitness = p.fitness;
                bestP = p.network;
            }
        }

        return bestP;
    }

    private Player spawnPlayer(NeatNetwork network, Vector3 pos){
        GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
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
        // for (int i = 0; i < ray_dists.Count; i++){
        //     inputs.Add(ray_dists[i]);
        // }
        // inputs.Add(target_GO.transform.position.x);
        // inputs.Add(target_GO.transform.position.z);

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
        float dist_from_targ = Vector3.Distance(p.player_GO.transform.position, target_GO.transform.position);
        float total_dist = Vector3.Distance(startPos, target_GO.transform.position);
        float change = total_dist - dist_from_targ;
        if (change < 0){ change = 0; }

        return change;
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

    public void OnClickSave(){
        saveNextGen = true;
    }

    public void OnClickTrain(){
        mode = Mode.TRAINING;
        pause = false;

        spawnPopulation();
    }

    public void OnClickTest(){
        mode = Mode.TESTING;
        int save_num;
        Debug.Log("WHICH SAVE: " + whichSave.text);
        try {
            save_num = int.Parse(whichSave.text);
        }
        catch {
            Debug.LogError("Failed To Get Save Num. Loading Save 0");
            save_num = 0;
        }



        // pause = false;
        NeatNetwork network = NeatNetwork.createNetworkFromSave(save_num, this);
        Player p = spawnPlayer(network, startPos);
        players.Add(p);
        populationSize = 1;

        pause = false;

    }
}
