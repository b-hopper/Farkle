using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class DieView : MonoBehaviour
{
    [SerializeField] private Sprite[] dieFaces;
    [SerializeField] private Sprite noneSprite;
    [SerializeField] private Image dieImage;
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private GameObject heldIndicator;
    
    [SerializeField] private bool debugMode;
    [SerializeField] private TextMeshProUGUI debugText;
    
    private Dice _dice;
    
    private int _index;
    
    private void Start()
    {
        SetDebugTextVisible(debugMode);
        
        _index = Array.IndexOf(ViewController.Instance.dieViews, this);
        
        if (_index == -1)
        {
            FarkleLogger.LogError("DieView not found in ViewController.");
        }
    }

    public void SetFaceValue(int value)
    {
        if (debugMode)
        {
            SetDebugText(value.ToString());
        }
        
        if (dieImage == null || 
            dieFaces.Length == 0 || 
            value < 1 || 
            value > dieFaces.Length)
        {
            dieImage.sprite = noneSprite;
            return;
        }
        
        dieImage.sprite = dieFaces[value - 1];
    }
    
    private void SetDebugText(string text)
    {
        if (debugText == null)
        {
            return;
        }
        
        debugText.text = text;
    }
    
    public void SetSelected(bool selected)
    {
        if (selectedIndicator == null)
        {
            return;
        }
        
        selectedIndicator.SetActive(selected);
    } 
    
    public void SetHeld(bool held)
    {
        if (heldIndicator == null)
        {
            return;
        }

        heldIndicator.SetActive(held);
    }
        
    public void SetDebugTextVisible(bool visible)
    {
        if (debugText == null)
        {
            return;
        }
        
        debugText.gameObject.SetActive(visible);
    }
    
    public void SetDice(Dice dice)
    {
        _dice = dice;
    }
    
    public void UpdateFromDice()
    {
        SetFaceValue(_dice.Value);
        SetSelected(_dice.IsSelected);
        SetHeld(_dice.IsHeld);
    }

    public void DoRollDiceAnimation(float duration)
    {
        if (dieImage == null)
        {
            return;
        }
        
        StartCoroutine(DoRollDiceImageChange(duration * 0.5f));
        
        dieImage.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
        
        int fullRotations = UnityEngine.Random.Range(1, 4);
        dieImage.transform.DORotate(new Vector3(0, 0, 360 * fullRotations), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuint);
        
        dieImage.transform.DOShakePosition(duration, 50f, 10, 180, false)
            .SetEase(Ease.OutQuint) 
            .OnComplete(() => dieImage.transform.DOLocalMove(Vector3.zero, 0.1f));
        
        dieImage.transform.localScale = Vector3.one * 0.5f;
        dieImage.transform.DOScale(new Vector3(1f, 1f, 1f), duration)
            .SetEase(Ease.OutQuint)
            .OnComplete(() => dieImage.transform.DOScale(Vector3.one, 0.1f));
    }
    
    private IEnumerator DoRollDiceImageChange(float duration)
    {
        float startTime = Time.time; // Track the start time
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            int value = UnityEngine.Random.Range(1, dieFaces.Length + 1);
            SetFaceValue(value);
        
            // Calculate progress as a percentage of total duration
            float progress = elapsedTime / duration;

            // Apply the Ease.OutQuint formula for smooth slowing down
            float easedProgress = 1 - Mathf.Pow(1 - progress, 5);

            // Calculate the delay based on eased progress
            float minDelay = 0.0025f;
            float maxDelay = 0.35f;
            float delay = Mathf.Lerp(minDelay, maxDelay, easedProgress);
            
            if (elapsedTime + delay >= duration)
            {
                // Ensure the final face is set, but make it feel natural
                SetFaceValue(_dice.Value);
            }

            // Wait for the calculated delay
            yield return new WaitForSeconds(delay);

            // Update the elapsed time by calculating the time since the start
            elapsedTime = Time.time - startTime;
        }

        // Ensure the final face is set
        SetFaceValue(_dice.Value);
    }
}
