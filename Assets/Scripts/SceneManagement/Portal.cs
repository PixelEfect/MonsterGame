using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.TextCore.Text;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIndentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        Debug.Log("Player triggered portal.");
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    public bool TriggerRepeatedly => false;

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
        Debug.Log("Fader initialized: " + (fader != null));
    }

    IEnumerator SwitchScene()
    {
        Debug.Log("Starting scene switch to scene index: " + sceneToLoad);

        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);
        Debug.Log("Game paused for scene transition.");

        yield return fader.FadeIn(0.5f);
        Debug.Log("Fade in completed.");

        yield return SceneManager.LoadSceneAsync(sceneToLoad);
        Debug.Log("Scene loaded: " + sceneToLoad);

        var destPortal = FindObjectsOfType<Portal>().FirstOrDefault(x => x != this && x.destinationPortal == this.destinationPortal);
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
        Debug.Log("Game unpaused after scene transition.");

        Destroy(gameObject);
        Debug.Log("Portal object destroyed.");
    }
    public Transform SpawnPoint => spawnPoint;
}
public enum DestinationIndentifier { A, B, C, D, E }
