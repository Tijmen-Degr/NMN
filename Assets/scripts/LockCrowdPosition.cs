using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ControlledRootMotion : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorMove()
    {
        // Apply ONLY the delta movement
        Vector3 delta = animator.deltaPosition;

        transform.position += delta;

        // Ignore root motion rotation
        // (you control rotation separately)
    }
}