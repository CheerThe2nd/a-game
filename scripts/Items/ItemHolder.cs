using Godot;
using System;

public partial class ItemHolder : Node3D
{
	[Export] private float _smoothAmount = 8f;
	[Export] private float _swayAmount = 0.02f;
	[Export] private float _maxSway = 0.1f;
	[Export] private float _rotationAmount = 2f;
	[Export] private float _maxRotationAmount = 5f;
	[Export] private float _smoothRotation = 6f;

	private Vector3 _initalPos;
	private Vector3 _initalRot;
	private Transform3D _transform;
	private Vector2 _deltaMouseMove;

	public override void _Ready()
	{
		_transform = this.Transform;
		_initalPos = this.Position;
		_initalRot = this.RotationDegrees;
	}

	public override void _Process(double delta)
	{
		_deltaMouseMove = _deltaMouseMove.Slerp(new Vector2(0f,0f), (float)delta);
		ApplyWeaponSway((float)delta);
		ApplyTiltSway((float)delta);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion m && Input.MouseMode == Input.MouseModeEnum.Captured)
			_deltaMouseMove = new Vector2(m.Relative.X, m.Relative.Y);
	}

	private void ApplyWeaponSway(float dTimeSec)
	{
		float swayX = Mathf.Clamp(_deltaMouseMove.X * _swayAmount, -_maxSway, _maxSway);
		float swayY = Mathf.Clamp(_deltaMouseMove.Y * _swayAmount, -_maxSway, _maxSway);
		Vector3 swayPosition = _initalPos + new Vector3(swayX, swayY, 0f);

		this.Position = this.Position.Lerp(swayPosition, dTimeSec * _smoothAmount);
	}

	private void ApplyTiltSway(float dTimeSec)
	{
		// Get mouse input for rotation
		float tiltX = -Mathf.Clamp(_deltaMouseMove.X * _rotationAmount, -_maxRotationAmount, _maxRotationAmount);
		float tiltY = Mathf.Clamp(_deltaMouseMove.Y * _rotationAmount, -_maxRotationAmount, _maxRotationAmount);

		// Smoothly rotate weapon
		this.RotationDegrees = this.RotationDegrees.Slerp(new Vector3(tiltY, tiltX, tiltX), dTimeSec * _smoothAmount);
	}

}
