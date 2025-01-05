using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FaceManager : MonoBehaviour
{
    public static List<FaceManager> instances = new List<FaceManager>();
    public String sceneName = "HandTracking";
    public Transform[] dirtParticles;
    public string brushTag;

    Transform[] currDirt;

    List<int> excludedIndices = new List<int>() {
        // Example index ranges (adjust based on Mediapipe spec)
        1, 2, 3, 4, 5, 6, 7, 8, 9,   // Nose
        61, 62, 63, 64, 65, 66, 67,  // Lips
        33, 34, 35, 36, 37, 38, 39,  // Eyes
        // Add more based on face anatomy
    };
    List<int> validIndices = new List<int>();

    Dictionary<int, Transform> faceObjects = new Dictionary<int, Transform>();
    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        //List<int> validIndices = Enumerable.Range(0, 468).Where(index => !excludedIndices.Contains(index)).ToList();
        //for (int i = 0; i < 268; i++)
        //{
        //    if (excludedIndices.Contains(i)) continue;
        //    validIndices.Add(i);
        //}
        validIndices = new List<int>()
        {
            6,8,9,10,21,32,34,36,50,54,58,67,68,69,71,92,93,101,103,104,108,109,111,116,117,118,
            123,127,132,135,136,137,138,139,140,143,147,148,149,150,151,152,162,164,165,167,168,
            169,170,171,172,175,176,177,187,192,194,197,199,200,201,202,203,205,206,207,208,210,211,212,213,
            214,215,216,227,234,251,262,264,280,284,288,297,298,299,301,322,323,329,330,332,333,337,338,
            340,345,346,347,348,352,356,361,364,365,366,367,368,369,372,376,377,378,379,389,391,393,
            394,395,396,397,400,401,411,416,418,421,422,424,425,426,427,428,430,431,432,433,434,435,436,447,454
        };
        //validIndices = cheekIndices.Concat(headIndices).ToList();
        instances.Add(this);
        clearFace();
        for (int i = 0; i < 15; i++)
        {
            placeDirt();
        }
    }
    private void Update()
    {
        Dictionary<int, Transform> faceObjects = this.faceObjects;
        foreach (var faceObject in faceObjects)
        {
            if (faceObject.Value == null)
            {

                //this.faceObjects.Remove(faceObject.Key);
                continue;
            }
            if (!faceObject.Value.gameObject.activeSelf) continue;
            ARTrackingManager.get.SyncFaceObject(faceObject.Value, faceObject.Key);
            Vector3 directionToCamera = Camera.main.transform.position - faceObject.Value.position;
            Ray ray = new Ray(faceObject.Value.position, directionToCamera);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the obstructing object has the "Brush" tag
                if (hit.collider.CompareTag(brushTag))
                {
                    faceObject.Value.gameObject.SetActive(false);
                }
            }
        }
    }
    void placeDirt()
    {
        if (dirtParticles.Length == 0) return;
        Transform particle = dirtParticles[Random.Range(0, dirtParticles.Length - 1)];
        Transform newParticle = Instantiate(particle);
        if (validIndices.Count == 0) return;


        int landmark = validIndices[getRandomLandmark()];
        if (faceObjects.ContainsKey(landmark))
        {
            placeDirt();
            return;
        }
        validIndices.Remove(landmark);
        faceObjects.Add(landmark, newParticle);
    }
    int getRandomLandmark()
    {
        int randNumber = Random.Range(0, validIndices.Count - 1);
        if (faceObjects.ContainsKey(randNumber)) return getRandomLandmark();
        return randNumber;
    }
    void clearFace()
    {
        Dictionary<int, Transform> faceObjects = this.faceObjects;
        foreach (var faceObject in faceObjects)
        {
            if (faceObject.Value)
            {
                Destroy(faceObject.Value.gameObject);
            }
            this.faceObjects.Remove(faceObject.Key);
        }
    }

    private void OnDisable()
    {
        clearFace();
        instances.Remove(this);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }
}
