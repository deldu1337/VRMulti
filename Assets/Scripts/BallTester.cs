using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class BallTester : MonoBehaviour
{
    [SerializeField]
    Rigidbody[] ballList;
    Vector3[] ballInitPosition;

    [SerializeField]
    Rigidbody myBall;
    [SerializeField]
    Rigidbody destBall;

    [SerializeField]
    float Pow = 5.0f;

    void Awake()
    {
        ballInitPosition = new Vector3[ballList.Length];

        for (int i = 0; i < ballList.Length; ++i)
        {
            ballInitPosition[i] = ballList[i].position;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            float rad = UnityEngine.Random.value * Mathf.PI * 2f;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            myBall.AddForce(new Vector3(dir.x, 0f, dir.y) * Pow, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (destBall == null)
            {
                Debug.Log("목적지로 설정 된 오브젝트 없음.");
                return;
            }

            myBall.linearVelocity = Vector3.zero;
            myBall.angularVelocity = Vector3.zero;

            destBall.linearVelocity = Vector3.zero;
            destBall.angularVelocity = Vector3.zero;

            Vector3 dir = destBall.position - myBall.position;
            dir.Normalize();
            myBall.AddForce(dir * Pow, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            for (int i = 0; i < ballList.Length; ++i)
            {
                ballList[i].position = ballInitPosition[i];
                ballList[i].linearVelocity = Vector3.zero;
                ballList[i].angularVelocity = Vector3.zero;

                float rad = UnityEngine.Random.value * Mathf.PI * 2f;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                ballList[i].AddForce(new Vector3(dir.x, 0f, dir.y) * Pow, ForceMode.Impulse);
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            for (int i = 0; i < ballList.Length; ++i)
            {
                ballList[i].angularVelocity = Vector3.zero;
                ballList[i].linearVelocity = Vector3.zero;
            }
        }
    }
}
