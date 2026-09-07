using UnityEngine;

public class CoinEventManager : MonoBehaviour
{
    private int _totalCoins = 0;

    public int GetCurrentTotal() => _totalCoins;

    public void AddCoins(int playerID, int amount)
    {
        _totalCoins += amount;

        // Dispara o evento atualizado repassando ID e Total
        PlayerOM.CoinsAreChanged(playerID, _totalCoins);

        Debug.Log($"<color=yellow>[CoinEventManager]</color> Jogador {playerID} +{amount} moedas | Novo Total: {_totalCoins}");
    }

    private void OnEnable()
    {
        PlayerOM.CoinCollected += AddCoins;
    }

    private void OnDisable()
    {
        PlayerOM.CoinCollected -= AddCoins;
    }

    public void ResetCoins(int playerID = 0)
    {
        _totalCoins = 0;
        PlayerOM.CoinsAreChanged(playerID, _totalCoins);
    }
}