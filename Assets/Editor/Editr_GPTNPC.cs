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
    public string whoIsTalking = "Stranger";

    private float baseWidth = 150f;
    private float extendedWidth = 300f;
    private float extendedheight = 50;

    private const int tokenWarning = 500;

    private const int windowWidth = 312;
    private const int windowHeight = 646;

    private bool foldout;
    private Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Open AI/Create New GPT NPC")]
    static void Init()
    {
        Editor_GPTNPC window = GetWindow<Editor_GPTNPC>("GPT_NPC Generator");
        var position = window.position;
        position.center = new Rect(0f, 0f, Screen.currentResolution.width / 2, Screen.currentResolution.height / 1.25f).center;
        window.position = new Rect(position.x, position.y, windowWidth, windowHeight);
        window.Show();
    }

    private void CreateHeader(string content)
    {

        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 18;

        //        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, new GUIContent(content));

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);//EditorStyles.whiteLabel
        EditorGUILayout.LabelField(content, headerStyle, GUILayout.Width(300));
        EditorGUILayout.Separator();
    }


    private void EndHeader()
    {
        EditorGUILayout.EndVertical();
    }

    private void CreateUIWithInstruction(ref string field, string labelContent, GUIStyle style, bool extendedWidth, params GUILayoutOption[] layout)
    {

        GUIStyle helpStyle = new GUIStyle(EditorStyles.largeLabel);
        helpStyle.fontStyle = FontStyle.Bold;
        helpStyle.fontSize = 12;
        if (!extendedWidth)
        {
            helpStyle.padding = new RectOffset(170, 0, -5, 0);
        }
        else
        {
            helpStyle.padding = new RectOffset(20, 0, 0, 0);
        }
        helpStyle.clipping = UnityEngine.TextClipping.Overflow;

        EditorGUILayout.BeginHorizontal();

        field = EditorGUILayout.TextField(field, style, layout);
        EditorGUILayout.LabelField(labelContent, helpStyle, GUILayout.Width(-5)); //-5 to make everything align

        EditorGUILayout.EndHorizontal();


    }



    private void OnGUI()
    {
        /*Editor_GPTNPC window = GetWindow<Editor_GPTNPC>("GPT_NPC Editor");
        Debug.Log(window.position);*/
        GUIStyle textStyle = new GUIStyle(EditorStyles.textArea);
        textStyle.fontSize = 13;
        textStyle.clipping = UnityEngine.TextClipping.Clip;
        textStyle.padding = new RectOffset(3, 0, 0, 0);
        textStyle.normal.textColor = Color.white;

        GUIStyle requiredStyle = new GUIStyle(EditorStyles.boldLabel);
        requiredStyle.fontSize = 12;
        requiredStyle.clipping = UnityEngine.TextClipping.Clip;

        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 18;


        Rect rect = new Rect(0, 0, position.width, 270);
        //EditorGUI.DrawRect(rect, Color.grey);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
        EditorGUILayout.BeginVertical();


        CreateHeader("Requirements");
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Name*", EditorStyles.boldLabel, GUILayout.Width(50));
        //EditorGUILayout.LabelField("required field",  requiredStyle,  GUILayout.Width(100

        EditorGUILayout.EndHorizontal();


        //NPC_Name = EditorGUILayout.TextField(NPC_Name, ); //REQUIRED
        CreateUIWithInstruction(ref NPC_Name, "No full stops", textStyle, false, GUILayout.Width(baseWidth));
       
        EditorGUILayout.LabelField("Personality Traits*", EditorStyles.boldLabel, GUILayout.Width(120));
        CreateUIWithInstruction(ref personality, "No captialization \n Example: friendly, caring" , textStyle, true, GUILayout.Width(extendedWidth), GUILayout.Height(extendedheight));


        EditorGUILayout.LabelField("Language*", EditorStyles.boldLabel, GUILayout.Width(baseWidth), GUILayout.Width(125));
        language = EditorGUILayout.TextField(language, GUILayout.Width(baseWidth)); //REQUIRED

        EndHeader();

        CreateHeader("Character Traits");

        EditorGUILayout.LabelField("Gender");
        gender = EditorGUILayout.TextField(gender, textStyle, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("Age");
        age = EditorGUILayout.IntField(age, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("Backstory");
        CreateUIWithInstruction(ref backstory, "Direction addressing only \n Example: You saved the world from the \n demong king, but you now wish for a simple and peaceful life", textStyle, true, GUILayout.Width(extendedWidth), GUILayout.Height(extendedheight));


        EditorGUILayout.LabelField("Job / Class");
        CreateUIWithInstruction(ref job, "Do not use \"a\" \n Example: Warrior, Adventurer", textStyle, false, GUILayout.Width(baseWidth));


        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Current Location");
        location = EditorGUILayout.TextField(location, GUILayout.Width(extendedWidth));

        EditorGUILayout.LabelField("Who the player is to the NPC");
        CreateUIWithInstruction(ref whoIsTalking, "Do not use \"a\" \n Example: Warrior, Adventurer", textStyle, false, GUILayout.Width(extendedWidth));


        EditorGUILayout.LabelField("Creativity (0-1)");
        creativity = EditorGUILayout.FloatField(creativity, GUILayout.Width(baseWidth));


        EndHeader();

        CreateHeader("Character Options");

        name_introduction = EditorGUILayout.Toggle("Introduce with Name", name_introduction, GUILayout.Width(baseWidth));
        assume_assitance = EditorGUILayout.Toggle("Assume Player wants assitance with something", assume_assitance);
        hasAge = EditorGUILayout.Toggle("NPC age is considered", hasAge);

        EndHeader();

        CreateHeader("World Options");

        EditorGUILayout.LabelField("World Name");
        world_name = EditorGUILayout.TextField(world_name, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("World Setting");
        world_setting = EditorGUILayout.TextField(world_setting, GUILayout.Width(baseWidth));

        EndHeader();

        CreateHeader("Estimated Tokens");

        int total_characters = NPC_Name.Length + gender.Length + personality.Length + backstory.Length + job.Length + location.Length + world_name.Length + world_setting.Length + language.Length;
        EditorGUILayout.LabelField($"Estimated Tokens {(int)(total_characters / 3.5f)} / {tokenWarning}");

        if (GUILayout.Button("Submit"))
        {

            if (string.IsNullOrEmpty(NPC_Name) || string.IsNullOrEmpty(personality) || string.IsNullOrEmpty(language) || creativity > 1 || !GameObject.Find("Dialogue_Canvas"))
            {
                EditorUtility.DisplayDialog("Error", "Name, Personality and Language are all required fields. Creativity cannot be above 1. \"Dialogue_Canvas\" must be an active GameObject and in the scene.", "OK");
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
                newNPC.whoIsTalking = whoIsTalking;
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
                    // Only creates a new script when it is now present
                    if (!gameObject.GetComponent<GPTNPC_Dialogue>())
                    {
                        gameObject.AddComponent(Type.GetType("GPTNPC_Dialogue, Assembly-CSharp"));
                    }

                    GPTNPC_Dialogue GPTNPC = gameObject.GetComponent<GPTNPC_Dialogue>();
                    Transform dialogueCanvas = GameObject.Find("Dialogue_Canvas").transform;

                    GPTNPC.NPC = newNPC;
                    GPTNPC.textField = dialogueCanvas.Find("Converation_TMP").GetComponent<TMP_Text>();
                    GPTNPC.inputField = dialogueCanvas.Find("InputField").GetComponent<TMP_InputField>();
                    GPTNPC.submitButton = dialogueCanvas.Find("Submit Button").GetComponent<UnityEngine.UI.Button>();
                    GPTNPC.closeButton = dialogueCanvas.Find("Close Button").GetComponent<UnityEngine.UI.Button>();
                    GPTNPC.slider = dialogueCanvas.Find("Slider").GetComponent<UnityEngine.UI.Slider>();

                    Debug.Log($"{gameObject.name} has been successfully set up");
                }
                else
                {
                    Debug.LogWarning("No Gameobject was selected");
                }
            }

        }

        GUILayout.EndVertical();
        EndHeader();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginVertical();
        CreateHeader("Instructions");
        //EditorGUILayout.LabelField("Requirements", headerStyle);
        //EditorGUILayout.Separator();
        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.largeLabel);
        subtitleStyle.fontSize = 16;
        subtitleStyle.padding = new RectOffset(8, 0, -8, 0); //, GUILayout.Height(extendedheight*3)

        GUIStyle instructionsStyle = new GUIStyle(EditorStyles.largeLabel);
        instructionsStyle.fontSize = 14;
        instructionsStyle.padding = new RectOffset(8, 0, 10, 0); //, GUILayout.Height(extendedheight*3)

        EditorGUILayout.LabelField("Do not use full stops in any field", subtitleStyle, GUILayout.Height(extendedheight * 1.15f), GUILayout.Width(extendedWidth + 60));
        /*EditorGUILayout.LabelField("No captialization \n Example: friendly, caring", instructionsStyle, GUILayout.Height(extendedheight));
        EditorGUILayout.LabelField("This field cannot be empty", instructionsStyle, GUILayout.Height(extendedheight));*/
        EndHeader();
        EditorGUILayout.EndVertical();
        GUILayout.EndScrollView();

        EditorGUILayout.EndHorizontal();



    }
}
