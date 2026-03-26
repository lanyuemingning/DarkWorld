using UnityEngine;

[System.Serializable]
public class PuzzleTile
{
    public Vector2Int pos;
    public Sprite sprite;
    public float rotation;
    public bool isMirrored;
    public int groupId;
    public int layer; 

    public PuzzleTile(Vector2Int p, Sprite s, float rot = 0f, int group = 1, int ly = 0)
    {
        pos = p;
        sprite = s;
        rotation = rot;
        groupId = group;
        isMirrored = false;
        layer = ly;
    }
}