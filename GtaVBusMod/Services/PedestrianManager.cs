using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;

namespace GtaVBusMod.Services
{
    /// <summary>
    /// Manages pedestrian entities for bus missions, including creation, blip management, 
    /// and relationship handling.
    /// </summary>
    public class PedestrianManager
    {
        private readonly List<Ped> _pedestrians = new List<Ped>();

        #region Properties

        /// <summary>
        /// Gets the first pedestrian in the list (typically used for route display).
        /// </summary>
        public Ped FirstPedestrian => _pedestrians.Count > 0 ? _pedestrians[0] : null;

        /// <summary>
        /// Gets the count of pedestrians being managed.
        /// </summary>
        public int Count => _pedestrians.Count;

        #endregion

        #region Pedestrian Creation

        /// <summary>
        /// Creates pedestrians based on data from the mission XML service.
        /// </summary>
        /// <param name="dataService">XML data service containing pedestrian information</param>
        public void CreatePedestrians(XmlMissionDataService dataService)
        {
            Clear();
            
            var pedestrianCount = dataService.GetPedestrianCount();
            
            for (var i = 0; i < pedestrianCount; i++)
            {
                var ped = World.CreatePed(
                    new Model(dataService.GetPedestrianHash(i)),
                    new Vector3(
                        dataService.GetCoordinate("ped", i, 'x'),
                        dataService.GetCoordinate("ped", i, 'y'),
                        dataService.GetCoordinate("ped", i, 'z')
                    ),
                    dataService.GetCoordinate("ped", i, 't')
                );

                _pedestrians.Add(ped);
            }
        }

        #endregion

        #region Blip Management

        /// <summary>
        /// Adds blips to all pedestrians.
        /// </summary>
        public void AddBlips()
        {
            foreach (var ped in _pedestrians)
            {
                ped.AddBlip();
            }
        }

        /// <summary>
        /// Sets the sprite for all pedestrian blips.
        /// </summary>
        /// <param name="sprite">The blip sprite to use</param>
        public void SetBlipSprite(BlipSprite sprite)
        {
            foreach (var ped in _pedestrians)
            {
                if (ped.AttachedBlip != null)
                {
                    ped.AttachedBlip.Sprite = sprite;
                }
            }
        }

        /// <summary>
        /// Removes number labels from all pedestrian blips.
        /// </summary>
        public void RemoveBlipLabels()
        {
            foreach (var ped in _pedestrians)
            {
                ped.AttachedBlip?.RemoveNumberLabel();
            }
        }

        #endregion

        #region Invincibility Management

        /// <summary>
        /// Makes all pedestrians invincible (used to prevent premature deaths).
        /// </summary>
        public void MakeInvincible()
        {
            foreach (var ped in _pedestrians)
            {
                ped.IsInvincible = true;
            }
        }

        /// <summary>
        /// Removes invincibility from all pedestrians.
        /// </summary>
        public void RemoveInvincibility()
        {
            foreach (var ped in _pedestrians)
            {
                ped.IsInvincible = false;
            }
        }

        #endregion

        #region Vehicle Operations

        /// <summary>
        /// Places all pedestrians into the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to place pedestrians into</param>
        public void EnterVehicle(Vehicle vehicle)
        {
            foreach (var ped in _pedestrians)
            {
                ped.SetIntoVehicle(vehicle, VehicleSeat.Any);
            }
        }

        /// <summary>
        /// Checks if any pedestrian is outside the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to check against</param>
        /// <returns>True if any pedestrian is not in the vehicle</returns>
        public bool IsAnyOutsideVehicle(Vehicle vehicle)
        {
            return _pedestrians.Any(ped => !ped.IsInVehicle(vehicle));
        }

        #endregion

        #region State Checks

        /// <summary>
        /// Checks if any pedestrian is dead.
        /// </summary>
        /// <returns>True if any pedestrian is not alive</returns>
        public bool IsAnyDead()
        {
            return _pedestrians.Any(ped => !ped.IsAlive);
        }

        #endregion

        #region Relationship Management

        /// <summary>
        /// Sets all pedestrians to the player's relationship group to prevent panic behavior.
        /// </summary>
        public void SetPlayerRelationship()
        {
            var playerGroup = Game.Player.Character.RelationshipGroup;
            
            foreach (var ped in _pedestrians)
            {
                ped.RelationshipGroup = playerGroup;
            }
            
            playerGroup.SetRelationshipBetweenGroups(playerGroup, Relationship.Respect);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Positions pedestrians around a specified location (used when dropping off).
        /// </summary>
        /// <param name="position">Center position</param>
        /// <param name="radius">Radius around the position</param>
        public void ScatterAround(Vector3 position, float radius)
        {
            foreach (var ped in _pedestrians)
            {
                ped.Position = position.Around(radius);
            }
        }

        /// <summary>
        /// Marks all pedestrians as no longer needed by the game engine.
        /// </summary>
        public void MarkAsNoLongerNeeded()
        {
            foreach (var ped in _pedestrians)
            {
                ped.MarkAsNoLongerNeeded();
            }
        }

        /// <summary>
        /// Clears the pedestrian list.
        /// </summary>
        public void Clear()
        {
            _pedestrians.Clear();
        }

        #endregion
    }
}
