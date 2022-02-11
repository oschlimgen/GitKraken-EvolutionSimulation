using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    public int ID;
    public float angle;
    
    public GameObject parent;
    public GameObject deadAgent;
    public float energy = 75f; //Initial Energy

    private float movementCost = 0f; //Energy Expended per Second (Minimum Value)
    public float speed;
    public float radius;
    public float size;
    private float timer = 0f;
    public float caution = 0.5f;

    private NavMeshAgent navigation;
    private GameObject[] food;

    private float x;
    private float z;
    private float elapsed;
    private Vector2[] visited;
    private NavMeshHit edge;
    private bool Return = false;
    private float border;
    private float sizeDifference = 1.25f;
    private bool pause = false; //Tracks if editting
    private GameObject[] agents;
    private float[,] agentStats;
    private float wait;

    public int foodEaten;


    void moveSmart(Vector3 pos, Vector2[] visited, float sense, GameObject[] creatures, bool[] runAwayFrom, out float finalAngle, out Vector2[] newVisited)
    {
        int close = 0;
        int[] closeVisited = new int[visited.Length];
        newVisited = new Vector2[visited.Length + 1];
        for(int index = 0; index < visited.Length; index++) {
            if (Mathf.Pow(pos.x - visited[index].x, 2f) + Mathf.Pow(pos.z - visited[index].y, 2f) < sense * sense)
            {
                closeVisited[close] = index;
                close++;
            }
            newVisited[index] = visited[index];
        }
        int bigCreatureCount = 0;
        for(int index = 0; index < runAwayFrom.Length; index++) {
            if(runAwayFrom[index]) {
                bigCreatureCount++;
            }
        }
        float angle0;
        float angle1;
        float angle2;
        float bigCreatureAngle = 0f;
        if(close == 0) {
            angle0 = Mathf.Atan2(pos.z, pos.x);
            angle1 = Random.value * Mathf.PI * 2f;
            angle2 = Random.value * Mathf.PI * 2f;
        } else {
            float[] angles = new float[close];
            for(int index = 0; index < close; index++) {
                angles[index] = Mathf.Atan2((pos.z - visited[closeVisited[index]].y), (pos.x - visited[closeVisited[index]].x));
            }
            angle0 = Mathf.Atan2(pos.z, pos.x);
            System.Array.Sort(angles);
            angle1 = angles[0];
            angle2 = angles[close - 1];
            float biggest = angles[0] - angles[close - 1] + 2 * Mathf.PI;
            for(int index = 1; index < close; index++) {
                if(angles[index] - angles[index - 1] > biggest) {
                    angle1 = angles[index];
                    angle2 = angles[index - 1];
                    biggest = angle1 - angle2;
                }
            }
        }
        for(int index = 0; index < runAwayFrom.Length; index++) {
            if(runAwayFrom[index]) {
                bigCreatureAngle += Mathf.Atan2(pos.z - creatures[index].transform.position.z, pos.x - creatures[index].transform.position.x);
            }
        }
        finalAngle = (2 * angle0 + angle1 + angle2) / 4f;
        if(bigCreatureCount != 0) {
            finalAngle = (finalAngle + bigCreatureAngle / bigCreatureCount) / 2f;
        }
        newVisited[visited.Length].x = pos.x;
        newVisited[visited.Length].y = pos.z;
    }


    void Start()
    {
        border = (parent.GetComponent<TerrainGeneration>().mapSize) / 2f - parent.GetComponent<TerrainGeneration>().agentBorder;
        TerrainGeneration.agentStats[ID,0] = 0f;
        speed = TerrainGeneration.agentStats[ID,2];
        radius = TerrainGeneration.agentStats[ID,3];
        size = TerrainGeneration.agentStats[ID,4];
        movementCost = 0f;
        movementCost += speed * speed;
        movementCost += radius;
        movementCost += size;
        TerrainGeneration.agentStats[ID,0] = 0f;
        TerrainGeneration.agentStats[ID,1] = energy / movementCost; //Time agent has remaining to find food
        navigation = this.GetComponent<NavMeshAgent>();
        navigation.speed = speed * 5f;
        navigation.acceleration = speed * 2f;
        gameObject.transform.localScale = new Vector3(size / 4f, size / 4f, size / 4f);
        visited = new Vector2[1];
        visited[0].x = gameObject.transform.position.x - Mathf.Cos(angle) * radius / 2f;
        visited[0].y = gameObject.transform.position.z - Mathf.Sin(angle) * radius / 2f;
        agents = TerrainGeneration.agents;
        agentStats = TerrainGeneration.agentStats;
        wait = energy;
    }

    
    void Update()
    {
        if(!TerrainGeneration.editing && TerrainGeneration.startRound[ID]) {
            if(pause) {
                pause = false;
                agents = TerrainGeneration.agents;
                agentStats = TerrainGeneration.agentStats;
            }
            if(timer >= 0.1f) {
                foodEaten = (int)agentStats[ID,0];
                x = gameObject.transform.position.x;
                z = gameObject.transform.position.z;
                if((x > border || x < -border || z > border || z < -border) && agentStats[ID,0] > 0f) {
                    Debug.Log("I've eaten " + agentStats[ID,0].ToString() + " food and have " + agentStats[ID,1].ToString() + " seconds left! My ID is " + ID.ToString());
                    TerrainGeneration.agentStats[ID,1] = 0f;
                } else {
                    NavMesh.FindClosestEdge(gameObject.transform.position, out edge, NavMesh.AllAreas);
                    if(agentStats[ID,0] == 1f && TerrainGeneration.agentStats[ID,1] < caution * (edge.distance + 5f) / speed) {
                        Return = true;
                    }
                    if(agentStats[ID,0] == 0f || (agentStats[ID,0] == 1f && Return == false)) {
                        bool[] creatureFood = new bool[agents.Length];
                        bool[] danger = new bool[agents.Length];
                        for(int i = 0; i < agents.Length; i++) {
                            creatureFood[i] = (agentStats[i,4] * sizeDifference) < size;
                            danger[i] = (size * sizeDifference) < agentStats[i,4];
                        }
                        food = GameObject.FindGameObjectsWithTag("Food");
                        float distance = radius * radius;
                        Vector3 closest = gameObject.transform.position;
                        for(int i = 0; i < food.Length; i++) {
                            if(Mathf.Pow(x - food[i].transform.position.x, 2f) + Mathf.Pow(z - food[i].transform.position.z, 2f) <= distance){
                                closest = food[i].transform.position;
                                distance = Mathf.Pow(x - food[i].transform.position.x, 2f) + Mathf.Pow(z - food[i].transform.position.z, 2f);
                            }
                        }
                        for(int i = 0; i < creatureFood.Length; i++) {
                            if(creatureFood[i]) {
                                if(Mathf.Pow(x - agents[i].transform.position.x, 2f) + Mathf.Pow(z - agents[i].transform.position.z, 2f) <= distance){
                                    closest = agents[i].transform.position;
                                    distance = Mathf.Pow(x - agents[i].transform.position.x, 2f) + Mathf.Pow(z - agents[i].transform.position.z, 2f);
                                }
                            }
                        }
                        if(closest.x == x && closest.z == z) {
                            if(TerrainGeneration.agentStats[ID,1] <= wait) {
                                moveSmart(gameObject.transform.position, visited, radius, agents, danger, out angle, out visited);
                                wait = energy;
                                closest = new Vector3(gameObject.transform.position.x + Mathf.Cos(angle), 0f, gameObject.transform.position.z + Mathf.Sin(angle));
                                if(closest.x > border || closest.x < -border || closest.z > border || closest.z < -border) {
                                    closest = parent.transform.position;
                                    wait = TerrainGeneration.agentStats[ID,1] - 0.5f;
                                }
                                navigation.SetDestination(closest);
                            }
                        } else {
                            navigation.SetDestination(closest);
                        }
                    }
                    if(agentStats[ID,0] >= 2 || Return == true) {
                        navigation.SetDestination(edge.position);
                    }
                }
                timer = 0f;
            } else {
                elapsed = Time.deltaTime;
                timer += elapsed;
                TerrainGeneration.agentStats[ID,1] -= elapsed;
                if(TerrainGeneration.agentStats[ID,1] <= 0f) {
                    TerrainGeneration.agentStats[ID,1] = energy / movementCost;
                    navigation.SetDestination(gameObject.transform.position);
                    if(transform.localPosition.x >= border) {
                        angle = Mathf.PI;
                    } else if(transform.localPosition.x <= -border) {
                        angle = 0f;
                    } else if(transform.localPosition.z >= border) {
                        angle = (3f / 2f) * Mathf.PI;
                    } else if(transform.localPosition.z <= -border) {
                        angle = (1f / 2f) * Mathf.PI;
                    } else {
                        Debug.Log("I didn't make it home. I ate " + agentStats[ID,0].ToString() + " food. My ID is " + ID.ToString());
                    }
                    visited = new Vector2[1];
                    visited[0].x = gameObject.transform.position.x - Mathf.Cos(angle) * radius / 2f;
                    visited[0].y = gameObject.transform.position.z - Mathf.Sin(angle) * radius / 2f;
                    timer = 0f;
                    Return = false;
                    TerrainGeneration.startRound[ID] = false;
                    pause = true;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if(TerrainGeneration.editing == false) {
            if(TerrainGeneration.startRound[ID]) {
                if(other.gameObject.CompareTag("Food")) {
                    Destroy(other.gameObject);
                    TerrainGeneration.agentStats[ID,0] += 1f;
                }
            }
        }
    }

    void OnCollisionEnter(Collision other) {
        if(TerrainGeneration.editing == false) {
            if(TerrainGeneration.startRound[ID]) {
                if(other.gameObject.CompareTag("Creature")) {
                    if(other.gameObject.GetComponent<AgentController>().size * sizeDifference < size) {
                        int otherID = other.gameObject.GetComponent<AgentController>().ID;
                        if(TerrainGeneration.startRound[otherID]) {
                            killCreature(other.gameObject, otherID);
                            TerrainGeneration.agentStats[ID,0] += 2f;
                        }
                    }
                }
            }
        }
    }

    void killCreature(GameObject creature, int number) {
        TerrainGeneration.agentStats[number,0] = 0f;
        TerrainGeneration.agentStats[number,1] = 0f;
        TerrainGeneration.agentStats[number,4] = 1000f;
        TerrainGeneration.startRound[number] = false;
        Destroy(creature);
        TerrainGeneration.agents[number] = Instantiate(deadAgent);
    }
}