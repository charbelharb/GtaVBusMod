using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using GTA;
using GTA.Math;

namespace GtaVBusMod
{
    public class Mission
    {
        private readonly XmlDocument _xml = new XmlDocument();

        public Mission()
        {
            try
            {
                var path = Directory.GetCurrentDirectory() + @"\scripts\bus_mod_missions.xml";
                using (var fS = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    _xml.Load(fS);
                    fS.Close();
                }
            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("Failed to load missions.xml");
            }
        }
        
        // Put all passengers here
        private readonly List<Ped> _ped = new List<Ped>();
        private Blip _destination;
        private string _mission = string.Empty;
        private Vehicle _coach;

        // Relationship group
        private RelationshipGroup _player = 0;

        // Same as above - Used for pedestrians
        private RelationshipGroup _magic = 0;

        public void PrepareMission(string mission)
        {
            _mission = mission;
            if (_ped.Count > 1)
            {
                // Clear the ped list from previous version
                // When using ped.clear in _check() scripts stop working and got stuck in an infinite loop
                _ped.Clear(); 
            }

            try
            {
                // Echo description
                var missionDescription = GetMissionDescription(mission);
                var descriptionSubtitles = missionDescription.Split('^').ToList();
                foreach (var t in descriptionSubtitles)
                {
                    GTA.UI.Screen.ShowSubtitle(t, 4000);
                    Script.Wait(4000);
                }
                
                try
                {
                    generate_ped(GetPedNumber());
                    MakePedestriansInvincible();
                }
                catch (Exception)
                {
                    GTA.UI.Screen.ShowSubtitle("GENERATE_PED_ERROR");
                }

                AddPedestriansBlips();
                AddDestinationBlip();
                AddPedBlipSprite(BlipSprite.Friend);

                try
                {
                    _coach = World.CreateVehicle(new Model(GetVehicleHash("vehicle", 0)),
                        new Vector3(GetCoordinate("vehicle", 0, 'x'), GetCoordinate("vehicle", 0, 'y'), GetCoordinate("vehicle", 0, 'z')),
                        GetCoordinate("vehicle", 0, 't'));
                    _coach.AddBlip();
                    _coach.AttachedBlip.Sprite = BlipSprite.Cab;
                    _coach.AttachedBlip.ShowRoute = true;
                    // To prevent damage and mission auto-cancel before arriving 
                    _coach.IsInvincible = true; 
                }
                catch (Exception)
                {
                    GTA.UI.Screen.ShowSubtitle("VEHICLE_ERROR");
                }
            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("PREPARE_MISSION_ERROR");
            }
            AddRelationship();

        }
        
        public bool Check()
        {
            var player = Game.Player.Character;

            // Condition to prevent destruction of vehicle before player uses it
            if (player.IsInVehicle(_coach))
            {
                _coach.IsInvincible = false;
            }
            
            // Auto cancel if vehicle is destroyed or some passenger dies
            if (IsAPassengerDead() || !_coach.IsAlive)
            {
                CancelMissionDeadPlayerOrPedestrian();
                return false;
            }

            // Show route to destination on map

            if (!player.IsInVehicle(_coach))
            {
                _coach.AttachedBlip.ShowRoute = true;
                _destination.ShowRoute = false;
            }
            else
            {
                _coach.AttachedBlip.ShowRoute = false;
            }
            
            if (IsAPassengerOutsideVehicle())
            {
                _ped[0].AttachedBlip.ShowRoute = true;
                _destination.ShowRoute = false;
            }
            else
            {
                _ped[0].AttachedBlip.ShowRoute = false;
                _destination.ShowRoute = true;
            }

            // Driver arrives to pick up pass
            if (_coach.Position.DistanceTo(_ped[0].Position) <= 30.0 && _coach.IsStopped &&
                Game.IsControlPressed(Control.VehicleHorn) && _coach.Position.DistanceTo(_destination.Position) >= 50.0)
            {
                RemovePedestriansInvincible();
                foreach (var t in _ped)
                {
                    t.SetIntoVehicle(_coach, VehicleSeat.Any);
                }
                GTA.UI.Screen.ShowSubtitle("Come on, get in!");
                return true;
            }

            // Driver arrives to destination
            if (_coach.Position.DistanceTo(_destination.Position) <= 30.0 && _coach.IsStopped &&
                Game.IsControlPressed(Control.VehicleHorn))
            {
                // Driver arrives at destination
                foreach (var t in _ped)
                {
                    t.Position = _coach.Position.Around(5);
                }

                GTA.UI.Screen.ShowSubtitle("Ok we're here. Thanks for using Dashound Bus Center.", 4000);

                _coach.MarkAsNoLongerNeeded();
                _destination.RemoveNumberLabel();
                RemovePedestriansBlips();
                _coach.AttachedBlip.RemoveNumberLabel();
                _destination.ShowRoute = false;
                MarkPedestriansAsNoLongerNeeded();
            }
            return true;
        }
        
        // Reading the XML and creating our ped
        private void generate_ped(int count)
        {
            try
            {
                for (var i = 0; i < count; i++)
                {
                    _ped.Add(World.CreatePed(
                        new Model(GetHash("ped", i)),
                        new GTA.Math.Vector3(GetCoordinate("ped", i, 'x'),
                            GetCoordinate("ped", i, 'y'),
                            GetCoordinate("ped", i, 'z')), 
                        GetCoordinate("ped", i, 't')));
                }
            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("GENERATE_PED_ERROR");
            }
        }
        
        // Check if a passenger is dead
        private bool IsAPassengerDead()
        {
            return _ped.Any(t => !t.IsAlive);
        }
        
