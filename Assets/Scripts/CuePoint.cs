using UnityEngine;

public class CuePoint : MonoBehaviour
{
    [SerializeField] GameObject _cuePointer;
    [SerializeField] LayerMask _layerMask;
    private const float length = 3f;
    private RaycastHit hit;
    private bool _checkLayer;
    private Vector3 _initPosition;

    private void Start()
    {
        _initPosition = transform.position;
    }

    void Update()
    {
        _checkLayer = Physics.Raycast(transform.position, transform.forward, out hit, length, _layerMask);
        if(_checkLayer )
            _cuePointer.transform.position = hit.point;

        //if(Input.GetKeyUp(KeyCode.Space))
        //    transform.position = _initPosition;

        Debug.DrawRay(transform.position, transform.forward * length, Color.red);
        
    }
}
