namespace GtaVBusMod
{
    /// <summary>
    /// Contains constant values used throughout the bus mod.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Path to the missions XML configuration file.
        /// </summary>
        public const string MissionsXmlPath = @"scripts\bus_mod_missions.xml";

        /// <summary>
        /// Path to the mod configuration INI file.
        /// </summary>
        public const string ConfigIniPath = @"scripts\bus_mod.ini";

        /// <summary>
        /// Distance threshold for vehicle proximity to pedestrians (in game units).
        /// </summary>
        public const float PickupDistance = 30.0f;

        /// <summary>
        /// Distance threshold for vehicle proximity to destination (in game units).
        /// </summary>
        public const float DestinationDistance = 30.0f;

        /// <summary>
        /// Minimum distance from destination to allow passenger pickup (in game units).
        /// </summary>
        public const float MinDistanceFromDestination = 50.0f;

        /// <summary>
        /// Radius around vehicle to place pedestrians when they exit (in game units).
        /// </summary>
        public const float PedestrianExitRadius = 5.0f;

        /// <summary>
        /// Duration for displaying mission description subtitles (in milliseconds).
        /// </summary>
        public const int DescriptionDisplayDuration = 4000;

        /// <summary>
        /// Duration for displaying mission completion message (in milliseconds).
        /// </summary>
        public const int CompletionMessageDuration = 4000;

        /// <summary>
        /// Duration for displaying mission failure messages (in milliseconds).
        /// </summary>
        public const int FailureMessageDuration = 3500;

        #region Messages

        public const string ModLoadedMessage = "Bus mod loaded - scorz";
        public const string PassengerPickupMessage = "Come on, get in!";
        public const string MissionCompleteMessage = "Ok we're here. Thanks for using Dashound Bus Center.";
        public const string MissionCanceledMessage = "MISSION CANCELED ALL YOU HAD TO DO WAS TAKE THEM TO THE DAMN DESTINATION!";
        public const string MissionFailedMessage = "ALL YOU HAD TO DO WAS TAKE THEM TO THE DAMN DESTINATION!";
        public const string MissionFailedInsultMessage = "YOU'RE USELESS AS A CORPSE'S DICK!";

        #endregion
    }
}
