using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class APIRequest : EditorWindow
{

    public static string prompt = "How are you bro";
    public static string apiKey = "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";

    private static string strin = "Reply as if you are in a game lobby";

    // The engine you want to use (keep in mind that it has to be the exact name of the engine)
    private static  string model = "text-davinci-003";
    public static float temperature = 0.5f;
    public static int maxTokens = 200;
    static bool called;

    [MenuItem("Open AI/Request")]
     static void InitA()
    {
        Debug.Log("YOOOOOOOOO");
        called = true;

        
        

    }

    static void MakeRequest()
    {
        string toJson = "{\"prompt\":\"" + strin + "\",\"model\":\"" + model + "\",\"temperature\":" + temperature + ",\"max_tokens\":" + maxTokens + "}";
        //newBody = new requestBody(prompt, model, temperature.ToString(), maxTokens.ToString());
        //string toJson = JsonUtility.ToJson(coffePot);

        Debug.Log(toJson);
    }

    /*void OnGUI()
   {
       Debug.Log("GOING");
       if (called){
           called = false;
           MakeRequest();
       }

   }*/



    /*IEnumerator MakeRequest()
    {
        string inputText = Input.text;

        // Create a JSON object with the necessary parameters, Unity's JsonUtility.ToJson failed me
        


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
            string textReponse = response.choices[0].text.TrimStart().TrimEnd();
            string className = ExtractClassName(textReponse);
            Debug.Log(textReponse);
            Debug.Log("CLASS NAME " + className);

            //textmesh.text = response.choices[0].text.TrimStart().TrimEnd().ToString();
            print("response.choices[0].text.TrimStart().TrimEnd().ToString()");
        }
    }*/

    // A class to hold the JSON response
    [System.Serializable]
    internal class Response
    {
        private Choice[] choices;
    }

    [System.Serializable]
    internal class Choice
    {
        private string text;
    }

}
