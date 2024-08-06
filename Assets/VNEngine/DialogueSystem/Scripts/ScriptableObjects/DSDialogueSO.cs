using System.Collections.Generic;
using UnityEngine;

namespace VNEngine.DS.ScriptableObjects
{
    using VNEngine.DS.Data;
    using VNEngine.Enumerations;

    public class DSDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName {  get; set; }
        [field: SerializeField] public string DialogueTitle { get; set; }
        [field: SerializeField] [field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<DSDialogueChioceData> Choices { get; set; }
        [field: SerializeField] public DSDialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }

        public void Initialize(string dialogueName, string dialogueTitle, string text, List<DSDialogueChioceData> choices, DSDialogueType dialogueType, bool isStartingDialogue)
        {
            this.DialogueName = dialogueName;
            this.DialogueTitle = dialogueTitle;
            this.Text = text;
            this.Choices = choices;
            this.DialogueType = dialogueType;
            this.IsStartingDialogue = isStartingDialogue;
        }
    }
}
