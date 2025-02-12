using UnityEngine;

public class DoubleInt
{
    public int first;
    public int second;

    public DoubleInt(int firstInt, int secondInt)
    {
        first = firstInt;
        second = secondInt;
    }

    public DoubleInt()
    {
        first = 0;
        second = 0;
    }

    public float GetBothAsFloat()
    {
        return first + (second / Mathf.Pow(10f, second.ToString().Length));
    }

    public string GetBothAsString(string separator) 
    {
        return $"{first}{separator}{second}";
    }

}
