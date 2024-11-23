using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStates : MonoBehaviour
{
    [SerializeField]
    public Level[] levels;
    
    public void FindAndLoadFirstPlayableLevel()
    {
        foreach (var level in levels)
        {
            if (level.isCompleted == false)
            {
                SceneManager.LoadScene("Level" + level.value);
                PlayerPrefsBehaviour.SetCurrentLevelValue(level.value);
                break;
            }

            PlayerPrefsBehaviour.SetCurrentLevelValue(1);
            SceneManager.LoadScene("Level" + PlayerPrefsBehaviour.GetCurrentLevelValue());
        }
    }

    public void LoadNextLevel()
    {
        if (levels[PlayerPrefsBehaviour.GetCurrentLevelValue() - 1].isCompleted)
        {
            var newValue = PlayerPrefsBehaviour.GetCurrentLevelValue();
            newValue++;
            PlayerPrefsBehaviour.SetCurrentLevelValue(newValue);
            SceneManager.LoadScene("Level" + newValue);
        }
    }

    public void LoadSameLevel()
    {
        SceneManager.LoadScene("Level" + PlayerPrefsBehaviour.GetCurrentLevelValue());
    }

    public void SelectLevel(Level level)
    {
        var levelIndex = Array.IndexOf(levels, level);
        var previousLevelIndex = levelIndex - 1;
        if (previousLevelIndex < 0)
        {
            previousLevelIndex = 0;
        }
        var selectedLevelValue = level.value;
        if (levels[previousLevelIndex].value < 1)
        {
            selectedLevelValue = 1;
        }

        if (levels[previousLevelIndex].isCompleted)
        {
            selectedLevelValue = level.value;
        }

        PlayerPrefsBehaviour.SetCurrentLevelValue(selectedLevelValue);
        SceneManager.LoadScene("Level" + selectedLevelValue);
    }
}
