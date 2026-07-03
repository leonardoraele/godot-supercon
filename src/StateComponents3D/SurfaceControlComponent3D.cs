using Godot;
using Godot.Collections;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents3D;

/// <summary>
/// Allows the player to control the character using directional input to move it along a surface. e.g. To move the
/// character over the floor or climbing a wall.
///
/// You can set different speed values for forward, lateral, and backward movement.
///
/// This component does not not update the character's rotation direction. For that, use the FacingComponent.
/// </summary>
[Tool][GlobalClass]
public partial class SurfaceControlComponent3D : SuperconStateComponent3D
{
	//==================================================================================================================
		#region STATICS
	//==================================================================================================================

	// public static readonly string MyConstant = "";

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region EXPORTS
	//==================================================================================================================

	[Export] public SurfaceTypeEnum Surface = SurfaceTypeEnum.Floor;
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxSpeed
		{ get; set { if (value != field) field = /*this.MaxForwardSpeed =*/ value.Clamped(0f, value); } }
		= 5f;
	[Export(PropertyHint.None, "suffix:m/s²")] public float Acceleration
		{ get; set => field = value.Clamped(0f, value); }
		= 10f;
	[Export(PropertyHint.None, "suffix:m/s²")] public float Deceleration
		{ get; set { if (value != field) field = /*this.DragDeceleration =*/ value.Clamped(0f, value); } }
		= 15f;
	[Export(PropertyHint.Range, "0,1080,5,radians_as_degrees,or_greater,suffix:°/s")] public float AngularVelocity
	{
		get;
		set
		{
			if (value == field)
				return;
			// this.TurnMinAngularVelocity = this.TurnMinAngularVelocity.Clamped(0f, value);
			/*this.TurnMaxAngularVelocity =*/ field = value;
		}
	}
		= Mathf.Pi * 2;
	[Export] public bool PreserveOrthogonalVelocity = true;

	[ExportGroup("Rotate Character", "Rotation")]
	[Export(PropertyHint.GroupEnable)] public bool RotationEnabled = false;
	[Export] public AlignmentOptionsEnum RotationForwardAlignment = AlignmentOptionsEnum.MovementDirection;
	[Export] public Vector3 RotationLocalForwardDirection = Vector3.Forward;
	[Export] public AlignmentOptionsEnum RotationUpAlignment = AlignmentOptionsEnum.SurfaceNormal;
	[Export] public Vector3 RotationLocalUpDirection = Vector3.Up;

	// [ExportGroup("Break MaxSpeed")]
	// [Export(PropertyHint.GroupEnable)] public bool MaxSpeedOptionsEnabled
	// 	{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	// 	= false;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxForwardSpeed
	// 	{ get; set { if (value != field) field = this.MaxSpeed = value.Clamped(0f, value); } }
	// 	= 5f;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxLateralSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxBackwardSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxVerticalSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;

	// [ExportGroup("Break Deceleration")]
	// /// <summary>
	// /// Determines the deceleration behavior when the character's speed exceeds the maximum speed.
	// ///
	// /// If this is higher than <see cref="Deceleration"/>, the character will decelerate faster when above max speed.
	// /// </summary>
	// [Export(PropertyHint.GroupEnable)] public bool DecelerationOptionsEnabled
	// 	{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	// 	= false;
	// [Export(PropertyHint.None, "suffix:m/s²")] public float DragDeceleration
	// 	{ get; set { if (value != field) field = this.Deceleration = value.Clamped(0f, value); } }
	// 	= 15f;
	// [Export(PropertyHint.None, "suffix:m/s²")] public float DecelerationAboveMaxSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;

	// [ExportGroup("Break Angular Velocity", "Turn")]
	// [Export(PropertyHint.GroupEnable)] public bool TurnEnabled
	// 	{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	// 	= false;
	// // TODO Implement dynamic range min/max using _ValidateProperty() instead of setter
	// [Export(PropertyHint.Range, "0,1080,5,radians_as_degrees,or_greater,suffix:°/s")] public float TurnMinAngularVelocity
	// 	{ get; set { field = value.Clamped(0f, this.TurnMaxAngularVelocity); } }
	// 	= Mathf.Pi / 2;
	// // TODO Implement dynamic range min/max using _ValidateProperty() instead of setter
	// [Export(PropertyHint.Range, "0,1080,5,radians_as_degrees,or_greater,suffix:°/s")] public float TurnMaxAngularVelocity
	// 	{ get; set { if (value != field) this.AngularVelocity = field = value.Clamped(this.TurnMinAngularVelocity, float.PositiveInfinity); } }
	// 	= Mathf.Pi * 2;
	// [Export(PropertyHint.None, "m/s²")] public float TurnLowerMovementSpeed
	// 	{ get; set { field = value.Clamped(0f, this.TurnUpperMovementSpeed); } }
	// 	= 0f;
	// [Export(PropertyHint.None, "m/s²")] public float TurnUpperMovementSpeed
	// 	{ get; set { field = value.Clamped(this.TurnLowerMovementSpeed, float.PositiveInfinity); } }
	// 	= 5f;
	// // [Export] public bool TurnExtrapolateBeyondMaxSpeed = false;
	// [Export(PropertyHint.ExpEasing, "attenuation")] public float TurnCurve = 1f;

