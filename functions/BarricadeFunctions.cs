using SDG.Unturned;
using System.Linq;
using UnityEngine;

namespace Ocelot.BlueCrystalCooking.functions
{
    public static class BarricadeFunctions
    {
        public static void BarricadeDamaged(Transform barricadeTransform, ushort pendingTotalDamage)
        {
            if (barricadeTransform == null)
                return;

            BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(barricadeTransform);
            BarricadeData bData = drop?.GetServersideData();
            if (bData?.barricade?.asset == null)
                return;

            if (bData.barricade.health <= pendingTotalDamage)
            {
                ushort assetId = bData.barricade.asset.id;
                if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId == assetId)
                {
                    BlueCrystalCookingPlugin.Instance.placedBarrelsTransformsIngredients.Remove(barricadeTransform);
                }
                else if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayId == assetId ||
                         BlueCrystalCookingPlugin.Instance.Configuration.Instance.LiquidTrayId == assetId)
                {
                    foreach (var tray in BlueCrystalCookingPlugin.Instance.freezingTrays.ToList())
                    {
                        if (tray.transform == null || tray.transform == barricadeTransform)
                        {
                            BlueCrystalCookingPlugin.Instance.freezingTrays.Remove(tray);
                        }
                    }
                }
            }
        }
    }
}
