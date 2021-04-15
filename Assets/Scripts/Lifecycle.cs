using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class Lifecycle : MonoBehaviour
{
    //simulation variables
    private RunSim environment;

    //track 'energy' and 'age'
    public float points;
    public int lifetime = 9000; //three minutes
    public int birthtime = 0;
    public int deathtime = 0;
    public string lastAncestor = null;
    public int generationNum;
    private int maxClones= 99;
    private int clonesSpawned = 0;

    public System.Random ran = new System.Random();

    private void Start()
    {
        birthtime = environment.GetComponent<RunSim>().GetOverallTicks();
    }

    void Update()
    {
        setSize();
        //setTransparency();
    }

    void FixedUpdate()
    {
        //if agent has lived through their lifetime, or has zero points, die
        if (GetComponent<AgentMotion>().Ticks > lifetime || points <= -1)
        {
            deathtime = environment.GetComponent<RunSim>().GetOverallTicks();
            environment.WriteGenome(gameObject, environment.writepath);
            var sortTarget = gameObject.GetComponent<AgentMotion>().targets.GroupBy(i => i).OrderBy(g => g.Count()).Select(g => g.Key).ToList();
            var mostCommonTarget = sortTarget.Last();
            transform.parent.GetComponent<ArenaInfo>().Agents--;
            //Destroy(GetComponentInChildren<GameObject>());
            Destroy(gameObject);
        }

        //if agent has more than minClonePoints and has not reached cloning cap, clone!
        if (points >= environment.minClonePoints && clonesSpawned < maxClones)
        {
            GameObject newbaby = environment.CloneAgent(gameObject);
            points -= environment.cloningPointLoss; //a point dissipated in the process
        }
    }

    void setSize() //sets size based on current amount of points
    {
        transform.localScale = Vector3.one * (0.5f + 0.0005f*points + 0.0005f * GetComponent<CollectFood>().foodStored);
    }  

    /*
    void setTransparency() //set transparency based on age (THIS IS NOT WORKING!)
    {
        var rend = gameObject.GetComponent<Renderer>();
        Color colorBegin =  rend.material.color;
        float a = (lifetime - GetComponent<AgentMotion>().Ticks)/lifetime;
        Color colorEnd = new Color(colorBegin.r, colorBegin.g, colorBegin.b, a);
        rend.material.color = colorEnd;
    }
    */

    public void SetEnvironment(RunSim env)
    {
        this.environment = env;
    }

}