using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    [SerializeField]
    public List<ItemDictionaryEntry> itemDictionary = new List<ItemDictionaryEntry>();

    private void Awake()
    {
        DOTween.Init(recycleAllByDefault: true);
        var dummyObject = new GameObject("DummyObject");
        dummyObject.transform.DOMove(Vector3.one, 0.1f).OnComplete(() =>
        {
            Destroy(dummyObject);
        });
    }
}