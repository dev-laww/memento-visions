using Godot;
using System.Collections.Generic;
using System.Linq;
using GodotUtilities;

namespace Game.World.Objects;

[Scene]
public partial class LeverManager : Node2D
{
    [Signal] public delegate void IsCompleteEventHandler();

    [Export] public NodePath TorchesPath;
    private Dictionary<Torch, int> toggleCounts = new();
    private List<Torch> allTorches = new();

    public override void _Ready()
    {
        var torchesNode = GetNode(TorchesPath);
        allTorches = torchesNode.GetChildren().OfType<Torch>().ToList();
        
        foreach (var torch in allTorches)
        {
            toggleCounts[torch] = 0;
            torch.LightUp(false);
        }
    }

    public void OnLeverToggled(Torch[] affectedTorches, bool isActivated)
    {
        foreach (var torch in affectedTorches)
        {
            toggleCounts[torch] += isActivated ? 1 : -1;
            if (toggleCounts[torch] < 0) toggleCounts[torch] = 0;
        }
        CheckPuzzleState();
    }
    

    private void CheckPuzzleState()
    {
        bool allLit = true;

        foreach (var torch in allTorches)
        {
            bool isLit = (toggleCounts[torch] % 2) != 0;
            torch.LightUp(isLit);

            if (!isLit) allLit = false;
        }

        if (allLit)
        {
            EmitSignalIsComplete();
            GD.Print("PUZZLE SOLVED! All torches lit.");
        }
    }
}