using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;
using Raele.GodotUtils.Input;

namespace Raele.Supercon;

[Tool][GlobalClass]
public partial class TransitionComponent : SuperconStateComponent
{
	//==================================================================================================================
	// STATICS
	//==================================================================================================================

	// public static readonly string MyConstant = "";

	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public SuperconState? NextState;

	[ExportGroup("Test Boolean Parameter", "Boolean")]
	[Export(PropertyHint.GroupEnable)] public bool BooleanParameterEnabled = false;
	[Export] public StringName BooleanParameter = "";
	[ExportSubgroup("Options", "Boolean")]
	/// <summary>
	/// If true, the transition will be triggered when the parameter is false. If false, the transition will
	/// be triggered when the parameter is true.
	/// </summary>
	[Export] public bool BooleanNegateParameter = false;
	/// <summary>
	/// If true, the parameter will automatically be reset to false after the transition is triggered. This is
	/// useful for one-time triggers, such as a "jump" action.
	/// </summary>
	[Export] public bool BooleanParameterIsTrigger = false;

	[ExportGroup("Test Expression", "Expression")]
	[Export(PropertyHint.GroupEnable)] public bool ExpressionEnabled = false;
	[Export] public Node? ExpressionContext
		{ get => field ?? (this.ExpressionEnabled ? this.Owner : null); set; }
	[Export] public Godot.Collections.Dictionary<string, Variant> ExpressionVariables = [];
	[Export(PropertyHint.Expression)] public string Expression = "";

	[ExportGroup("Test Input")]
	[Export(PropertyHint.GroupEnable)] public bool InputEnabled = false;
	[Export(PropertyHint.InputName)] public string InputName = "";
	[Export] public InputTestEnum InputAction = InputTestEnum.InputIsJustPressed;

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	private Expression Interpreter
	{
		get
		{
			if (field == null || Engine.IsEditorHint())
			{
				field = new();
				field.Parse(this.Expression, [
					..this.Controller3D?.Parameters.GetParameters().Select(param => param.Name) ?? [],
					..this.ExpressionVariables.Keys,
				]);
			}
			return field;
		}
	}

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================



	//==================================================================================================================
	// SIGNALS
	//==================================================================================================================

	// [Signal] public delegate void EventHandler()

	//==================================================================================================================
	// INTERNAL TYPES
	//==================================================================================================================

	// public enum Type {
	// 	Value1,
	// }

	//==================================================================================================================
	// VIRTUALS & OVERRIDES
	//==================================================================================================================

	public override string[] _GetConfigurationWarnings()
		=> (base._GetConfigurationWarnings() ?? [])
			.AppendIf(this.NextState == null, $"Mandatory property {nameof(this.NextState)} is not set.")
			.ToArray();

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.BooleanParameter):
				string[] options = this.GetFirstAncestorOrDefault<FreakController3D>()
					?.Parameters
					.GetParameters()
					.Where(attr => attr.Type == Variant.Type.Bool)
					.Select(attr => attr.Name)
					.ToArray()
					?? [];
				property["hint"] = (long) PropertyHint.Enum;
				property["hint_string"] = string.Join(",", options);
				property["usage"] = (long) PropertyUsageFlags.Default
					| (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
		}
	}

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _ExitTree()
	// {
	// 	base._ExitTree();
	// }

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	// public override void _Process(double delta)
	// {
	// 	base._Process(delta);
	// }

	protected override void _ActivityPhysicsProcessActive(double delta)
	{
		base._ActivityPhysicsProcessActive(delta);
		if (Engine.IsEditorHint())
			return;
		if (
			(!this.BooleanParameterEnabled || this.TestBooleanParameter())
			&& (!this.ExpressionEnabled || this.TestExpression())
			&& (!this.InputEnabled || this.TestInput())
		)
			this.NextState?.QueueTransition();
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	private bool TestBooleanParameter()
	{
		if (string.IsNullOrEmpty(this.BooleanParameter))
			return false;
		if (
			this.Controller3D?.ParameterContainer.GetParameterValue(this.BooleanParameter).AsBool()
				== this.BooleanNegateParameter
		)
			return false;
		if (this.BooleanParameterIsTrigger)
			this.Controller3D?.ParameterContainer.SetParameterValue(
				this.BooleanParameter,
				this.BooleanNegateParameter
			);
		return true;
	}

	private bool TestExpression()
	{
		if (string.IsNullOrWhiteSpace(this.Expression))
			return false;
		Godot.Collections.Array argumnets = [
			..(this.Controller3D?.Parameters.GetParameters() ?? [])
				.Select(param => this.Controller3D!.Parameters.GetParameterValue(param.Name)),
			..this.ExpressionVariables.Values,
		];
		Variant result = this.Interpreter.Execute(argumnets, this.Character3D);
		if (this.Interpreter.HasExecuteFailed())
		{
			GD.PrintErr($"Expression execution failed: {this.Interpreter.GetErrorText()}");
			return false;
		}
		if (!result.IsConvertibleTo(Variant.Type.Bool))
		{
			GD.PrintErr($"Expression result is not a boolean: {result} (type {result.VariantType})");
			return false;
		}
		return result.AsBool();
	}

	private bool TestInput()
	{
		if (string.IsNullOrEmpty(this.InputName))
			return false;
		return this.InputAction.Test(this.InputName);
	}
}
