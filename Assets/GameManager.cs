using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
public class GameManager : MonoBehaviour
{
    public String sceneName = "HandTracking";
    public Transform[] dirtParticles;

    Transform[] currDirt;
    // Start is called before the first frame update
    void Start()
    {
        // SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        placeDirt();
        placeDirt();
        placeDirt();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void placeDirt()
    {
        if(dirtParticles.Length == 0) return;
        Transform particle = dirtParticles[Random.Range(0, dirtParticles.Length-1)];
        Transform newParticle = Instantiate(particle);
        int landMark = Random.Range(0,488);
        GetComponent<ARTrackingManager>().mapObjectWithLandmark(newParticle, landMark);
    }
}
