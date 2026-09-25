using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    [Header("Configurações do Item")]
    [Tooltip("Quantidade de moedas que este item concede ao ser coletado.")]
    [SerializeField] private int coinValue = 1;

    [Tooltip("Efeito sonoro ou visual a ser instanciado ao coletar (opcional).")]
    [SerializeField] private GameObject collectEffect;

    private void OnTriggerEnter(Collider other)
    {
        // Identifica o Player que encostou no trigger (mesmo se o Collider estiver num filho do prefab)
        Player player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            // Pega o ID do jogador (Player 1 ou Player 2) e dispara o evento via método oficial do PlayerOM
            PlayerOM.CollectCoin(player.PlayerID, coinValue);

            // Instancia efeito de coleta se houver um atribuído
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, transform.rotation);
            }

            // Destroi a moeda da cena
            Destroy(gameObject);
        }
    }
}