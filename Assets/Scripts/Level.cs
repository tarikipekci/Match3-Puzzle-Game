using UnityEngine;

[CreateAssetMenu(menuName = "Match3/Level")]
public class Level : ScriptableObject
{
    public int value;
    public string guid;

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
        // Only generate a new GUID if one does not already exist
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
            Debug.Log($"New GUID generated for {name}: {guid}");
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(GetCompletedKey());
        PlayerPrefs.DeleteKey(GetBestScoreKey());
    }
}