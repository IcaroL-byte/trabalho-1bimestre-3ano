using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        // Pega o PlayerID de quem realmente encostou no Trigger
        Player player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            // Executa o método correto do PlayerOM passando o ID do jogador atual
            PlayerOM.CollectCoin(player.PlayerID, coinValue);

            Destroy(gameObject);
        }
    }
}