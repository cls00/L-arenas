using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AgentMotion : MonoBehaviour
{
    //simulation variables
    private RunSim environment;

    //motion variables
    public List<string> targets = new List<string>();
    private GameObject target;
    private float timeVar = 0;
    private float step = Mathf.PI / 60;
    private float minSpeed;
    private float maxSpeed;
    private float rotationRange;
    //private float randomRotation = 10.0f;
    private Rigidbody rBody;
    private float motionPointLoss = 0.1f;
    private int focus = 0;
    public System.Random ran = new System.Random();

    private bool newDecisionTrigger = false;
    private float visionRadius = 15;
    private string targetType = "random";


    public int Ticks;

    // Start is called before the first frame update
    void Start()
    {
        //Get min and max speed, and rotation range, from genome
        minSpeed = gameObject.GetComponent<Genome>().minSpeed;
        maxSpeed = gameObject.GetComponent<Genome>().maxSpeed;
        rotationRange = gameObject.GetComponent<Genome>().rotationRange;

        //adding a random target to the target list
        targets.Add("random");

        //Get Rigid body component just once at Start, as this is an expensive operation
        rBody = GetComponent<Rigidbody>();


        Vector3 rndDir = new Vector3(Random.Range(-180, 180f), Random.Range(-180, 180f), Random.Range(-180, 180f)); //set a random angle to turn

        transform.Rotate(rndDir);
        rBody.AddForce(transform.forward * 5);
        Ticks = 0;
    }

    // Update is called once per physics frame
    void FixedUpdate()
    {
        Ticks += 1; //count up a Tick at each physics update
        Target(); //select or keep a target
        Move();
        if (Ticks % 10==0)
        {
            SelfContain(); // check boundaries of the arena
        }
    }

    void Target()
    {
        if (Ticks % (200 + focus) == 0 || newDecisionTrigger == true) // a decision is made every n+genepram ticks or when a trigger is activated
        {
            newDecisionTrigger = false;
            targetType = targets[ran.Next(0, targets.Count)];
            bool hasTarget = false;

            if (targetType == "random")
            {
                hasTarget = true;
            }
            else
            {
                //look for surrounding objects
                Collider[] surroundings = Physics.OverlapSphere(gameObject.transform.position, visionRadius);
                foreach (var item in surroundings)
                {
                    //if it is of the decired type and not itself set as target
                    if (item.tag == targetType && item.gameObject != gameObject)
                    {
                        target = item.gameObject;
                        hasTarget = true;
                        break;
                    }
                }

            }
            if (hasTarget == false)
            {
                targetType = "random";
            }
        }

    }

    void Move()
    {
        if (targetType == "random")
        {
            RandomMotion();

        }
        else if (target != null)
        {
            float speedTowards = Random.Range(minSpeed, maxSpeed);
            transform.Translate(Vector3.forward * Time.deltaTime * minSpeed);
            var myRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, myRotation, 10);
            //rBody.velocity = transform.forward * (speedTowards / 10); //move towards that target

            // Debug.Log(gameObject.name + " in " + gameObject.transform.parent.name + " chasing " + target.tag);
        }

        //dissipate energy slowly over time while moving
        float subtractPerSecond = motionPointLoss;
        GetComponent<Lifecycle>().points -= subtractPerSecond * Time.deltaTime;
    }

    void RandomMotion()
    {
        float speed = Random.Range(minSpeed, maxSpeed); //set a random speed for this time step
                                                        //UnityEngine.Vector3 randomDirection = new UnityEngine.Vector3(0, Mathf.Sin(timeVar) * (rotationRange / 2), 0); //set a random angle to turn
        Vector3 randomDirection = new Vector3(Random.Range(-1, 1f), Random.Range(-1, 1f), Random.Range(-1, 1f)); //set a random angle to turn

        timeVar += step;
        transform.Rotate(randomDirection * Time.fixedDeltaTime);
        //rBody.AddForce(transform.forward * speed / 50);
        transform.Translate(Vector3.forward * Time.deltaTime * minSpeed / 10);
    }

    private void OnTriggerEnter(Collider other)
    {
        newDecisionTrigger = true;
    }


    public void SetEnvironment(RunSim env)
    {
        this.environment = env;
    }

    public void addFocus()
    {
        focus += 20;
    }

    public void removeFocus()
    {
        focus -= 20;
    }

    public void SelfContain()
    {
        //check if agent is beyond boundaris of the arena
        if (gameObject.transform.localPosition.sqrMagnitude > environment.GetArenaLimitSquared())
        {
            //Debug.Log(gameObject.name + " at " + gameObject.transform.localPosition + " mgt" + gameObject.transform.localPosition.sqrMagnitude);

            //pull agent back into the arena
            transform.position = Vector3.MoveTowards(transform.position, transform.parent.position, 1f);
            gameObject.GetComponent<Rigidbody>().AddForce((transform.parent.position - transform.position) *0.5f);
            newDecisionTrigger = true;
            //transform.Translate(-transform.position * 0.1f);
        }
    }


}