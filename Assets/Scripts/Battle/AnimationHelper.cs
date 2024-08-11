using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper
{
    public static float AnimateRadius(Vector3 enemyUnitPosition, GameObject radiusSprite)
    {
        Vector3 radiusSize = radiusSprite.GetComponent<SpriteRenderer>().bounds.size;
        var radiusObject = Object.Instantiate(radiusSprite, enemyUnitPosition, Quaternion.identity);

        float newXPosition = enemyUnitPosition.x - radiusSize.x / 2.8f;
        float newYPosition = enemyUnitPosition.y - radiusSize.y / 2.8f;
        radiusObject.transform.position = new Vector3(newXPosition, newYPosition, radiusObject.transform.position.z);

        ThrowAnimator throwAnimator = radiusObject.GetComponent<ThrowAnimator>(); 
        var shakeTime = throwAnimator.GetFrameRateCount() / 2;
        return throwAnimator.GetLightningFrameRate() * shakeTime;
    }

    public static float AnimateSphere(int shakeCount, Vector3 enemyUnitPosition, GameObject sphereSpriteS, GameObject sphereSprite1, GameObject sphereSprite2, GameObject sphereSprite3)
    {
        GameObject sphereSprite;

        if (shakeCount == 4)
        {
            sphereSprite = sphereSpriteS;
        }
        else if (shakeCount <= 1)
        {
            sphereSprite = sphereSprite1;
        }
        else if (shakeCount == 2)
        {
            sphereSprite = sphereSprite2;
        }
        else
        {
            sphereSprite = sphereSprite3;
        }
        Debug.Log($"{shakeCount}");

        var sphereObject = Object.Instantiate(sphereSprite, enemyUnitPosition, Quaternion.identity);
        ThrowAnimator throwAnimator = sphereObject.GetComponent<ThrowAnimator>();
        var shakeTime = throwAnimator.GetFrameRateCount();
        return throwAnimator.GetLightningFrameRate() * shakeTime;
    }
}

