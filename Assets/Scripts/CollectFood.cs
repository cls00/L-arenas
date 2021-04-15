using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CollectFood : MonoBehaviour
{

    //simulation variables
    private RunSim environment;

    //track grabbed items & connections
    public int releaseRate = 110;
    private List<GameObject> Grabbees = new List<GameObject>();
    private List<float> ReleaseTimes = new List<float>();
    public List<GameObject> Connections = new List<GameObject>();
    private int prefProb = 60;
    private int ranProb = 40;
    public System.Random ran = new System.Random();
    private List<GameObject> transferList = new List<GameObject>();
    private float tradeRadius = 20f;

    //resource counters
    public int foodsCollected = 0;
    public int foodsReceived = 0;
    public int foodsGiven = 0;
    public float foodStored = 0;
    private float p_red = 0;
    private float p_green = 0;
    //private float blue = 0;

    void FixedUpdate()
    {
        //if I'm grabbing food, digest it and get a point after releaseRate amount of time
        //iterate backwards as we may be deleting elements from the list

        if (foodStored > 0)
        {
            if (GetComponent<Lifecycle>().points > 1 && ran.Next(0, 100) <= GetComponent<Genome>().SharingPref)
            {
                Collider[] neighbours = Physics.OverlapSphere(gameObject.transform.position, tradeRadius);
                List<GameObject> available = new List<GameObject>();
                foreach (Collider neigh in neighbours)
                {
                   
                    if (Connections.Contains(neigh.gameObject))
                    {
                        available.Add(neigh.gameObject);
                    }
                }
                if (available.Count > 0)
                {
                    if (GetComponent<Genome>().SharerType == "Random")
                    {
                        //share randomly among available
                        SpecificSharing(available[Random.Range(0, available.Count)]);
                    }
                    else
                    {
                        //if (Connections.Take(2).Any(available.Contains))
                        var favourites = Connections.Take(2);
                        foreach (GameObject favourite in favourites)
                        {
                            if (available.Contains(favourite))
                            {
                                SpecificSharing(favourite);
                                break;
                            }
                        }
                        
                    }
                }
                
            }
            else
            {
                foodStored -= 1;
                gameObject.GetComponent<Lifecycle>().points+= 1;
                if (environment.agentColourMode == 1)
                {
                    changeColour();
                }    
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Pick Up")
        {
            foodsCollected += 1;
            foodStored += collision.gameObject.GetComponent<FoodVars>().energy;
            Destroy(collision.gameObject);
        }
        
    }

    void Grab(Collision collision)
    {
        Grabbees.Add(collision.gameObject); //add grabbee to my grabbees list
        ReleaseTimes.Add(GetComponent<AgentMotion>().Ticks + releaseRate); //add OverallTicks count for when to release
        foodsCollected += 1;
        GetComponent<Lifecycle>().points -= environment.pointLoss; //some energy is lost to grab food
    }

    void RandomSharing(int index)
    {
        int ranIndex = ran.Next(Connections.Count);
        if (Connections.Count > 0 && Connections[ranIndex] != null) //if there are connections with which to share
        {
            Connections[ranIndex].GetComponent<Lifecycle>().points++; //give a point to a random connection
            Connections[ranIndex].GetComponent<CollectFood>().foodsReceived += 1;
            if (environment.agentColourMode == 1)
            {
                Connections[ranIndex].GetComponent<CollectFood>().changeColour();
            }
            if (Connections[ranIndex] != gameObject)
            {
                foodsGiven += 1;
                transferList.Add(Connections[ranIndex]);
                if (environment.agentColourMode == 1)
                {
                    changeColour();
                }
            }
        }
        Grabbees[index].GetComponent<FoodVars>().energy--;
        if (Grabbees[index].GetComponent<FoodVars>().energy <= 0)
        {
            Destroy(Grabbees[index]); //destroy grabbee
            Grabbees.RemoveAt(index); //remove from Grabbee list
            ReleaseTimes.RemoveAt(index);
        }
        GetComponent<Lifecycle>().points -= environment.pointLoss;
    }

    void SpecificSharing(GameObject receiver)
    {
        foodStored -= 1;
        foodsGiven += 1;
        if (environment.agentColourMode == 1)
        {
            changeColour();
        }
        transferList.Add(receiver);
        receiver.GetComponent<CollectFood>().foodStored += 1;
        receiver.GetComponent<CollectFood>().foodsReceived += 1;
        if (environment.agentColourMode == 1)
        {
            receiver.GetComponent<CollectFood>().changeColour();
        }

        if(gameObject == receiver)
        {
            Debug.Log(gameObject + " Self transfer!!");
        }
    }



    void PrefSharing(int index)
    {
        int x = ran.Next(0, 101); //generating probabilities

        for (int i = 0; i < Connections.Count; i++)
        {
            if (Connections.Count > 2) //this function has a preference for the first 2 agents
            {
                if (x <= prefProb)
                {
                    int prefIndex = ran.Next(0, 2); //picks random agent between 2 preferred agents
                    if (Connections[prefIndex] != null)
                    {
                        Connections[prefIndex].GetComponent<Lifecycle>().points++;
                        Connections[prefIndex].GetComponent<CollectFood>().foodsReceived += 1;
                        if (environment.agentColourMode == 1)
                        {
                            Connections[prefIndex].GetComponent<CollectFood>().changeColour();
                        }
                        if (Connections[prefIndex] != gameObject)
                        {
                            transferList.Add(Connections[prefIndex]);
                            foodsGiven += 1;
                            if (environment.agentColourMode == 1)
                            {
                                changeColour();
                            }
                        }
                    }
                }
                if (x <= ranProb && Connections.Count > 2) //otherwise gives to another random connection
                {
                    int ind = ran.Next(2, Connections.Count);
                    if (Connections[ind] != null)
                    {
                        Connections[ind].GetComponent<Lifecycle>().points++;
                        Connections[ind].GetComponent<CollectFood>().foodsReceived += 1;
                        if (environment.agentColourMode == 1)
                        {
                            Connections[ind].GetComponent<CollectFood>().changeColour();
                        }
                        if (Connections[ind] != gameObject)
                        {
                            transferList.Add(Connections[ind]);
                            foodsGiven += 1;
                            if (environment.agentColourMode == 1)
                            {
                                changeColour();
                            }
                        }
                    }
                }
            }
        }

        Grabbees[index].GetComponent<FoodVars>().energy--;
        if (Grabbees[index].GetComponent<FoodVars>().energy <= 0)
        {
            Destroy(Grabbees[index]); //destroy grabbee
            Grabbees.RemoveAt(index); //remove from Grabbee list
            ReleaseTimes.RemoveAt(index); //also remove the corresponding release time
        }
        GetComponent<Lifecycle>().points -= environment.pointLoss;
    }

    public void changeColour() //sets colour based on how much food agent has given or received
    {
        var agentRenderer = gameObject.GetComponent<Renderer>();
        p_red = (foodsReceived * 0.03f);
        p_green = (foodsGiven * 0.03f);
        //p_blue = 1 - (foodsReceived * 0.1f) - (foodsGiven * 0.1f);
        agentRenderer.material.SetColor("_Color", new Color(p_red, p_green, 0));
    }

    public void SetEnvironment(RunSim env)
    {
        this.environment = env;
    }

    public List<GameObject> getTransferList()
    {
        return transferList;
    }

}