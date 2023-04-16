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
using static UnityEditor.Experimental.GraphView.GraphView;
using Player;
using UnityEditor.PackageManager;
using UnityEditorInternal;

public class GPTNPC_Dialogue : MonoBehaviour
{
    private string apiKey = "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"; 
    private string gpt3Endpoint = "https://api.openai.com/v1/chat/completions";
    private const float typingSpeed = 0.037f;
    private bool canSumbit = true;
    private UnityWebRequest www;

    [SerializeField]
    public GPT_NPC NPC;

    [HideInInspector]
    public TMP_Text textField;

    [HideInInspector]
    public TMP_InputField inputField;

    [HideInInspector]
    public UnityEngine.UI.Button submitButton;

    [HideInInspector]
    public UnityEngine.UI.Button closeButton;

    [HideInInspector]
    public Slider slider;

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

    void Start()
    {
        if (!closeButton){
            Debug.LogError("GPTNPC_Dialogue MUST be created from extension, go to Open API > Create new GPT NPC and follow instructions");
            enabled = false;
            return;
        }

        dialogueCanvas = closeButton.transform.root.gameObject;
        dialogueCanvas.SetActive(false);
        requestData.messages = new List<Messages>();
        requestData.model = "gpt-3.5-turbo"; // Set the model

        SetNPCData();
       
    }

    void GetResponse()
    {
        StartCoroutine(SendRequest());
    }

    private void OnCollisionEnter(Collision hit)
    {
        GameObject player = hit.gameObject;
        if (player.tag == "Player" && canSumbit)
        {
            player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            player.GetComponent<PlayerController>().cursor_locked = false;
            dialogueCanvas.SetActive(true);

            // Adds functions to listeners
            submitButton.onClick.AddListener(() => GetResponse());
            closeButton.onClick.AddListener(() => CloseButtonOnClick());

            StartCoroutine(SendRequest());
        }
    }

    public void CloseButtonOnClick()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        dialogueCanvas.SetActive(false);

        requestData.messages.Add(new Messages { role = "system", content = "Goodbye" });
        requestData.messages.Add(new Messages { role = "user", content = $"Hello again {NPC.name}" });

        // Prevents obvious errors
        submitButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        //StopCoroutine(SendRequest());

        textField.text = "";
        canSumbit = true;

        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        player.GetComponent<PlayerController>().cursor_locked = true;
    }


    void SetNPCData()
    {
        string promptInstructions = "";

        try
        {
            promptInstructions = $"From now on act as if you are a NPC  in a {NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. Your name is {NPC.name} {(NPC.hasAge ? $"Age {NPC.age}, " : null)}{(!string.IsNullOrEmpty(NPC.gender) ? $"Gender {NPC.gender}" : null)}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $" {NPC.name} introduces their self with their name." : null)} {(!string.IsNullOrEmpty(NPC.backstory) ? $"Your backstory is: {NPC.name}: {NPC.backstory}." : null)} {NPC.name} replies: Human-like, as if {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "a stranger")} greeted you, in {NPC.language} and short. {NPC.name} doesn't ever: say their personality traits, {(NPC.assume_assitance ? $"assume {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "the stranger")} needs assitance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themself with their name" : null)} say \"{NPC.name}\", no Narrative text.{(!string.IsNullOrEmpty(NPC.whoIsTalking) ? $"You are greeted by {NPC.whoIsTalking}" : null)}";
        }
        catch
        {
            Debug.LogError("ScriptableObject GPT NPC is incorrect, go to Open API > Create new GPT NPC and follow instructions");
            enabled = false;
            return;
        }
        requestData.messages.Add(new Messages { role = "assistant", content = promptInstructions });
        
        if (!string.IsNullOrEmpty(NPC.whoIsTalking)) {
            requestData.messages.Add(new Messages { role = "user", content = $"I am {NPC.whoIsTalking}" });
        }

    }

    IEnumerator SendRequest()
    {
        if (!canSumbit)
            yield break;

        // Make sure User cannot spam request
        canSumbit = false;

        closeButton.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);

        // After NPC greeting, start storing user input
        if (requestData.messages.Count != 1 && !string.IsNullOrEmpty(inputField.text)){
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

        //print("Data sent");

        textField.text = "";
        inputField.text = "";

        slider.gameObject.SetActive(true);
        StartCoroutine(LoadingBar());

        // Send request and wait for response
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Convert data
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);
            
            Choices systemMessage = responseData.choices[0];
            string assistantReply = RemoveSpeechMarks(systemMessage.message.content);

            // Add NPC reponse to list of messages
            requestData.messages.Add(new Messages { role = "system", content = assistantReply });

            /* Testing purposes
             Debug.Log(www.downloadHandler.text);
             Debug.Log("Assistant: " + assistantReply);*/

            StartCoroutine(TypeText(assistantReply));
        }
        else
        {
            Debug.Log("Error sending request: " + www.error);
        }

        canSumbit = true;
    }

     IEnumerator LoadingBar()
    {
        slider.value = 0;
        int completeionInt = 2;
        float progress = Mathf.Clamp(www.uploadProgress + www.downloadProgress, 0 , completeionInt);
        while (progress < completeionInt)
        {
            progress = Mathf.Clamp(www.uploadProgress + www.downloadProgress, 0, completeionInt);
            slider.value = slider.value < progress ? slider.value + progress * .01f : slider.value;
            yield return new WaitForSeconds(.01f);
        }

        slider.value = completeionInt;
        slider.gameObject.SetActive(false);

    }

    IEnumerator TypeText(string text)
    {
        textField.text = "";
        foreach (char c in text)
        {
            textField.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        submitButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
    }

    public string RemoveSpeechMarks(string input)
    {
        if (input.Contains("\""))
        {
            input = input.Replace("\"", "");
        }
        return input;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position + Vector3.up * 2, "GPT_NPC Gizmo.png", true);
    }
}
