using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using ManoMotion;
using Mediapipe.Unity;
using TMPro;

public class ARTrackingManager : MonoBehaviour
{
    public static ARTrackingManager get;
    public ARSession arSession;
    public ARCameraManager cameraManager;
    public ARFaceManager faceManager;
    public ARCameraBackground cameraBackground;
    public XROrigin origin;
    public Transform regionPrefab;
    public GameObject objectToSpawn; // Prefab to spawn
    public float zDepth = 1.0f; // Depth in the world space from the camera
    public Image debugCameraFeed;
    public RawImage debugRawCameraFeed;
    public TMP_Text fpsCounter;
    Transform region;
    GameObject spawnedObject; // Reference to the spawned object

    Dictionary<int, Transform> m_TrackedObjects = new Dictionary<int, Transform>();
    private void Awake()
    {
        get = this;
        Application.targetFrameRate = 60;
    }
    // Start is called before the first frame update
    float time;
    float smoothedFPS;
    void Start()
    {
        // ToggleFacingCamera();
        //StartCoroutine(DebugVisual());
        faceManager.facesChanged += onFaceChanged;
    }
    // Update is called once per frame
    void Update()
    {
        time = 1 / Time.deltaTime;
        smoothedFPS = Mathf.Lerp(smoothedFPS, time, 1 - .9f);
        fpsCounter.text = smoothedFPS.ToString(".00");
        //handleFace();
        //handleHand();
    }
    void onFaceChanged(ARFacesChangedEventArgs args)
    {
        foreach (var addedFace in args.added)
        {
            Debug.Log("Face added: " + addedFace.trackableId);
        }

        foreach (var removedFace in args.removed)
        {
            m_TrackedObjects.Clear();
        }
    }
    void handleFace()
    {
        syncTrackedObjects();
    }
    void syncTrackedObjects()
    {
        // Ensure there are tracked objects
        if (m_TrackedObjects == null || m_TrackedObjects.Count == 0) return;

        // Ensure there are active faces
        if (FaceManager.instances == null || FaceManager.instances.Count == 0) return;

        // Use the first available face (or handle multiple if needed)
        FaceManager faceManager = FaceManager.instances[0];
        if (faceManager == null) return;

        // Ensure MeshFilter and Mesh exist
        MeshFilter meshFilter = faceManager.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null) return;
        foreach (var tObject in m_TrackedObjects)
        {
            if (tObject.Value == null || tObject.Value.transform == null)
            {
                Debug.LogError("Tracked object or its transform is null.");
                continue;
            }

            if (tObject.Key >= mesh.vertices.Length)
            {
                Debug.LogError("Key exceeds mesh vertex count.");
                continue;
            }
            // Transform the vertex to world position
            Vector3 vertex = mesh.vertices[tObject.Key];
            Vector3 landmark = faceManager.transform.TransformPoint(vertex);

            // Update tracked object's position and parent
            tObject.Value.position = landmark;
            tObject.Value.parent = faceManager.transform;
            tObject.Value.rotation = Quaternion.identity;
        }
    }
    public void SyncFaceObject(Transform m_Object, int landmark)
    {

        // Ensure there are active faces
        if (FaceManager.instances == null || FaceManager.instances.Count == 0) return;

        // Use the first available face (or handle multiple if needed)
        FaceManager faceManager = FaceManager.instances[0];
        if (faceManager == null) return;

        // Ensure MeshFilter and Mesh exist
        MeshFilter meshFilter = faceManager.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null) return;

        if (landmark >= mesh.vertices.Length)
        {
            Debug.LogError("Key exceeds mesh vertex count.");
            return;
        }

        Vector3 vertex = mesh.vertices[landmark];
        Vector3 landmarkPos = faceManager.transform.TransformPoint(vertex);

        // Update tracked object's position and parent
        //Debug.Log(landmarkPos);
        m_Object.position = landmarkPos;
        m_Object.parent = faceManager.transform;
        m_Object.rotation = Quaternion.identity;

    }
    public bool mapObjectWithLandmark(Transform element, int Landmark)
    {
        if (m_TrackedObjects.ContainsKey(Landmark)) return false;
        m_TrackedObjects.Add(Landmark, element);
        return true;
    }
    void OnDestroy()
    {
    }
    public void ToggleFacingCamera()
    {
        Debug.Log("Changing Camera");

        // Stop the camera


        // Adding a delay to ensure proper camera reset
        StartCoroutine(RestartCamera());

    }
    private IEnumerator RestartCamera()
    {
        yield return new WaitForSeconds(.5f);
        cameraManager.GetComponent<ARCameraBackground>().enabled = false;
        cameraManager.subsystem.Stop();
        cameraManager.requestedFacingDirection =
                cameraManager.currentFacingDirection == CameraFacingDirection.World
                ? CameraFacingDirection.User
                : CameraFacingDirection.World;
        //arSession.Reset();
        cameraManager.GetComponent<ARCameraBackground>().enabled = true;
        cameraManager.subsystem.Start();  // Restart camera after a brief delay
        arSession.Reset();
        cameraManager.GetComponent<ARCameraBackground>().enabled = true;

        Debug.Log("Camera Changed");

    }

    IEnumerator DebugVisual()
    {
        if (cameraBackground.material.mainTexture == null)
        {
            Debug.Log("<color=red>Material is Null</color>");
        }
        else
        {
            Debug.Log($"Main Texture: {cameraBackground.material.mainTexture}, Type: {cameraBackground.material.mainTexture.GetType()}, Dimensions: {cameraBackground.material.mainTexture.width}x{cameraBackground.material.mainTexture.height}");
            RenderTexture renderTexture = new RenderTexture(cameraBackground.material.mainTexture.width, cameraBackground.material.mainTexture.height, 1, RenderTextureFormat.ARGB32);
            Graphics.Blit(null, renderTexture, cameraBackground.material);
            Sprite sprite = TextureToSpriteConversion(renderTexture);
            debugCameraFeed.sprite = sprite;
            debugRawCameraFeed.texture = renderTexture;
            Debug.Log(sprite);
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(DebugVisual());
    }
    Sprite TextureToSpriteConversion(Texture texture)
    {
        // Convert Texture (e.g., Texture2D or RenderTexture) to Sprite
        if (texture is Texture2D)
        {
            Texture2D texture2D = (Texture2D)texture;
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        }
        else if (texture is RenderTexture)
        {
            RenderTexture renderTexture = (RenderTexture)texture;
            // Read RenderTexture into Texture2D
            RenderTexture.active = renderTexture;
            Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height);
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;

            // Convert to Sprite
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        }

        return null;
    }
    void handleHand()
    {
        // Check if a hand is detected
        bool isHandDetected = ManomotionManager.Instance.Hand_infos.Length > 0;

        if (isHandDetected)
        {
            // Get normalized hand position
            Vector3 handPositionNormalized = ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.palm_center;

            // Convert normalized position to world position
            Camera mainCamera = Camera.main;
            Vector3 screenPosition = new Vector3(handPositionNormalized.x, handPositionNormalized.y, zDepth);
            Vector3 handWorldPosition = mainCamera.ViewportToWorldPoint(screenPosition);

            // Spawn the object if it doesn't exist
            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(objectToSpawn, handWorldPosition, Quaternion.identity);
            }
            else
            {
                // Update the position of the spawned object
                spawnedObject.transform.position = handWorldPosition;
                //Quaternion handRotation = ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.;

                // Update object rotation
                //spawnedObject.transform.rotation = handRotation;
            }
        }
        else
        {
            // Destroy the object if the hand is not detected
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }
        }
    }
}
