using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGeneration : MonoBehaviour
{
    public GameObject point;
    public GameObject camera;

    private GameObject[] points;
    private float[,] creatures;
    private bool reload = false;

    public float cameraAngle = Mathf.PI / 2f;
    public Vector3 cameraPos;

    void Start()
    {
        creatures = TerrainGeneration.agentStats;
        points = new GameObject[TerrainGeneration.agents.Length];
        Vector3 placement = new Vector3(0f, 0f, 0f);
        for(int i = 0; i < points.Length; i++) {
            placement.x = -(creatures[i,2] - 5f);
            placement.y = creatures[i,3] - 5f;
            placement.z = creatures[i,4] - 5f;
            points[i] = Instantiate(point, gameObject.transform);
            points[i].transform.localPosition = placement;
        }
        cameraPos = camera.transform.position;
    }

    void Update()
    {
        if(TerrainGeneration.editing) {
            reload = true;
        } else {
            if(!TerrainGeneration.editing) {
                for(int i = 0; i < points.Length; i++) {
                    Destroy(points[i]);
                }
                creatures = TerrainGeneration.agentStats;
                points = new GameObject[TerrainGeneration.agents.Length];
                Vector3 placement = new Vector3(0f, 0f, 0f);
                for(int i = 0; i < points.Length; i++) {
                    placement.x = -creatures[i,2] + 5f;
                    placement.y = creatures[i,3] - 5f;
                    placement.z = creatures[i,4] - 5f;
                    points[i] = Instantiate(point, gameObject.transform);
                    points[i].transform.localPosition = placement;
                }
                reload = false;
            }
        }
        if(Input.GetKey("right")) {
            if(cameraAngle < Mathf.PI) {
                cameraAngle += 0.01f;
                cameraPos.x = 10f * Mathf.Sin(cameraAngle);
                cameraPos.z = 10f * Mathf.Cos(cameraAngle);
                camera.transform.localPosition = cameraPos;
                camera.transform.rotation = Quaternion.Euler(0f, cameraAngle * (180f / Mathf.PI), 0f);
            }
        }
        if(Input.GetKey("left")) {
            if(cameraAngle > Mathf.PI / 2f) {
                cameraAngle += 0.01f;
                cameraPos.x = 10f * Mathf.Sin(cameraAngle);
                cameraPos.z = 10f * Mathf.Cos(cameraAngle);
                camera.transform.localPosition = cameraPos;
                camera.transform.rotation = Quaternion.Euler(0f, cameraAngle * (180f / Mathf.PI), 0f);
            }
        }
    }
}