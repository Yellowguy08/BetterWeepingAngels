using Godot;
using System;

public partial class main : Node
{
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Escape)) { GetTree().Quit(); }
	}
}
