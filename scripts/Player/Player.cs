using System.Runtime.CompilerServices;
using Godot;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;
using agame.scripts.Player;

public partial class Player : CharacterBody3D
{
	// --- Player camera ---
	Camera3D playerCamera;
	private float _cameraJiggleAmmount = 0.3f;
	private Vector3 _cameraInitalPos;
	private Vector3 _cameraTargetPos;
	private bool _wasOnFloor = false;

	// --- Grapple gun --- (TODO: move somewhere else)
	private Vector3? _grappleHit = null;
	private float _grappleStren = 80.0f;
	private Rope _rope = null;

	// --- Movement related ---
	private PlayerMovement _playerMovement;

	[Export] private WateringCan _waterCan; // TODO: make item base class so that we have one var for current item which then in turn inherites

	public override void _Ready()
	{
		_playerMovement = new PlayerMovement();
		playerCamera = (Camera3D)GetNode("PlayerCamera");
		_cameraTargetPos = playerCamera.Position;
		_cameraInitalPos = playerCamera.Position;
	}

	public override void _Process(double delta)
	{
		// --- Read input from player ---
		_playerMovement.HandleInput(playerCamera.Basis);

		// --- Camera control ---
		playerCamera.Position = playerCamera.Position.Lerp(_cameraTargetPos, (float)delta * 10f);

		// --- Watering can ---
		if (Input.IsActionJustPressed("mouse_click_right"))
		{
			_waterCan.setPouring(true);
		}
		if (Input.IsActionJustReleased("mouse_click_right"))
		{
			_waterCan.setPouring(false);
		}

		// --- Grapple gun ---
		if (Input.IsActionJustPressed("mouse_click_left"))
		{
			_grappleHit = GetCamRaycast(playerCamera, 3000.0f);
			if (_grappleHit != null)
			{
				PackedScene ropeScene = GD.Load<PackedScene>("res://assets/prefabs/rope.tscn");
				_rope = ropeScene.Instantiate<Rope>();
				GetTree().CurrentScene.AddChild(_rope);
			}
		}
		if (Input.IsActionJustReleased("mouse_click_left"))
		{
			_grappleHit = null;
			if (_rope != null)
				_rope.QueueFree();
		}

		if (_grappleHit.HasValue)
		{
			_rope.startPoint = playerCamera.GlobalPosition - new Vector3(0f, 0.3f, 0f);
			_rope.endPoint = _grappleHit.Value;

			Vector3 toGrapple = _grappleHit.Value - playerCamera.GlobalPosition;
			toGrapple = toGrapple.Normalized();
			_playerMovement.finalVelocity += toGrapple * (float)delta * _grappleStren * playerCamera.Position.DistanceTo(_grappleHit.Value) / 10.0f;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_playerMovement.HandleMovement(IsOnFloor(), (float)delta);
		Velocity = _playerMovement.finalVelocity;
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

	private async void DoCameraImpact()
	{
		_cameraTargetPos = _cameraInitalPos - new Vector3(0.0f, 0.5f, 0.0f);
		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		_cameraTargetPos = _cameraInitalPos;
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
