using UnityEngine;
using System.Collections.Generic;

public class PoseManager : MonoBehaviour
{
    [SerializeField] private Rigidbody2D ragdoll;

    private bool activated = false;

    private void Awake()
    {
        UpdateMode();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            UpdateMode();
        }
    }

    private void UpdateMode()
    {
        Muscle[] muscles = ragdoll.GetComponentsInChildren<Muscle>();
        activated = !activated;
        foreach (Muscle muscle in muscles)
        {
            muscle.musclesActive = !activated;
        }
    }
}
