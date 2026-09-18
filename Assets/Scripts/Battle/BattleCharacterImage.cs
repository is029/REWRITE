using UnityEngine;
using UnityEngine.UI;

public class BattleCharacterImage : MonoBehaviour
{
    [Header("表示するImage")]
    [SerializeField] private Image targetImage;

    [Header("BattleUnit")]
    [SerializeField] private BattleUnit battleUnit;

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = targetImage.GetComponent<RectTransform>();

        UpdateImage();
    }

    public void UpdateImage()
    {
        if (targetImage == null)
        {
            Debug.LogError(
                gameObject.name +
                "：Target Imageが設定されていません。"
            );
            return;
        }

        if (battleUnit == null)
        {
            Debug.LogError(
                gameObject.name +
                "：BattleUnitが設定されていません。"
            );
            return;
        }

        CharacterData data =
            battleUnit.CharacterData;

        if (data == null)
        {
            Debug.LogError(
                gameObject.name +
                "：CharacterDataがありません。"
            );
            return;
        }

        if (data.battleSprite == null)
        {
            Debug.LogWarning(
                data.characterName +
                " にBattle Spriteが設定されていません。"
            );
            return;
        }

        // 画像を変更
        targetImage.sprite = data.battleSprite;

        // キャラクターごとにサイズ変更
        rectTransform.sizeDelta =
            data.battleImageSize;
    }
}