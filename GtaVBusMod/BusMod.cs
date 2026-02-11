using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using GTA;
using LemonUI;
using LemonUI.Menus;

namespace GtaVBusMod
{
    public class BusMod : Script
    {
        private readonly List<string> _missionName = new List<string>();
        private NativeMenu main_m;
        private readonly ObjectPool _menuPool = new ObjectPool();
        private static readonly bool _lock = false;
        private string _keycode = string.Empty;

        public BusMod()
        {
            Interval = 0;
            GTA.UI.Notification.Show("Bus mod loaded - scorz");
            Tick += (o, e) => {
               
                _menuPool.Process();
            };

            KeyDown += (o, e) =>
            {
                if (e.KeyCode.ToString() == GetKey() && !_menuPool.AreAnyVisible)
                {
                    StartMainMenu();
                }
            };
        }
        
        private List<string> GetMissionName()
        {
            if (_missionName.Count > 0)
            {
                return _missionName;
            }
            try
            {
                const string path = @"scripts\\bus_mod_missions.xml";
                using (var fS = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(fS);
                    var nList = xmlDoc.SelectNodes("/missions/element/name");
                    Debug.Assert(nList != null, nameof(nList) + " != null");
                    _missionName.AddRange(from XmlNode node in nList select node.InnerText);
                    fS.Close();
                }
            } 
            catch (Exception e)
            {
                GTA.UI.Screen.ShowSubtitle(e.Message);
            }

            return _missionName;
        }
        
        private string GetKey()
        {
            if (_keycode != string.Empty)
            {
                return _keycode;
            }
            var key = "";
            const string path = @"scripts\\bus_mod.ini";
            try
            {
                using (var r = new StreamReader(path))
                {
                    key = r.ReadLine();
                    r.Close();
                }
            }
            catch (Exception e)
            {
                GTA.UI.Screen.ShowSubtitle(e.Message);
            }
            Debug.Assert(key != null, nameof(key) + " != null");
            var split = key.Split('=');
            _keycode = split[1];
            return _keycode;
        }
        
        // Returning first mission
        private string get_first_child()
        {
            return GetMissionName()[0];
        }
        
        private void StartMainMenu()
        {
            main_m = new NativeMenu("", "Bus mod by scorz");
            _menuPool.Add(main_m);
            var missionName = GetMissionName();
            var startBut = new NativeItem("Start", "Click to start mission.");
            main_m.Add(startBut);
            startBut.Activated += (sender, args) =>
            {
                GTA.UI.Screen.ShowSubtitle("Start mission");
            };
            var missionList = new NativeListItem<string>("Missions List",  "Choose mission")
            {
                Items = missionName
            };
            main_m.Add(missionList);
            missionList.ItemChanged += (sender, args) =>
            {
                GTA.UI.Screen.ShowSubtitle("Mission List Changed");
            };
            var cancel = new NativeItem("Cancel", "Cancel current mission")
            {
                Enabled = false
            };
            main_m.Add(cancel);
            if (_lock)
            {
                startBut.Enabled = false;
                missionList.Enabled = false;
                cancel.Enabled = true;
            }
            main_m.Visible = !main_m.Visible;
        }
       
    }
}