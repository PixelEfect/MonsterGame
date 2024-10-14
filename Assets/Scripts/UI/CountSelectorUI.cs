using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] Text countText;
    [SerializeField] Text priceText;

    bool selected;
    int currentCount;

    int maxCount;
    float pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit,
        Action<int> onCountSelected )
    {
        this.maxCount = maxCount;
        this.pricePerUnit = pricePerUnit;

        selected = false;
        currentCount = 1;

        gameObject.SetActive (true);
        SetValues ();

        yield return new WaitUntil(() => selected == true);

        onCountSelected?.Invoke(currentCount);
        gameObject.SetActive (false);
    }

    private void Update()
    {
        int prevCount = currentCount;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentCount;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentCount;
        }

        if (currentCount > maxCount) 
        {
            currentCount = 1;
        }
        else if (currentCount < 1)
        {
            currentCount = maxCount;
        }

        if (currentCount != prevCount) 
        {
            SetValues();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            selected = true;
        }
    }

    void SetValues()
    {
        countText.text = "x " + currentCount;
        priceText.text = "$ " + pricePerUnit * currentCount;
    }
}
