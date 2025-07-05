using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;
using System.Diagnostics;

public partial class monster : CharacterBody3D
{

	public enum Behaviors_States
	{

		Stalking,
		Staring,
		Repositioning

	}

	int frameCounter = 0;

	Behaviors_States curr_behavior = Behaviors_States.Stalking;
	Behaviors_States last_behavior;


	const float SPEED = 3.0f;
	Vector3 MAPSIZE = new(50, 50, 50);
	Node MAIN;

	// Navigation Components
	NavigationRegion3D nav_region;
	CollisionShape3D collider;
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

		l_sight_line = GetNode<RayCast3D>("l_sight_line");
		r_sight_line = GetNode<RayCast3D>("r_sight_line");
		f_wall_cast = GetNode<RayCast3D>("f_wall_cast");
		b_wall_cast = GetNode<RayCast3D>("b_wall_cast");

		player = GetNode<CharacterBody3D>("../CharacterBody");

		creature = GetNode<Node3D>("creature");
		collider = GetNode<CollisionShape3D>("CollisionShape3D");
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

		// Vector3 last_path_pos = Vector3.Inf;

		// Collect Sightline Information
		Vector3 pos_diff = player.GlobalPosition - GlobalPosition;
		float dist = Mathf.Sqrt(pos_diff.X * pos_diff.X + pos_diff.Z * pos_diff.Z);

		l_sight_line.TargetPosition = new Vector3(0.4f, 0, -dist);
		r_sight_line.TargetPosition = new Vector3(-0.4f, 0, -dist);

		LookAt(player.GlobalPosition);

		if (l_sight_line.IsColliding())
		{
			l_sight_target = l_sight_line.GetColliderRid();
		}

		if (r_sight_line.IsColliding())
		{
			r_sight_target = r_sight_line.GetColliderRid();
		}


		// Check Behavior
		if (over_stare)
		{

			curr_behavior = Behaviors_States.Repositioning;

		}
		else if (IsInSight())
		{

			if (!(curr_behavior == Behaviors_States.Repositioning && IsSingleSightLine()))
			{
				curr_behavior = Behaviors_States.Staring;
			}

		}
		else
		{

			curr_behavior = Behaviors_States.Stalking;

		}

		if (last_behavior != curr_behavior)
		{

			// GD.Print("Last Behav : " + last_behavior + " != Curr Behav: " + curr_behavior);

			InitializeNewBehavior(curr_behavior);

		}

		switch (curr_behavior)
		{

			case Behaviors_States.Stalking:
				nav_agent.TargetPosition = player.GlobalPosition;

				MoveAlongPath();

				break;

			case Behaviors_States.Staring:
				// Target Player Option 2

				// if (started && !IsSingleSightLine()) { }

				if (!started)
				{

					if (IsSingleSightLine())
					{
						timer.Start(5);
						timer.Paused = false;
						started = true;
					}

				}
				else if (!IsSingleSightLine()) { timer.Paused = true; started = false; }

				break;

			case Behaviors_States.Repositioning:

				if (IsInSight() && !IsSingleSightLine())
				{

					over_stare = false;

				}
				else
				{
					MoveAlongPath();
				}

				break;

		}

		/*if (curr_behavior == Behaviors_States.Stalking)
		{

			nav_agent.TargetPosition = player.GlobalPosition;

			MoveAlongPath();

		}
		else if (curr_behavior == Behaviors_States.Staring)
		{
			// Target Player Option 2

			// if (started && !IsSingleSightLine()) { }

			if (!started)
			{

				if (IsSingleSightLine())
				{
					timer.Start(5);
					timer.Paused = false;
					started = true;
					GD.Print("Started");
				}

			}
			else if (!IsSingleSightLine()) { timer.Paused = true; started = false; }

		}
		else if (curr_behavior == Behaviors_States.Repositioning)
		{

			MoveAlongPath();

		}*/

		// if (over_stare)
		// {
		// 	// Create Temp Pathing Wall on Timeout.
		// 	TimeoutFunction();
		// }
		// else
		// {

		// 	UpdateTargetPosition(player.GlobalTransform.Origin); // Done at end of physics frame

		// 	if (next_location != GlobalPosition)
		// 	{
		// 		creature.LookAt(next_location * new Vector3(1, 0, 1));
		// 	}

		// 	

		// 	if (!IsInSight())
		// 	{

		// 		anims.Play("Run");

		// 		// Save last position that is a suitable distance from temp wall.
		// 		if (Mathf.Abs(GlobalPosition.DistanceTo(last_position)) >= 1f)
		// 		{

		// 			last_position = GlobalPosition;

		// 		}

		// 		// UpdateTargetPosition(player.GlobalTransform.Origin);

		// 		

		// 	}
		// 	else
		// 	{

		// 		anims.Pause();

		// 		if (!started)
		// 		{
		// 			if (IsSingleSightLine())
		// 			{

		// 				

		// 				started = true;
		// 				timer.Start(5);
		// 			}

		// 		}
		// 	}
		// }

