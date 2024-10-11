using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable, ISavable
{

    [SerializeField] ItemBase item;
    PlayerController player;
    public bool Used { get; set; } = false;



    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);
        }
        Used = true;

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;

        string playerName = initiator.GetComponent<PlayerController>().PlayerName;

        yield return DialogManager.Instance.ShowDialogText($"{playerName} found {item.ItemName}");
    }
    public object CaptureState()
    {
        return Used;
    }
    public void RestoreState(object state)
    {
        Used = (bool)state;

        if (Used)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
