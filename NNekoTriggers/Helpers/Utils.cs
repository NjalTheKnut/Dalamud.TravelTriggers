using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using NNekoTriggers.Configuration;

namespace NNekoTriggers.Helpers
{
    public static unsafe class Utils
    {
        /// <summary>
        ///     A constant of the Id for Teleport.
        /// </summary>
        internal const uint Teleport = 5;

        /// <summary>
        ///     A constant of the Id for Return.
        /// </summary>
        internal const uint Return = 6;

        /// <summary>
        ///      A collection of item Ids that will trigger a teleport event when used.
        /// </summary>
        private static readonly uint[] TeleportItems = [
            21069, // Maelstrom Aetheryte Ticket
            21070, // Twin Adder Aetheryte Ticket
            21071, // Immortal Flames Aetheryte Ticket
            21071, // Vesper Bay Aetheryte Ticket
            41708, // Gold Saucer Aetheryte Ticket
            28064, // Firmament Aetheryte Ticket
            49121, // Cosmic Exploration Aetheryte Ticket
            2894,  // Eternity Ring
        ];

        /// <summary>
        ///     A collection of condition flags that indicate the player is likely loading into a new area.
        /// </summary>
        private static readonly ConditionFlag[] BusyFlags = [
            ConditionFlag.BetweenAreas,
            ConditionFlag.BetweenAreas51,
            ConditionFlag.Occupied,
            ConditionFlag.OccupiedInCutSceneEvent,
            ConditionFlag.Unconscious,
            ];

        /// <summary>
        ///     Obtains or initializes the configuration for the current character.
        /// </summary>
        /// <returns></returns>
        internal static CharacterConfiguration GetCharacterConfig()
        {
            if (NNekoTriggers.PluginConfiguration.CharacterConfigurations.TryGetValue(NNekoTriggers.PlayerState.ContentId, out var characterConfig))
            {
                return characterConfig;
            }
            else
            {
                characterConfig = new();
                NNekoTriggers.PluginConfiguration.CharacterConfigurations[NNekoTriggers.PlayerState.ContentId] = characterConfig;
                return characterConfig;
            }
        }

        /// <summary>
        ///     Checks whether the player can currently execute the specified action.
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static bool CanUseAction(ActionType actionType, uint actionId) => ActionManager.Instance()->GetActionStatus(actionType, actionId) == 0;

        /// <summary>
        ///     Identifies whether the player is in a sanctuary and not between areas (i.e. teleporting or still loading into a zone).
        /// </summary>
        /// <returns></returns>
        public static bool CanUseGlamourPlates() => TerritoryInfo.Instance()->InSanctuary && !IsBetweenAreas();

        /// <summary>
        ///     Checks whether the player character is still loading into a new area.
        /// </summary>
        /// <returns></returns>
        public static bool IsBetweenAreas()
        {
            var result = false;
            foreach (var item in BusyFlags)
            {
                if (NNekoTriggers.Condition[item])
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        ///     Confirms whether the character is currently casting and that the action will initiate a teleportation event once cast.
        /// </summary>
        /// <param name="acId"></param>
        /// <returns>boolean</returns>
        public static bool IsCastingTeleportAction(uint acId) => Player.IsCasting && CheckTpRetMnt(acId);

        /// <summary>
        ///     Checks whether an action will trigger a teleportation event.
        /// </summary>
        /// <param name="acId"></param>
        /// <returns>boolean</returns>
        public static bool CheckTpRetMnt(uint acId)
        {
            if (acId == Teleport)
            {
                return true;
            }

            if (TeleportItems.Contains(acId))
            {
                return true;
            }

            if (acId == Return)
            {
                return true;
            }

            return false;
        }
    }
}
