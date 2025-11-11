using UnityEngine;

public class WallReflect : MonoBehaviour
{
    // "ball" 레이어 번호 캐시
    private int ballLayer;

    void Awake()
    {
        ballLayer = LayerMask.NameToLayer("Ball");
        if (ballLayer == -1)
            Debug.LogWarning("레이어 'ball'이 존재하지 않습니다. Project Settings > Tags and Layers에서 추가하세요.");
    }

    /// <summary>
    /// 벡터 a(진행 속도)를 법선 n에 대해 반사시킨 벡터를 계산
    /// 수식: r = a - 2 * (a·n_hat) * n_hat
    /// 여기서는 n이 정규화되지 않아도 동작하도록 |n|^2로 나눠 처리
    /// </summary>
    Vector3 calculateReflect(Vector3 a, Vector3 n)
    {
        // a를 n 방향으로 투영한 성분(부호 반전 포함)
        // p = -(a·n) / |n|^2 * n
        Vector3 p = -Vector3.Dot(a, n) / n.magnitude * n / n.magnitude;

        // 원벡터 a에 대해 '표면까지의 수직 성분을 두 배' 더해 반사
        // b = a + 2p = a - 2(a·n)/|n|^2 * n
        Vector3 b = a + 2 * p;
        return b;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 레이어로 필터링
        if (collision.gameObject.layer != ballLayer) return;

        // 공의 Rigidbody
        Rigidbody ballRB = collision.gameObject.GetComponent<Rigidbody>();

        // 공의 현재 속도
        Vector3 velocity = collision.gameObject.GetComponent<BallStart>().velocity;

        // 충돌 지점의 법선 벡터.
        // Unity의 접촉 법선은 일반적으로 "벽 표면에서 튕겨나가는 방향"을 가리킴.
        // 이미 바깥쪽을 향하므로 보통은 그대로 쓰면 됨.
        // 아래처럼 -normal을 쓰면 반대 방향으로 반사될 수 있음.
        Vector3 contactNormal = collision.GetContact(0).normal;

        // 반사 속도 계산(법선 방향으로 반사)
        // calculateReflect(velocity, contactNormal) 가 일반적인 형태
        // 현재 코드처럼 음수 법선을 쓰면 방향이 뒤집힐 수 있으니 주의
        // ballRB.velocity = calculateReflect(velocity, contactNormal);

        ballRB.linearVelocity = calculateReflect(velocity, -collision.GetContact(0).normal);
    }
}
