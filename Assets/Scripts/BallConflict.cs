//using UnityEngine;

//public class BallConflict : MonoBehaviour
//{
//    // "ball" 레이어 번호 캐시
//    private int ballLayer;

//    void Awake()
//    {
//        ballLayer = LayerMask.NameToLayer("Ball");
//        if (ballLayer == -1)
//            Debug.LogWarning("레이어 'ball'이 존재하지 않습니다. Project Settings > Tags and Layers에서 추가하세요.");
//    }

//    // 3D 일반용: v와 평행하지 않은 축을 골라 Cross로 수직 벡터 생성
//    static Vector3 Perpendicular3D(Vector3 v)
//    {
//        if (v.sqrMagnitude <= Mathf.Epsilon) return Vector3.right;
//        Vector3 axis = (Mathf.Abs(v.y) < 0.99f) ? Vector3.up : Vector3.right;
//        return Vector3.Cross(v, axis).normalized;
//    }

//    (Vector3, Vector3) calculateBall2BallCollision(Vector3 v1, Vector3 v2, Vector3 c1, Vector3 c2, float e = 1f)
//    {
//        Vector3 basisX = (c2 - c1).normalized;
//        Vector3 basisY = Perpendicular3D(basisX);
//        float sin1, sin2, cos1, cos2;
//        if(v1.magnitude < 0.0001f)
//        {
//            sin1 = 0;
//            cos1 = 1;
//        }
//        else
//        {
//            cos1 = Vector3.Dot(v1, basisX) / v1.magnitude;
//            Vector3 cross = Vector3.Cross(v1, basisX);
//            if(cross.z > 0)
//            {
//                sin1 = cross.magnitude / v1.magnitude;
//            }
//            else
//            {
//                sin1 = -cross.magnitude / v1.magnitude;
//            }
//        }

//        if(v2.magnitude < 0.0001f)
//        {
//            sin2 = 0;
//            cos2 = 1;
//        }
//        else
//        {
//            cos2 = Vector3.Dot(v2, basisX) / v2.magnitude;
//            Vector3 cross = Vector3.Cross(v2, basisX);
//            if (cross.z > 0)
//            {
//                sin2 = cross.magnitude / v2.magnitude;
//            }
//            else
//            {
//                sin2 = -cross.magnitude / v2.magnitude;
//            }
//        }
//        Vector3 u1, u2;
//        u1 = ((1 - e) * v1.magnitude * cos1 + (1 + e) * v2.magnitude * cos2) / 2 * basisX - v1.magnitude * sin1 * basisY;
//        u2 = ((1 + e) * v1.magnitude * cos1 + (1 - e) * v2.magnitude * cos2) / 2 * basisX - v2.magnitude * sin2 * basisY;

//        return (u1, u2);
//    }

//    (Vector3, Vector3) calculateBall2BallCollisionSimple(Vector3 v1, Vector3 v2)
//    {
//        return (v1, v2);
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        // 레이어로 필터링
//        if (collision.gameObject.layer != ballLayer) return;

//        Rigidbody ball1RB = gameObject.GetComponent<Rigidbody>();
//        Rigidbody ball2RB = collision.gameObject.GetComponent<Rigidbody>();
//        Vector3 v1 = gameObject.GetComponent<BallStart>().velocity;
//        Vector3 v2 = collision.gameObject.GetComponent<BallStart>().velocity;
//        (ball1RB.linearVelocity, ball2RB.linearVelocity) = calculateBall2BallCollision(v1, v2, ball1RB.position, ball2RB.position);
//    }
//}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallConflict : MonoBehaviour
{
    [Range(0f, 1f)]
    public float restitution = 1f; // 탄성계수 e (1=완전탄성, 0=비탄성)

    private int ballLayer;

    void Awake()
    {
        ballLayer = LayerMask.NameToLayer("Ball");
        if (ballLayer == -1)
            Debug.LogWarning("레이어 'Ball'이 없습니다. Project Settings > Tags and Layers에서 추가하세요.");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 다른 공과만 처리
        if (collision.gameObject.layer != ballLayer) return;

        Rigidbody rb1 = GetComponent<Rigidbody>();
        Rigidbody rb2 = collision.rigidbody;
        if (!rb1 || !rb2) return;

        // 현재 속도(PhysX가 사용하는 값 사용! BallStart.velocity 말고 rb.velocity)
        Vector3 v1 = rb1.linearVelocity;
        Vector3 v2 = rb2.linearVelocity;

        // 두 공의 중심 방향을 법선으로 사용
        Vector3 n = (rb1.worldCenterOfMass - rb2.worldCenterOfMass);
        float nSqr = n.sqrMagnitude;
        if (nSqr < 1e-8f) return; // 거의 같은 위치면 스킵
        n /= Mathf.Sqrt(nSqr);    // normalize

        // 상대 속도의 법선 성분
        Vector3 rv = v1 - v2;
        float velAlongNormal = Vector3.Dot(rv, n);

        // 이미 서로 멀어지는 중이면(분리 중) 처리하지 않음
        if (velAlongNormal >= 0f) return;

        // 유효 질량(키네마틱이면 0으로 취급)
        float invMass1 = rb1.isKinematic ? 0f : 1f / rb1.mass;
        float invMass2 = rb2.isKinematic ? 0f : 1f / rb2.mass;
        float invMassSum = invMass1 + invMass2;
        if (invMassSum <= 0f) return;

        // 충돌 임펄스 (탄성 반영)
        float e = Mathf.Clamp01(restitution);
        float j = -(1f + e) * velAlongNormal / invMassSum;

        Vector3 impulse = j * n;

        // 속도 갱신
        v1 += impulse * invMass1;
        v2 -= impulse * invMass2;

        rb1.linearVelocity = v1;
        rb2.linearVelocity = v2;

        // (선택) 2D 테이블 게임이면 Z 고정 & 회전 X/Y 고정 추천
        // var c1 = rb1.constraints | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        // var c2 = rb2.constraints | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        // rb1.constraints = c1; rb2.constraints = c2;
    }
}
