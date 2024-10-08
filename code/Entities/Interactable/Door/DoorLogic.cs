using Sandbox.Entity;
using Sandbox.GameSystems.Player;

namespace Entity.Interactable.Door
{
	public sealed class DoorLogic : BaseEntity, Component.INetworkListener
	{
		[Property] public GameObject Door { get; set; }
		[Property] public DoorMenu DoorMenu { get; set; }
		[Property, Sync] public bool IsUnlocked { get; set; } = true;
		[Property, Sync] public bool IsOpen { get; set; } = false;
		[Property, Sync] public int Price { get; set; } = 100;
		[Sync, HostSync] public string DoorOwner { get; private set; }
		[Sync, HostSync] public string DoorTitle { get; private set; }
		private Player _playerStats { get; set; }

		public override void InteractUse( SceneTraceResult tr, GameObject player )
		{
			// Dont interact with the door if it is locked
			if ( IsUnlocked == false ) { return; }

			// Open / Close door
			OpenCloseDoor( player );
		}
		public override void InteractSpecial( SceneTraceResult tr, GameObject player )
		{
			if ( DoorOwner == null )
			{
				PurchaseDoor( player, player.Components.Get<Player>() );
				return;
			}

			if ( player.Network.OwnerConnection.DisplayName == DoorOwner )
			{
				DoorMenu.OpenDoorMenu( this.Door, player );
			}
		}

		public override void InteractAttack1( SceneTraceResult tr, GameObject player )
		{
			// TODO The user should have a "keys" weapon select to do the following interactions to avoid input conflicts
			if ( player.Network.OwnerConnection.DisplayName == DoorOwner )
			{
				LockDoor();
			}
			else
			{
				KnockOnDoor();
			}
		}

		public override void InteractAttack2( SceneTraceResult tr, GameObject player )
		{
			// TODO The user should have a "keys" weapon select to do the following interactions to avoid input conflicts
			if ( player.Network.OwnerConnection.DisplayName == DoorOwner )
			{
				UnlockDoor();
			}
			else
			{
				KnockOnDoor();
			}
		}


		public void UpdateDoorOwner( Player playerStats = null )
		{
			_playerStats = playerStats;
		}

		[Broadcast]
		public void PurchaseDoor( GameObject player, Player playerStats )
		{
			if ( playerStats.UpdateBalance( -Price ) )
			{
				Sound.Play( "audio/notification.sound", Door.Transform.World.Position );
				playerStats.Doors.Add( Door );
				UpdateDoorOwner( playerStats );

				// Take the ownership of the door when buying it
				playerStats.TakeDoorOwnership( this.GameObject );
				DoorOwner = player.Network.OwnerConnection.DisplayName;
				Log.Info( $"new door owner is : {DoorOwner}" );
			}
		}

		[Broadcast]
		public void SellDoor( Player playerStats ) //This Function does no longer removes the Door in Player.Stats or checks if it's done
		{
			if ( playerStats == null ) { return; }

			IsUnlocked = true;
			playerStats.Doors.Remove( this.Door );
			playerStats.UpdateBalance( Price / 2 );
			DoorTitle = string.Empty;

			UpdateDoorOwner();

			// Drop the Ownership of the door when selling it
			playerStats.DropDoorOwnership( this.GameObject );
			DoorOwner = null;
		}

		[Broadcast]
		public void SetDoorTitle( string title )
		{
			DoorTitle = title;
		}

		[Broadcast]
		private void OpenCloseDoor( GameObject player )
		{
			if ( Door == null ) { return; }
			IsOpen = !IsOpen;

			var currentRotation = Door.Transform.Rotation;
			var rotationIncrement = Rotation.From( 0, 90, 0 );

			var directionToDoor = (Door.Transform.Position - player.Transform.Position).Normal;

			var forward = Door.Transform.Rotation.Forward;
			var dotProduct = Vector3.Dot( forward, directionToDoor );

			var shouldOpenForward = dotProduct > 0;

			Door.Transform.Rotation = IsOpen
					? (shouldOpenForward ? currentRotation * rotationIncrement : currentRotation * rotationIncrement.Inverse)
					: (shouldOpenForward ? currentRotation * rotationIncrement.Inverse : currentRotation * rotationIncrement);

			Sound.Play( "audio/door.sound", Door.Transform.World.Position );
		}

		[Broadcast]
		public void LockDoor()
		{
			IsUnlocked = false;
			_playerStats?.SendMessage( "Door has been locked." );
			Sound.Play( "audio/lock.sound", Door.Transform.World.Position );
		}

		[Broadcast]
		public void UnlockDoor()
		{
			IsUnlocked = true;
			_playerStats?.SendMessage( "Door has been unlocked." );
			Sound.Play( "audio/lock.sound", Door.Transform.World.Position );
		}

		[Broadcast]
		private void KnockOnDoor()
		{
			Sound.Play( "audio/knock.sound", Door.Transform.World.Position );
		}
	}
}
