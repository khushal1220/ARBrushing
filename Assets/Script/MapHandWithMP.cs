using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[System.Serializable]
public class BonePair
{
    public Transform bone;
    public int index;
}

public class MapHandWithMP : MonoBehaviour
{
    public BonePair[] FingersBaseWithIndex;
    public Transform RootBone;
    public float HandRotNormalizer;
    public float GizmoSphereRadius;

    MPTrackingManager mpManager;
    Dictionary<Transform, Quaternion> initRot = new Dictionary<Transform, Quaternion>();
    // Start is called before the first frame update
    void Start()
    {
        mpManager = GetComponent<MPTrackingManager>();
        foreach (var pair in FingersBaseWithIndex)
        {
            Transform bone = pair.bone;
            initRot.Add(bone, bone.localRotation);
            if (pair.index == 0) continue;
            for (int i = 1; i < 4; i++)
            {
                bone = bone.GetChild(0);
                initRot.Add(bone, bone.localRotation);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHand();
    }
    void UpdateHand()
    {
        Transform[] landMarks = new Transform[21];

        if (mpManager.GetHandLandmark(20) == null) return;
        //for (int i = 0; i < 21; i++)
        //{
        //    Transform landmark = mpManager.GetHandLandmark(i);
        //    if (landmark == null) return;
        //    landMarks[i] = landmark;
        //}
        foreach (var pair in FingersBaseWithIndex)
        {
            int index = pair.index;
            Transform landMark = mpManager.GetHandLandmark(index);
            landMarks[index] = landMark;

            for (int i = 1; i < 4; i++)
            {
                landMark = mpManager.GetHandLandmark(index + i);
                landMarks[index + i] = landMark;
            }
        }

        foreach (var pair in FingersBaseWithIndex)
        {
            int index = pair.index;
            Transform bone = pair.bone;

            if (index == 0)
            {
                //AlignBoneToDirection(bone, landMarks[0], landMarks[9]);
                //Vector3 targetDirection = landMarks[9].position - landMarks[0].position;
                //bone.rotation = Quaternion.LookRotation(targetDirection);
                AlignBoneToDirectionWithoutRestrictions(bone, landMarks[index + 1], landMarks[index], bone.parent);

                continue;
            }
            //if (index == 1)
            //{
            //    AlignBoneToDirectionWithoutRestrictions(bone, landMarks[index + 1], landMarks[index], bone.parent);
            //}
            //else
            //{
            //    AlignBaseBoneToDirection(bone, landMarks[index + 1], landMarks[index], bone.parent);
            //}
            AlignBaseBoneToDirection(bone, landMarks[index + 1], landMarks[index], bone.parent);

            for (int i = 0; i < 2 && bone.childCount > 0; i++)
            {
                bone = bone.GetChild(0);
                if (bone.childCount != 0)
                {
                    AlignBoneToDirection(bone, landMarks[index + i + 2], landMarks[index + i + 1], bone.parent);

                    //if (index != 1)
                    //else
                    //    AlignBoneToDirectionWithoutRestrictions(bone, landMarks[index + i + 2], landMarks[index + i + 1], bone.parent);

                }
            }

        }
    }
    void AlignBaseBoneToDirection(Transform bone, Transform a, Transform b, Transform c)
    {

        Vector3 targetDirection = a.position - b.position;
        Quaternion relativeRotation = Quaternion.LookRotation(targetDirection);
        //bone.localRotation = Quaternion.Inverse(c.rotation) * relativeRotation;
        Vector3 up = (Quaternion.Inverse(c.rotation) * relativeRotation) * bone.up;
        //float angleX = Vector3.SignedAngle(up, c.up, Vector3.right);
        float angleX = Vector3.Angle(up, c.up);
        Debug.Log(angleX);
        Vector3 eulerAngles = new Vector3(angleX, 0, 0);
        //eulerAngles.x = Mathf.Abs(eulerAngles.x);
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, 0, 90);
        bone.localRotation = Quaternion.Euler(eulerAngles);
        //bone.localRotation = Quaternion.Inverse(c.rotation) * relativeRotation;
    }

    void AlignBoneToDirection(Transform bone, Transform a, Transform b, Transform c)
    {
        Vector3 targetDirection = a.position - b.position;
        Quaternion relativeRotation = Quaternion.LookRotation(targetDirection);
        //bone.localRotation = Quaternion.Inverse(c.rotation) * relativeRotation;
        Vector3 up = (Quaternion.Inverse(c.rotation) * relativeRotation) * bone.up;
        float angleX1 = Vector3.SignedAngle(up, c.up, Vector3.right);
        float angleX = Vector3.Angle(up, c.up);
        Debug.Log($"Single Angle : {angleX1} Angle : {angleX} Transform :{bone.eulerAngles.x}");
        Vector3 eulerAngles = new Vector3(angleX, 0, 0);
        //eulerAngles.x = Mathf.Abs(eulerAngles.x);
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, 0, 90);
        bone.localRotation = Quaternion.Euler(eulerAngles);
        //bone.localRotation = Quaternion.Inverse(c.rotation) * relativeRotation;
    }

    void AlignBoneToDirectionWithoutRestrictions(Transform bone, Transform a, Transform b, Transform c)
    {
        Vector3 targetDirection = a.position - b.position;
        Quaternion relativeRotation = Quaternion.LookRotation(targetDirection);
        bone.localRotation = Quaternion.Inverse(c.rotation) * relativeRotation;
    }
    private void OnDrawGizmos()
    {
        foreach (var pair in FingersBaseWithIndex)
        {
            //Vector3 HandPosNormalizer = Vector3.one;
            Transform bone = pair.bone;
            Gizmos.DrawSphere(bone.position, GizmoSphereRadius);
            for (int i = 0; i < 3 && bone.childCount > 0; i++)
            {
                bone = bone.GetChild(0);
                Gizmos.DrawSphere(bone.position, GizmoSphereRadius);
            }

        }
    }
}
