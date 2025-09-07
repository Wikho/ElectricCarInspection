// Assets/Editor/CarRevealControllerEditor.cs
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarRevealController))]
public class CarRevealControllerEditor : Editor
{
    private SerializedProperty carRenderersProp;
    private SerializedProperty transparentMaterialProp;
    private SerializedProperty transparentGlassMaterialProp;
    private SerializedProperty glassKeywordProp;
    private SerializedProperty componentGroupsProp;

    private int selectedComponentIndex;

    private void OnEnable()
    {
        carRenderersProp = serializedObject.FindProperty("carRenderers");
        transparentMaterialProp = serializedObject.FindProperty("transparentMaterial");
        transparentGlassMaterialProp = serializedObject.FindProperty("transparentGlassMaterial");
        glassKeywordProp = serializedObject.FindProperty("glassKeyword");
        componentGroupsProp = serializedObject.FindProperty("componentGroups");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Car Reveal Controller", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Assign renderers & materials, then use the buttons below (Play Mode) to switch modes and show components.",
            MessageType.Info);

        // Default fields
        EditorGUILayout.PropertyField(carRenderersProp, true);
        EditorGUILayout.Space(2);
        EditorGUILayout.PropertyField(transparentMaterialProp);
        EditorGUILayout.PropertyField(transparentGlassMaterialProp);
        EditorGUILayout.PropertyField(glassKeywordProp);
        EditorGUILayout.Space(2);
        EditorGUILayout.PropertyField(componentGroupsProp, true);

        EditorGUILayout.Space(8);
        DrawRuntimeControls();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRuntimeControls()
    {
        var ctrl = (CarRevealController)target;

        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use the buttons.", MessageType.Warning);
            return;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Normal", GUILayout.Height(28)))
            {
                Undo.RecordObject(ctrl, "CarReveal Normal");
                ctrl.SetMode_Normal();
            }
            if (GUILayout.Button("All (See‑through)", GUILayout.Height(28)))
            {
                Undo.RecordObject(ctrl, "CarReveal All");
                ctrl.SetMode_All();
            }
            if (GUILayout.Button("None", GUILayout.Height(28)))
            {
                Undo.RecordObject(ctrl, "CarReveal None");
                ctrl.SetNone();
            }
        }

        EditorGUILayout.Space(6);

        // Build enum popup from ComponentType
        var names = System.Enum.GetNames(typeof(CarRevealController.ComponentType));
        var values = (CarRevealController.ComponentType[])System.Enum.GetValues(typeof(CarRevealController.ComponentType));

        // current index defaults to selectedComponentIndex; user picks then "Show Component"
        selectedComponentIndex = EditorGUILayout.Popup("Component", selectedComponentIndex, names);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Show Component", GUILayout.Height(24)))
            {
                ctrl.SelectComponent(values[selectedComponentIndex]);
                ctrl.SetMode_All();
            }
            if (GUILayout.Button("Hide Component", GUILayout.Height(24)))
            {
                ctrl.SelectComponent(CarRevealController.ComponentType.None);
            }
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Quick Select", EditorStyles.miniBoldLabel);

        // Quick grid buttons (skip None at index 0)
        int cols = 3;
        int col = 0;
        EditorGUILayout.BeginHorizontal();
        for (int i = 1; i < names.Length; i++)
        {
            if (GUILayout.Button(names[i], GUILayout.Height(24)))
            {
                ctrl.SelectComponent((CarRevealController.ComponentType)values[i]);
                ctrl.SetMode_All();
                selectedComponentIndex = i;
            }
            col++;
            if (col >= cols && i < names.Length - 1)
            {
                col = 0;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
