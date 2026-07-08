using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents;

[Tool][GlobalClass]
public partial class ExpressionComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Node? Context { get => field ??= this; set; }
	[Export] public Godot.Collections.Dictionary Variables = [];
	[Export(PropertyHint.Expression)] public string Expression
		{ get; set { field = value; this.Interpreter = null!; } }
		= "";
	[Export] public FrequencyEnum AutoExecute = FrequencyEnum.OnProcess;

	[ExportGroup("Value Mapping")]
	[Export] public bool Absolute = false;
	[ExportSubgroup("Wrap", "Wrap")]
	[Export(PropertyHint.GroupEnable)] public bool WrapEnabled = false;
	[Export] public float WrapMin = 0;
	[Export] public float WrapMax = 1;
	[ExportSubgroup("Remap", "Remap")]
	[Export(PropertyHint.GroupEnable)] public bool RemapEnabled = false;
	[Export] public float RemapFromStart = 0;
	[Export] public float RemapFromEnd = 1;
	[Export] public float RemapToStart = 0;
	[Export] public float RemapToEnd = 1;

	[ExportGroup("Set Property", "Set")]
	[Export(PropertyHint.GroupEnable)] public bool SetPropertyEnabled = false;
	[Export] public Node? SetTarget
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	[Export] public string SetProperty = "";

	[ExportGroup("Additional Options")]
	[Export] public Variant.Type ExpectedResultType = Variant.Type.Nil;
	[Export] public Godot.Collections.Dictionary Parameters = [];

	[ExportSubgroup("Debug")]
	[Export] public bool RunInEditor = false;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------



	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private Expression Interpreter
	{
		get
		{
			if (field == null)
			{
				field = new();
				string[] parameters = this.Variables.Keys.Concat(this.Parameters.Keys)
					.Select(key => key.AsString())
					.ToArray();
				field.Parse(this.Expression, parameters);
			}
			return field;
		}
		set;
	}

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void ExecutedEventHandler(Variant result);
	[Signal] public delegate void TruthyResultEventHandler();
	[Signal] public delegate void FalsyResultEventHandler();

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum FrequencyEnum {
		OnProcess,
		OnPhysicsProcess,
		OnActivityStarted,
		OnActivityFinished,
		Never,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.Variables):
				property["type"] = Variant.Type.Dictionary.As<long>();
				property["hint"] = PropertyHint.DictionaryType.As<long>();
				property["hint_string"] = $"String;Nil";
				break;
			case nameof(this.Parameters):
				property["type"] = Variant.Type.Dictionary.As<long>();
				property["hint"] = PropertyHint.DictionaryType.As<long>();
				property["hint_string"] = $"String;{Variant.Type.Int:D}/{PropertyHint.Enum:D}:{Enum.GetNames<Variant.Type>().Join(",")}";
				break;
			case nameof(this.SetProperty):
				if (this.SetTarget == null)
					return;
				property["hint"] = PropertyHint.EnumSuggestion.As<long>();
				property["hint_string"] = this.SetTarget?.GetPropertyList()
					.Where(prop => !prop["usage"].AsUInt64().HasAnyBitSet(
						(ulong) PropertyUsageFlags.Group
						| (ulong) PropertyUsageFlags.Subgroup
						| (ulong) PropertyUsageFlags.Category
					))
					.Select(prop => prop["name"].AsString())
					.JoinIntoString(",")
					?? "";
				break;
		}
	}

	protected override void _ActivityProcessActive(double delta)
	{
		base._ActivityProcessActive(delta);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.AutoExecute == FrequencyEnum.OnProcess)
			this.Execute();
	}

	protected override void _ActivityPhysicsProcessActive(double delta)
	{
		base._ActivityPhysicsProcessActive(delta);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.AutoExecute == FrequencyEnum.OnPhysicsProcess)
			this.Execute();
	}

	protected override void _ActivityStarted(string mode, Variant argument)
	{
		base._ActivityStarted(mode, argument);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.AutoExecute == FrequencyEnum.OnActivityStarted)
			this.Execute();
	}

	protected override void _ActivityFinished(string reason, Variant details)
	{
		base._ActivityFinished(reason, details);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.AutoExecute == FrequencyEnum.OnActivityFinished)
			this.Execute();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public void Execute()
		=> this.Execute([]);

	public void Execute(Godot.Collections.Dictionary @params)
	{
		IEnumerable<Variant> paramValues = this.Parameters.Select(pair =>
		{
			if (!@params.ContainsKey(pair.Key))
				return new Variant();
			Variant value = @params[pair.Key];
			if (!value.VariantType.IsConvertibleTo(pair.Value.As<Variant.Type>()))
				return new Variant();
			return @params[pair.Key];
		});
		Godot.Collections.Array arguments = this.Variables.Values.Concat(paramValues).ToGodotArray();
		Variant result = this.Interpreter.Execute(arguments, this.Context);
		if (this.Interpreter.HasExecuteFailed())
		{
			GD.PushWarning($"Failed to execute expression. Cause: Execution errored. Error: {this.Interpreter.GetErrorText()}");
			return;
		}
		if (this.ExpectedResultType != Variant.Type.Nil && !result.VariantType.IsConvertibleTo(this.ExpectedResultType))
		{
			GD.PushWarning($"Failed to execute expression. Cause: Expression result type mismatch. Expected type: {this.ExpectedResultType}. Result: {result} (type '{result.VariantType}').");
			return;
		}
		result = this.ApplyValueMapping(result);
		this.SetTarget?.SetIndexed(this.SetProperty, result);
		if (result.IsTruthy())
			this.EmitSignal(SignalName.TruthyResult);
		else
			this.EmitSignal(SignalName.FalsyResult);
		this.EmitSignal(SignalName.Executed, result);
	}

	private Variant ApplyValueMapping(Variant value)
	{
		switch (value.VariantType)
		{
			case Variant.Type.Int:
				if (this.Absolute)
					value = Math.Abs(value.AsInt64());
				break;
			case Variant.Type.Float:
				if (this.Absolute)
					value = Math.Abs(value.AsDouble());
				if (this.WrapEnabled)
					value = Mathf.Wrap(value.AsDouble(), this.WrapMin, this.WrapMax);
				if (this.RemapEnabled)
					value = Mathf.Remap(value.AsDouble(), this.RemapFromStart, this.RemapFromEnd, this.RemapToStart, this.RemapToEnd);
				break;
		}
		return value;
	}
}
