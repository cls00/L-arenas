using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RunSim : MonoBehaviour
{
    //simulation variables
    public bool UseParameterFile = false;
    public GameObject FoodPrefab;
    public GameObject AgentPrefab;
    public GameObject SArenaPrefab;
    public Material AgentMat;

    public string writepath = "experiment.csv"; //write simulation data to this filename
    public string writepathPopulation = "population.csv"; //write simulation data to this filename
    public string writepathConns = "connections.csv";
    private List<(string, string, string)> connectionHistory = new List<(string, string, string)>();   //connection list as FromAgent ToAgent ArenaNum
    private int numRules;    

    //environment variables
    public float arenaSize = 30f;
    public int patchNum = 10;
    public int arenasPerRow = 1;
    public float arenaSeparator = 50f;
    public int foodAmount = 24;
    public float foodProb = 0.01f;
    public string foodRespawn = "basic";
    public int agentNum = 12;
    public float mutationRate = 0.01f; 
    public float energyPerFoodBlock = 200;
    private float x0;
    //private float z0;
    private float xzLim;
    private int OverallTicks;
    private float arenaLimitSquared;

    //agent variables
    public int minClonePoints = 700;
    public int startingPoints = 1;
    public float cloningPointLoss = 500f;
    public float pointLoss = 0.2f;
    public int nearestRadius = 20;
    public int agentCounter = 0;

    //collections
    private List<GameObject> arenaList = new List<GameObject>();
    private Dictionary<(GameObject, GameObject), GameObject> PhysicalConn = new Dictionary<(GameObject, GameObject), GameObject>();
    private List<(GameObject, GameObject)> connClearer = new List<(GameObject, GameObject)>();

    // Views and navigation
    public int connectionsViewMode = 2;
    public int agentColourMode = 0;

    //auxiliary
    private string experimentPath = "";
    private int seed;

    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 30;
        seed = System.DateTime.Now.Millisecond;
        Random.InitState(seed);
        experimentPath = Application.persistentDataPath + "/experiment-" +foodRespawn+ "_" + System.DateTime.Now.Year +"_" + System.DateTime.Now.Month + "_" + System.DateTime.Now.Day + "-" + System.DateTime.Now.Hour
                                                       + "-" + System.DateTime.Now.Minute + "-" + System.DateTime.Now.Millisecond;

        if (UseParameterFile)
        {           
            GetComponent<ParametersReader>().LoadParams();
            
        }
        
        Directory.CreateDirectory(experimentPath);
        OverallTicks = 0;
        arenaLimitSquared = Mathf.Pow(arenaSize, 2);
        numRules = AgentPrefab.GetComponent<Genome>().numRules;

        //write file header
        WriteData("ArenaName, SharingPref, GeneSequence, ConnectionNumber, GenerationNum, AgentName, GenitorName, FoodCollected, FoodGiven, FoodReceived, BirthTime, DeathTime", writepath);
        WriteData("ArenaName, Population Every 100 Ticks", writepathPopulation);
        WriteData("FromAgent, ToAgent, Arena", writepathConns);
        
        Debug.Log("Experiment Logs saved at: " + Application.persistentDataPath);

        //spawn patches
        x0 = 0f;
        //z0 = 0f;
        var row = 0;
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, 0);
            if (i == arenasPerRow) { row++; }
            x0 += arenaSize * 2 + arenaSeparator;
            GameObject arena = Instantiate(SArenaPrefab, position, Quaternion.identity);
            arena.transform.parent = gameObject.transform; // link arena to RunSim
            arena.transform.Find("Sphere").transform.localScale *= arenaSize * 2.1f;
            arena.name = "Arena" + i.ToString();
            arenaList.Add(arena);
            if (i == 0)
            {
                GetComponent<ArenaManager>().HighConnHighShare(arena, agentNum);
            }
            if (i == 1)
            {
                GetComponent<ArenaManager>().HighConnLowShare(arena, agentNum);
            }
            if (i == 2)
            {
                GetComponent<ArenaManager>().LowConnHighShare(arena, agentNum);
            }
            if (i == 3)
            {
                GetComponent<ArenaManager>().LowConnLowShare(arena, agentNum);
            }

            //Spawn initial food in random locations
            for (var k = 0; k < foodAmount; k++)
            {
                Vector3 arenaProximity = Random.insideUnitSphere * arenaSize + position;
                SpawnSingleFood(arenaProximity, energyPerFoodBlock, arena);
            }
        }
    }

    void FixedUpdate()
    {
        OverallTicks += 1;

        if (OverallTicks % 100 == 0)
        {
            WritePopulation(writepathPopulation);
        }

        //spawn food with food probability
        foreach (GameObject arena in arenaList)
        {
            if (arena.GetComponent<ArenaInfo>().Agents > 0)
            {
                GetComponent<ArenaManager>().SpawnFood(arena, foodRespawn);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        controls();

        //show physical connections between agents
        switch (connectionsViewMode)
        {
            // No connections displayed
            case 0:
                foreach (var conn in PhysicalConn)
                {
                    if (conn.Value != null)
                    {
                        conn.Value.GetComponent<LineRenderer>().SetPosition(0, conn.Key.Item1.transform.position);
                        conn.Value.GetComponent<LineRenderer>().SetPosition(1, conn.Key.Item1.transform.position);
                    }

                }
                break;
            // real time energy transfer
            case 1:
                foreach (var conn in PhysicalConn)
                {

                    if (conn.Key.Item1 != null && conn.Key.Item2 != null)
                    {
                        //chris edit
                        if (conn.Key.Item1.GetComponent<CollectFood>().getTransferList().Contains(conn.Key.Item2.gameObject))
                        {
                            conn.Value.GetComponent<LineRenderer>().SetPosition(0, conn.Key.Item1.transform.position);
                            conn.Value.GetComponent<LineRenderer>().SetPosition(1, conn.Key.Item2.transform.position);
                            conn.Key.Item1.GetComponent<CollectFood>().getTransferList().Remove(conn.Key.Item2.gameObject);
                            //Debug.Log("transfer" + conn.Key.Item1.name + " to " + conn.Key.Item2.name);
                        }
                        else
                        {
                            // do not draw the line (the two points are the same point... hiding the line)
                            conn.Value.GetComponent<LineRenderer>().SetPosition(0, conn.Key.Item1.transform.position);
                            conn.Value.GetComponent<LineRenderer>().SetPosition(1, conn.Key.Item1.transform.position);
                        }
                    }
                    else
                    {
                        connClearer.Add(conn.Key);
                    }
                }
                foreach (var conn in connClearer)
                {
                    Destroy(PhysicalConn[conn]);
                    PhysicalConn.Remove(conn);
                }
                connClearer.Clear();
                break;
            // connection network full
            case 2:
                foreach (var conn in PhysicalConn)
                {
                    //debug
                    if (conn.Key.Item1 == conn.Key.Item2)
                    {
                        //Debug.Log("Loop!! at " + conn.Key.Item1);
                    }
                    if (conn.Key.Item1 != null && conn.Key.Item2 != null)
                    {
                        conn.Value.GetComponent<LineRenderer>().SetPosition(0, conn.Key.Item1.transform.position);
                        conn.Value.GetComponent<LineRenderer>().SetPosition(1, conn.Key.Item2.transform.position);
                    }
                    else
                    {
                        connClearer.Add(conn.Key);
                    }
                }
                foreach (var conn in connClearer)
                {
                    Destroy(PhysicalConn[conn]);
                    PhysicalConn.Remove(conn);
                }

                connClearer.Clear();
                break;
            default:
                break;
        }

        // check boundaries ("spherical arenas")
    }


    public void SpawnSingleFood(Vector3 position, float energy, GameObject parent)
    {
        GameObject food = Instantiate(FoodPrefab, position, Quaternion.identity);
        food.transform.SetParent(parent.transform, true);
        food.GetComponent<FoodVars>().energy = energy;
    }


    public GameObject CreateAgent(Vector3 position)
    {
        GameObject baby = Instantiate(AgentPrefab, position, Quaternion.identity);
        baby.GetComponent<AgentMotion>().SetEnvironment(this);
        baby.GetComponent<Genome>().SetEnvironment(this);
        baby.GetComponent<Lifecycle>().SetEnvironment(this);
        baby.GetComponent<CollectFood>().SetEnvironment(this);
        baby.GetComponent<Lifecycle>().points = startingPoints;
        baby.GetComponent<Genome>().SharingPref = 50; //start sharing preference at 50/50 for everyone in 1st gen 
        // baby.GetComponent<Genome>().SetRan(ref referenceRandom);
        agentCounter += 1;
        baby.name = "Agent" + agentCounter.ToString();
        //baby.GetComponent<AgentBehaviour>().SetBirth(OverallTicks);
        return baby;
    }


    //This f'n is for creating clones
    public GameObject CloneAgent(GameObject cloneParent)
    {
        var position = Random.onUnitSphere + cloneParent.transform.position; //vec in the proximity of the cloning agent
        GameObject baby = Instantiate(AgentPrefab, position, Quaternion.identity);
        baby.GetComponent<AgentMotion>().SetEnvironment(this);
        baby.GetComponent<Genome>().SetEnvironment(this);
        baby.GetComponent<Lifecycle>().SetEnvironment(this);
        baby.GetComponent<CollectFood>().SetEnvironment(this);
        baby.transform.SetParent(cloneParent.transform.parent, true); //sets the clone parent's arena as the baby's parent
        baby.GetComponent<Lifecycle>().lastAncestor = cloneParent.name;
        baby.GetComponent<Lifecycle>().generationNum = cloneParent.GetComponent<Lifecycle>().generationNum + 1;
        baby.GetComponent<Lifecycle>().points = startingPoints;
        agentCounter += 1;
        baby.name = "Agent" + agentCounter.ToString();
        //baby.GetComponent<AgentBehaviour>().SetBirth(OverallTicks);
        List<int> genes = DeepCopyList(cloneParent.GetComponent<Genome>().GeneSequence); //baby inherits gene sequence from parent, with chance of mutation
        baby.GetComponent<Genome>().GeneSequence = genes;
        baby.transform.parent.GetComponent<ArenaInfo>().Agents++;
        return baby;
    }


    //copy with chance of mutation
    List<int> DeepCopyList(List<int> ListToCopy)
    {
        List<int> ListToPaste = new List<int>();
        for (int i = 0; i < ListToCopy.Count; i++)
        {
            if (Random.value <= mutationRate)
            {
                ListToPaste.Add(Random.Range(0,numRules));
            }
            else
            {
                ListToPaste.Add(ListToCopy[i]);
            }

        }
        return ListToPaste;
    }


    public void RandomConnect(GameObject agent, bool oneway)
    {
        Transform currentArena = agent.transform.parent;
        List<GameObject> currentAgents = new List<GameObject>();

        foreach (Transform child in currentArena)
        {
            if (child.name.Contains("Agent"))
            {
                currentAgents.Add(child.gameObject);
            }
        }
        //connects agent to random agent in arena
        int ranInd = Random.Range(0, currentAgents.Count);


        if (agent != currentAgents[ranInd] && !PhysicalConn.ContainsKey((agent, currentAgents[ranInd])) && oneway == false)
        {
            ConnectAgents(agent, currentAgents[ranInd]);
            ShowConnection(agent, currentAgents[ranInd]);
        }
        if (agent != currentAgents[ranInd] && !PhysicalConn.ContainsKey((agent, currentAgents[ranInd])) && oneway == true)
        {
            agent.GetComponent<CollectFood>().Connections.Add(currentAgents[ranInd]);
            ShowConnection(agent, currentAgents[ranInd]);
        }
    }


    public void ConnectNearest(GameObject currAgent)
    {
        Vector3 center = currAgent.transform.position;
        int radius = nearestRadius;

        Collider[] hits = Physics.OverlapSphere(center, radius);
        List<GameObject> CollisionAgents = new List<GameObject>();

        foreach (Collider hit in hits)
        {
            if (hit.tag == "Agent" && hit != currAgent) //change this to name.contains and get rid of agent tag
            {
                CollisionAgents.Add(hit.gameObject);
            }
        }
        //connects agent with closest agent within a radius
        if (CollisionAgents.Count > 0)
        {
            ConnectAgents(currAgent, CollisionAgents[0]);

            if (currAgent != CollisionAgents[0] && !PhysicalConn.ContainsKey((currAgent, CollisionAgents[0])))
            {
                ShowConnection(currAgent, CollisionAgents[0]);
                ShowConnection(CollisionAgents[0], currAgent);
            }
        }
    }

    public void ConnectNearestOneWay(GameObject currAgent)
    {

        Transform closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        //loop through items in the arena
        foreach (Transform obj in currAgent.transform.parent)
        {
            Vector3 diff = obj.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance && obj.tag == "Agent")
            {
                closest = obj;
                distance = curDistance;
            }
        }

        if (closest != null && !PhysicalConn.ContainsKey((currAgent, closest.gameObject)))
        {
            ConnectAgents(currAgent, closest.gameObject);
            ShowConnection(currAgent, closest.gameObject);
        }
    }

    public void ConnectAgents(GameObject agent1, GameObject agent2)
    {
        if (agent1 != agent2)
        {
            agent1.GetComponent<CollectFood>().Connections.Add(agent2);
            agent2.GetComponent<CollectFood>().Connections.Add(agent1);
        }
    }

    public void ShowConnection(GameObject agent1, GameObject agent2)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = agent1.transform.position;
        myLine.transform.SetParent(agent1.gameObject.transform);
        myLine.AddComponent<LineRenderer>();
        myLine.AddComponent<Rigidbody>();
        myLine.GetComponent<Rigidbody>().isKinematic = true;

        myLine.GetComponent<LineRenderer>().material = new Material(Shader.Find("Sprites/Default")); ;
        myLine.GetComponent<LineRenderer>().material.SetColor("_Color", Color.white);
        myLine.GetComponent<LineRenderer>().material.EnableKeyword("_EMISSION");
        myLine.GetComponent<LineRenderer>().material.SetColor("_EmissionColor", Color.white);
        //myLine.GetComponent<LineRenderer>().material.SetColor("_TintColor", Color.white);

        // line.startColor = Color.yellow;
        //line.endColor = Color.white;
        myLine.GetComponent<LineRenderer>().startWidth = 0.37f;
        myLine.GetComponent<LineRenderer>().endWidth = 0.09f;

        //TEMPORARY FIX TO CONN GLITCH
        myLine.GetComponent<LineRenderer>().SetPosition(0, agent1.transform.position);
        myLine.GetComponent<LineRenderer>().SetPosition(1, agent1.gameObject.transform.position);

        PhysicalConn.Add((agent1, agent2), myLine);
        // connectionHistory.Add((agent1.name, agent2.name,agent1.transform.parent.name));
        WriteData(agent1.name + "," + agent2.name + "," + agent1.transform.parent.name, writepathConns);
    }

    public void WriteData(string data, string destination)
    {
        StreamWriter writer = new StreamWriter(experimentPath + "\\" + destination, true);
        writer.WriteLine(data);
        writer.Close();
    }

    public void WriteGenome(GameObject agent, string destination)
    {
        string ArenaName = agent.transform.parent.name;
        string sharePref = agent.GetComponent<Genome>().SharingPref.ToString();
        int[] Genes = agent.GetComponent<Genome>().GeneSequence.ToArray();
        string Sequence = string.Join(".", Genes);
        int totConns = agent.GetComponent<CollectFood>().Connections.Count;
        string connNum = totConns.ToString();
        string genNum = agent.GetComponent<Lifecycle>().generationNum.ToString();
        string agentName = agent.name;
        string genitorName = null;
        if (agent.GetComponent<Lifecycle>().lastAncestor != null)
        {
            genitorName = agent.GetComponent<Lifecycle>().lastAncestor;
        }
        else
        {
            genitorName = "0";
        }
        string foodCollected = agent.GetComponent<CollectFood>().foodsCollected.ToString();
        string foodGiven = agent.GetComponent<CollectFood>().foodsGiven.ToString();
        string foodReceived = agent.GetComponent<CollectFood>().foodsReceived.ToString();
        string birthTicks = agent.GetComponent<Lifecycle>().birthtime.ToString();
        string deathTicks = agent.GetComponent<Lifecycle>().deathtime.ToString();
        string genomeData = ArenaName + "," + sharePref + "," + Sequence + "," + connNum + "," + genNum + "," + agentName + "," + genitorName + "," + foodCollected + "," + foodGiven + "," + foodReceived + "," + birthTicks + "," + deathTicks;
        WriteData(genomeData, destination);
    }

    public void WriteConnections(GameObject agent, string destination)
    {
        List<string> ConnNames = new List<string>();
        string Conns = null;
        foreach (GameObject connection in agent.GetComponent<CollectFood>().Connections)
        {
            ConnNames.Add(connection.name);
        }
        Conns = string.Join("", ConnNames);
        string output = agent.name + "," + Conns;
        WriteData(output, writepathConns);
    }

    public void WriteConnections(string agent1, string agent2, string arenaName, string destination)
    {
        string output = agent1 + "," + agent2 + "," + arenaName;
        WriteData(output, destination);
    }

    public void WritePopulation(string destination)
    {
        List<Transform> arenaTransforms = new List<Transform>();

        foreach (GameObject arena in arenaList)
        {
            arenaTransforms.Add(arena.transform);
        }
        foreach (Transform arenaTr in arenaTransforms)
        {
            List<GameObject> agentsHere = new List<GameObject>();
            foreach (Transform element in arenaTr)
            {
                if (element.name.Contains("Agent"))
                {
                    agentsHere.Add(element.gameObject);
                }
            }
            string ArenaName = arenaTr.gameObject.name;
            string agentsCount = agentsHere.Count.ToString();
            string summary = ArenaName + "," + agentsCount;
            WriteData(summary, destination);
        }
    }

    public int GetOverallTicks()
    {
        return OverallTicks;
    }

    public float GetArenaLimitSquared()
    {
        return arenaLimitSquared;
    }

    public void controls()
    {
        //alter connections view mode
        if (Input.GetKeyDown("v"))
        {
            connectionsViewMode++;
            connectionsViewMode %= 3;
            Debug.Log(connectionsViewMode);
        }

        //alter agent colour view mode
        if (Input.GetKeyDown("c"))
        {
            agentColourMode++;
            agentColourMode %= 3;
            Debug.Log(agentColourMode);
            //re-render colour of each agent every time this mode is changed
            foreach (GameObject arena in arenaList)
            {
                foreach (Transform element in arena.transform)
                {
                    if (element.name.Contains("Agent"))
                    {
                        if (agentColourMode % 3 == 0)
                        {
                            float pref = element.GetComponent<Genome>().SharingPref;
                            element.GetComponent<Genome>().setSharingPrefColour(pref);
                        }
                        else if (agentColourMode % 3 == 1)
                        {
                            element.GetComponent<CollectFood>().changeColour();
                        }
                        else
                        {
                            string type = element.GetComponent<Genome>().SharerType;
                            element.GetComponent<Genome>().setSharerTypeColour(type);
                        }
                    }
                }
            }

        }

        // alter time
        if (Input.GetKeyDown(KeyCode.F))
        {
            Time.timeScale *= 1.2f;
            Debug.Log("Timescale: " + Time.timeScale);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale *= 0.8f;
            Debug.Log("Timescale: " + Time.timeScale);
        }
        if (Input.GetKeyDown("n"))
        {
            Time.timeScale = 1.0f;
            Debug.Log("Timescale: " + Time.timeScale);
        }
    }

}