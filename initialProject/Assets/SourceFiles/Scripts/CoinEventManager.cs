using UnityEngine;

public static class CoinEventManager
{
    private static int _totalCoins = 0;

    public static int GetCurrentTotal() => _totalCoins;

    public static void AddCoins(int amount)
    {
        _totalCoins += amount;

        
        PlayerOM.CoinsAreChanged(_totalCoins);
        
        Debug.Log($"<color=yellow>[CoinEventManager]</color> +{amount} moedas | Total: {_totalCoins}");
    }

    public static void ResetCoins()
    {
        _totalCoins = 0;

        
        PlayerOM.CoinsAreChanged(_totalCoins);
    }
}