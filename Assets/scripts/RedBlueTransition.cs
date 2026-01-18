using UnityEngine;

public class RedBlueEmissionCycle : MonoBehaviour
{
    [Range(0.1f, 5f)]
    public float cycleSpeed = 1f;

    public float intensity = 2f;

    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
       	float t = Mathf.Abs(Mathf.Repeat(Time.time * cycleSpeed, 2f) - 1f);
	Color color = Color.Lerp(Color.red, Color.blue, t);

        mat.SetColor("_EmissionColor", color * intensity);
    }
}
