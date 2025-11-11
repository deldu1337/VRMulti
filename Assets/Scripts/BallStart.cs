using UnityEngine;

public class BallStart : MonoBehaviour
{
    // 공에 붙어있는 Rigidbody 캐시
    Rigidbody rb;

    // 시작할 때 줄 공 속도
    public float startSpeed = 5f;

    // 시작할 때 줄 선속도(월드 좌표계 기준)
    //public Vector3 startVelocity = new Vector3(10f, 0f, 10f);

    // 현재 속도를 외부에서 확인하고 싶을 때 읽어갈 수 있도록 공개
    public Vector3 velocity;

    private Vector3 initPosition;

    void Start()
    {
        // 같은 게임오브젝트에 붙은 Rigidbody 가져오기
        rb = gameObject.GetComponent<Rigidbody>();

        initPosition = transform.position;
    }

    public void Launch()
    {
        transform.position = initPosition;
        // XZ 평면에서 랜덤 단위 벡터 생성
        Vector2 v2 = Random.insideUnitCircle.normalized;   // 길이 1의 무작위 2D 방향
        Vector3 dir = new Vector3(v2.x, 0f, v2.y);         // XZ 평면 방향

        rb.linearVelocity = dir * startSpeed;
    }

    void Update()
    {
        // 매 프레임 현재 속도를 저장
        velocity = rb.linearVelocity;
    }
}
