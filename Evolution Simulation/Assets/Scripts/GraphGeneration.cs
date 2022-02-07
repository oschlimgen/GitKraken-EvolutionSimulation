using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGeneration : MonoBehaviour
{
    public GameObject point;
    public Transform camera;

    private GameObject[] points;
    private float[,] creatures;

    private float cameraAngleX = 0f;
    private float cameraAngleY = 0f;
    private Vector3 cameraPos;

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
        cameraPos = camera.position;
    }

    void Update()
    {
        if(creatures != TerrainGeneration.agentStats) {
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
        }
        if(Input.anyKey) {
            if(Input.GetKey("left")) {
                if(cameraAngleX < 0f && cameraAngleY <= 0f) {
                    cameraAngleX += 0.01f;
                    cameraAngleY = 0f;
                    cameraPos.x = 10f * Mathf.Sin(cameraAngleX);
                    cameraPos.y = 0f;
                    cameraPos.z = 10f * Mathf.Cos(cameraAngleX);
                    camera.localPosition = cameraPos;
                    camera.rotation = Quaternion.Euler(0f, 180f + cameraAngleX * (180f / Mathf.PI), 0f);
                }
            }
            if(Input.GetKey("right")) {
                if(cameraAngleX > -Mathf.PI / 2f && cameraAngleY <= 0f) {
                    cameraAngleX -= 0.01f;
                    cameraAngleY = 0f;
                    cameraPos.x = 10f * Mathf.Sin(cameraAngleX);
                    cameraPos.y = 0f;
                    cameraPos.z = 10f * Mathf.Cos(cameraAngleX);
                    camera.localPosition = cameraPos;
                    camera.rotation = Quaternion.Euler(0f, 180f + cameraAngleX * (180f / Mathf.PI), 0f);
                }
            }
            if(Input.GetKey("up")) {
                if(cameraAngleY < Mathf.PI / 2f && cameraAngleX <= -Mathf.PI / 2f) {
                    cameraAngleX = -Mathf.PI / 2f;
                    cameraAngleY += 0.01f;
                    cameraPos.x = -10f * Mathf.Cos(cameraAngleY);
                    cameraPos.y = 10f * Mathf.Sin(cameraAngleY);
                    cameraPos.z = 0f;
                    camera.localPosition = cameraPos;
                    camera.rotation = Quaternion.Euler(cameraAngleY * (180f / Mathf.PI), 90f, 0f);
                }
                if(cameraAngleY < Mathf.PI / 2f && cameraAngleX >= 0f) {
                    cameraAngleX = 0f;
                    cameraAngleY += 0.01f;
                    cameraPos.x = 0f;
                    cameraPos.y = 10f * Mathf.Sin(cameraAngleY);
                    cameraPos.z = 10f * Mathf.Cos(cameraAngleY);
                    camera.localPosition = cameraPos;
                    camera.rotation = Quaternion.Euler(cameraAngleY * (180f / Mathf.PI), 180f, 0f);
                }
            }
            if(Input.GetKey("down")) {
                if(cameraAngleY > 0f && cameraAngleX <= -Mathf.PI / 2f) {
                    cameraAngleX = -Mathf.PI / 2f;
                    cameraAngleY -= 0.01f;
                    cameraPos.x = -10f * Mathf.Cos(cameraAngleY);
                    cameraPos.y = 10f * Mathf.Sin(cameraAngleY);
                    cameraPos.z = 0f;
                    camera.localPosition = cameraPos;
                    camera.rotation = Quaternion.Euler(cameraAngleY * (180f / Mathf.PI), 90f, 0f);
                }
                if(cameraAngleY > 0f && cameraAngleX >= 0f) {
                    cameraAngleX = 0f;
                    cameraAngleY -= 0.01f;
                    cameraPos.x = 0f;
                    cameraPos.y = 10f * Mathf.Sin(cameraAngleY);
                    cameraPos.z = 10f * Mathf.Cos(cameraAngleY);
                    camera.localPosition = cameraPos;
                    camera.rotation = Quaternion.Euler(cameraAngleY * (180f / Mathf.PI), 180f, 0f);
                }
            }
        }
    }
}