using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string playerName;
    [SerializeField] Sprite sprite;

    private Vector2 input;
    // Start is called before the first frame update
    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }
    void Start()
    {

    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // remove diagonal movement
            if (input.x != 0)
            {
                input.y = 0;
            }

            if (input != Vector2.zero)
            {
                StartCoroutine (character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine (Interact());
        }
    }
    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f); ne dziala
        //Debug.Log($"FacingDir: {facingDir}, InteractPos: {interactPos}"); ale to dziala i zwraca dobre wartosci

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }
    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);
        
        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }

    public IEnumerator Interact(Transform initiator)
    {
        throw new System.NotImplementedException();
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            monsters = GetComponent<MonsterParty>().Monsters.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        //Restore Party
        GetComponent<MonsterParty>().Monsters = saveData.monsters.Select(s=> new Monster(s)).ToList();
    }

    public string Name
    {
        get => playerName;
    }
    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;
}
[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<MonsterSaveData> monsters;
}
