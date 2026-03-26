using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class PuzzlePrefabExporter
{
    public static void Export(PuzzleGrid grid, string path, string prefabName)
    {
        GameObject root = new GameObject(prefabName);
        List<PuzzleTile> allTiles = new List<PuzzleTile>();

        // 1. 遍历并同步，同时过滤掉 GroupId 为 0 的方块
        for (int x = 0; x < grid.width; x++) {
            for (int y = 0; y < grid.height; y++) {
                var bottom = grid.GetTile(x, y, 0);
                var top = grid.GetTile(x, y, 1);
                
                // 只有当 GroupId 不为 0 时才加入导出列表
                if (bottom != null && bottom.groupId != 0) {
                    allTiles.Add(bottom);
                    if (top != null) {
                        top.groupId = bottom.groupId; // 强制同步 Top 到 Bottom
                        allTiles.Add(top);
                    }
                } 
                // 如果没有底层但有顶层，且顶层 GroupId 不为 0
                else if (top != null && top.groupId != 0) {
                    allTiles.Add(top);
                }
            }
        }

        // 2. 按 Group ID 分组 (此时已排除了 0)
        var groups = allTiles.GroupBy(t => t.groupId);

        foreach (var group in groups)
        {
            GameObject gObj = new GameObject($"Group_{group.Key}");
            gObj.transform.SetParent(root.transform);
            
            var first = group.First();
            // 保持网格单位一致 (0.5f)
            gObj.transform.position = new Vector3(first.pos.x * 0.5f, first.pos.y * 0.5f, 0);

            foreach (var t in group)
            {
                GameObject tileObj = new GameObject($"Tile_L{t.layer}_{t.pos.x}_{t.pos.y}");
                tileObj.transform.SetParent(gObj.transform);
                var sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sprite = t.sprite;
                sr.sortingOrder = t.layer;

                tileObj.transform.localPosition = new Vector3((t.pos.x - first.pos.x) * 0.5f, (t.pos.y - first.pos.y) * 0.5f, 0);
                
                // 旋转修复：GUI(顺时针) -> 世界(逆时针)
                float finalRotation = -t.rotation; 
                tileObj.transform.localScale = new Vector3(t.isMirrored ? -1 : 1, 1, 1);
                tileObj.transform.localRotation = Quaternion.Euler(0, 0, finalRotation);
            }
        }

        // 3. 保存 Prefab
        if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
        string finalPath = $"{path}/{prefabName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, finalPath);
        GameObject.DestroyImmediate(root);
        AssetDatabase.Refresh();
        
        Debug.Log($"[导出成功] 已过滤 Group 0。共有 {groups.Count()} 个分组被导出到: {finalPath}");
    }
}