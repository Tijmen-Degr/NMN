using UnityEngine;

public class UnifiedEmissiveController : MonoBehaviour
{
    [Header("Emission Settings")]
    public float intensity = 2f;

    [Header("Random Mode Settings")]
    public float switchInterval = 3f;
    public float minSpeed = 0.5f;
    public float maxSpeed = 3f;

    private Material mat;
    private float timer;
    private float currentSpeed;

    private enum Mode { SmoothRedBlue, FlashRedBlue, FlashWhiteOff }
    private Mode currentMode;

    // State variables
    private bool isOn = true; // for flashing modes
    private float t;          // for smooth mode

    void Awake()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("UnifiedEmissiveController requires a Renderer!", this);
            enabled = false;
            return;
        }

        mat = rend.material;
        if (mat == null)
        {
            Debug.LogError("Renderer has no material assigned!", this);
            enabled = false;
            return;
        }

        mat.EnableKeyword("_EMISSION");

        // Pick the first random mode
        PickRandomMode();
    }

    void Update()
    {
        if (mat == null) return;

        // Update timer for mode switching
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0f;
            PickRandomMode();
        }

        // Update the material based on current mode
        switch (currentMode)
        {
            case Mode.SmoothRedBlue:
                t = Mathf.Abs(Mathf.Repeat(Time.time * currentSpeed, 2f) - 1f);
                mat.SetColor("_EmissionColor", Color.Lerp(Color.red, Color.blue, t) * intensity);
                break;

            case Mode.FlashRedBlue:
                UpdateFlash(Color.red, Color.blue);
                break;

            case Mode.FlashWhiteOff:
                UpdateFlash(Color.white, Color.black);
                break;
        }
    }

    private void PickRandomMode()
    {
        currentMode = (Mode)Random.Range(0, 3);
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        isOn = true; // reset flash state
    }

    private void UpdateFlash(Color color1, Color color2)
    {
        if (currentSpeed <= 0f) return;

        t += Time.deltaTime * currentSpeed;
        if (t >= 1f)
        {
            t = 0f;
            isOn = !isOn;
        }

        mat.SetColor("_EmissionColor", (isOn ? color1 : color2) * intensity);
    }
}
