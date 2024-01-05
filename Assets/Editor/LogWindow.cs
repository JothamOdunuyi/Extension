using UnityEditor;
using UnityEngine;

// Purpose is to better inform the user, since i feel like output won't be enough
public class LogWindow : EditorWindow
{
    private string logText = "";
    private Color logColor = Color.white;

    public static void ShowWindow()
    {
        LogWindow window = GetWindow<LogWindow>("Log Window");
        window.minSize = new Vector2(400, 100);
        window.maxSize = new Vector2(400, 100); 
    }

    public void LogError(string message)
    {
        Debug.LogError(message);
        logText += $"<color=red>{message}</color>\n";
        Repaint();
    }

    public void LogWarning(string message)
    {
        Debug.LogWarning(message);
        logText += $"<color=yellow>{message}</color>\n";
        Repaint();
    }

    private void OnGUI()
    {
        // Display log text with rich text formatting
        GUILayout.Label(logText, new GUIStyle { richText = true, wordWrap = true });

        // Clear log button
        if (GUILayout.Button("OK"))
        {
           Close();
        }
    }
}
