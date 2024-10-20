using Godot;

namespace Game.Components.Area;

[GlobalClass]
public partial class HurtBox : Area2D
{
	[Signal]
	public delegate void DamageReceivedEventHandler(float damage);

	public override void _Ready()
	{
		AddToGroup("HurtBox");
		AreaEntered += OnHurtBoxAreaEntered;
	}

	public void ReceiveDamage(float damage) => EmitSignal(SignalName.DamageReceived, damage);

	private void OnHurtBoxAreaEntered(Area2D area)
	{
		if (area is not HitBox hitBox) return;
		
		if (hitBox.Owner == Owner) return;

		ReceiveDamage(hitBox.Damage);
	}
}
