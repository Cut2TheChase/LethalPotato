using BepInEx;
using HarmonyLib;
using HotPotato.Behaviours;
using LethalLib.Modules;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace HotPotato
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.sigurd.csync")]
    [BepInDependency("evaisa.lethallib")]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "cuttothechase.LethalPotato";
        const string NAME = "Lethal Potato";
        const string VERSION = "1.2.0";

        public static Plugin instance;

        public static PotatoConfig MyConfig { get; private set; }


        void Awake()
        {
            instance = this;

            MyConfig = new PotatoConfig(base.Config);


            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "itemmod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir); 

            Item hotPotato = bundle.LoadAsset<Item>("Assets/Items/HotPotatoItem.asset");
            FancyShake shakeScript = hotPotato.spawnPrefab.AddComponent<FancyShake>();
            CountdownExplosion script = hotPotato.spawnPrefab.AddComponent<CountdownExplosion>();
            script.grabbable = true;
            script.grabbableToEnemies = true;
            script.itemProperties = hotPotato;

            NetworkPrefabs.RegisterNetworkPrefab(hotPotato.spawnPrefab);
            Utilities.FixMixerGroups(hotPotato.spawnPrefab);
            //Items.RegisterScrap(hotPotato, 25, Levels.LevelTypes.All);
            //int rarity = HotPotato.Config.Instance.configRarity.Value;
            Items.RegisterScrap(hotPotato, PotatoConfig.Instance.SCRAP_RARITY, Levels.LevelTypes.All);


            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "Dis potato HAWWWWWT. Gonna get spicy real fast\n\n";
            
            //Items.RegisterShopItem(hotPotato, null, null, node, 0);

            //Apparently need to do this to make netcode stuff work
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }



            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
            Logger.LogInfo("Patched Lethal Potato");

        }
    }
}
