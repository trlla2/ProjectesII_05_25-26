using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Tutorial/Tutorial Sequence")]
public class TutorialSequence : ScriptableObject
{
    public List<TutorialStep> steps;
}