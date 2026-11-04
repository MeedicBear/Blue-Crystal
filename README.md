# Blue Crystal Cooking

An interactive drug production and cooking plugin for Unturned servers running RocketMod. Players can set up cooking stations using barrels, mix required chemical ingredients, freeze the liquid trays in powered freezers, crush frozen trays into bags of Blue Crystal, and consume them for temporary movement/jump speed buffs.

---

## 📖 Description & Workflow

1. **Place a Barrel**:
   - Place down the designated barrel object (`BarrelObjectId`, default: `10102`).
2. **Add Ingredients**:
   - Place/deploy each required chemical ingredient (`drugIngredientIds`, default: `10103`, `10104`, `10105`) on top of the barrel. The ingredients are automatically consumed and stored into the barrel's state.
3. **Stir the Mix**:
   - Punch the barrel (left or right punch gesture).
   - Once all ingredients are inside, each punch adds mixing progress (`StirProgressAddPercentage`, default: `10%`).
   - When 100% progress is reached, the barrel yields a tray of liquid blue crystal (`BlueCrystalTrayId` / `LiquidTrayId`, default: `10106`).
4. **Freeze into Crystal**:
   - Place the liquid tray inside or on top of a freezer unit (`FreezerId`, default: `10101`).
   - If `FreezerNeedsPower` is enabled, ensure an active generator with fuel is within range.
   - After the freezing duration (`BlueCrystalTrayFreezingTimeSecs`, default: `5` seconds), the tray solidifies into a frozen crystal tray (`FrozenTrayId`, default: `10107`).
5. **Harvest & Consume**:
   - Punch the frozen tray to shatter it into multiple bags of Blue Crystal (`BlueCrystalBagId`, default: `10108`, amount randomized between `BlueCrystalBagsAmountMin` and `BlueCrystalBagsAmountMax`).
   - Consuming a bag grants custom speed and jump boosts for a configurable duration.

---

## ⚡ Commands

*This plugin does not register any chat or console commands.* All cooking interactions are handled dynamically via in-game actions:
- Placing barricades/ingredients on cooking stations.
- Punch gestures (punching barrels to stir, punching frozen trays to harvest).
- Consuming the resulting drug items.

---

## 🔒 Permissions

*There are no specific RocketMod permission nodes required.* 
Any player permitted to place barricades and interact with structures can engage in the cooking process unless restricted by other zone/protection plugins.

---

## ⚙️ Configuration

The configuration file is automatically generated at `Plugins/BlueCrystalCooking/BlueCrystalCooking.configuration.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<BlueCrystalCookingConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <!-- Notification / Chat Icon -->
  <IconImageUrl>https://i.imgur.com/fpl2UL7.png</IconImageUrl>

  <!-- Cooking Station IDs -->
  <BarrelObjectId>10102</BarrelObjectId>
  <drugIngredientIds>
    <Id>10103</Id>
    <Id>10104</Id>
    <Id>10105</Id>
  </drugIngredientIds>

  <!-- Stirring & Processing -->
  <BarrelStirEffectId>76</BarrelStirEffectId>
  <EnableBarrelStirEffect>true</EnableBarrelStirEffect>
  <StirProgressAddPercentage>10</StirProgressAddPercentage>

  <!-- Trays & Freezer -->
  <BlueCrystalTrayId>10106</BlueCrystalTrayId>
  <LiquidTrayId>10106</LiquidTrayId>
  <FrozenTrayId>10107</FrozenTrayId>
  <FreezerId>10101</FreezerId>
  <FreezerNeedsPower>true</FreezerNeedsPower>
  <BlueCrystalTrayFreezingTimeSecs>5</BlueCrystalTrayFreezingTimeSecs>
  <BlueCrystalFreezeEffectId>63</BlueCrystalFreezeEffectId>
  <EnableBlueCrystalFreezeEffect>true</EnableBlueCrystalFreezeEffect>

  <!-- Yield & Harvesting -->
  <BlueCrystalBagId>10108</BlueCrystalBagId>
  <BlueCrystalBagsAmountMin>2</BlueCrystalBagsAmountMin>
  <BlueCrystalBagsAmountMax>5</BlueCrystalBagsAmountMax>

  <!-- Drug Effects -->
  <UseDrugEffectSpeed>true</UseDrugEffectSpeed>
  <DrugEffectSpeedMultiplier>2</DrugEffectSpeedMultiplier>
  <UseDrugEffectJump>true</UseDrugEffectJump>
  <DrugEffectJumpMultiplier>2</DrugEffectJumpMultiplier>
  <DrugEffectDurationSecs>30</DrugEffectDurationSecs>
</BlueCrystalCookingConfiguration>
```

---

## 🌐 Localization (Translations)

Default translations can be customized in `Plugins/BlueCrystalCooking/BlueCrystalCooking.en.translation.xml`:

| Key | Default Text |
| :--- | :--- |
| `not_enough_ingredients` | `There are <color=#ff3c19>not enough ingredients</color> in the barrel to stir them into blue crystal.` |
| `ingredient_added` | `You have <color=#75ff19>added {0}</color> to the barrel.` |
| `stir_successful` | `You have <color=#75ff19>successfully mixed</color> the ingredients into a tray filled with <color=#1969ff>liquid blue crystal</color>.` |
| `bluecrystalbags_obtained` | `You have <color=#75ff19>successfully obtained {0} bags</color> filled with <color=#1969ff>blue crystal</color>.` |
