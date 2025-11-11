using UnityEngine;

public class Muscle : MonoBehaviour
{
    private float pBase = 30.0f;
    // ✨ 외부에서 제어되는 추가적인 근육 힘 (이전의 Muscle Strength)
    [Tooltip("외부에서 설정하는 추가적인 힘의 계수. 최종 힘에 더해집니다.")]
    [SerializeField] private float externalMuscleStrength = 10f;

    // ✨ 하중 구조 정의
    [Header("Load Calculation")]
    [Tooltip("이 Rigidbody가 직접적으로 짐을 지탱하는 하위 파츠들의 Muscle 컴포넌트 목록.")]
    [SerializeField] private Muscle[] carriedParts; // Muscle 컴포넌트를 참조하여 하위 부담 Mass를 가져옴
    private Rigidbody2D anchorPart;
    
    [Tooltip("관절의 지렛대 효과 계수 (e.g., Leg Down=1.4, Torso=1.0).")]
    [SerializeField] private float leverageFactor = 1.0f;
    
    [Tooltip("Torso의 최종 Factor를 10.0으로 맞추기 위한 기준 상수 (K=10.0).")]
    [SerializeField] private float baseTorqueFactorK = 10.0f;

    // ✨ 목표 자세 (세계 좌표 기준 Z축 각도)
    [Tooltip("이 Rigidbody가 유지하려는 목표 월드 Z축 각도입니다.")]
    [SerializeField] private float targetAngle;

    [Tooltip("하중 피전달 계수")]
    [SerializeField] private float loadInfluenceFactor = 1.0f;

    // ✨ 제어 토글
    public bool musclesActive = true;

    // ✨ PID 제어 변수
    [Header("PID Control Gains")]
    [Tooltip("자세 오차를 줄이는 주된 힘 (P-Gain).")]
    [SerializeField] private float pGain = 1.0f;
    
    [Tooltip("떨림을 억제하는 제동력 (D-Gain).")]
    [SerializeField] private float dampingFactor = 5f; 

