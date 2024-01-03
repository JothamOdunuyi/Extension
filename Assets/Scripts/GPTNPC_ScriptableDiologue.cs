using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GPT Diologue", fileName = "New Diologue")]
public class GPTNPC_ScriptableDiologue : ScriptableObject
{

    [Header("Base Values")]
    public GPTNPC_ScriptableDiologue parentPort;
    [TextArea]
    public string diologue;

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
