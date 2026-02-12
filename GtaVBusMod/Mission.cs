using System;
using System.Linq;
using GTA;
using GTA.Math;
using GtaVBusMod.Services;
using GtaVBusMod.Tools;

namespace GtaVBusMod
{
    /// <summary>
    /// Manages the lifecycle and logic of a bus mission, including passenger pickup,
    /// transportation, and delivery to destination.
    /// </summary>
    public class Mission
    {

        public Mission()
        {
            if (DebugMode)
            {
                _busLogger = new DevToolBusLogger();
            }
            else
            {
                _busLogger = new NullBusLogger();
            }
        }
        
        #region Fields

        private readonly PedestrianManager _pedestrianManager = new PedestrianManager();
        private XmlMissionDataService _missionData;
        private Blip _destinationBlip;
        private Vehicle _busVehicle;

        /// <summary>
        /// Keep this false to avoid getting flooded with UI logs
        /// </summary>
        private const bool DebugMode = false;

        private readonly IGtaVBusLogging _busLogger;
        
        #endregion

        #region Mission Lifecycle

        /// <summary>
        /// Prepares and starts a new mission by loading mission data, spawning entities,
        /// and initializing game state.
        /// </summary>
        /// <param name="missionName">Name of the mission to start</param>
        public void PrepareMission(string missionName)
        {
            _missionData = new XmlMissionDataService(missionName);

            // Clear any previous mission state
            _pedestrianManager.Clear();

            try
            {
                // Display mission description
                DisplayMissionDescription();

                // Create and setup pedestrians
                SetupPedestrians();

                // Create vehicle and destination
                SetupVehicle();
                SetupDestination();

                // Configure blips and relationships
                _pedestrianManager.SetBlipSprite(BlipSprite.Friend);
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"PREPARE_MISSION_ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks and updates the mission state each game tick.
        /// Handles passenger pickup, destination arrival, and failure conditions.
        /// </summary>
        /// <returns>True if mission is still active, false if completed or failed</returns>
        public bool Check()
        {
            var player = Game.Player.Character;

            // Make vehicle vulnerable once player enters it
            if (player.IsInVehicle(_busVehicle))
            {
                _busVehicle.IsInvincible = false;
            }

            // Check for failure conditions
            if (_pedestrianManager.IsAnyDead() || !_busVehicle.IsAlive)
            {
                HandleMissionFailure();
                return false;
            }

            // Manage route display based on player location
            UpdateRouteDisplay(player);

            // Handle passenger pickup
            if (ShouldPickupPassengers())
            {
                PickupPassengers();
                _busLogger.Log("Picked up passengers");
                return true;
            }

            // Handle destination arrival
            if (ShouldCompleteDelivery())
            {
                _busLogger.Log("Delivery Completed");
                CompleteDelivery();
            }

            return true;
        }

        /// <summary>
        /// Cancels the current mission (user-initiated).
        /// </summary>
        public void CancelMission()
        {
            GTA.UI.Screen.ShowSubtitle(Constants.MissionCanceledMessage, 8000);
            CleanupMission(applyMoneyPenalty: true);
        }

        #endregion

        #region Setup Methods

        /// <summary>
        /// Displays the mission description to the player.
        /// </summary>
        private void DisplayMissionDescription()
        {
            var description = _missionData.GetMissionDescription();
            var descriptionLines = description.Split('^').ToList();

            foreach (var line in descriptionLines)
            {
                GTA.UI.Screen.ShowSubtitle(line, Constants.DescriptionDisplayDuration);
                Script.Wait(Constants.DescriptionDisplayDuration);
            }
        }

        /// <summary>
        /// Creates and configures pedestrians for the mission.
        /// </summary>
        private void SetupPedestrians()
        {
            try
            {
                _pedestrianManager.CreatePedestrians(_missionData);
                _pedestrianManager.MakeInvincible();
                _pedestrianManager.AddBlips();
                _pedestrianManager.SetPlayerRelationship();
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"GENERATE_PED_ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates and configures the bus vehicle for the mission.
        /// </summary>
        private void SetupVehicle()
        {
            try
            {
                _busVehicle = World.CreateVehicle(
                    new Model(_missionData.GetVehicleHash(0)),
                    new Vector3(
                        _missionData.GetCoordinate("vehicle", 0, 'x'),
                        _missionData.GetCoordinate("vehicle", 0, 'y'),
                        _missionData.GetCoordinate("vehicle", 0, 'z')
                    ),
                    _missionData.GetCoordinate("vehicle", 0, 't')
                );

                _busVehicle.AddBlip();
                _busVehicle.AttachedBlip.Sprite = BlipSprite.Cab;
                _busVehicle.AttachedBlip.ShowRoute = true;
                
                // Prevent damage before player reaches vehicle
                _busVehicle.IsInvincible = true;
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"VEHICLE_ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the destination blip for the mission.
        /// </summary>
        private void SetupDestination()
        {
            try
            {
                _destinationBlip = World.CreateBlip(
                    new Vector3(
                        _missionData.GetCoordinate("destination", 0, 'x'),
                        _missionData.GetCoordinate("destination", 0, 'y'),
                        _missionData.GetCoordinate("destination", 0, 'z')
                    )
                );
                _destinationBlip.Color = BlipColor.Blue;
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"ADD_DESTINATION_ERROR: {ex.Message}");
            }
        }

        #endregion

        #region Mission State Updates

        /// <summary>
        /// Updates which route is displayed on the map based on current mission progress.
        /// </summary>
        /// <param name="player">The player character</param>
        private void UpdateRouteDisplay(Ped player)
        {
            // Show route to vehicle if player is not in it
            if (!player.IsInVehicle(_busVehicle))
            {
                _busVehicle.AttachedBlip.ShowRoute = true;
                _destinationBlip.ShowRoute = false;
            }
            else
            {
                _busVehicle.AttachedBlip.ShowRoute = false;
            }

            // Show route to first passenger if any are outside vehicle
            if (_pedestrianManager.IsAnyOutsideVehicle(_busVehicle))
            {
                if (_pedestrianManager.FirstPedestrian?.AttachedBlip != null)
                {
                    _pedestrianManager.FirstPedestrian.AttachedBlip.ShowRoute = true;
                }
                _destinationBlip.ShowRoute = false;
            }
            else
            {
                if (_pedestrianManager.FirstPedestrian?.AttachedBlip != null)
                {
                    _pedestrianManager.FirstPedestrian.AttachedBlip.ShowRoute = false;
                }
                _destinationBlip.ShowRoute = true;
            }
        }

        /// <summary>
        /// Determines if passengers should be picked up based on current conditions.
        /// </summary>
        /// <returns>True if conditions are met for pickup</returns>
        private bool ShouldPickupPassengers()
        {
            if (_pedestrianManager.FirstPedestrian == null)
                return false;

            return _busVehicle.Position.DistanceTo(_pedestrianManager.FirstPedestrian.Position) <= Constants.PickupDistance
                   && _busVehicle.IsStopped
                   && Game.IsControlPressed(Control.VehicleHorn)
                   && _busVehicle.Position.DistanceTo(_destinationBlip.Position) >= Constants.MinDistanceFromDestination;
        }

        /// <summary>
        /// Handles the passenger pickup sequence.
        /// </summary>
        private void PickupPassengers()
        {
            _pedestrianManager.RemoveInvincibility();
            _pedestrianManager.EnterVehicle(_busVehicle);
            GTA.UI.Screen.ShowSubtitle(Constants.PassengerPickupMessage);
        }

        /// <summary>
        /// Determines if the delivery should be completed based on current conditions.
        /// </summary>
        /// <returns>True if conditions are met for delivery completion</returns>
        private bool ShouldCompleteDelivery()
        {
            return _busVehicle.Position.DistanceTo(_destinationBlip.Position) <= Constants.DestinationDistance
                   && _busVehicle.IsStopped
                   && Game.IsControlPressed(Control.VehicleHorn);
        }

        /// <summary>
        /// Handles the successful delivery of passengers to the destination.
        /// </summary>
        private void CompleteDelivery()
        {
            // Exit passengers from vehicle
            _pedestrianManager.ScatterAround(_busVehicle.Position, Constants.PedestrianExitRadius);
            GTA.UI.Screen.ShowSubtitle(Constants.MissionCompleteMessage, Constants.CompletionMessageDuration);
            CleanupMission(applyMoneyPenalty: false);
        }

        #endregion

        #region Mission Failure and Cleanup

        /// <summary>
        /// Handles mission failure due to death of passenger or vehicle destruction.
        /// </summary>
        private void HandleMissionFailure()
        {
            GTA.UI.Screen.ShowSubtitle(Constants.MissionFailedMessage, Constants.FailureMessageDuration);
            Script.Wait(Constants.FailureMessageDuration);
            GTA.UI.Screen.ShowSubtitle(Constants.MissionFailedInsultMessage, Constants.FailureMessageDuration);
            CleanupMission(applyMoneyPenalty: true);
        }

        /// <summary>
        /// Cleans up mission entities and applies penalties if applicable.
        /// </summary>
        /// <param name="applyMoneyPenalty">Whether to apply money penalty for mission failure</param>
        private void CleanupMission(bool applyMoneyPenalty)
        {
            _pedestrianManager.DeletePedestriansBlip();
            _pedestrianManager.ScatterAround(_busVehicle.Position, 3f);
            _pedestrianManager.MarkAsNoLongerNeeded();

            if (applyMoneyPenalty)
            {
                var penalty = _missionData.GetMoney();
                Game.Player.Character.Money -= penalty;
            }

            _destinationBlip.Delete();
            _destinationBlip.ShowRoute = false;
            
            _busVehicle.AttachedBlip?.Delete();
            _busVehicle.MarkAsNoLongerNeeded();
        }

        #endregion
    }
}
