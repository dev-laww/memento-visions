using Godot;
using System;
using System.Collections.Generic;
using Game.Components;
using GodotUtilities;

namespace Game.World.Objects;

[Scene]
public partial class TorchPuzzleManager : Node
 {
  [Node] private Interaction interaction;
  
     [Export] private float displaySequenceDelay = 1.0f; 
     [Export] private float resetDelay = 20.0f;
     
     private List<StreetLight> torches = new List<StreetLight>();
     private List<int> correctSequence = new List<int>();
     private List<int> playerSequence = new List<int>();
     
     private bool isShowingSequence = false;
     private bool isPuzzleActive = false;
     private Timer sequenceTimer;
     private Timer resetTimer;
     
        public override void _Notification(int what)
        {
            if (what != NotificationSceneInstantiated || Engine.IsEditorHint()) return;
    
            WireNodes();
        }
     public override void _Ready()
     {
         
         interaction.Interacted += StartPuzzle;
         foreach (var child in GetChildren())
         {
             if (child is StreetLight torch)
             {
                 torches.Add(torch);
             }
         }
         sequenceTimer = new Timer();
         sequenceTimer.OneShot = true;
         AddChild(sequenceTimer);
         sequenceTimer.Timeout += ShowNextInSequence;
         resetTimer = new Timer();
         resetTimer.OneShot = true;
         resetTimer.WaitTime = resetDelay;
         AddChild(resetTimer);
         resetTimer.Timeout += ResetPuzzle;
         GenerateSequence(4);
         for (int i = 0; i < torches.Count; i++)
         {
             int torchIndex = i;
             torches[i].TorchLit += () => OnTorchLit(torchIndex);
         }
     }
     
     public void StartPuzzle()
     {
         ResetAllTorches();
         playerSequence.Clear();
         isShowingSequence = true;
         isPuzzleActive = false;
         ShowSequence();
     }
     
     private void GenerateSequence(int length)
     {
         correctSequence.Clear();
         Random rnd = new Random();
         HashSet<int> usedIndices = new HashSet<int>();
         
         while (correctSequence.Count < length)
         {
             int nextIndex = rnd.Next(0, torches.Count);
             if (usedIndices.Add(nextIndex))
             {
                 correctSequence.Add(nextIndex);
             }
         }
     }
     
     private void ShowSequence()
     {
         if (correctSequence.Count > 0)
         {
             currentSequenceIndex = 0;
             ShowNextInSequence();
         }
     }
     
     private int currentSequenceIndex = 0;
     
     private void ShowNextInSequence()
     {
         if (currentSequenceIndex < correctSequence.Count)
         {
             ResetAllTorches();

             int torchIndex = correctSequence[currentSequenceIndex];
             torches[torchIndex].LightUp(true);
             
             currentSequenceIndex++;

             sequenceTimer.WaitTime = displaySequenceDelay;
             sequenceTimer.Start();
         }
         else
         {
             ResetAllTorches();
             isShowingSequence = false;
             isPuzzleActive = true;
             resetTimer.Start();
         }
     }
     
     private void OnTorchLit(int torchIndex)
     {
         if (!isPuzzleActive || isShowingSequence) return;
         
         playerSequence.Add(torchIndex);
         resetTimer.Stop();
         resetTimer.Start();
         bool isCorrect = true;
         for (int i = 0; i < playerSequence.Count; i++)
         {
             if (playerSequence[i] != correctSequence[i])
             {
                 isCorrect = false;
                 break;
             }
         }
         
         if (!isCorrect)
         {
             OnPuzzleFailed();
             return;
         }
         
         if (playerSequence.Count == correctSequence.Count)
         {
             OnPuzzleSolved();
         }
     }
     
     private void OnPuzzleSolved()
     {
         isPuzzleActive = false;
         resetTimer.Stop();
         GD.Print("Puzzle solved!");
         EmitSignalPuzzleSolved();
     }
     
     private void OnPuzzleFailed()
     {
         isPuzzleActive = false;
         resetTimer.Stop();
         GD.Print("Puzzle failed!");
         ResetAllTorches();
     }
     private void ResetPuzzle()
     {
         // Time ran out
         OnPuzzleFailed();
     }
     
     private void ResetAllTorches()
     {
         foreach (var torch in torches)
         {
             torch.ResetLight();
         }
     }
     
     [Signal]
     public delegate void PuzzleSolvedEventHandler();
     
}