using UnityEngine;

/// <summary>
/// 공-공 충돌 전용 컴포넌트.
/// - 같은 "Ball" 레이어의 다른 구체와 충돌 시, 법선 임펄스로 속도 보정
/// - (옵션) 접선 감쇠로 슬라이딩 마찰 느낌 보강
/// - 중복 계산 방지(InstanceID 가드), 약간의 관통 보정 포함
/// 붙이는 위치: 공 프리팹(각 Ball 오브젝트)
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BallVsBallCollision : MonoBehaviour
{
    [Header("Physics Params")]
    [Range(0f, 1f)] public float restitution = 0.95f;   // 탄성계수 e (1=완전탄성)
    [Range(0f, 1f)] public float tangentKeep = 1.00f;   // 접선 성분 유지 비율(1=감쇠없음, 0=완전소멸)
    [Tooltip("관통 보정 강도(0~1)")] public float penetrationCorrection = 0.8f;
    [Tooltip("관통 허용 슬롭(미만이면 무시)")] public float penetrationSlop = 0.0005f;

    [Header("Filter")]
    public string ballLayerName = "Ball";

    Rigidbody rb;
    SphereCollider sphere;
    int ballLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();
        ballLayer = LayerMask.NameToLayer(ballLayerName);
        if (ballLayer == -1)
            Debug.LogWarning($"레이어 '{ballLayerName}'을(를) 찾을 수 없습니다. Project Settings > Tags and Layers에서 추가하세요.");
    }

    void OnCollisionEnter(Collision collision)
    {
        // 같은 Ball 레이어만 처리
        if (collision.gameObject.layer != ballLayer) return;

        Rigidbody rbOther = collision.rigidbody;
        if (!rbOther || rbOther == rb) return;

        // 중복 계산 방지: 두 공 중 InstanceID가 작은 쪽만 계산
        if (rb.GetInstanceID() > rbOther.GetInstanceID()) return;

        // 중심/반지름
        Vector3 p1 = rb.worldCenterOfMass;
        Vector3 p2 = rbOther.worldCenterOfMass;

        float r1 = GetRadius(rb);
        float r2 = GetRadius(rbOther);

        Vector3 n = (p1 - p2);
        float dist = n.magnitude;
        if (dist < 1e-8f) return;
        n /= dist; // 충돌 법선(다른 공->내 공 방향)

        // 상대속도
        Vector3 v1 = GetVel(rb);
        Vector3 v2 = GetVel(rbOther);
        Vector3 rv = v1 - v2;

        // 법선 속도 성분
        float velAlongNormal = Vector3.Dot(rv, n);
        // 서로 멀어지는 중이면 스킵
        if (velAlongNormal >= 0f) return;

        // 질량 역수
        float invM1 = rb.isKinematic ? 0f : 1f / rb.mass;
        float invM2 = rbOther.isKinematic ? 0f : 1f / rbOther.mass;
        float invMSum = invM1 + invM2;
        if (invMSum <= 0f) return;

        // ---- (1) 법선 임펄스(탄성 e) ----
        float e = Mathf.Clamp01(restitution);
        float j = -(1f + e) * velAlongNormal / invMSum;
        Vector3 impulseN = j * n;

        v1 += impulseN * invM1;
        v2 -= impulseN * invM2;

        // ---- (2) 접선 감쇠(옵션) ----
        // 접선 성분 = 전체 - 법선 성분
        Vector3 vRelAfter = v1 - v2;
        Vector3 vRelAfterN = Vector3.Dot(vRelAfter, n) * n;
        Vector3 vRelAfterT = vRelAfter - vRelAfterN;       // 충돌면 접선
        if (vRelAfterT.sqrMagnitude > 0f)
        {
            float keep = Mathf.Clamp01(tangentKeep);
            Vector3 vRelAfterT_damped = vRelAfterT * keep;
            // 상대 접선 성분 변화량을 두 물체에 반반 나눠 적용(간단 모델)
            Vector3 dvT = vRelAfterT_damped - vRelAfterT;
            v1 += dvT * (invM1 / invMSum);
            v2 -= dvT * (invM2 / invMSum);
        }

        // 속도 적용(프로젝트가 linearVelocity를 쓰면 거기에도 반영)
        SetVel(rb, v1);
        SetVel(rbOther, v2);

        // ---- (3) 관통 보정 ----
        float penetration = (r1 + r2) - dist;
        if (penetration > penetrationSlop)
        {
            float factor = penetrationCorrection * (penetration - penetrationSlop) / invMSum;
            Vector3 correction = factor * n;
            if (!rb.isKinematic) rb.position += correction * invM1;
            if (!rbOther.isKinematic) rbOther.position -= correction * invM2;
        }
    }

    // ---- helpers ----

    static float GetRadius(Rigidbody r)
    {
        var sc = r.GetComponent<SphereCollider>();
        if (!sc) return 0.03f;
        float maxScale = Mathf.Max(r.transform.lossyScale.x, r.transform.lossyScale.y, r.transform.lossyScale.z);
        return Mathf.Abs(sc.radius) * maxScale;
    }

    // 프로젝트에 linearVelocity가 있으면 사용, 없으면 velocity 사용
    static Vector3 GetVel(Rigidbody r)
    {
        var p = typeof(Rigidbody).GetProperty("linearVelocity");
        if (p != null) { try { return (Vector3)p.GetValue(r, null); } catch { } }
        return r.linearVelocity;
    }

    static void SetVel(Rigidbody r, Vector3 v)
    {
        var p = typeof(Rigidbody).GetProperty("linearVelocity");
        if (p != null)
        {
            try { p.SetValue(r, v, null); return; } catch { }
        }
        r.linearVelocity = v;
    }
}
