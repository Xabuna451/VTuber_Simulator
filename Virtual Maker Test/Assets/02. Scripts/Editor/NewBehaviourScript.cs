using UnityEditor;
using UnityEngine;
using System.IO;

public static class ScriptShortcuts
{
    // % = Ctrl (Windows) / Cmd (Mac)
    // & = Alt
    // # = Shift
    // 단축키 Ctrl + Alt + N
    [MenuItem("Custom Shortcuts/Create C# Script %&n")]
    private static void CreateScript()
    {
        string templatesFolder = EditorApplication.applicationContentsPath + "/Resources/ScriptTemplates";
        string[] candidates = Directory.GetFiles(templatesFolder, "*C# Script-NewBehaviourScript.cs.txt");

        if (candidates.Length > 0)
        {
            // ù ��° ��Ī�� ���ø� ���
            string templatePath = candidates[0];
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewScript.cs");
        }
        else
        {
            Debug.LogError("C# Script : " + templatesFolder);
        }
    }
}