using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Match3/Level")]
public class Level : ScriptableObject
{
    public int value;
    [SerializeField] private string guid;

    [SerializeField] private bool _isCompleted;
    [SerializeField] private int _bestScore;
    
    public bool isCompleted
    {
        get => PlayerPrefs.GetInt(GetCompletedKey(), 0) == 1;
        set
        {
            _isCompleted = value;
            PlayerPrefs.SetInt(GetCompletedKey(), value ? 1 : 0);
        }
    }

    public int BestScore
    {
        get => PlayerPrefs.GetInt(GetBestScoreKey(), 0);
        set
        {
            _bestScore = value;
            PlayerPrefs.SetInt(GetBestScoreKey(), value);
        }
    }

    private string GetCompletedKey() => $"{guid}_IsCompleted";
    private string GetBestScoreKey() => $"{guid}_BestScore";

    private void OnEnable()
    {
        _isCompleted = isCompleted;
        _bestScore = BestScore;
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
            Debug.Log($"New GUID generated for {name}: {guid}");

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(GetCompletedKey());
        PlayerPrefs.DeleteKey(GetBestScoreKey());
    }
}