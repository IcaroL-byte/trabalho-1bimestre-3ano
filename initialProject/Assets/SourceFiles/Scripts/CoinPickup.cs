using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private int coinValue = 1;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerOM.CollectCoin(coinValue);
            Destroy(gameObject);
        }
    }
}