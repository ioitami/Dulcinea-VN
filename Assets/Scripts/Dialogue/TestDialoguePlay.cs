using UnityEngine;

public class TestDialoguePlay : MonoBehaviour
{
    public DialogueGroup blockToPlay;

    public void PlayBlock()
    {
        GameSingleton.instance.dialogueManager.PlayGroup(blockToPlay);
    }

    private void Start()
    {
        PlayBlock();
    }
}
