using Godot;

namespace Game.Inventory;


public partial class CollectionArea : Area2D
{
	[Signal]
	public delegate void CollectedEventHandler();
	public override void _Ready()
	{
		AddToGroup("CollectionArea");
		AreaEntered += OnCollectionAreaAreaEntered;
	}
	public void collectItem(int itemcount) => EmitSignal(nameof(CollectedEventHandler));

	private void OnCollectionAreaAreaEntered(Area2D area)
	{
		if (area.HasMethod("collect"))
		{
			area.Call("collect");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
