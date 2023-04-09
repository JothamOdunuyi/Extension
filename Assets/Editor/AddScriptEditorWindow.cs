using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.Experimental.GraphView;
using System.Runtime.InteropServices;
using UnityEditor.PackageManager.UI;
using UnityEngine.Networking;
using static APIRequest;
using System.Reflection;
using UnityEditor.PackageManager.Requests;
using System.Text.RegularExpressions;


public class AddScriptEditorWindow : EditorWindow
{
    // Example of a Class Name
    static string className = "CoolGuyScript";

    // Used for EditorPrefs
    const string key = "PENDING_SCRIPT_KEY";
    
    public string prompt = "";

    // Please don't request too much, and please use ur own API key if u have one
    public const string apiKey = "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";

    private string strin = "Use C# Unity to write code, assume every task I give u requires a new C# script. ";

    // API Model and settings
    private string model = "text-davinci-003";
    public float temperature = 0.5f;
    public int maxTokens = 200;

    // Globals
    private UnityWebRequest thisReq;
    private MonoScript scriptGenerated;

    [MenuItem("Open AI/Add Script")]
    static void Init()
    {
        AddScriptEditorWindow window = (AddScriptEditorWindow)EditorWindow.GetWindow(typeof(AddScriptEditorWindow));
        window.Show();
    }

    void OnGUI()
    {
        // Only for Testing purposes
        /* GUILayout.Label("Instruction");
        strin = EditorGUILayout.TextField(strin);*/

        GUILayout.Label("Promt");
        prompt = EditorGUILayout.TextField(prompt);
      
        if (GUILayout.Button("Compile"))
        {
            Request();
        }
    }


    private void WaitForRequest()
    {
        // Wait for compilation to finish
        var request = thisReq;
        if (!thisReq.isDone)
        {
            return;
        }

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Deserialize the JSON response and Trim response line
            var response = JsonUtility.FromJson<Response>(request.downloadHandler.text);
            string textReponse = response.choices[0].text.Trim();
         
            // AI often returns example of code, which we do not want
            int firstNewlineIndex = textReponse.IndexOf('\n');
            string outputString = textReponse.Substring(firstNewlineIndex + 1);

            /*//if you wish to see the code in console
            Debug.Log(outputString);*/

            // Deal with actually implementing the Script
            AddNewScript(ExtractClassName(textReponse), outputString);


        }

        // Remove from delegate
        EditorApplication.update -= WaitForRequest;
    }

    void Request()
    {
        // Create a JSON object with the necessary parameters, Unity's JsonUtility.ToJson failed me
        string toJson = "{\"prompt\":\"" + strin  + prompt + "\",\"model\":\"" + model + "\",\"temperature\":" + temperature + ",\"max_tokens\":" + maxTokens + "}";
        //newBody = new requestBody(prompt, model, temperature.ToString(), maxTokens.ToString());


        byte[] body = System.Text.Encoding.UTF8.GetBytes(toJson);

        // Create a new POST (send data) UnityWebRequest
        var request = new UnityWebRequest("https://api.openai.com/v1/completions", "POST");

        // REQUIRED fields for API
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send request
        request.SendWebRequest();

        // Add WaitForRequest to Unity Editors update
        thisReq = request;
        EditorApplication.update += WaitForRequest;
    }

    // Third-Party method (CHATGPT3)
    public static string ExtractClassName(string codeString)
    {
        // Define a regular expression pattern to match the class declaration
        string pattern = @"class\s+(\w+)\s+:\s+MonoBehaviour";

        // Create a regular expression object with the pattern
        Regex regex = new Regex(pattern);

        // Use the regular expression to match the class declaration in the code string
        Match match = regex.Match(codeString);

        // If the regular expression matched the class declaration, return the class name
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        else
        {
            Debug.Log("Getting ClassName failed!");
            return null; // or throw an exception, depending on how you want to handle errors
        }
    }

    void AddNewScript(string localClassName, string scriptContents)
    {
        // Set className
        className = localClassName;

        // Set script path
        string scriptPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + className + ".cs");
        System.IO.File.WriteAllText(scriptPath, scriptContents);

        // Wihtout this function the generated script will always be added to selected GameObject every compile
        static void ImportScript(string scriptPath, string localClassName)
        {
            EditorPrefs.SetString(key, localClassName);
            AssetDatabase.ImportAsset(scriptPath);
        }

        ImportScript(scriptPath, localClassName);
    }

   
    // Called when unity complies scripts
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        // Check if compiled script is generated script
        if (!EditorPrefs.HasKey(key))
            return;

        Debug.Log("Scripts reloaded!");

        // Add the script as a component to the selected game object
        if (Selection.activeGameObject != null)
        {
            // .cs will NOT work, source for line: https://gist.github.com/tklee1975/d8c4d1ea671f238efd0a5c6902d07d8b
            Type type = Type.GetType(EditorPrefs.GetString(key) + ",Assembly-CSharp");
           
            Debug.Log("Attempting to add component " + type);
            Selection.activeGameObject.AddComponent(type);
            Debug.Log("Added component!");

            // Removes key
            EditorPrefs.DeleteKey(key);
        }
        else
        {
            Debug.LogError("No game object selected.");
        }
    }

    // Classes used increase readbility and to better read reposne data
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

}
