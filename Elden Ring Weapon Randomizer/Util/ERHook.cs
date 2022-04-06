using PropertyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using Infusion = Elden_Ring_Weapon_Randomizer.ERWeapon.Infusion;
using WeaponType = Elden_Ring_Weapon_Randomizer.ERWeapon.WeaponType;

namespace Elden_Ring_Weapon_Randomizer
{
    internal class ERHook : PHook, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private PHPointer GameDataManSetup { get; set; }
        private PHPointer GameDataMan { get; set; }
        private PHPointer PlayerGameData { get; set; }
        private PHPointer SoloParamRepositorySetup { get; set; }
        private PHPointer SoloParamRepository { get; set; }
        private PHPointer EquipParamWeapon { get; set; }
        private PHPointer EquipParamGem { get; set; }
        private PHPointer EquipItem { get; set; }

        //private PHPointer DurabilityAddr { get; set; }
        //private PHPointer DurabilitySpecialAddr { get; set; }
        public bool Loaded => this?.PlayerGameData?.Resolve() != IntPtr.Zero;
        public ERHook(int refreshInterval, int minLifetime, Func<Process, bool> processSelector)
            : base(refreshInterval, minLifetime, processSelector)
        {
            OnHooked += ERHook_OnHooked;
            GameDataManSetup = RegisterAbsoluteAOB(EROffsets.GameDataManSetupAoB);
            SoloParamRepositorySetup = RegisterAbsoluteAOB(EROffsets.SoloParamRepositorySetupAoB);
            EquipItem = RegisterAbsoluteAOB(EROffsets.EquipItemAoB);

            RHandTimer.Elapsed += RHandTimer_Elapsed;
            RHandTimer.AutoReset = true;

            LHandTimer.Elapsed += LHandTimer_Elapsed;
            LHandTimer.AutoReset = true;
        }
        private void ERHook_OnHooked(object? sender, PHEventArgs e)
        {
            GameDataMan = CreateBasePointer(BasePointerFromSetupPointer(GameDataManSetup));
            PlayerGameData = CreateChildPointer(GameDataMan, EROffsets.PlayerGameData);

            SoloParamRepository = CreateBasePointer(BasePointerFromSetupPointer(SoloParamRepositorySetup));
            EquipParamWeapon = CreateChildPointer(SoloParamRepository, EROffsets.EquipParamWeaponOffset1, EROffsets.EquipParamWeaponOffset2, EROffsets.EquipParamWeaponOffset3);
            EquipParamGem = CreateChildPointer(SoloParamRepository, EROffsets.EquipParamGemOffset1, EROffsets.EquipParamGemOffset2, EROffsets.EquipParamGemOffset3);
            var bytes = new byte[0];
            EquipParamWeaponOffsetDict = BuildOffsetDictionary(EquipParamWeapon, "EQUIP_PARAM_WEAPON_ST", ref bytes);
            EquipParamWeaponBytes = bytes;
            EquipParamGemOffsetDict = BuildOffsetDictionary(EquipParamGem, "RAM_GEM_ST", ref bytes);
            EquipParamGemBytes = bytes;
            GetParams();
            var lol = EquipItem.Resolve();
        }
        private void GetParams()
        {
            foreach (var category in ERItemCategory.All)
            {
                GetWeaponProperties(category);
            }

            foreach (var gem in ERGem.Gems)
            {
                GetGemProperties(gem);
                GetWeapons(gem);
            }
        }
        private void GetWeaponProperties(ERItemCategory category)
        {
            foreach (var weapon in category.Weapons)
            {
                if (!EquipParamWeaponOffsetDict.ContainsKey(weapon.ID))
                {
                    Debug.WriteLine($"{weapon.ID} {weapon.Name}");
                    continue;
                }
                weapon.Unique = BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.MaterialSetID) == 2200;

                if (weapon.Unique)
                    weapon.Infusible = false;

                weapon.SortID = BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.SortID);
                Array.Copy(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.OriginEquipWep, weapon.OriginEquipWep, 0x0, weapon.OriginEquipWep.Length);
                weapon.IconID = BitConverter.ToInt16(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.IconID);
                weapon.SwordArtId = BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.SwordArtsParamId);
                weapon.Type = (ERWeapon.WeaponType)BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.WepType);
                Array.Copy(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.OriginEquipWep16, weapon.OriginEquipWep16, 0x0, weapon.OriginEquipWep16.Length);
            }
        }
        private void GetGemProperties(ERGem gem)
        {
            gem.DefaultWeaponAttr = (byte)BitConverter.ToChar(EquipParamGemBytes, EquipParamGemOffsetDict[gem.ID] + (int)EROffsets.EquipParamGem.Default_WepAttr);
            gem.SwordArtID = BitConverter.ToInt32(EquipParamGemBytes, EquipParamGemOffsetDict[gem.ID] + (int)EROffsets.EquipParamGem.SwordArtsParamId);
        }
        private void GetWeapons(ERGem gem)
        {
            gem.WeaponTypes = new List<WeaponType>();
            var bitField = BitConverter.ToInt64(EquipParamGemBytes, EquipParamGemOffsetDict[gem.ID] + (int)EROffsets.EquipParamGem.CanMountWep_Dagger);
            if (bitField == 0)
                return;

            for (int i = 0; i < ERGem.Weapons.Count; i++)
            {
                if ((bitField & (1L << i)) != 0)
                    gem.WeaponTypes.Add(ERGem.Weapons[i]);
            }
        }

        public Dictionary<int, int> EquipParamWeaponOffsetDict { get; private set; }
        private byte[] EquipParamWeaponBytes { get; set; }
        public Dictionary<int, int> EquipParamGemOffsetDict { get; private set; }
        private byte[] EquipParamGemBytes { get; set; }
        private Dictionary<int, int> BuildOffsetDictionary(PHPointer pointer, string expectedParamName, ref byte[] paramBytes)
        {
            var dictionary = new Dictionary<int, int>();
            var nameOffset = pointer.ReadInt32((int)EROffsets.Param.TotalParamLength);
            var paramName = pointer.ReadString(nameOffset, Encoding.UTF8, 0x18);
            if (paramName != expectedParamName)
                throw new InvalidOperationException($"Incorrect Param Pointer: {expectedParamName}");

            paramBytes = pointer.ReadBytes((int)EROffsets.Param.TotalParamLength, (uint)nameOffset);
            var tableLength = pointer.ReadInt32((int)EROffsets.Param.TableLength);
            var param = 0x40;
            var paramID = 0x0;
            var paramOffset = 0x8;
            var nextParam = 0x18;

            while (param < tableLength)
            {
                var itemID = pointer.ReadInt32(param + paramID);
                var itemParamOffset = pointer.ReadInt32(param + paramOffset);
                dictionary.Add(itemID, itemParamOffset);

                param += nextParam;
            }

            return dictionary;
        }

        public void RestoreParams()
        {
            if (!Hooked)
                return;

            EquipParamWeapon.WriteBytes((int)EROffsets.Param.TotalParamLength, EquipParamWeaponBytes);
        }
        public IntPtr BasePointerFromSetupPointer(PHPointer pointer)
        {
            var readInt = pointer.ReadInt32(EROffsets.BasePtrOffset1);
            return pointer.ReadIntPtr(readInt + EROffsets.BasePtrOffset2);
        }
        public int Level => PlayerGameData.ReadInt32((int)EROffsets.Player.Level);
        public string LevelString => PlayerGameData?.ReadInt32((int)EROffsets.Player.Level).ToString() ?? "";
        public byte ArmStyle
        {
            get => PlayerGameData.ReadByte((int)EROffsets.Weapons.ArmStyle);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteByte((int)EROffsets.Weapons.ArmStyle, value);
            }
        }
        public int CurrWepSlotOffsetLeft
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.CurrWepSlotOffsetLeft);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.CurrWepSlotOffsetLeft, value);
            }
        }
        public int CurrWepSlotOffsetRight
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.CurrWepSlotOffsetRight);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.CurrWepSlotOffsetRight, value);
            }
        }
        public int RHandWeapon1
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.RHandWeapon1);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.RHandWeapon1, value);
            }
        }
        public int RHandWeapon2
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.RHandWeapon2);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.RHandWeapon2, value);
            }
        }
        public int RHandWeapon3
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.RHandWeapon3);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.RHandWeapon3, value);
            }
        }
        public int LHandWeapon1
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.LHandWeapon1);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.LHandWeapon1, value);
            }
        }
        public int LHandWeapon2
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.LHandWeapon2);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.LHandWeapon2, value);
            }
        }
        public int LHandWeapon3
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.LHandWeapon3);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.LHandWeapon3, value);
            }
        }
        public int Arrow1
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.Arrow1);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.Arrow1, value);
            }
        }
        public int Arrow2
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.Arrow2);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.Arrow2, value);
            }
        }
        public int Bolt1
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.Bolt1);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.Bolt1, value);
            }
        }
        public int Bolt2
        {
            get => PlayerGameData.ReadInt32((int)EROffsets.Weapons.Bolt2);
            set
            {
                if (!Loaded)
                    return;
                PlayerGameData.WriteInt32((int)EROffsets.Weapons.Bolt2, value);
            }
        }

        private int OGRHandWeapon1 { get; set; }
        private PHPointer OGRHandWeapon1Param 
        { 
            get => CreateBasePointer(EquipParamWeapon.Resolve() + EquipParamWeaponOffsetDict[OGRHandWeapon1]);
        }
        private int OGRHandSortID
        {
            get => OGRHandWeapon1Param?.ReadInt32((int)EROffsets.EquipParamWeapon.SortID) ?? 0;
            set => OGRHandWeapon1Param?.WriteInt32((int)EROffsets.EquipParamWeapon.SortID, value);
        }
        private int OGRHandWeapon1SwordArtID
        {
            get => OGRHandWeapon1Param?.ReadInt32((int)EROffsets.EquipParamWeapon.SwordArtsParamId) ?? 0;
            set => OGRHandWeapon1Param?.WriteInt32((int)EROffsets.EquipParamWeapon.SwordArtsParamId, value);
        }
        private short OGRHandIconID
        {
            get => OGRHandWeapon1Param?.ReadInt16((int)EROffsets.EquipParamWeapon.IconID) ?? 0;
            set => OGRHandWeapon1Param?.WriteInt16((int)EROffsets.EquipParamWeapon.IconID, value);
        }
        private byte[] OGRHandOriginEquipWep
        { 
            get => OGRHandWeapon1Param?.ReadBytes((int)EROffsets.EquipParamWeapon.OriginEquipWep, 0x40) ?? new byte[0]; 
            set => OGRHandWeapon1Param?.WriteBytes((int)EROffsets.EquipParamWeapon.OriginEquipWep, value);
        }
        private byte[] OGRHandOriginEquipWep16
        {
            get => OGRHandWeapon1Param?.ReadBytes((int)EROffsets.EquipParamWeapon.OriginEquipWep16, 0x28) ?? new byte[0];
            set => OGRHandWeapon1Param?.WriteBytes((int)EROffsets.EquipParamWeapon.OriginEquipWep16, value);
        }
        private int OGLHandWeapon1 { get; set; }
        private PHPointer OGLHandWeapon1Param
        {
            get => CreateBasePointer(EquipParamWeapon.Resolve() + EquipParamWeaponOffsetDict[OGLHandWeapon1]);
        }
        private int OGLHandSortID
        {
            get => OGLHandWeapon1Param?.ReadInt32((int)EROffsets.EquipParamWeapon.SortID) ?? 0;
            set => OGLHandWeapon1Param?.WriteInt32((int)EROffsets.EquipParamWeapon.SortID, value);
        }
        private int OGLHandWeapon1SwordArtID
        {
            get => OGLHandWeapon1Param?.ReadInt32((int)EROffsets.EquipParamWeapon.SwordArtsParamId) ?? 0;
            set => OGLHandWeapon1Param?.WriteInt32((int)EROffsets.EquipParamWeapon.SwordArtsParamId, value);
        }
        private short OGLHandIconID
        {
            get => OGLHandWeapon1Param?.ReadInt16((int)EROffsets.EquipParamWeapon.IconID) ?? 0;
            set => OGLHandWeapon1Param?.WriteInt16((int)EROffsets.EquipParamWeapon.IconID, value);
        }
        private byte[] OGLHandOriginEquipWep
        {
            get => OGLHandWeapon1Param?.ReadBytes((int)EROffsets.EquipParamWeapon.OriginEquipWep, 0x40) ?? new byte[0];
            set => OGLHandWeapon1Param?.WriteBytes((int)EROffsets.EquipParamWeapon.OriginEquipWep, value);
        }
        private byte[] OGLHandOriginEquipWep16
        {
            get => OGLHandWeapon1Param?.ReadBytes((int)EROffsets.EquipParamWeapon.OriginEquipWep16, 0x28) ?? new byte[0];
            set => OGLHandWeapon1Param?.WriteBytes((int)EROffsets.EquipParamWeapon.OriginEquipWep16, value);
        }

        List<int> UsedWeapons = new List<int>();

        static Random RRand = new Random();
        static Random LRand = new Random(RRand.Next());
        public bool LevelRestrict { get; set; }
        public bool RandomizeAsh { get; set; }

        Timer RHandTimer = new Timer();
        public int RHandTime { get; set; } = 60;
        private bool _rHandRandom;
        public bool RHandRandom
        {
            get => _rHandRandom;
            set
            {
                _rHandRandom = value;
                if (_rHandRandom)
                {
                    RRand = new Random(RRand.Next());
                    OGRHandWeapon1 = Util.DeleteFromEnd(RHandWeapon1, 2) * 100; //remove the levels from the weapon
                    RHandTimer.Interval = RHandTime * 1000;
                    RHandTimer.Start();
                }
                else
                {
                    RHandTimer.Stop();
                    RHandWeapon1 = OGRHandWeapon1;
                    OGRHandSortID = BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGRHandWeapon1] + (int)EROffsets.EquipParamWeapon.SortID);
                    Array.Copy(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGRHandWeapon1] + (int)EROffsets.EquipParamWeapon.OriginEquipWep, OGRHandOriginEquipWep, 0x0, OGRHandOriginEquipWep.Length);
                    OGRHandIconID = BitConverter.ToInt16(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGRHandWeapon1] + (int)EROffsets.EquipParamWeapon.IconID);
                    OGRHandWeapon1SwordArtID = BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGRHandWeapon1] + (int)EROffsets.EquipParamWeapon.SwordArtsParamId);
                    Array.Copy(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGRHandWeapon1] + (int)EROffsets.EquipParamWeapon.OriginEquipWep16, OGRHandOriginEquipWep16, 0x0, OGRHandOriginEquipWep16.Length);
                }
            }
        }

        private void RHandTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UsedWeapons.Clear();
            UsedWeapons.Add(Util.DeleteFromEnd(RHandWeapon1, 3));
            if (_lHandRandom)
            {
                UsedWeapons.Add(Util.DeleteFromEnd(LHandWeapon1, 3));
            }

            ERWeapon weapon = GetWeapon(RRand);
            AssignRHWeapon(OGRHandWeapon1Param, weapon);
            

            RHandTimer.Interval = RHandTime * 1000;
            //Arrow1 = ERItemCategory.Arrows.Weapons[RRand.Next(ERItemCategory.Arrows.Weapons.Count)].ID;
            //Arrow2 = ERItemCategory.GreatArrows.Weapons[RRand.Next(ERItemCategory.GreatArrows.Weapons.Count)].ID;
            //Bolt1 = ERItemCategory.Bolts.Weapons[RRand.Next(ERItemCategory.Bolts.Weapons.Count)].ID;
            //Bolt2 = ERItemCategory.Bolts.Weapons[RRand.Next(ERItemCategory.Bolts.Weapons.Count)].ID;
        }

        private void AssignRHWeapon(PHPointer oGRHandWeapon1Param, ERWeapon weapon)
        {
            RHandWeapon1 = weapon.ID;
            OGRHandSortID = weapon.SortID;
            OGRHandOriginEquipWep = weapon.OriginEquipWep;
            OGRHandIconID = weapon.IconID;
            OGRHandWeapon1SwordArtID = weapon.SwordArtId;
            OGRHandOriginEquipWep16 = weapon.OriginEquipWep16;
        }

        Timer LHandTimer = new Timer();
        public int LHandTime { get; set; } = 60;
        private bool _lHandRandom;
        public bool LHandRandom
        {
            get => _lHandRandom;
            set
            {
                _lHandRandom = value;
                if (_lHandRandom)
                {
                    LRand = new Random(LRand.Next());
                    OGLHandWeapon1 = Util.DeleteFromEnd(LHandWeapon1, 2) * 100; //remove the levels from the weapon
                    LHandTimer.Interval = LHandTime * 1000;
                    LHandTimer.Start();
                }
                else
                {
                    LHandTimer.Stop();
                    LHandWeapon1 = OGLHandWeapon1;
                    OGLHandSortID = BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGRHandWeapon1] + (int)EROffsets.EquipParamWeapon.SortID);
                    Array.Copy(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGLHandWeapon1] + (int)EROffsets.EquipParamWeapon.OriginEquipWep, OGLHandOriginEquipWep, 0x0, OGLHandOriginEquipWep.Length);
                    OGLHandIconID = BitConverter.ToInt16(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGLHandWeapon1] + (int)EROffsets.EquipParamWeapon.IconID);
                    OGLHandWeapon1SwordArtID = BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGLHandWeapon1] + (int)EROffsets.EquipParamWeapon.SwordArtsParamId);
                    Array.Copy(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[OGLHandWeapon1] + (int)EROffsets.EquipParamWeapon.OriginEquipWep16, OGLHandOriginEquipWep16, 0x0, OGLHandOriginEquipWep16.Length);
                }
            }
        }

        private void LHandTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_rHandRandom)
            {
                UsedWeapons.Clear();
                UsedWeapons.Add(Util.DeleteFromEnd(LHandWeapon1, 3));
            }
            ERWeapon weapon = GetWeapon(LRand);
            AssignLHWeapon(OGLHandWeapon1Param, weapon);


            LHandTimer.Interval = LHandTime * 1000;
        }

        private void AssignLHWeapon(PHPointer oGLHandWeapon1Param, ERWeapon weapon)
        {
            LHandWeapon1 = weapon.ID;
            OGLHandSortID = weapon.SortID;
            OGLHandOriginEquipWep = weapon.OriginEquipWep;
            OGLHandIconID = weapon.IconID;
            OGLHandWeapon1SwordArtID = weapon.SwordArtId;
            OGLHandOriginEquipWep16 = weapon.OriginEquipWep16;
        }

        private ERWeapon GetWeapon(Random rand)
        {
            ERWeapon newWeapon = new ERWeapon();
            ERWeapon weapon;
            do
            {
                var newWeaponCategory = ERItemCategory.All[rand.Next(ERItemCategory.All.Count)];
                weapon = newWeaponCategory.Weapons[rand.Next(newWeaponCategory.Weapons.Count)];
            } 
            while (Util.DeleteFromEnd(RHandWeapon1, 3) == weapon.RealID ||
            Util.DeleteFromEnd(RHandWeapon2, 3) == weapon.RealID ||
            Util.DeleteFromEnd(LHandWeapon1, 3) == weapon.RealID ||
            Util.DeleteFromEnd(LHandWeapon2, 3) == weapon.RealID || 
            UsedWeapons.Contains(weapon.RealID));

            newWeapon.Clone(weapon);

            var id = newWeapon.ID;

            ERGem? ash;

            if (RandomizeAsh && !newWeapon.Unique)
            {
                var gems = ERGem.Gems.Where(x => x.WeaponTypes.Contains(newWeapon.Type)).ToList();
                ash = gems[rand.Next(gems.Count)];
            }
            else
            {
                ash = ERGem.Gems.FirstOrDefault(x => x.SwordArtID == newWeapon.SwordArtId);
            }

            newWeapon.SwordArtId = ash?.SwordArtID ?? newWeapon.SwordArtId;

            var infusion = 0;
            if (newWeapon.Infusible && ash != null)
                infusion = (int)ash?.Infusions[rand.Next(ash.Infusions.Count)];
            
            id += infusion;
            var maxLevel = newWeapon.Unique ? 10 : 25;

            if (LevelRestrict)
                id += GetLevel(maxLevel);
            else
                id += rand.Next(maxLevel);

            return newWeapon;
        }

        public int MaxLevel { get; set; } = 80;

        public int GetLevel(int maxLevel)
        {
            if (maxLevel == 1 || Level >= MaxLevel)
                return maxLevel - 1;

            var levels = (float)MaxLevel / (maxLevel - 1);

            return (int)Math.Floor(Level / levels);
        }
    }
}
