using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public float mouseSensitivity = 500f;
    public float topClamp = -90f;
    public float bottomClamp = 90f;
    
    float xRotation = 0f;
    float yRotation = 0f;
    
    
    
    void Start()
    {
        //Locking the cursor to the middle of the screen and making it invisible
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //getting the mouse inputs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        //Rotation around the x-axis (look up and down)
        xRotation -= mouseY;
        
        //Clamp the rotation
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);
        
        //Rotation around the y-axis (look left and right)
        yRotation += mouseX;
        
        //Apply rotation to our transform
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
