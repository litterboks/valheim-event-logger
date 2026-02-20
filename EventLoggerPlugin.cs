using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace EventLogger;

[BepInPlugin("games.blockfactory.eventlogger", "EventLogger", "4.0.0")]
public class EventLoggerPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;
    internal static Dictionary<long, string> KnownPlayers = new Dictionary<long, string>();
    internal static PortalScanner PortalScanner;

    // Fermenter ZDO scan state: uid -> "fermenting" | "done"
    internal static Dictionary<ZDOID, string> FermenterStates = new Dictionary<ZDOID, string>();
    // Who filled each fermenter (by ZDO uid)
    internal static Dictionary<ZDOID, string> FermenterFilledBy = new Dictionary<ZDOID, string>();

    private float fermenterScanTimer;

    private FermenterScanner fermenterScanner;

    internal static string CleanName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "unknown";
        var idx = name.IndexOf('(');
        if (idx > 0) name = name.Substring(0, idx);
        return name.Trim().Replace(" ", "_");
    }

    internal static string GetPlayerNameFromPeer(long sender)
    {
        if (ZNet.instance == null) return null;
        var peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return null;
        return CleanName(peer.m_playerName);
    }

    void Awake()
    {
        Log = Logger;
        PluginConfig.Bind(Config);
        PortalScanner = new PortalScanner();
        fermenterScanner = new FermenterScanner();

        var harmony = new Harmony("games.blockfactory.eventlogger");

        PatchSafe(harmony, typeof(PeerInfoPatch), "ZNet.RPC_PeerInfo");
        PatchSafe(harmony, typeof(PeerDisconnectPatch), "ZNet.RPC_Disconnect");
        PatchSafe(harmony, typeof(CharacterDropPatch), "CharacterDrop.OnDeath");
        PatchSafe(harmony, typeof(PickablePatch), "Pickable.RPC_Pick");
        PatchSafe(harmony, typeof(TameablePatch), "Tameable.Tame");
        PatchSafe(harmony, typeof(RandomEventPatch), "RandEventSystem.SetActiveEvent");
        PatchSafe(harmony, typeof(ChatPingPatch), "Chat.OnNewChatMessage");
        PatchSafe(harmony, typeof(EatFoodPatch), "Player.EatFood");
        PatchSafe(harmony, typeof(SleepStartPatch), "Game.SleepStart");
        PatchSafe(harmony, typeof(SleepStopPatch), "Game.SleepStop");
        PatchSafe(harmony, typeof(BossSummonPatch), "OfferingBowl.SpawnBoss");
        PatchSafe(harmony, typeof(SmelterFuelPatch), "Smelter.OnAddFuel");
        PatchSafe(harmony, typeof(SmelterOrePatch), "Smelter.OnAddOre");
        PatchSafe(harmony, typeof(SmelterDonePatch), "Smelter.Spawn");
        PatchSafe(harmony, typeof(PlantGrowPatch), "Plant.Grow");
        PatchSafe(harmony, typeof(BeehiveExtractPatch), "Beehive.RPC_Extract");
        PatchSafe(harmony, typeof(SapExtractPatch), "SapCollector.RPC_Extract");
        PatchSafe(harmony, typeof(FermenterTapPatch), "Fermenter.RPC_Tap");
        PatchSafe(harmony, typeof(FermenterAddPatch), "Fermenter.RPC_AddItem");
        PatchSafe(harmony, typeof(CookingAddPatch), "CookingStation.RPC_AddItem");
        PatchSafe(harmony, typeof(CookingRemoveDonePatch), "CookingStation.RPC_RemoveDoneItem");
        PatchSafe(harmony, typeof(TombstoneCreatePatch), "Player.CreateTombStone");
        PatchSafe(harmony, typeof(TombstonePickupPatch), "Container.RPC_RequestTakeAll (tombstone)");
        PatchSafe(harmony, typeof(PortalPlacePatch), "Piece.SetCreator (portal)");
        PatchSafe(harmony, typeof(PortalDestroyPatch), "WearNTear.Destroy (portal)");
        PatchSafe(harmony, typeof(PortalRenamePatch), "TeleportWorld.RPC_SetTag");

        // Initial portal scan (will retry if world isn't loaded yet)
        PortalScanner.RequestScan();

        Log.LogInfo("EventLogger v4.0.0 loaded");
    }

    void Update()
    {
        try
        {
            float dt = Time.deltaTime;
            fermenterScanTimer += dt;

            if (fermenterScanTimer >= PluginConfig.FermenterScanInterval.Value)
            {
                fermenterScanner.Scan();
                fermenterScanTimer = 0f;
            }

            PortalScanner.CheckAndScan();
        }
        catch (Exception e)
        {
            Log.LogError($"Update error: {e}");
        }
    }

    private static void PatchSafe(Harmony harmony, Type type, string name)
    {
        try
        {
            harmony.PatchAll(type);
            Log.LogInfo($"Patched {name}");
        }
        catch (Exception e)
        {
            Log.LogError($"{name} patch failed: {e}");
        }
    }
}
