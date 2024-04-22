using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Enums;
using StardewValley;
using HarmonyLib;
using System.Threading;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewModdingAPI.Utilities;

namespace Pravoloxinone
{
    public class ModEntry
        : Mod
    {
        private static readonly string[] debuffs = { "12", "14", "17", "25", "26", "27" };
        private static IMonitor monitor;
        private static IModHelper helper;
        public static readonly PerScreen<bool> deathbypravoloxinone = new PerScreen<bool>();
        public override void Entry(IModHelper helper)
        {
            monitor = this.Monitor;
            ModEntry.helper = this.Helper;

            this.Helper.Events.Content.AssetRequested += this.AssetRequested;
            i18n.gethelpers(helper.Translation);

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
              original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doneEating)),
              postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.doneEating_Postfix))
          );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkForEvents)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.checkForEvents_Prefix))
          );

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.exitEvent)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.exitEvent_Postfix))
         );
        }

        private void AssetRequested(object sender, AssetRequestedEventArgs e)
        {

            if (e.NameWithoutLocale.IsEquivalentTo("Data\\Objects"))
            {
                e.Edit(asset =>
                {
                    // Why does this non-DRY method not give me null refernce exceptions? But creating a new instance does?
                    string texturename = this.Helper.ModContent.GetInternalAssetName("assets/Pravoloxinone.png").Name;
                    var objects = asset.AsDictionary<string, StardewValley.GameData.Objects.ObjectData>();

                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"] = new StardewValley.GameData.Objects.ObjectData();
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].Name = "TheMightyAmondee.Pravoloxinone/Pravoloxinone";
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].DisplayName = i18n.string_Pravoloxinone();
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].Description = i18n.string_Pravoloxinone_Description();
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].Type = "Crafting";
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].Category = 0;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].Price = 1000;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].Edibility = 20;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].IsDrink = true;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].Buffs = null;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].GeodeDropsDefaultItems = false;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].GeodeDrops = null;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].ArtifactSpotChances = null;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].ExcludeFromFishingCollection = false;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].ExcludeFromShippingCollection = false;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].ExcludeFromRandomSale = true;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].Texture = texturename;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].SpriteIndex = 0;
                    objects.Data["TheMightyAmondee.Pravoloxinone_Pravoloxinone"].ContextTags = new List<string> { "color_green", "medicine_item" };

                });
                  
            }

            else if (e.NameWithoutLocale.IsEquivalentTo("Data\\CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var recipes = asset.AsDictionary<string,string>();
                    recipes.Data[i18n.string_Pravoloxinone()] = "349 1 351 1 306 1/Field/TheMightyAmondee.Pravoloxinone_Pravoloxinone/false/null";
                });

            }

            else if (e.NameWithoutLocale.IsEquivalentTo("Data\\mail"))
            {
                e.Edit(asset =>
                {
                    var recipes = asset.AsDictionary<string, string>();
                    recipes.Data["PravoloxinoneRecipe"] = $"{i18n.string_HarveyMail()}%item craftingRecipe {i18n.string_Pravoloxinone()} %%[#]Pravoloxinone recipe";
                });

            }

            else if (e.NameWithoutLocale.IsEquivalentTo("Data\\Shops"))
            {
                e.Edit(asset =>
                {
                    var shops = asset.AsDictionary<string, StardewValley.GameData.Shops.ShopData>();
                    var pravoloxinoneitem = new StardewValley.GameData.Shops.ShopItemData()
                    {
                        Price = 2000,
                        Id = "TheMightyAmondee.Pravoloxinone_Pravoloxinone",
                        ItemId = "TheMightyAmondee.Pravoloxinone_Pravoloxinone",
                        Condition = "PLAYER_HAS_MAIL Current PravoloxinoneRecipe Received"
                    };

                    shops.Data["Hospital"].Items.Add(pravoloxinoneitem);
                });

            }
        } 
        
        public static bool checkForEvents_Prefix(GameLocation __instance)
        {
            try
            {
                if (Game1.killScreen == true && Game1.eventUp == false && deathbypravoloxinone.Value == true)
                {
                    var events = helper.ModContent.Load<Dictionary<string, string>>("assets\\Events.json");
                    __instance.currentEvent = new Event(string.Format(events["PlayerKilled_Pravoloxinone"], i18n.string_HarveySpeak1(), i18n.string_HarveySpeak2(), i18n.string_HarveySpeak3()));
                    deathbypravoloxinone.Value = false;

                    if (__instance.currentEvent != null)
                    {
                        Game1.eventUp = true;
                    }
                    Game1.changeMusicTrack("none", track_interruptable: true);
                    Game1.killScreen = false;
                    Game1.player.health = 10;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed in {nameof(checkForEvents_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
           
        }

        public static void exitEvent_Postfix(Event __instance)
        {
            try
            {
                if(__instance.id == "57" && Game1.player.mailForTomorrow.Contains("PravoloxinoneRecipe") == false && Game1.player.mailReceived.Contains("PravoloxinoneRecipe") == false)
                {
                    Game1.player.mailForTomorrow.Add("PravoloxinoneRecipe");
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed in {nameof(exitEvent_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void doneEating_Postfix(Farmer __instance)
        {
            try
            {
                // Return early if nothing was consumed
                if (__instance.itemToEat == null)
                {
                    return;
                }

                StardewValley.Object consumed = __instance.itemToEat as StardewValley.Object;

                //Is item just consumed pravoloxinone?
                if (__instance.IsLocalPlayer && consumed != null && consumed.ItemId == "TheMightyAmondee.Pravoloxinone_Pravoloxinone")
                {
                    // Yes, try and apply effects
                    monitor.Log($"Pravoloxinone was consumed... I hope you're lucky");
                    TryApplyPravoloxinoneEffects();
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed in {nameof(doneEating_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void TryApplyPravoloxinoneEffects()
        {
            Random random = new Random();

            var chanceofeffect = random.NextDouble();

            // Is random number returned lower than 0.2?
            if (chanceofeffect < 0.2)
            {
                // Yes, Apply a random debuff from the list, also randomly selected for 60 seconds
                var applieddebuff = new Buff(random.ChooseFrom(debuffs))
                {
                    totalMillisecondsDuration = 60000,
                    millisecondsDuration = 60000,
                    source = i18n.string_Pravoloxinone(),
                    displaySource = i18n.string_Pravoloxinone()
                };
                Game1.player.applyBuff(applieddebuff);
            }

            // Not lower than 0.3? Well is the returned number lower than 0.85?
            else if (chanceofeffect < 0.85)
            {
                // Yes, apply a randomw buff to a randomly selected skill, of a random strength for 5 minutes
                var whichbuff = random.Next(0, 10);
                var buffstrength = random.Next(1, 4);
                var buffeffect = new BuffEffects();
                switch (whichbuff)
                {
                    case 0:
                        buffeffect.FarmingLevel.Value = buffstrength;
                        break;
                    case 1:
                        buffeffect.FishingLevel.Value = buffstrength;
                        break;
                    case 2:
                        buffeffect.MiningLevel.Value = buffstrength;
                        break;
                    case 3:
                        buffeffect.LuckLevel.Value = buffstrength;
                        break;
                    case 4:
                        buffeffect.ForagingLevel.Value = buffstrength;
                        break;
                    case 5:
                        buffeffect.MaxStamina.Value = buffstrength;
                        break;
                    case 6:
                        buffeffect.MagneticRadius.Value = buffstrength;
                        break;
                    case 7:
                        buffeffect.Speed.Value = buffstrength;
                        break;
                    case 8:
                        buffeffect.Defense.Value = buffstrength;
                        break;
                    case 9:
                        buffeffect.Attack.Value = buffstrength;
                        break;
                }
                var appliedbuff = new Buff("TheMightyAmondee.Buff.Pravoloxinone", i18n.string_Pravoloxinone(), i18n.string_Pravoloxinone(), 600000, effects: buffeffect, displayName: i18n.string_Pravoloxinone_Buff());
                Game1.player.applyBuff(appliedbuff);
            }

            // Not lower than 0.85? Well is the returned number less than to 0.95?
            else if (chanceofeffect < 0.95)
            {
                // Yes, subtract a random amount of health between 10 and half of current health
                var healthtolose = random.Next(10, Game1.player.health / 2);
                // Lose determined health, or set health to 1 if player would die
                Game1.player.health -= Math.Min(Game1.player.health - 1, healthtolose);
                if (Game1.player.health <= 0)
                {
                    deathbypravoloxinone.Value = true;
                }
            }
            // None? Alright...
            else
            {
                // Yes, kill player
                Game1.player.health = 0;
                deathbypravoloxinone.Value = true;
            }
        }
    }
}