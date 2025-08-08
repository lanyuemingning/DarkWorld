using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

public static class ButtonSoundBinder
{
    private static List<MethodInfo> cachedMethods;

    #region Bind

    // 用于获取 ButtonSoundExtension 类的所有无参静态方法
    private static List<MethodInfo> GetStaticParameterlessMethods(Type type)
    {
        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var result = new List<MethodInfo>();
        foreach (var method in methods)
        {
            if (method.GetParameters().Length == 0 && method.ReturnType == typeof(void))
            {
                result.Add(method);
            }
        }
        return result;
    }

    // 供外部调用，弹窗选择绑定方法
    [MenuItem("Assets/_按钮音效/选择绑定方法并绑定按钮音效（Prefab）", false, 10)]
    public static void ShowBindMethodSelector()
    {
        // 获取所有无参静态方法
        cachedMethods = GetStaticParameterlessMethods(typeof(ButtonSoundExtension));
        if (cachedMethods.Count == 0)
        {
            Debug.LogWarning("未找到任何符合条件的静态无参方法");
            return;
        }

        // 弹出编辑器窗口，供选择
        ButtonSoundBinderWindow.ShowWindow();
    }

    // 验证选中的是Prefab
    [MenuItem("Assets/_按钮音效/选择绑定方法并绑定按钮音效（Prefab）", true)]
    public static bool ValidateShowBindMethodSelector()
    {
        return Selection.activeObject is GameObject prefab &&
               AssetDatabase.GetAssetPath(prefab).EndsWith(".prefab");
    }

    #endregion

    #region UnBind
    [MenuItem("Assets/_按钮音效/选择解绑方法并移除按钮音效（Prefab）", false, 11)]
    public static void ShowUnbindMethodSelector()
    {
        cachedMethods = GetStaticParameterlessMethods(typeof(ButtonSoundExtension));
        if (cachedMethods.Count == 0)
        {
            Debug.LogWarning("未找到任何符合条件的静态无参方法");
            return;
        }

        ButtonSoundUnbinderWindow.ShowWindow(); // 调用解绑窗口
    }

    [MenuItem("Assets/_按钮音效/选择解绑方法并移除按钮音效（Prefab）", true)]
    public static bool ValidateShowUnbindMethodSelector()
    {
        return Selection.activeObject is GameObject prefab &&
               AssetDatabase.GetAssetPath(prefab).EndsWith(".prefab");
    }
    /* // 解绑指定方法事件
     [MenuItem("Assets/_按钮音效/选择解绑方法并移除按钮音效（Prefab）", false, 11)]
     public static void ShowUnbindMethodSelector()
     {
         cachedMethods = GetStaticParameterlessMethods(typeof(ButtonSoundExtension));
         if (cachedMethods.Count == 0)
         {
             Debug.LogWarning("未找到任何符合条件的静态无参方法");
             return;
         }

         ButtonSoundBinderWindow.ShowWindow();
         //ButtonSoundBinderWindow.ShowWindow(true);
     }

     [MenuItem("Assets/_按钮音效/选择解绑方法并移除按钮音效（Prefab）", true)]
     public static bool ValidateShowUnbindMethodSelector()
     {
         return Selection.activeObject is GameObject prefab &&
                AssetDatabase.GetAssetPath(prefab).EndsWith(".prefab");
     }*/
    #endregion

    // 供编辑器窗口访问方法
    public static List<MethodInfo> GetCachedMethods() => cachedMethods;
}
