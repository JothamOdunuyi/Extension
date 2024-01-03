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
using System.Collections.Generic;

public class AddScriptEditorWindow : EditorWindow
{
    // Example of a Class Name
    static string className = "CoolGuyScript";

    // Used for EditorPrefs
    const string key = "PENDING_SCRIPT_KEY";
    
    public string prompt = "";

    // Please don't request too much, but have fun! And obviously do not share this key
    public const string apiKey = "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";//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

    private string assistancePrompt2 = " and make sure the code uses the correct namespaces"; 
    private string assistancePrompt = $"Only respond with Unity C# code, code must make a class with unique name and inherit from MonoBehaviour or EditorWindow ";
    
    // API Model and settings
    private string model = "text-davinci-003";
    public float temperature = 0.5f;
    public int maxTokens = 3000;

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
    private bool setStyles;
    private UnityWebRequest request;
    private int maxCharacters = 200;
    GameObject activeGameObject;

    [MenuItem("Open AI/Generate Script")]
    static void OpenWindow()
    {
        AddScriptEditorWindow window = GetWindow<AddScriptEditorWindow>("Script Generator");
        window.minSize = new Vector2(windowWidth, windowHeight);
        window.maxSize = new Vector2(windowWidth + 140, windowHeight + 50);
        window.Show();
    }

    void OnGUI()
    {
        #region UI Creation Methods

        void SetStyles()
        {
            if (!setStyles)
            {
                // Set the styles
                promptStyle.fontSize = 16;
                promptStyle.normal.textColor = Color.white;
                buttonStyle.normal.textColor = Color.white;
                buttonStyle.alignment = TextAnchor.MiddleCenter;
                setStyles = true;
            }
        }

        void MaxCharacterLabel()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUILayout.Label(maxCharactersOver >= 0 ? $"Max Characters!! ({maxCharactersOver})" : "", buttonStyle, GUILayout.Width(150), GUILayout.Height(15));
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        void SendMessageInPrompt(Rect promptRect)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                GUI.TextField(new Rect(promptRect.x + 1, promptRect.y + 1, promptRect.width - 2, promptRect.height - 2), "Create a script that...", promptStyle);
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

        void PromptField()
        {
            EditorGUI.BeginChangeCheck();

            if (Event.current.Equals(Event.KeyboardEvent("return")))
            {
                if (maxCharactersOver < 0)
                {
                    Request();
                }
            };

            GUI.SetNextControlName("PromptField");
            prompt = EditorGUILayout.TextField(prompt.Substring(0, Mathf.Clamp(prompt.Length, 0, maxCharacters)), promptStyle, GUILayout.Width(windowWidth - 80), GUILayout.Height(windowHeight - 20));
            if (EditorGUI.EndChangeCheck())
            {
                maxCharactersOver = prompt.Length - maxCharacters;
            }
        }

        void SubmitButton()
        {
            // Disables the submit button if maxcharacters reached
            GUI.enabled = maxCharactersOver < 0 ? true : false;
            if (GUILayout.Button("", GUILayout.Width(windowWidth - 400), GUILayout.Height(40)))
            {
                Request();
            }
            Rect buttonRect = GUILayoutUtility.GetLastRect();
            GUI.Label(new Rect(buttonRect.x, buttonRect.y, buttonRect.width, buttonRect.height), "Submit", buttonStyle);

        }

        #endregion

        SetStyles();

        MaxCharacterLabel();

        EditorGUILayout.BeginHorizontal();

        PromptField();

        SendMessageInPrompt(GUILayoutUtility.GetLastRect());

        EditorGUILayout.BeginVertical();

        // Game Object field
        EditorGUI.BeginDisabledGroup(true); // Disable editing of the field

        activeGameObject = EditorGUILayout.ObjectField(Selection.activeGameObject, typeof(GameObject), true, GUILayout.Width(windowWidth - 400), GUILayout.Height(25)) as GameObject;
       
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        SubmitButton();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    #region Request Handlers

    // Send API request
    void Request()
    {
        // Create a JSON object with the necessary parameters
        string toJson = "{\"prompt\":\"" + assistancePrompt + assistancePrompt2 + prompt + "." + "\",\"model\":\"" + model + "\",\"temperature\":" + temperature + ",\"max_tokens\":" + maxTokens + "}";

        byte[] body = System.Text.Encoding.UTF8.GetBytes(toJson);

        // Create a new POST (send data) UnityWebRequest
        request = new UnityWebRequest("https://api.openai.com/v1/completions", "POST");

        // REQUIRED fields for API
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send request
        request.SendWebRequest();

        // Add WaitForRequest to Unity Editors update
        EditorApplication.update += WaitForRequest;
    }

    // Wait for compilation to finish
    private void WaitForRequest()
    {
        if (!request.isDone)
        {
            // Shows progress
            /* float progress = Mathf.Clamp01(request.downloadProgress + request.uploadProgress);
            Debug.Log(progress * 100 + "%");*/
            return;
        }

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning(request.error);
        }
        else
        {
            // Deserialize the JSON response and Trim response line
            var response = JsonUtility.FromJson<Response>(request.downloadHandler.text);
            string textReponse = response.choices[0].text.Trim();
            string outputString = textReponse;

            // Deal with actually implementing the Script
            AddNewScript(ExtractClassName(textReponse), textReponse);
        }

        // Remove from delegate
        EditorApplication.update -= WaitForRequest;
    }

    #endregion

    #region Script Generation

    // Third-Party method (CHATGPT-3)
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

    // Creates the generated script .cs file
    void AddNewScript(string localClassName, string scriptContents)
    {
        // Set className
        className = localClassName == null ? "GeneratedScript" : localClassName;

        // Set script path
        string scriptPath = AssetDatabase.GenerateUniqueAssetPath("Assets/GeneratedScripts/" + className + ".cs");
        System.IO.File.WriteAllText(scriptPath, scriptContents);

        // Wihtout this function the generated script will always be added to selected GameObject every compile
        static void ImportScript(string scriptPath, string localClassName)
        {
            EditorPrefs.SetString(key, localClassName);
            AssetDatabase.ImportAsset(scriptPath);
        }

        ImportScript(scriptPath, localClassName);
    }

    // Attempts to add the script into a game object, called when unity complies scripts
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

        // Delete the key after use
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
