using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class RagdollSet : MonoBehaviour
{
    [SerializeField] private Muscle[] lastBases;
    [SerializeField] private float pBase = 30.0f; 

    void Awake()
    {
        HashSet<Muscle> finishCalcPool = new();

        foreach (Muscle keyBase in lastBases)
        {
            // 각 루트 파츠(keyBase)로부터 재귀적으로 하향 탐색을 시작합니다.
            // 계산 순서는 '자식 -> 부모'가 되도록 보장합니다.
            TraverseAndCalculateLoad(keyBase, finishCalcPool);
        }

        Destroy(this);
    }

    // 5. 재귀적 하중 계산을 위한 헬퍼 메서드 (Post-order DFS)
    private void TraverseAndCalculateLoad(Muscle currentPart, HashSet<Muscle> calculatedParts)
    {
        // null이거나 이미 계산된 부분이면 즉시 반환하여 무한 루프와 중복 계산을 방지합니다.
        if (currentPart == null || calculatedParts.Contains(currentPart))
        {
            return;
        }

        // 1. 자식 방문 (Recurse): 모든 자식이 먼저 계산되도록 합니다 (Post-order의 핵심).
        if (currentPart.GetCarriedParts().Count() > 0) // Muscle 스크립트에 carriedParts가 있다고 가정
        {
            foreach (Muscle carriedPart in currentPart.GetCarriedParts())
            {
                // 자식 파츠의 계산을 먼저 요청합니다.
                TraverseAndCalculateLoad(carriedPart, calculatedParts);
            }
        }

        // 2. 현재 노드 처리: 모든 자식이 계산을 마쳤으므로, 이제 현재 노드의 Load를 계산합니다.
        // Muscle 스크립트의 CalculateTotalLoadAndFactor()는 이제 안전하게 호출될 수 있습니다.
        currentPart.CalculateTotalLoadAndFactor();
        currentPart.SetPBase(pBase);

        // 3. 계산 완료 풀에 추가
        calculatedParts.Add(currentPart);
        
    }
}
