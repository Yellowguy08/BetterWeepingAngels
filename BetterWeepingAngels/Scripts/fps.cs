using Godot;
using System;

public partial class fps : Label
{
	public override void _Process(double delta) { Text = Engine.GetFramesPerSecond().ToString(); }
}
