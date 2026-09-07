using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private string prefixo = "Moedas: ";
    [SerializeField] private int targetPlayerID = 0;

    private void OnEnable()
    {
        PlayerOM.ChangeCoins += AtualizarUI;
    }

    private void OnDisable()
    {
        PlayerOM.ChangeCoins -= AtualizarUI;
    }

    private void Start()
    {
        if (coinText != null)
        {
            coinText.color = Color.yellow;
        }
        AtualizarUI(targetPlayerID, 0);
    }

    private void AtualizarUI(int playerID, int total)
    {
        if (playerID == targetPlayerID && coinText != null)
        {
            coinText.text = $"{prefixo}{total}";
        }
    }
}