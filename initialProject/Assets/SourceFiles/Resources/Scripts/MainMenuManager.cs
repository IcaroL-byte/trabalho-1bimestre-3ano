using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string levelName;
      

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NewGame()
    {
        GameMenager.Instance.CarregarCena(levelName);
    }
   
    public void Exit()
    {
        GameMenager.Instance.SairDoJogo();
    }
}
