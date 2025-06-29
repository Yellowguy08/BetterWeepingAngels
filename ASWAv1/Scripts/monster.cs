using Godot;
using Godot.NativeInterop;
using System;
using System.Diagnostics;
// using System.Numerics;

public partial class monster : CharacterBody3D
{

	const float SPEED = 3.0f;
	Vector3 MAPSIZE = new(50, 50, 50);
	Node MAIN;

	// Navigation Components
	NavigationRegion3D nav_region;
	NavigationAgent3D nav_agent;
	Vector3 last_position;
	Vector3 next_location;
	Vector3 new_velocity;
	Node pathing_walls;

	bool over_stare = false;
	bool is_on_screen = false;
	bool started = false;

	RayCast3D l_sight_line,
			  r_sight_line,
			  f_wall_cast,
			  b_wall_cast;

	Rid l_sight_target,
		r_sight_target;


	CharacterBody3D player;


	Node3D creature;
	AnimationPlayer anims;
	Timer timer;



	public override void _Ready()
	{

		MAIN = GetNode("/root/MAIN");

		nav_region = GetNode<NavigationRegion3D>("../NavigationRegion3D");
		nav_agent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		pathing_walls = nav_region.GetNode<Node>("pathWalls");

		l_sight_line = GetNode<RayCast3D>("RayCast3D");
		r_sight_line = GetNode<RayCast3D>("RayCast3D2");
		f_wall_cast = GetNode<RayCast3D>("f_wall_cast");
		b_wall_cast = GetNode<RayCast3D>("b_wall_cast");
		
		player = GetNode<CharacterBody3D>("../CharacterBody");
		
		creature = GetNode<Node3D>("creature");
		anims = creature.GetNode<AnimationPlayer>("AnimationPlayer");
		timer = GetNode<Timer>("Timer");

		if (!(bool)MAIN.GetMeta("monster"))
		{
			creature.Visible = false;
			creature.ProcessMode = ProcessModeEnum.Disabled;
			GetNode<MeshInstance3D>("meshbody").Visible = true;
		}

		f_wall_cast.TargetPosition = new(0, -1, -MAPSIZE.Z);
		b_wall_cast.TargetPosition = new(0, -1, MAPSIZE.Z);

		last_position = GlobalPosition;

		SetPhysicsProcess(false);
		NavigationServer3D.MapChanged += MapChanged;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public override void _PhysicsProcess(double delta)
	{

		if (over_stare)
		{
			// Create Temp Pathing Wall on Timeout.
			TimeoutFunction();
		}
		else
		{

			UpdateTargetPosition(player.GlobalTransform.Origin); // Done at end of physics frame

			LookAt(player.GlobalPosition);

			if (next_location != GlobalPosition)
			{
				creature.LookAt(next_location * new Vector3(1,0,1));
			}

			Vector3 curr_location = GlobalTransform.Origin;

			new_velocity = (next_location - curr_location).Normalized() * SPEED;

			Vector3 pos_diff = player.GlobalPosition - r_sight_line.GlobalPosition;
			float dist = Mathf.Sqrt(pos_diff.X * pos_diff.X + pos_diff.Z * pos_diff.Z);

			l_sight_line.TargetPosition = new Vector3(0.4f, 0, -dist);
			r_sight_line.TargetPosition = new Vector3(-0.4f, 0, -dist);

			if (l_sight_line.IsColliding())
			{
				l_sight_target = l_sight_line.GetColliderRid();
			}

			if (r_sight_line.IsColliding())
			{
				r_sight_target = r_sight_line.GetColliderRid();
			}

			if (!IsInSight())
			{

				next_location = nav_agent.GetNextPathPosition(); // Gets current map path.

				anims.Play("Run");

				// Save last position that is a suitable distance from temp wall.
				if (Mathf.Abs(GlobalPosition.DistanceTo(last_position)) >= 1f)
				{

					last_position = GlobalPosition;

				}

				// UpdateTargetPosition(player.GlobalTransform.Origin);

				Velocity = Velocity.MoveToward(new_velocity, 0.25f);
				MoveAndSlide();

			}
			else
			{

				anims.Pause();

				if (!started)
				{
					if (IsSingleSightLine())
					{

						foreach (Node3D child in pathing_walls.GetChildren())
						{
							child.QueueFree();
						}

						started = true;
						timer.Start(5);
					}

				}
			}
		}
	}


	public void UpdateTargetPosition(Vector3 targetPosition)
	{
		nav_agent.TargetPosition = targetPosition;
	}

	public void screenEntered()
	{
		is_on_screen = true;
	}

	public void screenExited()
	{
		is_on_screen = false;
	}

	public void targetReached()
	{
		MAIN.GetNode("Control").ProcessMode = ProcessModeEnum.Inherit;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		MAIN.GetNode<Control>("Control").Visible = true;
		GetParent().QueueFree();
	}

	public void timeout()
	{

		over_stare = true;

	}

	public void TimeoutFunction()
	{
		if (IsSingleSightLine())
		{

			CreateWall();

			nav_region.BakeNavigationMesh();

			GlobalPosition = last_position;
			Velocity = new_velocity * new Vector3(-1, 1, -1);

		}

		started = false;
		over_stare = false;
	}

	public bool IsSingleSightLine()
	{
		bool r1 = l_sight_line.IsColliding() && l_sight_line.GetColliderRid() == player.GetRid();
		bool r2 = r_sight_line.IsColliding() && r_sight_line.GetColliderRid() == player.GetRid();
		bool r3 = f_wall_cast.IsColliding() && f_wall_cast.GetColliderRid() == player.GetRid();

		if (r3 || (r2 == r1))
		{
			return false;
		}

		return true;

	}

	public bool IsInSight()
	{
		return is_on_screen && (l_sight_target == player.GetRid() || r_sight_target == player.GetRid());
	}

	public void CreateWall()
	{

		Vector3 midpoint = GlobalPosition;
		float wall_size = 0;

		if (f_wall_cast.IsColliding() && b_wall_cast.IsColliding())
		{

			Vector3 fWall = f_wall_cast.GetCollisionPoint();
			Vector3 bWall = b_wall_cast.GetCollisionPoint();

			wall_size = fWall.DistanceTo(bWall);
			midpoint = fWall - (fWall - bWall) / 2;

		}

		MeshInstance3D wall = new();
		BoxMesh mesh = new();

		mesh.Size = new(0.01f, 10, wall_size);

		wall.Mesh = mesh;
		wall.Visible = false;
		wall.Rotation = Rotation;

		pathing_walls.AddChild(wall);
		pathing_walls.GetChild<Node3D>(pathing_walls.GetChildCount() - 1).GlobalPosition = midpoint;

	}

	public void MapChanged(Rid map)
	{

		SetPhysicsProcess(true);
		NavigationServer3D.MapChanged -= MapChanged;

	}
}
