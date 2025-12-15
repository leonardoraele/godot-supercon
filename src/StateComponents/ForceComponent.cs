using Godot;
using Godot.Collections;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class ForceComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum ForceTypeEnum
	{
		FixedDirection = 0,
		Drag = 1,
		AlignedToFacingDirection = 2,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public ForceTypeEnum ForceType
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } } = ForceTypeEnum.FixedDirection;
	[Export] public Vector2 Direction = Vector2.Zero;
	[Export] public float AccelerationPxPSecSq = 500f;
	[Export] public float MaxSpeedPxPSec = float.PositiveInfinity;

	// TODO
	// [ExportGroup("Changes Over Time")]
	// [Export(PropertyHint.GroupEnable)] public bool TimeBased = false;
	// [Export] public float InitialAccelerationPxPSecSq = 0f;
	// [Export(PropertyHint.ExpEasing)] public float Curve = 1f;

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	public Vector2 ForceDirection => this.ForceType switch
	{
		ForceTypeEnum.FixedDirection => this.Direction.Normalized(),
		ForceTypeEnum.AlignedToFacingDirection => Vector2.Right * this.Character.HorizontalFacingDirection,
		ForceTypeEnum.Drag => this.Character.Velocity.Normalized() * -1,
		_ => Vector2.Zero,
	};

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _SuperconPhysicsProcess(double delta)
	{
		base._SuperconPhysicsProcess(delta);
		switch (this.ForceType)
		{
			case ForceTypeEnum.Drag when this.Character.Velocity.Length() < this.AccelerationPxPSecSq * (float) delta:
				this.Character.Velocity = Vector2.Zero;
				break;
			case ForceTypeEnum.Drag:
				this.Character.ApplyForce(this.ForceDirection * this.AccelerationPxPSecSq * (float) delta);
				break;
			default:
				this.Character.ApplyForce(
					this.ForceDirection * this.AccelerationPxPSecSq * (float) delta,
					this.MaxSpeedPxPSec
				);
				break;
		}
	}

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(Direction):
				property["usage"] = (long) (this.ForceType switch
				{
					ForceTypeEnum.FixedDirection => PropertyUsageFlags.Default,
					_ => PropertyUsageFlags.NoEditor,
				});
				break;
			case nameof(MaxSpeedPxPSec):
				property["usage"] = (long) (this.ForceType switch
				{
					ForceTypeEnum.Drag => PropertyUsageFlags.NoEditor,
					_ => PropertyUsageFlags.Default,
				});
				break;
		}
	}
}
