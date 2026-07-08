using System.Linq;
using Godot;
using Godot.Collections;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents3D;

[Tool][GlobalClass]
public partial class ForceComponent3D : SuperconStateComponent3D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public ForceTypeEnum ForceType = ForceTypeEnum.GlobalDirection;

	/// <summary>
	/// The direction to which the force is applied.
	/// </summary>
	[Export] public Vector3 Direction
		{ get; set { field = value; this.UpdateConfigurationWarnings(); } }
		= Vector3.Up;

	[Export(PropertyHint.None, "suffix:m/s²")] public float Acceleration = 1f;

	[ExportGroup("Additional Options")]
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxSpeed = float.PositiveInfinity;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public Vector3 GlobalDirection => this.ForceType switch
		{
			ForceTypeEnum.LocalDirection => ((this.Character3D?.GlobalBasis ?? Basis.Identity) * this.Direction).Normalized(),
			ForceTypeEnum.Drag => this.Character3D?.Velocity.Normalized() * -1 ?? Vector3.Zero,
			ForceTypeEnum.GlobalDirection or _ => this.Direction.Normalized(),
		};

	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum ForceTypeEnum
	{
		GlobalDirection = 16,
		LocalDirection = 32,
		Drag = 48,
		// Expression = 64, // TODO
	}

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.ForceType):
				property["usage"] = (long) (PropertyUsageFlags.Default | PropertyUsageFlags.UpdateAllIfModified);
				break;
			case nameof(this.Direction):
				if (this.ForceType == ForceTypeEnum.Drag)
					property["usage"] = (long) PropertyUsageFlags.None;
				break;
		}
	}

	public override string[] _GetConfigurationWarnings()
		=> (base._GetConfigurationWarnings() ?? [])
			.AppendIf(this.ForceType != ForceTypeEnum.Drag && this.Direction == Vector3.Zero, $"The {nameof(this.Direction)} property should not be a zero vector.")
			.ToArray();

	protected override void _ActivityPhysicsProcessActive(double delta)
	{
		base._ActivityPhysicsProcessActive(delta);
		this.Character3D?.ApplyForceAndLimitSpeed(this.GlobalDirection * this.Acceleration * (float) delta, this.MaxSpeed);
	}
}
