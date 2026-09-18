using System.Collections.Generic;

[System.Serializable]
public class MapData
{
    [System.Serializable]
    public class FloorData
    {
        public List<NodeData> nodes =
            new List<NodeData>();
    }

    [System.Serializable]
    public class NodeData
    {
        public int floor;
        public int nodeIndex;

        public MapNodeType nodeType;

        // Ú‘±æ‚ÌFloor
        public List<int> nextFloors =
            new List<int>();

        // Ú‘±æ‚ÌNode”Ô†
        public List<int> nextNodeIndexes =
            new List<int>();
    }

    public List<FloorData> floors =
        new List<FloorData>();
}