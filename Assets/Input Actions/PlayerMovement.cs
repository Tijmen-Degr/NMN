using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 10f; // rotation smoothing

    private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Vector3 move = Vector3.zero;

        // Inverted controls
        if (Keyboard.current.aKey.isPressed)
            move += Vector3.right;   // A moves right
        if (Keyboard.current.dKey.isPressed)
            move += Vector3.left;    // D moves left
        if (Keyboard.current.wKey.isPressed)
            move += Vector3.back;    // W backward
        if (Keyboard.current.sKey.isPressed)
            move += Vector3.forward; // S forward

        // Normalize for diagonal movement
        Vector3 moveNormalized = move.normalized * moveSpeed;

        // Apply movement to Rigidbody
        rb.linearVelocity = new Vector3(moveNormalized.x, rb.linearVelocity.y, moveNormalized.z);

        // Walking animation speed
        float speed = new Vector3(moveNormalized.x, 0, moveNormalized.z).magnitude;
        animator.SetFloat("Speed", speed);

        // Rotate in movement direction
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }
    }
}
