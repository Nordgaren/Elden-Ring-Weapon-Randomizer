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

        //private PHPointer DurabilityAddr { get; set; }
        //private PHPointer DurabilitySpecialAddr { get; set; }
        public bool Loaded => this?.PlayerGameData?.Resolve() != IntPtr.Zero;
        public ERHook(int refreshInterval, int minLifetime, Func<Process, bool> processSelector)
            : base(refreshInterval, minLifetime, processSelector)
        {
            OnHooked += ERHook_OnHooked;
            GameDataManSetup = RegisterAbsoluteAOB(EROffsets.GameDataManSetupAoB);
            SoloParamRepositorySetup = RegisterAbsoluteAOB(EROffsets.SoloParamRepositorySetupAoB);

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
            var bytes = new byte[0];
            EquipParamWeaponOffsetDict = BuildOffsetDictionary(EquipParamWeapon, "EQUIP_PARAM_WEAPON_ST", ref bytes);
            EquipParamWeaponBytes = bytes;
            GetWeaponParams();
        }
        private void GetWeaponParams()
        {
            foreach (var category in ERItemCategory.All)
            {
                GetProperties(category);
            }
        }

        private void GetProperties(ERItemCategory category)
        {
            foreach (var weapon in category.Weapons)
            {
                if (!EquipParamWeaponOffsetDict.ContainsKey(weapon.ID))
                {
                    Debug.WriteLine($"{weapon.ID} {weapon.Name}");
                    continue;
                }
                weapon.Unique = BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.MaterialSetID) == 2200;
                weapon.SwordArtId = BitConverter.ToInt32(EquipParamWeaponBytes, EquipParamWeaponOffsetDict[weapon.ID] + (int)EROffsets.EquipParamWeapon.SwordArtsParamId);
            }
        }

        public Dictionary<int, int> EquipParamWeaponOffsetDict { get; private set; }
        private byte[] EquipParamWeaponBytes { get; set; }
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

        List<int> UsedWeapons = new List<int>();

        static Random RRand = new Random();
        static Random LRand = new Random(RRand.Next());
        public bool LevelRestrict { get; set; }

        Timer RHandTimer = new Timer();
        public int RHandTime { get; set; } = 60;
        private bool _rHandRandom;
        public bool RHandRandom
        {
            set
            {
                _rHandRandom = value;
                if (_rHandRandom)
                {
                    RRand = new Random(RRand.Next());
                    RHandTimer.Interval = RHandTime * 1000;
                    RHandTimer.Start();
                }
                else
                    RHandTimer.Stop();
            }
        }

        private void RHandTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UsedWeapons.Clear();
            UsedWeapons.Add(Util.DeleteFromEnd(RHandWeapon1, 3));
            //UsedWeapons.Add(Util.DeleteFromEnd(RHandWeapon2, 3));
            if (_lHandRandom)
            {
                UsedWeapons.Add(Util.DeleteFromEnd(LHandWeapon1, 3));
                //UsedWeapons.Add(Util.DeleteFromEnd(LHandWeapon2, 3));
            }
            

            RHandWeapon1 = GetWeapon(RRand);
            //RHandWeapon2 = GetWeapon(RRand);
            //if (_lHandRandom)
            //    LHandWeapon2 = GetWeapon();
            //else
            //    RHandWeapon2 = GetWeapon();

            RHandTimer.Interval = RHandTime * 1000;
            //Arrow1 = ERItemCategory.Arrows.Weapons[RRand.Next(ERItemCategory.Arrows.Weapons.Count)].ID;
            //Arrow2 = ERItemCategory.GreatArrows.Weapons[RRand.Next(ERItemCategory.GreatArrows.Weapons.Count)].ID;
            //Bolt1 = ERItemCategory.Bolts.Weapons[RRand.Next(ERItemCategory.Bolts.Weapons.Count)].ID;
            //Bolt2 = ERItemCategory.Bolts.Weapons[RRand.Next(ERItemCategory.Bolts.Weapons.Count)].ID;
        }

        Timer LHandTimer = new Timer();
        public int LHandTime { get; set; } = 60;
        private bool _lHandRandom;
        public bool LHandRandom
        {
            set
            {
                _lHandRandom = value;
                if (_lHandRandom)
                {
                    LRand = new Random(LRand.Next());
                    LHandTimer.Interval = LHandTime * 1000;
                    LHandTimer.Start();
                }
                else
                    LHandTimer.Stop();
            }
        }

        private void LHandTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_rHandRandom)
            {
                UsedWeapons.Clear();
                UsedWeapons.Add(Util.DeleteFromEnd(LHandWeapon1, 3));
                //UsedWeapons.Add(Util.DeleteFromEnd(LHandWeapon2, 3));
            }

            LHandWeapon1 = GetWeapon(LRand);
            //LHandWeapon2 = GetWeapon(LRand);
            //if (_rHandRandom)
            //    RHandWeapon2 = GetWeapon();
            //else
            //    LHandWeapon2 = GetWeapon();

            LHandTimer.Interval = LHandTime * 1000;
            //if (!_rHandRandom)
            //{
            //    Arrow1 = ERItemCategory.Arrows.Weapons[LRand.Next(ERItemCategory.Arrows.Weapons.Count)].ID;
            //    Arrow2 = ERItemCategory.GreatArrows.Weapons[LRand.Next(ERItemCategory.GreatArrows.Weapons.Count)].ID;
            //    Bolt1 = ERItemCategory.Bolts.Weapons[LRand.Next(ERItemCategory.Bolts.Weapons.Count)].ID;
            //    Bolt2 = ERItemCategory.Bolts.Weapons[LRand.Next(ERItemCategory.Bolts.Weapons.Count)].ID;
            //}
        }

        private int GetWeapon(Random rand)
        {
            ERWeapon newWeapon;
            do
            {
                var newWeaponCategory = ERItemCategory.All[rand.Next(ERItemCategory.All.Count)];
                newWeapon = newWeaponCategory.Weapons[rand.Next(newWeaponCategory.Weapons.Count)];
            } 
            while (Util.DeleteFromEnd(RHandWeapon1, 3) == newWeapon.RealID ||
            Util.DeleteFromEnd(RHandWeapon2, 3) == newWeapon.RealID ||
            Util.DeleteFromEnd(LHandWeapon1, 3) == newWeapon.RealID ||
            Util.DeleteFromEnd(LHandWeapon2, 3) == newWeapon.RealID || 
            UsedWeapons.Contains(newWeapon.RealID));

            var id = newWeapon.ID;
            var infusion = !newWeapon.Unique && newWeapon.Infusible ? (rand.Next(13) * 100) : 0;
            id += infusion;
            var maxLevel = newWeapon.Unique ? 10 : 25;

            if (LevelRestrict)
                id += GetLevel(maxLevel);
            else
                id += rand.Next(maxLevel);

            return id;
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
