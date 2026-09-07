using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleItemUI : MonoBehaviour
{
    [Header("インベントリ")]
    [SerializeField] private GameObject itemPanel;

    [Header("アイテムボタン")]
    [SerializeField] private Button[] itemButtons;

    [Header("アイコン")]
    [SerializeField] private Image[] itemIcons;

    [Header("説明ポップアップ")]
    [SerializeField] private TMP_Text descriptionText;

    [Header("使用確認")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("クールダウン")]
    [SerializeField] private TMP_Text cooldownText;

    private int selectedItemIndex = -1;

    private void Start()
    {
        if (itemPanel != null)
        {
            itemPanel.SetActive(false);
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(ConfirmUseItem);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(CancelUseItem);
        }

        RefreshUI();
    }

    // インベントリを開く
    public void OpenItemPanel()
    {
        RefreshUI();

        if (itemPanel != null)
        {
            itemPanel.SetActive(true);
        }
    }

    // インベントリを閉じる
    public void CloseItemPanel()
    {
        if (itemPanel != null)
        {
            itemPanel.SetActive(false);
        }

        HideDescription();
        CancelUseItem();
    }

    // UI更新
    public void RefreshUI()
    {
        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (i < data.items.Count &&
                data.items[i] != null)
            {
                int index = i;
                ItemData item = data.items[i];

                itemButtons[i].gameObject.SetActive(true);

                // アイコン表示
                if (itemIcons[i] != null)
                {
                    itemIcons[i].sprite = item.icon;
                    itemIcons[i].gameObject.SetActive(
                        item.icon != null
                    );
                }

                // クリック
                itemButtons[i].onClick.RemoveAllListeners();

                itemButtons[i].onClick.AddListener(
                    () => SelectItem(index)
                );
            }
            else
            {
                itemButtons[i].gameObject.SetActive(false);
            }
        }

        UpdateCooldownUI();
    }

    // アイテム選択
    private void SelectItem(int index)
    {
        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        if (index < 0 ||
            index >= data.items.Count)
        {
            return;
        }

        ItemData item = data.items[index];

        if (item == null)
        {
            return;
        }

        selectedItemIndex = index;

        // クールダウン中
        if (!data.CanUseItem())
        {
            Debug.Log(
                "アイテムはあと " +
                data.itemCooldown +
                " ターン使用できません。"
            );

            return;
        }

        // 使用確認を表示
        ShowConfirm(item);
    }

    // 説明表示
    public void ShowDescription(int index)
    {
        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        if (index < 0 ||
            index >= data.items.Count)
        {
            return;
        }

        ItemData item = data.items[index];

        if (item == null)
        {
            return;
        }

        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(true);

            descriptionText.text =
                item.itemName +
                "\n\n" +
                item.description;
        }
    }

    // 説明を非表示
    public void HideDescription()
    {
        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(false);
        }
    }

    // 使用確認表示
    private void ShowConfirm(ItemData item)
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }

        if (confirmText != null)
        {
            confirmText.text =
                item.itemName +
                "を使用しますか？";
        }
    }

    // 使用する
    private void ConfirmUseItem()
    {
        if (selectedItemIndex < 0)
        {
            return;
        }

        bool success =
            BattleItemManager.Instance
                .UseItem(selectedItemIndex);

        if (!success)
        {
            return;
        }

        selectedItemIndex = -1;

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        RefreshUI();
    }

    // 使用しない
    private void CancelUseItem()
    {
        selectedItemIndex = -1;

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    // クールダウン表示
    private void UpdateCooldownUI()
    {
        if (cooldownText == null)
        {
            return;
        }

        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        if (data.CanUseItem())
        {
            cooldownText.text =
                "アイテム使用可能";
        }
        else
        {
            cooldownText.text =
                "あと " +
                data.itemCooldown +
                " ターン";
        }
    }
}