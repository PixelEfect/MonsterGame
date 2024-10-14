using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image monsterImage;

    [SerializeField] AudioClip evolutionMusic;


    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;
    public static EvolutionManager i { get; private set; }

    private void Awake()
    {
        
        i = this;
    }
    public IEnumerator Evolve (Monster monster, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive (true);

        AudioManager.i.PlayMusic (evolutionMusic);

        monsterImage.sprite = monster.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} is evolving");

        var oldMonster = monster.Base.Name;
        monster.Evolve (evolution);

        monsterImage.sprite = monster.Base.FrontSprite;

        yield return DialogManager.Instance.ShowDialogText($"{oldMonster} evolved into {monster.Base.Name}");
    
        evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
}
