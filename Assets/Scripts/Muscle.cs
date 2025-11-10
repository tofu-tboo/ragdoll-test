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
        

        float normalizedRotation = Mathf.Repeat(rb.rotation + 180f, 360f) - 180f;
        // rb.SetRotation(normalizedRotation);
        rb.rotation = normalizedRotation;

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

        // 1. 각도 오차 계산
        float currentAngle = rb.rotation;
        
    
        
        // 동적으로 계산된 closest360Angle을 목표 각도로 사용하여 오차를 계산합니다.
        float angleError = Mathf.DeltaAngle(currentAngle, targetAngle);

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