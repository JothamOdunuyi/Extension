using System;
using System.Collections;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Networking;
using static ChatGPT;

public class ChatGPT : MonoBehaviour
{
    public const string prompt = "How are you bro";
    public const string apiKey = "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";

    private const string strin = "Reply as if you are in a game lobby";

    // The engine you want to use (keep in mind that it has to be the exact name of the engine)
    private string model = "text-davinci-003";
    public float temperature = 0.5f;
    public int maxTokens = 200;

    public TMP_Text textmesh;
    public TMP_InputField Input;

    
    // Declare your serializable data.
    [System.Serializable]
    public class CoffeeMaker
    {
        public string prompt = "How are you";
        public string model = "text-davinci-003";
        public float temperature = 333f;
        public int max_tokens = 225;

        public CoffeeMaker(string prompt, string model, float temperature, int max_tokens)
        {
            this.prompt = prompt;
            this.model = model;
            this.temperature = temperature;
            this.max_tokens = max_tokens;
        }
    }




    public CoffeeMaker coffePot = new CoffeeMaker("Are you ready to play?", "text-davinci-003", .5f, 200);



    private void Start()
    {
        GetResponse();
    }

    public void GetResponse()
    {
        StartCoroutine(MakeRequest());
    }

    IEnumerator MakeRequest()
    {
        string inputText = Input.text;

        // Create a JSON object with the necessary parameters, Unity's JsonUtility.ToJson failed me
        string json = "{\"prompt\":\"" + strin + "\",\"model\":\"" + model + "\",\"temperature\":" + temperature + ",\"max_tokens\":" + maxTokens + "}";
        //newBody = new requestBody(prompt, model, temperature.ToString(), maxTokens.ToString());
        string toJson = JsonUtility.ToJson(coffePot);
        print(toJson);


        byte[] body = System.Text.Encoding.UTF8.GetBytes(toJson);

        // Create a new UnityWebRequest
        var request = new UnityWebRequest("https://api.openai.com/v1/completions", "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Deserialize the JSON response
            var response = JsonUtility.FromJson<Response>(request.downloadHandler.text);
            Debug.Log(response.choices[0].text.TrimStart().TrimEnd());

            //textmesh.text = response.choices[0].text.TrimStart().TrimEnd().ToString();
            print("response.choices[0].text.TrimStart().TrimEnd().ToString()");
        }
    }

    // A class to hold the JSON response
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