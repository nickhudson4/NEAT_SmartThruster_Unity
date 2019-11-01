using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    // public GameObject playerPrefab;
    // public int populationSize;
    // public GameObject target_GO;
    // public GameObject db1;
    // public GameObject db2;


    // //POPULATION VARS
    // private List<Player> players;
    // public int isDeadCount = 0;
    // private float maxStuckTime = 5.0f;
    // private Vector3 startPos = new Vector3(0.0f, 0.8f, -11.0f);

    // //MUTATION VARS
    // public float mutationRate;
    // private int gen_num = 0;
    // public Material child_mat;
    // public Material parent_mat;
    // public Material random_mat;

    // //NEURAL NET VARS
    // private int inputSize = 10;
    // private int hiddenSize = 10;
    // private int numHidden = 1;
    // private int outputSize = 1;

    // //RUN VARS
    // private float maxRunTime = 7.0f;
    // public float runTimeCounter = 0;

    // //Raycast variables
    // int layerMask = 1 << 9;

    // //MISC
    // public TextMeshProUGUI mut_rate_tmp;
    // public TextMeshProUGUI current_gen_tmp;


    // NeuralNetwork tmp_net;


    // void Start(){
    //     // tmp_net = new NeuralNetwork(inputSize, 6, outputSize, 1);
    //     players = spawnPopulation(startPos);
    //     mut_rate_tmp.text = "Mutation Rate: " + mutationRate;
    // }

    // void Update(){
    //     run();
    // }

    // private void run(){
    //     for (int i = 0; i < players.Count; i++){
    //         // Player tmp = players[i];
    //         if (players[i].isDead){ continue; }

    //         List<float> dists = getRaycastDistances(players[i]);
    //         Matrix inputs = createInputMatrix(players[i], dists);
    //         Matrix outputs = players[i].network.train(inputs);
    //         // int dir = players[i].network.getPrediction(outputs);
    //         // players[i].controller.applyForceOnSide(dir, outputs.get(dir));
    //         // players[i].controller.applyForces(outputs);
    //         //!OLD
    //         // players[i].controller.applyForceOnAxis(changeRange(outputs.get(0)), changeRange(outputs.get(1)));
    //         //!

    //         //!NEW
    //             float horiz = changeRange(outputs.get(0));
    //             players[i].controller.moveForwardWithRot(horiz);
    //         //!
    //         // outputs.printMatrix("OUTPUTS FOR: " + players[i].player_GO.name);
    //         // Debug.Log("Output 0 : " + changeRange(outputs.get(0)) + " OUTPUT 1: " + changeRange(outputs.get(1)));
    //         // Debug.Log(players[i].controller.rb.velocity.magnitude);

    //         // if (players[i].controller.rb.velocity.magnitude <= 0.07){
    //         //     players[i].stuckCounter += Time.deltaTime;
    //         // }
    //         // else {
    //         //     players[i].stuckCounter = 0;
    //         // }


    //         // players[i].network.printNetwork("******** " + i + " *********");


    //         float changeInPos = Vector3.Distance(players[i].player_transform.position, players[i].lastPos);
    //         // Debug.Log("Change in pos: " + changeInPos);
    //         if (changeInPos <= 0.01){
    //             players[i].stuckCounter += Time.deltaTime;
    //         }
    //         else {
    //             players[i].stuckCounter = 0;
    //         }

    //         players[i] = players[i];
    //         if (players[i].stuckCounter >= maxStuckTime){
    //             onDeath(i);
    //         }
    //         if (runTimeCounter >= maxRunTime){
    //             // Debug.Log("HERE");
    //             onDeath(i);
    //         }

    //         players[i].lastPos = players[i].player_transform.position;

    //     }

    //     runTimeCounter += Time.deltaTime;
    //     if (isDeadCount >= players.Count){
    //         setupNextGen();
    //     }
    // }

    // private void setupNextGen(){
    //         calc_fitness();
    //         runTimeCounter = 0;

    //         List<Player> parents = selectParents(players);
    //         mutate(parents);
    //         // Debug.Log("888888 OUT RESET 888888");

    //         isDeadCount = 0;
    //         gen_num++;
    //         current_gen_tmp.text = "Gen: " + gen_num;
    // }

    // private List<Player> selectParents(List<Player> players){

    //         List<Player> selected = new List<Player>();

    //         //TODO: FINISH THIS
    //         int wheel_size = 1000;
    //         List<Player> prob_wheel = new List<Player>();
    //         foreach(Player p in players){
    //             // float percent = 100.0f / (float)players.Count;
    //             // float cutoff_decimal = percent/100.0f;
    //             // if (p.fitness < cutoff_decimal){ continue; }



    //             int num_entrys = (int)(p.fitness * wheel_size);
    //             Debug.Log("" + p.player_GO.name + " Num entries: " + num_entrys + " with fitness: " + p.fitness);
    //             for (int i = 0; i < num_entrys; i++){
    //                 prob_wheel.Add(p);
    //             }

    //         }
    //         // Debug.Log("WHEEL SIZE: " + prob_wheel.Count);
    //         int rand_pick_1 = Random.Range(0, prob_wheel.Count);

    //         selected.Add(prob_wheel[rand_pick_1]);
    //         int rand_pick_2 = Random.Range(0, prob_wheel.Count);
    //         Player selection2 = prob_wheel[rand_pick_2];
    //         while (selection2.player_GO == selected[0].player_GO){
    //             rand_pick_2 = Random.Range(0, prob_wheel.Count);
    //             selection2 = prob_wheel[rand_pick_2];
    //         }
    //         selected.Add(selection2);

    //         db1.transform.position = selected[0].player_transform.position;
    //         db2.transform.position = selected[1].player_transform.position;
    //         foreach(Player p in selected){
    //             Debug.Log("selected player with fitness: " + p.fitness, p.player_GO);
    //             // p.network.layers[1].biases.printMatrix("PARENT");
    //             setMaterialColor(p, "parent");
    //             // p.network.printNetwork("********* PARENT ORIG: " + p.player_GO.name + " **********");


    //         }

    //         return selected;
    // }

    // private void mutate(List<Player> parents){
    //     int mut_count = 0;
    //     for(int i = 0; i < players.Count; i++){
    //         if (players[i].player_GO == parents[0].player_GO || players[i].player_GO == parents[1].player_GO){
    //             resetPlayer(i);
    //             continue;

    //         }
    //         //TODO: TEST THIS
    //         float mutateChance = Random.Range(0.0f, 1.0f);
    //         if (mutateChance < mutationRate){ //Spawn with parents genes
    //             players[i].network.mutate(parents[0].network, parents[1].network);
    //             // players[i].network = new NeuralNetwork(inputSize, hiddenSize, outputSize, numHidden);
    //             setMaterialColor(players[i], "child");
    //             // players[i].network.setupNetwork(players[i].network.input_layer_size, players[i].network.hidden_layer_size, players[i].network.output_layer_size, players[i].network.num_hidden_layers);
    //             mut_count++;
    //         }
    //         else { //Spawn brand new player
    //             players[i].network = new NeuralNetwork(inputSize, hiddenSize, outputSize, numHidden);
    //             setMaterialColor(players[i], "random");

    //         }


    //         resetPlayer(i);


    //     }
    //     // Debug.Log("NUM MUTATIONS: " + mut_count);

    // }

    // private void resetPlayer(int playerIndex){
    //         // Debug.Log("888888 IN RESET 888888");
    //     Player tmp2 = players[playerIndex];
    //     tmp2.isDead = false;
    //     tmp2.stuckCounter = 0;
    //     tmp2.controller.resetPlayer(startPos);
    //     players[playerIndex] = tmp2;

    // }

    // private void setMaterialColor(Player p, string mut){
    //     if (mut == "parent"){
    //         p.player_GO.GetComponent<Renderer>().material = parent_mat;

    //     }
    //     else if (mut == "child"){
    //         p.player_GO.GetComponent<Renderer>().material = child_mat;

    //     }
    //     else if (mut == "random"){
    //         p.player_GO.GetComponent<Renderer>().material = random_mat;

    //     }
    // }

    // private List<float> getRaycastDistances(Player p){
    //      RaycastHit hit;
    //      Vector3 forward_right = getDirBetween(p.player_transform.forward, p.player_transform.right);
    //      Vector3 forward_left = getDirBetween(p.player_transform.forward, -p.player_transform.right);
    //      Vector3 back_right = getDirBetween(-p.player_transform.forward, p.player_transform.right);
    //      Vector3 back_left = getDirBetween(-p.player_transform.forward, -p.player_transform.right);

    //     List<float> distances = new List<float>();

    //     if (Physics.Raycast(p.player_transform.position, p.player_transform.forward, out hit, Mathf.Infinity, layerMask)){
    //         distances.Add(Vector3.Distance(p.player_transform.position, hit.transform.position));
    //         // Debug.DrawRay(p.player_transform.position, p.player_transform.forward, Color.green);
    //     }
    //     if (Physics.Raycast(p.player_transform.position, forward_right, out hit, Mathf.Infinity, layerMask)){
    //         distances.Add(Vector3.Distance(p.player_transform.position, hit.transform.position));
    //         // Debug.DrawRay(p.player_transform.position, forward_right, Color.green);
    //     }
    //     if (Physics.Raycast(p.player_transform.position, p.player_transform.right, out hit, Mathf.Infinity, layerMask)){
    //         distances.Add(Vector3.Distance(p.player_transform.position, hit.transform.position));
    //         // Debug.DrawRay(p.player_transform.position, p.player_transform.right, Color.green);
    //     }
    //     if (Physics.Raycast(p.player_transform.position, back_right, out hit, Mathf.Infinity, layerMask)){
    //         distances.Add(Vector3.Distance(p.player_transform.position, hit.transform.position));
    //         // Debug.DrawRay(p.player_transform.position, back_right, Color.green);
    //     }
    //     if (Physics.Raycast(p.player_transform.position, -p.player_transform.forward, out hit, Mathf.Infinity, layerMask)){
    //         distances.Add(Vector3.Distance(p.player_transform.position, hit.transform.position));
    //         // Debug.DrawRay(p.player_transform.position, -p.player_transform.forward, Color.green);
    //     }
    //     if (Physics.Raycast(p.player_transform.position, back_left, out hit, Mathf.Infinity, layerMask)){
    //         distances.Add(Vector3.Distance(p.player_transform.position, hit.transform.position));
    //         // Debug.DrawRay(p.player_transform.position, back_left, Color.green);
    //     }
    //     if (Physics.Raycast(p.player_transform.position, -p.player_transform.right, out hit, Mathf.Infinity, layerMask)){
    //         distances.Add(Vector3.Distance(p.player_transform.position, hit.transform.position));
    //         // Debug.DrawRay(p.player_transform.position, -p.player_transform.right, Color.green);
    //     }
    //     if (Physics.Raycast(p.player_transform.position, forward_left, out hit, Mathf.Infinity, layerMask)){
    //         distances.Add(Vector3.Distance(p.player_transform.position, hit.transform.position));
    //         // Debug.DrawRay(p.player_transform.position, forward_left, Color.green);
    //     }
    //     return distances;
    // }

    // private Matrix createInputMatrix(Player p, List<float> dists){
    //     Matrix mat = new Matrix(inputSize, 1);
    //     int i;
    //     for (i = 0; i < dists.Count; i++){
    //         mat.insert(dists[i], i);
    //     }
    //     mat.insert(target_GO.transform.position.x, i);
    //     i++;
    //     mat.insert(target_GO.transform.position.z, i);
    //     return mat;
    // }

    // private List<Player> spawnPopulation(Vector3 pos){
    //     List<Player> tmp = new List<Player>();
    //     for (int i = 0; i < populationSize; i++){
    //         GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
    //         player.name = "Player_" + i;
    //         Player newPlayer = new Player(player, pos);
    //         NeuralNetwork myNetwork = new NeuralNetwork(inputSize, hiddenSize, outputSize, numHidden);
    //         newPlayer.network = myNetwork;
    //         // newPlayer.network = tmp_net;
    //         tmp.Add(newPlayer);
    //     }

    //     return tmp;
    // }

    // public int getPlayerIndexFromObj(GameObject p_obj){
    //     for (int i = 0; i < players.Count; i++){
    //         if (players[i].player_GO == p_obj){
    //             return i;
    //         }
    //     }
    //     return -1;
    // }

    // private float calc_score(Player p){
    //     float dist_from_targ = Vector3.Distance(p.player_transform.position, target_GO.transform.position);
    //     float total_dist = Vector3.Distance(startPos, target_GO.transform.position);
    //     float change = total_dist - dist_from_targ;
    //     if (change < 0){ change = 0; }

    //     return change;
    // }

    // private void calc_fitness(){
    //     float scoreSum = 0;
    //     for (int i = 0; i < players.Count; i++){
    //         scoreSum += players[i].score;
    //     }

    //     for (int i = 0; i < players.Count; i++){
    //         Player p = players[i];
    //         p.fitness = p.score / scoreSum;
    //         players[i] = p;
    //     }
    // }

    // private float changeRange(float val){
    //     return (val - 0.5f) * 2;
    // }

    // public void OnPlayerWallHitHelper(GameObject p_obj, GameObject otherObject){
    //     int player_index = getPlayerIndexFromObj(p_obj);
    //     if (otherObject.tag == "MazeWall"){
    //         onDeath(player_index);
    //     }
    //     else if (otherObject.tag == "MazeTarget"){
    //         onMazeCompletion(player_index);
    //     }


    // }

    // private void onDeath(int playerIndex){
    //     // Debug.Log("HERE FOR : " + players[playerIndex].player_GO.name);
    //     players[playerIndex].isDead = true;
    //     players[playerIndex].score = calc_score(players[playerIndex]);
    //     // players[playerIndex].player_GO.SetActive(false);
    //     players[playerIndex].controller.rb.isKinematic = true;
    //     isDeadCount++;
    // }

    // private void onMazeCompletion(int playerIndex){
    //     // Player tmp = players[playerIndex];
    //     onDeath(playerIndex);
    // }

    // private Vector3 getDirBetween(Vector3 dir1, Vector3 dir2){ //Dir between right angle formed by dir1 and dir2
    //     Vector3 newDir = Vector3.zero;
    //     newDir += dir1/2;
    //     newDir += dir2/2;

    //     return newDir;
    // }

    // public Vector2 vector3_to_vector2(Vector3 orig, string dropAxis){
    //     if (dropAxis == "y"){
    //         return new Vector2(orig.x, orig.z);
    //     }
    //     if (dropAxis== "x"){
    //         return new Vector2(orig.y, orig.z);
    //     }
    //     else {
    //         return new Vector2(orig.x, orig.y);
    //     }
    // }

    // public Vector3 vector2_to_vector3(Vector2 orig, string dropAxis){
    //     if (dropAxis == "y"){
    //         return new Vector3(orig.x, 0, orig.y);
    //     }
    //     if (dropAxis== "x"){
    //         return new Vector3(0, orig.x, orig.y);
    //     }
    //     else {
    //         return new Vector3(orig.x, orig.y, 0);
    //     }
    // }

    // IEnumerator sleep()
    // {
    //     print(Time.time);
    //     yield return new WaitForSeconds(5);
    //     print(Time.time);
    // }

    // public class Player{
    //     public Player(GameObject player_GO, Vector3 startPos){
    //         this.player_GO = player_GO;
    //         this.player_transform = player_GO.transform;
    //         this.network = new NeuralNetwork(0, 0, 0, 0);
    //         this.controller = player_GO.GetComponent<PlayerController>();

    //         this.startPos = startPos;

    //         this.isDead = false;
    //         this.score = 0;
    //         this.fitness = 0;
    //         this.stuckCounter = 0;
    //         this.lastPos = Vector3.zero;
    //     }

    //     public NeuralNetwork network;
    //     public GameObject player_GO;
    //     public Transform player_transform;
    //     public PlayerController controller;

    //     public Vector3 startPos;

    //     public bool isDead;
    //     public float score;
    //     public float fitness;
    //     public float stuckCounter;
    //     public Vector3 lastPos;

    // }
}