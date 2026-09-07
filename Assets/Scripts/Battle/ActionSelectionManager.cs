using UnityEngine;

public class ActionSelectionManager : MonoBehaviour
{
    public static ActionSelectionManager Instance { get; private set; }

    private int selectedActionIndex = -1;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public void SelectActionSlot(int index)
    {
        if (index < 0 || index >= 3)
        {
            return;
        }

        selectedActionIndex = index;

        Debug.Log(
            "s“®" +
            (index + 1) +
            "‚ð‘I‘ð’†"
        );
    }


    public bool HasSelectedSlot()
    {
        return selectedActionIndex >= 0;
    }


    public int GetSelectedActionIndex()
    {
        return selectedActionIndex;
    }


    public void ClearSelection()
    {
        selectedActionIndex = -1;
    }
}