using SDG.Unturned;
using System.Linq;
using UnityEngine;

namespace Ocelot.BlueCrystalCooking.functions
{
    public static class FreezerFunctions
    {
        public static void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            Vector3 pos = point;
            if (asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.LiquidTrayId)
            {
                if (Physics.Raycast(pos, Vector3.down, out RaycastHit raycastHit, 10, RayMasks.BARRICADE))
                {
                    if (Physics.Raycast(pos, Vector3.up, out RaycastHit raycastHitUp, 10, RayMasks.BARRICADE))
                    {
                        if (raycastHit.transform == raycastHitUp.transform)
                        {
                            BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(raycastHit.transform);
                            if (drop?.asset != null && drop.asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.FreezerId)
                            {
                                ulong ownerTray = owner;
                                ulong groupTray = group;
                                float angle_x_tray = angle_x;
                                float angle_y_tray = angle_y;
                                float angle_z_tray = angle_z;
                                BlueCrystalCookingPlugin.Instance.Wait(0.5f, () =>
                                {
                                    Transform tray = BlueCrystalCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                                    if (tray != null)
                                    {
                                        BlueCrystalCookingPlugin.Instance.freezingTrays.Add(new FreezingTrayObject(tray, pos, ownerTray, groupTray, angle_x_tray, angle_y_tray, angle_z_tray, 0));
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }

        public static void Update()
        {
            foreach (var tray in BlueCrystalCookingPlugin.Instance.freezingTrays.ToList())
            {
                if (tray.transform == null)
                {
                    BlueCrystalCookingPlugin.Instance.freezingTrays.Remove(tray);
                    continue;
                }

                bool hasPower = !BlueCrystalCookingPlugin.Instance.Configuration.Instance.FreezerNeedsPower;
                if (!hasPower)
                {
                    var generators = PowerTool.checkGenerators(tray.pos, PowerTool.MAX_POWER_RANGE, ushort.MaxValue);
                    if (generators != null)
                    {
                        foreach (var generator in generators)
                        {
                            if (generator != null && generator.fuel > 0 && generator.isPowered && generator.wirerange >= (tray.pos - generator.transform.position).magnitude)
                            {
                                hasPower = true;
                                break;
                            }
                        }
                    }
                }

                if (hasPower)
                {
                    tray.freezingSeconds += 1;
                    if (tray.freezingSeconds >= BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayFreezingTimeSecs)
                    {
                        BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(tray.transform);
                        if (drop != null && BarricadeManager.tryGetRegion(tray.transform, out byte x, out byte y, out ushort plant, out _))
                        {
                            BarricadeManager.destroyBarricade(drop, x, y, plant);

                            Barricade frozenTray = BlueCrystalCookingPlugin.CreateBarricade(BlueCrystalCookingPlugin.Instance.Configuration.Instance.FrozenTrayId);
                            if (frozenTray != null)
                            {
                                BarricadeManager.dropBarricade(frozenTray, null, tray.pos, tray.angle_x, tray.angle_y, tray.angle_z, tray.owner, tray.group);
                            }

                            if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBlueCrystalFreezeEffect)
                            {
                                BlueCrystalCookingPlugin.TriggerEffect(
                                    BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalFreezeEffectId,
                                    10f,
                                    tray.pos);
                            }
                        }
                        BlueCrystalCookingPlugin.Instance.freezingTrays.Remove(tray);
                    }
                }
            }
        }
    }
}
