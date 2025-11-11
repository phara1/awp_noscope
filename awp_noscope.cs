using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace awp_noscope
{
    public class awp_noscope : BasePlugin
    {
        public override string ModuleAuthor => "keno";
        public override string ModuleName => "awp_noscope";
        public override string ModuleVersion => "1.0";
        public override string ModuleDescription => "AWP - No Scope Only";

        private static readonly string[] ScopedWeapons =
        {
            "weapon_awp",
            "weapon_ssg08"
        };

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            RegisterListener<Listeners.OnTick>(OnTick);
        }

        public static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (player == null || !player.IsValid || player.IsBot)
                    continue;

                if (player.PawnIsAlive)
                {
                    PreventScoping(player);
                }
            }
        }
        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                return HookResult.Continue;

            Server.NextFrame(() =>
            {
                RemoveAllWeapons(player);
                player.GiveNamedItem("weapon_awp");
            });

            return HookResult.Continue;
        }

        private void RemoveAllWeapons(CCSPlayerController player)
        {
            if (player == null || !player.IsValid)
                return;

            var weaponServices = player.PlayerPawn?.Value?.WeaponServices;
            if (weaponServices?.MyWeapons == null)
                return;

            foreach (var weapon in weaponServices.MyWeapons)
            {
                if (weapon?.IsValid == true && weapon.Value != null)
                {
                    weapon.Value.AddEntityIOEvent("Kill", weapon.Value, null, "", 0.1f);
                }
            }
        }
        private static void PreventScoping(CCSPlayerController player)
        {
            var activeWeapon = player.PlayerPawn?.Value?.WeaponServices?.ActiveWeapon.Value as CBasePlayerWeapon;
            if (activeWeapon == null)
                return;

            if (!ScopedWeapons.Contains(activeWeapon.DesignerName))
                return;

            activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
        }
    }
}

