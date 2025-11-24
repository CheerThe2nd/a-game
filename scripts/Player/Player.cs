using System.Runtime.CompilerServices;
using Godot;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;

public partial class Player : CharacterBody3D
{
	Camera3D playerCamera;
	private float _accel = 10.0f;
	private float _maxSpeed = 10.0f;
	private float _jumpStren = 20.0f;
	private float _gravityStren = 30.0f;
	private float _airCtl = 0.04f;
	private float _firction = 14.0f;
	private float _frictionSpeedThreshold = 0.5f;
	private Vector3 _desireMoveDir;
	private float _cameraJiggleAmmount = 0.3f;

	private Vector3 _cameraInitalPos;
	private Vector3 _cameraTargetPos;
	private bool _wasOnFloor = false;
	private Vector3? _grappleHit = null;
	private float _grappleStren = 80.0f;
	private Rope _rope = null;

	public override void _Ready()
	{
		playerCamera = (Camera3D)GetNode("PlayerCamera");
		_cameraTargetPos = playerCamera.Position;
		_cameraInitalPos = playerCamera.Position;
	}

	public void HandleMovementInput()
	{
		Vector3 localVelocity = new Vector3(0.0f, 0.0f, 0.0f);
		if (Input.IsActionPressed("move_left"))
			localVelocity.X = -1.0f;
		else if (Input.IsActionPressed("move_right"))
			localVelocity.X = 1.0f;
		else
			localVelocity.X = 0.0f;

		if (Input.IsActionPressed("move_forward"))
			localVelocity.Z = -1.0f;
		else if (Input.IsActionPressed("move_backwards"))
			localVelocity.Z = 1.0f;
		else
			localVelocity.Z = 0.0f;

		localVelocity = localVelocity.Normalized();
		Godot.Basis basis = playerCamera.GlobalBasis;
		_desireMoveDir = basis.X * localVelocity.X + basis.Z * localVelocity.Z;
	}


	public override void _Process(double delta)
	{
		HandleMovementInput();
		playerCamera.Position = playerCamera.Position.Lerp(_cameraTargetPos, (float)delta * 10f);

		// --- Raycast action ---
		if (Input.IsActionJustPressed("mouse_click"))
		{
			_grappleHit = GetCamRaycast(playerCamera, 100.0f);
			if (_grappleHit != null)
			{
				PackedScene ropeScene = GD.Load<PackedScene>("res://assets/prefabs/rope.tscn");
				_rope = ropeScene.Instantiate<Rope>();
				GetTree().CurrentScene.AddChild(_rope);
			}
		}
		if (Input.IsActionJustReleased("mouse_click"))
		{
			_grappleHit = null;
			if(_rope != null)
				_rope.QueueFree();
		}

		if (_grappleHit.HasValue)
		{
			_rope.startPoint = playerCamera.GlobalPosition - new Vector3(0f, 0.3f, 0f);
			_rope.endPoint = _grappleHit.Value;

			Vector3 toGrapple = _grappleHit.Value - playerCamera.GlobalPosition;
			toGrapple = toGrapple.Normalized();
			Velocity += toGrapple * (float)delta * _grappleStren * playerCamera.Position.DistanceTo(_grappleHit.Value) / 10.0f;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyGravity(delta);
		if (IsOnFloor())
		{
			// --- Movement ---
			if (Input.IsActionJustPressed("jump"))
				Velocity += new Vector3(0.0f, _jumpStren, 0.0f);

			if (Velocity.Length() < _maxSpeed)
			{
				Velocity += new Vector3(
				_desireMoveDir.X * _accel,
				0.0f,
				_desireMoveDir.Z * _accel);
			}
			DoFriction((float)delta);
		}
		else
		{
			if (Velocity.Length() < _maxSpeed && Velocity.Dot(_desireMoveDir) > -0.1f)
			{
				Velocity += new Vector3(
				_desireMoveDir.X * _accel * _airCtl,
				0.0f,
				_desireMoveDir.Z * _accel * _airCtl);
			}
		}

		MoveAndSlide();

		// Check for chnage
		if (IsOnFloor() != _wasOnFloor)
		{
			if (_wasOnFloor)
				OnJumpStart();
			else
				OnJumpImpact();
		}
		_wasOnFloor = IsOnFloor();
	}

	// --- Jumping callbacks ---
	private void OnJumpImpact()
	{
		DoCameraImpact();
	}

	private void OnJumpStart()
	{

	}

	public void DoFriction(float dTimeSec)
	{
		float speed = Velocity.Length();
		if (speed <= 0.00001f)
			return;
		float downLimit = Mathf.Max(speed, _frictionSpeedThreshold);
		float dropAmount = speed - (downLimit * _firction * dTimeSec);
		if (dropAmount < 0)
			dropAmount = 0;
		Velocity *= dropAmount / speed;
	}

	private async void DoCameraImpact()
	{
		_cameraTargetPos = _cameraInitalPos - new Vector3(0.0f, 0.5f, 0.0f);
		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		_cameraTargetPos = _cameraInitalPos;
	}
	private void ApplyGravity(double delta)
	{
		if (!IsOnFloor())
			Velocity -= new Vector3(0.0f, _gravityStren * (float)delta, 0.0f);
		else if (Velocity.Y < 0)
			Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
	}
	private Vector3? GetCamRaycast(Camera3D camera, float maxDistance)
	{
		PhysicsRayQueryParameters3D parameters = new PhysicsRayQueryParameters3D
		{
			From = camera.GlobalPosition,
			To = camera.GlobalPosition + -camera.GlobalTransform.Basis.Z * maxDistance,
			CollideWithAreas = true,
			CollideWithBodies = true
		};
		parameters.Exclude = new Godot.Collections.Array<Rid> { camera.GetCameraRid() };
		var spaceState = GetWorld3D().DirectSpaceState;
		Godot.Collections.Dictionary result = spaceState.IntersectRay(parameters);
		if (result.Count > 0)
			return (Vector3)result["position"];
		return null;
	}


}
