using UnityEngine;
using UnityEditor;

public class OpenAI : EditorWindow
{
    [MenuItem("OpenAI/Rigidbody Test")]
    static void Init()
    {
        OpenAI window = (OpenAI)EditorWindow.GetWindow(typeof(OpenAI));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Click the button to add a Rigidbody component to the selected game object.");

        if (GUILayout.Button("Add Rigidbody"))
        {
            GameObject selectedGameObject = Selection.activeGameObject;

            if (selectedGameObject != null)
            {
                selectedGameObject.AddComponent<Rigidbody>();
            }
        }
    }
}
