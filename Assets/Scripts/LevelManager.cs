using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Target levelTarget;
    public Image[] targetIcons;
    public Text[] targetAmounts;

    public GameObject victoryPanel;
    
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
        await deflateSequence.Play().AsyncWaitForCompletion();
    }
}
