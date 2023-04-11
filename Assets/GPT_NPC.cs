using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( menuName = "GPT NPC", fileName = "New NPC")]
public class GPT_NPC : ScriptableObject
{
    public new string name;
    public string gender;
    public int age;
    public string personality;
    public string backstory;
    public string job;
    public string location;
    public float creativity = .5f;
    public bool name_introduction;
    public bool assume_assitance;
    public bool hasAge;
    public string language = "English";
    public string world_name;
    public string world_setting;
}
