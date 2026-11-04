using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Linq;
using UnityEngine;

namespace Ocelot.BlueCrystalCooking.functions
{
    public static class MethBagFunctions
    {
        public static void OnGestureChanged(UnturnedPlayer player, EPlayerGesture gesture)
        {
            if (player?.Player == null)
                return;

            if (Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out RaycastHit raycastHit, 2, RayMasks.BARRICADE))
            {
                if (raycastHit.transform == null)
                    return;

                BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(raycastHit.transform);
                if (drop?.asset != null && drop.asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.FrozenTrayId)
                {
                    if (BarricadeManager.tryGetRegion(drop.model, out byte x, out byte y, out ushort plant, out _))
                    {
                        Vector3 dropPos = drop.model.position;
                        int random = UnityEngine.Random.Range(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalBagsAmountMin, BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalBagsAmountMax);
                        for (int i = 0; i < random; i++)
                        {
                            ItemManager.dropItem(new Item(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalBagId, true), new Vector3(dropPos.x, dropPos.y + 2, dropPos.z), false, true, false);
                            if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBlueCrystalFreezeEffect)
                            {
                                BlueCrystalCookingPlugin.TriggerEffect(
                                    BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalFreezeEffectId,
                                    10f,
                                    dropPos);
                            }
                        }
                        ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("bluecrystalbags_obtained", random), Color.white, null, player.SteamPlayer(), EChatMode.SAY, BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                        BarricadeManager.destroyBarricade(drop, x, y, plant);
                    }
                }
            }
        }

        public static void ConsumeAction(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            if (instigatingPlayer == null || consumeableAsset == null)
                return;

            UnturnedPlayer player = UnturnedPlayer.FromPlayer(instigatingPlayer);
            if (consumeableAsset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalBagId)
            {
                DrugeffectTimeObject existingEffect = BlueCrystalCookingPlugin.Instance.drugeffectPlayersList.FirstOrDefault(d => d.playerId == player.Id);
                if (existingEffect != null)
                {
                    existingEffect.time = BlueCrystalCookingPlugin.getCurrentTime();
                }
                else
                {
                    BlueCrystalCookingPlugin.Instance.drugeffectPlayersList.Add(new DrugeffectTimeObject(player.Id));
                }

                if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.UseDrugEffectSpeed)
                {
                    player.Player.movement.sendPluginSpeedMultiplier(BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugEffectSpeedMultiplier);
                }
                if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.UseDrugEffectJump)
                {
                    player.Player.movement.sendPluginJumpMultiplier(BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugEffectJumpMultiplier);
                }
            }
        }

        public static void Update()
        {
            foreach (var drugeffect in BlueCrystalCookingPlugin.Instance.drugeffectPlayersList.ToList())
            {
                if (BlueCrystalCookingPlugin.getCurrentTime() - drugeffect.time >= BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugEffectDurationSecs)
                {
                    BlueCrystalCookingPlugin.Instance.drugeffectPlayersList.Remove(drugeffect);
                    if (ulong.TryParse(drugeffect.playerId, out ulong steamId))
                    {
                        Player uPlayer = PlayerTool.getPlayer(new CSteamID(steamId));
                        if (uPlayer?.movement != null)
                        {
                            if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.UseDrugEffectSpeed)
                            {
                                uPlayer.movement.sendPluginSpeedMultiplier(1);
                            }
                            if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.UseDrugEffectJump)
                            {
                                uPlayer.movement.sendPluginJumpMultiplier(1);
                            }
                        }
                    }
                }
            }
        }
    }
}
