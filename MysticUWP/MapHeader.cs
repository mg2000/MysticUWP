using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysticUWP
{
	public class MapHeader
	{
		public string ID
		{
			get;
			set;
		}

		public int Width
		{
			get;
			set;
		}

		public int Height
		{
			get;
			set;
		}

		public PositionType TileType
		{
			get;
			set;
		}

		public bool Encounter
		{
			get;
			set;
		}

		public bool Handicap
		{
			get;
			set;
		}

		public int StartX
		{
			get;
			set;
		}

		public int StartY
		{
			get;
			set;
		}

		public string ExitMap
		{
			get;
			set;
		}

		public int ExitX
		{
			get;
			set;
		}

		public int ExitY
		{
			get;
			set;
		}

		public string EnterMap
		{
			get;
			set;
		}

		public int EnterX
		{
			get;
			set;
		}

		public int EnterY
		{
			get;
			set;
		}

		public int Default
		{
			get;
			set;
		}

		public int HandicapBit
		{
			get;
			set;
		}

		public byte[] Layer
		{
			get;
			set;
		}
	}
}
