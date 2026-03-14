using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public void UnlockStartingPaths(List<GameObject> nodes)
    {
        foreach (GameObject node in nodes)
        {
            if (node != null)
            {
                Node mapNode = node.GetComponent<Node>();
                if ( mapNode.floorLevel == 0)
                {
                    node.layer = LayerMask.NameToLayer("Node");
                }
            }
        }
    }
}