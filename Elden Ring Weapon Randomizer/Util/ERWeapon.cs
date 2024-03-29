﻿using System;
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

        public enum WeaponType
        {
            Dagger = 1, //Dagger
            StraightSword = 3, //SwordNormal
            Greatsword = 5, //SwordLarge
            ColossalSword = 7, //SwordGigantic
            CurvedSword = 9, //SabreNormal
            CurvedGreatsword = 11, //SabreLarge
            Katana = 13, //katana
            Twinblade = 14, //SwordDoulbeEdge
            ThrustingSword = 15, //SwordPierce
            HeavyThrustingSword = 16, //RapierHeavy
            Axe = 17, //AxeNormal
            Greataxe = 19, //AxeLarge
            Hammer = 21, //HammerNormal
            GreatHammer = 23, //HammerLarge
            Flail = 24, //Flail
            Spear = 25, //SpearNormal
            SpearHeavy = 27, //SpearLarge Placeholder ID
            GreatSpear = 28, //SpearHeavy
            Halberd = 29, //SpearAxe
            Reaper = 31, //Sickle
            //Unarmed = 33
            Fist = 35, //Knuckle
            Claws = 37, //Claw
            Whip = 39, //Whip
            ColossalWeapon = 41, //Axhammerlarge
            LightBow = 50, //BowSmall
            Bow = 51, //BowNormal
            Greatbow = 53, //BowLarge
            Crossbow = 55, //Clossbow
            Ballista = 56, //Ballista
            GlintstoneStaff = 57, //Staff
            Sorcery = 58, //Sorcery Placeholder ID
            FingerSeal = 61, //Talisman
            SmallShield = 65, //ShieldSmall
            MediumShield = 67, //ShieldNormal
            Greatshield = 69, //SheildLarge
            Torch = 87 //Torch
        }

        public enum AmmoType
        {
            Arrow = 81,
            GreatArrow = 83,
            Bolt = 85,
            BallistaBolt = 86,
        }

        public int SortID { get; set; }
        public int RealID { get; set; }
        public bool Infusible { get; set; }
        public byte[] OriginEquipWep { get; set; } = new byte[0x40];
        public bool Unique { get; set; }
        public short IconID { get; set; }
        public int SwordArtId { get; set; }
        public WeaponType Type { get; set; }
        public byte[] OriginEquipWep16 { get; set; } = new byte[0x28];
        public ERWeapon(string config, bool infusible) : base(config) 
        {
            RealID = Util.DeleteFromEnd(ID, 3);
            Infusible = infusible;
        }

        public ERWeapon()
        {
        }

        internal void Clone(ERWeapon source)
        {
           Name = source.Name;
           ID = source.ID;
           RealID = source.RealID;
           Infusible = source.Infusible;
           Array.Copy(source.OriginEquipWep, OriginEquipWep,source.OriginEquipWep.Length);
           IconID = source.IconID;
           Unique = source.Unique;
           SwordArtId = source.SwordArtId;
           Type = source.Type;
           Array.Copy(source.OriginEquipWep16, OriginEquipWep16, source.OriginEquipWep16.Length);
        }
    }
}
