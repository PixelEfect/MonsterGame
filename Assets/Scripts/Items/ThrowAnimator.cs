using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> animateSprites; // 5 obrazków b³yskawicy
    [SerializeField] float lightningFrameRate = 0.05f; // Szybkoœæ zmiany klatek (dostosuj do potrzeb)

    SpriteAnimator lightningAnim;
    SpriteRenderer spriteRenderer;

    int currentFrame;
    bool isAnimationFinished;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lightningAnim = new SpriteAnimator(animateSprites, spriteRenderer, lightningFrameRate);
        lightningAnim.Start();
        currentFrame = 0;
        isAnimationFinished = false;
    }

    public void PlayLightning()
    {
        lightningAnim.Start();
    }
    private void Update()
    {
        if (!isAnimationFinished)
        {
            lightningAnim.HandleUpdate();

            // Zaktualizuj numer klatki poprzez aktualny sprite w rendererze
            if (spriteRenderer.sprite == animateSprites[currentFrame])
            {
                // Zwiêksz klatkê, ale tylko wtedy, gdy aktualne sprite jest nowy
                currentFrame++;
            }

            // Gdy osi¹gnie koniec listy, uznajemy animacjê za zakoñczon¹
            if (currentFrame >= animateSprites.Count)
            {
                currentFrame = animateSprites.Count - 1; // Ustaw na ostatni¹ klatkê
                isAnimationFinished = true;
                OnAnimationEnd();
            }
        }
    }
    private void OnAnimationEnd()
    { 
        StartCoroutine(WaitAndDestroy(lightningFrameRate));
    }
    public float GetLightningFrameRate()
    {
        return lightningFrameRate;
    }
    public int GetFrameRateCount()
    {
        return animateSprites.Count;
    }
    private IEnumerator WaitAndDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

}
