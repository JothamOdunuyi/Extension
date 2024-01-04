using log4net;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PresetDiologuesEditorScript : EditorWindow
{


    private string apiKey = "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"; //OLD KEY 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
    private string gpt3Endpoint = "https://api.openai.com/v1/chat/completions";

    private GPT_NPC gptNpc;
    private GPTNPC_ScriptableDiologue selectedDialogue;

    private UnityWebRequest www;
    private RequestData requestData = new RequestData();
    bool canSumbit = true;

    private GPT_NPC_PresetDiologues found = null;

    private int promptAmount = 1;
    private int maxPromptAmount = 10;

    private float progress = 0f;
    // This value is due to Loading data only ever being 0, 0.5 or 1
    private float fakeProgress = 0f;

    private bool generateForAllConnectedDiologues = false;

    private List<GPT_NPC_PresetDiologues> foundDiologuesToGenerateInto;

    [MenuItem("Open AI/Preset Diologue Generator")]
    public static void ShowWindow()
    {
        PresetDiologuesEditorScript window = GetWindow<PresetDiologuesEditorScript>("Preset Diologue Generator");
        window.minSize = new Vector2(400, 300);
        window.maxSize = new Vector2(800, 800);
    }

    private void OnGUI()
    {
        GUILayout.Label("Hello, this is a custom editor window!");

        // Drag and drop area for ScriptableObject
        GUILayout.Space(10);
        GUILayout.Label("Drag your GPT_NPC ScriptableObject here:");

        gptNpc = (GPT_NPC)EditorGUILayout.ObjectField(gptNpc, typeof(GPT_NPC), false);

        if (gptNpc != null)
        {
            GUILayout.Label("You've dragged in a GPT_NPC ScriptableObject!");

            // Additional information or actions related to the GPT_NPC can be added here
        }

        GUILayout.Space(10);

        // Display the ObjectField for selecting a ScriptableObject
        selectedDialogue = (GPTNPC_ScriptableDiologue)EditorGUILayout.ObjectField("Select GPTNPC_ScriptableDiologue", selectedDialogue, typeof(GPTNPC_ScriptableDiologue), false);

        // Check if the selected object is of the correct type
        if (selectedDialogue != null)
        {
            if (!(selectedDialogue is GPTNPC_ScriptableDiologue))
            {
                Debug.LogError("Invalid selection. Please choose a GPTNPC_ScriptableDiologue instance.");
                selectedDialogue = null; // Reset the selection if it's not of the correct type
            }
            else
            {
                // Valid selection, you can access the GPTNPC_ScriptableDiologue instance through 'selectedDialogue'
                //Debug.Log("Selected GPTNPC_ScriptableDiologue: " + selectedDialogue.name);
            }
        }

        GUILayout.Space(10);
        // Input field for the user to enter a number
        promptAmount = EditorGUILayout.IntField($"Enter a number (1-{maxPromptAmount}):", promptAmount);

        promptAmount = Mathf.Clamp(promptAmount, 1, maxPromptAmount);

        GUILayout.Space(10);

        generateForAllConnectedDiologues = EditorGUILayout.Toggle("Generate for connected Diologues", generateForAllConnectedDiologues);

        GUILayout.Space(10);


        if (GUILayout.Button("Press Me"))
        {
            if (selectedDialogue == null) {GetWindow<LogWindow>("Log Window").LogError("Please select a Diologue (GPT_NPC_PresetDiologues)"); return; }
            if (gptNpc == null) { GetWindow<LogWindow>("Log Window").LogError("Please select a NPC (GPT_NPC)"); return; }

            if (requestData.messages != null)
            {
                requestData.messages.Clear();
            }
            else
            {
                requestData.messages = new List<Messages>();
                requestData.model = "gpt-3.5-turbo";
            }

            found = null;

            foreach (GPT_NPC_PresetDiologues preset in selectedDialogue.presetDiologues)
            {
                if (preset.NPC == gptNpc)
                {
                    found = preset;
                }
            }

            // Add a new element to the list
            if (found == null)
            {
                selectedDialogue.presetDiologues.Add(new GPT_NPC_PresetDiologues { NPC = gptNpc, diologues = new List<string>() });
                found = selectedDialogue.presetDiologues[selectedDialogue.presetDiologues.Count - 1];
            }


            //found.diologues.Add("NEW THING POG");

            SetNPCData();
            if (generateForAllConnectedDiologues)
            {
                foundDiologuesToGenerateInto = new List<GPT_NPC_PresetDiologues>();
                GenerateForConnectedDiologues(selectedDialogue);
            }
            else
            {
                AddAssitantMessageToSayPrompt(selectedDialogue);
            }
           
            Request();


        }

        //Object[] selectedObjects = Selection.objects;


        //foreach (Object selectedObject in selectedObjects)
        //{
        //    Debug.Log("Selected Object: " + selectedObject.name);
        //}

        // Simulate loading progress


    }

    // This will add a assitant message telling GPT to say the prompt for all connected diologues
    void GenerateForConnectedDiologues(GPTNPC_ScriptableDiologue currentDiologue)
    {
        AddAssitantMessageToSayPrompt(currentDiologue);


        found = null;

        foreach (GPT_NPC_PresetDiologues preset in selectedDialogue.presetDiologues)
        {
            if (preset.NPC == gptNpc)
            {
                found = preset;
            }
        }

        // Add a new element to the list
        if (found == null)
        {
            selectedDialogue.presetDiologues.Add(new GPT_NPC_PresetDiologues { NPC = gptNpc, diologues = new List<string>() });
            found = selectedDialogue.presetDiologues[selectedDialogue.presetDiologues.Count - 1];
        }

        foundDiologuesToGenerateInto.Add(found);

        Debug.Log($"Added Message for \"{currentDiologue.name}\"");

        if (currentDiologue.choice1Port != null)
        {
            GenerateForConnectedDiologues(currentDiologue.choice1Port);
        }

        if (currentDiologue.choice2Port != null)
        {
            GenerateForConnectedDiologues(currentDiologue.choice2Port);
        }

        if (currentDiologue.choice3Port != null)
        {
            GenerateForConnectedDiologues(currentDiologue.choice3Port);
        }

    }

    void AddAssitantMessageToSayPrompt(GPTNPC_ScriptableDiologue sentDiologue)
    {
        requestData.messages.Add(new Messages { role = "assistant", content = $"Say the following but differently {Mathf.Clamp(promptAmount, 1, maxPromptAmount)} times:\"{sentDiologue.diologue}\"" });
        Debug.Log($"Say the following but differently {Mathf.Clamp(promptAmount, 1, maxPromptAmount)} times:\"{sentDiologue.diologue}\"");
    }

    void SetNPCData()
    {
        GPT_NPC NPC = gptNpc;

        string promptInstructions = "";

        try
        {
            //promptInstructions = $"Role-play as {NPC.name}{(NPC.hasAge ? $",a {NPC.age}-year-old" : null)}{(!string.IsNullOrEmpty(NPC.gender) ? $" {NPC.gender}" : null)} in a{NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $" {NPC.name} introduces their self with their name." : null)} {(!string.IsNullOrEmpty(NPC.backstory) ? $"Your backstory is: {NPC.name}: {NPC.backstory}." : null)} {NPC.name} replies: Human-like, as if {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "a stranger")} greeted you, in {NPC.language} and short. {NPC.name} never does the following: say their personality traits, {(NPC.assume_assitance ? $"assume {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "the stranger")} needs assitance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themself with their name" : null)} say \"{NPC.name}\". Remember to never do these things.{(!string.IsNullOrEmpty(NPC.whoIsTalking) ? $"You are greeted by {NPC.whoIsTalking}" : null)}";
            promptInstructions = $"Role-play as {NPC.name}{(NPC.hasAge ? $", a {NPC.age}-year-old" : null)}{(string.IsNullOrEmpty(NPC.gender) ? null : $" {NPC.gender}")} in a{NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $" {NPC.name} introduces themselves without using their name." : null)} {(!string.IsNullOrEmpty(NPC.backstory) ? $"Your backstory is: {NPC.name}: {NPC.backstory}." : null)} {NPC.name} replies: Human-like, as if {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "a stranger")} is talking, in {NPC.language} and short. {NPC.name} never does the following: say their personality traits, {(NPC.assume_assitance ? $"assume {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "the stranger")} needs assistance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themselves with their name" : null)} say \"{NPC.name}\". Remember to never do these things.{(!string.IsNullOrEmpty(NPC.whoIsTalking) ? $" You are greeted by {NPC.whoIsTalking}" : null)}";

        }
        catch
        {
            Debug.LogError("ScriptableObject GPT NPC is incorrect, go to Open API > Create new GPT NPC and follow instructions");
            //enabled = false;
            return;
        }

        requestData.messages.Add(new Messages { role = "assistant", content = promptInstructions });

        if (!string.IsNullOrEmpty(NPC.whoIsTalking))
        {
            requestData.messages.Add(new Messages { role = "user", content = $"I am {NPC.whoIsTalking}" });
        }


        //Debug.Log($"Say the following as {NPC.name} :\"{selectedDialogue.diologue}\"");


    }

    // Send API request
    private void Request()
    {
        // Make sure User cannot spam request
        if (!canSumbit)
            return;

        canSumbit = false;

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

        fakeProgress = 0f;
        // Due to now Update() we use a delegate
        EditorApplication.update += WaitForRequest;
    }

    // Wait, then output request
    private void WaitForRequest()
    {
        if (!canSumbit)
        {

            // Show Loading Bar
            if (!www.isDone)
            {
                // This value is due to Loading data only ever being 0, 0.5 or 1
                fakeProgress += 0.005f;
                progress = Mathf.Clamp01((www.downloadProgress + www.uploadProgress) / 2 + fakeProgress);

                if (www.uploadProgress == 0)
                {
                    EditorUtility.DisplayProgressBar("Requesting API...", $"Progress: {progress * 100}%", Mathf.Clamp(progress, 0, 90));
                }
                else if (www.downloadProgress == 0)
                {
                    EditorUtility.DisplayProgressBar("Waiting for API...", $"Progress: {progress * 100}%", Mathf.Clamp(progress, 0, 90));
                }

                return;
            }

            EditorUtility.DisplayProgressBar("Completed API Request", "Progress: 100% ", 100);

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Convert data
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);

                Choices systemMessage = responseData.choices[0];
                string assistantReply = RemoveSpeechMarks(systemMessage.message.content);

                // Adds AI's reponse to message history
                requestData.messages.Add(new Messages { role = "system", content = assistantReply });

                // Add Loading here too
                if (generateForAllConnectedDiologues)
                {
                    Debug.Log(assistantReply);

                    string[] lines = assistantReply.Split('\n');

                    int tempAmount = promptAmount;
                    int indexToAddTo = 0;
                    int i = 0; // used as the actual index

                    // Could just do it for total legnth in lines
                    for (int j = 0; j < promptAmount * foundDiologuesToGenerateInto.Count; j++)
                    {
                        i += 1;
                        foundDiologuesToGenerateInto[indexToAddTo].diologues.Add(lines[j]);
                        Debug.Log($"Added : {lines[j]} to {foundDiologuesToGenerateInto[indexToAddTo].NPC.name}");
                        if(i == promptAmount)
                        {
                            i = 0;
                            indexToAddTo += 1;
                            Debug.Log($"Reset i, now adding to line {indexToAddTo}");
                        }
                    }

                }
                else
                {
                    string[] lines = assistantReply.Split('\n');

                    foreach (string line in lines)
                    {
                        found.diologues.Add(line);
                    }

                }

                canSumbit = true;

                // Displays the updated conversation without having to wait for OnGUI
                //Repaint();
            }
            else
            {
                Debug.LogWarning(www.error);
                Debug.Log(requestData.messages.Count);
                foreach (var item in requestData.messages)
                {
                    Debug.Log(item);
                }
                GetWindow<LogWindow>("Log Window").LogError(www.error);
            }

            // Remove from delegate
            EditorApplication.update -= WaitForRequest;
        }
        else
        {
            EditorApplication.update -= WaitForRequest;
            GetWindow<LogWindow>("Log Window").LogWarning("You are already in the middle of requesting");
        }

        // Close loading bar
        EditorUtility.ClearProgressBar();
    }


    private string RemoveSpeechMarks(string input)
    {
        if (input.Contains("\""))
        {
            input = input.Replace("\"", "");
        }
        return input;
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