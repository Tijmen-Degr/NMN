using UnityEngine;
using UnityEditor;

public class CircleArrangerWindow : EditorWindow
{
    private float radius = 5f;
    private Vector3 center = Vector3.zero;
    private bool faceCenter = false;

    [MenuItem("Tools/Circle Arranger")]
    private static void ShowWindow()
    {
        GetWindow<CircleArrangerWindow>("Circle Arranger");
    }

    private void OnGUI()
    {
        GUILayout.Label("Arrange Selected Objects in a Circle", EditorStyles.boldLabel);

        radius = EditorGUILayout.FloatField("Radius", radius);
        center = EditorGUILayout.Vector3Field("Center Position", center);
        faceCenter = EditorGUILayout.Toggle("Face Center", faceCenter);

        GUILayout.Space(10);

        if (GUILayout.Button("Arrange"))
        {
            ArrangeObjects();
        }
    }

    private void ArrangeObjects()
    {
        Transform[] objs = Selection.transforms;
        if (objs.Length == 0)
        {
            Debug.LogWarning("No objects selected to arrange.");
            return;
        }

        int count = objs.Length;

        Undo.RecordObjects(objs, "Arrange in Circle");

        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 newPos = center + new Vector3(x, 0, z);
            objs[i].position = newPos;

            if (faceCenter)
            {
                objs[i].LookAt(center);
            }
        }

        Debug.Log("Objects arranged in circle!");
    }
}
