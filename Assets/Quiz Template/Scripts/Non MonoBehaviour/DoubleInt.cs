using System;

[Serializable]
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

    public DoubleInt((int, int) tuple)
    {
        first = tuple.Item1;
        second = tuple.Item2;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        DoubleInt other = (DoubleInt) obj;
        return first == other.first && second == other.second;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(first, second);
    }

    public string ToString(string separator)
    {
        return $"{first}{separator}{second}";
    }

    public float ToFloat()
    {
        return (float) ToDouble();
    }

    public double ToDouble()
    {
        return first + (second / Math.Pow(10f, second.ToString().Length));
    }

    public (int, int) ToTuple()
    {
        return new Tuple<int, int>(first, second).ToValueTuple();
    }

}
