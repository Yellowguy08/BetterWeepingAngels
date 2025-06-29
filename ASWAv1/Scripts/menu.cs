using Godot;
using System;

public partial class menu : Control
{

	Node mother;

	CheckBox atmosphere, monster, fps;
	Label fps_label;
	public override void _Ready()
	{
		mother = GetParent();
		atmosphere = GetNode<CheckBox>("l_atmosphere/CheckBox");
		monster = GetNode<CheckBox>("l_monster/CheckBox");
		fps = GetNode<CheckBox>("l_FPS/CheckBox");
		fps_label = mother.GetNode<Label>("fps_label");

		atmosphere.ButtonPressed = (bool)mother.GetMeta("atmosphere");
		monster.ButtonPressed = (bool)mother.GetMeta("monster");
		fps.ButtonPressed = (bool)mother.GetMeta("fps");

	}


	public void atmosphereToggled(bool button_pressed)
	{
		mother.SetMeta("atmosphere", button_pressed);
	}

	public void monsterToggled(bool button_pressed)
	{
		mother.SetMeta("monster", button_pressed);
	}

	public void fpsToggled(bool button_pressed)
	{
		mother.SetMeta("fps", button_pressed);
		if (button_pressed)
		{
			fps_label.ProcessMode = ProcessModeEnum.Inherit;
			fps_label.Visible = true;
		}
		else
		{
			fps_label.ProcessMode = ProcessModeEnum.Disabled;
			fps_label.Visible = false;
		}
	}

	public void play()
	{
		Visible = false;

		var level = GD.Load<PackedScene>("res://Scenes/level.tscn");

		mother.AddChild(level.Instantiate());

		ProcessMode = ProcessModeEnum.Disabled;
	}

}
