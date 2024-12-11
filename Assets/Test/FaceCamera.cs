using UnityEngine;
public class FaceCamera : MonoBehaviour
{
    public Transform Refernce;
    float xFactor = 0;
    float yFactor = 0;
    Vector3 parentScale;
    Vector3 selfScale;
    Vector3 scale;

    public bool canRotate = true;
    public bool canScale = true;
    private void Start()
    {
        parentScale = Refernce.localScale;
        scale = transform.localScale;
    }
    void Update()
    {
        parentScale = Refernce.localScale;

        if (canScale)
        {
            selfScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
            transform.localScale = selfScale;
        }

        if (canRotate)
            transform.LookAt(Camera.main.transform);
    }

}