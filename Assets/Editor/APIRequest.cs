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
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Runtime.Remoting.Contexts;
using UnityEditor.UIElements;

public class APIRequest : EditorWindow
{
    // Please don't request too much, but have fun! And obviously do not share this key
    private string apiKey = "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";
    private string gpt3Endpoint = "https://api.openai.com/v1/chat/completions";

    //Globals
    private string prompt = "";
    private string converation;
    private int maxCharacters = 1000;
    private const float typingSpeed = 0.037f;
    private bool rememberPrompts = true;
    private bool canSumbit = true;
    private bool windowClosed = false;
    private bool isTyping;
    private GameObject dialogueCanvas;
    private Vector2 scrollPosition = Vector2.zero; // Scroll position
    private Rect promtRect;
    private UnityWebRequest www;
    private RequestData requestData = new RequestData();

    // Styles
    GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
    GUIStyle promptTextAreaStyle = new GUIStyle(EditorStyles.textArea);

    // Window Values, const allows static access
    private const int windowWidth = 1000;
    private const int windowHeight = 1000;

    [MenuItem("Open AI/Ask GPT")]
    static void OpenWindow()
    {
        APIRequest window = GetWindow<APIRequest>("UnityGPT Chatbot");

        window.maxSize = new Vector2(windowWidth, windowHeight );
        window.minSize = new Vector2(600, 400);

        window.Show();
    }

    void OnGUI()
    {
        textAreaStyle.fontSize = 16;
        promptTextAreaStyle.fontSize = 14;

        void PromptField()
        {
            // IF statement from https://answers.unity.com/questions/178117/check-if-enter-is-pressed-inside-a-gui-textfield.html
            // This checks if the Enter key is pressed while in the textfield
            if (Event.current.Equals(Event.KeyboardEvent("return"))) { AttemptRequest(); };
            GUI.SetNextControlName("PromptField");
            prompt = EditorGUILayout.TextField(prompt, promptTextAreaStyle, GUILayout.Height(40), GUILayout.Width(400));
            promtRect = GUILayoutUtility.GetLastRect();

            if (string.IsNullOrEmpty(prompt))
            {
                GUI.TextField(new Rect(promtRect.x + 1, promtRect.y + 1, promtRect.width - 2, promtRect.height - 2), "Send message...");
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

        GUILayout.BeginVertical();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width));
        EditorGUILayout.LabelField(converation, textAreaStyle, GUILayout.ExpandHeight(true));

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        PromptField();
        
        GUI.enabled = canSumbit;
        if (GUILayout.Button("Submit", GUILayout.Height(40)))
        {
            AttemptRequest();
        }

        GUILayout.FlexibleSpace();     
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        rememberPrompts = EditorGUILayout.Toggle("Remember prompts", rememberPrompts);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.enabled = !canSumbit;

    }

    // Check if under max characters
    void AttemptRequest()
    {
        if (prompt.Length < maxCharacters)
        {
            Request();
            prompt = ""; 
            EditorGUIUtility.keyboardControl = 0;
        }
        else
        {
            EditorUtility.DisplayDialog("Error", $"Promt is over {maxCharacters} characters", "OK");
        }
    }

    // Add the assitant and user message
    private void StartUnityGPT()
    {
        requestData.messages = new List<Messages>();
        requestData.model = "gpt-3.5-turbo"; // Set the model

        requestData.messages.Add(new Messages { role = "assistant", content = "You are an helpful AI chatbot in Unity. Your name is UnityGPT." });
        requestData.messages.Add(new Messages { role = "user", content = prompt });
    }

    // Send API request
    private void Request()
    {
        // Make sure User cannot spam request
        if (!canSumbit)
            return;

        canSumbit = false;

        // After NPC greeting, start storing user input
        if (requestData.messages == null||requestData.messages.Count == 0 || !rememberPrompts){
            StartUnityGPT();
        }
        else{
            requestData.messages.Add(new Messages { role = "user", content = prompt });
        }

        // Convert request data to JSON
        string json = JsonUtility.ToJson(requestData);

        // Create UnityWebRequest (Same as putting "POST" at the end
        www = new UnityWebRequest(gpt3Endpoint, UnityWebRequest.kHttpVerbPOST);
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);
        www.SetRequestHeader("Content-Type", "application/json");

        // Encode JSON data as bytes
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(jsonBytes);
        www.uploadHandler.contentType = "application/json";

        // Set download handler to receive response
        www.downloadHandler = new DownloadHandlerBuffer();

        Debug.Log("Now waiting");
        www.SendWebRequest();

        // Due to now Update() we use a delegate
        EditorApplication.update += WaitForRequest;
    }

    // Wait, then output request
    private void WaitForRequest()
    {
        if (!canSumbit)
        {

            if (!www.isDone)
            {
                /*float progress = Mathf.Clamp01(www.downloadProgress + www.uploadProgress);
                if (progress > 0)
                {
                    Debug.Log(progress * 100 + "%");
                }*/

                return;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Convert data
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);

                Choices systemMessage = responseData.choices[0];
                string assistantReply = systemMessage.message.content;

                // Adds AI's reponse to message history
                requestData.messages.Add(new Messages { role = "system", content = assistantReply });

                //So that the conversation is updated only after all data is added
                string localConvo = "";

                // Prevents first line making a new line
                bool firstInput = false;

                // Outputs the conversation
                foreach (Messages message in requestData.messages)
                {
                    if (message.role == "user" && !firstInput)
                    {
                        firstInput = true;
                        localConvo += $"{"You"} : {message.content} \n ";
                    }
                    else if (message.role != "assistant")
                    {
                        localConvo += $"\n{(message.role == "system" ? "UnityGPT" : "You")} : {message.content} \n ";
                    }
                }

                converation = localConvo;
                canSumbit = true;

                // Displays the updated conversation without having to wait for OnGUI
                Repaint();
            }
            else
            {
                Debug.LogWarning("Error sending request: " + www.error);
            }

            // Remove from delegate
            EditorApplication.update -= WaitForRequest;
        }
        else
        {
            EditorApplication.update -= WaitForRequest;
        }
    }

    #region Data Classes

    [System.Serializable]
    public class RequestData
    {
        public string model;
        public List<Messages> messages;
    }

    [System.Serializable]
    public class Messages
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class ResponseData
    {
        public string id;
        public string object_name;
        public string created;
        public string model;
        public string usage;
        public List<Choices> choices;
    }

    [System.Serializable]
    public class Choices
    {
        public Messages message;
        public double finish_probability;
    }

    #endregion

}