        public void CancelMission()
        {
            GTA.UI.Screen.ShowSubtitle("MISSION CANCELED ALL YOU HAD TO DO WAS TAKE THEM TO THE DAMN DESTINATION!", 8000);
            RemovePedestriansBlip();
            CancelMissionInternal();
        }
        
        private void CancelMissionDeadPlayerOrPedestrian()
        {
            GTA.UI.Screen.ShowSubtitle("ALL YOU HAD TO DO WAS TAKE THEM TO THE DAMN DESTINATION!", 3500);
            Script.Wait(3500);
            GTA.UI.Screen.ShowSubtitle("YOU'RE USELESS AS A CORPSE'S DICK!", 3500);
            CancelMissionInternal();
        }

        private void CancelMissionInternal()
        {
            RemovePedestriansBlip();
            foreach (var t in _ped)
            {
                t.Position.Around(3);
            }
            Game.Player.Character.Money -= GetMoney();
            _destination.RemoveNumberLabel();
            _coach.AttachedBlip.RemoveNumberLabel();
            _coach.MarkAsNoLongerNeeded();
            MarkPedestriansAsNoLongerNeeded();
            _destination.ShowRoute = false;
        }
        
        private void RemovePedestriansBlip()
        {
            foreach (var t in _ped)
            {
                t.AttachedBlip.RemoveNumberLabel();
            }
        }
        
        private int GetMoney()
        {
            try
            {
                var nList = _xml.SelectNodes("/missions/element[name='" + _mission + "']/money");
                return int.TryParse(nList?[0].InnerText , out var i) ? i : -1;

            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("GET_MONEY_ERROR");
                return -1;
            }
        }
        
        // Bye
        private void MarkPedestriansAsNoLongerNeeded()
        {
            foreach (var t in _ped)
            {
                t.MarkAsNoLongerNeeded();
            }
        }
        
        private bool IsAPassengerOutsideVehicle()
        {
            return _ped.Any(t => !t.IsInVehicle(_coach));
        }
        
        // Remove god mod of passengers
        private void RemovePedestriansInvincible()
        {
            foreach (var t in _ped)
            {
                t.IsInvincible = false;
            }
        }
        
        private void RemovePedestriansBlips()
        {
            foreach (var t in _ped)
            {
                t.AttachedBlip.RemoveNumberLabel();
            }
        }
        
        /// <summary>
        /// Get mission description from XML
        /// </summary>
        /// <param name="mission"></param>
        /// <returns></returns>
        private string GetMissionDescription(string mission)
        {
            try
            {
                var desc = _xml.SelectSingleNode("/missions/element[name='" + mission + "']/description");
                var descStr = desc?.InnerText;
                return descStr;
            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("GET_DESC_ERROR");
            }
            return string.Empty;
        }
        
        /// <summary>
        /// Get hash from XML
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private int GetHash(string element, int index)
        {
            try
            {
                var nList = _xml.SelectNodes("/missions/element[name='" + _mission + "']/" + element + "/hash");
                return int.TryParse(nList?[index].InnerText, out var n) ? n : -1;
            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("GET_HASH_ERROR");
            }
            return -1;
        }
        
        /// <summary>
        /// Get coordinate from XML
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        private float GetCoordinate(string element, int index, char coord)
        {
            try
            {
                var nList = _xml.SelectNodes("/missions/element[name='" + _mission + "']/" + element + "/position/" + coord);
                return float.TryParse(nList?[index].InnerText, out var x) ? x : 100000;;
            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("GET_CO_ERROR" + element + " " + index + " " + coord + " ");
            }
            return 100000;
        }
        
        /// <summary>
        /// Get number of pedestrians from XML
        /// </summary>
        /// <returns>Number<see cref="int"/>of pedestrians</returns>
        private int GetPedNumber()
        {
            try
            {
                var nList = _xml.SelectNodes("/missions/element[name='" + _mission + "']/ped");
                return nList?.Count ?? -1;
            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("GET_PED_NUMBER_ERROR");
                return -1;
            }
        }
        
        /// <summary>
        /// This is temporary to avoid pedestrians dying before player reaches them 
        /// </summary>
        private void MakePedestriansInvincible()
        {
            foreach (var t in _ped)
            {
                t.IsInvincible = true;
            }
        }
        
        /// <summary>
        /// Add blips for pedestrians
        /// </summary>
        private void AddPedestriansBlips()
        {
            foreach (var t in _ped)
            {
                t.AddBlip();
            }
        }
        
        /// <summary>
        /// Adding relationship to avoid passenger freaking out
        /// </summary>
        private void AddRelationship()
        {
            var playerGroup = Game.Player.Character.RelationshipGroup;
            foreach (var p in _ped)
            {
                p.RelationshipGroup = playerGroup;
            }
            playerGroup.SetRelationshipBetweenGroups(playerGroup, Relationship.Respect);
        }
        
        private void AddDestinationBlip()
        {
            try
            {
                _destination = World.CreateBlip(new Vector3(GetCoordinate("destination", 0, 'x'),
                    GetCoordinate("destination", 0, 'y'),
                    GetCoordinate("destination", 0, 'z')));
                _destination.Color = BlipColor.Blue;
            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("ADD_DESTINATION_ERROR");
            }
        }
        
        private void AddPedBlipSprite(in BlipSprite sprite)
        {
            foreach (var t in _ped)
            {
                t.AttachedBlip.Sprite = sprite;
            }
        }
        
        private string GetVehicleHash(string element, int index)
        {
            try
            {
                var nList = _xml.SelectNodes(@"/missions/element[name='" + _mission + "']/" + element + "/hash");
                return nList?[index].InnerText ?? "s";
            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("GET_HASH_ERROR");
            }
            return "s";
        }
    }
}