		last_behavior = curr_behavior;
		creature.LookAt(next_location);
	}


	// public void UpdateTargetPosition(Vector3 targetPosition)
	// {
	// 	nav_agent.TargetPosition = targetPosition;
	// }

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

		if (over_stare)
		{
			nav_region.BakeNavigationMesh();
			over_stare = false;

		}
		else
		{
			MAIN.GetNode("Control").ProcessMode = ProcessModeEnum.Inherit;
			Input.MouseMode = Input.MouseModeEnum.Visible;
			MAIN.GetNode<Control>("Control").Visible = true;
			GetParent().QueueFree();
		}
	}

	public void timeout()
	{


		if (IsSingleSightLine())
		{
			over_stare = true;
			CreateWall();

			// FInd new target


		}

	}

	public void SetRepositioningTarget()
	{

		Vector3 global_target;
		Vector3 normal_vector;

		if (r_sight_target == player.GetRid())
		{

			global_target = player.GlobalPosition + (GlobalPosition.MoveToward(l_sight_line.GlobalPosition, 0.1f) - GlobalPosition);
			normal_vector = new Vector3(1, 0, 1) * (l_sight_line.GlobalPosition - global_target).Normalized();

			nav_agent.TargetPosition = l_sight_line.GlobalPosition + normal_vector * new RandomNumberGenerator().RandfRange(2.0f, 3.0f);

		}
		else
		{

			global_target = player.GlobalPosition + (GlobalPosition.MoveToward(r_sight_line.GlobalPosition, 0.1f) - GlobalPosition);
			normal_vector = new Vector3(1, 0, 1) * (r_sight_line.GlobalPosition - global_target).Normalized();

			nav_agent.TargetPosition = r_sight_line.GlobalPosition + normal_vector * new RandomNumberGenerator().RandfRange(2.0f, 3.0f);

		}

		// CreateWaypoint("servertarget", nav_agent.TargetPosition, new(0, 0, 0));

	}

	// public void TimeoutFunction()
	// {
	// 	if (IsSingleSightLine())
	// 	{

	// 		CreateWall();

	// 		nav_region.BakeNavigationMesh();

	// 		GlobalPosition = last_position;
	// 		Velocity = new_velocity * new Vector3(-1, 1, -1);

	// 	}

	// 	started = false;
	// 	over_stare = false;
	// }

	public bool IsSingleSightLine()
	{
		bool r1 = l_sight_line.IsColliding() && l_sight_line.GetColliderRid() == player.GetRid();
		bool r2 = r_sight_line.IsColliding() && r_sight_line.GetColliderRid() == player.GetRid();
		bool r3 = f_wall_cast.IsColliding() && f_wall_cast.GetColliderRid() == player.GetRid();

		if (r3 || (r2 == r1))
		{
			// GD.Print("Not Single Line of Sight");
			return false;
		}
		// GD.Print("Is Single Line of Sight");
		return true;

	}

	public bool IsInSight()
	{
		// if (is_on_screen && (l_sight_target == player.GetRid() || r_sight_target == player.GetRid()))
		// {
		// 	GD.Print("In Sight");
		// }
		// else
		// {
		// 	GD.Print("Not In Sight");
		// }

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
		wall.TreeExited += ReBake;
		pathing_walls.GetChild<Node3D>(pathing_walls.GetChildCount() - 1).GlobalPosition = midpoint;

	}

	public void MapChanged(Rid map)
	{

		SetPhysicsProcess(true);
		NavigationServer3D.MapChanged -= MapChanged;

	}

	public void MoveToPoint(Vector3 goal)
	{


		next_location = nav_agent.GetNextPathPosition(); // Gets current map path.

		new_velocity = (next_location - GlobalTransform.Origin).Normalized() * SPEED;

		Velocity = Velocity.MoveToward(new_velocity, 0.25f);
		MoveAndSlide();

	}

	public void InitializeNewBehavior(Behaviors_States new_behavior)
	{

		switch (new_behavior)
		{

			case Behaviors_States.Stalking:
				anims.Play("Run");
				nav_agent.TargetDesiredDistance = 2;
				return;

			case Behaviors_States.Staring:
				anims.Stop();
				// Target Player Option 1
				Velocity = Vector3.Zero;

				if (started) { timer.Paused = true; started = false; }

				if (pathing_walls.GetChildCount() > 0)
				{

					foreach (Node3D child in pathing_walls.GetChildren()) { child.QueueFree(); }

				}

				return;

			case Behaviors_States.Repositioning:
				nav_agent.TargetDesiredDistance = 1;
				SetRepositioningTarget();

				return;

		}

	}

	public void CreateWaypoint(String name, Vector3 position, Vector3 rotation)
	{

		MeshInstance3D a = new();
		BoxMesh a_mesh = new();
		a_mesh.Size = new(0.5f, 0.5f, 0.5f);
		a.Mesh = a_mesh;
		a.Name = name;
		a.RotationDegrees = rotation;

		GetParent().AddChild(a);
		GetParent().GetNode<Node3D>(name).GlobalPosition = position;

	}

	public void MoveAlongPath()
	{

		Vector3 movement_velocity = (nav_agent.GetNextPathPosition() - GlobalPosition).Normalized();

		// GD.Print("Player Pos: " + player.GlobalPosition);
		// GD.Print("Target Pos: " + nav_agent.TargetPosition);
		// GD.Print("Next Path Pos: " + nav_agent.GetNextPathPosition());
		// GD.Print("Movement Velocity: " + movement_velocity);

		Velocity = movement_velocity * SPEED;

		next_location = nav_agent.GetNextPathPosition() * new Vector3(1, 0, 1);

		// GD.Print("Velocity: " + Velocity);

		// GD.Print("");

		MoveAndSlide();

	}


	public void ReBake()
	{
		nav_region.BakeNavigationMesh();

	}
	
}
