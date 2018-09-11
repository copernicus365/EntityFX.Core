
namespace EntityFX.Core
{
	/// <summary>
	/// Copies the System.Data.ParameterDirection enum so 
	/// SqlParameter can be used without dependency on System.Data.
	/// </summary>
	public enum ParamDirection
	{
		Input = 1,
		InputOutput = 3,
		Output = 2,
		ReturnValue = 6
	}
}
