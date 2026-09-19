using System.Collections.Generic;
using UnityEngine;

public class CoinEventManager : MonoBehaviour
{
    // Guarda as moedas separadamente para cada PlayerID.
    // Exemplo:
    // Player 1 = 5 moedas
    // Player 2 = 3 moedas
    private Dictionary<int, int> _playerCoins = new Dictionary<int, int>();

    /// <summary>
    /// Retorna o total de moedas de um jogador específico.
    /// </summary>
    public int GetCurrentTotal(int playerID)
    {
        if (_playerCoins.TryGetValue(playerID, out int coins))
        {
            return coins;
        }

        return 0;
    }

    private void OnEnable()
    {
        PlayerOM.CoinCollected += AddCoins;
    }

    private void OnDisable()
    {
        PlayerOM.CoinCollected -= AddCoins;
    }

    /// <summary>
    /// Adiciona moedas somente ao jogador informado pelo PlayerID.
    /// </summary>
    private void AddCoins(int playerID, int amount)
    {
        // ID inválido não deve receber moedas.
        if (playerID <= 0)
        {
            Debug.LogWarning(
                $"[CoinEventManager] Tentativa de adicionar moedas para PlayerID inválido: {playerID}"
            );

            return;
        }

        // Se o jogador ainda não existe no dicionário,
        // cria o registro começando em zero.
        if (!_playerCoins.ContainsKey(playerID))
        {
            _playerCoins.Add(playerID, 0);
        }

        // Adiciona as moedas SOMENTE ao PlayerID recebido.
        _playerCoins[playerID] += amount;

        int totalDoJogador = _playerCoins[playerID];

        // Envia o ID junto com o total.
        // Somente os componentes daquele PlayerID devem reagir.
        PlayerOM.CoinsAreChanged(playerID, totalDoJogador);

        Debug.Log(
            $"<color=yellow>[CoinEventManager]</color> " +
            $"Player {playerID} recebeu +{amount} moeda(s). " +
            $"Total individual: {totalDoJogador}"
        );
    }

    /// <summary>
    /// Reseta as moedas de um jogador específico.
    /// Se playerID for 0, reseta todos.
    /// </summary>
    public void ResetCoins(int playerID = 0)
    {
        // Resetar todos os jogadores.
        if (playerID == 0)
        {
            List<int> playerIDs = new List<int>(_playerCoins.Keys);

            foreach (int id in playerIDs)
            {
                _playerCoins[id] = 0;

                // Avisa somente aquele jogador.
                PlayerOM.CoinsAreChanged(id, 0);
            }

            Debug.Log(
                "<color=orange>[CoinEventManager]</color> " +
                "Moedas de todos os jogadores foram resetadas."
            );

            return;
        }

        // Resetar somente um jogador.
        if (_playerCoins.ContainsKey(playerID))
        {
            _playerCoins[playerID] = 0;

            PlayerOM.CoinsAreChanged(playerID, 0);

            Debug.Log(
                $"<color=orange>[CoinEventManager]</color> " +
                $"Moedas do Player {playerID} foram resetadas."
            );
        }
    }
}