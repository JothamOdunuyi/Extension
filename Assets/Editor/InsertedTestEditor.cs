using UnityEngine;
using UnityEditor;
using System.IO;

public class InsertedTestEditor : EditorWindow
{
    [MenuItem("OpenAI/Real Insert Test Script")]
    static void InsertTestScript()
    {
        // Create a new script asset
        string scriptName = "InsertedTest2";
        string scriptPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + scriptName + ".cs");
        //string scriptPath = Path.Combine("Assets/", $"{scriptName}.cs");
        string scriptContents = @"
using UnityEngine;

public class " + scriptName + @" : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}";

        File.WriteAllText(scriptPath, scriptContents);
        AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        // Wait for the script to compile
        EditorApplication.update += WaitForScriptCompilation;


        void WaitForScriptCompilation()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.Log("Loading");
                return;
            }

            Debug.Log("Finished");
            // Remove the update listener
            EditorApplication.update -= WaitForScriptCompilation;
            // Add the component to the selected object
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject != null)
            {
                MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/" + scriptName +".cs");
                Debug.Log("Selected not null attempting " + scriptAsset.GetClass());
                selectedObject.AddComponent(scriptAsset.GetClass());
            }
            else
            {
                Debug.Log("Null selected");
            }
        }
    }
}
