using System;

public static class PlayerOM
{
    public static event Action<int> ChangeCoins;
    
    public static event Action<int> CoinCollected;

    
    public static void CoinsAreChanged(int quantidade)
    {
        ChangeCoins?.Invoke(quantidade);
    }

    public static void CollectCoin(int amount)
    {
        CoinCollected?.Invoke(amount);
    }

}