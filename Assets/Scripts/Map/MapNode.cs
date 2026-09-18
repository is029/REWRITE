using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MapNodeType
{
    Battle,
    Elite,
    Shop,
    Boss
}

public class MapNode : MonoBehaviour
{
    [Header("ノード種類")]
    public MapNodeType nodeType;

    [Header("次に進めるノード")]
    public List<MapNode> nextNodes =
        new List<MapNode>();

    [Header("状態")]
    public bool isCurrent;
    public bool isAvailable;

    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    [Header("ノード画像")]
    [SerializeField] private Sprite battleSprite;
    [SerializeField] private Sprite eliteSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private Sprite bossSprite;

    [Header("マップ位置")]
    public int floor;
    public int nodeIndex;


    // ========================================
    // ノード種類を設定
    // ========================================

    public void SetNodeType(MapNodeType type)
    {
        nodeType = type;

        UpdateUI();
    }


    // ========================================
    // Source Image変更
    // ========================================

    private void UpdateUI()
    {
        if (iconImage == null)
        {
            return;
        }

        switch (nodeType)
        {
            case MapNodeType.Battle:

                iconImage.sprite =
                    battleSprite;

                break;


            case MapNodeType.Elite:

                iconImage.sprite =
                    eliteSprite;

                break;


            case MapNodeType.Shop:

                iconImage.sprite =
                    shopSprite;

                break;


            case MapNodeType.Boss:

                iconImage.sprite =
                    bossSprite;

                break;
        }
    }


    // ========================================
    // 選択可能状態
    // ========================================

    public void SetAvailable(bool available)
    {
        isAvailable = available;

        if (button != null)
        {
            button.interactable =
                available;
        }
    }


    // ========================================
    // 現在地
    // ========================================
    public void SetCurrent(bool current)
    {
        isCurrent = current;
    }

    public void OnClick()
    {
        if (!isAvailable)
        {
            return;
        }

        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManagerがありません。");
            return;
        }

        MapManager.Instance.SelectNode(this);
    }
}