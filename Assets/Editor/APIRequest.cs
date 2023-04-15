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
    private string apiKey = "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";
    private string gpt3Endpoint = "https://api.openai.com/v1/chat/completions";
    private const float typingSpeed = 0.037f;
    private bool canSumbit = true;
    private string prompt = "";
    private int maxCharacters = 1000;
    private bool rememberPrompts = true;
    private string converation;
    private UnityWebRequest www;
    private UnityWebRequest thisReq;
    private bool windowClosed = false;
    GameObject dialogueCanvas;

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

    RequestData requestData = new RequestData();
    private Vector2 scrollPosition = Vector2.zero; // Scroll position

    GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
    GUIStyle promptTextAreaStyle = new GUIStyle(EditorStyles.textArea);

    private bool isTyping;

    Rect promtRect;
  

    [MenuItem("Open AI/Ask GPT")]
    static void Init()
    {
        APIRequest window = GetWindow<APIRequest>("UnityGPT Chatbot");
        window.Show();
        
    }


    void OnGUI()
    {
        textAreaStyle.fontSize = 16;
        promptTextAreaStyle.fontSize = 14;

        Rect windowRect = new Rect(0, 0, position.width*2, position.height * 4);
       

        Rect conversationRect = new Rect(0, 0, windowRect.width, windowRect.height );


        

        // Only for Testing purposes
        // GUILayout.Label("UnityGPT");
        //prompt = EditorGUILayout.TextField(prompt);
        GUILayout.BeginVertical();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width));
        
        // Add a label field that covers half the window with a fixed width
        EditorGUILayout.LabelField(converation, textAreaStyle, GUILayout.ExpandHeight(true));

        //.Label(converation, GUILayout.Width(position.width), GUILayout.Height(position.height/4));
        GUILayout.EndScrollView();
        // End the horizontal layout group
        GUILayout.EndVertical();

        // Set the text field size to match the window size

        Rect textFieldRect = new Rect(10, 10, windowRect.width - 40, windowRect.height / 4);

        // Draw a button below the text field
        Rect buttonRect = new Rect(textFieldRect.xMax, windowRect.yMax , textFieldRect.width, 10);


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // IF statement from https://answers.unity.com/questions/178117/check-if-enter-is-pressed-inside-a-gui-textfield.html
        // This checks if the Enter key is pressed while in the textfield
        if (Event.current.Equals(Event.KeyboardEvent("return"))) { Debug.Log("ENTERR"); AttemptRequest();};
        GUI.SetNextControlName("PromptField");
        prompt = EditorGUILayout.TextField(prompt, promptTextAreaStyle, GUILayout.Height(40), GUILayout.Width(400));
        promtRect = GUILayoutUtility.GetLastRect();

        if (string.IsNullOrEmpty(prompt))
        {
            GUI.TextField(new Rect(promtRect.x+1, promtRect.y+1, promtRect.width-2, promtRect.height-2), "Send message...");
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

    private void StartUnityGPT()
    {
        requestData.messages = new List<Messages>();
        requestData.model = "gpt-3.5-turbo"; // Set the model

        requestData.messages.Add(new Messages { role = "assistant", content = "You are an helpful AI chatbot in Unity. Your name is UnityGPT." });
        requestData.messages.Add(new Messages { role = "user", content = prompt });
    }

    private void WaitForRequest()
    {
        if (!canSumbit)
        {
            thisReq = www;


            if (!thisReq.isDone)
            {
                /*float progress = Mathf.Clamp01(www.downloadProgress + www.uploadProgress);
                if (progress > 0)
                {
                    Debug.Log(progress * 100 + "%");
                }*/

                return;
            }


            if (thisReq.result == UnityWebRequest.Result.Success)
            {
                // Convert data
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(thisReq.downloadHandler.text);

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

                // Displays the updated conversation without havint to wait for OnGUI
                Repaint();
            }
            else
            {
                Debug.LogWarning("Error sending request: " + thisReq.error);
            }

            // Remove from delegate
            EditorApplication.update -= WaitForRequest;
        }
        else
        {
            EditorApplication.update -= WaitForRequest;
        }
    }

    private void Request()
    {
        if (!canSumbit)
            return;

        // Make sure User cannot spam request
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

        //Testing purposes
        //Debug.Log(json);

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

        thisReq = www;

        Debug.Log("Now waiting");
        www.SendWebRequest();
        EditorApplication.update += WaitForRequest;
    }
   

}
