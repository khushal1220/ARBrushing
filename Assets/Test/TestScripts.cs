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

    public Transform brush; // The brush GameObject (e.g., tracked by hand tracking).
    public float brushRadius = 20; // Radius of the brush in pixels.
    public Color brushColor = Color.red; // The color applied by the brush.

    private Texture2D faceTexture;
    public Material faceMaterial;
    Mesh faceMesh;

    bool isSetUp = false;
    void Start()
    {
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
        if (Input.GetMouseButton(0) && GetComponent<MeshFilter>())
        {
            Brush();
        }
    }

    void Brush()
    {
        if (!isSetUp) SetupFaceCanvas();
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

        if (Physics.Raycast(ray, out hit))
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

    void SetupUVs()
    {
        faceMesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = faceMesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            // Map vertex local positions to UV coordinates (normalized between 0 and 1)
            uvs[i] = new Vector2(vertices[i].x + 0.5f, vertices[i].y + 0.5f);
        }

        faceMesh.uv = uvs; // Assign the UVs to the mesh
    }
    void SetupUVs1()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.mesh == null)
        {
            Debug.LogError("MeshFilter or Mesh not found on the object!");
            return;
        }

        Mesh mesh = meshFilter.mesh;

        // Generate UVs if they are not already set
        if (mesh.uv == null || mesh.uv.Length != mesh.vertexCount)
        {
            Debug.LogWarning("UVs not found or incomplete on the mesh. Generating default UVs.");
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = new Vector2[vertices.Length];

            // Map UVs based on bounding box projection
            Bounds bounds = mesh.bounds;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                uvs[i] = new Vector2(
                    (vertex.x - bounds.min.x) / bounds.size.x,
                    (vertex.y - bounds.min.y) / bounds.size.y
                );
            }

            mesh.uv = uvs;
        }
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
