using log4net;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class PresetDiologuesEditorScript : EditorWindow
{


    private string apiKey = "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"; //OLD KEY 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
    private string gpt3Endpoint = "https://api.openai.com/v1/chat/completions";

    private GPT_NPC gptNpc;
    private GPTNPC_ScriptableDiologue selectedDialogue;

    private UnityWebRequest www;
    private RequestData requestData = new RequestData();

    [MenuItem("Open AI/Preset Diologue Generator")]
    public static void ShowWindow()
    {
        GetWindow<PresetDiologuesEditorScript>("Custom Window");
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


        if (GUILayout.Button("Press Me"))
        {
            //Debug.Log("Button pressed!");
            GPT_NPC_PresetDiologues found = null;

            foreach (GPT_NPC_PresetDiologues item in selectedDialogue.presetDiologues)
            {
                if(item.NPC == gptNpc)
                {
                    found = item;
                }
            }

            // Add a new element to the list
            if (found == null){
                selectedDialogue.presetDiologues.Add(new GPT_NPC_PresetDiologues { NPC = gptNpc, diologues = new List<string>() });
                found = selectedDialogue.presetDiologues[selectedDialogue.presetDiologues.Count - 1];
                Debug.Log("Did not find");
            }
            else
            {
                Debug.Log("Did find");
            }

            //found.diologues.Add("NEW THING POG");

            SetNPCData();






        }

    }

    void SetNPCData()
    {
        GPT_NPC NPC = gptNpc;
        string promptInstructions = "";

        try
        {
            //promptInstructions = $"Role-play as {NPC.name}{(NPC.hasAge ? $",a {NPC.age}-year-old" : null)}{(!string.IsNullOrEmpty(NPC.gender) ? $" {NPC.gender}" : null)} in a{NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $" {NPC.name} introduces their self with their name." : null)} {(!string.IsNullOrEmpty(NPC.backstory) ? $"Your backstory is: {NPC.name}: {NPC.backstory}." : null)} {NPC.name} replies: Human-like, as if {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "a stranger")} greeted you, in {NPC.language} and short. {NPC.name} never does the following: say their personality traits, {(NPC.assume_assitance ? $"assume {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "the stranger")} needs assitance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themself with their name" : null)} say \"{NPC.name}\". Remember to never do these things.{(!string.IsNullOrEmpty(NPC.whoIsTalking) ? $"You are greeted by {NPC.whoIsTalking}" : null)}";
            promptInstructions = $"Role-play as {NPC.name}{(NPC.hasAge ? $", a {NPC.age}-year-old" : null)}{(string.IsNullOrEmpty(NPC.gender) ? null : $" {NPC.gender}")} in a{NPC.world_setting} world{(!string.IsNullOrEmpty(NPC.world_name) ? $" called {NPC.world_name}" : null)}. {NPC.name} is:{(!string.IsNullOrEmpty(NPC.job) ? $" a {NPC.job}" : null)}{(!string.IsNullOrEmpty(NPC.location) ? $" currently in a {NPC.location}," : null)} {NPC.personality}.{(NPC.name_introduction ? $" {NPC.name} introduces themselves without using their name." : null)} {(!string.IsNullOrEmpty(NPC.backstory) ? $"Your backstory is: {NPC.name}: {NPC.backstory}." : null)} {NPC.name} replies: Human-like, as if {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "a stranger")} greeted you, in {NPC.language} and short. {NPC.name} never does the following: say their personality traits, {(NPC.assume_assitance ? $"assume {(!string.IsNullOrEmpty(NPC.whoIsTalking) ? NPC.whoIsTalking : "the stranger")} needs assistance and ask if they need it{(NPC.name_introduction ? "," : null)}" : null)} {(!NPC.name_introduction ? "introduce themselves with their name" : null)} say \"{NPC.name}\". Remember to never do these things.{(!string.IsNullOrEmpty(NPC.whoIsTalking) ? $" You are greeted by {NPC.whoIsTalking}" : null)}";
           
        }
        catch
        {
            Debug.LogError("ScriptableObject GPT NPC is incorrect, go to Open API > Create new GPT NPC and follow instructions");
            //enabled = false;
            return;
        }

        Debug.Log(promptInstructions);
        requestData.messages.Add(new Messages { role = "assistant", content = promptInstructions });

        if (!string.IsNullOrEmpty(NPC.whoIsTalking))
        {
            requestData.messages.Add(new Messages { role = "user", content = $"I am {NPC.whoIsTalking}" });
            Debug.Log("Added extra diolgoue for user saying who is talking since its not nil");
        }

        requestData.messages.Add(new Messages { role = "assistant", content = $"Say the following as {NPC.name} :\"{selectedDialogue.diologue}\"" });

        Debug.Log($"Say the following as {NPC.name} :\"{selectedDialogue.diologue}\"");


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
