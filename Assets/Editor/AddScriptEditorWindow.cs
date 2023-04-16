using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.Experimental.GraphView;
using System.Runtime.InteropServices;
using UnityEditor.PackageManager.UI;
using UnityEngine.Networking;
using System.Reflection;
using UnityEditor.PackageManager.Requests;
using System.Text.RegularExpressions;
using System.Net;
using Unity.Profiling.LowLevel;
using System.Runtime.CompilerServices;

public class AddScriptEditorWindow : EditorWindow
{
    // Example of a Class Name
    static string className = "CoolGuyScript";

    // Used for EditorPrefs
    const string key = "PENDING_SCRIPT_KEY";
    
    public string prompt = "";

    // Please don't request too much, and please use ur own API key if u have one
    public const string apiKey = "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";

    private string strin = "Only respond with Unity C# code, code must make a class with unique name. ";

    // API Model and settings
    private string model = "text-davinci-003";
    public float temperature = 0.5f;
    public int maxTokens = 3000;

    private int maxCharacters = 200;

    //Styles
    GUIStyle promptStyle = new GUIStyle(EditorStyles.textArea);
    GUIStyle buttonStyle = new GUIStyle(EditorStyles.boldLabel);

    // Window Values, const allows static access
    private const int windowWidth = 585;
    private const int windowHeight = 100;

    // Globals
    private UnityWebRequest thisReq;
    private MonoScript scriptGenerated;
    string textInput;
    private int maxCharactersOver = -1; //as long as its not > 0
    private bool isTyping;

    GameObject activeGameObject;

    [MenuItem("Open AI/Generate Script")]
    static void Init()
    {
        AddScriptEditorWindow window = GetWindow<AddScriptEditorWindow>("Script Generator");
        window.minSize = new Vector2(windowWidth, windowHeight);
        window.maxSize = new Vector2(windowWidth + 140, windowHeight + 50);
        window.Show();
    }

    void OnGUI()
    {

        promptStyle.fontSize = 16;
        promptStyle.normal.textColor = Color.white;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.alignment = TextAnchor.MiddleCenter;

        AddScriptEditorWindow window = GetWindow<AddScriptEditorWindow>("CoolThing");
        //Debug.Log(window.position);

        Rect windowRect = new Rect(0, 0, position.width, position.height);

        // Set the text field size to match the window size
        Rect textFieldRect = new Rect(10, 10, windowRect.width - 20, windowRect.height - 60);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        GUILayout.Label(maxCharactersOver >= 0 ? $"Max Characters!! ({maxCharactersOver})" : "", buttonStyle, GUILayout.Width(150), GUILayout.Height(15));
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();

        if (Event.current.Equals(Event.KeyboardEvent("return"))) {
            if(maxCharactersOver < 0) {
                Request();
            }
        };

        GUI.SetNextControlName("PromptField");
        prompt = EditorGUILayout.TextField(prompt.Substring(0, Mathf.Clamp(prompt.Length, 0, maxCharacters)), promptStyle, GUILayout.Width(windowWidth - 80), GUILayout.Height(windowHeight - 20));
        if (EditorGUI.EndChangeCheck())
        {
            maxCharactersOver = prompt.Length - maxCharacters;
        }

        Rect promtRect = GUILayoutUtility.GetLastRect();

        void SendMessageInPromp()
        {
            if (string.IsNullOrEmpty(prompt))
            {
                GUI.TextField(new Rect(promtRect.x + 1, promtRect.y + 1, promtRect.width - 2, promtRect.height - 2), "Send message...", promptStyle);
                isTyping = false;
                EditorGUI.FocusTextInControl("PromptField");
            }

            if (GUI.GetNameOfFocusedControl() == "PromptField")
            {
                if (!isTyping)
                {
                    isTyping = true;
                    Repaint();
                }
            }
        }

        SendMessageInPromp();

        EditorGUILayout.BeginVertical();

        EditorGUI.BeginDisabledGroup(true); // Disable editing of the field
        activeGameObject = EditorGUILayout.ObjectField(Selection.activeGameObject, typeof(GameObject), true, GUILayout.Width(windowWidth - 400), GUILayout.Height(25)) as GameObject;
        EditorGUI.EndDisabledGroup();

        
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Disables the submit button if maxcharacters reached
        GUI.enabled = maxCharactersOver < 0 ? true : false;
        if (GUILayout.Button("",GUILayout.Width(windowWidth - 400), GUILayout.Height(40)))
        {
            Request();
        }
        Rect buttonRect = GUILayoutUtility.GetLastRect();
        GUI.Label(new Rect(buttonRect.x , buttonRect.y, buttonRect.width, buttonRect.height), "Submit", buttonStyle);


        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

 

    }

    #region Request Handlers
    // Wait for compilation to finish
    private void WaitForRequest()
    {
        var request = thisReq;
        if (!thisReq.isDone)
        {
          /*  float progress = Mathf.Clamp01(request.downloadProgress + request.uploadProgress);
            Debug.Log(progress * 100 + "%");*/
            return;
        }

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Deserialize the JSON response and Trim response line
            var response = JsonUtility.FromJson<Response>(request.downloadHandler.text);
            string textReponse = response.choices[0].text.Trim();
         
            // AI often returns example of code in first line, which we do not want
            int firstNewlineIndex = textReponse.IndexOf('\n');
            string outputString = !textReponse.Substring(firstNewlineIndex, firstNewlineIndex).Contains("using") ? textReponse : textReponse.Substring(firstNewlineIndex + 1);

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

    #endregion

    #region Script Generation
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
            Debug.LogWarning("Getting ClassName failed!");
            return null; // or throw an exception, depending on how you want to handle errors
        }
    }

    void AddNewScript(string localClassName, string scriptContents)
    {
        // Set className
        className = localClassName == null ? "GeneratedScript" : localClassName;

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

        // Add the script as a component to the selected game object
        if (Selection.activeGameObject != null)
        {
            // .cs will NOT work, source for line: https://gist.github.com/tklee1975/d8c4d1ea671f238efd0a5c6902d07d8b
            Type type = Type.GetType(EditorPrefs.GetString(key) + ",Assembly-CSharp");
           
            Selection.activeGameObject.AddComponent(type);
            Debug.Log($"Added component {type}");

        }
        else
        {
            Debug.LogWarning("No game object was selected, only generated script.");
        }

        EditorPrefs.DeleteKey(key);

    }
    #endregion

    #region Reponse Classes
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

    #endregion
}
