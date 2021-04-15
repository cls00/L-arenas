using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Genome : MonoBehaviour
{
    //simulation variables
    private RunSim environment;
    
    //motion genes
    public float minSpeed = 10f;
    public float maxSpeed = 40f;
    public float rotationRange = 120f;

    //behaviour genes
    public string SharerType; //random or preferential sharing
    public float SharingPref = 50; //probability of sharing the foodcollected
    private float g_red = 0;
    private float g_green = 0;
    private int ShararRandom = 0;
    private int ShararPreferential = 0;

    //gene sequence
    public List<int> GeneSequence = new List<int>();
    public int numRules; 
 

    void Start()
    {
        numRules = 15; //update this number when rules are added/removed
        
        //sequence genome once at birth
        Codesome();

        //set colour based on SharingPref by default
        if (environment.agentColourMode == 0)
        {
            setSharingPrefColour(SharingPref);
        }
    }

    void Codesome()
    {
        int index = 0;

        //reading the sequence
        while (index < GeneSequence.Count)
        {
            switch (GeneSequence[index])
            {
                //replace with a random gene and add one random gene to the end
                case 0:
                    GeneSequence[index] = Random.Range(1, numRules);
                    GeneSequence.Add(Random.Range(1, numRules));
                    break;
                //Remove item from sequence -- shortens the genome
                case 1:
                    GeneSequence.RemoveAt(index);
                    break;
                //create one random mutual connection
                case 2:
                    environment.RandomConnect(gameObject,false);
                    break;
                //create one nearby mutual connection
                case 3:
                    environment.ConnectNearest(gameObject);
                    break;
                //create one random one-way connection
                case 4:
                    environment.RandomConnect(gameObject, true);
                    break;
                //create one nearby one-way connection
                case 5:
                    environment.ConnectNearestOneWay(gameObject);
                    break;
                //add agent target to list of targets
                case 6:
                    GetComponent<AgentMotion>().targets.Add("Agent");
                    break;
                //add food target to list of targets
                case 7:
                    GetComponent<AgentMotion>().targets.Add("Pick Up");
                    break;
                //add random target to list of targets
                case 8:
                    GetComponent<AgentMotion>().targets.Add("random"); //either agent or food
                    break;
                //Stay focussed on a single target for longer time
                case 9:
                    GetComponent<AgentMotion>().addFocus();
                    break;
                //Stay focussed on a single target for shorter time
                case 10:
                    GetComponent<AgentMotion>().removeFocus();
                    break;
                //Increase sharing pref
                case 11:
                    SharingPref += (100-SharingPref)/2;
                    break;
                //Decrease sharing pref
                case 12:
                    SharingPref -= SharingPref/2;
                    break;
                //Increase tendency to share with random agent in my network
                case 13:
                    ShararRandom++;
                    break;
                //Increase tendency to share with a preferred agent in my network
                case 14:
                    ShararPreferential++;
                    break;
                //If any other number is found, ignore...
                default:
                break;
            }
            index++;
            gameObject.GetComponent<Lifecycle>().points -= environment.pointLoss; //points spent for sequencing each gene (so there is a cost for having a longer genome)
        }
        
       
        if (ShararRandom >= ShararPreferential)
        {
            SharerType = "Random";
        }
        else
        {
            SharerType = "Preferential";
        }
    }

    public void setSharingPrefColour(float pref)
    {
        var agentRenderer = gameObject.GetComponent<Renderer>();
        g_red = (50 - SharingPref)/50;
        g_green = (SharingPref - 50)/50;
        agentRenderer.material.SetColor("_Color", new Color(g_red, g_green, 1));
    }

    public void setSharerTypeColour(string type)
    {
        var agentRenderer = gameObject.GetComponent<Renderer>();
        if (type == "Preferential")
        {
            agentRenderer.material.SetColor("_Color", new Color(1, 0, 1, 1));
        }
        else 
        {
            agentRenderer.material.SetColor("_Color", new Color(1, 1, 1, 1));
        } 
    }

    public void SetEnvironment(RunSim env)
    {
        this.environment = env;
    }

    public int GetNumRules()
    {
        return numRules;
    }

    public List<int> GetGenes()
    {
        return GeneSequence;
    }

    public void SetGenes(List<int> genes)
    {
        GeneSequence = genes;
        //Debug.Log(gameObject.name + " setting genes");
    }



}
