using UnityEditor;
using UnityEngine;

public class LogWindow : EditorWindow
{
    private string logText = "";
    private Color logColor = Color.white;

    [MenuItem("Open AI/WIONDOWW")]
    public static void ShowWindow()
    {
        LogWindow window = GetWindow<LogWindow>("Log Window");
        window.minSize = new Vector2(400, 100);
        window.maxSize = new Vector2(400, 100); 
    }

    public void LogError(string message)
    {
        logText += "<color=red>" + message + "</color>\n";
        Repaint();
        ShowWindow();
    }

    public void LogWarning(string message)
    {
        logText += "<color=yellow>" + message + "</color>\n";
        Repaint();
        ShowWindow();
    }

    private void OnGUI()
    {
        GUILayout.Label("Log Window", EditorStyles.boldLabel);

        // Display log text with rich text formatting
        GUILayout.Label(logText, new GUIStyle { richText = true });

        // Clear log button
        if (GUILayout.Button("OK"))
        {
           Close();
        }
    }
}
