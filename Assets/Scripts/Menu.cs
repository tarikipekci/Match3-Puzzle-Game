using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
   public void OpenGameScene()
   {
      SceneManager.LoadScene("Game");
   }

   public void QuitGame()
   {
      Application.Quit();
   }
}
