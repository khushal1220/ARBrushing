using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScripts : MonoBehaviour
{
    public Transform cubeA;
    public Transform cubePrefab;
    public LineRenderer[] lineRenderers;
    List<Transform> cubes = new List<Transform>();

    List<Transform> testCubes = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        Transform c = cubeA;
        int i = 0;
        cubes.Add(c);
        while (i < cubeA.childCount)
        {
            //cubes.Add(c);

            //c = c.GetChild(0);

            //if (c.childCount == 0)
            //    cubes.Add(c);
            cubes.Add(cubeA.GetChild(i++));

        }

    }

    // Update is called once per frame
    void Update()
    {
        MakeLines(cubes, lineRenderers[0]);
        MakeLines(testCubes, lineRenderers[1]);

        if (Input.GetKeyDown(KeyCode.C) || true)
        {
            //Vector3 AtoB = cubes[1].position - cubes[0].position;
            //Vector3 BtoC = cubes[2].position - cubes[1].position;

            //Debug.Log("Scalar Angle" + Vector3.Angle(AtoB, BtoC));

            //for (int i = 0; i < cubes.Count - 1; i++)
            //{
            //    Vector3 directionVector = (cubes[1].position - cubes[0].position).normalized;

            //    // Calculate the rotation of object A around its local Y-axis
            //    Quaternion rotationA = Quaternion.LookRotation(directionVector, cubes[0].up);
            //    Debug.Log($"Euler angles : {rotationA.eulerAngles}");
            //    cubes[i].localRotation = rotationA;
            //    //cubeA.rotation = rotationA;
            //}

            if (testCubes.Count == 0)
            {
                Transform prevCube = null;
                foreach (var cube in cubes)
                {
                    if (!prevCube)
                    {
                        prevCube = Instantiate(cubePrefab);
                        prevCube.localScale = Vector3.one * 15;
                    }
                    else
                    {
                        prevCube = Instantiate(cubePrefab, prevCube);
                    }
                    testCubes.Add(prevCube);

                }
            }

            for (int i = 0; i < cubes.Count - 1; i++)
            {
                Vector3 directionVector = (cubes[i + 1].position - cubes[i].position).normalized;
                //Vector3 fromDirectionVector = (cubes[i].position - cubes[i - 1].position).normalized;
                Quaternion rotationA = Quaternion.LookRotation(directionVector, testCubes[i].up);

                testCubes[i].rotation = rotationA;
            }
        }
    }

    void MakeLines(List<Transform> cubes, LineRenderer lineRenderer)
    {
        lineRenderer.positionCount = cubes.Count;

        for (int i = 0; i < cubes.Count; i++)
        {
            lineRenderer.SetPosition(i, cubes[i].position);
        }
    }
}
