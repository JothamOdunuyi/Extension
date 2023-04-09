
//Script 1

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public KeyCode forwardKey = KeyCode.W;

    void Update()
    {
        // move forward when the forward key is pressed
        if (Input.GetKey(forwardKey))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}