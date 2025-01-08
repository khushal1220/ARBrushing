using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;

public class TestScripts : MonoBehaviour
{
    public Transform cubeA;
    public Transform cubePrefab;
    public LineRenderer[] lineRenderers;
    List<Transform> cubes = new List<Transform>();

    List<Transform> testCubes = new List<Transform>();

    public Transform brush; // The brush GameObject (e.g., tracked by hand tracking).
    public float brushRadius = 20; // Radius of the brush in pixels.
    public Color brushColor = Color.red; // The color applied by the brush.

    private Texture2D faceTexture;
    public Material faceMaterial;
    public MeshRenderer lipstickMesh;
    Mesh faceMesh;

    bool isSetUp = false;
    Transform index;
    MPTrackingManager mpManager;
    void Start()
    {
        mpManager = MPTrackingManager.get;
        lipstickMesh = mpManager.LipStickMeshRendrer;
        //Transform c = cubeA;
        //int i = 0;
        //cubes.Add(c);
        //while (i < cubeA.childCount)
        //{
        //    cubes.Add(cubeA.GetChild(i++));

        //}
        //SetupFaceCanvas();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButton(0) && GetComponent<MeshFilter>())
        //{
        //    CheckBrush();
        //}
        CheckBrush();

    }
    Transform GetHandLandmark(int landmark)
    {
        return mpManager.GetHandLandmark(landmark);
    }

    float ratioPalm;
    float maxPalm = 0;

    void CheckBrush()
    {
        //if (GetHandLandmark(8))
        Color color = new Color(1, 1, 1, 1);

        if (GetHandLandmark(8))
        {
            Vector3 pos = (GetHandLandmark(5).position + GetHandLandmark(9).position + GetHandLandmark(13).position + GetHandLandmark(17).position) / 4;
            Vector3 wristPos = GetHandLandmark(0).position;

            bool indexRaised = IsFingerRaised(8, 5);
            bool middleRaised = IsFingerRaised(12, 9);
            if (indexRaised && middleRaised)
            {
                //Debug.LogError("Both index and middle fingers are raised!");
                //lipstickMesh.material.SetColor("_BaseColor", new Color(255, 255, 255, 255));

                color.a = .3f;
                lipstickMesh.material.color = color;
            }
            else
            {
                //lipstickMesh.material.SetColor("_BaseColor", new Color(255, 255, 255, 90));
                lipstickMesh.material.color = color;
                Brush();
                //Debug.LogError("Down");
            }
        }
        else
        {
            color.a = .3f;
            lipstickMesh.material.color = color;
        }

        return;
        if (mpManager.isActive())
        {
            Transform index = mpManager.GetHandLandmark(8);
            Transform indexBase = mpManager.GetHandLandmark(5);
            Transform thumb = mpManager.GetHandLandmark(4);
            Transform wrist = mpManager.GetHandLandmark(0);
            float palmDistace = Vector2.Distance(mpManager.GetHandLandmark(1).position, mpManager.GetHandLandmark(17).position);
            if (maxPalm < palmDistace)
            {
                maxPalm = palmDistace;
            }

            ratioPalm = palmDistace / maxPalm;
            Vector3 pos = (mpManager.GetHandLandmark(5).position + mpManager.GetHandLandmark(9).position + mpManager.GetHandLandmark(13).position + mpManager.GetHandLandmark(17).position) / 4;
            float distanceAverageToIndex = Vector2.Distance(pos, indexBase.position);
            float distanceAverageToThumb = Vector2.Distance(pos, thumb.position);
            //Debug.LogError(distanceAverageToIndex > distanceAverageToThumb && ratioPalm > .5f);
            if (distanceAverageToIndex > distanceAverageToThumb && ratioPalm > .5f)
            {
                Debug.Log("true");
                Brush();
            }
            else
            {
                Debug.Log("false");
            }
        }
        else
        {
            Debug.Log($"Landmark is null {mpManager.isActive()}");
        }
    }
    private bool IsFingerRaised(int tipIndex, int mcpIndex)
    {
        Vector3 tip = GetHandLandmark(tipIndex).position;
        Vector3 mcp = GetHandLandmark(mcpIndex).position;
        Vector3 wrist = GetHandLandmark(0).position;

        // Calculate distances
        float tipToWristDistance = Vector3.Distance(tip, wrist);
        float mcpToWristDistance = Vector3.Distance(mcp, wrist);

        // A finger is raised if the tip is significantly further from the wrist compared to its base
        return tipToWristDistance > mcpToWristDistance * 1.5f; // Adjust multiplier for sensitivity
    }
    void Brush()
    {
        if (!isSetUp) SetupFaceCanvas();
        if (index == null)
        {
            index = GetHandLandmark(8);
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int pixelX = 0;
        int pixelY = 0;
        //for (int i = 40; i < 50; i++)
        //{
        //    for (int j = 0; j < 100; j++)
        //    {
        //        faceTexture.SetPixel(i, j, brushColor);
        //    }
        //}

        // if (Physics.Raycast(ray, out hit))
        if (Physics.Raycast(Camera.main.transform.position, index.position - Camera.main.transform.position, out hit))
        {
            // Get the material of the hit object.
            Debug.Log("Enter RayCast");
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.material.mainTexture is Texture2D texture)
            {
                // Get the UV coordinates at the hit point.
                Vector2 uv = hit.textureCoord;

                // Convert UV to texture pixel coordinates.
                pixelX = Mathf.FloorToInt(uv.x * texture.width);
                pixelY = Mathf.FloorToInt(uv.y * texture.height);
                Debug.Log($"Width: {texture.width} X Height: {texture.height}");
                Debug.Log($"UV {uv.x} {uv.y}");
                // Get the pixel color from the texture.
                Color pixelColor = texture.GetPixel(pixelX, pixelY);
                faceTexture.SetPixel(pixelX, pixelY, brushColor);

                Debug.Log($"Pixel Color: {pixelColor} X: {pixelX} Y: {pixelY}");
            }
        }


        for (int x = (int)-brushRadius; x <= brushRadius; x++)
        {
            for (int y = (int)-brushRadius; y <= brushRadius; y++)
            {
                // Check if the pixel is within the circular brush radius.
                if (x * x + y * y <= brushRadius * brushRadius)
                {
                    int centerX = Mathf.Clamp(pixelX + x, 0, faceTexture.width - 1);
                    int centerY = Mathf.Clamp(pixelY + y, 0, faceTexture.height - 1);
                    faceTexture.SetPixel(centerX, centerY, brushColor);
                    //Debug.Log($"X: {centerX} Y: {centerY}");
                }
            }
        }
        faceTexture.Apply();
        //faceMaterial.mainTexture = faceTexture;

    }

