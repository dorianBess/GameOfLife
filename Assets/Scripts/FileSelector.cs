using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class FileSelector : MonoBehaviour
{
    public string filePath = "";

    [MenuItem("GameObject/File Selector", false, 10)]
    static void CreateWindow(MenuCommand menuCommand)
    {
        EditorWindow.GetWindow<FileSelectorWindow>("File Selector");
    }
}

public class FileSelectorWindow : EditorWindow
{
    string filePath = "";

    void OnGUI()
    {
        GUILayout.Label("File Selector", EditorStyles.boldLabel);

        GUILayout.Space(10);

        GUILayout.Label("Selected File: " + filePath, EditorStyles.label);

        GUILayout.Space(20);

        if (GUILayout.Button("Select File"))
        {
            string path = EditorUtility.OpenFilePanel("Select a File", "", "");

            if (!string.IsNullOrEmpty(path))
            {
                filePath = path;
                Repaint(); // Force repaint to update GUI
            }
        }
    }
}
