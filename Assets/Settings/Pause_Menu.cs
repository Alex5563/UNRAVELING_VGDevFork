using UnityEngine;

public class Pause_Menu : MonoBehaviour
{
    public GameObject Container;
    public static bool PauseActive = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(PauseActive)
            {
                Container.SetActive(false);
                Time.timeScale = 1;
                PauseActive = false;
            }
            else
            {
                Container.SetActive(true);
                Time.timeScale = 0;
                PauseActive = true;
            }
        }
    }

    public void ResumeButton() 
    {
        Container.SetActive(false);
        Time.timeScale = 1;
        PauseActive = false;
    }

    public void MainMenuButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("");
    }
}
