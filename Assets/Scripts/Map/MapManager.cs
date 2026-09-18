using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    private MapData mapData;

    private MapNodeType currentBattleType;

    [Header("現在地")]
    private int currentFloor = -1;
    private int currentNodeIndex = -1;

    [Header("マップ状態")]
    private bool mapGenerated = false;

    public bool IsMapGenerated
    {
        get { return mapGenerated; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void SetMapData(MapData data)
    {
        mapData = data;

        Debug.Log(
            "MapDataを保存しました。"
        );
    }

    public MapData GetMapData()
    {
        return mapData;
    }

    public MapNodeType GetCurrentBattleType()
    {
        return currentBattleType;
    }

    // ========================================
    // ノード選択
    // ========================================

    public void SelectNode(MapNode node)
    {
        if (node == null)
        {
            return;
        }

        if (!node.isAvailable)
        {
            return;
        }

        // 現在地を保存
        currentFloor = node.floor;
        currentNodeIndex = node.nodeIndex;

        currentBattleType = node.nodeType;

        Debug.Log(
            "===== NODE SELECT =====\n" +
            "Floor : " + currentFloor + "\n" +
            "Node  : " + currentNodeIndex + "\n" +
            "Type  : " + node.nodeType
        );

        switch (node.nodeType)
        {
            case MapNodeType.Battle:

                RoguelikeManager.Instance.StartNormalBattle();

                break;

            case MapNodeType.Elite:

                RoguelikeManager.Instance.StartNormalBattle();

                break;

            case MapNodeType.Shop:

                RoguelikeManager.Instance.StartUpgrade();

                break;

            case MapNodeType.Boss:

                RoguelikeManager.Instance.StartBoss();

                break;
        }
    }

    // ========================================
    // 現在地取得
    // ========================================

    public int GetCurrentFloor()
    {
        return currentFloor;
    }

    public int GetCurrentNodeIndex()
    {
        return currentNodeIndex;
    }

    // ========================================
    // マップ生成済み設定
    // ========================================

    public void SetMapGenerated(bool value)
    {
        mapGenerated = value;

        Debug.Log(
            "MapGenerated : " +
            mapGenerated
        );
    }

    // ========================================
    // マップ進行リセット
    // ========================================

    public void ResetMapProgress()
    {
        currentFloor = -1;
        currentNodeIndex = -1;
        mapGenerated = false;

        Debug.Log(
            "===== MAP PROGRESS RESET ====="
        );
    }

    // ========================================
    // MapSceneへ戻る
    // ========================================

    public void ReturnToMap()
    {
        RoguelikeManager.Instance.CurrentStage = RoguelikeManager.StageType.MapSelect;
        SceneManager.LoadScene("MapScene");
    }
}