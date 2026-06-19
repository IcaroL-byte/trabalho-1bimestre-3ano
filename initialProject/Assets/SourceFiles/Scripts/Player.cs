using UnityEngine;

public class Player : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerOM.ChangeCoins += MoedasAlteradas;
    }

    private void OnDisable()
    {
        PlayerOM.ChangeCoins -= MoedasAlteradas;
    }

    private void MoedasAlteradas(int quantidade)
    {
        Debug.Log($"<color=blue>[Player]</color> foi notificado! Total de moedas: {quantidade}");
    }
}