using GameSystems.Player;

namespace GameSystems.Config
{
	public enum IArgumentType
	{
		Integer,
		String,
		Player
	}
	public interface IArgument
	{
		string Name { get; }
		string Description { get; }
		IArgumentType Type { get; }
	}
	/// <summary>
	/// Interface for command configuration.
	/// </summary>
	public interface ICommandConfig
	{
		string Name { get; }
		string Description { get; }
		PermissionLevel PermissionLevel { get; }

		bool ClientOnly { get; }

		[Broadcast]
		void CommandFunction( GameObject player, Scene scene, IArgument[] args );
	}
}