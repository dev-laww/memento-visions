using Godot;
using System;

public partial class Collectables : Area2D
{
	public void collect ()
	{
		this.QueueFree();
		GD.Print($"Collect: {this.Name}");
	}
}
