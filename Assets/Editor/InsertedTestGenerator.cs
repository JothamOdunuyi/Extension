using System.IO;
using UnityEditor;
using UnityEngine;

public class InsertedTestGenerator : EditorWindow
{
    private GameObject selectedGameObject;
    static string scriptName;

    [MenuItem("OpenAI/CREEEEEEEATEEE")]
    public static void CreateScript()
    {
        scriptName = "InsertedTest";
        var scriptPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + scriptName + ".cs");

        var scriptContents = @"
using UnityEngine;

public class " + scriptName + @" : MonoBehaviour
{
    // Insert code here
}";
        // Write the script file
        File.WriteAllText(scriptPath, scriptContents);
        //AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate);

        // Refresh the asset database to make sure the new script appears
        AssetDatabase.Refresh();
    }
/*
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        // Get the selected game object when the scripts are reloaded
        var selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            Debug.Log("Scripts loading , setting struff");
            // Store a reference to the selected game object
            var instance = (InsertedTestGenerator)GetWindow(typeof(InsertedTestGenerator));
            instance.selectedGameObject = selectedObject;
        }
    }

    private void Update()
    {
        // Check if a game object has been selected
        if (selectedGameObject != null)
        {
            MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/InsertedTest.cs");
            // Add the InsertedTest component to the selected game object
            selectedGameObject.AddComponent(scriptAsset.GetType());
            //Debug.Log(scriptAsset.GetType());
            Debug.Log("InsertedTest component added to " + selectedGameObject.name);

            // Reset the selected game object reference
            selectedGameObject = null;
        }
    }*/
}
