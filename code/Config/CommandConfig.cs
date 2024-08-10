using System;

namespace Commands
{
  /// <summary>
  /// Interface for command configuration.
  /// </summary>
  public interface ICommandConfig
  {
    string Name { get; }
    string Description { get; }
    int PermissionLevel { get; }
    bool CommandFunction();
    bool CommandFunction( string[] args );
  }

  /// <summary>
  /// Command class implementing ICommandConfig.
  /// </summary>
  public class Command : ICommandConfig
  {
    public string Name { get; }
    public string Description { get; }
    public int PermissionLevel { get; } = 0;
    private readonly Func<bool> _commandFunction;
    private readonly Func<string[], bool> _commandFunctionWithArgs;

    public Command( string name, string description, int permissionLevel, Func<bool> commandFunction, Func<string[], bool> commandFunctionWithArgs = null )
    {
      Name = name.ToLowerInvariant() ?? throw new ArgumentNullException( nameof( name ) );
      Description = description ?? throw new ArgumentNullException( nameof( description ) );
      PermissionLevel = permissionLevel;
      _commandFunction = commandFunction ?? throw new ArgumentNullException( nameof( commandFunction ) );
      _commandFunctionWithArgs = commandFunctionWithArgs;
    }

    public bool CommandFunction() => _commandFunction();

    public bool CommandFunction( string[] args )
    {
      if ( _commandFunctionWithArgs != null )
      {
        return _commandFunctionWithArgs( args );
      }
      throw new InvalidOperationException( "This command does not support arguments." );
    }
  }

  /// <summary>
  /// Command configuration.
  /// </summary>
  public class CommandConfig
  {
    private readonly Dictionary<string, ICommandConfig> _commands = new();

    public IReadOnlyCollection<ICommandConfig> Commands => _commands.Values;

    public void RegisterCommand( ICommandConfig command )
    {
      if ( command == null )
        throw new ArgumentNullException( nameof( command ) );

      var commandNameLower = command.Name.ToLowerInvariant();
      if ( _commands.ContainsKey( commandNameLower ) )
        throw new InvalidOperationException( $"Command with name \"{commandNameLower}\" already exists." );

      _commands[commandNameLower] = command;
    }

    public void UnregisterCommand( string commandName )
    {
      commandName = commandName.ToLowerInvariant();
      if ( string.IsNullOrWhiteSpace( commandName ) )
        throw new ArgumentException( "Command name cannot be null or whitespace.", nameof( commandName ) );

      if ( !_commands.Remove( commandName ) )
        throw new KeyNotFoundException( $"Command with name \"{commandName}\" does not exist." );
    }

    public ICommandConfig GetCommand( string commandName )
    {
      // Lowercase the command name to make it case-insensitive.
      commandName = commandName.ToLowerInvariant();
      if ( string.IsNullOrWhiteSpace( commandName ) )
        throw new ArgumentException( "Command name cannot be null or whitespace.", nameof( commandName ) );

      if ( _commands.TryGetValue( commandName, out var command ) )
        return command;

      throw new KeyNotFoundException( $"Command with name {commandName} does not exist." );
    }

    public string[] GetCommandNames() => _commands.Keys.ToArray();

    public bool ExecuteCommand( string commandName, string[] args )
    {
      try
      {
        var command = GetCommand( commandName );
        Log.Info( $"Executing command \"{commandName}\"." );
        return command.CommandFunction( args );
      }
      catch ( Exception e )
      {
        Log.Error( $"Failed to execute command \"{commandName}\": {e.Message}" );
        return false;
      }
    }
  }
}

public static class ConfigManagerHelper
{
  private static readonly Guid ConfigManagerGuid = new Guid( "657ada82-4355-4f3b-b28a-e4858fe4d86b" );

  public static ConfigManager GetConfigManager( Scene scene )
  {
    try
    {
      var configManager = scene.Directory.FindComponentByGuid( ConfigManagerGuid ) as ConfigManager;
      if ( configManager is null )
      {
        Log.Error( "Config Manager not found" );
      }
      return configManager;
    }
    catch ( Exception e )
    {
      Log.Error( $"Failed to get Config Manager: {e.Message}" );
      return null;
    }
  }
}