    // --- 내부 변수 ---
    private Rigidbody2D rb;
    private float totalMassToCarry; // 이 파츠와 하위 파츠 전체가 지탱하는 Mass
    private float finalLoadPer1; // 최종 계산된 Factor
    private bool loadCalculated = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"Rigidbody2D component missing on {gameObject.name}. Muscle requires a Rigidbody2D.");
            enabled = false;
            return;
        }
        anchorPart = GetComponent<HingeJoint2D>().connectedBody;
    }
    
    /// <summary>
    /// 이 파츠와 하위 파츠 전체가 지탱해야 하는 총 Mass를 반환합니다.
    /// </summary>
    public float GetTotalMassLoad()
    {
        // Awake에서 이미 계산된 최종 Mass Load를 반환
        return totalMassToCarry;
    }

    /// <summary>
    /// Factor = [(자신 Mass + 누적 하중) * Leverage] / 자신 Mass * K
    /// </summary>
    public void CalculateTotalLoadAndFactor()
    {
        // 1. 자신이 지탱하는 총 질량 계산 (자신 Mass + 누적 하중)
        totalMassToCarry = rb.mass;
        
        // 하위 파츠 리스트를 순회하여 그들이 지탱하는 Mass를 더합니다.
        // 이 파츠는 하위 파츠의 Mass 전체를 지탱해야 합니다.
        if (carriedParts != null)
        {
            foreach (Muscle partMuscle in carriedParts)
            {
                if (partMuscle != null)
                {
                    // totalMassToCarry += partMuscle.GetComponent<Rigidbody2D>().mass;
                    // 하위 파츠가 최종적으로 지탱하는 총 Mass를 가져와 더합니다.
                    totalMassToCarry += partMuscle.GetTotalMassLoad() * loadInfluenceFactor;
                }
            }
        }
        
        // 2. Final Factor 계산
        if (rb.mass > 0)
        {
            float ratio = (totalMassToCarry * leverageFactor) / rb.mass;
            // 최종 Factor = Ratio * K
            finalLoadPer1 = baseTorqueFactorK * ratio;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has zero mass. Setting factor to 0 to prevent division by zero.");
            finalLoadPer1 = 0f;
            return;
        }

        loadCalculated = true;
        // 디버그 출력
        Debug.Log($"[Muscle Factor] {gameObject.name}: Total Load = {totalMassToCarry:F2}, Final Factor = {finalLoadPer1:F2}");
    }

    void FixedUpdate()
    {
        // float normalizedRotation = Mathf.Repeat(rb.rotation, 360f);
        // rb.SetRotation(normalizedRotation);
        // rb.rotation = normalizedRotation;
    //    float currentRelativeAngle;

    //     if (anchorPart != null)
    //     {
    //         // 1. 상대 각도 계산
    //         currentRelativeAngle = Mathf.DeltaAngle(anchorPart.rotation, rb.rotation);
            
    //         // 2. 강제 정규화 (rb.rotation에 덮어쓰기)
    //         // Hinge Joint Limit의 상대 각도와 일치하는 방식으로 Rigidbody의 월드 회전을 강제 수정합니다.
    //         // anchorPart의 회전이 0이라고 가정할 때의 누적 없는 회전값이 됩니다.
    //         rb.rotation = currentRelativeAngle; 
    //     }
    //     else
    //     {
    //         // 최상위 파츠 (월드 회전 기준)
    //         currentRelativeAngle = Mathf.Repeat(rb.rotation, 360f);
            
    //         // 2. 강제 정규화 (rb.rotation에 덮어쓰기)
    //         // 월드 회전을 -180 ~ 180으로 강제 리셋
    //         rb.rotation = currentRelativeAngle;
    //     }

        Debug.Log(rb.name + ": " + rb.rotation);
        if (!musclesActive)
        {
            return;
        }
        ApplyTorqueToMaintainPose();
    }

    private void ApplyTorqueToMaintainPose()
    {
        if (!loadCalculated) return;

        // 1. 각도 오차 계산 및 상대 각도 계산
        float currentRelativeAngle;

        if (anchorPart != null)
        {
            // 💡 상대 각도 계산: Hinge Joint Limit처럼 앵커 파트에 대한 상대 각도를 얻습니다.
            // 이 값은 이미 -180 ~ 180 범위에 해당합니다.
            currentRelativeAngle = Mathf.DeltaAngle(anchorPart.rotation, rb.rotation);
        }
        else
        {
            // 최상위 파츠 (앵커 없음): 월드 각도를 정규화하여 사용
            currentRelativeAngle = Mathf.Repeat(rb.rotation + 180f, 360f) - 180f;
        }

        // 오차는 (현재 상대 각도)와 (Inspector에 설정된 상대 목표 각도)의 차이입니다.
        float angleError = Mathf.DeltaAngle(currentRelativeAngle, targetAngle);

        float proportionalVelocity = angleError * pGain * pBase;
        
        // D-Term: 현재 각속도에 반대 방향으로 작용하여 떨림을 억제하는 감쇠 각속도
        // (-rb.angularVelocity * dampingFactor)
        float dampingVelocity = -rb.angularVelocity * dampingFactor;
        
        // PD Control Output: 순수한 PD 제어에 의한 목표 각속도
        float pdTargetVelocity = proportionalVelocity + dampingVelocity;

        // 2. 하중 기반 및 외부 힘 기반 최종 속도 강도 결정
        // finalTorqueStrength는 PD 제어 결과를 스케일링하여 하중과 근육 강도를 반영합니다.
        // 하중이 높을수록, 근육 힘이 강할수록 목표 속도 크기를 높여줍니다.
        float finalTorqueStrength = finalLoadPer1 + externalMuscleStrength;

        // 3. 최종 목표 각속도 계산 및 클램프
        float finalTargetAngularVelocity = pdTargetVelocity * finalTorqueStrength * rb.mass;

        // 최종 각속도 적용 (강체에 직접 설정)
        // 이 방식은 관성을 무시하고 매 FixedUpdate마다 Rigidbody의 각속도를 재정의합니다.
        rb.angularVelocity = finalTargetAngularVelocity;
    }

    // 외부 스크립트에서 토글 상태를 변경하는 Public 메서드
    public void SetMusclesActive(bool isActive)
    {
        musclesActive = isActive;
        if (!isActive && rb != null)
        {
            rb.angularVelocity = 0f;
        }
    }

    public Muscle[] GetCarriedParts() => carriedParts;
    public void SetPBase(float value)
    {
        if (value > 0)
            pBase = value;
    }
}