using UnityEngine;

public class TitleManager : MonoBehaviour
{
   public void StartGame()
    {
        RoguelikeManager.Instance.StartNewGame();
    }
}
