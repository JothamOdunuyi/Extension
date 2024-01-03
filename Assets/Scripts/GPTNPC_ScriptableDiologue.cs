using System;
using System.Collections;
using System.Collections.Generic;
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

}
