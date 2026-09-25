using System;

public static class PlayerOM
{
    // Eventos do Observer
    public static event Action<int, int> ChangeCoins;
    public static event Action<int, int> CoinCollected;
    public static event Action<int> GameOver;

    /// <summary>
    /// Chamado pelos scripts de coleta (CoinPickup / Pickup) quando um player pega uma moeda.
    /// </summary>
    public static void CollectCoin(int playerID, int amount)
    {
        CoinCollected?.Invoke(playerID, amount);
    }

    /// <summary>
    /// Chamado pelo gerenciador para notificar que o saldo de moedas mudou.
    /// </summary>
    public static void CoinsAreChanged(int playerID, int total)
    {
        ChangeCoins?.Invoke(playerID, total);
    }

    /// <summary>
    /// Chamado para encerrar o jogo informando o ID do vencedor.
    /// </summary>
    public static void EndGame(int winnerID)
    {
        GameOver?.Invoke(winnerID);
    }
}