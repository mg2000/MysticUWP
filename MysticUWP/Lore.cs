﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysticUWP
{
	class Lore
	{
		public string Name
		{
			get;
			set;
		}

		public GenderType Gender
		{
			get;
			set;
		}

		public int Class
		{
			get;
			set;
		}

		public int Strength
		{
			get;
			set;
		}

		public int Mentality
		{
			get;
			set;
		}

		public int Concentration
		{
			get;
			set;
		}

		public int Endurance
		{
			get;
			set;
		}

		public int Resistance
		{
			get;
			set;
		}

		public int Agility
		{
			get;
			set;
		}

		public int Accuracy
		{
			get;
			set;
		}

		public int Luck
		{
			get;
			set;
		}

		public int Poison
		{
			get;
			set;
		}

		public int Unconscious
		{
			get;
			set;
		}

		public int Dead
		{
			get;
			set;
		}

		public int HP
		{
			get;
			set;
		}

		public int SP
		{
			get;
			set;
		}

		public int AC
		{
			get;
			set;
		}

		public int Level
		{
			get;
			set;
		}

		public long Experience
		{
			get;
			set;

		}

		public long PotentialExperience
		{
			get;
			set;
		}

		public int Weapon
		{
			get;
			set;
		}

		public int Shield
		{
			get;
			set;
		}

		public int Armor
		{
			get;
			set;
		}

		public int PotentialAC
		{
			get;
			set;
		}

		public int WeaPower
		{
			get;
			set;
		}

		public int ShiPower
		{
			get;
			set;
		}

		public int ArmPower
		{
			get;
			set;
		}

		public string NameJosa
		{
			get
			{
				if (Common.HasJongsung(Name[Name.Length - 1]))
					return Name + "의";
				else
					return Name + "가";
			}
		}

		public string NameSubjectJosa
		{
			get
			{
				if (Common.HasJongsung(Name[Name.Length - 1]))
					return Name + "은";
				else
					return Name + "는";
			}
		}

		public string NameMokjukJosa
		{
			get
			{
				if ((Name[Name.Length - 1] - 0xAC00) % 28 + 0x11A8 - 1 == 0)
					return Name + "를";
				else
					return Name + "을";
			}
		}

		public string GenderPronoun
		{
			get
			{
				if (Gender == GenderType.Male)
					return "그";
				else
					return "그녀";
			}
		}

		public ClassCategory ClassType
		{
			get;
			set;
		}

		public int SwordSkill
		{
			get;
			set;
		}

		public int AxeSkill
		{
			get;
			set;
		}

		public int SpearSkill
		{
			get;
			set;
		}

		public int BowSkill
		{
			get;
			set;
		}

		public int ShieldSkill
		{
			get;
			set;
		}

		public int FistSkill
		{
			get;
			set;
		}

		public int AttackMagic
		{
			get;
			set;
		}

		public int PhenoMagic
		{
			get;
			set;
		}

		public int CureMagic
		{
			get;
			set;
		}

		public int SpecialMagic
		{
			get;
			set;
		}

		public int ESPMagic
		{
			get;
			set;
		}

		public int SummonMagic
		{
			get;
			set;
		}

		public bool IsClassAvailable(int requestClass)
		{
			if (ClassType == ClassCategory.Sword)
			{
				switch (requestClass)
				{
					case 1:
						return SwordSkill >= 10 && AxeSkill >= 10 && SpearSkill >= 10 && BowSkill >= 10 && ShieldSkill >= 10;
					case 2:
						return SwordSkill >= 10 && AxeSkill >= 10 && SpearSkill >= 5 && ShieldSkill >= 20;
					case 3:
						return SwordSkill >= 40;
					case 4:
						return AxeSkill >= 5 && SpearSkill >= 5 && BowSkill >= 40;
					case 5:
						return FistSkill >= 40;
					case 6:
						return SwordSkill >= 10 && BowSkill >= 10 && FistSkill >= 20;
					default:
						return SwordSkill >= 25 && SpearSkill >= 5 && ShieldSkill >= 20 && FistSkill >= 10;
				}
			}
			else
			{
				switch (requestClass)
				{
					case 1:
						return AttackMagic >= 10 && PhenoMagic >= 10 && CureMagic >= 10;
					case 2:
						return SummonMagic >= 10 && PhenoMagic >= 10 && CureMagic >= 10;
					case 3:
						return SummonMagic >= 10 && CureMagic >= 10;
					case 4:
						return AttackMagic >= 40 && PhenoMagic >= 25 && CureMagic >= 25;
					case 5:
						return AttackMagic >= 20 && PhenoMagic >= 20 && CureMagic >= 40 && SummonMagic >= 40;
					case 6:
						return AttackMagic >= 10 && PhenoMagic >= 40 && CureMagic >= 30 && SummonMagic >= 20;
					default:
						return AttackMagic >= 40 && PhenoMagic >= 40 && CureMagic >= 40 && SpecialMagic >= 20 && ESPMagic >= 20 && SummonMagic >= 20;
				}
			}
		}

		public string GenderStr
		{
			get
			{
				switch (Gender)
				{
					case GenderType.Male:
						return "남성";
					case GenderType.Female:
						return "여성";
					default:
						return "불확실함";
				}
			}
		}

		public string ClassTypeStr
		{
			get
			{
				switch (ClassType)
				{
					case ClassCategory.Sword:
						return "전투사계";
					default:
						return "마법사계";
				}
			}
		}

		public string ClassStr
		{
			get
			{
				return Common.GetClass(ClassType, Class);
			}
		}

		//public void UpdatePotentialExperience()
		//{
		//	if (1 <= Level && Level <= 40)
		//		PotentialExperience = Common.GetLevelUpExperience(Level);
		//	else
		//		PotentialExperience = 0;
		//}

		public bool IsAvailable
		{
			get
			{
				if (Unconscious == 0 && Dead == 0 && HP > 0)
					return true;
				else
					return false;
			}
		}
	}

	enum GenderType {
		Male,
		Female,
		Neutral
	}

	public enum ClassCategory
	{
		Sword,
		Magic,
		Elemental,
		Unknown,
		Giant,
		Golem,
		Dragon
	}
}
