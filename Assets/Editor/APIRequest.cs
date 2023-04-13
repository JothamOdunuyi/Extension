/*using UnityEngine;
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

public class APIRequest : EditorWindow
{
    private string apiKey = "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";
    private string gpt3Endpoint = "https://api.openai.com/v1/chat/completions";
    private const float typingSpeed = 0.037f;
    private bool canSumbit = true;
    private UnityWebRequest www;

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

    [MenuItem("Open AI/Ask GPT")]
    static void Init()
    {
        AddScriptEditorWindow window = (AddScriptEditorWindow)EditorWindow.GetWindow(typeof(AddScriptEditorWindow));
        window.Show();
    }

    void OnGUI()
    {*//*
        // Only for Testing purposes
        *//* GUILayout.Label("Instruction");
        strin = EditorGUILayout.TextField(strin);*//*

        Rect windowRect = new Rect(0, 0, position.width, position.height);

        // Set the text field size to match the window size
        Rect textFieldRect = new Rect(10, 10, windowRect.width - 20, windowRect.height - 60);

        EditorGUI.BeginChangeCheck();
        // Draw the text field
        prompt = EditorGUI.TextField(textFieldRect, prompt.Substring(0, Mathf.Clamp(prompt.Length, 0, maxCharacters)));
        if (EditorGUI.EndChangeCheck())
        {
            if (prompt.Length > maxCharacters)
            {
                Debug.Log("max length reached");
            }

        }
        //prompt = prompt.Substring(Mathf.Clamp(prompt.Length, 0, 200));
        //char[] f = prompt.ToCharArray();

        // Draw a button below the text field
        Rect buttonRect = new Rect(textFieldRect.x, textFieldRect.yMax + 10, textFieldRect.width, 30);
        if (GUI.Button(buttonRect, "Submit"))
        {
            Request();
        }

        *//* GUILayout.Label("Promt");
         prompt = EditorGUILayout.TextField(prompt);

         if (GUILayout.Button("Compile"))
         {
             Request();
         }*//*
    }
    IEnumerator SendRequest()
    {
        if (!canSumbit)
            yield break;

        // Make sure User cannot spam request
        canSumbit = false;


        // After NPC greeting, start storing user input
        if (requestData.messages.Count != 1 && !string.IsNullOrEmpty(inputField.text))
        {
            //print("sent player input");
            requestData.messages.Add(new Messages { role = "user", content = inputField.text });
        }

        // Convert request data to JSON
        string json = JsonUtility.ToJson(requestData);

        //Testing purposes
        //print(json);

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


        // Send request and wait for response
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Convert data
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);

            Choices systemMessage = responseData.choices[0];
            string assistantReply = systemMessage.message.content;

            // Add NPC reponse to list of messages
            requestData.messages.Add(new Messages { role = "system", content = assistantReply });

            *//* Testing purposes
             Debug.Log(www.downloadHandler.text);
             Debug.Log("Assistant: " + assistantReply);*//*

        }
        else
        {
            Debug.Log("Error sending request: " + www.error);
        }

        canSumbit = true;
    }
}
*/