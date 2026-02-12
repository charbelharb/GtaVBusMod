using System;
using System.IO;
using System.Xml;
using GTA;

namespace GtaVBusMod.Services
{
    /// <summary>
    /// Service class responsible for reading and parsing mission data from XML configuration files.
    /// </summary>
    public class XmlMissionDataService
    {
        private readonly XmlDocument _xmlDocument = new XmlDocument();
        private readonly string _currentMission;

        /// <summary>
        /// Initializes a new instance of the XmlMissionDataService and loads the missions XML file.
        /// </summary>
        /// <param name="missionName">The name of the mission to load data for</param>
        public XmlMissionDataService(string missionName)
        {
            _currentMission = missionName;
            LoadXmlDocument();
        }

        #region XML Document Loading

        /// <summary>
        /// Loads the missions XML document from the file system.
        /// </summary>
        private void LoadXmlDocument()
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), Constants.MissionsXmlPath);
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    _xmlDocument.Load(fileStream);
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"Failed to load missions.xml: {ex.Message}");
            }
        }

        #endregion

        #region Mission Description

        /// <summary>
        /// Retrieves the mission description from the XML.
        /// </summary>
        /// <returns>Mission description text, or empty string if not found</returns>
        public string GetMissionDescription()
        {
            try
            {
                var descriptionNode = _xmlDocument.SelectSingleNode($"/missions/element[name='{_currentMission}']/description");
                return descriptionNode?.InnerText ?? string.Empty;
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"GET_DESC_ERROR: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion

        #region Pedestrian Data

        /// <summary>
        /// Gets the number of pedestrians for the current mission.
        /// </summary>
        /// <returns>Number of pedestrians, or -1 if error occurs</returns>
        public int GetPedestrianCount()
        {
            try
            {
                var pedNodes = _xmlDocument.SelectNodes($"/missions/element[name='{_currentMission}']/ped");
                return pedNodes?.Count ?? -1;
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"GET_PED_NUMBER_ERROR: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Gets the hash code for a pedestrian model.
        /// </summary>
        /// <param name="index">Index of the pedestrian</param>
        /// <returns>Hash code for the pedestrian model</returns>
        public int GetPedestrianHash(int index)
        {
            return GetElementHash("ped", index);
        }

        #endregion

        #region Vehicle Data

        /// <summary>
        /// Gets the hash string for a vehicle.
        /// </summary>
        /// <param name="index">Index of the vehicle</param>
        /// <returns>Vehicle hash string</returns>
        public string GetVehicleHash(int index)
        {
            try
            {
                var vehicleNodes = _xmlDocument.SelectNodes($"/missions/element[name='{_currentMission}']/vehicle/hash");
                return vehicleNodes?[index].InnerText ?? "bus";
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"GET_VEHICLE_HASH_ERROR: {ex.Message}");
                return "bus";
            }
        }

        #endregion

        #region Coordinates

        /// <summary>
        /// Gets a coordinate value from the XML for a specified element.
        /// </summary>
        /// <param name="elementType">Type of element (e.g., "ped", "vehicle", "destination")</param>
        /// <param name="index">Index of the element</param>
        /// <param name="coordinate">Coordinate type ('x', 'y', 'z', or 't' for heading)</param>
        /// <returns>Coordinate value, or 100000 if error occurs</returns>
        public float GetCoordinate(string elementType, int index, char coordinate)
        {
            try
            {
                var coordinateNodes = _xmlDocument.SelectNodes(
                    $"/missions/element[name='{_currentMission}']/{elementType}/position/{coordinate}");
                
                if (coordinateNodes != null && coordinateNodes.Count > index)
                {
                    return float.TryParse(coordinateNodes[index].InnerText, out var value) 
                        ? value 
                        : 100000f;
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"GET_COORDINATE_ERROR: {elementType} {index} {coordinate} - {ex.Message}");
            }

            return 100000f;
        }

        #endregion

        #region Money Reward

        /// <summary>
        /// Gets the money penalty/reward for the current mission.
        /// Handles both old format: &lt;money&gt;&lt;ammount&gt;100&lt;/ammount&gt;&lt;/money&gt;
        /// and new format: &lt;money&gt;100&lt;/money&gt;
        /// </summary>
        /// <returns>Money amount, or -1 if error occurs</returns>
        public int GetMoney()
        {
            try
            {
                // Try new format first: <money>100</money>
                var moneyNodes = _xmlDocument.SelectNodes($"/missions/element[name='{_currentMission}']/money");
                if (moneyNodes != null && moneyNodes.Count > 0)
                {
                    var moneyText = moneyNodes[0].InnerText.Trim();
                    
                    // If InnerText is just a number, use it (new format)
                    if (int.TryParse(moneyText, out var amount))
                    {
                        return amount;
                    }
                    
                    // Otherwise, try old format: <money><ammount>100</ammount></money>
                    var amountNode = moneyNodes[0].SelectSingleNode("ammount") ?? moneyNodes[0].SelectSingleNode("amount");
                    if (amountNode != null && int.TryParse(amountNode.InnerText, out var oldAmount))
                    {
                        return oldAmount;
                    }
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"GET_MONEY_ERROR: {ex.Message}");
            }

            return -1;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Gets the hash code for a generic element.
        /// </summary>
        /// <param name="elementType">Type of element</param>
        /// <param name="index">Index of the element</param>
        /// <returns>Hash code, or -1 if error occurs</returns>
        private int GetElementHash(string elementType, int index)
        {
            try
            {
                var hashNodes = _xmlDocument.SelectNodes(
                    $"/missions/element[name='{_currentMission}']/{elementType}/hash");
                
                if (hashNodes != null && hashNodes.Count > index)
                {
                    return int.TryParse(hashNodes[index].InnerText, out var hash) ? hash : -1;
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle($"GET_HASH_ERROR: {elementType} - {ex.Message}");
            }

            return -1;
        }

        #endregion
    }
}
