using MelonLoader;

namespace ReplantedArchipelago
{
    public class APSettings
    {
        private static MelonPreferences_Category category;

        public static MelonPreferences_Entry<string> Host;
        public static MelonPreferences_Entry<string> SlotName;
        public static MelonPreferences_Entry<string> Password;

        public static void Init()
        {
            category = MelonPreferences.CreateCategory("Archipelago Connection");
            Host = category.CreateEntry("Host", "archipelago.gg:");
            SlotName = category.CreateEntry("SlotName", "");
            Password = category.CreateEntry("Password", "");
        }

        public static void Save()
        {
            MelonPreferences.Save();
        }
    }
}
