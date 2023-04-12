using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using UnityEditor.PackageManager.Requests;
using TMPro;
using System.Data;
using System;
using UnityEditor;
using UnityEngine.UI;

public class GPTNPC_Dialogue : MonoBehaviour
{
    private string apiKey = "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"; // Replace with your OpenAI API key
    private string gpt3Endpoint = "https://api.openai.com/v1/chat/completions";
    private const float typingSpeed = 0.05f;
    private bool canSumbit = true;
    private UnityWebRequest www;

    [SerializeField]
    public GPT_NPC NPC;

    [SerializeField]
    public TMP_Text textField;

    [SerializeField]
    public TMP_InputField inputField;

    [SerializeField]
    public UnityEngine.UI.Button sumbitButton;

    [SerializeField]
    public Slider slider;

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

    void Start()
    {
        requestData.messages = new List<Messages>();
        requestData.model = "gpt-3.5-turbo"; // Set the model
        SetNPCData();
        // Start a coroutine to send a request to OpenAI API
        sumbitButton.onClick.AddListener(() => GetResponse());
        StartCoroutine(SendRequest());
    }

    void GetResponse()
    {
        StartCoroutine(SendRequest());
    }


    void SetNPCData()
    {
        string promtInstructions = $"From now on act as if you are a NPC in a {NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. Your name is {NPC.name}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $"{NPC.name} introduces their self with their name." : null)} {NPC.name} replies: Human like, as if a traveler greeted you, in {NPC.language} and short. {NPC.name} doesn't ever: say their personality traits, {(NPC.assume_assitance ? $"assume the traveler needs assitance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themself with their name" : null)} say \"{NPC.name}\". ";
        requestData.messages.Add(new Messages { role = "assistant", content = promtInstructions });
    }

    IEnumerator SendRequest()
    {
        if (!canSumbit)
            yield break;

        canSumbit = false;
        sumbitButton.gameObject.SetActive(false);

        // Prepare request data
        if (requestData.messages.Count == 1)
        {
            print("First call");
        }
        else
        {
            requestData.messages.Add(new Messages { role = "user", content = inputField.text });
        }

        // Convert request data to JSON
        string json = JsonUtility.ToJson(requestData);

        print(json);

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

        print("SENT");

        slider.gameObject.SetActive(true);
        EditorApplication.update += LoadingBar;
        // Send request and wait for response
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);
            Choices systemMessage = responseData.choices[0];
            string assistantReply = RemoveSpeechMarks(systemMessage.message.content);

            /*print("Choices data " + responseData.choices);
            print("Choice 0 data " + responseData.choices[0]);*/

            requestData.messages.Add(new Messages { role = "system", content = assistantReply });

            /* foreach (var msgContent in requestData.messages)
             {
                 textField.text += $"{msgContent}\n";
             }*/
            // Do something with the assistant's reply
            Debug.Log(www.downloadHandler.text);
            Debug.Log("Assistant: " + assistantReply);
            StartCoroutine(TypeText(assistantReply));
        }
        else
        {
            Debug.Log("Error sending request: " + www.error);
        }

        sumbitButton.gameObject.SetActive(true);
        canSumbit = true;
    }

     private void LoadingBar()
    {
        float progress = Mathf.Clamp01(www.downloadProgress);
        Debug.Log(progress);
        slider.value = (int)(progress * 100f);


        if(progress > 0)
        {
            slider.gameObject.SetActive(false);
            EditorApplication.update -= LoadingBar;
        }
    }

    IEnumerator TypeText(string text)
    {
        textField.text = "";
        foreach (char c in text)
        {
            textField.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public string RemoveSpeechMarks(string input)
    {
        if (input.Contains("\""))
        {
            input = input.Replace("\"", "");
        }
        return input;
    }
}
