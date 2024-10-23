using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class InteractableObject : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog; // Referencja do obiektu dialogu

    // Metoda Interact, która wyœwietla dialog
    public IEnumerator Interact(Transform initiator)
    {
        if (dialog != null && dialog.Lines != null)
        {
            foreach (string line in dialog.Lines)
            {
                // Wyœwietl ka¿d¹ liniê dialogu po kolei
                yield return DialogManager.Instance.ShowDialogText(line);

                // Mo¿esz dodaæ jak¹œ pauzê miêdzy liniami, jeœli tego wymagasz
                // yield return new WaitForSeconds(1f); // Dodanie pauzy 1 sekundy, jeœli potrzebujesz
            }
        }
        else
        {
            Debug.LogWarning("No dialog or empty dialog assigned to this InteractableObject.");
        }
    }
}
