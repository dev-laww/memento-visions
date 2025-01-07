using Godot;
using System;
using Game.Components.Managers;
using Game.Entities.Player;
using Game.Globals;
using Game.Utils.Extensions;

[Tool]
[GlobalClass]
public partial class TransitionArea : Area2D
{
	
	[Export] public string TargetScenePath;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var Player = this.GetPlayer();
		BodyEntered += OnBodyEntered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnBodyEntered(Node body)
	{
		if (body is Player player)
		{
			GameManager.ChangeScene(TargetScenePath, Vector2.Zero, Transition.Fade);
		}
	}
}
