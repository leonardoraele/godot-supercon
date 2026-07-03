using Godot;
using Godot.Collections;
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

	[Export] public Node? Context { get => field ??= this.Owner; set; }
	[Export(PropertyHint.Expression)] public string Expression = "";
	[Export] public FrequencyEnum Frequency = FrequencyEnum.OnProcess;

	[ExportGroup("Options")]
	[Export] public Variant Argument = new Variant();
	[Export] public Variant.Type ExpectedType = Variant.Type.Nil;

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
			if (field == null || Engine.IsEditorHint() && this.RunInEditor)
			{
				field = new();
				field.Parse(this.Expression, ["argument"]);
			}
			return field;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void ExecutedEventHandler(Variant result);

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
		Never,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(Argument):
				property["type"] = Variant.Type.Nil.As<long>();
				property["usage"] = PropertyUsageFlags.NilIsVariant
					.Union(PropertyUsageFlags.Default).As<long>();
				break;
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.Frequency == FrequencyEnum.OnTreeEnter || this.Frequency == FrequencyEnum.OnTreeEnterOrExit)
			this.Execute();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.Frequency == FrequencyEnum.OnTreeExit || this.Frequency == FrequencyEnum.OnTreeEnterOrExit)
			this.Execute();
	}

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.Frequency == FrequencyEnum.OnReady)
			this.Execute();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.Frequency == FrequencyEnum.OnProcess)
			this.Execute();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.Frequency == FrequencyEnum.OnPhysicsProcess)
			this.Execute();
	}

	// public override string[] _GetConfigurationWarnings()
	// 	=> base._PhysicsProcess(delta);

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public void Execute()
		=> this.Execute(this.Argument);

	public void Execute(Variant argumentOverride)
	{
		Variant result = this.Interpreter.Execute([argumentOverride], this.Context);
		if (this.Interpreter.HasExecuteFailed())
		{
			GD.PushWarning(this.Interpreter.GetErrorText());
			return;
		}
		if (this.ExpectedType != Variant.Type.Nil && !result.VariantType.IsConvertibleTo(this.ExpectedType))
		{
			GD.PushWarning($"Expression result type mismatch: expected {this.ExpectedType}, got {result.VariantType}");
			return;
		}
		this.EmitSignal(SignalName.Executed, result);
	}
}
