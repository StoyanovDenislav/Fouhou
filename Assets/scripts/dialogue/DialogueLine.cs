using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)] public string text;  // ✅ Unity editor-friendly multiline text
    public Sprite portrait;               // Optional speaker portrait
}

