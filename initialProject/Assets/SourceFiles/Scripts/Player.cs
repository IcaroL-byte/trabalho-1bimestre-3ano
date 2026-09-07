using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public int PlayerID { get; private set; } = 0;

    public void SetupPlayer(int id)
    {
        PlayerID = id;
    }

    private void OnEnable()
    {
        PlayerOM.ChangeCoins += MoedasAlteradas;
    }

    private void OnDisable()
    {
        PlayerOM.ChangeCoins -= MoedasAlteradas;
    }

    // Agora recebe o playerID e a quantidade
    private void MoedasAlteradas(int playerID, int quantidade)
    {
        Debug.Log($"<color=blue>[Player {playerID}]</color> foi notificado! Total de moedas: {quantidade}");
    }
}