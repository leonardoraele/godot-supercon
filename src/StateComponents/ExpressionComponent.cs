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
		OnTreeEnter,
		OnTreeExit,
		OnTreeEnterOrExit,
		OnReady,
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
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.AutoExecute == FrequencyEnum.OnTreeEnter || this.AutoExecute == FrequencyEnum.OnTreeEnterOrExit)
			this.Execute();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.AutoExecute == FrequencyEnum.OnTreeExit || this.AutoExecute == FrequencyEnum.OnTreeEnterOrExit)
			this.Execute();
	}

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.AutoExecute == FrequencyEnum.OnReady)
			this.Execute();
	}

	protected override void _ActivityProcess(double delta)
	{
		base._ActivityProcess(delta);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.AutoExecute == FrequencyEnum.OnProcess)
			this.Execute();
	}

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
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
		if (result.IsTruthy())
			this.EmitSignal(SignalName.TruthyResult);
		else
			this.EmitSignal(SignalName.FalsyResult);
		this.EmitSignal(SignalName.Executed, result);
	}
}
