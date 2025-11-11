using UnityEngine;

public class BallRigidBodySetter : MonoBehaviour
{
    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 720.0f;
    }
}
