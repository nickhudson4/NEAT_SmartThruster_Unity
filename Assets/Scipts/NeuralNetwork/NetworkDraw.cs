using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkDraw : MonoBehaviour
{
    NeatManager manager;
    NeatNetwork network;

    public Sprite nodeSprite;
    public Sprite connectionSprite;

    float screen_width;
    float screen_height;

    public GridLayoutGroup input_grid;
    public GridLayoutGroup output_grid;
    public GameObject hiddenNodesParent;
    public GameObject connectionsParent;

    public GameObject nodeTextPrefab;
    public GameObject nodeValuePrefab;

    private float cellSize;

    Dictionary<int, Node> nodes;
    Dictionary<int, Vector2> hiddenNodesPositions;
    List<Connection> connections;

    private bool drawNetwork = false;

    void Start() {
        manager = GameObject.Find("NeatManager").GetComponent<NeatManager>();

        nodes = new Dictionary<int, Node>();
        hiddenNodesPositions = new Dictionary<int, Vector2>();
        connections = new List<Connection>();
    }

    public void drawUpdate(List<float> inputs, List<float> outputs){
        if (!drawNetwork || network == null){ return; };
        manager.printFloatList(inputs);
        manager.printFloatList(outputs);

        foreach (var n in nodes){
            if (n.Key < network.input_size){ //Input node
                n.Value.valueText.text = inputs[n.Key].ToString("F1");
            }
            else if (n.Key < network.input_size + network.output_size && n.Key >= network.input_size){ //Output node
                n.Value.valueText.text = outputs[n.Key - network.input_size].ToString("F1");
            }
            else { //Hidden node

            }
        }
    }

    public void draw(NeatNetwork network){
        if (!drawNetwork){ return; };

        screen_width = Screen.width;
        screen_width = GetComponent<RectTransform>().rect.width;
        screen_height = GetComponent<RectTransform>().rect.height;
        screen_height = Screen.height;

        this.network = network;
        // network.printNetwork();
        clear();

        cellSize = 20.0f;
        input_grid.cellSize = new Vector2(20.0f, 20.0f);
        output_grid.cellSize = new Vector2(20.0f, 20.0f);

        setSpacing();

        foreach (NeatNetwork.Connection c in network.connections){
            if (network.nodes[c.in_node].type == NeatNetwork.Node.Type.INPUT && network.nodes[c.out_node].type == NeatNetwork.Node.Type.OUTPUT){
                if (!nodeExists(c.in_node)){
                    GameObject img_go = createImage(nodeSprite, input_grid.transform, Color.white);
                    // Canvas ca = img_go.AddComponent<Canvas>();
                    // ca.overrideSorting = true;
                    // ca.sortingOrder = 0;
                    int nodeNum = input_grid.transform.childCount;
                    Node n = new Node(img_go, getScreenPos(input_grid, nodeNum), nodeNum);
                    addNodeText(img_go, c.in_node);
                    addNodeValueText(n, 0.0f);
                    nodes.Add(c.in_node, n);
                }
                if (!nodeExists(c.out_node)){
                    GameObject img_go = createImage(nodeSprite, output_grid.transform, Color.white);
                    // Canvas ca = img_go.AddComponent<Canvas>();
                    // ca.overrideSorting = true;
                    // ca.sortingOrder = 0;
                    int nodeNum = output_grid.transform.childCount;
                    Node n = new Node(img_go, getScreenPos(output_grid, nodeNum), nodeNum);
                    addNodeText(img_go, c.out_node);
                    addNodeValueText(n, 0.0f);
                    nodes.Add(c.out_node, n);
                }
                connections.Add(new Connection(c.in_node, c.out_node, c.enabled));
                // addConnections(c.in_node, c.out_node);
            }
            else if (network.nodes[c.in_node].type == NeatNetwork.Node.Type.INPUT && network.nodes[c.out_node].type == NeatNetwork.Node.Type.HIDDEN){
                if (!nodeExists(c.in_node)){
                    GameObject img_go = createImage(nodeSprite, input_grid.transform, Color.white);
                    // Canvas ca = img_go.AddComponent<Canvas>();
                    // ca.overrideSorting = true;
                    // ca.sortingOrder = 0;
                    int nodeNum = input_grid.transform.childCount;

                    Node n = new Node(img_go, getScreenPos(input_grid, nodeNum), nodeNum);
                    addNodeText(img_go, c.in_node);
                    addNodeValueText(n, 0.0f);
                    nodes.Add(c.in_node, n);

                }
                if (!nodeExists(c.out_node)){ //Hidden Node
                    GameObject img_go = createImage(nodeSprite, hiddenNodesParent.transform, Color.white);
                    // Canvas ca = img_go.AddComponent<Canvas>();
                    // ca.overrideSorting = true;
                    // ca.sortingOrder = 0;
                    RectTransform r = img_go.GetComponent<RectTransform>();
                    Vector2 screenPos;
                    if (!hiddenNodesPositions.ContainsKey(c.out_node)){
                        screenPos = new Vector2(Random.Range(0.0f + cellSize*2, screen_width - cellSize*2), Random.Range(0.0f + cellSize, screen_height - cellSize));
                        hiddenNodesPositions.Add(c.out_node, screenPos);
                    }
                    else {
                        screenPos = hiddenNodesPositions[c.out_node];
                    }
                    r.position = screenPos;
                    r.sizeDelta = new Vector2(cellSize, cellSize);
                    addNodeText(img_go, c.out_node);
                    nodes.Add(c.out_node, new Node(img_go, screenPos, -1));




                }
                nodes[c.out_node].connectedTo.Add(c.in_node); //Add hidden node connection
                connections.Add(new Connection(c.in_node, c.out_node, c.enabled));

            }
            else if (network.nodes[c.in_node].type == NeatNetwork.Node.Type.HIDDEN && network.nodes[c.out_node].type == NeatNetwork.Node.Type.OUTPUT){

                if (!nodeExists(c.in_node)){
                    GameObject img_go = createImage(nodeSprite, hiddenNodesParent.transform, Color.white);
                    // Canvas ca = img_go.AddComponent<Canvas>();
                    // ca.overrideSorting = true;
                    // ca.sortingOrder = 0;
                    RectTransform r = img_go.GetComponent<RectTransform>();
                    Vector2 screenPos;
                    if (!hiddenNodesPositions.ContainsKey(c.in_node)){
                        screenPos = new Vector2(Random.Range(0.0f + cellSize*2, screen_width - cellSize*2), Random.Range(0.0f + cellSize, screen_height - cellSize));
                        hiddenNodesPositions.Add(c.in_node, screenPos);
                    }
                    else {
                        screenPos = hiddenNodesPositions[c.in_node];
                    }
                    r.position = screenPos;
                    r.sizeDelta = new Vector2(cellSize, cellSize);
                    addNodeText(img_go, c.in_node);
                    nodes.Add(c.in_node, new Node(img_go, screenPos, -1));
                }
                if (!nodeExists(c.out_node)){
                    GameObject img_go = createImage(nodeSprite, output_grid.transform, Color.white);
                    // Canvas ca = img_go.AddComponent<Canvas>();
                    // ca.overrideSorting = true;
                    // ca.sortingOrder = 0;
                    int nodeNum = output_grid.transform.childCount;

                    Node n = new Node(img_go, getScreenPos(output_grid, nodeNum), nodeNum);
                    addNodeText(img_go, c.out_node);
                    addNodeValueText(n, 0.0f);
                    nodes.Add(c.out_node, n);
                }
                nodes[c.in_node].connectedTo.Add(c.out_node); //Add hidden node connection
                connections.Add(new Connection(c.in_node, c.out_node, c.enabled));

            }
            else if (network.nodes[c.in_node].type == NeatNetwork.Node.Type.HIDDEN && network.nodes[c.out_node].type == NeatNetwork.Node.Type.HIDDEN){
                if (!nodeExists(c.in_node)){
                    GameObject img_go = createImage(nodeSprite, hiddenNodesParent.transform, Color.white);
                    // Canvas ca = img_go.AddComponent<Canvas>();
                    // ca.overrideSorting = true;
                    // ca.sortingOrder = 0;
                    RectTransform r = img_go.GetComponent<RectTransform>();
                    Vector2 screenPos;
                    if (!hiddenNodesPositions.ContainsKey(c.in_node)){
                        screenPos = new Vector2(Random.Range(0.0f + cellSize*2, screen_width - cellSize*2), Random.Range(0.0f + cellSize, screen_height - cellSize));
                        hiddenNodesPositions.Add(c.in_node, screenPos);
                    }
                    else {
                        screenPos = hiddenNodesPositions[c.in_node];
                    }
                    r.position = screenPos;
                    r.sizeDelta = new Vector2(cellSize, cellSize);
                    addNodeText(img_go, c.in_node);
                    nodes.Add(c.in_node, new Node(img_go, screenPos, -1));
                }
                if (!nodeExists(c.out_node)){
                    GameObject img_go = createImage(nodeSprite, hiddenNodesParent.transform, Color.white);
                    // Canvas ca = img_go.AddComponent<Canvas>();
                    // ca.overrideSorting = true;
                    // ca.sortingOrder = 0;
                    RectTransform r = img_go.GetComponent<RectTransform>();
                    Vector2 screenPos;
                    if (!hiddenNodesPositions.ContainsKey(c.out_node)){
                        screenPos = new Vector2(Random.Range(0.0f + cellSize*2, screen_width - cellSize*2), Random.Range(0.0f + cellSize, screen_height - cellSize));
                        hiddenNodesPositions.Add(c.out_node, screenPos);
                    }
                    else {
                        screenPos = hiddenNodesPositions[c.out_node];
                    }
                    r.position = screenPos;
                    r.sizeDelta = new Vector2(cellSize, cellSize);
                    addNodeText(img_go, c.out_node);
                    nodes.Add(c.out_node, new Node(img_go, screenPos, -1));
                }
                nodes[c.in_node].connectedTo.Add(c.out_node); //Add hidden node connection
                nodes[c.out_node].connectedTo.Add(c.in_node); //Add hidden node connection
                connections.Add(new Connection(c.in_node, c.out_node, c.enabled));
            }
        }
        // foreach (var n in nodes){
        //     if (n.Value.nodeNum == -1){
        //         Vector2 sumVector = Vector2.zero;
        //         foreach (int i in n.Value.connectedTo){
        //             sumVector += nodes[i].screenPos;
        //         }
        //         sumVector = sumVector / n.Value.connectedTo.Count;
        //         n.Value.screenPos = sumVector;
        //         n.Value.node_go.GetComponent<RectTransform>().position = sumVector;
        //     }
        // }
        foreach (Connection c in connections){
            GameObject line = addConnections(c.in_node, c.out_node, c.enabled);
            c.line_go = line;
        }
    }

    private void addNodeText(GameObject node, int nodeid){
        GameObject nodeNumText = Instantiate(nodeTextPrefab, nodeTextPrefab.transform.position, Quaternion.identity);
        nodeNumText.transform.SetParent(node.transform, false);

        nodeNumText.GetComponent<TextMeshProUGUI>().text = "" + nodeid;
    }

    private void addNodeValueText(Node node, float value){
        GameObject node_go = node.node_go;
        GameObject nodeValueText = Instantiate(nodeValuePrefab, nodeValuePrefab.transform.position, Quaternion.identity);
        nodeValueText.transform.SetParent(node_go.transform, false);
        nodeValueText.GetComponent<TextMeshProUGUI>().text = "" + value;

        node.valueText = nodeValueText.GetComponent<TextMeshProUGUI>();
    }

    private GameObject addConnections(int n1, int n2, bool enabled){
        Node node1 = nodes[n1];
        Node node2 = nodes[n2];
        Color color;
        if (enabled){
            color = new Color(52.0f/255.0f, 235.0f/255.0f, 79.0f/255.0f,  1.0f);
        }
        else {
            color = new Color(235.0f/255.0f, 64.0f/255.0f, 52.0f/255.0f,  1.0f);
        }
        GameObject line = createImage(connectionSprite, connectionsParent.transform, color);
        // Canvas ca = line.AddComponent<Canvas>();
        // ca.overrideSorting = true;
        // ca.sortingOrder = 1;

        RectTransform line_rect = line.GetComponent<RectTransform>();
        float length = Vector2.Distance(node1.screenPos, node2.screenPos) - cellSize;
        line_rect.sizeDelta = new Vector2(length, 3.0f);
        line_rect.position = getPointBetween(node1.screenPos, node2.screenPos);
        Vector2 diff = node2.screenPos - node1.screenPos;
        line.transform.right = diff;

        return line;
    }

    private Vector2 getPointBetween(Vector2 p1, Vector2 p2){
        return (p1 + p2) / 2;
    }

    private GameObject createImage(Sprite sprite, Transform parent, Color color){
        GameObject img_go = new GameObject(); //Create the GameObject
        Image img = img_go.AddComponent<Image>(); //Add the Image Component script
        img.sprite = sprite; //Set the Sprite of the Image Component on the new GameObject
        img_go.GetComponent<RectTransform>().SetParent(parent); //Assign the newly created Image GameObject as a Child of the Parent Panel.
        img_go.SetActive(true); //Activate the GameObject
        img_go.GetComponent<Image>().color = color;


        return img_go;
    }

    private bool nodeExists(int id){
        return nodes.ContainsKey(id);
    }

    private Vector2 getScreenPos(GridLayoutGroup grid, int cell_num){
        // Debug.Log("WIDTH: " + grid.gameObject.GetComponent<RectTransform>().rect.width);
        RectTransform grid_rect = grid.gameObject.GetComponent<RectTransform>();
        Vector2 grid_pos = grid.transform.position;

        int numRows = (int)grid_rect.rect.height / (int)grid.cellSize.y;
        int numCols = (int)grid_rect.rect.width / (int)grid.cellSize.x;
        // Debug.Log("Num rows: " + numRows);
        // Debug.Log("widdth: " + grid_rect.rect.width + " cellSize: " + grid.cellSize.x);
        // Debug.Log("Num cols: " + numCols);
        int row;
        int col;
        if (cell_num <= numRows){
            row = cell_num - 1;
            col = 0;
        }
        else {
            row = (cell_num % numRows) - 1;
            col = cell_num / numRows;
        }
        // Debug.Log("CellNum: " + cell_num + " numRows: " + numRows);

        // Debug.Log("ROW: " + row + " COL: " + col);
        Vector2 pos = new Vector2(grid_pos.x + (grid.cellSize.x * col), ((grid_pos.y + grid_rect.rect.height/2) - cellSize/2) - (grid.cellSize.y * row));
        pos.y -= (grid.spacing.y) * (cell_num - 1);
        // if (cell_num > 1){
        //     pos.y -= (spacing_y * (cell_num - 1));
        // }
        // GameObject tmp_img = createImage(nodeSprite, this.transform, Color.red);
        // tmp_img.GetComponent<RectTransform>().position = pos;
        // tmp_img.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);


        return pos;
    }

    private void setSpacing(){
        float total_room_left_input = screen_height - (cellSize * network.input_size);
        input_grid.spacing = new Vector2(0.0f, total_room_left_input / network.input_size);

        float total_room_left_output = screen_height - (cellSize * network.output_size);
        output_grid.spacing = new Vector2(0.0f, total_room_left_output / network.output_size);
    }

    private void clear(){
        for (int i = 0; i < transform.childCount; i++){
            GameObject tmp = transform.GetChild(i).gameObject;
            // if (tmp.name == "HiddenNodes"){ continue; }
            for (int j = 0; j < tmp.transform.childCount; j++){
                GameObject tmp2 = tmp.transform.GetChild(j).gameObject;
                Destroy(tmp2);
            }
            tmp.transform.DetachChildren();
        }
        nodes.Clear();
        connections.Clear();
    }

    public void OnToggleChangeDrawNetwork(Toggle change){
        drawNetwork = change.isOn;
        if (drawNetwork){
            if (network != null){
                draw(network);
            }
        }
        else {
            clear();
        }
    }

    public class Node {
        public Node (GameObject node_go, Vector2 screenPos, int nodeNum){
            this.node_go = node_go;
            this.screenPos =  screenPos;
            this.nodeNum = nodeNum;

            this.connectedTo = new List<int>();
        }
        public GameObject node_go;
        public Vector2 screenPos;
        public int nodeNum;

        public List<int> connectedTo; //Used for hidden node positioning

        public TextMeshProUGUI valueText;
    }
    public class Connection {
        public Connection(int in_node, int out_node, bool enabled){
            this.in_node = in_node;
            this.out_node = out_node;
            this.enabled = enabled;
        }
        public int in_node;
        public int out_node;
        public bool enabled;
        public GameObject line_go;
    }
}
