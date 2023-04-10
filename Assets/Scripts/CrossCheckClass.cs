using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CrossCheckClass
{
    public static bool CheckDotInLine(Vector3 a, Vector3 b, Vector3 dot)
    {
        float error = 0.001f;
        float dAB = Vector3.Distance(a, b);
        float dADot = Vector3.Distance(a, dot);
        float dBDot = Vector3.Distance(b, dot);

        return ((dAB + error) >= (dADot + dBDot));
    }

    public static bool CrossCheck(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 intersection)
    {
        float x1, x2, x3, x4, y1, y2, y3, y4, X, Y;

        x1 = a.x; y1 = a.z;
        x2 = b.x; y2 = b.z;
        x3 = c.x; y3 = c.z;
        x4 = d.x; y4 = d.z;

        float cross = ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));

        // 평행한 경우        
        if (cross == 0)
        {
            intersection = Vector3.zero;
            return false;
        }

        // 교점을 구한다
        X = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / cross;
        Y = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / cross;
        intersection = new Vector3(X, 0, Y);

        return
            CheckDotInLine(a, b, new Vector3(X, 0, Y))
            && CheckDotInLine(c, d, new Vector3(X, 0, Y));
    }

    public static bool CrossCheck(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        float x1, x2, x3, x4, y1, y2, y3, y4, X, Y;

        x1 = a.x; y1 = a.z;
        x2 = b.x; y2 = b.z;
        x3 = c.x; y3 = c.z;
        x4 = d.x; y4 = d.z;

        float cross = ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));

        // 평행한 경우        
        if (cross == 0)
        {
            return false;
        }

        // 교점을 구한다
        X = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / cross;
        Y = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / cross;

        return
            CheckDotInLine(a, b, new Vector3(X, 0, Y))
            && CheckDotInLine(c, d, new Vector3(X, 0, Y));
    }
}
