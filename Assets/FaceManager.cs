using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceManager : MonoBehaviour
{
    public static List<FaceManager> instances = new List<FaceManager>();
    // Start is called before the first frame update
    void Start()
    {
        instances.Add(this);
    }

    private void OnDisable()
    {
        instances.Remove(this);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }
}
