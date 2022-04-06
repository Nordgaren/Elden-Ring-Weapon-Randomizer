using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Infusion = Elden_Ring_Weapon_Randomizer.ERWeapon.Infusion;
using WeaponType = Elden_Ring_Weapon_Randomizer.ERWeapon.WeaponType;

namespace Elden_Ring_Weapon_Randomizer
{
    class ERGem
    {
        public static List<ERGem> Gems;
        private static Regex gemEntryRx = new Regex(@"^\s*(?<id>\S+)\s+(?<name>.*)$");

        public string Name;
        public int ID;
        public int SwordArtID;
        public byte DefaultWeaponAttr;
        public List<Infusion> Infusions
        {
            get { return GetInfusions(); }
        }

        private List<Infusion> GetInfusions()
        {
            switch (DefaultWeaponAttr)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return AllInfusions;
                case 4:
                case 5:
                    var fire = new List<Infusion>(Base);
                    fire.AddRange(Fire);
                    return fire;
                case 6:
                case 7:
                    var sacred = new List<Infusion>(Base);
                    sacred.AddRange(Sacred);
                    return sacred;
                case 8:
                case 9:
                    var magic = new List<Infusion>(Base);
                    magic.AddRange(Sacred);
                    return magic;
                case 10:
                case 11:
                case 12:
                    var occult = new List<Infusion>(Base);
                    occult.AddRange(Sacred);
                    return occult;
                default:
                    return new List<Infusion>();
            }
        }

        public List<WeaponType> WeaponTypes = new List<WeaponType>();

        public ERGem(string config)
        {
            Match itemEntry = gemEntryRx.Match(config);
            Name = itemEntry.Groups["name"].Value.Replace("\r", "");
            ID = Convert.ToInt32(itemEntry.Groups["id"].Value);
        }

        public static void GetGems()
        {
            string result = Util.GetTxtResource("Resources/Weapons/Gems.txt");
            Gems = new List<ERGem>();

            foreach (string line in result.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!line.Contains("//")) //determine if line is a valid resource or not
                {
                    Gems.Add(new ERGem(line));
                }
            };
        }

        public static List<WeaponType> Weapons = Enum.GetValues(typeof(WeaponType)).Cast<WeaponType>().ToList();

        public static List<Infusion> AllInfusions = Enum.GetValues(typeof(Infusion)).Cast<Infusion>().ToList();

        public static List<Infusion> Base = new List<Infusion>() 
        {
            Infusion.Standard,
            Infusion.Heavy,
            Infusion.Keen,
            Infusion.Quality
        };

        public static List<Infusion> Magic = new List<Infusion>()
        { 
            Infusion.Cold,
            Infusion.Magic
        };

        public static List<Infusion> Fire = new List<Infusion>()
        {
            Infusion.Fire,
            Infusion.FlameArt
        };

        public static List<Infusion> Sacred = new List<Infusion>()
        {
            Infusion.Lightning,
            Infusion.Sacred
        };

        public static List<Infusion> Occult = new List<Infusion>()
        {
            Infusion.Posion,
            Infusion.Blood,
            Infusion.Occult
        };
    }
}
