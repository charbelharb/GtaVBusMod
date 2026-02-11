using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using GTA;

namespace GtaVBusMod
{
    public class Mission
    {
        //put all passengers here
        private List<Ped> _ped = new List<Ped>();
        private Blip destination;
        private string mission = "";

        private Vehicle coach;

        //rel group
        private int _player = 0;

        //same as above
        private int _magic = 0;

        public void Check()
        {
            var player = Game.Player.Character;
            
            //condition to prevent destruction of vehicle before player uses it
            if (player.IsInVehicle(coach))
            {
                coach.IsInvincible = false;
            }
            
            //auto cancel if vehicle is destroyed or some passenger dies
            if (IsAPassengerDead() || !coach.IsAlive)
            {
                //bus_mod._lock = false;
                CancelMissionDeadPedestrian();
            }

            //show route to destination on map
            if (IsAPassengerOutsideVehicle())
            {
                destination.ShowRoute = false;
            }

            //driver arrives to pick up pass
            if (coach.Position.DistanceTo(_ped[0].Position) <= 30.0 && coach.IsStopped &&
                Game.IsControlPressed(Control.VehicleHorn))
            {
                RemovePedestriansInvincible();
                foreach (var t in _ped)
                {
                    t.SetIntoVehicle(coach, VehicleSeat.Any);
                }
                GTA.UI.Screen.ShowSubtitle("Come on, get in!");
            }
            
            if (!(coach.Position.DistanceTo(destination.Position) <= 30.0) || !coach.IsStopped ||
                !Game.IsControlPressed(Control.VehicleHorn)) return;
            
            // Driver arrives at destination
            foreach (var t in _ped)
            {
                t.Position = coach.Position.Around(5);
            }

            GTA.UI.Screen.ShowSubtitle("Ok we're here. Thanks for using Dashound Bus Center.", 4000);

            coach.MarkAsNoLongerNeeded();
            destination.RemoveNumberLabel();
            RemovePedestriansBlips();
            coach.AttachedBlip.RemoveNumberLabel();
            destination.ShowRoute = false;

            MarkPedestriansAsNoLongerNeeded();
            //bus_mod._lock = false;
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
        
        private void CancelMissionDeadPedestrian()
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
            Game.Player.Character.Money -= get_money();
            destination.RemoveNumberLabel();
            coach.AttachedBlip.RemoveNumberLabel();
            coach.MarkAsNoLongerNeeded();
            MarkPedestriansAsNoLongerNeeded();
            destination.ShowRoute = false;
        }
        
        private void RemovePedestriansBlip()
        {
            foreach (var t in _ped)
            {
                t.AttachedBlip.RemoveNumberLabel();
            }
        }
        
        private int get_money()
        {
            try
            {
                var path = Directory.GetCurrentDirectory() + @"\scripts\bus_mod_missions.xml";
                var fS = new FileStream(path, FileMode.Open, FileAccess.Read);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(fS);
                var nList = xmlDoc.SelectNodes("/missions/element[name='" + mission + "']/money");
                fS.Close();
                Debug.Assert(nList != null, nameof(nList) + " != null");
                return int.Parse(nList[0].InnerText);

            }
            catch (Exception)
            {
                GTA.UI.Screen.ShowSubtitle("GET_MONEY_ERROR");
                return -1;
            }
        }
        
        //bye bye
        private void MarkPedestriansAsNoLongerNeeded()
        {
            foreach (var t in _ped)
            {
                t.MarkAsNoLongerNeeded();
            }
        }
        
        private bool IsAPassengerOutsideVehicle()
        {
            return _ped.Any(t => !t.IsInVehicle(coach));
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
    }
    
}