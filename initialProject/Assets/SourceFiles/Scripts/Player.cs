using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("ID único do jogador (ex: 1 para Player 1, 2 para Player 2)")]
    public int PlayerID = 1;

    [Header("Stats")]
    public int TotalMoedas { get; private set; } = 0;

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

    private void MoedasAlteradas(int targetPlayerID, int totalCoins)
    {
        // Só atualiza se a notificação for para este PlayerID específico
        if (targetPlayerID == PlayerID)
        {
            TotalMoedas = totalCoins;
            Debug.Log($"<color=cyan>[Player {PlayerID}]</color> Moeda coletada! Total individual: {TotalMoedas}");
        }
    }
}