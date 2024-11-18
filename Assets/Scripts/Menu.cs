using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
   public GameObject levelSelectionPanel;
   private bool isLevelSelectionOpened;
   
   public void OpenGameScene()
   {
      SceneManager.LoadScene("Level1");
   }

   public void OpenLevelSelection()
   {
      if (isLevelSelectionOpened == false)
      {
         levelSelectionPanel.SetActive(true);
         isLevelSelectionOpened = true;
      }
   }

   public void CloseLevelSelection()
   {
      if (isLevelSelectionOpened)
      {
         levelSelectionPanel.SetActive(false);
         isLevelSelectionOpened = false;
      }
   }
   
   public void SelectLevel(Level level)
   {
      SceneManager.LoadScene("Level" + level.value);
   }
   
   public void QuitGame()
   {
      Application.Quit();
   }
}
