using UnityEngine;

public class DontDestroyOnLoadManager : MonoBehaviour
{
    private void Awake()
    {
        var existingObjects = FindObjectsOfType<DontDestroyOnLoadManager>();
        foreach (var obj in existingObjects)
        {
            if (obj != this && obj.name == name)
            {
                Destroy(gameObject); 
                return;
            }
        }

        DontDestroyOnLoad(gameObject);
    }
}