using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Effects")]
    public GameObject particleEffectPrefab;

    [Header("Motion Settings")]
    public float rotationSpeed = 100f;
    public float bobbingAmount = 0.1f;
    public float bobbingSpeed = 1f;

    private Vector3 startPosition;
    private float timer;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Rotação da moeda
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );

        // Movimento de sobe e desce
        timer += Time.deltaTime * bobbingSpeed;

        float newY =
            startPosition.y +
            Mathf.Sin(timer) * bobbingAmount;

        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        // Só jogadores podem coletar.
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Procura o Player no próprio objeto
        // ou nos objetos pais.
        Player player = other.GetComponentInParent<Player>();

        // Se não encontrou, procura nos filhos.
        if (player == null)
        {
            player = other.GetComponentInChildren<Player>();
        }

        // NÃO assume Player 1 se não encontrar.
        if (player == null)
        {
            Debug.LogWarning(
                $"[Pickup] Não foi possível identificar o Player " +
                $"que tentou coletar a moeda. " +
                $"Objeto: {other.gameObject.name}"
            );

            return;
        }

        // Garante que o ID seja válido.
        if (player.PlayerID <= 0)
        {
            Debug.LogWarning(
                $"[Pickup] PlayerID inválido ({player.PlayerID}) " +
                $"no objeto {player.gameObject.name}."
            );

            return;
        }

        // Pega o ID REAL do jogador que tocou na moeda.
        int playerID = player.PlayerID;

        Debug.Log(
            $"<color=green>[Pickup]</color> " +
            $"Moeda coletada pelo Player {playerID}."
        );

        // Envia a moeda SOMENTE para esse PlayerID.
        PlayerOM.CollectCoin(playerID, 1);

        // Efeito visual.
        if (particleEffectPrefab != null)
        {
            Instantiate(
                particleEffectPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // Remove a moeda.
        Destroy(gameObject);
    }
}

