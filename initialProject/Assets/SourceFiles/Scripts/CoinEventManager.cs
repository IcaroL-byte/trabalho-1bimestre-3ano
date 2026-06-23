using UnityEngine;

public class CoinEventManager: MonoBehaviour
{
    private int _totalCoins = 0;

    public int GetCurrentTotal() => _totalCoins;

    public void AddCoins(int amount)
    {
        _totalCoins += amount;

        
        PlayerOM.CoinsAreChanged(_totalCoins);
        
        Debug.Log($"<color=yellow>[CoinEventManager]</color> +{amount} moedas | Total: {_totalCoins}");
    }
    
    private void OnEnable()
    {
        PlayerOM.CoinCollected += AddCoins;
    }

    private void OnDisable()
    {
        PlayerOM.CoinCollected -= AddCoins;
    }
    
    public void ResetCoins()
    {
        _totalCoins = 0;

        
        PlayerOM.CoinsAreChanged(_totalCoins);
    }
}