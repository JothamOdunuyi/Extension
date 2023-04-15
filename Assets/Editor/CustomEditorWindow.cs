using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomEditorWindow : EditorWindow
{
    private string textFieldText = "";
    private string textFieldText2 = "";
    private bool openedWindow;

    private string myTextField = "Text";
    private bool myButtonClicked = false;

    private bool isTextFieldFocused = false;

    [MenuItem("Window/Custom Editor Window")]
    private static void ShowWindow()
    {
        var window = GetWindow<CustomEditorWindow>();
        window.titleContent = new GUIContent("Custom Editor Window");
        window.minSize = new Vector2(200, 100);
        window.maxSize = new Vector2(400, 200);
        window.Show();
        // Set focus on TextField
    }


    private void OnGUI()
    {
        EditorGUILayout.LabelField("TextField with Button", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        textFieldText = EditorGUILayout.TextField(textFieldText);
        if (GUILayout.Button("Button", GUILayout.Width(80f))) // Set desired width for the button
        {
            // Handle button click event
            myButtonClicked = true;
        }
        GUILayout.EndHorizontal();

        if (myButtonClicked)
        {
            EditorGUILayout.LabelField("Button Clicked!", EditorStyles.boldLabel);
        }
    }
}

