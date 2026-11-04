using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ocelot.BlueCrystalCooking.functions
{
    public static class BarrelFunctions
    {
        public static void OnGestureChanged(UnturnedPlayer player, EPlayerGesture gesture)
        {
            if (player?.Player == null)
                return;

            if (Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out RaycastHit raycastHit, 2, RayMasks.BARRICADE))
            {
                if (raycastHit.transform == null)
                    return;

                BarricadeDrop hitDrop = BarricadeManager.FindBarricadeByRootTransform(raycastHit.transform);
                Transform targetTransform = hitDrop?.model != null ? hitDrop.model : raycastHit.transform;

                foreach (var barrel in BlueCrystalCookingPlugin.Instance.placedBarrelsTransformsIngredients.ToList())
                {
                    if (barrel.Key == null)
                    {
                        BlueCrystalCookingPlugin.Instance.placedBarrelsTransformsIngredients.Remove(barrel.Key);
                        continue;
                    }

                    if (barrel.Key == targetTransform || barrel.Key.position == targetTransform.position)
                    {
                        uint ingredientCount = 0;
                        for (int i = 0; i < BlueCrystalCookingPlugin.Instance.Configuration.Instance.drugIngredientIds.Count; i++)
                        {
                            if (barrel.Value.ingredients.Contains(BlueCrystalCookingPlugin.Instance.Configuration.Instance.drugIngredientIds[i]))
                            {
                                ingredientCount += 1;
                            }
                        }

                        if (ingredientCount == BlueCrystalCookingPlugin.Instance.Configuration.Instance.drugIngredientIds.Count)
                        {
                            if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBarrelStirEffect)
                            {
                                BlueCrystalCookingPlugin.TriggerEffect(
                                    BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelStirEffectId,
                                    4f,
                                    targetTransform.position);
                            }
                            barrel.Value.progress += BlueCrystalCookingPlugin.Instance.Configuration.Instance.StirProgressAddPercentage;
                        }
                        else
                        {
                            ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("not_enough_ingredients"), Color.white, null, player.SteamPlayer(), EChatMode.SAY, BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                        }

                        if (barrel.Value.progress >= 100)
                        {
                            barrel.Value.progress = 0;
                            for (int i = 0; i < BlueCrystalCookingPlugin.Instance.Configuration.Instance.drugIngredientIds.Count; i++)
                            {
                                barrel.Value.ingredients.Remove(BlueCrystalCookingPlugin.Instance.Configuration.Instance.drugIngredientIds[i]);
                            }
                            ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("stir_successful"), Color.white, null, player.SteamPlayer(), EChatMode.SAY, BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);

                            Barricade trayBarricade = BlueCrystalCookingPlugin.CreateBarricade(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayId);
                            if (trayBarricade != null)
                            {
                                BarricadeManager.dropBarricade(trayBarricade, null, player.Position, 0, 0, 0, (ulong)player.CSteamID, (ulong)player.Player.quests.groupID);
                            }
                        }
                        break;
                    }
                }
            }
        }

        public static void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            Vector3 pos = point;
            ulong ownerBarricade = owner;
            if (asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId)
            {
                BlueCrystalCookingPlugin.Instance.Wait(0.2f, () =>
                {
                    Transform barrelTransform = BlueCrystalCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                    if (barrelTransform != null && !BlueCrystalCookingPlugin.Instance.placedBarrelsTransformsIngredients.ContainsKey(barrelTransform))
                    {
                        BlueCrystalCookingPlugin.Instance.placedBarrelsTransformsIngredients.Add(barrelTransform, new BarrelObject(new List<ushort>(), 0));
                    }
                });
            }

            foreach (var drugObject in BlueCrystalCookingPlugin.Instance.Configuration.Instance.drugIngredientIds)
            {
                if (drugObject == asset.id)
                {
                    if (Physics.Raycast(pos + Vector3.up * 0.1f, Vector3.down, out RaycastHit raycastHit, 10, RayMasks.BARRICADE))
                    {
                        BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(raycastHit.transform);
                        if (drop?.asset != null && drop.asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId)
                        {
                            foreach (var barrel in BlueCrystalCookingPlugin.Instance.placedBarrelsTransformsIngredients.ToList())
                            {
                                if (barrel.Key == drop.model || barrel.Key == raycastHit.transform)
                                {
                                    Player ownerPlayer = PlayerTool.getPlayer(new CSteamID(ownerBarricade));
                                    if (ownerPlayer != null)
                                    {
                                        ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("ingredient_added", asset.itemName), Color.white, null, ownerPlayer.channel.owner, EChatMode.SAY, BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                                    }
                                    barrel.Value.ingredients.Add(asset.id);
                                    BlueCrystalCookingPlugin.Instance.Wait(0.2f, () =>
                                    {
                                        Transform ingredientTransform = BlueCrystalCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                                        if (ingredientTransform != null)
                                        {
                                            BarricadeDrop ingredientDrop = BarricadeManager.FindBarricadeByRootTransform(ingredientTransform);
                                            if (ingredientDrop != null && BarricadeManager.tryGetRegion(ingredientTransform, out byte xingredient, out byte yingredient, out ushort plantingredient, out _))
                                            {
                                                BarricadeManager.destroyBarricade(ingredientDrop, xingredient, yingredient, plantingredient);
                                            }
                                        }
                                    });
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}
