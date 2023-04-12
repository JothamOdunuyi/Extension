using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class Editor_GPTNPC : EditorWindow
{

    // Strings are delcared as "" so that string.Length will not return null
    private string NPC_Name = "";
    public string gender =  "";
    public int age = 18;
    private string personality = "";
    public string backstory = "";
    private string job = "";
    private string location = "";
    private string world_name = "";
    private string world_setting = "";
    private string language = "English";
    private float creativity = .5f;
    private bool name_introduction;
    private bool assume_assitance;
    private bool hasAge;

    private const int maxCharacters = 100;

    [MenuItem("Open AI/Create COOL GPT NPC")]
    static void Init()
    {
        Editor_GPTNPC window = (Editor_GPTNPC)EditorWindow.GetWindow(typeof(Editor_GPTNPC));
        window.Show();
    }

    private void OnGUI()
    {

        EditorGUILayout.LabelField("Name");
        NPC_Name = EditorGUILayout.TextField(NPC_Name); //REQUIRED

        EditorGUILayout.LabelField("Gender");
        gender = EditorGUILayout.TextField(gender);

        EditorGUILayout.LabelField("Age");
        age = EditorGUILayout.IntField(age); 

        EditorGUILayout.LabelField("Personality Traits");
        personality = EditorGUILayout.TextField(personality); //REQUIRED

        EditorGUILayout.LabelField("Backstory");
        backstory = EditorGUILayout.TextField(backstory);

        EditorGUILayout.LabelField("Job / Class");
        job = EditorGUILayout.TextField(job);

        EditorGUILayout.LabelField("Current Location");
        location = EditorGUILayout.TextField(location);

        EditorGUILayout.LabelField("Creativity (0-1)");
        creativity = EditorGUILayout.FloatField(creativity);

        EditorGUILayout.LabelField("Language");
        language = EditorGUILayout.TextField(language); //REQUIRED

        EditorGUILayout.LabelField("World Name");
        world_name = EditorGUILayout.TextField(world_name);

        EditorGUILayout.LabelField("World Setting");
        world_setting = EditorGUILayout.TextField(world_setting);

        name_introduction = EditorGUILayout.Toggle("Introduce with Name", name_introduction);
        assume_assitance = EditorGUILayout.Toggle("Assume Player wants assitance with something", assume_assitance);
        hasAge = EditorGUILayout.Toggle("NPC age is considered", hasAge);

        int total_characters = NPC_Name.Length + gender.Length + personality.Length + backstory.Length + job.Length + location.Length + world_name.Length + world_setting.Length + language.Length;
        EditorGUILayout.LabelField($"Total characters {total_characters}/ {maxCharacters}");

        if (GUILayout.Button("Submit"))
        {
            GPT_NPC newNPC = ScriptableObject.CreateInstance<GPT_NPC>();

            newNPC.name = NPC_Name;
            newNPC.gender = gender;
            newNPC.age = age;
            newNPC.personality = personality;
            newNPC.backstory = backstory;
            newNPC.job = job;
            newNPC.location = location ;
            newNPC.creativity = Mathf.Clamp(creativity, 0 , 1);
            newNPC.language = language;
            newNPC.name_introduction = name_introduction;
            newNPC.assume_assitance = assume_assitance;
            newNPC.world_name = world_name;
            newNPC.world_setting = world_setting;

            // Set the asset path and name
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/GPT_NPC ScriptableObjects/" + NPC_Name + ".asset");

            // Create the ScriptableObject asset
            AssetDatabase.CreateAsset(newNPC, assetPathAndName);

            // Save any changes to the asset database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject gameObject = Selection.activeGameObject;


            if (gameObject)
            {

                gameObject.AddComponent(Type.GetType("GPTNPC_Dialogue, Assembly-CSharp"));
                
                GPTNPC_Dialogue GPTNPC = gameObject.GetComponent<GPTNPC_Dialogue>();
                Transform dialogueCanvas = GameObject.Find("Dialogue_Canvas").transform;
                
                GPTNPC.NPC = newNPC;
                GPTNPC.textField = dialogueCanvas.Find("TMP").GetComponent<TMP_Text>();
                GPTNPC.inputField = dialogueCanvas.Find("InputField").GetComponent<TMP_InputField>();
                GPTNPC.sumbitButton = dialogueCanvas.Find("Button").GetComponent<UnityEngine.UI.Button>();
                GPTNPC.slider = dialogueCanvas.Find("Slider").GetComponent<UnityEngine.UI.Slider>();

                Debug.Log("Added component with ScriptableObject");
            }
            else
            {
                Debug.LogWarning("No Gameobject was selected");
            }
        }
    }
}
