using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class FileRenamerTool : EditorWindow
{
    private string prefix = "";
    private bool useFolderName = true;
    private bool deleteOriginal = true;
    private bool recursive = false;
    private Vector2 scrollPosition;
    private List<string> logMessages = new List<string>();

    [MenuItem("Tools/文件重命名工具")]
    public static void ShowWindow()
    {
        GetWindow<FileRenamerTool>("文件重命名工具");
    }

    void OnGUI()
    {
        GUILayout.Label("文件重命名工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 设置选项
        useFolderName = EditorGUILayout.Toggle("使用文件夹名前缀", useFolderName);
        
        if (!useFolderName)
        {
            prefix = EditorGUILayout.TextField("自定义前缀", prefix);
        }
        
        deleteOriginal = EditorGUILayout.Toggle("删除原文件", deleteOriginal);
        recursive = EditorGUILayout.Toggle("包含子文件夹", recursive);
        
        EditorGUILayout.Space();
        
        // 显示当前选中文件夹信息
        if (Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Directory.Exists(path))
            {
                EditorGUILayout.HelpBox($"当前选中文件夹: {Path.GetFileName(path)}", MessageType.Info);
            }
        }
        
        EditorGUILayout.Space();
        
        // 操作按钮
        if (GUILayout.Button("批量重命名文件", GUILayout.Height(40)))
        {
            RenameFilesInSelectedFolder();
        }
        
        if (GUILayout.Button("预览重命名", GUILayout.Height(30)))
        {
            PreviewRenaming();
        }
        
        if (GUILayout.Button("清空日志", GUILayout.Height(25)))
        {
            logMessages.Clear();
        }
        
        EditorGUILayout.Space();
        
        // 日志显示区域
        GUILayout.Label("操作日志:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        foreach (var log in logMessages)
        {
            EditorGUILayout.LabelField(log);
        }
        EditorGUILayout.EndScrollView();
    }

    private void RenameFilesInSelectedFolder()
    {
        if (Selection.activeObject == null)
        {
            AddLog("错误: 请先选择一个文件夹");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!Directory.Exists(folderPath))
        {
            AddLog("错误: 选中的不是文件夹");
            return;
        }

        string folderName = Path.GetFileName(folderPath);
        string targetPrefix = useFolderName ? folderName + "_" : prefix + "_";
        
        int successCount = 0;
        int skipCount = 0;
        int errorCount = 0;

        // 获取文件列表
        SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        string[] files = Directory.GetFiles(folderPath, "*", searchOption);
        
        AddLog($"开始处理文件夹: {folderName}");
        AddLog($"找到 {files.Length} 个文件");
        AddLog("------------------------");

        AssetDatabase.StartAssetEditing();
        
        foreach (string file in files)
        {
            // 跳过.meta文件和自身
            if (file.EndsWith(".meta") || Path.GetFileName(file) == "FileRenamerTool.cs")
                continue;

            string fileName = Path.GetFileName(file);
            string fileExtension = Path.GetExtension(file);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            string directory = Path.GetDirectoryName(file);
            
            string newFileName = targetPrefix + fileNameWithoutExt + fileExtension;
            string newFilePath = Path.Combine(directory, newFileName);
            
            // 检查是否已经是目标格式
            if (fileName == newFileName)
            {
                AddLog($"跳过: {fileName} (已经是目标格式)");
                skipCount++;
                continue;
            }
            
            // 检查目标文件是否已存在
            if (File.Exists(newFilePath))
            {
                AddLog($"跳过: {fileName} -> {newFileName} (文件已存在)");
                skipCount++;
                continue;
            }
            
            try
            {
                // 重命名文件
                if (deleteOriginal)
                {
                    // 移动文件（相当于重命名+删除原文件）
                    File.Move(file, newFilePath);
                    
                    // 如果存在.meta文件，也一起移动
                    string metaFile = file + ".meta";
                    if (File.Exists(metaFile))
                    {
                        File.Move(metaFile, newFilePath + ".meta");
                    }
                }
                else
                {
                    // 复制文件（保留原文件）
                    File.Copy(file, newFilePath, false);
                    
                    // 如果存在.meta文件，也一起复制
                    string metaFile = file + ".meta";
                    if (File.Exists(metaFile))
                    {
                        File.Copy(metaFile, newFilePath + ".meta", false);
                    }
                }
                
                AddLog($"成功: {fileName} -> {newFileName}");
                successCount++;
            }
            catch (System.Exception e)
            {
                AddLog($"错误: 处理 {fileName} 失败 - {e.Message}");
                errorCount++;
            }
        }
        
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
        
        AddLog("------------------------");
        AddLog($"处理完成!");
        AddLog($"成功: {successCount} 个文件");
        AddLog($"跳过: {skipCount} 个文件");
        AddLog($"错误: {errorCount} 个文件");
    }

    private void PreviewRenaming()
    {
        if (Selection.activeObject == null)
        {
            AddLog("错误: 请先选择一个文件夹");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!Directory.Exists(folderPath))
        {
            AddLog("错误: 选中的不是文件夹");
            return;
        }

        string folderName = Path.GetFileName(folderPath);
        string targetPrefix = useFolderName ? folderName + "_" : prefix + "_";
        
        AddLog($"预览重命名 - 文件夹: {folderName}");
        AddLog($"前缀: {targetPrefix}");
        AddLog("预览列表:");
        AddLog("------------------------");
        
        SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        string[] files = Directory.GetFiles(folderPath, "*", searchOption);
        
        int count = 0;
        foreach (string file in files)
        {
            if (file.EndsWith(".meta") || Path.GetFileName(file) == "FileRenamerTool.cs")
                continue;
                
            string fileName = Path.GetFileName(file);
            string fileExtension = Path.GetExtension(file);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            
            string newFileName = targetPrefix + fileNameWithoutExt + fileExtension;
            
            AddLog($"{fileName} -> {newFileName}");
            count++;
            
            if (count >= 20) // 最多显示20个预览
            {
                AddLog("... (更多文件未显示)");
                break;
            }
        }
        
        AddLog("------------------------");
        AddLog($"共预览 {Mathf.Min(count, files.Length)} 个文件");
    }

    private void AddLog(string message)
    {
        logMessages.Add(message);
        Repaint(); // 刷新UI
    }
}