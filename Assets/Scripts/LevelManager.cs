using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Target levelTarget;
    public Image[] targetIcons;
    public Text[] targetAmounts;
    [FormerlySerializedAs("LevelStates")] public LevelStates levelStates;
    public GameObject resultPanel;
    public Text moveCountText;
    public Text currentScoreText;
    public Text bestScoreText;
    public Text resultText;
    public Sprite victoryTexture;
    public Sprite failTexture;
    public Image resultButtonImage;
    public int numberOfMoves;
    public bool levelFinished;

    private void Awake()
    {
        for (int i = 0; i < levelTarget.targetAmount.Length; i++)
        {
            targetIcons[i].sprite = levelTarget.targetItem[i].sprite;
            targetAmounts[i].text = levelTarget.targetAmount[i].ToString();
        }
    }

    private async void Start()
    {
        var deflateSequence = DOTween.Sequence();
        deflateSequence.Join(resultPanel.transform.DOScale(Vector3.zero, 0));
        await deflateSequence.Play().AsyncWaitForCompletion();
    }

    public async void OpenResultPanel()
    {
        if (levelFinished)
        {
            resultText.text = "Congrats";
            resultButtonImage.sprite = victoryTexture;
        }
        else
        {
            resultText.text = "Failed";
            resultButtonImage.sprite = failTexture;
        }

        var currentScore = ScoreCounter.Instance.Score;
        var bestScore = levelStates.levels[PlayerPrefsBehaviour.GetCurrentLevelValue() - 1].BestScore;
        
        currentScoreText.text = "Current: " + currentScore;
        if (currentScore < bestScore)
        {
            bestScoreText.text = "Best: " + bestScore;   
        }
        else
        {
            bestScoreText.text = "Best: " + currentScore;
        }
        
        resultPanel.SetActive(true);
        var inflateSequence = DOTween.Sequence();
        inflateSequence.Join(resultPanel.transform.DOScale(Vector3.one, Board.Instance.tweenDuration));
        await inflateSequence.Play().AsyncWaitForCompletion();
    }

    public void UpdateMoveCount()
    {
        moveCountText.text = "Moves: " + numberOfMoves;

        if (numberOfMoves <= 0)
        {
            if (levelFinished == false)
            {
                OpenResultPanel();
            }
        }
    }

    public void ManageNextLevel()
    {
        if (levelFinished)
        {
            levelStates.LoadNextLevel();
        }
        else
        {
            levelStates.LoadSameLevel();
        }
    }
}