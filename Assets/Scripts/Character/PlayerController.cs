using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, Interactable
{
    [SerializeField] string playerName;
    [SerializeField] Sprite sprite;

    const float offsetY = 0.3f;

    public event Action OnEncountered;
    public event Action<Collider2D> OnEnterTrainersView;

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
            Interact();
        }
    }
    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f); ne dziala
        //Debug.Log($"FacingDir: {facingDir}, InteractPos: {interactPos}"); ale to dziala i zwraca dobre wartosci

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }
    private void OnMoveOver()
    {
        CheckForEncounters();
        CheckIfInTrainersView();
    }
    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position - new Vector3(0, offsetY), 0.2f, GameLayers.i.GrassLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 101) <= 10)
            {
                character.Animator.IsMoving = false;
                OnEncountered();
            }
        }
    }

    private void CheckIfInTrainersView()
    {   
        var collider = Physics2D.OverlapCircle(transform.position - new Vector3(0, offsetY), 0.2f, GameLayers.i.FovLayer);
        if (collider != null)
        {
            character.Animator.IsMoving = false;
            OnEnterTrainersView?.Invoke(collider);
        }
    }

    public void Interact(Transform initiator)
    {
        throw new System.NotImplementedException();
    }

    public string Name
    {
        get => playerName;
    }
    public Sprite Sprite
    {
        get => sprite;
    }
}
