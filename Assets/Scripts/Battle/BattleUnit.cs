using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;


public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }
    public BattleHud Hud 
    { 
        get { return hud; } 
    }

    public Monster Monster {  get; set; }

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }
    public void Setup(Monster monster)
    {
        Monster = monster;
        if (isPlayerUnit)
        {
            image.sprite = Monster.Base.BackSprite;
        }
        else
        {
            image.sprite = Monster.Base.FrontSprite;
        }

        transform.localScale = new Vector3 (1, 1, 1);
        hud.gameObject.SetActive(true);
        hud.SetData(monster);

        image.color = originalColor;
        PlayEnterAnimation();
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-1250f, originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(1200f, originalPos.y);
        }

        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();

        if (isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 80f, 0.25f));
            sequence.Join(image.transform.DOLocalMoveY(originalPos.y + 40f, 0.25f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 80f, 0.25f));
            sequence.Join(image.transform.DOLocalMoveY(originalPos.y - 40f, 0.25f));
        }

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
        sequence.Join(image.transform.DOLocalMoveY(originalPos.y, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));

    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 200f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }

    public void PlayCaptureAnimation()
    {
        Vector3 targetScale = image.transform.localScale * 0.3f;
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0,2f));
        sequence.Join(image.transform.DOScale(targetScale, 2f));
    }

    public IEnumerator PlayBrakeOutAnimation(Vector3 originalScale)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1, 0.5f));
        sequence.Join(image.transform.DOScale(originalScale, 0.5f));
        yield return sequence.WaitForCompletion();
    }
}
