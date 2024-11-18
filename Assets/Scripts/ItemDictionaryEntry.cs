[System.Serializable]
public class ItemDictionaryEntry
{
    public string key;
    public Item[] value;
}

[System.Serializable]
public class Target
{
    public int[] targetAmount;
    public Item[] targetItem;
}