using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.Experimental.GraphView;
using System.Runtime.InteropServices;
using UnityEditor.PackageManager.UI;
using UnityEngine.Networking;
using static APIRequest;

/*public class RunForMe : MonoBehaviour
{
    public ChatGPT chatGPT;
    private void Start()
    {
        Debug.Log("ccool");
        chatGPT = AssetDatabase.LoadAssetAtPath<ChatGPT>("Assets/Editor/ChatGPT.cs");
        print(chatGPT);
    }
    public AddScriptEditorWindow window
    {
        get { return window; }
        set {
            Debug.Log("CAlled to set");
            StartCoroutine(chatGPT.MakeRequest()); }
    }
}*/

public class AddScriptEditorWindow : EditorWindow
{
    static string className = "CoolGuyScript";
    const string key = "PENDING_SCRIPT_KEY";
    public bool run;

    
    public const string prompt = "How are you bro";
    public const string apiKey = "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";

    private const string strin = "Reply as if you are in a game lobby";

    // The engine you want to use (keep in mind that it has to be the exact name of the engine)
    private string model = "text-davinci-003";
    public float temperature = 0.5f;
    public int maxTokens = 200;

    MonoScript scriptGenerated;

    [MenuItem("Open AI/Add Script")]
    static void Init()
    {
        AddScriptEditorWindow window = (AddScriptEditorWindow)EditorWindow.GetWindow(typeof(AddScriptEditorWindow));
        window.Show();
    }

    void OnGUI()
    {
      
        className = EditorGUILayout.TextField("Class Name: ", className);
      
        if (GUILayout.Button("Compile"))
        {
            /*RunForMe t = new RunForMe();
            t.window = this;*/
            //AddNewScript(EditorGUILayout.TextField("Class Name: ", className));
            Request();
        }
    }

    void Request()
    {
        // Create a JSON object with the necessary parameters, Unity's JsonUtility.ToJson failed me
        string toJson = "{\"prompt\":\"" + strin + "\",\"model\":\"" + model + "\",\"temperature\":" + temperature + ",\"max_tokens\":" + maxTokens + "}";
        //newBody = new requestBody(prompt, model, temperature.ToString(), maxTokens.ToString());


        Debug.Log(toJson);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(toJson);

        // Create a new UnityWebRequest
        var request = new UnityWebRequest("https://api.openai.com/v1/completions", "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        request.SendWebRequest();
        Debug.Log("Set request");

        while (!request.isDone)
        {
            // Wait
            Debug.Log("Waiting");
        }
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Deserialize the JSON response
            var response = JsonUtility.FromJson<Response>(request.downloadHandler.text);
            string textReponse = response.choices[0].text.TrimStart().TrimEnd();
           /* string className = ExtractClassName(textReponse);
            Debug.Log(textReponse);
            Debug.Log("CLASS NAME " + className);*/

            //textmesh.text = response.choices[0].text.TrimStart().TrimEnd().ToString();
            Debug.Log("response.choices[0].text.TrimStart().TrimEnd().ToString()");
        }


    }

    [System.Serializable]
    private class Response
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public string text;
    }

    void AddNewScript(string className)
    {
        
        string scriptContents = @"
using UnityEngine;

public class " + className + @" : MonoBehaviour
{
    // Insert code here
}";

        // Create a new script file
        string scriptPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + className + ".cs");
        System.IO.File.WriteAllText(scriptPath, scriptContents);

        // Wait for compilation to finish
        Debug.Log("Generated script to path");
        ImportScript(scriptPath);
    }

    internal static void ImportScript(string scriptPath)
    {
        EditorPrefs.SetString(key, className);
        AssetDatabase.ImportAsset(scriptPath);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (!EditorPrefs.HasKey(key))
            return;


        Debug.Log("Scripts reloaded!");

        // Add the script as a component to the selected game object
        if (Selection.activeGameObject != null)
        {
            EditorPrefs.DeleteKey(key);
            Debug.Log("Attempting to add component ");
            Type type = Type.GetType(className + ",Assembly-CSharp");
            Selection.activeGameObject.AddComponent(type);
            Debug.Log("Added component!");
            //AddComponent();
            // Selection.activeGameObject.AddComponent(typeof(AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/" + className + ".cs")));
        }
        else
        {
            Debug.LogError("No game object selected.");
        }
    }

    private void AddComponent()
    {
        var selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogError("Please select a game object to add the component to.");
            return;
        }

        var component = selectedObject.AddComponent(System.Type.GetType(className));
        if (component == null)
        {
            Debug.LogError($"Failed to add {className} component to the selected object.");
        }
        else
        {
            Debug.Log($"Added {className} component to {selectedObject.name}.");
        }
    }
}
