using Godot;
using Godot.Collections;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class ForceComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum ForceTypeEnum
	{
		FixedDirection,
		Gravity,
		FacingDirection,
		MovementResistance,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public ForceTypeEnum ForceType
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } } = ForceTypeEnum.FixedDirection;
	[Export] public Vector2 Direction = Vector2.Zero;
	[Export] public float AccelerationPxPSecSq = 600f;
	[Export] public float MaxSpeedPxPSec = 600f;

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	public Vector2 ComputedDirection => this.ForceType switch
	{
		ForceTypeEnum.FixedDirection => this.Direction,
		ForceTypeEnum.Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity_vector").AsVector2(),
		ForceTypeEnum.FacingDirection => Vector2.Right * this.Character.FacingDirection,
		ForceTypeEnum.MovementResistance => this.Character.Velocity.Normalized() * -1,
		_ => Vector2.Zero,
	};
	public float ComputedAccelerationPxPSecSq => this.ForceType switch
	{
		ForceTypeEnum.Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle(),
		_ => this.AccelerationPxPSecSq,
	};

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		base._PhysicsProcessActive(delta);
		this.Character.Accelerate(
			this.ComputedDirection.Normalized() * this.MaxSpeedPxPSec,
			this.ComputedAccelerationPxPSecSq * (float) delta
		);
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
			case nameof(AccelerationPxPSecSq):
				property["usage"] = (long) (this.ForceType switch
				{
					ForceTypeEnum.Gravity => PropertyUsageFlags.NoEditor,
					_ => PropertyUsageFlags.Default,
				});
				break;
		}
	}
}
