using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class AutoLabels : MonoBehaviour
{
    public Transform Reference;
    public Transform[] Labels;
    public string LabelPrefix;
    public string Separation;
    public string NoDynamicPrefix;
    public string NonRotationPrefix;
    public string NonScalePrefix;
    public Transform labelPrefab;
    public Vector3 RotationOffset;
    public Vector3 ScaleOffset;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var label in Labels)
        {
            Transform newLabel = Instantiate(labelPrefab);
            newLabel.parent = label;
            newLabel.position = label.position;
            newLabel.localScale = ScaleOffset;
            newLabel.localRotation = Quaternion.Euler(RotationOffset);

            string labelName = label.name;
            labelName.Replace(LabelPrefix, "");
            string[] name;
            name = labelName.Split(Separation);
            if (name[0].Contains(NoDynamicPrefix))
            {
                Destroy(newLabel.GetComponent<FaceCamera>());
            }
            else
            {
                if (name[0].Contains(NonScalePrefix))
                {
                    newLabel.GetComponent<FaceCamera>().canScale = false;
                }
                else if (name[0].Contains(NonRotationPrefix))
                {
                    newLabel.GetComponent<FaceCamera>().canRotate = false;
                }
                newLabel.GetComponent<FaceCamera>().Refernce = Reference;
            }

            string[] lines = name[1].Split("\\");
            newLabel.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = lines[0] + "\n";
            for (int i = 1; i < lines.Length; i++)
            {
                newLabel.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text += lines[i];
            }

        }
    }

}