using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float zoomSpeed = 1000f;
    public float speed = 5f;
    private Camera mCam;

    private void Start()
    {
        mCam = GetComponent<Camera>();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            float zoom = scroll * zoomSpeed * Time.deltaTime;
            mCam.orthographicSize -= zoom;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveHorizontal, moveVertical, 0).normalized;
        transform.position += moveDirection * speed * Time.deltaTime;
    }
}
