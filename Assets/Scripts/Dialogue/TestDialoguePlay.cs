using UnityEngine;

public class TestDialoguePlay : MonoBehaviour
{
    public DialogueBlock blockToPlay;

    public void PlayBlock()
    {
        GameSingleton.instance.dialogueManager.PlayBlock(blockToPlay);
    }

    private void Start()
    {
        PlayBlock();
    }
}
