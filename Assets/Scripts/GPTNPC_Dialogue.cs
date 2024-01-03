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
    // Please don't request too much, but have fun! And obviously do not share this key
    private string apiKey = "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"; //OLD KEY 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
    private string gpt3Endpoint = "https://api.openai.com/v1/chat/completions";

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

    // We get these from dialogueCanvas
    private UnityEngine.UI.Button choice1Button;
    private UnityEngine.UI.Button choice2Button;
    private UnityEngine.UI.Button choice3Button;

    private TMP_Text errorMessageTMP;

    [SerializeField]
    private GPTNPC_ScriptableDiologue rootChoiceDiologue;

    [SerializeField]
    public bool saveDiologueHistory = false;

    private GPTNPC_ScriptableDiologue currentChoiceDiologue;

    private GameObject dialogueCanvas;
    private const float typingSpeed = 0.037f;
    private bool canSumbit = true;
    private UnityWebRequest www;
    private RequestData requestData = new RequestData();
    AudioManager audioManager;

    void Start()
    {
        // Error check
        if (!closeButton){
            Debug.LogError("GPTNPC_Dialogue MUST be created from extension, go to Open API > Create new GPT NPC and follow instructions");
            enabled = false;
            return;
        }

        dialogueCanvas = closeButton.transform.root.gameObject;

        choice1Button = dialogueCanvas.transform.Find("Choice1").GetComponent<Button>();
        choice2Button = dialogueCanvas.transform.Find("Choice2").GetComponent<Button>();
        choice3Button = dialogueCanvas.transform.Find("Choice3").GetComponent<Button>();

        errorMessageTMP = dialogueCanvas.transform.Find("ErrorMessage").GetComponent<TMP_Text>();

        choice1Button.gameObject.SetActive(false);
        choice2Button.gameObject.SetActive(false);
        choice3Button.gameObject.SetActive(false);

        dialogueCanvas.SetActive(false);
        requestData.messages = new List<Messages>();
        requestData.model = "gpt-3.5-turbo"; // Set the model
        audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();

        currentChoiceDiologue = rootChoiceDiologue;

        SetNPCData();
    }

    void GetResponse(int choiceNumber)
    {

        choice1Button.gameObject.SetActive(false);
        choice2Button.gameObject.SetActive(false);
        choice3Button.gameObject.SetActive(false);

        audioManager.PlaySound("Button Press");

        string userMsg;

        switch (choiceNumber)
        {
            case 1:
                userMsg = currentChoiceDiologue.choice1;
                currentChoiceDiologue = currentChoiceDiologue.choice1Port;
                break;
            case 2:
                userMsg = currentChoiceDiologue.choice2;
                currentChoiceDiologue = currentChoiceDiologue.choice2Port;
                break ;
            case 3:
                userMsg = currentChoiceDiologue.choice3;
                currentChoiceDiologue = currentChoiceDiologue.choice3Port;
                break;
            default:
                userMsg = string.Empty;
                break;
        }

        //if (currentChoiceDiologue == null)
        //{
        //    userMsg = userMsg + " Goodbye.";
        //    print($"Adding goodbye to currenChoiceDiologue since it is null");
        //}

        StartCoroutine(SendRequest(userMsg));
    }

    void DisplayChoices()
    {
        choice1Button.gameObject.SetActive(false);
        choice2Button.gameObject.SetActive(false);
        choice3Button.gameObject.SetActive(false);

        if (currentChoiceDiologue == null){
            // Could link choice1 button function to EXIT function
            return;
        }

        if (!string.IsNullOrEmpty(currentChoiceDiologue.choice1))
        {
            choice1Button.gameObject.SetActive(true);
            choice1Button.transform.GetChild(0).GetComponent<TMP_Text>().text = currentChoiceDiologue.choice1;
        }

        if (!string.IsNullOrEmpty(currentChoiceDiologue.choice2))
        {
            choice2Button.gameObject.SetActive(true);
            choice2Button.transform.GetChild(0).GetComponent<TMP_Text>().text = currentChoiceDiologue.choice2;
        }

        if (!string.IsNullOrEmpty(currentChoiceDiologue.choice3))
        {
            choice3Button.gameObject.SetActive(true);
            choice3Button.transform.GetChild(0).GetComponent<TMP_Text>().text = currentChoiceDiologue.choice3;
        }

    }
    // Start dialogue when player touches NPC
    private void OnCollisionEnter(Collision hit)
    {
        GameObject player = hit.gameObject;
        if (player.tag == "Player" && canSumbit)
        {
            player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            player.GetComponent<PlayerController>().cursor_locked = false;
            dialogueCanvas.SetActive(true);
            currentChoiceDiologue = rootChoiceDiologue;

            // Adds functions to listeners
            //submitButton.onClick.AddListener(() => GetResponse(1));
            closeButton.onClick.AddListener(() => CloseButtonOnClick());

            choice1Button.onClick.AddListener(() => GetResponse(1));
            choice2Button.onClick.AddListener(() => GetResponse(2));
            choice3Button.onClick.AddListener(() => GetResponse(3));

            StartCoroutine(SendRequest(string.Empty));
        }
    }

    // Reset what has to be and free player
    public void CloseButtonOnClick()
    {
        audioManager.PlaySound("Button Press");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        dialogueCanvas.SetActive(false);

        //requestData.messages.Add(new Messages { role = "system", content = "Goodbye" });
        if (saveDiologueHistory)
        {
            // Nevcer tried  the below but dont want to confuse the AI
            //requestData.messages.Add(new Messages { role = "assistant", content = "Stay in character" });
            
            requestData.messages.Add(new Messages { role = "user", content = $"Hello again {NPC.name}" });
            print("Keeping diologue history");
        }
        else
        {
            print("Clearing diologue history");
            requestData.messages.Clear();
            SetNPCData();
        }

        // Prevents obvious errors
        //submitButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        choice1Button.onClick.RemoveAllListeners();
        choice2Button.onClick.RemoveAllListeners();
        choice3Button.onClick.RemoveAllListeners();


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
            //promptInstructions = $"Role-play as {NPC.name}{(NPC.hasAge ? $",a {NPC.age}-year-old" : null)}{(!string.IsNullOrEmpty(NPC.gender) ? $" {NPC.gender}" : null)} in a{NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $" {NPC.name} introduces their self with their name." : null)} {(!string.IsNullOrEmpty(NPC.backstory) ? $"Your backstory is: {NPC.name}: {NPC.backstory}." : null)} {NPC.name} replies: Human-like, as if {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "a stranger")} greeted you, in {NPC.language} and short. {NPC.name} never does the following: say their personality traits, {(NPC.assume_assitance ? $"assume {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "the stranger")} needs assitance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themself with their name" : null)} say \"{NPC.name}\". Remember to never do these things.{(!string.IsNullOrEmpty(NPC.whoIsTalking) ? $"You are greeted by {NPC.whoIsTalking}" : null)}";
            promptInstructions = $"Role-play as {NPC.name}{(NPC.hasAge ? $", a {NPC.age}-year-old" : null)}{(string.IsNullOrEmpty(NPC.gender) ? null : $" {NPC.gender}")} in a{NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $" {NPC.name} introduces themselves without using their name." : null)} {(!string.IsNullOrEmpty(NPC.backstory) ? $"Your backstory is: {NPC.name}: {NPC.backstory}." : null)} {NPC.name} replies: Human-like, as if {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "a stranger")} greeted you, in {NPC.language} and short. {NPC.name} never does the following: say their personality traits, {(NPC.assume_assitance ? $"assume {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "the stranger")} needs assistance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themselves with their name" : null)} say \"{NPC.name}\". Remember to never do these things.{(!string.IsNullOrEmpty(NPC.whoIsTalking) ? $" You are greeted by {NPC.whoIsTalking}" : null)}";

            print(promptInstructions);
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
            print("Added extra diolgoue for user saying who is talking since its not nil");
        }

    }

    IEnumerator SendRequest(string userMsg)
    {
        // Make sure User cannot spam request
        if (!canSumbit)
            yield break;

        canSumbit = false;

        closeButton.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);

        // After NPC greeting, start storing user input
        if (requestData.messages.Count != 1 && !string.IsNullOrEmpty(userMsg)){
            requestData.messages.Add(new Messages { role = "user", content = userMsg });
        }
        if (string.IsNullOrEmpty(userMsg) || userMsg == null)
        {
            Debug.LogWarning("usrMsg is null or empty");
        }

        print($"USERMSG: {userMsg}");

        if (currentChoiceDiologue && !string.IsNullOrEmpty(currentChoiceDiologue.diologue))
        {
            requestData.messages.Add(new Messages { role = "assistant", content = $"Say the following as {NPC.name} :\"{currentChoiceDiologue.diologue}\"" });
            print($"Told NPC to say: {currentChoiceDiologue.diologue}");
        }
        else
        {
            requestData.messages.Add(new Messages { role = "assistant", content = $"Say the following as {NPC.name} :" });
            print("ADDED extra");
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

        // Reset input and AI reponse field
        textField.text = "";
        inputField.text = "";

        // Start loading UI
        slider.gameObject.SetActive(true);
        StartCoroutine(LoadingBar());

        while (www.result != UnityWebRequest.Result.Success)
        {

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

                // Type out reponse message
                StartCoroutine(TypeText(assistantReply));
            }
            else
            {
                Debug.LogWarning("Error sending request: " + www.error);

                errorMessageTMP.text = www.error;
                yield return new WaitForSeconds(2);

                errorMessageTMP.text = "Trying again in (5) seconds";
                yield return new WaitForSeconds(5);

                errorMessageTMP.text = "";
            }


            yield return null;
        }
       
        canSumbit = true;
    }

    IEnumerator LoadingBar()
    {
        slider.value = 0;
        int completeionInt = 2;

        //Clamps ypload and download data
        float progress = Mathf.Clamp(www.uploadProgress + www.downloadProgress, 0 , completeionInt);

        // Unfortunately download and uploader values are only ever 0 or 1 hence the overcomplicated slider loading
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

        //submitButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
        DisplayChoices();
    }

    public string RemoveSpeechMarks(string input)
    {
        if (input.Contains("\""))
        {
            input = input.Replace("\"", "");
        }
        return input;
    }

    // Draws GPT NPC Gizmo
    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position + Vector3.up * 2, "GPT_NPC Gizmo.png", true);
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
