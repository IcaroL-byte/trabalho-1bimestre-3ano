using System;

public static class PlayerOM
{
    // Passa (int playerID, int newTotal)
    public static event Action<int, int> ChangeCoins;

    // Disparado no pickup (int playerID, int amount)
    public static event Action<int, int> CoinCollected;

    // Disparado no fim da partida (int winningPlayerID)
    public static event Action<int> GameOver;

    public static void CoinsAreChanged(int playerID, int total)
    {
        ChangeCoins?.Invoke(playerID, total);
    }

    public static void CollectCoin(int playerID, int amount = 1)
    {
        CoinCollected?.Invoke(playerID, amount);
    }

    public static void TriggerGameOver(int winnerID)
    {
        GameOver?.Invoke(winnerID);
    }
}