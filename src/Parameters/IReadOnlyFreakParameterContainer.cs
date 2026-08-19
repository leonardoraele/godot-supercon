using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

public interface IReadOnlyFreakParameterContainer
{
	//==================================================================================================================
	// ABSTRACTS
	//==================================================================================================================

	public bool HasParameter(string name);
	public Variant GetParameterValue(string name);
	public IFreakParameter GetParameter(string name);
	public IEnumerable<IFreakParameter> GetParameters();

	//==================================================================================================================
	// CONCRETES
	//==================================================================================================================

	public IEnumerable<string> GetParameterNames()
		=> this.GetParameters().Select(definition => definition.Name);
	public Godot.Collections.Dictionary<string, Variant> ToDictionary()
		=> this.GetParameterNames()
			.ToDictionary(name => name, this.GetParameterValue)
			.ToGodotDictionaryT();
}

public static class IReadOnlyFreakParameterContainerExtensions
{
	public static IReadOnlyFreakParameterContainer AsReadOnlyParameterContainer(this IReadOnlyFreakParameterContainer source) => source;
}
