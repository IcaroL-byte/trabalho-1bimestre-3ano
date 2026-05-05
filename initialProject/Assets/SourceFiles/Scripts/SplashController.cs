using UnityEngine;
using System.Collections;

public class SplashController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Splash());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Splash()
    {
        
        yield return new WaitForSeconds(2f);
        if (GameMenager.Instance != null)
        {
            GameMenager.Instance.CarregarCena("MenuPrincipal");
        }
    }
}
