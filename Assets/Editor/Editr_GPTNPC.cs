using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Editor_GPTNPC : EditorWindow
{
    // Strings are delcared as "" so that string.Length will not return null
    private string NPC_Name = "";
    public string gender =  "";
    private string personality = "";
    public string backstory = "";
    private string job = "";
    private string location = "";
    private string world_name = "";
    private string world_setting = "";
    private string language = "English";
    public string whoIsTalking = "Stranger";
    private bool name_introduction;
    private bool assume_assitance;
    private bool hasAge;
    private bool setTextStyles;
    public int age = 18;
    private float creativity = .5f;
    private const int tokenWarning = 500;
    private Vector2 scrollPosition = Vector2.zero;
    private static Editor_GPTNPC window;

    //UI values
    private float baseWidth = 150f;
    private float extendedWidth = 300f;
    private float extendedheight = 50;

    // Window Values, const allows static access
    private const int windowWidth = 652;
    private const int windowHeight = 646;

    //Styles
    GUIStyle textStyle = new GUIStyle(EditorStyles.textArea);
    GUIStyle helpStyle = new GUIStyle(EditorStyles.largeLabel);
    GUIStyle toggleHelpStyle = new GUIStyle(EditorStyles.largeLabel);
    GUIStyle requiredStyle = new GUIStyle(EditorStyles.boldLabel);
    GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);

    [MenuItem("Open AI/Create New GPT NPC")]
    static void OpenWindow()
    {
        window = GetWindow<Editor_GPTNPC>("GPT NPC Creator");
     
        window.maxSize = new Vector2(windowWidth, windowHeight+200);
        window.minSize = new Vector2(windowWidth, windowHeight);

        window.Show();
    }

    private void OnGUI()
    {

        #region UI Creation Methods

        void CreateHeader(string content)
        {

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.fontSize = 18;

            //        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, new GUIContent(content));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);//EditorStyles.whiteLabel
            EditorGUILayout.LabelField(content, headerStyle, GUILayout.Width(300));
            EditorGUILayout.Separator();
        }

        // For readability
        void EndHeader()
        {
            EditorGUILayout.EndVertical();
        }

        // Generics prevent overloading functions, DRY
        void CreateUIWithInstruction<T>(ref T field, string InstructionContent, GUIStyle style, bool extendedWidth, params GUILayoutOption[] layout)
        {

            helpStyle.padding = new RectOffset(extendedWidth ? 20 : 170, 0, -10, 0);

            EditorGUILayout.BeginHorizontal();

            // Get type and make correct field
            if (typeof(T) == typeof(string))
            {
                string stringValue = field as string; // Only do-able because string is a reference type
                stringValue = EditorGUILayout.TextField(stringValue, style, layout);
                field = (T)Convert.ChangeType(stringValue, typeof(T)); // ChangeType returns type Object which is parsable to any type
            }
            else if (typeof(T) == typeof(float))
            {
                float floatValue = (float)Convert.ChangeType(field, typeof(float));
                floatValue = EditorGUILayout.FloatField(floatValue, style, layout);
                field = (T)Convert.ChangeType(floatValue, typeof(T));
            }

            EditorGUILayout.LabelField(InstructionContent, helpStyle, GUILayout.Width(-5)); //-5 to make everything align
            EditorGUILayout.EndHorizontal();
        }

        void CreateToggleWithInstruction(ref bool toggle, string toggleContent, string InstructionContent)
        {
            GUILayout.BeginHorizontal();

            toggle = EditorGUILayout.Toggle(toggleContent, toggle, GUILayout.Width(270));

            EditorGUILayout.LabelField(InstructionContent, toggleHelpStyle); //-5 to make everything align

            GUILayout.EndHorizontal();
        }

        // Set style settings
        void SetTextStyles()
        {
            if (!setTextStyles)
            {
                textStyle.fontSize = 13;
                textStyle.clipping = UnityEngine.TextClipping.Clip;
                textStyle.padding = new RectOffset(3, 0, 0, 0);
                textStyle.normal.textColor = Color.white;

                requiredStyle.fontSize = 12;
                requiredStyle.clipping = UnityEngine.TextClipping.Clip;

                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.fontSize = 18;

                toggleHelpStyle.fontStyle = FontStyle.Bold;
                toggleHelpStyle.fontSize = 12;
                toggleHelpStyle.padding = new RectOffset(50, 0, 0, 0);
                toggleHelpStyle.clipping = UnityEngine.TextClipping.Overflow;

                helpStyle.fontStyle = FontStyle.Bold;
                helpStyle.fontSize = 12;
                helpStyle.clipping = UnityEngine.TextClipping.Overflow;

                setTextStyles = true;
            }
        }

        void SubmitButton()
        { 
            if (GUILayout.Button("Submit", GUILayout.Width(100), GUILayout.Height(30)))
            {

                if (string.IsNullOrEmpty(NPC_Name) || string.IsNullOrEmpty(personality) || string.IsNullOrEmpty(language) || creativity > 1 || !GameObject.Find("Dialogue_Canvas"))
                {
                    EditorUtility.DisplayDialog("Error", "1. Fill out all required fields \n2. Creativity cannot be above 1. \n3. \"Dialogue_Canvas\" must be an active GameObject and in the scene.", "OK");
                }
                else
                {
                    SubmitData();
                }

            }
        }

        #endregion

        SetTextStyles();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
        EditorGUILayout.BeginVertical();

        CreateHeader("Requirements");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Name*", EditorStyles.boldLabel, GUILayout.Width(50));

        EditorGUILayout.EndHorizontal();

        CreateUIWithInstruction(ref NPC_Name, "Do NOT \n use full stops in any field", textStyle, false, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("Personality Traits*", EditorStyles.boldLabel, GUILayout.Width(120));
        CreateUIWithInstruction(ref personality, "No captialization \n Example: friendly, caring" , textStyle, true, GUILayout.Width(extendedWidth), GUILayout.Height(extendedheight));

        EditorGUILayout.LabelField("Language*", EditorStyles.boldLabel, GUILayout.Width(baseWidth), GUILayout.Width(125));
        language = EditorGUILayout.TextField(language, GUILayout.Width(baseWidth));

        EndHeader();

        CreateHeader("Character Traits");

        EditorGUILayout.LabelField("Gender");
        gender = EditorGUILayout.TextField(gender, textStyle, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("Age");
        age = EditorGUILayout.IntField(age, textStyle, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("Backstory");
        CreateUIWithInstruction(ref backstory, "Direction addressing only \n Example: You saved the world from the \n demong king, but you now wish for a simple \n and peaceful life", textStyle, true, GUILayout.Width(extendedWidth), GUILayout.Height(extendedheight));

        EditorGUILayout.LabelField("Job / Class");
        CreateUIWithInstruction(ref job, "Do not use \"a\" \n Example: Warrior, Adventurer", textStyle, false, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("Current Location");
        CreateUIWithInstruction(ref location, "The current location \n of your NPC", textStyle, true, GUILayout.Width(extendedWidth));

        EditorGUILayout.LabelField("Who the player is to the NPC");
        CreateUIWithInstruction(ref whoIsTalking, "Stranger is the default, when using, heavily \n advised  to use a name the character knows", textStyle, true, GUILayout.Width(extendedWidth));

        EditorGUILayout.LabelField("Creativity");
        CreateUIWithInstruction(ref creativity, "The higher the value the more likely \n the NPC will reply uniquely (must be between 0-1)", textStyle, false, GUILayout.Width(baseWidth));

        EndHeader();

        CreateHeader("Character Options");

        name_introduction = EditorGUILayout.Toggle("Introduce with Name", name_introduction, GUILayout.Width(baseWidth));

        CreateToggleWithInstruction(ref assume_assitance, "Assume Assitance", "NPC will assume the player requires assistance");

        CreateToggleWithInstruction(ref hasAge, "Age is considered", "NPC age will be added in the prompt");

        EndHeader();

        CreateHeader("World Options");

        EditorGUILayout.LabelField("World Name");
        world_name = EditorGUILayout.TextField(world_name, textStyle, GUILayout.Width(baseWidth));

        EditorGUILayout.LabelField("World Setting");
        CreateUIWithInstruction(ref world_setting, "World genre \n Example: Dark Fantasy", textStyle, false, GUILayout.Width(baseWidth));

        EndHeader();

        CreateHeader("Estimated Tokens");

        GUILayout.BeginHorizontal();

        int total_characters = NPC_Name.Length + gender.Length + personality.Length + backstory.Length + job.Length + location.Length + world_name.Length + world_setting.Length + language.Length;
        EditorGUILayout.LabelField($"Character Prompt : {(int) (total_characters / 3.5f)} / {tokenWarning}", GUILayout.Width(200));

        SubmitButton();

        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"Token per \n interaction : {(int) ((total_characters + 50 + 120) / 3.5f)}", GUILayout.Width(200), GUILayout.Height(25));

        GUILayout.EndVertical();
        EndHeader();

        GUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }
    
    private void SubmitData()
    {
        GPT_NPC newNPC = ScriptableObject.CreateInstance<GPT_NPC>();

        // Put all the values typed into the new ScriptableObject
        void SetNewNPCValues() 
        {
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
        }

        SetNewNPCValues();

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

            // Set the new script's values
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
            Debug.LogWarning("No Gameobject was selected, you can find the GPT NPC ScriptableObject in Assets/GPT_NPC ScriptableObjects/");
        }

        window.Close();
    }
}
