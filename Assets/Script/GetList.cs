using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetList : MonoBehaviour
{
    List<Transform> landmarks = new List<Transform>();
    // Start is called before the first frame update
    void Start()
    {
        string list = "{";
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform Landmark = transform.GetChild(i);
            if (Landmark.GetComponent<MeshRenderer>().enabled)
            {
                landmarks.Add(Landmark);
                list += i.ToString() + (i < transform.childCount - 1 ? "," : " ");
            }
        }
        list += "}";
        Debug.Log(list);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
