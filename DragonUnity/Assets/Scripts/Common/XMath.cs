using UnityEngine;

public static class XMath
{
    public static float rescale(float value, float start, float end, float newStart, float newEnd, bool isClamp = false)
    {
        if (start == end || value == start)
            return newStart;
        else if (value == end)
            return newEnd;
        else {
            float result = (((value - start) / (end - start)) * (newEnd - newStart) + newStart);
            if (isClamp)
                Mathf.Clamp(result, newStart, newEnd);
            return result;
        }
    }

    public static float RepeatScalar(float value, float start = 0, float end = 1)
    {
        if (start == end)
            return start;

        if (start > end) {
            float temp = start;
            start = end;
            end = temp;
        }

        return Mathf.Repeat(value - start, end - start) + start;
    }

    public static int RepeatInteger(int value, int start, int end)
    {
        if (start == end)
            return start;

        if (start > end) {
            int temp = start;
            start = end;
            end = temp;
        }

        int distance = value - start;
        int repeatLength = end - start + 1;
        int firstMod = distance % repeatLength;
        return firstMod + start + (firstMod < 0 ? repeatLength : 0);
    }

}
