using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("I am in");
    }

    public void Menu() 
    {
        SceneManager.LoadScene("Menu");
    }
}
