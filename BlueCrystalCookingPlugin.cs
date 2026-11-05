using Ocelot.BlueCrystalCooking.functions;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Ocelot.BlueCrystalCooking
{
    public class BlueCrystalCookingPlugin : RocketPlugin<BlueCrystalCookingConfiguration>
    {
        public static BlueCrystalCookingPlugin Instance;
        public const string VERSION = "1.1.3";

        private int Frame = 0;
        public long timer = 0;
        public Dictionary<Transform, BarrelObject> placedBarrelsTransformsIngredients = new Dictionary<Transform, BarrelObject>();
        public List<DrugeffectTimeObject> drugeffectPlayersList = new List<DrugeffectTimeObject>();
        public List<FreezingTrayObject> freezingTrays = new List<FreezingTrayObject>();

        protected override void Load()
        {
            Instance = this;

            BarricadeManager.onDeployBarricadeRequested += BarricadeDeployed;
            BarricadeDrop.OnSalvageRequested_Global += OnBarricadeSalvageRequested;
            PlayerAnimator.OnGestureChanged_Global += OnGestureChanged;
            UseableConsumeable.onConsumePerformed += ConsumeAction;
            BarricadeManager.onDamageBarricadeRequested += BarricadeDamaged;
            if (Level.isLoaded)
            {
                AddExistingBarrels(1);
            }
            else
            {
                Level.onLevelLoaded += AddExistingBarrels;
            }

            Logger.Log("=================================================", ConsoleColor.Cyan);
            Logger.Log($"  {Name} v{VERSION} loaded successfully!", ConsoleColor.Green);
            Logger.Log("  Interactive Drug Cooking & Chemical Processing", ConsoleColor.Yellow);
            Logger.Log("  Powered by UnturnedForge: https://www.unturnedforge.com", ConsoleColor.Cyan);
            Logger.Log("=================================================", ConsoleColor.Cyan);
        }

        private void OnBarricadeSalvageRequested(BarricadeDrop barricade, SteamPlayer instigatorClient, ref bool shouldAllow)
        {
            if (!shouldAllow || barricade?.model == null || barricade.asset == null)
                return;

            if (barricade.asset.id == Configuration.Instance.BarrelObjectId)
            {
                placedBarrelsTransformsIngredients.Remove(barricade.model);
            }
            else if (barricade.asset.id == Configuration.Instance.LiquidTrayId || barricade.asset.id == Configuration.Instance.BlueCrystalTrayId)
            {
                freezingTrays.RemoveAll(t => t.transform == null || t.transform == barricade.model);
            }
        }

        protected override void Unload()
        {
            BarricadeManager.onDeployBarricadeRequested -= BarricadeDeployed;
            BarricadeDrop.OnSalvageRequested_Global -= OnBarricadeSalvageRequested;
            PlayerAnimator.OnGestureChanged_Global -= OnGestureChanged;
            UseableConsumeable.onConsumePerformed -= ConsumeAction;
            BarricadeManager.onDamageBarricadeRequested -= BarricadeDamaged;
            Level.onLevelLoaded -= AddExistingBarrels;

            foreach (var drugeffect in drugeffectPlayersList)
            {
                if (ulong.TryParse(drugeffect.playerId, out ulong steamId))
                {
                    Player player = PlayerTool.getPlayer(new CSteamID(steamId));
                    if (player?.movement != null)
                    {
                        if (Configuration.Instance.UseDrugEffectSpeed)
                            player.movement.sendPluginSpeedMultiplier(1);
                        if (Configuration.Instance.UseDrugEffectJump)
                            player.movement.sendPluginJumpMultiplier(1);
                    }
                }
            }
            drugeffectPlayersList.Clear();
            placedBarrelsTransformsIngredients.Clear();
            freezingTrays.Clear();

            Logger.Log("=================================================", ConsoleColor.Cyan);
            Logger.Log($"  {Name} has been unloaded!", ConsoleColor.Yellow);
            Logger.Log("  Visit https://www.unturnedforge.com for updates!", ConsoleColor.Cyan);
            Logger.Log("=================================================", ConsoleColor.Cyan);
        }

        private void ConsumeAction(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            MethBagFunctions.ConsumeAction(instigatingPlayer, consumeableAsset);
        }

        private void OnGestureChanged(PlayerAnimator arg1, EPlayerGesture gesture)
        {
            if (arg1?.player == null)
                return;

            if (gesture == EPlayerGesture.PUNCH_LEFT || gesture == EPlayerGesture.PUNCH_RIGHT)
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(arg1.player);
                BarrelFunctions.OnGestureChanged(uPlayer, gesture);
                MethBagFunctions.OnGestureChanged(uPlayer, gesture);
            }
        }

        private void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!shouldAllow || asset == null)
                return;

            BarrelFunctions.BarricadeDeployed(barricade, asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
            FreezerFunctions.BarricadeDeployed(barricade, asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
        }

        private void BarricadeDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (!shouldAllow)
                return;

            BarricadeFunctions.BarricadeDamaged(barricadeTransform, pendingTotalDamage);
        }

        public Dictionary<Vector3, Transform> GetAllObjects()
        {
            Dictionary<Vector3, Transform> objectsOnMap = new Dictionary<Vector3, Transform>();
            if (BarricadeManager.regions != null)
            {
                foreach (var region in BarricadeManager.regions)
                {
                    if (region?.drops == null)
                        continue;

                    foreach (var drop in region.drops)
                    {
                        if (drop?.model != null && !objectsOnMap.ContainsKey(drop.model.position))
                        {
                            objectsOnMap.Add(drop.model.position, drop.model);
                        }
                    }
                }
            }
            if (BarricadeManager.vehicleRegions != null)
            {
                foreach (var region in BarricadeManager.vehicleRegions)
                {
                    if (region?.drops == null)
                        continue;

                    foreach (var drop in region.drops)
                    {
                        if (drop?.model != null && !objectsOnMap.ContainsKey(drop.model.position))
                        {
                            objectsOnMap.Add(drop.model.position, drop.model);
                        }
                    }
                }
            }
            return objectsOnMap;
        }

        public Transform GetPlacedObjectTransform(Vector3 objectPosition)
        {
            Dictionary<Vector3, Transform> objectsOnMap = GetAllObjects();

            foreach (var mapObject in objectsOnMap)
            {
                if (mapObject.Key == objectPosition)
                {
                    return mapObject.Value;
                }
            }
            return null;
        }

        public BarricadeData getBarricadeDataAtPosition(Vector3 position)
        {
            if (BarricadeManager.regions != null)
            {
                foreach (var region in BarricadeManager.regions)
                {
                    if (region?.drops == null)
                        continue;

                    foreach (var drop in region.drops)
                    {
                        BarricadeData data = drop?.GetServersideData();
                        if (data != null && data.point == position)
                            return data;
                    }
                }
            }
            if (BarricadeManager.vehicleRegions != null)
            {
                foreach (var region in BarricadeManager.vehicleRegions)
                {
                    if (region?.drops == null)
                        continue;

                    foreach (var drop in region.drops)
                    {
                        BarricadeData data = drop?.GetServersideData();
                        if (data != null && data.point == position)
                            return data;
                    }
                }
            }
            return null;
        }

        private void AddExistingBarrels(int level)
        {
            Logger.Log("Adding map barrels to list...", ConsoleColor.Green);
            if (BarricadeManager.regions != null)
            {
                foreach (var region in BarricadeManager.regions)
                {
                    if (region?.drops == null)
                        continue;

                    foreach (var drop in region.drops)
                    {
                        if (drop?.asset != null && drop.model != null && drop.asset.id == Configuration.Instance.BarrelObjectId)
                        {
                            if (placedBarrelsTransformsIngredients.ContainsKey(drop.model))
                            {
                                Logger.Log("Duplicated entry detected, skipping object. (No need to worry)", ConsoleColor.Yellow);
                            }
                            else
                            {
                                placedBarrelsTransformsIngredients.Add(drop.model, new BarrelObject(new List<ushort>(), 0));
                            }
                        }
                    }
                }
            }
            if (BarricadeManager.vehicleRegions != null)
            {
                foreach (var region in BarricadeManager.vehicleRegions)
                {
                    if (region?.drops == null)
                        continue;

                    foreach (var drop in region.drops)
                    {
                        if (drop?.asset != null && drop.model != null && drop.asset.id == Configuration.Instance.BarrelObjectId)
                        {
                            if (!placedBarrelsTransformsIngredients.ContainsKey(drop.model))
                            {
                                placedBarrelsTransformsIngredients.Add(drop.model, new BarrelObject(new List<ushort>(), 0));
                            }
                        }
                    }
                }
            }
            Logger.Log("All barrels added.", ConsoleColor.Green);
        }

        public static void TriggerEffect(ushort effectId, float relevantDistance, Vector3 position)
        {
            if (Assets.find(EAssetType.EFFECT, effectId) is EffectAsset effectAsset)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset)
                {
                    position = position,
                    relevantDistance = relevantDistance,
                    shouldReplicate = true
                };
                EffectManager.triggerEffect(parameters);
            }
        }

        public static Barricade CreateBarricade(ushort itemId)
        {
            if (Assets.find(EAssetType.ITEM, itemId) is ItemBarricadeAsset barricadeAsset)
            {
                return new Barricade(barricadeAsset);
            }
            return null;
        }

        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"not_enough_ingredients", "There are <color=#ff3c19>not enough ingredients</color> in the barrel to stir them into blue crystal." },
            {"ingredient_added", "You have <color=#75ff19>added {0}</color> to the barrel." },
            {"stir_successful", "You have <color=#75ff19>successfully mixed</color> the ingredients into a tray filled with <color=#1969ff>liquid blue crystal</color>." },
            {"bluecrystalbags_obtained", "You have <color=#75ff19>successfully obtained {0} bags</color> filled with <color=#1969ff>blue crystal</color>." }
        };

        private void Update()
        {
            Frame++;
            if (Frame % 5 != 0) return;

            if (getCurrentTime() - timer >= 1)
            {
                timer = getCurrentTime();
                MethBagFunctions.Update();
                FreezerFunctions.Update();
            }
        }

        public static Int32 getCurrentTime()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public void Wait(float seconds, System.Action action)
        {
            StartCoroutine(_wait(seconds, action));
        }

        IEnumerator _wait(float time, System.Action callback)
        {
            yield return new WaitForSeconds(time);
            callback();
        }
    }
}
