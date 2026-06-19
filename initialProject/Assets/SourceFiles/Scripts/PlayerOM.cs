using System;

public static class PlayerOM
{
    public static event Action<int> ChangeCoins;
   

    public static void CoinsAreChanged(int quantidade)
    {
        ChangeCoins?.Invoke(quantidade);
    }
    
}