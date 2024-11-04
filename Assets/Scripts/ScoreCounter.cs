using UnityEngine;
using UnityEngine.UI;

public sealed class ScoreCounter : MonoBehaviour
{
    public static ScoreCounter Instance { get; private set; }

    [SerializeField] private Text scoreText;

    private int _score;

    public int Score
    {
        get => _score;

        set
        {
            if (_score == value) return;

            _score = value;

            scoreText.text = $"Score: {_score}";
        }
    }

    private void Awake()
    {
        Instance = this;
    }
}