using Godot;

namespace Game.Inventory;


public partial class CollectionArea : Area2D
{
	[Signal]
	public delegate void CollectedEventHandler();
	private Inventory inventory = new Inventory();
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
			GD.Print("CollectionArea collect method called");
			area.Call("collect",inventory);
		}
	}


}
