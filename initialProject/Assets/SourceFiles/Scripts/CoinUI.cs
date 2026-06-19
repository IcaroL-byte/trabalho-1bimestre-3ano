using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private string prefixo = "Moedas: ";

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
        CoinEventManager.ResetCoins();

        coinText.color = Color.yellow;

        AtualizarUI(0);
    }

    private void AtualizarUI(int total)
    {
        coinText.text = $"{prefixo}{total}";
    }
}