using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysticUWP
{
	class SaveData
	{
		public List<Lore> PlayerList
		{
			get;
			set;
		}

		public List<Lore> BackupPlayerList {
			get;
			set;
		}

		public Lore AssistPlayer
		{
			get;
			set;
		}

		public LorePlayer Party
		{
			get;
			set;
		}

		public MapHeader MapHeader
		{
			get;
			set;
		}

		public int Encounter
		{
			get;
			set;
		}

		public int MaxEnemy
		{
			get;
			set;
		}

		public bool Cruel {
			get;
			set;
		}

		public bool Ebony {
			get;
			set;
		}

		public bool MoonLight {
			get;
			set;
		}

		public int WatchYear {
			get;
			set;
		}

		public int WatchDay {
			get;
			set;
		}

		public int WatchHour {
			get;
			set;
		}

		public int WatchMin {
			get;
			set;
		}

		public int WatchSec {
			get;
			set;
		}

		public bool TimeWatch {
			get;
			set;
		}

		public int TimeEvent {
			get;
			set;
		}

		public long SaveTime
		{
			get;
			set;
		}
	}
}
