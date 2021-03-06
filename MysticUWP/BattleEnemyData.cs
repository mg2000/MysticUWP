using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysticUWP
{
	class BattleEnemyData
	{
		public BattleEnemyData(int enemyID, EnemyData enemy)
		{
			ENumber = enemyID;
			Name = enemy.Name;
			Strength = enemy.Strength;
			Mentality = enemy.Mentality;
			Endurance = enemy.Endurance;
			Resistance = enemy.Resistance;
			Agility = enemy.Agility;
			Accuracy = enemy.Accuracy;
			AC = enemy.AC;
			Special = enemy.Special;
			CastLevel = enemy.CastLevel;
			SpecialCastLevel = enemy.SpecialCastLevel;
			Level = enemy.Level;

			HP = Endurance * Level * 10;
			AuxHP = 0;
			Posion = false;
			Unconscious = false;
			Dead = false;
		}

		public int ENumber
		{
			get;
			set;
		}

		public string Name
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

		public int Endurance
		{
			get;
			set;
		}

		public int[] Resistance
		{
			get;
			set;
		}

		public int Agility
		{
			get;
			set;
		}

		public int[] Accuracy
		{
			get;
			set;
		}

		public int AC
		{
			get;
			set;
		}

		public int Special
		{
			get;
			set;
		}

		public int CastLevel
		{
			get;
			set;
		}

		public int SpecialCastLevel
		{
			get;
			set;
		}

		public int Level
		{
			get;
			set;
		}

		public int HP
		{
			get;
			set;
		}

		public int AuxHP {
			get;
			set;
		}

		public bool Posion
		{
			get;
			set;
		}

		public bool Unconscious
		{
			get;
			set;
		}

		public bool Dead
		{
			get;
			set;
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

		public string NameJosa
		{
			get
			{
				if (Common.HasJongsung(Name[Name.Length - 1]))
					return Name + "을";
				else
					return Name + "를";
			}
		}
	}
}
