using GameSystems.Player;

namespace GameSystems.Config
{
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
		void CommandFunction( GameObject player, Scene scene, string[] args );
	}
}