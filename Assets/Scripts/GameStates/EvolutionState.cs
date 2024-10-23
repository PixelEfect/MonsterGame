using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionState : State<GameController>
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image monsterImage;

    [SerializeField] AudioClip evolutionMusic;

    public static EvolutionState i { get; private set; }

    private void Awake()
    {
        
        i = this;
    }
    public IEnumerator Evolve (Monster monster, Evolution evolution)
    {
        var gc = GameController.Instance;
        gc.StateMachine.Push(this);

        evolutionUI.SetActive (true);

        AudioManager.i.PlayMusic (evolutionMusic);

        monsterImage.sprite = monster.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} is evolving");

        var oldMonster = monster.Base.Name;
        monster.Evolve (evolution);

        monsterImage.sprite = monster.Base.FrontSprite;

        yield return DialogManager.Instance.ShowDialogText($"{oldMonster} evolved into {monster.Base.Name}");
    
        evolutionUI.SetActive(false);

        gc.PartyScreen.SetPartyData();

        AudioManager.i.PlayMusic(gc.CurrentScene.SceneMusic, fade: true);

        var prevState = gc.StateMachine.GetPrevState();

        //if (prevState == CutsceneState.i)//chyba dziala ale do poprawy w przyszlosci
        //{
        //    gc.StateMachine.Pop();
        //}
        gc.StateMachine.Pop();
    }
}
