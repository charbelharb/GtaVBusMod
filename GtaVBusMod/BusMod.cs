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
        private  bool _lock;
        private string _keycode = string.Empty;
        private Mission _mission = new Mission();
        private int _currentMissionIndex = 0;

        public BusMod()
        {
            Interval = 0;
            GTA.UI.Notification.Show("Bus mod loaded - scorz");
            Tick += (o, e) => {
               
                _menuPool.Process();
                if (_lock)
                {
                    _lock = _mission.Check();
                };
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
            if (!string.IsNullOrWhiteSpace(_keycode))
            {
                return _keycode;
            }
            var split = new string[] { };
            const string path = @"scripts\\bus_mod.ini";
            try
            {
                using (var r = new StreamReader(path))
                {
                    var key = r.ReadLine();
                    split = key?.Split('=');
                    r.Close();
                }
            }
            catch (Exception e)
            {
                GTA.UI.Screen.ShowSubtitle(e.Message);
            }
            _keycode = split?[1];
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
            main_m.BannerText.Text = "Dashound Bus Center";
            main_m.Add(startBut);
            startBut.Activated += (sender, args) =>
            {
                _lock = true;
                _menuPool.HideAll();
                _mission.PrepareMission(_missionName[_currentMissionIndex]);
            };
            var missionList = new NativeListItem<string>("Missions List",  "Choose mission")
            {
                Items = missionName
            };
            main_m.Add(missionList);
            missionList.ItemChanged += (sender, args) =>
            {
                _currentMissionIndex = args.Index;
            };
            var cancel = new NativeItem("Cancel", "Cancel current mission")
            {
                Enabled = false
            };
            main_m.Add(cancel);
            cancel.Activated += (sender, args) =>
            {
                _menuPool.HideAll();
                _mission.CancelMission();
                _lock = false;
            };
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