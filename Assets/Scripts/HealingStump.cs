using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingStump : MonoBehaviour
{
    public int healSpeed = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(Heal(collision.GetComponent<PlayerController>()));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        StopCoroutine(Heal(collision.GetComponent<PlayerController>()));
    }

    IEnumerator Heal(PlayerController player)
    {
        while (PlayerController.pokemon.HP != PlayerController.pokemon.MaxHp)
        {
            PlayerController.pokemon.HP += healSpeed;
            player.UI.UpdateUI();
            yield return new WaitForSeconds(1);
        }
    }
}
