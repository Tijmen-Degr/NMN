using UnityEngine;
using UnityEngine.InputSystem;

public class DanceInput : MonoBehaviour
{
    private PlayerControls controls;
    private Animator anim;

    void Awake()
    {
        controls = new PlayerControls();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Read keyboard state
        bool w = Keyboard.current.wKey.isPressed;
        bool a = Keyboard.current.aKey.isPressed;
        bool s = Keyboard.current.sKey.isPressed;
        bool d = Keyboard.current.dKey.isPressed;

        // ---------- COMBOS ----------
        if (w && a && !s && !d)
        {
            anim.SetTrigger("DanceWA");
            return;
        }
        if (w && s == false && d && !a)
        {
            anim.SetTrigger("DanceWD");
            return;
        }
        if (w && s && !a && !d)
        {
            anim.SetTrigger("DanceWS");
            return;
        }
        if (a && s && !w && !d)
        {
            anim.SetTrigger("DanceAS");
            return;
        }
        if (a && d && !w && !s)
        {
            anim.SetTrigger("DanceAD");
            return;
        }
        if (s && d && !w && !a)
        {
            anim.SetTrigger("DanceSD");
            return;
        }

        // ---------- SINGLE KEY ANIMATIONS ----------
        if (Keyboard.current.wKey.wasPressedThisFrame)
            anim.SetTrigger("DanceW");

        if (Keyboard.current.aKey.wasPressedThisFrame)
            anim.SetTrigger("DanceA");

        if (Keyboard.current.sKey.wasPressedThisFrame)
            anim.SetTrigger("DanceS");

        if (Keyboard.current.dKey.wasPressedThisFrame)
            anim.SetTrigger("DanceD");
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();
}
