using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[System.Serializable]
public class GPT_NPC_PresetDiologues
{
    public GPT_NPC NPC;
    public List<string> diologues;

    //public List<Sound> sounds;
}



[CreateAssetMenu(menuName = "GPT Diologue", fileName = "New Diologue")]
public class GPTNPC_ScriptableDiologue : ScriptableObject
{

    [Header("Parent Object")]
    public GPTNPC_ScriptableDiologue parentPort;

    [Space(10)]
    [Header("Diologue Configuration")]
    [TextArea]
    public string diologue;

    [Space(10)]
    public List<GPT_NPC_PresetDiologues> presetDiologues;

    public Dictionary<GPT_NPC, GPT_NPC_PresetDiologues> presetDiologuesDictonary = new Dictionary<GPT_NPC, GPT_NPC_PresetDiologues>();
   
    [HideInInspector]
    public bool reset_presetDiologuesDictonary = false;
    private bool addingToDict = false;

    [Space(10)]
    [Header("Choices")]
    public GPTNPC_ScriptableDiologue choice1Port;
    public string choice1;

    [Space(10)]
    public GPTNPC_ScriptableDiologue choice2Port;
    public string choice2;

    [Space(10)]
    public GPTNPC_ScriptableDiologue choice3Port;
    public string choice3;

   
    public GPT_NPC_PresetDiologues GetPresetDiologue(GPT_NPC needle)
    {
        foreach (GPT_NPC_PresetDiologues item in presetDiologues)
        {
            if (item.NPC == needle)
            {
               return item;
            }
        }
        Debug.LogWarning($"Could not find {needle.name} in diologues");
        return null;
    }

    // Not using as it would cause more complications than convience
    // Prevents having to run a foreach when seraching EVERY time
    public void FillPresetDiologuesDictonary()
    {
        if (reset_presetDiologuesDictonary == true || addingToDict == true) { Debug.Log("One of the value was tre"); return; }
        addingToDict = true;

        // Prevents mishaps in case things were changed, as ScriptableObjects save their values
        presetDiologuesDictonary.Clear();

        foreach (GPT_NPC_PresetDiologues item in presetDiologues)
        {
            presetDiologuesDictonary.Add(item.NPC, item);
        }

        reset_presetDiologuesDictonary = true;

        foreach (KeyValuePair<GPT_NPC, GPT_NPC_PresetDiologues> kvp in presetDiologuesDictonary)
        {
            Debug.Log("Key: " + kvp.Key.name);
        }

        addingToDict = false;
    }

}
