using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] Dialog dialog;

    [Header("Quests")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;

    [Header("Movement")]
    [SerializeField] List<MovementStep> movementPattern;
    //[Header("Movement")]
    //[SerializeField] List<Vector2> movementPattern;
    //[SerializeField] float timeBetweenPattern;

    NPCState state;
    //float idleTimer = 0f;
    int currentPattern = 0;
    Quest activeQuest;

    Character character;
    ItemGiver itemGiver;
    MonsterGiver monsterGiver;
    Healer healer;
    Merchant merchant;
    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        monsterGiver = GetComponent<MonsterGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
    }

    //w tym przypadku inicjatorem jest player wiec to on jest inicjowany za pomoca transform initiator
    public IEnumerator Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            if (questToComplete != null && monsterGiver == null)
            {
                var quest = new Quest(questToComplete);
                yield return quest.CompleteQuest(initiator);
                questToComplete = null;

                Debug.Log($"{quest.Base.name} completed");
            }
            else if (questToComplete != null && monsterGiver != null && monsterGiver.CanBeGiven())
            {
                // Przekazujemy zadanie do `GiveMonster`, aby mog³o byæ zakoñczone, jeœli gracz przyjmie potwora
                yield return monsterGiver.GiveMonster(initiator.GetComponent<PlayerController>(), dialog, questToComplete, initiator);

                // Reset questToComplete tylko jeœli gracz przyj¹³ potwora (to jest obs³ugiwane wewn¹trz `GiveMonster`)
                if (monsterGiver.CanBeGiven() == false)
                {
                    questToComplete = null;
                }
            }
            else if (monsterGiver != null && monsterGiver.CanBeGiven())
            {
                yield return monsterGiver.GiveMonster(initiator.GetComponent<PlayerController>(), dialog, questToComplete, initiator);//dodany dialog
            }

            else if (itemGiver != null && itemGiver.CanBeGiven()) // zmieniono na else if zamiast if
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            }

            else if (questToStart != null)
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null;

                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
            }

            else if (activeQuest != null)
            {
                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
                else
                {
                    yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialogue);
                }
            }

            else if (healer != null)
            {
                yield return healer.Heal(initiator, dialog);
            }

            else if (merchant != null)
            {
                yield return merchant.Trade();
            }

            else
            {
                yield return DialogManager.Instance.ShowDialog(dialog);
            }
            //idleTimer = 0f;
            state = NPCState.Idle;
        }
        //StartCoroutine(character.Move(new Vector2(0, 2)));  przesuwanie obiektu;)
    }

    private void Update()
    {
        if (state == NPCState.Idle)
        {
            if (movementPattern.Count > 0)
            {
                StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        // Pobierz obecny krok ruchu (kierunek + czas trwania)
        var movementStep = movementPattern[currentPattern];
        var oldPos = transform.position;

        // Rozpocznij ruch w danym kierunku
        yield return character.Move(movementStep.direction);

        // Po zakoñczeniu ruchu, jeœli NPC siê przemieœci³, zaktualizuj obecny wzorzec
        if (transform.position != oldPos)
        {
            // Ustaw timer na czas trwania tego etapu ruchu
            yield return new WaitForSeconds(movementStep.duration);

            // PrzejdŸ do kolejnego kroku w patternie
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        }

        state = NPCState.Idle;
    }

    public object CaptureState()
    {
        var saveData = new NPCQuesstSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();

        if (questToStart != null)
        {
            saveData.questToStart = (new Quest(questToStart)).GetSaveData();
        }
        if (questToComplete != null)
        {
            saveData.questToComplete = (new Quest(questToComplete)).GetSaveData();
        }
        return saveData;

    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCQuesstSaveData;
        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null)? new Quest( saveData.activeQuest) : null;
            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}
[System.Serializable]
public class MovementStep
{
    public Vector2 direction; // Kierunek ruchu
    public float duration;    // Czas trwania tego ruchu (w sekundach)
}

[System.Serializable]
public class NPCQuesstSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState { Idle, Walking, Dialog }