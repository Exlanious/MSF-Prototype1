using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "DialogueOptions", menuName = "SO/DialogueOptions")]
public class DialogueOptions : ScriptableObject
{
    public List<string> options;

    //default off. Used for prompting LLM to generate more options. 
    public string LLMPrompt = null;

}