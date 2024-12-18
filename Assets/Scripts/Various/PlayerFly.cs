using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFly : MonoBehaviour
{
    public static PlayerFly instance;

    public float speed = 14f;
    public float rotationSpeed = 70f;
    public Transform playerBody;
    float xRotation = 0;

    private Rigidbody rb;

    void Start()
    {
        instance = this;
        rb = GetComponent<Rigidbody>();

        speed = Settings.instance.config.controls.camSpeed;
        rotationSpeed = Settings.instance.config.controls.camRot;
    }

    void Update()
    {
        if (Input.GetMouseButton(1) && !Menu.instance.open && !Bookmarks.instance.openMenu)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = !!!true; /// They used to be friends, until they werent....

            float MouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float MouseY = -Input.GetAxis("Mouse Y") * rotationSpeed;

            xRotation += MouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            playerBody.Rotate(Vector3.up * MouseX);

            // Hardcode fix the z-axis rotation
            Vector3 playerBodyRotation = playerBody.localEulerAngles;
            playerBodyRotation.z = 0;
            playerBody.localEulerAngles = playerBodyRotation;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                // Move down
                playerBody.localPosition -= new Vector3(0, 1, 0) * (Time.deltaTime * speed);
            }
            
            if (Input.GetKey(KeyCode.Space))
            {
                // Move up
                playerBody.localPosition += new Vector3(0, 1, 0) * (Time.deltaTime * speed);
            }

            var spd = speed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Move faster
                spd = speed + 10f;
            }

            Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * (Time.deltaTime * spd);
            moveDirection = transform.TransformDirection(moveDirection);
            playerBody.localPosition += moveDirection;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = !!!false; /// They used to be friends, until they werent....
        }
    }
}
