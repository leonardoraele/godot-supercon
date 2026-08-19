using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.Supercon;

[Tool][GlobalClass]
public partial class FreakParameterProfile : Resource, IReadOnlyFreakParameterContainer
{
	//------------------------------------------------------------------------------------------------------------------
	// STATICS
	//------------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	//------------------------------------------------------------------------------------------------------------------
	// EXPORTS
	//------------------------------------------------------------------------------------------------------------------

	[Export(PropertyHint.ResourceType, $"{nameof(StandardFreakParameter)},{nameof(ComputedFreakParameter)}")]
	public Resource[] CustomParameters = [];

	//------------------------------------------------------------------------------------------------------------------
	// FIELDS
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	//------------------------------------------------------------------------------------------------------------------

	public Dictionary<string, IFreakParameter> ParameterDict
	{
		get
		{
			if (field != null)
				return field;
			Dictionary<string, IFreakParameter> result = this.CustomParameters.OfType<IFreakParameter>()
				.ToDictionary(def => def.Name);
			if (!Engine.IsEditorHint())
				field = result;
			return result;
		}
	}

	//------------------------------------------------------------------------------------------------------------------
	// SIGNALS
	//------------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	//------------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// METHODS
	//------------------------------------------------------------------------------------------------------------------

	public IEnumerable<IFreakParameter> GetParameters()
		=> this.ParameterDict.Values;

	public bool HasParameter(string name)
		=> this.ParameterDict.ContainsKey(name);

	public Variant GetParameterValue(string name)
		=> this.GetParameter(name).RunGetter(this, new Variant());

	public IFreakParameter GetParameter(string name)
		=> this.ParameterDict.TryGetValue(name, out IFreakParameter? definition)
			? definition
			: throw new KeyNotFoundException($"Failed to get definition of attribute \"{name}\" in attribute profile \"{this.ResourcePath}\". Cause: Attribute does not exist in the profile.");
}
