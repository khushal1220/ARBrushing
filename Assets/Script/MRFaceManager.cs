using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this is an class for the Mideapipe related tracking
public class MRFaceManager : MonoBehaviour
{
    public Transform FaceLandmarkParent;
    public Transform HandLandmarkParent;

    public Transform FacePrefab;
    public Transform HandPrefab;

    public float smoothingFactor;
    public float brushRotationSpeed;

    public float faceScaleFactor;
    public float handScaleFactor;

    Dictionary<int, Transform> faceLandmarks = new Dictionary<int, Transform>();
    Dictionary<int, Transform> handLandmarks = new Dictionary<int, Transform>();

    Transform faceLandmarkParent;
    Transform handLandmarkParent;

    Transform face;
    Transform hand;

    float faceInitScaleFactor;
    float handInitScaleFactor;

    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(setupHandLandmarks), 2);
    }

    [ContextMenu("FaceSetup")]
    void setupFaceLandmark()
    {
        if (FaceLandmarkParent.childCount == 0) return;
        faceLandmarkParent = FaceLandmarkParent.GetChild(0).GetChild(0).GetChild(0);

        if (!face)
        {
            face = Instantiate(FacePrefab);
        }
        faceInitScaleFactor = Vector3.Distance(GetFaceLandmark(33).position, GetFaceLandmark(133).position);
    }

    [ContextMenu("HandSetup")]
    void setupHandLandmarks()
    {
        if (HandLandmarkParent.childCount == 0) return;
        handLandmarkParent = HandLandmarkParent?.GetChild(0)?.GetChild(0);

        if (!hand)
        {
            hand = Instantiate(HandPrefab);
        }
    }
    Transform GetFaceLandmark(int key)
    {
        if (faceLandmarks.ContainsKey(key))
        {
            return faceLandmarks[key];
        }
        else
        {
            Transform landmark = faceLandmarkParent.GetChild(key);
            faceLandmarks.Add(key, landmark);
            return landmark;
        }
    }
    Transform GetHandLandmark(int key)
    {
        if (handLandmarks.ContainsKey(key))
        {
            return handLandmarks[key];
        }
        else
        {
            Transform landmark = handLandmarkParent.GetChild(key);
            handLandmarks.Add(key, landmark);
            return landmark;
        }
    }
    // Update is called once per frame
    void Update()
    {
        handleFace();
        handleHand();
    }

    void handleFace()
    {
        if (!face)
        {
            setupFaceLandmark();
        }
        else
        {
            float currentDistance = Vector3.Distance(GetFaceLandmark(33).position, GetFaceLandmark(133).position);
            float scaleFactor = currentDistance / faceInitScaleFactor;
            //face.localScale = Vector3.one * scaleFactor;
            face.localScale = Vector3.Lerp(face.localScale, faceScaleFactor * Vector3.one * scaleFactor, smoothingFactor * Time.deltaTime);

            face.position = GetFaceLandmark(1).position;
        }
    }

    void handleHand()
    {
        if (!hand)
        {
            setupHandLandmarks();
        }
        else
        {
            Vector3 wristPosition = GetHandLandmark(0).position;
            Vector3 indexTipPosition = GetHandLandmark(9).position;
            Vector3 thumbBasePosition = GetHandLandmark(1).position;
            float distance = Vector3.Distance(wristPosition, indexTipPosition);

            float scaleFactor = handScaleFactor * distance;

            hand.GetChild(0).localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            hand.position = (wristPosition + indexTipPosition) / 2;

            Vector3 wristToThumbBase = thumbBasePosition - wristPosition;

            float zRotation = Mathf.Atan2(wristToThumbBase.y, wristToThumbBase.x) * Mathf.Rad2Deg;

            //hand.GetChild(0).rotation = Quaternion.Euler(0, 0, zRotation);
            hand.GetChild(0).rotation = Quaternion.Slerp(hand.GetChild(0).rotation, Quaternion.Euler(0, 0, zRotation), Time.deltaTime * brushRotationSpeed);

        }
    }

}
