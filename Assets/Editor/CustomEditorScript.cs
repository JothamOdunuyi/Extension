using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(GameObject))]
public class CustomEditorScript : Editor
{

    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Space(80f);
        if (GUILayout.Button("Generate Script", GUILayout.Width(240), GUILayout.Height(25)))
        {
            EditorWindow.GetWindow(typeof(AddScriptEditorWindow));
        }
        GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}


/*[InitializeOnLoad]
public static class AddCustomComponentButton
{
    private static readonly string customComponentName = "MyCustomComponent";

    static AddCustomComponentButton()
    {
        EditorApplication.update += AddButtonToComponentMenu;
    }

    private static void AddButtonToComponentMenu()
    {
        if (Selection.activeGameObject != null)
        {
            var inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var trackerType = typeof(Editor).Assembly.GetType("UnityEditor.ActiveEditorTracker");
            var tracker = new ActiveEditorTracker();
            var inspectors = tracker.activeEditors;

            foreach (var inspector in inspectors)
            {
                if (inspector.GetType() == inspectorWindowType)
                {
                    var window = inspector as EditorWindow;
                    var root = window.rootVisualElement;
                    var componentButton = root.Q<ToolbarMenu>("GameObject").Q<ToolbarButton>("GameObject.AddComponent");
                    VisualElement menuThang = null; ;
                    if (componentButton != null)
                    {
                        var menu = componentButton.hierarchy.Children();

                        var exists = false;

                        foreach (var menuItem in menu)
                        {
                            var scriptName = menuItem.name;

                            if (scriptName == customComponentName)
                            {
                                exists = true;
                                menuThang = menuItem;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            componentButton.Add(menuThang);
                            //componentButton.menu.AppendAction(customComponentName, x => AddCustomComponent(), x => DropdownMenuAction.Status.Normal);
                        }
                    }
                }
            }

            tracker.Dispose();
        }
    }

    private static void AddCustomComponent()
    {
        var go = Selection.activeGameObject;
        var type = Type.GetType(customComponentName);
        go.AddComponent(type);
    }
}
*/