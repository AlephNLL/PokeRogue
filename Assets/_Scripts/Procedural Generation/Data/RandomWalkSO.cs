using UnityEngine;

[CreateAssetMenu(fileName = "RandomWalkData_", menuName = "ProcGenData/RandomWalkData")]
public class RandomWalkSO : ScriptableObject
{
    public int iterations = 10;
    public int walkLength = 10;
    public bool startRandomly = true;
}
