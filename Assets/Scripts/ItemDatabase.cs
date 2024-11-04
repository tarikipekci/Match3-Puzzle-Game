using UnityEngine;

public static class ItemDatabase
{
    public static Item[] items { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        items = Resources.LoadAll<Item>("Items/");
    }
}
