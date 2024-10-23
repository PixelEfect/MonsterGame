using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class InteractableObject : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog; // Referencja do obiektu dialogu

    // Metoda Interact, kt�ra wy�wietla dialog
    public IEnumerator Interact(Transform initiator)
    {
        if (dialog != null && dialog.Lines != null)
        {
            foreach (string line in dialog.Lines)
            {
                // Wy�wietl ka�d� lini� dialogu po kolei
                yield return DialogManager.Instance.ShowDialogText(line);

                // Mo�esz doda� jak�� pauz� mi�dzy liniami, je�li tego wymagasz
                // yield return new WaitForSeconds(1f); // Dodanie pauzy 1 sekundy, je�li potrzebujesz
            }
        }
        else
        {
            Debug.LogWarning("No dialog or empty dialog assigned to this InteractableObject.");
        }
    }
}
