using System;
using Game.Components;
using Game.Inventory;
using Game.Logic.States;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;


namespace Game.Entities.Player;

[Scene]
[GlobalClass]
public partial class Player : CharacterBody2D
{
	[Export]
	public float DashStaminaCost { get; set; } = 10f;
	[Export]
	public Inventory.Inventory Inventory { get; private set; }

	[Node]
	public AnimatedSprite2D sprites;

	[Node]
	private StateMachine stateMachine;

	[Node]
	private StatsManager statsManager;

	[Node]
	private HurtBox hurtBox;

	[Node]
	private CollectionArea collectionArea;

	public string MoveDirection => GetMoveDirection();

	public Vector2 lastMoveDirection = Vector2.Down;
	public Vector2 DashVelocity { get; set; }
	public bool CanDash { get; set; } = true;
	public bool Dashing { get; set; }
	public bool CanMove { get; set; } = true;

	public override void _Notification(int what)
	{
		if (what != NotificationSceneInstantiated) return;

		WireNodes();
	}

	public override void _Ready()
	{
		Inventory = new Inventory.Inventory();
		statsManager.StaminaChanged += (stamina) => CanDash = stamina > DashStaminaCost;
		hurtBox.DamageReceived += (damage) => GD.Print($"Player received {damage} damage.");
		collectionArea.AreaEntered += (itemname) => GD.Print("godot collection area entered");
	
		

	}



	public override void _PhysicsProcess(double delta)
	{
		Velocity = Input.GetVector("move_left", "move_right", "move_up", "move_down") * statsManager.Speed;

		if (Dashing)
			Velocity = DashVelocity;

		Velocity = Velocity.SnapToGrid();
		Velocity = CanMove ? Velocity : Vector2.Zero;

		if (Velocity.Length() > 0 && CanMove)
			lastMoveDirection = Velocity.Normalized();

		MoveAndSlide();

		if (Input.IsActionJustPressed("attack")) stateMachine.ChangeState("attack");

		if (!Input.IsActionJustPressed("dash") || !CanDash || !CanMove) return;

		statsManager.ConsumeStamina(DashStaminaCost);
		stateMachine.ChangeState("dash");
	}

	private string GetMoveDirection()
	{
		if (lastMoveDirection == Vector2.Zero) return "front";

		if (Math.Abs(lastMoveDirection.X) > Math.Abs(lastMoveDirection.Y))
			return lastMoveDirection.X > 0 ? "right" : "left";

		return lastMoveDirection.Y < 0 ? "back" : "front";
	}
}
