using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.IO;

public class InsertWindow : EditorWindow
{
    [MenuItem("OpenAI/Insert Test Script")]
    static void Init()
    {
        InsertWindow window = (InsertWindow)EditorWindow.GetWindow(typeof(InsertWindow));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Click the button to insert a new C# script called InsertedTest into the selected game object.");

        if (GUILayout.Button("Insert Script"))
        {
            GameObject selectedGameObject = Selection.activeGameObject;

            if (selectedGameObject != null)
            {
                string scriptName = "InsertedTest" + Random.Range(1, 100000);
                string scriptContents = "using UnityEngine;\n\npublic class " + scriptName + " : MonoBehaviour\n{\n    // Start is called before the first frame update\n    void Start()\n    {\n        \n    }\n\n    // Update is called once per frame\n    void Update()\n    {\n        \n    }\n}";

                // Create the new script asset
                string path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + scriptName + ".cs");
                System.IO.File.WriteAllText(path, scriptContents);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                // Add the new script component to the selected game object
                MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/" + scriptName + ".cs");
                if (scriptAsset != null)
                {
                    Debug.Log("Set Component");
                    selectedGameObject.AddComponent(scriptAsset.GetType());
                }
            }
        }

        if (GUILayout.Button("AAt"))
        {
            GameObject selectedGameObject = Selection.activeGameObject;
            MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/InsertedTest89303.cs");
            if (scriptAsset != null && selectedGameObject != null)
            {
                Debug.Log("Set Component " + scriptAsset.GetClass());
                selectedGameObject.AddComponent(scriptAsset.GetClass());
            }
            else
            {
                if (scriptAsset == null)
                    Debug.Log("ASsets");
                if (selectedGameObject == null)
                    Debug.Log("GMAO");
            }
        }
    }
}
