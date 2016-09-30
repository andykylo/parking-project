using UnityEngine;
using System.Collections;

public class DirectionPenalty {

    public string direction;
    public int penalty;

    public DirectionPenalty(string _dir, int _pen)
    {
        direction = _dir;
        penalty = _pen;
    }

}
