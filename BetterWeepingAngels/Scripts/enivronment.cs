using Godot;
using System;
using System.Diagnostics;

public partial class enivronment : WorldEnvironment
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		if (!(bool)GetNode<Node>("/root/MAIN").GetMeta("atmosphere"))
		{

			QueueFree();

		}

	}
}
