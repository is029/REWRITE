using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [Header("マップ設定")]
    [SerializeField] private int battleFloors = 5;
    [SerializeField] private int nodesPerFloor = 3;

    [Header("マップUI")]
    [SerializeField] private RectTransform mapPanel;

    [Header("ノード")]
    [SerializeField] private MapNode nodePrefab;

    [Header("配置")]
    [SerializeField] private float horizontalPadding = 80f;
    [SerializeField] private float verticalPadding = 50f;

    [Header("接続線")]
    [SerializeField] private float lineWidth = 8f;

    [SerializeField]
    private Color lineColor =
        new Color(0.8f, 0.15f, 0.1f, 1f);

    private List<List<MapNode>> floors =
        new List<List<MapNode>>();

    // 現在のマップが何階層あるか
    private int totalFloorsForLayout = 0;


    // ========================================
    // Start
    // ========================================

    private void Start()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError(
                "MapManagerがありません。"
            );

            return;
        }

        // 初回だけランダム生成
        if (!MapManager.Instance.IsMapGenerated)
        {
            GenerateMap();

            MapManager.Instance.SetMapGenerated(true);
        }
        else
        {
            // 保存したマップを再構築
            LoadMapData();
        }
    }


    // ========================================
    // マップ生成
    // ========================================

    public void GenerateMap()
    {
        ClearMap();

        floors.Clear();

        // ========================================
        // 今回の横方向の階層数
        //
        // Battle 5階
        // Shop 1階
        // Boss 1階
        //
        // 合計7階層
        // ========================================

        totalFloorsForLayout =
            battleFloors + 2;


        // ========================================
        // 1. 通常戦闘・エリート階層
        // ========================================

        for (int floor = 0;
             floor < battleFloors;
             floor++)
        {
            List<MapNode> currentFloor =
                new List<MapNode>();

            for (int i = 0;
                 i < nodesPerFloor;
                 i++)
            {
                MapNode node =
                    CreateNode(floor, i);

                // 通常戦闘 or エリート
                if (Random.Range(0, 100) < 20)
                {
                    node.SetNodeType(
                        MapNodeType.Elite
                    );
                }
                else
                {
                    node.SetNodeType(
                        MapNodeType.Battle
                    );
                }

                currentFloor.Add(node);
            }

            floors.Add(currentFloor);
        }


        // ========================================
        // 2. ショップ
        // ========================================

        List<MapNode> shopFloor =
            new List<MapNode>();

        MapNode shop =
            CreateNode(
                battleFloors,
                0
            );

        shop.SetNodeType(
            MapNodeType.Shop
        );

        shopFloor.Add(shop);

        floors.Add(shopFloor);


        // ========================================
        // 3. Boss
        // ========================================

        List<MapNode> bossFloor =
            new List<MapNode>();

        MapNode boss =
            CreateNode(
                battleFloors + 1,
                0
            );

        boss.SetNodeType(
            MapNodeType.Boss
        );

        bossFloor.Add(boss);

        floors.Add(bossFloor);


        // ========================================
        // 4. ルートを接続
        // ========================================

        ConnectFloors();

        // ========================================
        // 5. 接続線を描画
        // ========================================

        DrawConnections();

        // ========================================
        // 6. マップ保存
        // ========================================

        SaveMapData();


        // ========================================
        // 7. 現在地復元
        // ========================================

        RestoreMapProgress();


        Debug.Log(
            "===== MAP GENERATE COMPLETE ====="
        );
    }


    // ========================================
    // ノード生成
    // ========================================

    private MapNode CreateNode(
        int floor,
        int index)
    {
        if (mapPanel == null)
        {
            Debug.LogError(
                "MapGenerator：MapPanelが設定されていません。"
            );

            return null;
        }

        if (nodePrefab == null)
        {
            Debug.LogError(
                "MapGenerator：NodePrefabが設定されていません。"
            );

            return null;
        }


        // ========================================
        // ノード生成
        // ========================================

        MapNode node =
            Instantiate(
                nodePrefab,
                mapPanel
            );


        RectTransform rect =
            node.GetComponent<RectTransform>();


        // ========================================
        // Panelのサイズ
        // ========================================

        float panelWidth =
            mapPanel.rect.width;

        float panelHeight =
            mapPanel.rect.height;


        // ========================================
        // ノードサイズ
        // ========================================

        float nodeWidth = 0f;
        float nodeHeight = 0f;

        if (rect != null)
        {
            nodeWidth =
                rect.rect.width;

            nodeHeight =
                rect.rect.height;
        }


        // ========================================
        // 横方向の配置
        //
        // Floor 0 → 左
        // Floor 1
        // Floor 2
        // ...
        // Shop
        // Boss → 右
        // ========================================

        float usableWidth =
            panelWidth
            - nodeWidth
            - horizontalPadding * 2f;

        usableWidth =
            Mathf.Max(
                0f,
                usableWidth
            );


        float x = 0f;

        if (totalFloorsForLayout > 1)
        {
            float startX =
                -usableWidth * 0.5f;

            float step =
                usableWidth /
                (totalFloorsForLayout - 1);

            x =
                startX +
                step * floor;
        }


        // ========================================
        // 縦方向の配置
        //
        // 3ノードなら
        //
        //    ⚔
        //
        //    ⚔
        //
        //    ⚔
        //
        // となる
        // ========================================

        float usableHeight =
            panelHeight
            - nodeHeight
            - verticalPadding * 2f;

        usableHeight =
            Mathf.Max(
                0f,
                usableHeight
            );


        float y = 0f;


        // ========================================
        // ノードが1個なら必ず中央
        //
        // Shop / Boss はここ
        // ========================================

        if (nodesPerFloor <= 1)
        {
            y = 0f;
        }
        else
        {
            float startY =
                usableHeight * 0.5f;

            float step =
                usableHeight /
                (nodesPerFloor - 1);

            y =
                startY -
                step * index;
        }


        // ========================================
        // 位置設定
        // ========================================

        if (rect != null)
        {
            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                new Vector2(
                    x,
                    y
                );
        }


        // ========================================
        // ノード情報
        // ========================================

        node.floor = floor;
        node.nodeIndex = index;


        // ========================================
        // 初期状態
        // ========================================

        node.SetCurrent(false);
        node.SetAvailable(false);


        node.name =
            "MapNode_Floor" +
            floor +
            "_" +
            index;


        return node;
    }


    // ========================================
    // 階層同士を接続
    // 現在地から上下1以内だけ接続
    // ========================================

    private void ConnectFloors()
    {
        for (int floor = 0;
             floor < floors.Count - 1;
             floor++)
        {
            List<MapNode> currentFloor =
                floors[floor];

            List<MapNode> nextFloor =
                floors[floor + 1];


            // ========================================
            // Shop・Bossへの接続
            // ========================================

            if (nextFloor.Count == 1)
            {
                foreach (MapNode currentNode
                    in currentFloor)
                {
                    if (!currentNode.nextNodes.Contains(
                        nextFloor[0]))
                    {
                        currentNode.nextNodes.Add(
                            nextFloor[0]
                        );
                    }
                }

                continue;
            }


            // ========================================
            // 通常階層
            // ========================================

            for (int i = 0;
                 i < currentFloor.Count;
                 i++)
            {
                MapNode currentNode =
                    currentFloor[i];


                // ------------------------------------
                // 自分と同じ位置
                // ------------------------------------

                if (i < nextFloor.Count)
                {
                    AddConnection(
                        currentNode,
                        nextFloor[i]
                    );
                }


                // ------------------------------------
                // 上の位置
                // ------------------------------------

                int upperIndex =
                    i - 1;

                if (upperIndex >= 0 &&
                    upperIndex < nextFloor.Count)
                {
                    AddConnection(
                        currentNode,
                        nextFloor[upperIndex]
                    );
                }


                // ------------------------------------
                // 下の位置
                // ------------------------------------

                int lowerIndex =
                    i + 1;

                if (lowerIndex >= 0 &&
                    lowerIndex < nextFloor.Count)
                {
                    AddConnection(
                        currentNode,
                        nextFloor[lowerIndex]
                    );
                }
            }


            // ========================================
            // 全ノードが最低1本は接続されるようにする
            // ========================================

            for (int i = 0;
                 i < nextFloor.Count;
                 i++)
            {
                MapNode nextNode =
                    nextFloor[i];

                bool connected = false;


                foreach (MapNode currentNode
                    in currentFloor)
                {
                    if (currentNode.nextNodes.Contains(
                        nextNode))
                    {
                        connected = true;
                        break;
                    }
                }


                // 念のため接続
                if (!connected)
                {
                    int safeIndex =
                        Mathf.Clamp(
                            i,
                            0,
                            currentFloor.Count - 1
                        );

                    AddConnection(
                        currentFloor[safeIndex],
                        nextNode
                    );
                }
            }
        }
    }


    // ========================================
    // 接続を追加
    // ========================================

    private void AddConnection(
        MapNode from,
        MapNode to)
    {
        if (from == null || to == null)
        {
            return;
        }


        // ========================================
        // 上下1以内かチェック
        // ========================================

        int difference =
            Mathf.Abs(
                from.nodeIndex -
                to.nodeIndex
            );


        if (difference > 1)
        {
            return;
        }


        // ========================================
        // 重複防止
        // ========================================

        if (!from.nextNodes.Contains(to))
        {
            from.nextNodes.Add(to);
        }
    }


    // ========================================
    // 現在地を復元
    // ========================================

    private void RestoreMapProgress()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogWarning(
                "MapManagerがありません。"
            );

            return;
        }

        int currentFloor =
            MapManager.Instance.GetCurrentFloor();

        int currentNodeIndex =
            MapManager.Instance.GetCurrentNodeIndex();


        // ========================================
        // 初回
        // ========================================

        if (currentFloor < 0 ||
            currentNodeIndex < 0)
        {
            SetFirstNodesAvailable();

            return;
        }


        // ========================================
        // 保存された現在地を探す
        // ========================================

        MapNode currentNode = null;

        if (currentFloor < floors.Count)
        {
            List<MapNode> floor =
                floors[currentFloor];

            if (currentNodeIndex < floor.Count)
            {
                currentNode =
                    floor[currentNodeIndex];
            }
        }


        if (currentNode == null)
        {
            Debug.LogWarning(
                "保存されていた現在地を見つけられませんでした。"
            );

            SetFirstNodesAvailable();

            return;
        }


        // ========================================
        // 現在地
        // ========================================

        currentNode.SetCurrent(true);


        // ========================================
        // 次に進めるノードだけ開く
        // 現在地の上下1以内のみ
        // ========================================

        foreach (MapNode nextNode
            in currentNode.nextNodes)
        {
            if (nextNode == null)
            {
                continue;
            }

            int difference =
                Mathf.Abs(
                    nextNode.nodeIndex -
                    currentNode.nodeIndex
                );

            // 現在地の上下1以内
            if (difference <= 1)
            {
                nextNode.SetAvailable(true);
            }
        }


        Debug.Log(
            "===== MAP PROGRESS RESTORED ====="
        );

        Debug.Log(
            "現在地：" +
            currentFloor +
            "階 / " +
            currentNodeIndex +
            "番"
        );
    }


    // ========================================
    // 初回の選択可能ノード
    // ========================================

    private void SetFirstNodesAvailable()
    {
        if (floors.Count == 0)
        {
            return;
        }

        List<MapNode> firstFloor =
            floors[0];

        foreach (MapNode node
            in firstFloor)
        {
            if (node != null)
            {
                node.SetAvailable(true);
            }
        }

        Debug.Log(
            "===== MAP START ====="
        );
    }


    // ========================================
    // マップ削除
    // ========================================

    private void ClearMap()
    {
        if (mapPanel == null)
        {
            return;
        }

        List<Transform> children =
            new List<Transform>();

        foreach (Transform child
            in mapPanel)
        {
            children.Add(child);
        }

        foreach (Transform child
            in children)
        {
            Destroy(child.gameObject);
        }
    }


    // ========================================
    // マップデータの保存
    // ========================================

    private void SaveMapData()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError(
                "MapManagerがありません。"
            );

            return;
        }

        MapData mapData =
            new MapData();


        // ========================================
        // 全階層
        // ========================================

        for (int floor = 0;
             floor < floors.Count;
             floor++)
        {
            MapData.FloorData floorData =
                new MapData.FloorData();

            List<MapNode> currentFloor =
                floors[floor];


            // ========================================
            // 階層内の全ノード
            // ========================================

            for (int i = 0;
                 i < currentFloor.Count;
                 i++)
            {
                MapNode node =
                    currentFloor[i];

                MapData.NodeData nodeData =
                    new MapData.NodeData();

                nodeData.floor =
                    node.floor;

                nodeData.nodeIndex =
                    node.nodeIndex;

                nodeData.nodeType =
                    node.nodeType;


                // ========================================
                // 接続先を保存
                // ========================================

                foreach (MapNode nextNode
                    in node.nextNodes)
                {
                    if (nextNode == null)
                    {
                        continue;
                    }

                    nodeData.nextFloors.Add(
                        nextNode.floor
                    );

                    nodeData.nextNodeIndexes.Add(
                        nextNode.nodeIndex
                    );
                }

                floorData.nodes.Add(
                    nodeData
                );
            }

            mapData.floors.Add(
                floorData
            );
        }


        // ========================================
        // MapManagerへ保存
        // ========================================

        MapManager.Instance.SetMapData(
            mapData
        );

        Debug.Log(
            "===== MAP DATA SAVED ====="
        );
    }


    // ========================================
    // 保存したマップを読み込む
    // ========================================

    private void LoadMapData()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError(
                "MapManagerがありません。"
            );

            return;
        }

        MapData mapData =
            MapManager.Instance.GetMapData();


        if (mapData == null ||
            mapData.floors == null ||
            mapData.floors.Count == 0)
        {
            Debug.LogWarning(
                "保存されたMapDataがありません。"
            );

            GenerateMap();

            return;
        }


        ClearMap();

        floors.Clear();


        // ========================================
        // 保存データの階層数をレイアウトに使用
        // ========================================

        totalFloorsForLayout =
            mapData.floors.Count;


        // ========================================
        // ノードを再生成
        // ========================================

        for (int floor = 0;
             floor < mapData.floors.Count;
             floor++)
        {
            List<MapNode> currentFloor =
                new List<MapNode>();

            MapData.FloorData floorData =
                mapData.floors[floor];


            for (int i = 0;
                 i < floorData.nodes.Count;
                 i++)
            {
                MapData.NodeData nodeData =
                    floorData.nodes[i];

                MapNode node =
                    CreateNode(
                        nodeData.floor,
                        nodeData.nodeIndex
                    );


                // ========================================
                // 保存されていた種類を復元
                // ========================================

                node.SetNodeType(
                    nodeData.nodeType
                );

                currentFloor.Add(node);
            }

            floors.Add(currentFloor);
        }


        // ========================================
        // 接続を復元
        // ========================================

        for (int floor = 0;
             floor < mapData.floors.Count;
             floor++)
        {
            MapData.FloorData floorData =
                mapData.floors[floor];

            for (int i = 0;
                 i < floorData.nodes.Count;
                 i++)
            {
                MapData.NodeData nodeData =
                    floorData.nodes[i];

                MapNode node =
                    floors[floor][i];


                for (int j = 0;
                     j < nodeData.nextFloors.Count;
                     j++)
                {
                    int nextFloor =
                        nodeData.nextFloors[j];

                    int nextIndex =
                        nodeData.nextNodeIndexes[j];


                    if (nextFloor < 0 ||
                        nextFloor >= floors.Count)
                    {
                        continue;
                    }

                    if (nextIndex < 0 ||
                        nextIndex >=
                        floors[nextFloor].Count)
                    {
                        continue;
                    }


                    MapNode nextNode =
                        floors[nextFloor][nextIndex];


                    if (!node.nextNodes.Contains(
                        nextNode))
                    {
                        node.nextNodes.Add(
                            nextNode
                        );
                    }
                }
            }
        }


        // ========================================
        // 接続線を描画
        // ========================================

        DrawConnections();

        // ========================================
        // 現在地を復元
        // ========================================

        RestoreMapProgress();


        Debug.Log(
            "===== MAP DATA LOAD COMPLETE ====="
        );
    }


    // ========================================
    // 接続線を描画
    // ========================================

    private void DrawConnections()
    {
        if (mapPanel == null)
        {
            Debug.LogError(
                "MapGenerator：MapPanelがありません。"
            );

            return;
        }

        // すでにある線を削除
        for (int i = mapPanel.childCount - 1; i >= 0; i--)
        {
            Transform child =
                mapPanel.GetChild(i);

            if (child.name.StartsWith("MapLine_"))
            {
                Destroy(child.gameObject);
            }
        }


        int lineIndex = 0;


        // 全ノードを調べる
        foreach (List<MapNode> floor in floors)
        {
            foreach (MapNode node in floor)
            {
                if (node == null)
                {
                    continue;
                }


                // 接続先を調べる
                foreach (MapNode nextNode in node.nextNodes)
                {
                    if (nextNode == null)
                    {
                        continue;
                    }


                    RectTransform startRect =
                        node.GetComponent<RectTransform>();

                    RectTransform endRect =
                        nextNode.GetComponent<RectTransform>();


                    if (startRect == null ||
                        endRect == null)
                    {
                        continue;
                    }


                    CreateLine(
                        startRect.anchoredPosition,
                        endRect.anchoredPosition,
                        lineIndex
                    );

                    lineIndex++;
                }
            }
        }


        Debug.Log(
            "===== MAP CONNECTION LINES : " +
            lineIndex +
            " ====="
        );
    }

    // ========================================
    // 1本の線を作成
    // ========================================

    private void CreateLine(
        Vector2 start,
        Vector2 end,
        int index)
    {
        GameObject lineObject =
            new GameObject(
                "MapLine_" + index
            );


        lineObject.transform.SetParent(
            mapPanel,
            false
        );


        RectTransform rect =
            lineObject.AddComponent<RectTransform>();


        Image image =
            lineObject.AddComponent<Image>();


        // ========================================
        // 線の位置
        // ========================================

        Vector2 direction =
            end - start;

        float distance =
            direction.magnitude;


        Vector2 middle =
            (start + end) * 0.5f;


        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);


        rect.anchoredPosition =
            middle;


        // ========================================
        // 線のサイズ
        // ========================================

        rect.sizeDelta =
            new Vector2(
                distance,
                lineWidth
            );


        // ========================================
        // 線の角度
        // ========================================

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;


        rect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );


        // ========================================
        // 色
        // ========================================

        image.color =
            lineColor;


        // ========================================
        // ノードより後ろにする
        // ========================================

        rect.SetAsFirstSibling();
    }
}