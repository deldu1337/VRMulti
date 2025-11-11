using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LineHandler : MonoBehaviour
{
    [field: SerializeField]
    public Transform parentPosition { private get; set; }

    [field: SerializeField]
    public Transform rotateHandler { private get; set; }

    Rigidbody rg;

    void Awake()
    {
        rg = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rg.MovePosition(parentPosition.position);

        Vector3 dir = rotateHandler.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(dir.normalized);
        rg.MoveRotation(rot);
    }
}