	// [ExportGroup("Brake On Sharp Turns")]
	// [Export(PropertyHint.GroupEnable)] public bool SharpTurnOptionsEnabled = false;
	// [Export(PropertyHint.Range, "0,180,5,radians_as_degrees,suffix:°")] public float SoftBrakeTurnAngle = Mathf.Pi / 3;
	// [Export(PropertyHint.ExpEasing, "attenuation")] public float SoftBrakeVelocityLoss = 1f;
	// [Export(PropertyHint.Range, "0,180,5,radians_as_degrees,suffix:°")] public float HardBrakeTurnAngle = Mathf.Pi / 3;
	// [Export(PropertyHint.None, "suffix:m/s²")] public float HardBrakeDeceleration
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 50f;

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region FIELDS
	//==================================================================================================================

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region COMPUTED PROPERTIES
	//==================================================================================================================

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region EVENTS & SIGNALS
	//==================================================================================================================

	[Signal] public delegate void SurfaceExitEventHandler();

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region INTERNAL TYPES
	//==================================================================================================================

	public enum SurfaceTypeEnum : sbyte {
		Floor = 16,
		Wall = 20,
		Ceiling = 24,
	}

	public enum AlignmentOptionsEnum : sbyte {
		MovementDirection = 16,
		SurfaceNormal = 32,
		CameraForward = 48,
		Gravity = 64,
		Global = 112,
		NoAlignment = 120,
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region OVERRIDES & VIRTUALS
	//==================================================================================================================

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.RotationLocalForwardDirection):
				if (this.RotationForwardAlignment == AlignmentOptionsEnum.NoAlignment)
				{
					this.RotationLocalForwardDirection = Vector3.Forward;
					property["usage"] = (long) PropertyUsageFlags.None;
				}
				if (this.RotationLocalForwardDirection.IsZeroApprox())
					property["error"] = "The Forward direction cannot be a zero vector.";
				break;
			case nameof(this.RotationLocalUpDirection):
				if (this.RotationUpAlignment == AlignmentOptionsEnum.NoAlignment)
				{
					this.RotationLocalUpDirection = Vector3.Up;
					property["usage"] = (long) PropertyUsageFlags.None;
					break;
				}
				if (this.RotationLocalUpDirection.IsZeroApprox())
					property["error"] = "The Up direction cannot be a zero vector.";
				break;
		}
	}

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		if (this.Character == null)
			return;
		if (this.ResolveGlobalMovementPlane() is not Plane plane)
		{
			if (this.TestSurfaceExit())
				this.EmitSignalSurfaceExit();
			return;
		}
		if (this.Character.GetViewport().GetCamera3D() is not Camera3D camera)
			return;
		Vector3 projectedVelocity = this.Character.Velocity.Project(plane with { D = 0 });
		float currentSpeed = projectedVelocity.Length();
		bool isMoving = currentSpeed > Mathf.Epsilon;
		Vector3 currentDirection = projectedVelocity.Normalized().DefaultIfZero(this.Character.GlobalBasis.Forward);
		Vector2 normalInput = this.Character.InputController?.NormalizedDirectionalInput ?? Vector2.Zero;
		float inputStrength = normalInput.Length();
		bool hasInput = inputStrength > Mathf.Epsilon;
		Vector3 inputDirection = normalInput.IsZeroApprox()
			? Vector3.Zero
			: Basis.LookingAt(plane.Normal * -1, camera.GlobalBasis.Up)
				* new Vector3(normalInput.X, normalInput.Y * -1, 0);
		Vector3 newGlobalDirection = isMoving && hasInput
				? currentDirection.RotateToward(inputDirection, this.AngularVelocity * delta)
			: isMoving ? currentDirection
			: hasInput ? inputDirection
			: this.Character.Basis.Forward;
		float targetSpeed = this.MaxSpeed * inputStrength;
		float acceleration = targetSpeed > currentSpeed - Mathf.Epsilon
			? this.Acceleration
			: this.Deceleration;
		float newSpeed = currentSpeed.MoveToward(targetSpeed, acceleration * (float) delta);
		this.Character.Velocity = newGlobalDirection * newSpeed
			+ (this.PreserveOrthogonalVelocity ? this.Character.Velocity.Project(plane.Normal) : Vector3.Zero);

		if (!this.RotationEnabled)
			return;

		Basis localBasis = new Basis(this.RotationLocalForwardDirection.Cross(this.RotationLocalUpDirection), this.RotationLocalUpDirection, this.RotationLocalForwardDirection);
		Vector3 forward = this.RotationForwardAlignment switch
			{
				AlignmentOptionsEnum.MovementDirection => newGlobalDirection,
				AlignmentOptionsEnum.SurfaceNormal => plane.Normal,
				AlignmentOptionsEnum.CameraForward => camera.GlobalBasis.Forward.Normalized(),
				AlignmentOptionsEnum.Gravity => this.Character.GetGravity().Normalized(),
				AlignmentOptionsEnum.Global => this.RotationLocalForwardDirection,
				_ => this.Character.GlobalBasis.Forward,
			};
		Vector3 up = this.RotationUpAlignment switch
			{
				AlignmentOptionsEnum.MovementDirection => newGlobalDirection,
				AlignmentOptionsEnum.SurfaceNormal => plane.Normal,
				AlignmentOptionsEnum.CameraForward => camera.GlobalBasis.Forward.Normalized(),
				AlignmentOptionsEnum.Gravity => this.Character.GetGravity().Normalized(),
				AlignmentOptionsEnum.Global => this.RotationLocalUpDirection,
				_ => this.Character.GlobalBasis.Up,
			};
		Basis globalBasis = new Basis(forward.Cross(up), up, forward);
		this.Character.GlobalBasis = (globalBasis * localBasis).Orthonormalized();
	}

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region METHODS
	//==================================================================================================================

	// private void SnapToSurface()
	// {
	// 	if (this.Character == null)
	// 		return;
	// 	switch (this.Surface)
	// 	{
	// 		case SurfaceTypeEnum.Floor when this.Character.IsOnFloor():
	// 			this.Character.ApplyFloorSnap();
	// 			break;
	// 		case SurfaceTypeEnum.Wall when this.Character.IsOnWall():
	// 			this.Character.MoveAndCollide(this.Character.GetWallNormal() * -1);
	// 			break;
	// 		case SurfaceTypeEnum.Ceiling when this.Character.IsOnCeiling():
	// 			this.Character.MoveAndCollide(this.Character.GlobalBasis.Up);
	// 			break;
	// 	}
	// }

	public Plane? ResolveGlobalMovementPlane()
		=> this.Surface switch
		{
			SurfaceTypeEnum.Floor when this.Character?.IsOnFloor() == true
				&& this.Character.GetLastSlideCollision() is KinematicCollision3D collision
					=> new Plane(collision.GetNormal(), collision.GetPosition()),
			SurfaceTypeEnum.Wall when this.Character?.IsOnWall() == true
				&& this.Character.GetLastSlideCollision() is KinematicCollision3D collision
					=> new Plane(collision.GetNormal(), collision.GetPosition()),
			SurfaceTypeEnum.Ceiling when this.Character?.IsOnCeiling() == true
				&& this.Character.GetLastSlideCollision() is KinematicCollision3D collision
					=> new Plane(collision.GetNormal(), collision.GetPosition()),
			_ => null,
		};

	private bool TestSurfaceExit()
		// Note: We assume that if this method is being called, then the character is not on the surface anymore. This
		// method only tests if the character has exit the surface this frame.
		=> this.Character != null && this.Surface switch
		{
			SurfaceTypeEnum.Floor => this.Character.TimeOnFloor > this.Character.GetPhysicsProcessDeltaTime() * -1 - Mathf.Epsilon,
			SurfaceTypeEnum.Wall => this.Character.TimeOnWall > this.Character.GetPhysicsProcessDeltaTime() * -1 - Mathf.Epsilon,
			SurfaceTypeEnum.Ceiling => this.Character.TimeOnCeiling > this.Character.GetPhysicsProcessDeltaTime() * -1 - Mathf.Epsilon,
			_ => false,
		};

	//==================================================================================================================
		#endregion
	//==================================================================================================================
}
