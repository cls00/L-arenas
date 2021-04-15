using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{

    //each agent with different random gene sequence
    public List<GameObject> BasicPop(GameObject arena, int popSize)
    {
        List<GameObject> pop = new List<GameObject>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 arenaProximity = Random.insideUnitSphere * gameObject.GetComponent<RunSim>().arenaSize + arena.transform.position;
            GameObject agent = gameObject.GetComponent<RunSim>().CreateAgent(arenaProximity);
            agent.GetComponent<Genome>().GeneSequence = new List<int>() {0, 0, 0, 0, 0, 0};
            agent.transform.SetParent(arena.transform, true);
            agent.GetComponent<Lifecycle>().generationNum = 0;
            agent.transform.parent.GetComponent<ArenaInfo>().Agents++;
            pop.Add(agent);
        }
        return pop;
    }

    //highly connected
    public List<GameObject> HighConnPop(GameObject arena, int popSize)
    {
        List<GameObject> pop = new List<GameObject>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 arenaProximity = Random.insideUnitSphere * gameObject.GetComponent<RunSim>().arenaSize + arena.transform.position;
            GameObject agent = gameObject.GetComponent<RunSim>().CreateAgent(arenaProximity);
            agent.GetComponent<Genome>().GeneSequence = new List<int>() { 0, 7, 2, 2, 2, 2, 2, 2 };
            agent.transform.SetParent(arena.transform, true);
            agent.GetComponent<Lifecycle>().generationNum = 0;
            agent.transform.parent.GetComponent<ArenaInfo>().Agents++;
            pop.Add(agent);
        }
        return pop;
    }

    //local connections, high sharingPref
    public List<GameObject> LocalSharingPop(GameObject arena, int popSize)
    {
        List<GameObject> pop = new List<GameObject>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 arenaProximity = Random.insideUnitSphere * gameObject.GetComponent<RunSim>().arenaSize + arena.transform.position;
            GameObject agent = gameObject.GetComponent<RunSim>().CreateAgent(arenaProximity);
            agent.GetComponent<Genome>().GeneSequence = new List<int>() { 0, 7, 3, 3, 3, 11};
            agent.transform.SetParent(arena.transform, true);
            agent.GetComponent<Lifecycle>().generationNum = 0;
            agent.transform.parent.GetComponent<ArenaInfo>().Agents++;
            pop.Add(agent);
        }
        return pop;
    }

    //Grouping up 
    public List<GameObject> GroupingUpPop(GameObject arena, int popSize)
    {
        List<GameObject> pop = new List<GameObject>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 arenaProximity = Random.insideUnitSphere * gameObject.GetComponent<RunSim>().arenaSize + arena.transform.position;
            GameObject agent = gameObject.GetComponent<RunSim>().CreateAgent(arenaProximity);
            agent.GetComponent<Genome>().GeneSequence = new List<int>() { 0, 7, 6, 6, 6, 3, 3, 3};
            agent.transform.SetParent(arena.transform, true);
            agent.GetComponent<Lifecycle>().generationNum = 0;
            agent.transform.parent.GetComponent<ArenaInfo>().Agents++;
            pop.Add(agent);
        }

        return pop;
    }

    //High conn, high sharing
    public List<GameObject> HighConnHighShare(GameObject arena, int popSize)
    {
        List<GameObject> pop = new List<GameObject>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 arenaProximity = Random.insideUnitSphere * gameObject.GetComponent<RunSim>().arenaSize + arena.transform.position;
            GameObject agent = gameObject.GetComponent<RunSim>().CreateAgent(arenaProximity);
            agent.GetComponent<Genome>().GeneSequence = new List<int>() { 0, 7, 2, 2, 2, 2, 2, 2, 11, 11};
            agent.transform.SetParent(arena.transform, true);
            agent.GetComponent<Lifecycle>().generationNum = 0;
            agent.transform.parent.GetComponent<ArenaInfo>().Agents++;
            pop.Add(agent);
        }
        return pop;
    }

    //High conn, low sharing
    public List<GameObject> HighConnLowShare(GameObject arena, int popSize)
    {
        List<GameObject> pop = new List<GameObject>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 arenaProximity = Random.insideUnitSphere * gameObject.GetComponent<RunSim>().arenaSize + arena.transform.position;
            GameObject agent = gameObject.GetComponent<RunSim>().CreateAgent(arenaProximity);
            agent.GetComponent<Genome>().GeneSequence = new List<int>() { 0, 7, 2, 2, 2, 2, 2, 2, 12, 12};
            agent.transform.SetParent(arena.transform, true);
            agent.GetComponent<Lifecycle>().generationNum = 0;
            agent.transform.parent.GetComponent<ArenaInfo>().Agents++;
            pop.Add(agent);
        }
        return pop;
    }

    //Low conn, high sharing
    public List<GameObject> LowConnHighShare(GameObject arena, int popSize)
    {
        List<GameObject> pop = new List<GameObject>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 arenaProximity = Random.insideUnitSphere * gameObject.GetComponent<RunSim>().arenaSize + arena.transform.position;
            GameObject agent = gameObject.GetComponent<RunSim>().CreateAgent(arenaProximity);
            agent.GetComponent<Genome>().GeneSequence = new List<int>() { 0, 7, 3, 11, 11};
            agent.transform.SetParent(arena.transform, true);
            agent.GetComponent<Lifecycle>().generationNum = 0;
            agent.transform.parent.GetComponent<ArenaInfo>().Agents++;
            pop.Add(agent);
        }
        return pop;
    }

    //Low conn, low sharing
    public List<GameObject> LowConnLowShare(GameObject arena, int popSize)
    {
        List<GameObject> pop = new List<GameObject>();
        for (int i = 0; i < popSize; i++)
        {
            Vector3 arenaProximity = Random.insideUnitSphere * gameObject.GetComponent<RunSim>().arenaSize + arena.transform.position;
            GameObject agent = gameObject.GetComponent<RunSim>().CreateAgent(arenaProximity);
            agent.GetComponent<Genome>().GeneSequence = new List<int>() { 0, 7, 3, 12, 12};
            agent.transform.SetParent(arena.transform, true);
            agent.GetComponent<Lifecycle>().generationNum = 0;
            agent.transform.parent.GetComponent<ArenaInfo>().Agents++;
            pop.Add(agent);
        }
        return pop;
    }


    public void SpawnFood(GameObject arena, string arenaType)
    {
        switch (arenaType)
        {
            case "basic":
                if(Random.value < GetComponent<RunSim>().foodProb)
                {
                    Vector3 arenaProximity = Random.insideUnitSphere * GetComponent<RunSim>().arenaSize + arena.transform.position;
                    GetComponent<RunSim>().SpawnSingleFood(arenaProximity, GetComponent<RunSim>().energyPerFoodBlock, arena);
                    //Debug.Log("spawning at" + GetComponent<RunSim>().GetOverallTicks());
                }               
                break;

            case "oscillating":
                if (Random.value  < GetComponent<RunSim>().foodProb * Mathf.Abs(Mathf.Sin(GetComponent<RunSim>().GetOverallTicks()/100)))
                {
                    Vector3 arenaProximity = Random.insideUnitSphere * GetComponent<RunSim>().arenaSize + arena.transform.position;
                    GetComponent<RunSim>().SpawnSingleFood(arenaProximity, GetComponent<RunSim>().energyPerFoodBlock, arena);
                    //Debug.Log("spawning at" + GetComponent<RunSim>().GetOverallTicks());
                }
                break;

            case "expandingWave":
                if (Random.value < GetComponent<RunSim>().foodProb * 0.5f *(1 + Mathf.Sin((Mathf.Pow(GetComponent<RunSim>().GetOverallTicks(),0.5f)/100)))){
                    Vector3 arenaProximity = Random.insideUnitSphere * GetComponent<RunSim>().arenaSize + arena.transform.position;
                    GetComponent<RunSim>().SpawnSingleFood(arenaProximity, GetComponent<RunSim>().energyPerFoodBlock, arena);
                    //Debug.Log("spawning at" + GetComponent<RunSim>().GetOverallTicks());
                }
                break;

                    default:
                break;
        }
    }

}

