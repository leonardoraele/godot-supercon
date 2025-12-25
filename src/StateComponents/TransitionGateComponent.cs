using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDictionary = Godot.Collections.Dictionary;
using Raele.GodotUtils.StateMachine;

namespace Raele.Supercon2D.StateComponents;

[Tool][GlobalClass]
public partial class TransitionGateComponent : SuperconStateComponent
{
	//------------------------------------------------------------------------------------------------------------------
	// STATICS
	//------------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	//------------------------------------------------------------------------------------------------------------------
	// EXPORTS
	//------------------------------------------------------------------------------------------------------------------

	[ExportGroup("Allowed Transitions")]
	[Export] public NodePath[] NextStateAllowlist = [];
	[Export] public NodePath[] PreviousStateAllowlist = [];

	[ExportGroup("Forbidden Transitions")]
	[Export] public NodePath[] NextStateForbidlist = [];
	[Export] public NodePath[] PreviousStateForbidlist = [];

	//------------------------------------------------------------------------------------------------------------------
	// FIELDS
	//------------------------------------------------------------------------------------------------------------------

	private IEnumerable<SuperconState> PreviousStateAllowlistResolved => this.ResolveList(this.PreviousStateAllowlist);
	private IEnumerable<SuperconState> PreviousStateForbidlistResolved => this.ResolveList(this.PreviousStateForbidlist);
	private IEnumerable<SuperconState> NextStateAllowlistResolved => this.ResolveList(this.NextStateAllowlist);
	private IEnumerable<SuperconState> NextStateForbidlistResolved => this.ResolveList(this.NextStateForbidlist);

	//------------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	//------------------------------------------------------------------------------------------------------------------


	//------------------------------------------------------------------------------------------------------------------
	// SIGNALS
	//------------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	//------------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	//------------------------------------------------------------------------------------------------------------------

	// public enum Type {
	// 	Value1,
	// }

	//------------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	//------------------------------------------------------------------------------------------------------------------

	public override void _SuperconEnter(StateMachine<SuperconState>.Transition transition)
	{
		base._SuperconEnter(transition);
		if (!this.ShouldProcess)
		{
			return;
		}
		if (
			!this.TestPreviousStateForbidList(transition.ExitState)
			|| !this.TestPreviousStateAllowlist(transition.ExitState)
		)
		{
			transition.Cancel();
		}
	}

	public override void _SuperconExit(StateMachine<SuperconState>.Transition transition)
	{
		base._SuperconExit(transition);
		if (!this.ShouldProcess)
		{
			return;
		}
		if (!this.TestNextStateForbidlist(transition.EnterState) || !this.TestNextStateAllowlist(transition.EnterState))
		{
			transition.Cancel();
		}
	}

	// public override string[] _GetConfigurationWarnings()
	// 	=> new List<string>()
	// 		.Concat(true ? ["This node is not configured correctly. Is any mandatory property empty?"] : [])
	// 		.ToArray();

	public override void _ValidateProperty(GodotDictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.PreviousStateAllowlist):
			case nameof(this.PreviousStateForbidlist):
			case nameof(this.NextStateAllowlist):
			case nameof(this.NextStateForbidlist):
				property["hint"] = (long) PropertyHint.ArrayType;
				property["hint_string"] = $"{Variant.Type.NodePath:D}/{PropertyHint.NodePathValidTypes:D}:{nameof(SuperconState)}";
				break;
		}
	}

	//------------------------------------------------------------------------------------------------------------------
	// METHODS
	//------------------------------------------------------------------------------------------------------------------

	private IEnumerable<SuperconState> ResolveList(IEnumerable<NodePath?> list)
		=> list.OfType<NodePath>()
			.Select(path => this.GetNodeOrNull<SuperconState>(path))
			.OfType<SuperconState>();

	/// <summary>
	/// Tests the forbidlist rule for previous state. Returns `true` if transition is permitted.
	/// </summary>
	private bool TestPreviousStateForbidList(SuperconState? previousState)
		=> previousState == null
			|| this.PreviousStateForbidlistResolved.All(forbidState => forbidState != previousState);

	/// <summary>
	/// Tests the allowlist rule for previous state. Returns `true` if transition is permitted.
	/// </summary>
	private bool TestPreviousStateAllowlist(SuperconState? previousState)
		=> previousState == null
			|| this.PreviousStateAllowlist.Length == 0
			|| this.PreviousStateAllowlistResolved.Contains(previousState);

	/// <summary>
	/// Tests the forbid list for next state. Returns `true` if transition is permitted.
	/// </summary>
	private bool TestNextStateForbidlist(SuperconState? nextState)
		=> nextState == null
			|| this.NextStateForbidlistResolved.All(forbidState => forbidState != nextState);

	/// <summary>
	/// Tests the allowlist for next state. Returns `true` if transition is permitted.
	/// </summary>
	private bool TestNextStateAllowlist(SuperconState? nextState)
		=> nextState == null
			|| this.NextStateAllowlist.Length == 0
			|| this.NextStateAllowlistResolved.Contains(nextState);
}
