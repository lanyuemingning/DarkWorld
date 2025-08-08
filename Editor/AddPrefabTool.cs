#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.MaterialProperty;

public class AddPrefabTool : EditorWindow
{
    private string _filePath = "YourFilePath";
    private string _parent = "YourParent";
    private GameObject _prefab;

    [MenuItem("Tools/AddPrefabTool")]
    public static void ShowWindow()
    {
        GetWindow<AddPrefabTool>();
    }

    private void OnGUI()
    {
        GUILayout.Label("批量向模型添加内容", EditorStyles.boldLabel);
        _filePath = EditorGUILayout.TextField("被添加的物体路径", _filePath);
        _prefab = (GameObject)EditorGUILayout.ObjectField("添加的物体", _prefab, typeof(GameObject), true);
        if (GUILayout.Button("添加"))
        {
            AddPrefab();
        }
    }

    private void AddPrefab()
    {
        // 查找指定文件夹中的所有预制体
        string[] guids = AssetDatabase.FindAssets("UIRankClear t:Prefab", new[] { _filePath });
        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        // 打开预制体进行编辑
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(assetPath);

        // 实例化预制体
        GameObject instantiate = PrefabUtility.InstantiatePrefab(_prefab) as GameObject;


        List<string> nameList = new List<string>();
        List<string> starList = new List<string>();
        TextAsset Rank = Resources.Load<TextAsset>("Text/Rank");
        string[] textList = Rank.text.Split("\r\n");

        foreach (var item in textList)
        {
            item.Replace(" ", "");
            //Debug.Log(item);
            string[] textPair = item.Split("\t", 2);
            nameList.Add(textPair[0].Trim());
            starList.Add(textPair[1].Trim());
        }
        //for(int i = 0; i < panel.rankNode.childCount; i++)
        //{
        //    var item = panel.rankNode.GetChild(i).GetComponent<RankItemCtrl>();
        //    panel.rankItemCtrls.Add(item);
        //    //item.GetText(i, nameList[i], starList[i]);
        //}
        //prefabInstance = instantiate;
        // 保存对预制体的修改
        PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
        // 卸载预制体内容
        PrefabUtility.UnloadPrefabContents(prefabInstance);
        AssetDatabase.Refresh();

        // 刷新资产数据库
        AssetDatabase.Refresh();
    }
}
#endif