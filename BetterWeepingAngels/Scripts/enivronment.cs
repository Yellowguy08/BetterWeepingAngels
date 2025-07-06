using Godot;
using System;
using System.Diagnostics;

public partial class enivronment : WorldEnvironment
{
	public override void _Ready()
	{

		if (!(bool)GetNode<Node>("/root/MAIN").GetMeta("atmosphere")) { QueueFree(); }

	}
}
