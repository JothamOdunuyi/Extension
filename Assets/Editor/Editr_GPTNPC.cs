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

    private float baseWidth = 150f;
    private float extendedWidth = 300f;

    private const int tokenWarning = 500;

    [MenuItem("Open AI/Create COOL GPT NPC")]
    static void Init()
    {
        Editor_GPTNPC window = (Editor_GPTNPC)EditorWindow.GetWindow(typeof(Editor_GPTNPC));
        window.Show();
    }

    private void OnGUI()
    {

        EditorGUILayout.LabelField("Name*", EditorStyles.boldLabel);
        NPC_Name = EditorGUILayout.TextField(NPC_Name, GUILayout.Width(baseWidth)); //REQUIRED

        EditorGUILayout.LabelField("Gender");
        gender = EditorGUILayout.TextField(gender, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("Age");
        age = EditorGUILayout.IntField(age, GUILayout.Width(baseWidth)); 

        EditorGUILayout.LabelField("Personality Traits*", EditorStyles.boldLabel);
        personality = EditorGUILayout.TextField(personality, GUILayout.Width(extendedWidth)); //REQUIRED

        EditorGUILayout.LabelField("Backstory");
        backstory = EditorGUILayout.TextField(backstory, GUILayout.Width(extendedWidth));

        EditorGUILayout.LabelField("Job / Class");
        job = EditorGUILayout.TextField(job, GUILayout.Width(baseWidth));

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Current Location");
        location = EditorGUILayout.TextField(location, GUILayout.Width(extendedWidth));

        EditorGUILayout.LabelField("Creativity (0-1)");
        creativity = EditorGUILayout.FloatField(creativity, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("Language*", EditorStyles.boldLabel, GUILayout.Width(baseWidth));
        language = EditorGUILayout.TextField(language, GUILayout.Width(baseWidth)); //REQUIRED

        EditorGUILayout.LabelField("World Name");
        world_name = EditorGUILayout.TextField(world_name, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("World Setting");
        world_setting = EditorGUILayout.TextField(world_setting, GUILayout.Width(baseWidth));

        EditorGUILayout.Space();

        name_introduction = EditorGUILayout.Toggle("Introduce with Name", name_introduction, GUILayout.Width(baseWidth));
        assume_assitance = EditorGUILayout.Toggle("Assume Player wants assitance with something", assume_assitance);
        hasAge = EditorGUILayout.Toggle("NPC age is considered", hasAge);



        int total_characters = NPC_Name.Length + gender.Length + personality.Length + backstory.Length + job.Length + location.Length + world_name.Length + world_setting.Length + language.Length;
        EditorGUILayout.LabelField($"Estimated Tokens {(int)(total_characters / 3.5f)} / {tokenWarning}");

        if (GUILayout.Button("Submit"))
        {

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(personality) || string.IsNullOrEmpty(language))
            {
                EditorUtility.DisplayDialog("Error", "Name, Personality and Language are all required fields.", "OK");
            }
            else
            {
                GPT_NPC newNPC = ScriptableObject.CreateInstance<GPT_NPC>();

                newNPC.name = NPC_Name;
                newNPC.gender = gender;
                newNPC.age = age;
                newNPC.personality = personality;
                newNPC.backstory = backstory;
                newNPC.job = job;
                newNPC.location = location;
                newNPC.creativity = Mathf.Clamp(creativity, 0, 1);
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
}
