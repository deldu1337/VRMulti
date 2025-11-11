//using System.Collections;
//using UnityEngine;

//public class ImpulseCue : MonoBehaviour
//{
//    Rigidbody rb;

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            Debug.Log("Input Space");
//            StartCoroutine(Shoot());
//        }
//    }

//    IEnumerator Shoot()
//    {
//        rb.AddForce(transform.up * 10, ForceMode.Impulse);
//        yield return new WaitForSeconds(0.1f);
//        rb.linearVelocity = Vector3.zero;
//        transform.localPosition = Vector3.zero;
//        yield break;
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class ImpulseCue : MonoBehaviour
{
    [Header("Charge/Gauge Settings")]
    [SerializeField, Min(0f)] private float maxImpulse = 10f;     // 최대 힘 (요청: 10)
    [SerializeField, Min(0.1f)] private float chargeTime = 1.5f;  // 최대치까지 차는 데 걸리는 시간(초)
    [SerializeField]
    private AnimationCurve chargeCurve =          // 충전 곡선(초반 느리고 후반 빠르게)
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI")]
    [SerializeField] private Image gaugeFill;      // Fill Image (fillAmount 0~1 사용)
    [SerializeField] private Text gaugeText;       // 선택: 수치 표시(Text가 없으면 비워둬도 됨)

    [Header("Reset Settings (Optional)")]
    [SerializeField] private bool autoResetAfterShot = true;
    [SerializeField, Min(0f)] private float resetDelay = 0.15f;
    [SerializeField] private Vector3 localResetPosition = Vector3.zero;

    private Rigidbody rb;
    private bool isCharging = false;
    private float charge01 = 0f;          // 0~1 (게이지 비율)
    private float chargeStartTime = 0f;   // 충전 시작 시각
    private float currentImpulse = 0f;    // 실제 적용할 임펄스 값

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        UpdateUI(0f, 0f);
    }

    void Update()
    {
        // 1) 스페이스바를 누르는 순간: 충전 시작
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            chargeStartTime = Time.time;
        }

        // 2) 누르고 있는 동안: 게이지 상승
        if (isCharging && Input.GetKey(KeyCode.Space))
        {
            // 0~1로 정상화된 시간 비율
            float t = Mathf.Clamp01((Time.time - chargeStartTime) / chargeTime);

            // 곡선을 타서 부드럽게(초반 느리고 후반 빠르게 등)
            float curved = Mathf.Clamp01(chargeCurve.Evaluate(t));

            // 최종 임펄스 (0 ~ maxImpulse)
            currentImpulse = curved * maxImpulse;

            // 게이지는 곡선 적용값으로 표시
            charge01 = curved;

            UpdateUI(charge01, currentImpulse);
        }

        // 3) 스페이스바를 떼는 순간: 발사
        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            isCharging = false;
            Shoot(currentImpulse);
            ResetChargeUI();

            if (autoResetAfterShot)
                StartCoroutine(ResetAfterDelay());
        }
    }

    private void Shoot(float impulse)
    {
        if (impulse <= 0f) return;

        // 큐대의 "어느 방향"으로 힘을 줄지 결정 (여기서는 큐대의 위쪽 방향 기준)
        // 필요에 따라 transform.forward/transform.right 등으로 바꾸세요.
        Vector3 dir = transform.up.normalized;

        //rb.AddForce(dir * impulse, ForceMode.Impulse);
        rb.AddForceAtPosition(dir * impulse, dir, ForceMode.Impulse);
        // Debug.Log($"Shot with impulse: {impulse:F2}");
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        // 발사 후 제자리/정지
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = localResetPosition;
    }

    private void ResetChargeUI()
    {
        charge01 = 0f;
        currentImpulse = 0f;
        UpdateUI(0f, 0f);
    }

    private void UpdateUI(float ratio01, float impulse)
    {
        if (gaugeFill) gaugeFill.fillAmount = ratio01;
        if (gaugeText) gaugeText.text = $"{impulse:0.0} / {maxImpulse:0.0}";
    }

    // --- 선택: UI 버튼으로도 발사하고 싶을 때 사용할 메서드 ---
    public void FireNowFullPower()   // 예: UI 버튼에 연결(최대치로 즉시 발사)
    {
        Shoot(maxImpulse);
        ResetChargeUI();
        if (autoResetAfterShot) StartCoroutine(ResetAfterDelay());
    }
}
