using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] private Transform followCamera;

    private float largeMapx;
    private float largeMapY;

    private float mediumMapx;
    private float mediumMapy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = new Vector3(Mathf.Clamp(followCamera.position.x, -1.5f, 1.5f), Mathf.Clamp(followCamera.position.y, -1f, 0.2f), Mathf.Clamp(followCamera.position.z, -10, -10)); 
    }
}
