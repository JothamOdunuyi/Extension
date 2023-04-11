using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static ChatGPT;

public class ChatGPT : MonoBehaviour
{
    private const string apiKey = "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";

    [SerializeField]
    public GPT_NPC NPC;
    
    // Declare your serializable data.
    public class GPTMessageBody 
    {
        public string prompt = "";
        public string model = "text-davinci-003";
        public float temperature = .5f;
        public int max_tokens = 4000;

        public GPTMessageBody(string prompt, string model, float temperature, int max_tokens)
        {
            this.prompt = prompt;
            this.model = model;
            this.temperature = temperature;
            this.max_tokens = max_tokens;
        }

        public GPTMessageBody(string prompt, string model, float temperature)
        {
            this.prompt = prompt;
            this.model = model;
            this.temperature = temperature;
        }

        public GPTMessageBody(string prompt, string model)
        {
            this.prompt = prompt;
            this.model = model;
        }

        public GPTMessageBody(string prompt)
        {
            this.prompt = prompt;
        }
    }

    public GPTMessageBody messageBody;

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
            return null; // or throw an exception, depending on how you want to handle errors
        }
    }


    private void Start()
    {
        GetResponse();
    }

    public void GetResponse()
    {
        StartCoroutine(MakeRequest());
    }

    public IEnumerator MakeRequest()
    {

        string promtInstructions = $"From now on act as if you are a NPC in a {NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. Your name is {NPC.name}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $"{NPC.name} introduces their self with their name." : null)} {NPC.name} replies: Human like, as if a traveler greeted you, in {NPC.language} and short. {NPC.name} doesn't ever: say their personality traits, {(NPC.assume_assitance ? $"assume the traveler needs assitance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themself with their name" : null)} say \"{NPC.name}\". ";
        messageBody = new GPTMessageBody(promtInstructions, "text-davinci-003", NPC.creativity, 3000);
       
        string toJson = JsonUtility.ToJson(messageBody);
        print(toJson);


        byte[] body = System.Text.Encoding.UTF8.GetBytes(toJson);
        print(body.Length);

        // Create a new UnityWebRequest
        var request = new UnityWebRequest("https://api.openai.com/v1/completions", "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        
        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Deserialize the JSON response
            var response = JsonUtility.FromJson<Response>(request.downloadHandler.text);
            string textReponse = response.choices[0].text.Trim();
            //string className = ExtractClassName(textReponse);
            Debug.Log(textReponse);
            //Debug.Log("CLASS NAME " + className);

            //textmesh.text = response.choices[0].text.TrimStart().TrimEnd().ToString();
            //print("response.choices[0].text.TrimStart().TrimEnd().ToString()");
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