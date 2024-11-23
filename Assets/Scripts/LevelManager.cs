using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Target levelTarget;
    public Image[] targetIcons;
    public Text[] targetAmounts;

    public GameObject victoryPanel;
    public GameObject failPanel;
    public Text moveCountText;
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
        deflateSequence.Join(victoryPanel.transform.DOScale(Vector3.zero, 0));
        deflateSequence.Join(failPanel.transform.DOScale(Vector3.zero, 0));
        await deflateSequence.Play().AsyncWaitForCompletion();
    }

    private async void OpenFailPanel()
    {
        failPanel.SetActive(true);
        var inflateSequence = DOTween.Sequence();
        inflateSequence.Join(failPanel.transform.DOScale(Vector3.one, Board.Instance.tweenDuration));
        await inflateSequence.Play().AsyncWaitForCompletion();
    }

    public void UpdateMoveCount()
    {
        moveCountText.text = "Moves: " + numberOfMoves;

        if (numberOfMoves <= 0)
        {
            if (levelFinished == false)
            {
                OpenFailPanel();
            }
        }
    }
}