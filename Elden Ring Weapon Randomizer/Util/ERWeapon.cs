using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elden_Ring_Weapon_Randomizer
{
    internal class ERWeapon : ERItem
    {
        public enum Infusion
        {
            Standard = 000,
            Heavy = 100,
            Keen = 200,
            Quality = 300,
            Fire = 400,
            FlameArt = 500,
            Lightning = 600,
            Sacred = 700,
            Magic = 800,
            Cold = 900,
            Posion = 1000,
            Blood = 1100,
            Occult = 1200,
        };
        public int RealID { get; set; }
        public bool Infusible { get; set; }
        public bool Unique { get; set; }
        public int SwordArtId { get; set; }
        public ERWeapon(string config, bool infusible) : base(config) 
        {
            RealID = Util.DeleteFromEnd(ID, 3);
            Infusible = infusible;
        }
    }
}
