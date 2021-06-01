using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysticUWP
{
	class Common
	{
		public static string[] SwordClass = new string[] { "투사", "기사", "검사", "사냥꾼", "전투승", "암살자", "전사", "불확실함" };
		public static string[] MagicClass = new string[] { "메이지", "컨져러", "주술사", "위저드", "강령술사", "대마법사", "타임워커", "불확실함" };

		public static string GetClass(ClassCategory category, int playerClass)
		{

			if (1 <= playerClass && playerClass <= 10)
			{
				if (category == ClassCategory.Sword)
					return SwordClass[playerClass - 1];
				else
					return MagicClass[playerClass - 1];
			}
			else
				return "불확실함";
		}
		public static bool HasJongsung(char chr)
		{
			if (chr < 0xAC00 || chr > 0xD7A3)
				return false;
			else if ((chr - 0xAc00) % 28 > 0)
				return true;
			else
				return false;
		}
	}
}
