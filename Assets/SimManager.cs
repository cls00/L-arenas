using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimManager : MonoBehaviour
{
    public int repetitions = 10;
    public int ticksPerSim = 300000;
    public GameObject simPrefab;
    private GameObject sim;

    // Start is called before the first frame update
    void Start()
    {
        sim = Instantiate(simPrefab);
    }

    
    void FixedUpdate()
    {
        if(sim != null)
        {
            if (sim.GetComponent<RunSim>().GetOverallTicks() > ticksPerSim)
            {
                Destroy(sim);
            }
        } else
        {
            sim = Instantiate(simPrefab);
        }
        
    }
}
