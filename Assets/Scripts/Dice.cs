using UnityEngine;

public class Dice
{
    public int Faces { get; } = 6;
    public int Value { get; private set; } = 0;
    public bool IsSelected { get; private set; }
    public bool IsHeld { get; private set; } = false;

    public void Roll()
    {
        Value = Random.Range(1, Faces + 1);
    }

    public void SetHeld()
    {
        IsHeld = true;
        IsSelected = false;
    }
    
    public void Select()
    {
        if (IsHeld)
        {
            return;
        }
        
        IsSelected = true;
    }
    
    public void Deselect()
    {
        IsSelected = false;
    }
    
    public void Reset()
    {
        Value = 0;
        IsSelected = false;
        IsHeld = false;
    }    
}