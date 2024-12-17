using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
#if UNITY_ANDROID
using UnityEngine.XR.ARCore;
#endif
public class ARTrackingManager : MonoBehaviour
{
    public ARSession arSession;
    public ARCameraManager cameraManager;
    public ARFaceManager faceManager;
    public XROrigin origin;
    public Transform regionPrefab;

    Transform region;
#if UNITY_ANDROID
    NativeArray<ARCoreFaceRegionData> m_FaceRegions;

    Dictionary<TrackableId, Dictionary<ARCoreFaceRegion, GameObject>> m_InstantiatedPrefabs;
#endif

    Dictionary<int,Transform> m_TrackedObjects = new Dictionary<int, Transform>();
    // Start is called before the first frame update
    void Start()
    {
        // ToggleFacingCamera();
#if UNITY_ANDROID && !UNITY_EDITOR
            m_InstantiatedPrefabs = new Dictionary<TrackableId, Dictionary<ARCoreFaceRegion, GameObject>>();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        handleFace();
    }

    void handleFace()
    {
        syncTrackedObjects();
#if UNITY_ANDROID
        return;
        var subsystem = (ARCoreFaceSubsystem)faceManager.subsystem;
        if (subsystem == null)
            return;
        foreach (var face in faceManager.trackables)
        {
            Dictionary<ARCoreFaceRegion, GameObject> regionGos;
            if (!m_InstantiatedPrefabs.TryGetValue(face.trackableId, out regionGos))
            {
                regionGos = new Dictionary<ARCoreFaceRegion, GameObject>();
                m_InstantiatedPrefabs.Add(face.trackableId, regionGos);
            }

            subsystem.GetRegionPoses(face.trackableId, Allocator.Persistent, ref m_FaceRegions);
            for (int i = 0; i < m_FaceRegions.Length; ++i)
            {
                var regionType = m_FaceRegions[i].region;

                GameObject go;
                if (!regionGos.TryGetValue(regionType, out go))
                {
                    go = Instantiate(regionPrefab.gameObject, origin.TrackablesParent);
                    regionGos.Add(regionType, go);
                }

                go.transform.localPosition = m_FaceRegions[i].pose.position;
                go.transform.localRotation = m_FaceRegions[i].pose.rotation;
            }
        }
#endif
    }
    void syncTrackedObjects()
    {
        foreach (var tObject in m_TrackedObjects)
        {
            if (FaceManager.instances.Count == 0) return;
            if (FaceManager.instances[0] == null) return;
            //FaceManager faceManager = FaceManager.instances[Random.Range(0, FaceManager.instances.Count-1)];
            FaceManager faceManager = FaceManager.instances[0];
            MeshFilter meshFilter = faceManager.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.sharedMesh;
            if(mesh.vertices.Length <= tObject.Key) continue;
            Vector3 landmark = faceManager.transform.TransformPoint(mesh.vertices[tObject.Key]);
            tObject.Value.position = landmark;
            tObject.Value.parent = faceManager.transform;
        }
    }
    public bool mapObjectWithLandmark(Transform element, int Landmark)
    {
        if(m_TrackedObjects.ContainsKey(Landmark))return false;
        m_TrackedObjects.Add(Landmark,element);
        return true;
    }
    void OnDestroy()
    {
#if UNITY_ANDROID
        if (m_FaceRegions.IsCreated)
            m_FaceRegions.Dispose();
#endif
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
}
