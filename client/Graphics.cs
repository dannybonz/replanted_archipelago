using Archipelago.MultiClient.Net.Models;
using Il2CppReloaded.DataModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ReplantedArchipelago
{
    public class Graphics
    {
        public static Dictionary<string, AddressableSpriteValueModel> spriteModels = new Dictionary<string, AddressableSpriteValueModel>();
        public static string[] spriteModelGuids = { "98123da20d3f9e849b589d3c93b1f1ff", "e168cb13e5f9f9d4ebe670d29eef97fd", "27f638cf93497ea4ebec247605f7a5c1", "6a719f1058ed1674791239ad95867751", "cf87720f4d7c4664d8e291dc39ba5374", "57d9181facd031f43a6e8496a79ef947", "b86eea166023cfe4f83495f20acbd520", "fe5391fc50f02a544ad78e926b171830", "e27c73e451154d94889daee6c05f481e", "8e0949f5ac27758408ec48f94fe016d4", "1b733a48cd638f94fae3e98b575ec984", "f2df806336ad04d44a568bf118393da6", "e53e9ee1169610d4fb9157ba63addb90", "dae9fb6c5be0643469fa728a9edeb1bd", "67d4698c7e715134ea68bb4b464c10a5", "005fbbf458f8f5f47a8a3ce9ba3ca712", "8cd7b1e0269f67944af9925743e7a672", "5fdd50a8dd2df1543b2d05cfa156432a", "df5786327a484cc44a5a918e81f91400", "1885d1d6e0b32584f8c2c80770afe814", "a5638b24fc0d98b488121ab9ee1d49ce", "9a51438e978a2ce4c925963496655da3", "49da1a6b78ff64140a5e2caf002af5c7", "d7be87734fe55c24f9f53a1720ba6cb0", "74c24c594918b2e46b6fe01f5867329d", "75d981afb747c314d985c27925451bc6", "7e15cda91281ff948a872fe786c9a537", "dc567afd8deb48849a435f9175be3254", "0b1890fce9183ca43833464df55dcd5b", "502f66d4a5836e14da79b977b342cbb8", "d4a382e72211172409f474e929a94ca1", "bfc0969ccbe5890419cd53819f8f1dd0", "c78879535d9ecff48a7c4418a4a92779", "48e4fb4fac8d33f46833abd84e711594", "61023f6935dd5bf4f920f8d098bdc5f4", "5f6bb8f9f74eed24f885fc2259fd2ccc", "a84b3bda57f87244ea2120205315cc95", "6e600b8bbd46e2840893204ae406b259", "e7a654214c0bf784fa95f6f42a5d38a8", "b6c4bffd4cec1e3449d01cdde7aa15f3", "bfa7e27a845e1b74895b9413601d768c", "941680cb467b17941b912cf117ff5555", "ed865106ffcb9924b9a02329946f6a5f", "78a8ab05ec9b5c64498088b684bc7f82", "3e8e06d91dec6dd4c9af9c5036425a50", "7dae3f3bb58f688478c732d226bc22c5", "271d6adaab31c6d48ac11bdba26ec66d", "cfb66f31e595a7b43adaa920304cf096", "a1561706a4302544e8019173e346faf9", "5acaceb04902b7c4aa7691e05f338325", "0e8a30200f55a714b8bb73813f8e3a45", "78582a4b391e618419d618bc5416188c", "9cd575a6dbf321448af6c5335c7870c7", "fc060dd1b0ba00943843df1307a13729", "bd60209eac4d8e641b6fdfb39f716c31", "023a387fdbba1394e91c3f1d437c80a5", "7b16b057b9e4bb1438125bad4ad4b696", "0b92fa00365e8b44f8ae7bb53f7e5941", "509709605b60c81498439aaabf1acb71", "5a34792d0bbe95a45bc0f03b0ee23be8", "c3a31cdb98c2ccd459be0719d876f690", "980e1114ba8bd93419396716f33a79ca", "cc5eefd8e7cf01249bdf81682b9d6001", "0e9781fb590ab3e439a2dde911dfaef4", "7140f470945ed85468abea4009f63953", "c1a4caa2e0cf15140857e6e059743b66", "0ec86d5cec208d4488e1dad9743a816b", "061920ac66dbe8546987393f79b81fdb", "71c17aa42149bfb47ad73ba4365decad", "dc6a4e156515ab54ba2a0c59f9bb8f37", "2e2015a71f872f0458b9b44d35144391" };
        public static Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
        public static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
        public static Dictionary<string, string> customSprites = new Dictionary<string, string>()
        {
            { "Repick", "ReplantedArchipelago.Assets.Repick.png" },
            { "Archipelago", "ReplantedArchipelago.Assets.Archipelago.png" },
            { "Key", "ReplantedArchipelago.Assets.Key.png" }
        };

        public class TextureAtlasMapping
        {
            public string AtlasName { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public bool Rotated { get; set; }
        }

        public static Dictionary<string, TextureAtlasMapping> atlasSprites = new Dictionary<string, TextureAtlasMapping>()
        {
            { "leafbunch1", new TextureAtlasMapping { AtlasName = "TreeofWisdom", X = 3533, Y = 2265, Width = 670, Height = 650, Rotated = true  } },
            { "Diamond", new TextureAtlasMapping { AtlasName = "Diamond", X = 2, Y = 82, Width = 340, Height = 260, Rotated = false  } }
        };

        public static Sprite GetGraphic(string name)
        {
            if (!cachedSprites.ContainsKey(name))
            {
                if (customSprites.ContainsKey(name))
                {
                    cachedSprites[name] = LoadEmbeddedSprite(customSprites[name]);
                    return cachedSprites[name];
                }
                else if (atlasSprites.ContainsKey(name))
                {
                    cachedSprites[name] = GetSpriteFromAtlas(name);
                    return cachedSprites[name];
                }
                else
                {
                    Sprite desiredSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == name);
                    if (desiredSprite == null)
                    {
                        return null;
                    }
                    cachedSprites[name] = desiredSprite;
                }
            }
            return cachedSprites[name];
        }

        public static byte[] LoadEmbeddedResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            MemoryStream memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public static Sprite LoadEmbeddedSprite(string resourceName)
        {
            byte[] data = LoadEmbeddedResource(resourceName);

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, true);
            texture.LoadImage(data);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply(true);

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static Texture2D GetTexture(string name)
        {
            if (!cachedTextures.ContainsKey(name))
            {
                Texture2D desiredTexture = Resources.FindObjectsOfTypeAll<Texture2D>().FirstOrDefault(t => t.name == name);
                if (desiredTexture == null)
                {
                    return null;
                }
                cachedTextures[name] = desiredTexture;
            }
            return cachedTextures[name];
        }

        public static Sprite GetSpriteFromAtlas(string name)
        {
            TextureAtlasMapping textureAtlasMapping = atlasSprites[name];
            Texture2D atlasTexture = GetTexture(atlasSprites[name].AtlasName);

            RenderTexture renderTexture = RenderTexture.GetTemporary(atlasTexture.width, atlasTexture.height, 0, RenderTextureFormat.ARGB32); //Create a temporary canvas matching the dimensions of the atlas
            UnityEngine.Graphics.Blit(atlasTexture, renderTexture); //Draw atlas onto canvas
            RenderTexture previous = RenderTexture.active; //Store previously used canvas
            RenderTexture.active = renderTexture; //Set active canvas to our custom canvas

            Texture2D croppedAtlasTexture = new Texture2D(textureAtlasMapping.Width, textureAtlasMapping.Height, TextureFormat.RGBA32, false); //Create new Texture2D to store the cropped image
            croppedAtlasTexture.ReadPixels(new Rect(textureAtlasMapping.X, atlasTexture.height - textureAtlasMapping.Y - textureAtlasMapping.Height, textureAtlasMapping.Width, textureAtlasMapping.Height), 0, 0); //Copy pixels from currently active RenderTexture (i.e. the whole atlas image) onto the croppedAtlasTexture
            croppedAtlasTexture.Apply();

            RenderTexture.active = previous; //Return active canvas to whatever it was before this function ran
            RenderTexture.ReleaseTemporary(renderTexture);

            if (textureAtlasMapping.Rotated)
            {
                return Sprite.Create(RotateTexture(croppedAtlasTexture), new Rect(0, 0, croppedAtlasTexture.height, croppedAtlasTexture.width), new Vector2(0.5f, 0.5f));
            }
            else
            {
                return Sprite.Create(croppedAtlasTexture, new Rect(0, 0, croppedAtlasTexture.width, croppedAtlasTexture.height), new Vector2(0.5f, 0.5f));
            }
        }

        public static Texture2D RotateTexture(Texture2D texture)
        {
            int w = texture.width;
            int h = texture.height;
            Texture2D rotatedTexture = new Texture2D(h, w, TextureFormat.RGBA32, false); //Create new texture to store rotated texture on (swaps width and height)

            for (int y = 0; y < h; y++) //Loop through every pixel
            {
                for (int x = 0; x < w; x++)
                {
                    rotatedTexture.SetPixel(y, w - 1 - x, texture.GetPixel(x, y)); //Set pixel based on rotation
                }
            }
            rotatedTexture.Apply();

            return rotatedTexture;
        }

        public static void RefreshGraphics()
        {
            cachedSprites.Clear();
            cachedTextures.Clear();
            foreach (string guid in spriteModelGuids)
            {
                AssetReferenceSprite assetReferenceSprite = new AssetReferenceSprite(guid);
                AddressableSpriteValueModel spriteModel = new AddressableSpriteValueModel();
                spriteModel.m_reference = assetReferenceSprite;
                spriteModel.LoadSpriteReference();
                spriteModels[guid] = spriteModel;
            }
        }

        public static void LoadCustomGraphics()
        {
            foreach (KeyValuePair<string, string> customSprite in customSprites)
            {
                GetGraphic(customSprite.Key);
            }
        }

        public static Dictionary<long, string> itemIdSpriteName = new Dictionary<long, string> //Can be dropped as end-of-level awards/triggers award screen
        {
            { 2, "SPR_CarKeys" },
            { 3, "SPR_Store_Item_SeedPackets" },
            { 4, "SPR_ShovelHiRes" },
            { 5, "SPR_Almanac" },
            { 6, "SPR_WateringCan" },
            { 7, "Key" },
            { 8, "Key" },
            { 9, "Key" },
            { 10, "Key" },
            { 11, "Key" },
            { 12, "SPR_Store_Item_RootCleaner" },
            { 13, "SPR_Store_Item_PoolCleaner" },
            { 16, "SPR_Store_Item_FirstAid" },
            { 17, "SPR_Store_Item_Rake" },

            { 20, "Key" },
            { 21, "Key" },
            { 22, "Key" },
            { 23, "Key" },
            { 24, "Key" },
            { 25, "Key" },
            { 26, "SPR_ShovelHiRes" },
            { 27, "SPR_Taco" },

            { 30, "SPR_Zen_Icon_Glove" },
            { 31, "SPR_Zen_Icon_WateringCanGold" },
            { 32, "SPR_Zen_Icon_Music" },
            { 33, "SPR_Store_Item_Stinky" },
            { 34, "SPR_Zen_Icon_Wheelbarrow" },

            { 64, "SPR_Scary_Pot_4" },
            { 65, "TreeFood" },
            { 66, "Fertilizer" },
            { 67, "bug_spray" },
            { 68, "Chocolate" },

            { 70, "SPR_Scary_Pot_5" },
            { 71, "SPR_Scary_Pot_5" },
            { 72, "SPR_Scary_Pot_5" },
            { 73, "SPR_Scary_Pot_5" },
            { 74, "SPR_Scary_Pot_5" },
            { 75, "SPR_Scary_Pot_5" },
            { 76, "SPR_Scary_Pot_5" },

            { 100, "SPR_Almanac_Seedpackets_PeaShooter" },
            { 101, "SPR_Almanac_Seedpackets_Sunflower" },
            { 102, "SPR_Almanac_Seedpackets_CherryBomb" },
            { 103, "SPR_Almanac_Seedpackets_Wall-Nut" },
            { 104, "SPR_Almanac_Seedpackets_PotatoMine" },
            { 105, "SPR_Almanac_Seedpackets_SnowPea" },
            { 106, "SPR_Almanac_Seedpackets_Chomper" },
            { 107, "SPR_Almanac_Seedpackets_Repeater" },
            { 108, "SPR_Almanac_Seedpackets_Puff-shroom" },
            { 109, "SPR_Almanac_Seedpackets_Sun-shroom" },
            { 110, "SPR_Almanac_Seedpackets_Fume-shroom" },
            { 111, "SPR_Almanac_Seedpackets_GraveBuster" },
            { 112, "SPR_Almanac_Seedpackets_Hypno-shroom" },
            { 113, "SPR_Almanac_Seedpackets_Scaredy-shroom" },
            { 114, "SPR_Almanac_Seedpackets_Ice-shroom" },
            { 115, "SPR_Almanac_Seedpackets_Doom-shroom" },
            { 116, "SPR_Almanac_Seedpackets_LilyPad" },
            { 117, "SPR_Almanac_Seedpackets_Squash" },
            { 118, "SPR_Almanac_Seedpackets_Threepeater" },
            { 119, "SPR_Almanac_Seedpackets_TangleKelp" },
            { 120, "SPR_Almanac_Seedpackets_Jalapeno" },
            { 121, "SPR_Almanac_Seedpackets_Spikeweed" },
            { 122, "SPR_Almanac_Seedpackets_Torchwood" },
            { 123, "SPR_Almanac_Seedpackets_Tall-nut" },
            { 124, "SPR_Almanac_Seedpackets_Sea-shroom" },
            { 125, "SPR_Almanac_Seedpackets_Plantern" },
            { 126, "SPR_Almanac_Seedpackets_Cactus" },
            { 127, "SPR_Almanac_Seedpackets_Blover" },
            { 128, "SPR_Almanac_Seedpackets_SplitPea" },
            { 129, "SPR_Almanac_Seedpackets_Starfruit" },
            { 130, "SPR_Almanac_Seedpackets_Pumpkin" },
            { 131, "SPR_Almanac_Seedpackets_Magnet-shroom" },
            { 132, "SPR_Almanac_Seedpackets_Cabbage-pult" },
            { 133, "SPR_Almanac_Seedpackets_FlowerPot" },
            { 134, "SPR_Almanac_Seedpackets_Kernel-pult" },
            { 135, "SPR_Almanac_Seedpackets_CoffeeBean" },
            { 136, "SPR_Almanac_Seedpackets_Garlic" },
            { 137, "SPR_Almanac_Seedpackets_UmbrellaLeaf" },
            { 138, "SPR_Almanac_Seedpackets_Marigold" },
            { 139, "SPR_Almanac_Seedpackets_Melon-pult" },
            { 140, "SPR_Almanac_Seedpackets_GatlingPea" },
            { 141, "SPR_Almanac_Seedpackets_TwinSunflower" },
            { 142, "SPR_Almanac_Seedpackets_Gloom-shroom" },
            { 143, "SPR_Almanac_Seedpackets_Cattail" },
            { 144, "SPR_Almanac_Seedpackets_WinterMelon" },
            { 145, "SPR_Almanac_Seedpackets_GoldMagnet" },
            { 146, "SPR_Almanac_Seedpackets_Spikerock" },
            { 147, "SPR_Almanac_Seedpackets_CobCannon" },
            { 148, "SPR_Almanac_Seedpackets_Imitater" }
        };

        public static Dictionary<long, string> itemIdStoreImage = new Dictionary<long, string> //Used in store display as a priority over itemIdSpriteName
        {
            { 30, "SPR_Store_Item_GardeningGlove" },
            { 31, "SPR_Store_Item_GoldenWateringCan" },
            { 32, "SPR_Store_Item_Phonograph" },
            { 34, "SPR_Store_Item_Wheelbarrow" },

            { 60, "coin_silver_dollar" },
            { 61, "coin_gold_dollar" },
            { 62, "Diamond" },
            { 65, "SPR_Store_Item_Treefood" },
            { 66, "SPR_Store_Item_Fertilizer" },
            { 67, "SPR_Store_Item_BugSpray" },
        };

        public static Dictionary<string, float> resizeImageForAward = new Dictionary<string, float>
        {
            { "SPR_Present", 0.5f },
            { "Chocolate", 0.0025f },
            { "Fertilizer", 0.0025f },
            { "TreeFood", 0.9f },
            { "bug_spray", 0.0025f },
            { "SPR_Zen_Icon_Glove", 0.9f },
            { "SPR_Zen_Icon_Music", 0.9f },
            { "SPR_Zen_Icon_WateringCanGold", 0.95f },
            { "SPR_Zen_Icon_Wheelbarrow", 0.9f },
            { "SPR_Store_Item_SeedPackets", 0.9f },
            { "SPR_Store_Item_PoolCleaner", 0.9f },
            { "SPR_Store_Item_RootCleaner", 0.9f },
            { "SPR_Store_Item_FirstAid", 0.9f },
            { "SPR_Store_Item_Stinky", 0.9f },
            { "coin_silver_dollar", 0.7f },
            { "coin_gold_dollar", 0.7f },
            { "moneybag", 0.5f },
            { "SPR_Scary_Pot_4", 0.35f },
            { "SPR_Scary_Pot_5", 0.35f },
            { "SPR_Taco", 0.35f },
            { "Key", 0.125f }
        };

        public static (Sprite sprite, float scale) GetSpriteAndScaleForItemDrop(ItemInfo itemInfo)
        {
            if (itemInfo.Player.Slot != APClient.apSession.Players.ActivePlayer.Slot) //Foreign items
            {
                return (GetGraphic("Archipelago"), 0.1f);
            }
            else if (itemInfo.ItemId >= 200 && itemInfo.ItemId < 500) //Level unlocks
            {
                return (GetGraphic("Key"), 0.125f);
            }
            else if (itemIdSpriteName.ContainsKey(itemInfo.ItemId)) //Has a set image
            {
                string spriteName = itemIdSpriteName[itemInfo.ItemId];
                Sprite sprite = GetGraphic(spriteName);

                if (resizeImageForAward.TryGetValue(spriteName, out float resize))
                    return (sprite, resize);

                if (itemInfo.ItemId >= 100 && itemInfo.ItemId < 200)
                    return (sprite, 0.5f);

                return (sprite, 1f);
            }

            //Otherwise, default to present box
            return (GetGraphic("SPR_Present"), 0.5f);
        }
    }
}
