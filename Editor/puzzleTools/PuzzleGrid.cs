using UnityEngine;
using System;

[Serializable]
public class PuzzleGrid
{
    public int width;
    public int height;
    public PuzzleTile[] layerBottom;
    public PuzzleTile[] layerTop;

    public PuzzleGrid(int w, int h)
    {
        width = w;
        height = h;
        layerBottom = new PuzzleTile[w * h];
        layerTop = new PuzzleTile[w * h];
    }

    public void CheckInitialization()
    {
        if (layerBottom == null || layerBottom.Length != width * height)
            layerBottom = new PuzzleTile[width * height];
        if (layerTop == null || layerTop.Length != width * height)
            layerTop = new PuzzleTile[width * height];
    }

    public PuzzleTile GetTile(int x, int y, int layer)
    {
        CheckInitialization();
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        var array = (layer == 1) ? layerTop : layerBottom;
        return array[y * width + x];
    }

    public void SetTile(int x, int y, Sprite sprite, float rotation, int groupId, int layer)
    {
        CheckInitialization();
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        var array = (layer == 1) ? layerTop : layerBottom;
        array[y * width + x] = new PuzzleTile(new Vector2Int(x, y), sprite, rotation, groupId, layer);
    }

    public bool InBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    public void RemoveTile(int x, int y, int layer)
    {
        CheckInitialization();
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        var array = (layer == 1) ? layerTop : layerBottom;
        array[y * width + x] = null;
    }
}