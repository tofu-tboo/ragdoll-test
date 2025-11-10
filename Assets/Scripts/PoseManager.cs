using UnityEngine;
using System.Collections.Generic;

public class PoseManager : MonoBehaviour
{
    [SerializeField] private Ragdoll ragdoll;


    private void Awake()
    {
        ragdoll.UpdateMode();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ragdoll.UpdateMode();
        }
    }
}
