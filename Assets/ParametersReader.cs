using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ParametersReader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadParams()
    {
        List<string> parameters = new List<string>();
        var filePath = Application.persistentDataPath + "/Params0.txt";
        //System.IO.StreamReader file = new System.IO.StreamReader(@filePath);
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            
            foreach (string line in lines)
            {
                var param = line.Split('-');
                parameters.Add(param[0]);
            }

            try
            {
                gameObject.GetComponent<RunSim>().arenaSize = float.Parse(parameters[0]);
                gameObject.GetComponent<RunSim>().patchNum = int.Parse(parameters[1]);
                gameObject.GetComponent<RunSim>().foodAmount = int.Parse(parameters[2]);
                gameObject.GetComponent<RunSim>().foodProb = float.Parse(parameters[3]);
                gameObject.GetComponent<RunSim>().energyPerFoodBlock = float.Parse(parameters[4]);
                gameObject.GetComponent<RunSim>().minClonePoints = int.Parse(parameters[5]);
                gameObject.GetComponent<RunSim>().cloningPointLoss = float.Parse(parameters[6]);
                gameObject.GetComponent<RunSim>().startingPoints = int.Parse(parameters[7]);
                gameObject.GetComponent<RunSim>().pointLoss = float.Parse(parameters[8]);
                gameObject.GetComponent<RunSim>().nearestRadius = int.Parse(parameters[9]);
                gameObject.GetComponent<RunSim>().mutationRate = float.Parse(parameters[10]);
                gameObject.GetComponent<RunSim>().foodRespawn = parameters[11];

                Debug.Log("Parameters from " + filePath + " loaded.");
            }
            catch
            {
                Debug.Log("Parsing Error");
            }
        }
        catch
        {
            Debug.Log("Parameter file error! Using default parameters");
        }

        



    }
}
