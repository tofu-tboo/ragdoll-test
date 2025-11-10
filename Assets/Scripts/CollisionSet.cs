using UnityEngine;
using System.Collections.Generic;
public class CollisionSet : MonoBehaviour
{
    void Awake()
    {
        List<Collider2D> allColliders = new List<Collider2D>();
        // 1. 현재 객체와 모든 자식 객체에서 Collider 2D를 찾습니다.
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        
        // 2. 리스트에 모든 Collider를 추가합니다.
        allColliders.AddRange(colliders);

        // 3. 리스트에 있는 모든 Collider 쌍을 서로 충돌 무시 설정합니다.
        IgnoreSelfAndChildrenCollisions(allColliders);
    }

    private void IgnoreSelfAndChildrenCollisions(List<Collider2D> allColliders)
    {
        // 리스트의 각 Collider(colliderA)를 선택합니다.
        for (int i = 0; i < allColliders.Count; i++)
        {
            Collider2D colliderA = allColliders[i];

            // colliderA 이후의 모든 Collider(colliderB)와 쌍을 이룹니다.
            // (이미 이전 반복에서 처리한 쌍은 건너뛰어 중복 설정을 방지합니다.)
            for (int j = i + 1; j < allColliders.Count; j++)
            {
                Collider2D colliderB = allColliders[j];

                // Physics2D.IgnoreCollision 메서드를 사용하여 두 Collider가 충돌을 무시하도록 설정합니다.
                Physics2D.IgnoreCollision(colliderA, colliderB, true);
                
                // Debug.Log($"Ignored collision between: {colliderA.name} and {colliderB.name}");
            }
        }
    }
}
