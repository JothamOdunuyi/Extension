
using UnityEngine;

public class MoveSphere : MonoBehaviour
{
    public float speed;
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            rb.velocity = transform.forward * speed;
        }
    }
}