    void SetupFaceCanvas()
    {
        //SetupUVs();
        NormalizeUVs(GetComponent<MeshFilter>().mesh);
        faceMaterial = GetComponent<MeshRenderer>().material;
        int textureWidth = 1000;
        int textureHeight = 1000;
        faceTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        ClearTexture(faceTexture);

        // Assign the texture to the face material.
        if (faceMaterial != null)
        {
            faceMaterial.mainTexture = faceTexture;
        }
        isSetUp = true;
    }
    void NormalizeUVs(Mesh mesh)
    {
        Vector2[] uvs = mesh.uv;

        // Calculate UV bounds
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (Vector2 uv in uvs)
        {
            min = Vector2.Min(min, uv);
            max = Vector2.Max(max, uv);
        }

        Vector2 size = max - min;

        // Normalize UVs to fit entire 0-1 space
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(
                (uvs[i].x - min.x) / size.x,
                (uvs[i].y - min.y) / size.y
            );
        }

        // Apply the normalized UVs
        mesh.uv = uvs;
    }

    void SetColor(int pixelX, int pixelY, Color color)
    {
        Color currentColor = faceTexture.GetPixel(pixelX, pixelY);
        faceTexture.SetPixel(pixelX, pixelY, Color.Lerp(currentColor, color, 0.5f));
    }
    private void ClearTexture(Texture2D texture)
    {
        // Set all pixels to transparent.
        Color clearColor = new Color(0, 0, 0, 0); // Transparent
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = clearColor;
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

    void SetupLines()
    {
        MakeLines(cubes, lineRenderers[0]);
        MakeLines(testCubes, lineRenderers[1]);

        if (Input.GetKeyDown(KeyCode.C) || true)
        {
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
