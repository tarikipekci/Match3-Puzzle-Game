using UnityEngine;

[CreateAssetMenu(menuName = "Match3/Item")]
public sealed class Item : ScriptableObject
{
    public int value;

    public Sprite sprite;
}