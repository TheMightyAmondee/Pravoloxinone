using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pravoloxinone
{
    internal static class i18n
    {
        private static ITranslationHelper translation;
        public static void gethelpers(ITranslationHelper translation)
        {
            i18n.translation = translation;
        }
        public static string string_Pravoloxinone()
        {
            return i18n.GetTranslation("TheMightyAmondee.Pravoloxinone/Pravoloxinone");
        }
        public static string string_Pravoloxinone_Description()
        {
            return i18n.GetTranslation("TheMightyAmondee.Pravoloxinone/Pravoloxinone_Description");
        }
        public static string string_Pravoloxinone_Buff()
        {
            return i18n.GetTranslation("TheMightyAmondee.Pravoloxinone/Pravoloxinone_Buff");
        }
        public static string string_HarveySpeak1()
        {
            return i18n.GetTranslation("TheMightyAmondee.Pravoloxinone/HarveySpeak1", new { PlayerName = Game1.player.Name });
        }
        public static string string_HarveySpeak2()
        {
            return i18n.GetTranslation("TheMightyAmondee.Pravoloxinone/HarveySpeak2");
        }
        public static string string_HarveySpeak3()
        {
            return i18n.GetTranslation("TheMightyAmondee.Pravoloxinone/HarveySpeak3");
        }
        public static string string_HarveyMail()
        {
            return i18n.GetTranslation("TheMightyAmondee.Pravoloxinone/HarveyMail");
        }

        /// <summary>
        /// Gets the correct translation
        /// </summary>
        /// <param name="key">The translation key</param>
        /// <param name="tokens">Tokens, if any</param>
        /// <returns>The translated string</returns>
        public static Translation GetTranslation(string key, object tokens = null)
        {
            if (i18n.translation == null)
            {
                throw new InvalidOperationException($"You must call {nameof(i18n)}.{nameof(i18n.gethelpers)} from the mod's entry method before reading translations.");
            }

            return i18n.translation.Get(key, tokens);
        }
    }
}
