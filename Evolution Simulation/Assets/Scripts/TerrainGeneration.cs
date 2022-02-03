using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TerrainGeneration : MonoBehaviour
{
    public GameObject agent;
    public GameObject food;

    [SerializeField, Range(1, 100)]
    int foodCount = 80;

    [SerializeField, Range(1, 100)]
    int creatureCount = 50;

    public float mapSize = 30f;
    public float agentBorder = 1f;

    //Food Count, Complete, Attributes
    public static float[,] agentStats;
    public static GameObject[] agents;
    public static bool[] startRound;
    public static bool editing = true;

    //Speed, Sensing Radius, Size
    public static float[] attributes = new float[3] {2f, 4f, 4f};
    

    GameObject[] positionAgents(GameObject[] positions, float[,] traits) {
        for(int index = 0; index < positions.Length; index++){
            if((int)traits[index, 0] == 100f) { //If the agent was just spawned
                int side = Random.Range(0,4);
                Vector3 pos = new Vector3(0f, 1f, 0f);
                if(side == 0){
                    pos.x = (mapSize)/2f - agentBorder;
                    pos.z = (Random.value - 0.5f) * (mapSize - agentBorder * 2f);
                    positions[index].GetComponent<AgentController>().angle = Mathf.PI;
                }
                if(side == 1){
                    pos.x = -(mapSize)/2f + agentBorder;
                    pos.z = (Random.value - 0.5f) * (mapSize - agentBorder * 2f);
                    positions[index].GetComponent<AgentController>().angle = 0f;
                }
                if(side == 2){
                    pos.z = (mapSize)/2f - agentBorder;
                    pos.x = (Random.value - 0.5f) * (mapSize - agentBorder * 2f);
                    positions[index].GetComponent<AgentController>().angle = (3f / 2f) * Mathf.PI;
                }
                if(side == 3){
                    pos.z = -(mapSize)/2f + agentBorder;
                    pos.x = (Random.value - 0.5f) * (mapSize - agentBorder * 2f);
                    positions[index].GetComponent<AgentController>().angle = (1f / 2f) * Mathf.PI;
                }
                positions[index].transform.localPosition = pos;
            }
        }
        return positions;
    }

    void newGeneration(float[,] currentGen, GameObject[] positions, int subLength, out float[,] newGen, out GameObject[] newPositions) {
        int newCreatureCount = 0;
        for(int index = 0; index < positions.Length; index++) {
            if(!(positions[index].transform.localPosition.x < (mapSize)/2f - agentBorder && positions[index].transform.localPosition.x > -(mapSize)/2f + agentBorder && positions[index].transform.localPosition.z < (mapSize)/2f - agentBorder && positions[index].transform.localPosition.z > -(mapSize)/2f + agentBorder)) {
                if(currentGen[index,0] == 1) {
                    newCreatureCount += 1;
                }
                if(currentGen[index,0] >= 2) {
                    newCreatureCount += 2;
                }
            }
        }
        newGen = new float[newCreatureCount, subLength];
        newPositions = new GameObject[newCreatureCount];
        int alive = 0;
        for(int index = 0; index < positions.Length; index++) {
            if(positions[index].transform.localPosition.x < (mapSize)/2f - agentBorder && positions[index].transform.localPosition.x > -(mapSize)/2f + agentBorder && positions[index].transform.localPosition.z < (mapSize)/2f - agentBorder && positions[index].transform.localPosition.z > -(mapSize)/2f + agentBorder) {
                Destroy(positions[index]);
            } else {
                if(currentGen[index,0] == 0) {
                    Destroy(positions[index]);
                } else if(currentGen[index,0] == 1) {
                    currentGen[index,0] = 0f;
                    newGen[alive,0] = 0f;
                    for(int jndex = 1; jndex < subLength; jndex++) {
                        newGen[alive,jndex] = currentGen[index,jndex];
                    }
                    newPositions[alive] = positions[index];
                    newPositions[alive].GetComponent<AgentController>().ID = alive;
                    alive++;
                } else if(currentGen[index,0] >= 2) {
                    //Itself
                    currentGen[index,0] = 0f;
                    newGen[alive,0] = 0f;
                    for(int jndex = 1; jndex < subLength; jndex++) {
                        newGen[alive,jndex] = currentGen[index,jndex];
                    }
                    newPositions[alive] = positions[index];
                    newPositions[alive].GetComponent<AgentController>().ID = alive;
                    alive++;
                    //Its child
                    newGen[alive,0] = 100f; //Indicates it was just spawned.
                    newGen[alive,1] = currentGen[index,1];
                    for(int jndex = 2; jndex < subLength; jndex++){
                        newGen[alive,jndex] = currentGen[index,jndex] + Mathf.Pow((2f * Random.value) - 1f, 3f);
                    }
                    newPositions[alive] = Instantiate(agent);
                    newPositions[alive].GetComponent<AgentController>().ID = alive;
                    alive++;
                }
            }
        }
        newPositions = positionAgents(newPositions, newGen);
        startRound = new bool[newCreatureCount];
        for(int index = 0; index < newCreatureCount; index++) {
            startRound[index] = true;
        }
    }

    bool allFalse(bool[] conditions) {
        for(int index = 0; index < conditions.Length; index++) {
            if(conditions[index]) {
                return false;
            }
        }
        return true;
    }
    
    void Start()
    {
        SceneManager.UnloadSceneAsync("Graph");
        agents = new GameObject[creatureCount];
        agentStats = new float[creatureCount, attributes.Length + 2];
        startRound = new bool[creatureCount];
        for(int i = 0; i < creatureCount; i++){
            agentStats[i,0] = 100f; //Indicates it was just spawned.
            agentStats[i,1] = 0f;
            for(int j = 0; j < attributes.Length; j++) {
                agentStats[i, j + 2] = attributes[j];
            }
            startRound[i] = true;
            agents[i] = Instantiate(agent);
            agents[i].GetComponent<AgentController>().ID = i;
        }
        agents = positionAgents(agents, agentStats);
        GameObject f;
        for(int i = 0; i < foodCount; i++){
            f = Instantiate(food);
            f.transform.localPosition = new Vector3((Random.value - 0.5f) * (mapSize - 4f), 0.5f, (Random.value - 0.5f) * (mapSize - 4f));
        }
        editing = false;
    }

    
    void Update()
    {
        if(Input.GetKeyDown("space")) {
            if(SceneManager.sceneCount == 1) {
                SceneManager.LoadScene("Graph", LoadSceneMode.Additive);
            } else {
                SceneManager.UnloadSceneAsync("Graph");
            }
        }
        if(allFalse(startRound)) {
            editing = true;
            newGeneration(agentStats, agents, attributes.Length + 2, out agentStats, out agents);
            GameObject[] foodOnBoard = GameObject.FindGameObjectsWithTag("Food");
            for(int i = 0; i < foodOnBoard.Length; i++) {
                Destroy(foodOnBoard[i]);
            }
            GameObject f;
            for(int i = 0; i < foodCount; i++){
                f = Instantiate(food);
                f.transform.localPosition = new Vector3((Random.value - 0.5f) * (mapSize - 4f), 0.5f, (Random.value - 0.5f) * (mapSize - 4f));
            }
            editing = false;
        }
    }
}
