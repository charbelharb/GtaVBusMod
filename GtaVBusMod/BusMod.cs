using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using GTA;
using LemonUI;
using LemonUI.Menus;

namespace GtaVBusMod
{
    /// <summary>
    /// Main script class for the GTA V Bus Mod. Handles menu initialization, 
    /// user input, and mission lifecycle management.
    /// </summary>
    public class BusMod : Script
    {
        #region Fields

        private readonly List<string> _missionNames = new List<string>();
        private NativeMenu _mainMenu;
        private readonly ObjectPool _menuPool = new ObjectPool();
        private bool _isMissionActive;
        private string _keyCode = string.Empty;
        private readonly Mission _mission = new Mission();
        private int _currentMissionIndex = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the BusMod script.
        /// Sets up event handlers for menu processing and key inputs.
        /// </summary>
        public BusMod()
        {
            Interval = 0;
            GTA.UI.Notification.Show("Bus mod loaded - scorz");

            // Process menu pool and mission state on each tick
            Tick += (o, e) =>
            {
                _menuPool.Process();
                if (_isMissionActive)
                {
                    _isMissionActive = _mission.Check();
                }
            };

            // Handle key input for menu toggle
            KeyDown += (o, e) =>
            {
                if (e.KeyCode.ToString() == GetActivationKey() && !_menuPool.AreAnyVisible)
                {
                    ShowMainMenu();
                }
            };
        }

        #endregion

        #region Mission Loading

        /// <summary>
        /// Retrieves the list of available mission names from the XML configuration file.
        /// Caches the result to avoid repeated file reads.
        /// </summary>
        /// <returns>List of mission names</returns>
        private List<string> GetMissionNames()
        {
            if (_missionNames.Count > 0)
            {
                return _missionNames;
            }

            try
            {
                const string missionFilePath = @"scripts\bus_mod_missions.xml";
                using (var fileStream = new FileStream(missionFilePath, FileMode.Open, FileAccess.Read))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(fileStream);
                    var missionNodes = xmlDoc.SelectNodes("/missions/element/name");

                    if (missionNodes != null)
                    {
                        _missionNames.AddRange(from XmlNode node in missionNodes select node.InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"Error loading missions: {ex.Message}");
            }

            return _missionNames;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Reads the activation key from the INI configuration file.
        /// Expected format: Key=KeyCode
        /// </summary>
        /// <returns>The key code string to activate the menu</returns>
        private string GetActivationKey()
        {
            if (!string.IsNullOrWhiteSpace(_keyCode))
            {
                return _keyCode;
            }

            const string configFilePath = @"scripts\bus_mod.ini";
            try
            {
                using (var reader = new StreamReader(configFilePath))
                {
                    var configLine = reader.ReadLine();
                    var keyValuePair = configLine?.Split('=');

                    if (keyValuePair != null && keyValuePair.Length > 1)
                    {
                        _keyCode = keyValuePair[1];
                    }
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"Error loading config: {ex.Message}");
            }

            return _keyCode;
        }

        #endregion

        #region Menu Management

        /// <summary>
        /// Creates and displays the main menu for mission selection and control.
        /// Provides options to start missions, select from mission list, and cancel active missions.
        /// </summary>
        private void ShowMainMenu()
        {
            _mainMenu = new NativeMenu("", "Bus mod by scorz")
            {
                BannerText = { Text = "Dashound Bus Center" }
            };
            _menuPool.Add(_mainMenu);

            var missionNames = GetMissionNames();

            // Start Mission Button
            var startButton = new NativeItem("Start", "Click to start the selected mission.");
            _mainMenu.Add(startButton);
            startButton.Activated += (sender, args) =>
            {
                _isMissionActive = true;
                _menuPool.HideAll();
                _mission.PrepareMission(_missionNames[_currentMissionIndex]);
            };

            // Mission List Dropdown
            var missionListItem = new NativeListItem<string>("Missions List", "Choose a mission to play")
            {
                Items = missionNames
            };
            _mainMenu.Add(missionListItem);
            missionListItem.ItemChanged += (sender, args) =>
            {
                _currentMissionIndex = args.Index;
            };

            // Cancel Mission Button
            var cancelButton = new NativeItem("Cancel", "Cancel the currently active mission")
            {
                Enabled = false
            };
            _mainMenu.Add(cancelButton);
            cancelButton.Activated += (sender, args) =>
            {
                _menuPool.HideAll();
                _mission.CancelMission();
                _isMissionActive = false;
            };

            // Update menu item states based on mission status
            if (_isMissionActive)
            {
                startButton.Enabled = false;
                missionListItem.Enabled = false;
                cancelButton.Enabled = true;
            }

            _mainMenu.Visible = !_mainMenu.Visible;
        }

        #endregion
    }
}
