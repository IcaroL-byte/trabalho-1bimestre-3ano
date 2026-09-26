using System;
using System.Collections.Generic;
using UnityEngine;

public class CoinEventManager : MonoBehaviour
{
    [Header("Status da Fase")]
    [Tooltip("Quantidade de moedas restantes no mapa.")]
    public int RemainingCoins;

    // Guarda as moedas separadamente para cada PlayerID.
    private Dictionary<int, int> _playerCoins = new Dictionary<int, int>();

    private void Start()
    {
        // Conta automaticamente a quantidade inicial de moedas presentes na cena
        UpdateRemainingCoinsCount();
    }

    /// <summary>
    /// Busca na cena os itens coletáveis e define a contagem inicial de moedas restantes.
    /// </summary>

    public void UpdateRemainingCoinsCount()
    {
        int pickupCount = FindObjectsByType<Pickup>(FindObjectsSortMode.None).Length;

        // Caso seu projeto use também o script 'CoinPickup', soma a quantidade total
        Type coinPickupType = Type.GetType("CoinPickup");
        if (coinPickupType != null)
        {
            pickupCount += FindObjectsByType(coinPickupType, FindObjectsSortMode.None).Length;
        }

        RemainingCoins = pickupCount;
    }

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

        // Subtrai da quantidade global de moedas restantes no mapa
        RemainingCoins = Mathf.Max(0, RemainingCoins - amount);

        int totalDoJogador = _playerCoins[playerID];

        // Envia o ID junto com o total.
        // Somente os componentes daquele PlayerID devem reagir.
        PlayerOM.CoinsAreChanged(playerID, totalDoJogador);

        Debug.Log(
            $"<color=yellow>[CoinEventManager]</color> " +
            $"Player {playerID} recebeu +{amount} moeda(s). " +
            $"Total individual: {totalDoJogador} | Moedas restantes na cena: {RemainingCoins}"
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

            // Recalcula o total de moedas disponíveis no mapa
            UpdateRemainingCoinsCount();

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