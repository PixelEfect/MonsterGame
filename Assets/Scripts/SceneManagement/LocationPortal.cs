using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

// Teleports the player to a different position without switching scenes
public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] DestinationIndentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        Debug.Log("Player triggered location portal.");
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    public bool TriggerRepeatedly => false;

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
        Debug.Log("Fader initialized: " + (fader != null));
    }

    IEnumerator Teleport()
    {
        Debug.Log("Starting teleportation to destination portal: " + destinationPortal);

        GameController.Instance.PauseGame(true);
        Debug.Log("Game paused for teleportation.");

        yield return fader.FadeIn(0.5f);
        Debug.Log("Fade in completed.");

        var destPortal = FindObjectsOfType<LocationPortal>().FirstOrDefault(x => x != this && x.destinationPortal == this.destinationPortal);
        if (destPortal != null)
        {
            player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
            Debug.Log("Player position set to destination portal.");
        }
        else
        {
            Debug.LogError("Destination portal not found!");
        }

        yield return fader.FadeOut(0.5f);
        Debug.Log("Fade out completed.");

        GameController.Instance.PauseGame(false);
        Debug.Log("Game unpaused after teleportation.");
    }

    public Transform SpawnPoint => spawnPoint;
}
