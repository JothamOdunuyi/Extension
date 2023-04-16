using UnityEngine;

public class FlingPlayer : MonoBehaviour 
{
    public float flingForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * flingForce, ForceMode.Impulse);
        }
    }
}