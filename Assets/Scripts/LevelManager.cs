using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void SelectLevel(Level level)
    {
        SceneManager.LoadScene("Level" + level.value);
    }
}
