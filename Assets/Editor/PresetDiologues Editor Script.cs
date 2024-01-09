using Codice.CM.Common.Tree;
using log4net;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
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


    // Generation in All diolgoue Variables
    private List<GPTNPC_ScriptableDiologue> foundDiologuesToGenerateInto;
    private List<GPT_NPC_PresetDiologues> foundPresetDiologuesToGenerateInto;

    private bool findingDiologueToGenerateInto = false;
    private bool generateTickCalledRequest = false;
    private bool generateForAllConnectedDiologues = false;
    private bool generateTickBusy = false;
    private int GTi = -1; // Generate Tick Index
    private float generateTickLoadPreviousTime; //used to create a DeltaTime
    private float generateTickLoadProgress;


    // Folder + New Diologue creation variables
    string newDiologueName = "";
    string folderName = "";

    [MenuItem("Open AI/Preset Diologue Generator")]
    public static void ShowWindow()
    {
        PresetDiologuesEditorScript window = GetWindow<PresetDiologuesEditorScript>("Preset Diologue Generator");
        window.minSize = new Vector2(400, 300);
        window.maxSize = new Vector2(800, 555);
    }

    #region OnGUI Functions
    
    void SetActiveObj(UnityEngine.Object obj)
    {
        Selection.activeObject = selectedDialogue;

        // Refresh Inspector
        EditorUtility.SetDirty(selectedDialogue);
    }

    LogWindow GetLogWindow()
    {
        return GetWindow<LogWindow>("Log Window");
    }

    void SetFound(GPTNPC_ScriptableDiologue currentDiologue)
    {
        found = null;

        foreach (GPT_NPC_PresetDiologues preset in currentDiologue.presetDiologues)
        {
            if (preset.NPC == gptNpc)
            {
                found = preset;
            }
        }

        // Add a new element to the list
        if (found == null)
        {
            currentDiologue.presetDiologues.Add(new GPT_NPC_PresetDiologues { NPC = gptNpc, diologues = new List<string>() });
            found = currentDiologue.presetDiologues[currentDiologue.presetDiologues.Count - 1];
        }
    }

    void NewDiologueGenerationGUI()
    {

        if (string.IsNullOrEmpty(newDiologueName))
        {
            GUILayout.Label("Create new Diolgoue Presets here:");

        }

        newDiologueName = EditorGUILayout.TextField($"Preset Diologue Name", newDiologueName);

        if (string.IsNullOrEmpty(folderName))
        {
            GUILayout.Label("You can create or re-direct the new diologue in a folder here:");
        }

        folderName = EditorGUILayout.TextField($"Folder Name", folderName);


        MakeSpace();

        if (GUILayout.Button("Create new Diolgoue"))
        {
            if (string.IsNullOrEmpty(newDiologueName)) { GetLogWindow().LogError("Please type a name for your new Diologue"); return; }

            string assetPathAndName;
            string folderPath = string.Empty;

            GPTNPC_ScriptableDiologue newDiologue = ScriptableObject.CreateInstance<GPTNPC_ScriptableDiologue>();

            if (!string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder($"Assets/Diologue ScriptableObjects", folderName);
                folderPath = $"{folderName}/";
            }

            // Set the asset path and name
            assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"Assets/Diologue ScriptableObjects/{folderPath}{newDiologueName}.asset");

            // Create the ScriptableObject asset
            AssetDatabase.CreateAsset(newDiologue, assetPathAndName);

            // Save any changes to the asset
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            selectedDialogue = newDiologue;

            SetActiveObj(selectedDialogue);

            Debug.Log("Created new Preset Diologue");
        }
    }

    void ExtraGPTNPCInfoGUI()
    {
        if (gptNpc != null)
        {
            string npcName = gptNpc.name;
            string npcGender = gptNpc.gender;
            string npcPersonality = gptNpc.personality;
            string npcBackstory = gptNpc.backstory;

            string npcData = $"<b>Name:</b> {npcName}\n\n<b>Gender:</b> {npcGender}\n\n<b>Personality:</b> {npcPersonality}\n\n<b>Backstory:</b> {npcBackstory}";

            EditorGUILayout.LabelField(npcData, new GUIStyle(EditorStyles.textArea) { richText = true});
      
        }
    }
    
    void MakeSpace()
    {
        GUILayout.Space(10);
    }

    void GenerateDiologueUI()
    {
        if (GUILayout.Button("Generate Diologue"))
        {
            if (selectedDialogue == null) { GetLogWindow().LogError("Please select a Diologue (GPT_NPC_PresetDiologues)"); return; }
            if (gptNpc == null) { GetLogWindow().LogError("Please select a NPC (GPT_NPC)"); return; }
            if (string.IsNullOrEmpty(selectedDialogue.diologue)) { GetLogWindow().LogError($"Diologue in \"{selectedDialogue.name}\" (GPT_NPC_PresetDiologues) cannot be empty!"); return; }
            
            if (requestData.messages != null)
            {
                requestData.messages.Clear();
            }
            else
            {
                requestData.messages = new List<Messages>();
                requestData.model = "gpt-3.5-turbo";
            }

            SetFound(selectedDialogue);

            // Clamp Visually (No gurantee this runs before Generate Button is pressed)
            promptAmount = Mathf.Clamp(promptAmount, 1, maxPromptAmount);

            SetNPCData();

            if (generateForAllConnectedDiologues)
            {
                foundDiologuesToGenerateInto = new List<GPTNPC_ScriptableDiologue>();
                foundPresetDiologuesToGenerateInto = new List<GPT_NPC_PresetDiologues>();
                findingDiologueToGenerateInto = false;
                GTi = -1;

                GenerateForConnectedDiologues(selectedDialogue);
            }
            else
            {
                AddAssitantMessageToSayPrompt(selectedDialogue);
                Request();
            }


        }
    }

    void SelectedNPCPresetDiologuesInfo()
    {
        if (selectedDialogue != null)
        {
            if (gptNpc != null && selectedDialogue.presetDiologues != null)
            {
                foreach (GPT_NPC_PresetDiologues preset in selectedDialogue.presetDiologues)
                {
                    if (preset.NPC == gptNpc)
                    {
                        GUILayout.Label($"{gptNpc.name} has {preset.diologues.Count} preset diologue(s).");
                    }
                }
            }
        }
    }
    #endregion

    // Draws the window
    private void OnGUI()
    {
        GUILayout.Label("Drag your GPT_NPC ScriptableObject here:");

        gptNpc = (GPT_NPC)EditorGUILayout.ObjectField(gptNpc, typeof(GPT_NPC), false);

        // Displays addtional info about the NPC
        ExtraGPTNPCInfoGUI();

        MakeSpace();

        // Display the ObjectField for selecting the Preset Diologue SO
        selectedDialogue = (GPTNPC_ScriptableDiologue)EditorGUILayout.ObjectField("Select Preset Diologue", selectedDialogue, typeof(GPTNPC_ScriptableDiologue), false);

        // Displays how many preset diologues the selected NPC has already
        SelectedNPCPresetDiologuesInfo();

        MakeSpace();

        NewDiologueGenerationGUI();

        MakeSpace();

        GUILayout.Label("Choose how many prompts you wanted generated here:");

        promptAmount = EditorGUILayout.IntField($"Enter a number (1-{maxPromptAmount}):", Mathf.Clamp(promptAmount, 1, 5));

        MakeSpace();

        generateForAllConnectedDiologues = EditorGUILayout.Toggle("Generate for ALL Diologue", generateForAllConnectedDiologues);

        MakeSpace();

        GenerateDiologueUI();

    }

    #region All Diolgoue Generation Functions
    // Loops through all the diologues and adds information needed for the requests to specified lists
    void GenerateForConnectedDiologues(GPTNPC_ScriptableDiologue currentDiologue)
    {
        bool firstThread = false; // Checks if this is the first thread

        // The first thread is the last thread to run (See more under)
        if (!findingDiologueToGenerateInto)
        {
            findingDiologueToGenerateInto = true;
            firstThread = true;
        }

        SetFound(currentDiologue);

        foundDiologuesToGenerateInto.Add(currentDiologue);
        foundPresetDiologuesToGenerateInto.Add(found);

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

        // This will allow the final ran thread (the first) to start the API requests ONLY after all diolgoues has been added to lists
        if (firstThread)
        {
            EditorApplication.update += GenerateTick;
        }

    }

    // This is the loop for requesting diologues when generate in all dio is toggled
    void GenerateTick()
    {
        if (generateTickBusy) { return; }

        if (!generateTickCalledRequest)
        {
            generateTickCalledRequest = true;
            EditorUtility.ClearProgressBar(); // incase GenerateTickLoad has a thread "overlap", this will clear the loading bar
            GTi += 1; // Has to be here bc this will run BEFORE WaitRequest as it was added earlier to the delegate
            
            // Check if the end has been reached, and prevent any more quests
            if(GTi > foundDiologuesToGenerateInto.Count - 1) { 
                EditorApplication.update -= GenerateTick; 
                GetLogWindow().Log("Generation Complete!");
                generateTickCalledRequest = false;
                canSumbit = true;
                return; 
            }

            AddAssitantMessageToSayPrompt(foundDiologuesToGenerateInto[GTi]);
            Request();
        }
        else
        {
            // Handles success and error of the sent request
            if(www.isDone && !generateTickBusy) // prevents unwanted repeat due to later threads
            {
                // Remove previous prompt diologue and allow the request to be sent again
                if(www.result == UnityWebRequest.Result.Success)
                {
                    generateTickBusy = true; // prevents unwanted repeat due to later threads
                    requestData.messages.RemoveAt(requestData.messages.Count - 1); // remove assitant message for that diologue to replace it later
                    generateTickCalledRequest = false;
                    generateTickBusy = false;
                    canSumbit = true;
                }
                else
                {
                    // Start to wait before next request
                    generateTickBusy = true;
                    Debug.Log("Waiting, since request had an error.");
                    EditorApplication.update -= GenerateTick;
                    generateTickLoadPreviousTime = (float)EditorApplication.timeSinceStartup;
                    EditorApplication.update += GenerateTickLoad;
                }
            } 

        }
    }

    // Wait 10 seconds and create a loading bar before requesting again (This happens when a "Generate in all dio" request has a error)
    void GenerateTickLoad()
    {
        // Calculate deltaTime
        float currentTime = (float)EditorApplication.timeSinceStartup;
        float deltaTime = currentTime - generateTickLoadPreviousTime;

        // Update the previous time for the next frame
        generateTickLoadPreviousTime = currentTime;

        generateTickLoadProgress += 1 * deltaTime;

        EditorUtility.DisplayProgressBar("Retrying API Request after some waiting...", "", generateTickLoadProgress * 10); // wait time is 10 seconds so we multiple by 10 here

        // Wait for 10 seconds then attempt to request API agian
        if(generateTickLoadProgress >= 10)
        {
            EditorApplication.update -= GenerateTickLoad;
            generateTickBusy = false;
            generateTickCalledRequest = false;
            EditorApplication.update += GenerateTick;
        }
    }

    #endregion

    #region Prompt Functions

    // Prompt for the NPC's persanlity, background etc
    void SetNPCData()
    {
        GPT_NPC NPC = gptNpc;

        string promptInstructions = "";

        try
        {
            promptInstructions = $"Role-play as {NPC.name}{(NPC.hasAge ? $", a {NPC.age}-year-old" : null)}{(string.IsNullOrEmpty(NPC.gender) ? null : $" {NPC.gender}")} in a{NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $" {NPC.name} introduces themselves without using their name." : null)} {(!string.IsNullOrEmpty(NPC.backstory) ? $"Your backstory is: {NPC.name}: {NPC.backstory}." : null)} {NPC.name} replies: Human-like, as if {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "a stranger")} is talking, in {NPC.language} and short. {NPC.name} never does the following: say their personality traits, {(NPC.assume_assitance ? $"assume {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "the stranger")} needs assistance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themselves with their name" : null)} say \"{NPC.name}\". Remember to never do these things.{(!string.IsNullOrEmpty(NPC.whoIsTalking) ? $" You are greeted by {NPC.whoIsTalking}" : null)}";

        }
        catch
        {
            Debug.LogError("ScriptableObject GPT NPC is incorrect, go to Open API > Create new GPT NPC and follow instructions");
            return;
        }

        requestData.messages.Add(new Messages { role = "assistant", content = promptInstructions });

        if (!string.IsNullOrEmpty(NPC.whoIsTalking))
        {
            requestData.messages.Add(new Messages { role = "user", content = $"I am {NPC.whoIsTalking}" });
        }

    }

    // Leaving this for the future (I was attempting to hvae it say everything in every diologue but for one prompt)
    void AddForMutlipleAssitantMessageToSayPrompt(GPTNPC_ScriptableDiologue sentDiologue)
    {
        requestData.messages.Add(new Messages { role = "assistant", content = $"Say the following differently: {Mathf.Clamp(promptAmount, 1, maxPromptAmount)} times:\"{sentDiologue.diologue}\"" });
        Debug.Log($"Please provide variations for the following sentences: {Mathf.Clamp(promptAmount, 1, maxPromptAmount)} times:\"{sentDiologue.diologue}\"");
    }

    // Adds what the NPC should say
    void AddAssitantMessageToSayPrompt(GPTNPC_ScriptableDiologue sentDiologue)
    {
        requestData.messages.Add(new Messages { role = "assistant", content = $"Say the following but differently {Mathf.Clamp(promptAmount, 1, maxPromptAmount)} times:\"{sentDiologue.diologue}\"" });
    }

    #endregion

    #region API Request Handles
    // Send API request
    private void Request()
    {
        // Make sure User cannot spam request
        if (!canSumbit)
        {
            GetLogWindow().LogWarning("You are already in the middle of requesting");
            return;
        }

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

        www.SendWebRequest();

        // Reset fake prog
        fakeProgress = 0f;

        Debug.Log("Requested");
        // Due to no Update() or availble use of coroutines, we use a delegate
        EditorApplication.update += WaitForRequest;
    }

    // Wait, then output request
    private void WaitForRequest()
    {
        if (!canSumbit)
        {

            // Show Loading Bar while request is waiting
            if (!www.isDone)
            {
                Debug.Log("Not done");
                // This value is due to? Loading data only ever being 0, 0.5 or 1
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
            Debug.Log($"RESULT: {www.result}");
            if (www.result == UnityWebRequest.Result.Success)
            {

                EditorUtility.DisplayProgressBar("Adding Diologue...", "Converting Data", 0);
                // Convert data
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);

                Choices systemMessage = responseData.choices[0];
                string assistantReply = RemoveSpeechMarks(systemMessage.message.content);

                EditorUtility.DisplayProgressBar("Adding Diologue...", "Adding Diologue", 50);

                Debug.Log(assistantReply);
                Debug.Log("GEENRATION: " + generateForAllConnectedDiologues);

                if (generateForAllConnectedDiologues)
                {

                    int timesAdded = 0;
                    string[] lines = assistantReply.Split('\n');

                    foreach (string line in lines)
                    {
                        timesAdded += 1;
                        //Debug.Log($"GTi: {GTi} is {foundDiologuesToGenerateInto[GTi].name} ");
                        Debug.Log($"GTi: {GTi}, adding line: {line} \nTO DIOLOGUE: {foundDiologuesToGenerateInto[GTi].diologue}");
                        foundPresetDiologuesToGenerateInto[GTi].diologues.Add(line);
                    }

                    // Log a warning if there weren't as much lines as the user expected
                    CheckForTokenLogWarning(timesAdded);

                }
                else
                {
                    int timesAdded = 0;
                    string[] lines = assistantReply.Split('\n');

                    foreach (string line in lines)
                    {
                        timesAdded += 1;
                        found.diologues.Add(line);
                    }

                    // Log a warning if there weren't as much lines as the user expected
                    CheckForTokenLogWarning(timesAdded);

                }


            }
            else
            {

                Debug.LogWarning(www.error);
                Debug.Log(requestData.messages.Count);
                //foreach (var item in requestData.messages)
                //{
                //    Debug.Log($"Role: {item.role} Content: {item.content}");
                //}
                GetLogWindow().LogError(www.error);
            }

            EditorUtility.DisplayProgressBar("Complete", "", 100);

        }

        // remove from delegate
        EditorApplication.update -= WaitForRequest;

        // Close loading bar
        EditorUtility.ClearProgressBar();

        if (!generateForAllConnectedDiologues) { canSumbit = true; }
    }

    #endregion

    #region Request Helper Functions

    private string RemoveSpeechMarks(string input)
    {
        if (input.Contains("\""))
        {
            input = input.Replace("\"", "");
        }
        return input;
    }

    // A warning for when there could be a Token issue
    void CheckForTokenLogWarning(int timesAdded)
    {
        // Log a warning if there weren't as much lines as the user expected
        if (timesAdded < promptAmount)
        {
            GetLogWindow().LogWarning($"For DiologuePreset \"{foundDiologuesToGenerateInto[GTi].name}\" the token count was too high to generate {promptAmount} diologues!\n Considering shortening the diologue or waiting a couple minutes for more tokens.");
        }
    }

    #endregion

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