using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using UnityEditor.PackageManager.Requests;
public class GPT3Assistant2 : MonoBehaviour
{
    private string apiKey = "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"; // Replace with your OpenAI API key
    private string gpt3Endpoint = "https://api.openai.com/v1/chat/completions";

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

    void Start()
    {
        // Start a coroutine to send a request to OpenAI API
        StartCoroutine(SendRequest());
    }

    IEnumerator SendRequest()
    {
        // Prepare request data
        RequestData requestData = new RequestData();
        requestData.model = "gpt-3.5-turbo"; // Set the model
        requestData.messages = new List<Messages>()
        {
            new Messages { role = "system", content = "Knock knock" },
            new Messages { role = "user", content = "Who's there" }
        };
       /* requestData.messages[0].content = "You are a helpful assistant."; // System message
        requestData.messages[1].content = "Hello please help me"; // User message
*/
        // Convert request data to JSON
        string json = JsonUtility.ToJson(requestData);

        print(json);
        // Create UnityWebRequest
        UnityWebRequest www = new UnityWebRequest(gpt3Endpoint, UnityWebRequest.kHttpVerbPOST);
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);
        www.SetRequestHeader("Content-Type", "application/json");

        // Encode JSON data as bytes
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(jsonBytes);
        www.uploadHandler.contentType = "application/json";

        // Set download handler to receive response
        www.downloadHandler = new DownloadHandlerBuffer();

        print("SENT");
        // Send request and wait for response
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);
            string assistantReply = responseData.choices[0].message.content;

            // Do something with the assistant's reply
            Debug.Log("Assistant: " + assistantReply);
          
        }
        else
        {
            Debug.Log("Error sending request: " + www.error);
        }
    }
}
