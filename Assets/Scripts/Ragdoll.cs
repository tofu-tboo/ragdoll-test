using UnityEngine;
using System.Collections.Generic;

public class Ragdoll : MonoBehaviour
{
    private bool activated = false;
    List<Collider2D> allColliders = new List<Collider2D>();

    private void Awake()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        allColliders.AddRange(colliders);
    }

    public void UpdateMode()
    {
        //
        Muscle[] muscles = GetComponentsInChildren<Muscle>();
        foreach (Muscle muscle in muscles)
        {
            muscle.musclesActive = activated;
        }
        SetSelfAndChildrenCollisions(activated);
        SetHingeLimit(!activated);
        activated = !activated;

    }

    private void SetHingeLimit(bool toggle)
    {
        foreach (Collider2D collider in allColliders)
        {
            HingeJoint2D hinge = collider.GetComponent<HingeJoint2D>();
            hinge.useLimits = toggle;
        }
    }
    
    private void SetSelfAndChildrenCollisions(bool toggle)
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
                if (!toggle)
                {
                    if ((colliderA.name == "Torso" && colliderB.name.Contains("Arm") && colliderB.name.Contains("D")) || (colliderB.name == "Torso" && colliderA.name.Contains("Arm") && colliderA.name.Contains("D"))) continue;
                    else if ((colliderA.name == "Pelvis" && colliderB.name.Contains("Leg") && colliderB.name.Contains("D")) || (colliderB.name == "Pelvis" && colliderA.name.Contains("Leg") && colliderA.name.Contains("D"))) continue;
                }
                Physics2D.IgnoreCollision(colliderA, colliderB, true);

                // Debug.Log($"Ignored collision between: {colliderA.name} and {colliderB.name}");
            }
        }
    }
}
