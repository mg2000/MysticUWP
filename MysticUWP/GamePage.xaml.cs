using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MysticUWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class GamePage : Page
	{
		private int mXAxis = 0;
		private int mYAxis = 0;
		private string mMapName = "";
		private MapHeader mMapHeader;

		private SpriteSheet mWizardEyeTile;
		private SpriteSheet mMapTiles;
		private SpriteSheet mCharacterTiles;
		private readonly object mapLock = new object();

		private int mXWide; // 시야 범위
		private int mYWide; // 시야 범위

		private LorePlayer mParty;
		private List<Lore> mPlayerList;
		private Lore mAssistPlayer = null;

		private int mFace = 0;
		private int mEncounter = 0;
		private int mMaxEnemy = 0;

		private WizardEye mWizardEye = new WizardEye();
		private volatile bool mWizardEyePosBlink = false;
		private DispatcherTimer mWizardEyeTimer = new DispatcherTimer();
		private int mWizardEyePosX;
		private int mWizardEyePosY;

		private List<TextBlock> mPlayerNameList = new List<TextBlock>();
		private List<TextBlock> mPlayerHPList = new List<TextBlock>();
		private List<TextBlock> mPlayerSPList = new List<TextBlock>();
		private List<TextBlock> mPlayerConditionList = new List<TextBlock>();
		private List<TextBlock> mEnemyTextList = new List<TextBlock>();
		private List<Border> mEnemyBlockList = new List<Border>();
		private List<HealthTextBlock> mHealthTextList = new List<HealthTextBlock>();

		private List<TextBlock> mMenuList = new List<TextBlock>();
		private MenuMode mMenuMode = MenuMode.None;
		private int mMenuCount = 0;
		private int mMenuFocusID = 0;

		private bool mLoading = true;

		private const int DIALOG_MAX_LINES = 13;
		private readonly List<string> mRemainDialog = new List<string>();
		private AfterDialogType mAfterDialogType = AfterDialogType.None;

		private SpecialEventType mSpecialEvent = SpecialEventType.None;
		private bool mTriggeredDownEvent = false;

		private BattleEvent mBattleEvent = BattleEvent.None;
		private volatile AnimationType mAnimationEvent = AnimationType.None;
		private int mAnimationFrame = 0;

		private SpinnerType mSpinnerType = SpinnerType.None;
		private Tuple<string, int>[] mSpinnerItems;
		private int mSpinnerID;

		private Lore mTrainPlayer;
		private static readonly List<Tuple<int, int>> list = new List<Tuple<int, int>>();
		private List<Tuple<int, int>> mTrainSkillList = list;
		private readonly List<string> mChangableClassList = new List<string>();
		private readonly List<int> mChangableClassIDList = new List<int>();

		private TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyDownEvent = null;
		private TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyUpEvent = null;

		private Random mRand = new Random();

		public GamePage()
		{
			this.InitializeComponent();

			mPlayerNameList.Add(PlayerName0);
			mPlayerNameList.Add(PlayerName1);
			mPlayerNameList.Add(PlayerName2);
			mPlayerNameList.Add(PlayerName3);
			mPlayerNameList.Add(PlayerName4);
			mPlayerNameList.Add(PlayerName5);

			mPlayerHPList.Add(PlayerHP0);
			mPlayerHPList.Add(PlayerHP1);
			mPlayerHPList.Add(PlayerHP2);
			mPlayerHPList.Add(PlayerHP3);
			mPlayerHPList.Add(PlayerHP4);
			mPlayerHPList.Add(PlayerHP5);

			mPlayerSPList.Add(PlayerSP0);
			mPlayerSPList.Add(PlayerSP1);
			mPlayerSPList.Add(PlayerSP2);
			mPlayerSPList.Add(PlayerSP3);
			mPlayerSPList.Add(PlayerSP4);
			mPlayerSPList.Add(PlayerSP5);

			mPlayerConditionList.Add(PlayerCondition0);
			mPlayerConditionList.Add(PlayerCondition1);
			mPlayerConditionList.Add(PlayerCondition2);
			mPlayerConditionList.Add(PlayerCondition3);
			mPlayerConditionList.Add(PlayerCondition4);
			mPlayerConditionList.Add(PlayerCondition5);

			mMenuList.Add(GameMenuText0);
			mMenuList.Add(GameMenuText1);
			mMenuList.Add(GameMenuText2);
			mMenuList.Add(GameMenuText3);
			mMenuList.Add(GameMenuText4);
			mMenuList.Add(GameMenuText5);
			mMenuList.Add(GameMenuText6);
			mMenuList.Add(GameMenuText7);
			mMenuList.Add(GameMenuText8);
			mMenuList.Add(GameMenuText9);

			mEnemyBlockList.Add(EnemyBlock0);
			mEnemyBlockList.Add(EnemyBlock1);
			mEnemyBlockList.Add(EnemyBlock2);
			mEnemyBlockList.Add(EnemyBlock3);
			mEnemyBlockList.Add(EnemyBlock4);
			mEnemyBlockList.Add(EnemyBlock5);
			mEnemyBlockList.Add(EnemyBlock6);
			mEnemyBlockList.Add(EnemyBlock7);

			mEnemyTextList.Add(EnemyText0);
			mEnemyTextList.Add(EnemyText1);
			mEnemyTextList.Add(EnemyText2);
			mEnemyTextList.Add(EnemyText3);
			mEnemyTextList.Add(EnemyText4);
			mEnemyTextList.Add(EnemyText5);
			mEnemyTextList.Add(EnemyText6);
			mEnemyTextList.Add(EnemyText7);

			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName1, HealthPoison1, HealthUnconscious1, HealthDead1));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName2, HealthPoison2, HealthUnconscious2, HealthDead2));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName3, HealthPoison3, HealthUnconscious3, HealthDead3));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName4, HealthPoison4, HealthUnconscious4, HealthDead4));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName5, HealthPoison5, HealthUnconscious5, HealthDead5));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName6, HealthPoison6, HealthUnconscious6, HealthDead6));

			gamePageKeyDownEvent = async (sender, args) =>
			{
				if (mLoading || mSpecialEvent != SpecialEventType.None || mAnimationEvent != AnimationType.None || ContinueText.Visibility == Visibility.Visible || mTriggeredDownEvent)
					return;

				if (mMenuMode == MenuMode.None && mSpinnerType == SpinnerType.None && (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.Right ||
				 args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight ||
				 args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadDPadLeft || args.VirtualKey == VirtualKey.GamepadDPadRight))
				{
					var x = mXAxis;
					var y = mYAxis;

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
					{
						y--;
						mFace = 5;
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown)
					{
						y++;
						mFace = 4;
					}
					else if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft)
					{
						x--;
						mFace = 7;
					}
					else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight)
					{
						x++;
						mFace = 6;
					}

					if (mPlayerList[0].ClassType == ClassCategory.Sword)
					{
						if (mMapHeader.TileType == PositionType.Town)
							mFace -= 4;
						else if (mMapName == "Menace")
							mFace -= 4;
					}
					else if (mPlayerList[0].ClassType == ClassCategory.Magic)
						mFace += 4;
					

					if (x > 3 && x < mMapHeader.Width - 4 && y > 4 && y < mMapHeader.Height - 5)
					{
						void EnterMap()
						{
							//if (mParty.Map == 1)
							//{
							//	if (x == 19 && y == 10)
							//		ShowEnterMenu(EnterType.CastleLore);
							//	else if (x == 75 && y == 56)
							//		ShowEnterMenu(EnterType.CastleLastDitch);
							//	else if (x == 16 && y == 88)
							//		ShowEnterMenu(EnterType.Menace);
							//	else if (x == 83 && y == 85)
							//		ShowEnterMenu(EnterType.UnknownPyramid);
							//}
							//else if (mParty.Map == 2)
							//{
							//	if (x == 99 && y == 99)
							//		ShowExitMenu();
							//	else if (x == 15 && y == 15)
							//		ShowEnterMenu(EnterType.ProofOfInfortune);
							//	else if (x == 148 && y == 64)
							//		ShowEnterMenu(EnterType.ClueOfInfortune);
							//}
							//else if (mParty.Map == 3)
							//{
							//	if (x == 65 && y == 77)
							//		ShowEnterMenu(EnterType.RoofOfLight);
							//	else if (x == 88 && y == 93)
							//		ShowEnterMenu(EnterType.TempleOfLight);
							//	else if (x == 32 && y == 48)
							//		ShowEnterMenu(EnterType.SurvivalOfPerishment);
							//	else if (x == 35 && y == 15)
							//		ShowEnterMenu(EnterType.CaveOfBerial);
							//	else if (x == 92 && y == 5)
							//		ShowEnterMenu(EnterType.CaveOfMolok);
							//}
							//else if (mParty.Map == 4)
							//{
							//	if (x == 41 && y == 75)
							//		ShowEnterMenu(EnterType.TeleportationGate1);
							//	else if (x == 12 && y == 70)
							//		ShowEnterMenu(EnterType.TeleportationGate2);
							//	else if (x == 40 && y == 53)
							//		ShowEnterMenu(EnterType.TeleportationGate3);
							//	else if (x == 13 && y == 42)
							//		ShowEnterMenu(EnterType.CaveOfAsmodeus1);
							//	else if (x == 8 && y == 20)
							//		ShowEnterMenu(EnterType.CaveOfAsmodeus2);
							//	else if (x == 26 && y == 8)
							//		ShowEnterMenu(EnterType.FortressOfMephistopheles);
							//}
							//else if (mParty.Map == 7)
							//{
							//	AppendText(new string[] {
							//		" 당신이 동굴 입구에 들어가려 할 때 어떤 글을 보았다.",
							//		"",
							//		"",
							//		"",
							//		$"[color={RGB.White}]   여기는 한때 피라미드라고 불리는 악마의 동굴이었지만 지금은 폐쇄되어 아무도 들어갈 수가 없습니다.[/color]"
							//	});
							//}
							//else if (mParty.Map == 11)
							//{
							//	if ((x == 24 && y == 6) || (x == 25 && y == 6))
							//	{
							//		AppendText(" 당신이 입구에 들어가려 했지만 이미 입구는 함몰되어 들어 갈 수가 없었다.");
							//		if (mPlayerList.Count > 1 && (mParty.Etc[30] & (1 << 7)) == 0)
							//		{
							//			mSpecialEvent = SpecialEventType.InvestigationCave;
							//			ContinueText.Visibility = Visibility.Visible;
							//		}
							//	}
							//}
							//else if (mParty.Map == 12)
							//{
							//	if ((x == 24 && y == 27) || (x == 25 && y == 27))
							//	{
							//		AppendText(" 당신은 이 동굴에 들어가려고 했지만 동굴의 입구는 어떠한 강한 힘에 의해 무너져 있었고" +
							//		" 일행들의 힘으로는 도저히 들어갈 방도가 없었다. 결국에 일행은 들어가기를 포기했다.");

							//		UpdateTileInfo(24, 24, 52);
							//		UpdateTileInfo(25, 24, 52);
							//	}
							//}
						}

						if (mMapHeader.TileType == PositionType.Town)
						{
							if (GetTileInfo(x, y) == 0 || GetTileInfo(x, y) == 19)
							{
								var oriX = mXAxis;
								var oriY = mYAxis;
								MovePlayer(x, y);
								if (await InvokeSpecialEvent(oriX, oriY))
									mTriggeredDownEvent = true;
							}
							else if (1 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 18 || GetTileInfo(x, y) == 20 || GetTileInfo(x, y) == 21)
							{
								// Don't Move
							}
							else if (GetTileInfo(x, y) == 22)
							{
								EnterMap();
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 23)
							{
								ShowSign(x, y);
							}
							else if (GetTileInfo(x, y) == 24)
							{
								if (EnterWater())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 25)
							{
								if (EnterSwamp())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 26)
							{
								if (EnterLava())
									MovePlayer(x, y);
							}
							else if (27 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								TalkMode(x, y);
								mTriggeredDownEvent = true;
							}
						}
						else if (mMapHeader.TileType == PositionType.Ground)
						{
							if (GetTileInfo(x, y) == 0 || GetTileInfo(x, y) == 20)
							{
								var oriX = mXAxis;
								var oriY = mYAxis;
								MovePlayer(x, y);
								if (await InvokeSpecialEvent(oriX, oriY))
									mTriggeredDownEvent = true;
							}
							else if (1 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 19)
							{
								// Don't Move
							}
							else if (GetTileInfo(x, y) == 22)
							{
								ShowSign(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 48)
							{
								if (EnterWater())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 23 || GetTileInfo(x, y) == 49)
							{
								if (EnterSwamp())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 50)
							{
								if (EnterLava())
									MovePlayer(x, y);
							}
							else if (24 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								EnterMap();
								mTriggeredDownEvent = true;
							}
						}
						else if (mMapHeader.TileType == PositionType.Den)
						{
							if (GetTileInfo(x, y) == 0 || GetTileInfo(x, y) == 52)
							{
								var oriX = mXAxis;
								var oriY = mYAxis;
								MovePlayer(x, y);
								if (await InvokeSpecialEvent(oriX, oriY))
									mTriggeredDownEvent = true;
							}
							else if ((1 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 19) || (22 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 40) || GetTileInfo(x, y) == 51)
							{

							}
							else if (GetTileInfo(x, y) == 53)
							{
								ShowSign(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 48)
							{
								if (EnterWater())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 49)
							{
								if (EnterSwamp())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 50)
							{
								if (EnterLava())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 54)
							{
								EnterMap();
								mTriggeredDownEvent = true;
							}
							else if (41 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								TalkMode(x, y);
								mTriggeredDownEvent = true;
							}
						}
						else if (mMapHeader.TileType == PositionType.Keep)
						{
							if (GetTileInfo(x, y) == 0 || GetTileInfo(x, y) == 52)
							{
								var oriX = mXAxis;
								var oriY = mYAxis;
								MovePlayer(x, y);
								if (await InvokeSpecialEvent(oriX, oriY))
									mTriggeredDownEvent = true;

								mTriggeredDownEvent = true;
							}
							else if ((1 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 39) || GetTileInfo(x, y) == 51)
							{

							}
							else if (GetTileInfo(x, y) == 53)
							{
								ShowSign(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 48)
							{
								if (EnterWater())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 49)
							{
								if (EnterSwamp())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 50)
							{
								if (EnterLava())
									MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 54)
							{
								EnterMap();
								mTriggeredDownEvent = true;
							}
							else if (40 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								TalkMode(x, y);
								mTriggeredDownEvent = true;
							}
						}
					}
				}
			};

			gamePageKeyUpEvent = async (sender, args) =>
			{
				var swordEnableClass = new int[,] {
						{  50,  50,  50,  50,  50,  50 },
						{  60,  60,  50,   0,  60,  50 },
						{ 100,   0,   0,   0,  30,  30 },
						{   0,  50,  50, 100,   0,  60 },
						{   0,   0,   0,   0,   0, 100 },
						{  80,   0,  60,  80,   0,  70 },
						{  60,  50,  50,  30,  70,  50 }
					};

				var magicEnableClass = new int[,] {
						{  50,  30,  30,   0,  50,   0 },
						{  20,  50,  30,  10,  50,  30 },
						{  20,  20,  50,  10,  50,  50 },
						{ 100,  60,  60,  50, 100,   0 },
						{  60,  70, 100, 100, 100, 100 },
						{  60, 100,  70, 100, 100,  50 },
						{  70, 100,  70,  50, 100, 100 }
					};

				var weaponPrice = new int[,] {
					{ 500, 3_000, 5_000,  7_000, 12_000, 40_000,  70_000 },
					{ 500, 3_000, 5_000, 10_000, 30_000, 60_000, 100_000 },
					{ 100, 1_000, 1_500,  4_000,  8_000, 35_000,  50_000 },
					{ 200,   300,   800,  2_000,  5_000, 10_000,  30_000 }
				};

				var shieldPrice = new int[] { 3_000, 15_000, 45_000, 80_000, 150_000 };
				var armorPrice = new int[] { 2_000, 5_000, 22_000, 45_000, 75_000, 100_000, 140_000, 200_000, 350_000, 500_000 };

//				void ShowTrainSkillMenu(int defaultMenuID)
//				{
//					AppendText($"[color={RGB.White}]{mTrainPlayer.Name}의 현재 능력치[/color]");

//					var trainSkillMenuList = new List<string>();
//					mTrainSkillList.Clear();

//					if (swordEnableClass[mTrainPlayer.Class - 1, 0] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  베는 무기 기술치 :\t{mTrainPlayer.SwordSkill}[/color]", true);
//						trainSkillMenuList.Add("  베는 무기 기술치");
//						mTrainSkillList.Add(new Tuple<int, int>(0, swordEnableClass[mTrainPlayer.Class - 1, 0]));
//					}

//					if (swordEnableClass[mTrainPlayer.Class - 1, 1] > 0)
//					{
//						if (mTrainPlayer.Class != 7)
//						{
//							AppendText($"[color={RGB.LightCyan}]  찍는 무기 기술치 :\t{mTrainPlayer.AxeSkill}[/color]", true);
//							trainSkillMenuList.Add("  찍는 무기 기술치");
//						}
//						else
//						{
//							AppendText($"[color={RGB.LightCyan}]  치료 마법 능력치 :\t{mTrainPlayer.AxeSkill}[/color]", true);
//							trainSkillMenuList.Add("  치료 마법 능력치");
//						}

//						mTrainSkillList.Add(new Tuple<int, int>(1, swordEnableClass[mTrainPlayer.Class - 1, 1]));
//					}

//					if (swordEnableClass[mTrainPlayer.Class - 1, 2] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  찌르는 무기 기술치 :\t{mTrainPlayer.SpearSkill}[/color]", true);
//						trainSkillMenuList.Add("  찌르는 무기 기술치");
//						mTrainSkillList.Add(new Tuple<int, int>(2, swordEnableClass[mTrainPlayer.Class - 1, 2]));
//					}

//					if (swordEnableClass[mTrainPlayer.Class - 1, 3] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  쏘는 무기 기술치 :\t{mTrainPlayer.BowSkill}[/color]", true);
//						trainSkillMenuList.Add("  쏘는 무기 기술치");
//						mTrainSkillList.Add(new Tuple<int, int>(3, swordEnableClass[mTrainPlayer.Class - 1, 3]));
//					}

//					if (swordEnableClass[mTrainPlayer.Class - 1, 4] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  방패 사용 기술치 :\t{mTrainPlayer.ShieldSkill}[/color]", true);
//						trainSkillMenuList.Add("  방패 사용 능력치");
//						mTrainSkillList.Add(new Tuple<int, int>(4, swordEnableClass[mTrainPlayer.Class - 1, 4]));
//					}

//					if (swordEnableClass[mTrainPlayer.Class - 1, 5] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  맨손 사용 기술치 :\t{mTrainPlayer.FistSkill}[/color]", true);
//						trainSkillMenuList.Add("  맨손 사용 기술치");
//						mTrainSkillList.Add(new Tuple<int, int>(5, swordEnableClass[mTrainPlayer.Class - 1, 5]));
//					}

//					AppendText($"[color={RGB.LightGreen}] 여분의 경험치 :\t{mTrainPlayer.Experience.ToString("#,#0")}[/color]", true);

//					AppendText($"[color={RGB.LightRed}]당신이 수련하고 싶은 부분을 고르시오.[/color]", true);

//					ShowMenu(MenuMode.ChooseTrainSkill, trainSkillMenuList.ToArray(), defaultMenuID);
//				}

//				void ShowTrainMagicMenu(int defaultMenuID)
//				{
//					AppendText($"[color={RGB.White}]{mTrainPlayer.Name}의 현재 능력치[/color]");

//					var trainSkillMenuList = new List<string>();
//					mTrainSkillList.Clear();

//					if (magicEnableClass[mTrainPlayer.Class - 1, 0] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  공격 마법 능력치 :\t{mTrainPlayer.AttackMagic}[/color]", true);
//						trainSkillMenuList.Add("  공격 마법 능력치");
//						mTrainSkillList.Add(new Tuple<int, int>(0, magicEnableClass[mTrainPlayer.Class - 1, 0]));
//					}

//					if (magicEnableClass[mTrainPlayer.Class - 1, 1] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  변화 마법 능력치 :\t{mTrainPlayer.PhenoMagic}[/color]", true);
//						trainSkillMenuList.Add("  변화 마법 능력치");
//						mTrainSkillList.Add(new Tuple<int, int>(1, magicEnableClass[mTrainPlayer.Class - 1, 1]));
//					}

//					if (magicEnableClass[mTrainPlayer.Class - 1, 2] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  치료 마법 능력치 :\t{mTrainPlayer.CureMagic}[/color]", true);
//						trainSkillMenuList.Add("  치료 마법 능력치");
//						mTrainSkillList.Add(new Tuple<int, int>(2, magicEnableClass[mTrainPlayer.Class - 1, 2]));
//					}

//					if (magicEnableClass[mTrainPlayer.Class - 1, 3] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  특수 마법 능력치 :\t{mTrainPlayer.SpecialMagic}[/color]", true);
//						trainSkillMenuList.Add("  특수 마법 능력치");
//						mTrainSkillList.Add(new Tuple<int, int>(3, magicEnableClass[mTrainPlayer.Class - 1, 3]));
//					}

//					if (magicEnableClass[mTrainPlayer.Class - 1, 4] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  초 자연력 능력치 :\t{mTrainPlayer.ESPMagic}[/color]", true);
//						trainSkillMenuList.Add("  초 자연력 능력치");
//						mTrainSkillList.Add(new Tuple<int, int>(4, magicEnableClass[mTrainPlayer.Class - 1, 4]));
//					}

//					if (magicEnableClass[mTrainPlayer.Class - 1, 5] > 0)
//					{
//						AppendText($"[color={RGB.LightCyan}]  소환 마법 능력치 :\t{mTrainPlayer.SummonMagic}[/color]", true);
//						trainSkillMenuList.Add("  소환 마법 능력치");
//						mTrainSkillList.Add(new Tuple<int, int>(5, magicEnableClass[mTrainPlayer.Class - 1, 5]));
//					}

//					AppendText($"[color={RGB.LightGreen}] 여분의 경험치 :\t{mTrainPlayer.Experience.ToString("#,#0")}[/color]", true);

//					AppendText($"[color={RGB.LightRed}]당신이 배우고 싶은 부분을 고르시오.[/color]", true);

//					ShowMenu(MenuMode.ChooseTrainMagic, trainSkillMenuList.ToArray(), defaultMenuID);
//				}

//				void ShowChooseTrainSkillMemberMenu()
//				{
//					AppendText($"[color={RGB.White}]누가 훈련을 받겠습니까?[/color]");
//					ShowCharacterMenu(MenuMode.ChooseTrainSkillMember, false);
//				}

//				void ShowChooseTrainMagicMemberMenu()
//				{
//					AppendText($"[color={RGB.White}]누가 가르침을 받겠습니까?[/color]");
//					ShowCharacterMenu(MenuMode.ChooseTrainMagicMember, false);
//				}

//				bool EnoughMoneyToChangeJob()
//				{
//					if (mParty.Gold < 10_000)
//					{
//						Talk(" 그러나 일행에게는 직업을 바꿀 때 드는 비용인 금 10,000 개가 없습니다.");
//						return false;
//					}
//					else
//						return true;
//				}

//				void ShowChooseChangeSwordMemberMenu()
//				{
//					if (EnoughMoneyToChangeJob())
//					{
//						AppendText($"[color={RGB.White}]누가 전투사 계열의 직업을 바꾸겠습니까?[/color]");
//						ShowCharacterMenu(MenuMode.ChooseChangeSwordMember);
//					}
//				}

//				void ShowChooseChangeMagicMemberMenu()
//				{
//					if (EnoughMoneyToChangeJob())
//					{

//						AppendText($"[color={RGB.White}]누가 마법사 계열의 직업을 바꾸겠습니까?[/color]");
//						ShowCharacterMenu(MenuMode.ChooseChangeMagicMember);
//					}
//				}

//				bool IsUsableWeapon(Lore player, int weapon)
//				{
//					if (player.ClassType == ClassCategory.Magic)
//						return false;
//					else
//					{
//						if ((player.Class == 1 || player.Class == 2 || player.Class == 3 || player.Class == 6 || player.Class == 7) && 1 <= weapon && weapon <= 7)
//							return true;
//						else if ((player.Class == 1 || player.Class == 2 || player.Class == 4) && 8 <= weapon && weapon <= 14)
//							return true;
//						else if ((player.Class == 1 || player.Class == 2 || player.Class == 4 || player.Class == 6 || player.Class == 7) && 15 <= weapon && weapon <= 21)
//							return true;
//						else if ((player.Class == 1 || player.Class == 4 || player.Class == 6 || player.Class == 7) && 22 <= weapon && weapon <= 28)
//							return true;
//						else
//							return false;
//					}
//				}

//				bool IsUsableShield(Lore player)
//				{
//					if (player.ClassType == ClassCategory.Magic)
//						return false;
//					else
//					{
//						if (player.Class == 1 || player.Class == 2 || player.Class == 3 || player.Class == 7)
//							return true;
//						else
//							return false;
//					}
//				}

//				bool IsUsableArmor(Lore player, int armor)
//				{
//					if (player.ClassType == ClassCategory.Magic && armor == 1)
//						return true;
//					else if (player.ClassType == ClassCategory.Sword && ((1 <= armor && armor <= 10) || armor == 255))
//						return true;
//					else
//						return false;
//				}

//				void UpdateItem(Lore player)
//				{
//					var weaponData = new int[,,] {
//						{
//							{ 15, 15, 15, 15, 15, 25, 15 },
//							{ 30, 30, 25, 25, 25, 25, 30 },
//							{ 35, 40, 35, 35, 35, 35, 40 },
//							{ 45, 48, 50, 40, 40, 40, 40 },
//							{ 50, 55, 60, 50, 50, 50, 55 },
//							{ 60, 70, 70, 60, 60, 60, 65 },
//							{ 70, 70, 80, 70, 70, 70, 70 }
//						},
//						{
//							{ 15, 15, 15, 15, 15, 15, 15 },
//							{ 35, 30, 30, 37, 30, 30, 30 },
//							{ 35, 40, 35, 35, 35, 35, 35 },
//							{ 52, 45, 45, 45, 45, 45, 45 },
//							{ 60, 60, 55, 55, 55, 55, 55 },
//							{ 75, 70, 70, 70, 70, 70, 70 },
//							{ 80, 85, 80, 80, 80, 80, 80 }
//						},
//						{
//							{ 10, 10, 10, 25, 10, 20, 10 },
//							{ 35, 40, 35, 35, 35, 35, 40 },
//							{ 35, 30, 30, 35, 30, 30, 30 },
//							{ 40, 40, 40, 45, 40, 40, 40 },
//							{ 60, 60, 60, 60, 60, 60, 60 },
//							{ 80, 80, 80, 80, 80, 80, 80 },
//							{ 90, 90, 90, 90, 90, 90, 90 }
//						},
//						{
//							{ 10, 10, 10, 15, 10, 15, 10 },
//							{ 10, 10, 10, 10, 10, 20, 10 },
//							{ 20, 20, 20, 27, 20, 20, 20 },
//							{ 35, 35, 35, 40, 35, 38, 35 },
//							{ 45, 45, 45, 55, 45, 45, 45 },
//							{ 55, 55, 55, 65, 55, 55, 55 },
//							{ 70, 70, 70, 85, 70, 70, 70 }
//						}
//					};

//					if (IsUsableWeapon(player, player.Weapon))
//					{
//						if (player.Weapon > 0)
//						{
//							int sort = (player.Weapon - 1) / 7;
//							int order = player.Weapon % 7;
//							player.WeaPower = weaponData[sort, order, player.Class - 1];
//						}
//						else
//							player.WeaPower = 5;
//					}

//					if (IsUsableShield(player))
//						player.ShiPower = player.Shield;
//					else
//						player.ShiPower = 0;

//					if (IsUsableArmor(player, player.Armor))
//					{
//						player.ArmPower = player.Armor;
//						if (player.Armor == 255)
//							player.ArmPower = 20;
//					}
//					else
//						player.ArmPower = 0;

//					player.AC = player.PotentialAC + player.ArmPower;
//				}

//				void ShowHealType()
//				{
//					AppendText(new string[] { $"[color={RGB.White}]어떤 치료입니까?[/color]" });

//					ShowMenu(MenuMode.HealType, new string[]
//					{
//						"상처를 치료",
//						"독을 제거",
//						"의식의 회복",
//						"부활"
//					});
//				}

//				void WinMephistopheles()
//				{
//					mParty.Etc[8] |= 1 << 2;

//					Talk(" 일행이 메피스토펠레스를 물리치고는 모든 마법의 힘을 하나로 뭉쳐 지상으로 공간 이동을 시도했다.");

//					mSpecialEvent = SpecialEventType.ReturnToGround;
//				}

//				async Task EndBattle()
//				{
//					void RevivalBerial()
//					{
//						mEncounterEnemyList.Clear();
//						for (var i = 0; i < 4; i++)
//							JoinEnemy(50).Name = "도마뱀 인간";

//						DisplayEnemy();

//						Talk(new string[] {
//							$"[color={RGB.LightMagenta}] 으윽~~~ 하지만 내가 그리 쉽게 너에게 당할 것 같으냐!!",
//							"",
//							" 베리알이 죽으면서 흘린 피가 도마뱀 인간의 시체로 스며 들어갔고 도마뱀 인간은 더욱 강하게 부활을 했다."
//						});

//						mParty.Etc[8] |= 1 << 5;
//						mSpecialEvent = SpecialEventType.BattleRevivalBerial;
//					}

//					void WinAsmodeus()
//					{
//						mParty.Etc[8] |= 1 << 1;

//						Dialog(new string[] {
//							$"[color={RGB.LightMagenta}] 하지만 너의 실력으로도 메피스토펠레스님은 이기지 못할 게다. 그분은 불사불멸의 신적인 존재이니까.[/color]",
//							"",
//							" 아스모데우스는 이 말을 마치고는 쓰러졌다."
//						});
//					}

//					var battleEvent = mBattleEvent;
//					mBattleEvent = BattleEvent.None;

//					if (mBattleTurn == BattleTurn.Win)
//					{
//						mBattleCommandQueue.Clear();
//						mBatteEnemyQueue.Clear();

//						var endMessage = "";

//						if (mParty.Etc[5] == 2)
//							endMessage = "";
//						else
//						{
//#if DEBUG
//							var goldPlus = 10_000;
//#else
//							var goldPlus = 0;
//							foreach (var enemy in mEncounterEnemyList)
//							{
//								var enemyInfo = mEnemyDataList[enemy.ENumber];
//								var point = enemyInfo.AC == 0 ? 1 : enemyInfo.AC;
//								var plus = enemyInfo.Level;
//								plus *= enemyInfo.Level;
//								plus *= enemyInfo.Level;
//								plus *= point;
//								goldPlus += plus;
//							}
//#endif

//							mParty.Gold += goldPlus;

//							endMessage = $"[color={RGB.White}]일행은 {goldPlus.ToString("#,#0")}개의 금을 얻었다.[/color]";

//							AppendText(new string[] { endMessage, "" });
//						}

//						if (battleEvent == BattleEvent.MenaceMurder)
//						{
//							StartBattleEvent(BattleEvent.MenaceMurder);
//							return;
//						}
//						else if (battleEvent == BattleEvent.GuardOfObsidianArmor)
//						{
//							AppendText(" 당신은 보물 파수꾼들을 물리쳤다.");
//							mParty.Etc[44] |= 1 << 3;
//						}
//						else if (battleEvent == BattleEvent.Slaim)
//							mParty.Etc[44] |= 1 << 4;
//						else if (battleEvent == BattleEvent.CaveEntrance)
//							mParty.Etc[44] |= 1 << 5;
//						else if (battleEvent == BattleEvent.CaveOfBerialEntrance)
//						{
//							mParty.Map = 15;
//							mParty.XAxis = 24;
//							mParty.YAxis = 43;

//							await RefreshGame();
//						}
//						else if (battleEvent == BattleEvent.CaveOfAsmodeusEntrance)
//						{
//							mParty.Map = 17;
//							mParty.XAxis = 24;
//							mParty.YAxis = 43;

//							await RefreshGame();

//							mParty.Etc[42] = 0;
//							mParty.Etc[40] |= 1 << 6;

//							Lore slowestPlayer = null;
//							foreach (var player in mPlayerList)
//							{
//								if (player.HP > 0)
//								{
//									if (slowestPlayer == null || player.Agility <= slowestPlayer.Agility)
//										slowestPlayer = player;
//								}
//							}

//							if (mAssistPlayer != null && mAssistPlayer.Agility <= slowestPlayer.Agility)
//								slowestPlayer = mAssistPlayer;

//							AppendText(new string[] {
//								$"[color={RGB.LightMagenta}] 우욱... 하지만 나는 죽더라도 한 사람은 지옥으로 보내 주겠다.[/color]",
//								"",
//								" 가디안 레프트는 죽기 직전에 일행의 뒤에서 거대한 마법 독화살을 쏘았다.",
//								"",
//								$"[color={RGB.LightRed}] 가디안 레프트의 마법 독화살은 일행 중 가장 민첩성이 낮은 {slowestPlayer.Name}에게 명중했다.[/color]",
//								$"[color={RGB.LightRed}] 그리고, {slowestPlayer.NameSubjectJosa} 즉사했다.[/color]"
//							});

//							slowestPlayer.HP = 0;
//							slowestPlayer.Poison = 1;
//							slowestPlayer.Unconscious = slowestPlayer.Endurance * slowestPlayer.Level * 10 - 2;
//							if (slowestPlayer.Unconscious < 1)
//								slowestPlayer.Unconscious = 1;
//							slowestPlayer.Dead = 1;

//							UpdatePlayersStat();
//						}
//						else if (battleEvent == BattleEvent.Wisp)
//							mParty.Etc[41] |= 1;
//						else if (battleEvent == BattleEvent.RockMan)
//						{
//							mEncounterEnemyList.Clear();

//							for (var i = 0; i < 6; i++)
//								JoinEnemy(46);

//							DisplayEnemy();

//							Talk($"[color={RGB.LightMagenta}] 누가 우리 애들을 건드리느냐! 이 사이클롭스님의 곤봉 맛을 보아라!!");

//							mSpecialEvent = SpecialEventType.BattleCyclopes;
//							return;
//						}
//						else if (battleEvent == BattleEvent.Cyclopes)
//						{
//							mEncounterEnemyList.Clear();

//							for (var i = 0; i < 3; i++)
//								JoinEnemy(46);

//							for (var i = 0; i < 2; i++)
//								JoinEnemy(58);

//							JoinEnemy(62);

//							DisplayEnemy();

//							Talk($"[color={RGB.LightMagenta}] 나는 이곳의 대장인 죽음의 기사님이시다. 더 이상의 희생이 나기 전에 나의 손에서 끝을 맺어야겠군. 자 그럼 받아랏!!!![/color]");

//							mSpecialEvent = SpecialEventType.BattleDeathKnight;
//							return;
//						}
//						else if (battleEvent == BattleEvent.DeathKnight)
//						{
//							mParty.Etc[41] |= 1 << 1;
//							Ask(new string[] {
//								" 일행은 죽음의 기사를 물리쳤다. 하지만 죽음의 기사는 완전히 죽지 않고 우리에게 말을 걸었다.",
//								"",
//								$"[color={RGB.LightMagenta}] 으윽~~~ 나는 여지껏 여기서 산적 생활을 해왔지만 당신들처럼 강한 상대는 보지 못했소." +
//								" 혹시 내가 당신들 일행의 끄트 머리에 따라다니며 한 수 배워도 되겠소?[/color]"
//							}, MenuMode.JoinDeathKnight, new string[] {
//								"좋소, 당신이 원한다면",
//								"아니 되오, 산적을 일행으로 둘 수는 없소"
//							});
//						}
//						else if (battleEvent == BattleEvent.DeathSoul)
//						{
//							mParty.Etc[41] |= 1 << 2;
//						}
//						else if (battleEvent == BattleEvent.WarriorOfCrux)
//							mParty.Etc[41] |= 1 << 3;
//						else if (battleEvent == BattleEvent.CaveOfBerial)
//							mParty.Etc[40] |= 1 << 2;
//						else if (battleEvent == BattleEvent.Gagoyle)
//							mParty.Etc[40] |= 1 << 3;
//						else if (battleEvent == BattleEvent.CaveOfBerialCyclopes)
//							mParty.Etc[40] |= 1 << 4;
//						else if (battleEvent == BattleEvent.Berial)
//						{
//							RevivalBerial();
//							return;
//						}
//						else if (battleEvent == BattleEvent.Illusion)
//						{
//							Talk($"[color={RGB.LightMagenta}] 하지만 나의 마법은 이 정도로 끝나는 것이 아니라오." +
//							" 다시 한번 당신의 환상에 빠져들어 보시오. 하하하하 ~~[/color]");

//							mSpecialEvent = SpecialEventType.SummonIllusion;

//							return;
//						}
//						else if (battleEvent == BattleEvent.Molok)
//						{
//							mParty.Etc[8] |= 1 << 6;

//							Talk(new string[] {
//								$"[color={RGB.LightMagenta}] 휴~~~ 겨우 몰록의 결계에서 벗어났군. 나의 출현이 도움이 되었나? 나는 또 다음에 나타나도록 하지. 그때까지 안녕히 ...[/color]",
//								"",
//								" 레드 안타레스는 점점 희미해지더니 이내 사라지고 말았다."
//							});

//							mSpecialEvent = SpecialEventType.WinMolok;
//						}
//						else if (battleEvent == BattleEvent.Dragon)
//						{
//							Dialog(" 용의 동굴에는 동쪽으로 이어지는 통로가 있었다.");

//							for (var x = 20; x < 29; x++)
//								UpdateTileInfo(x, 71, 41);
//						}
//						else if (battleEvent == BattleEvent.FlyingDragon)
//						{
//							Dialog(" 비룡들을 물리치자 조금 전에는 보지 못했던 통로가 있었다.");

//							for (var x = 20; x < 29; x++)
//								UpdateTileInfo(x, 73, 41);
//						}
//						else if (battleEvent == BattleEvent.MachineRobot)
//						{
//							UpdateTileInfo(35, 43, 41);
//							mParty.Etc[41] |= 1 << 4;
//						}
//						else if (battleEvent == BattleEvent.Asmodeus)
//							WinAsmodeus();
//						else if (battleEvent == BattleEvent.Prison)
//						{
//							Dialog($"[color={RGB.White}]당신들은 수감소 병사들을 물리쳤다.[/color]");

//							mParty.Etc[29] |= 1 << 4;

//							UpdateTileInfo(50, 11, 44);
//							UpdateTileInfo(51, 11, 44);

//							UpdateTileInfo(49, 10, 44);
//							UpdateTileInfo(52, 10, 44);
//						}
//						else if (battleEvent == BattleEvent.LastGuardian)
//							mParty.Etc[41] |= 1 << 5;
//						else if (battleEvent == BattleEvent.Mephistopheles)
//						{
//							WinMephistopheles();
//						}

//						mEncounterEnemyList.Clear();
//						mBattleEvent = 0;

//						ShowMap();

//					}
//					else if (mBattleTurn == BattleTurn.RunAway)
//					{
//						AppendText(new string[] { "" });

//						mBattlePlayerID = 0;
//						while (!mPlayerList[mBattlePlayerID].IsAvailable && mBattlePlayerID < mPlayerList.Count)
//							mBattlePlayerID++;

//						if (battleEvent == BattleEvent.MenaceMurder)
//						{
//							ShowMap();
//							Talk(" 하지만 너무 많은 적들에게 포위되어 도망갈 수가 없었다.");

//							mBattleEvent = BattleEvent.MenaceMurder;
//							mSpecialEvent = SpecialEventType.BackToBattleMode;
//							return;
//						}
//						else if (battleEvent == BattleEvent.GuardOfObsidianArmor)
//							mParty.YAxis++;
//						else if (battleEvent == BattleEvent.Slaim)
//							mParty.YAxis--;
//						else if (battleEvent == BattleEvent.CaveEntrance)
//							mParty.YAxis++;
//						else if (battleEvent == BattleEvent.RockMan)
//							mParty.YAxis--;
//						else if (battleEvent == BattleEvent.Cyclopes)
//							mParty.YAxis--;
//						else if (battleEvent == BattleEvent.DeathKnight)
//							mParty.YAxis--;
//						else if (battleEvent == BattleEvent.DeathSoul)
//							mParty.YAxis--;
//						else if (battleEvent == BattleEvent.CaveOfBerial)
//							mParty.YAxis--;
//						else if (battleEvent == BattleEvent.Gagoyle)
//							mParty.YAxis++;
//						else if (battleEvent == BattleEvent.CaveOfBerialCyclopes)
//						{
//							ShowMap();
//							Talk(" 하지만 적은 끝까지 우리를 따라붙었다.");

//							mBattleEvent = BattleEvent.CaveOfBerialCyclopes;
//							mSpecialEvent = SpecialEventType.BackToBattleMode;
//							return;
//						}
//						else if (battleEvent == BattleEvent.Berial)
//						{
//							if (mEncounterEnemyList[4].Dead)
//							{
//								RevivalBerial();
//								return;
//							}
//							else
//							{
//								mParty.XAxis = 24;
//								mParty.YAxis = 43;
//							}
//						}
//						else if (battleEvent == BattleEvent.RevivalBerial)
//						{
//							mParty.XAxis = 24;
//							mParty.YAxis = 43;
//						}
//						else if (battleEvent == BattleEvent.Illusion)
//						{
//							ShowMap();
//							Talk(" 하지만 일행은 환상에서 헤어날 수 없었다.");

//							mBattleEvent = BattleEvent.Illusion;
//							mSpecialEvent = SpecialEventType.BackToBattleMode;
//							return;
//						}
//						else if (battleEvent == BattleEvent.Molok)
//						{
//							ShowMap();
//							Talk(" 하지만 일행은 몰록의 결계에서 벗어날 수 없었다.");

//							mBattleEvent = BattleEvent.Illusion;
//							mSpecialEvent = SpecialEventType.BackToBattleMode;
//							return;
//						}
//						else if (battleEvent == BattleEvent.Dragon)
//							mParty.YAxis--;
//						else if (battleEvent == BattleEvent.FlyingDragon)
//							mParty.YAxis--;
//						else if (battleEvent == BattleEvent.MachineRobot)
//							mParty.YAxis++;
//						else if (battleEvent == BattleEvent.Asmodeus)
//						{
//							if (!mEncounterEnemyList[7].Dead)
//								mParty.YAxis++;
//							else
//								WinAsmodeus();
//						}
//						else if (battleEvent == BattleEvent.LastGuardian)
//						{
//							mParty.XAxis = mPrevX;
//							mParty.YAxis = mPrevY;
//						}
//						else if (battleEvent == BattleEvent.Mephistopheles)
//						{
//							if (!mEncounterEnemyList[7].Dead)
//							{
//								Talk(" 하지만 메피스토펠레스는 일행의 도주를 허용하지 않았다.");

//								mBattleEvent = BattleEvent.Mephistopheles;
//								mSpecialEvent = SpecialEventType.BackToBattleMode;

//								return;
//							}
//							else
//								WinMephistopheles();
//						}
//						else if (battleEvent == BattleEvent.CyllianCominus)
//						{
//							Talk($"[color={RGB.LightMagenta}] 크크크.. 너희들은 반드시 나의 손에 죽어 줘야겠다. 크크크..[/color]");

//							mBattleEvent = BattleEvent.CyllianCominus;
//							mSpecialEvent = SpecialEventType.BackToBattleMode;

//							return;
//						}

//						mEncounterEnemyList.Clear();
//						ShowMap();
//					}
//					else if (mBattleTurn == BattleTurn.Lose)
//					{
//						if (battleEvent == BattleEvent.MenaceMurder)
//						{
//							Talk(new string[] {
//								$"[color={RGB.LightMagenta}] 당신은 악마 사냥꾼에게 기습을 받아서 거의 다 죽게 되었을 때 갑자기 낯익은 목소리가 먼 곳에서 들려 왔다.[/color]",
//								$"[color={RGB.LightMagenta}] {mPlayerList[0].Name}. 나는 레드 안타레스일세. 당신도 실력이 많이 줄었군. 이런 조무래기들에게 당하다니." +
//								" 그럼 약간의 도움을 주도록 하지. 잘 보게나.[/color]"
//							});

//							mSpecialEvent = SpecialEventType.HelpRedAntares;
//						}
//						else if (battleEvent == BattleEvent.Illusion)
//						{
//							Dialog(new string[] {
//								" 일행이 환상에 빠져 스스로 파멸하고 있을 때 멀리서 귀에 익은 음성이 들려왔다.",
//								"",
//								$"[color={RGB.LightGreen}] {mPlayerList[0].Name}. 나는 레드 안타레스일세.[/color]",
//								$"[color={RGB.LightGreen}] 다시 또 만나게 됐군. 상당히 위험한 상황인 것 같군. 그럼 내가 환상을 깨어 주도록 하지.[/color]",
//							});

//							InvokeAnimation(AnimationType.RemoveIllusion);
//						}
//						else
//						{
//							ShowGameOver(new string[] {
//								$"[color={RGB.LightMagenta}]일행은 모두 전투에서 패했다!![/color]",
//								$"[color={RGB.LightGreen}]    어떻게 하시겠습니까?[/color]"
//							});
//						}
//					}

//					mBattleTurn = BattleTurn.None;
//				}

//				void ShowCureResult(bool battleCure)
//				{
//					if (battleCure)
//					{
//						Talk(mCureResult.ToArray());
//						mSpecialEvent = SpecialEventType.SkipTurn;
//					}
//					else
//						Dialog(mCureResult.ToArray());
//				}

//				void ShowWeaponTypeMenu(int weaponCategory)
//				{
//					mWeaponTypeID = weaponCategory;

//					if (0 <= weaponCategory && weaponCategory <= 3)
//					{
//						AppendText($"[color={RGB.White}]어떤 무기를 원하십니까?[/color]");

//						var weaponNameArr = new string[7];
//						for (var i = 1; i <= 7; i++)
//						{
//							if (Common.GetWeaponName(mWeaponTypeID * 7 + i).Length < 3)
//								weaponNameArr[i - 1] = $"{Common.GetWeaponName(mWeaponTypeID * 7 + i)}\t\t\t금 {weaponPrice[mWeaponTypeID, i - 1].ToString("#,#0")} 개";
//							else if (Common.GetWeaponName(mWeaponTypeID * 7 + i).Length < 5)
//								weaponNameArr[i - 1] = $"{Common.GetWeaponName(mWeaponTypeID * 7 + i)}\t\t금 {weaponPrice[mWeaponTypeID, i - 1].ToString("#,#0")} 개";
//							else

//								weaponNameArr[i - 1] = $"{Common.GetWeaponName(mWeaponTypeID * 7 + i)}\t금 {weaponPrice[mWeaponTypeID, i - 1].ToString("#,#0")} 개";
//						}

//						ShowMenu(MenuMode.BuyWeapon, weaponNameArr);
//					}
//					else if (weaponCategory == 4)
//					{
//						AppendText($"[color={RGB.White}]어떤 방패를 원하십니까?[/color]");

//						var shieldNameArr = new string[5];
//						for (var i = 1; i <= 5; i++)
//						{
//							if (Common.GetShieldName(i).Length <= 5)
//								shieldNameArr[i - 1] = $"{Common.GetShieldName(i)}\t\t금 {shieldPrice[i - 1].ToString("#,#0")} 개";
//							else
//								shieldNameArr[i - 1] = $"{Common.GetShieldName(i)}\t금 {shieldPrice[i - 1].ToString("#,#0")} 개";
//						}

//						ShowMenu(MenuMode.BuyShield, shieldNameArr);
//					}
//					else if (weaponCategory == 5)
//					{
//						AppendText($"[color={RGB.White}]어떤 갑옷을 원하십니까?[/color]");

//						var armorNameArr = new string[10];
//						for (var i = 1; i <= 10; i++)
//						{
//							if (Common.GetArmorName(i).Length <= 5)
//								armorNameArr[i - 1] = $"{Common.GetArmorName(i)}\t\t금 {armorPrice[i - 1].ToString("#,#0")} 개";
//							else
//								armorNameArr[i - 1] = $"{Common.GetArmorName(i)}\t금 {armorPrice[i - 1].ToString("#,#0")} 개";
//						}

//						ShowMenu(MenuMode.BuyArmor, armorNameArr);
//					}
//				}

				if (mLoading || (mAnimationEvent != AnimationType.None && ContinueText.Visibility != Visibility.Visible && mMenuMode == MenuMode.None) || mTriggeredDownEvent)
				{
					mTriggeredDownEvent = false;
					return;
				}
				else if (EndingMessage.Visibility == Visibility.Visible)
				{
					//EndingMessage.Visibility = Visibility.Collapsed;

					//mParty.Map = 6;
					//mParty.XAxis = 90;
					//mParty.YAxis = 79;

					//mParty.Etc[0] = 255;
					//await RefreshGame();
					//BGMPlayer.Source = null;

					//InvokeAnimation(AnimationType.Cookie);
				}
				else if (ContinueText.Visibility == Visibility.Visible)
				{
					async Task InvokeSpecialEventLaterPart()
					{
						var specialEvent = mSpecialEvent;
						mSpecialEvent = SpecialEventType.None;

						//if (specialEvent == SpecialEventType.CantTrain)
						//	ShowTrainSkillMenu(mMenuFocusID);
						//else if (specialEvent == SpecialEventType.CantTrainMagic)
						//	ShowTrainMagicMenu(mMenuFocusID);
						//else if (specialEvent == SpecialEventType.TrainSkill)
						//	ShowChooseTrainSkillMemberMenu();
						//else if (specialEvent == SpecialEventType.TrainMagic)
						//	ShowChooseTrainMagicMemberMenu();
						//else if (specialEvent == SpecialEventType.ChangeJobForSword)
						//	ShowChooseChangeSwordMemberMenu();
						//else if (specialEvent == SpecialEventType.ChangeJobForMagic)
						//	ShowChooseChangeMagicMemberMenu();
						//else if (specialEvent == SpecialEventType.CureComplete)
						//{
						//	AppendText($"[color={RGB.White}]누가 치료를 받겠습니까?[/color]");

						//	ShowCharacterMenu(MenuMode.Hospital);
						//}
						//else if (specialEvent == SpecialEventType.NotCured)
						//{
						//	ShowHealType();
						//}
						//else if (specialEvent == SpecialEventType.LeaveSoldier)
						//	InvokeAnimation(AnimationType.LeaveSoldier);
						//else if (specialEvent == SpecialEventType.ViewGeniusKieLetter)
						//{
						//	Talk(new string[] {
						//		$"{mPlayerList[0].Name}에게.",
						//		"",
						//		"",
						//		" 자네라면 여기에 올 거라고 믿고 이 글을 쓴다네. 나는 지금 은신 중이라 내가 있는 곳을 밝힐 순 없지만 나는 지금 무사하다네." +
						//		" 자네와의 기나긴 모험을 끝마치고 돌아오던 중 나는 나름대로의 뜻이 있어 로어 성에 돌아오지 않았다네." +
						//		" 나 자신의 의지였다면 영영 로어 성에 돌아오지 않았겠지만 곧 발생할 새로운 위협을 나는 느끼고 있기 때문에 자네에게 이런 쪽지를 보내게 되었네."
						//	});

						//	mSpecialEvent = SpecialEventType.ViewGeniusKieLetter2;
						//}
						//else if (specialEvent == SpecialEventType.ViewGeniusKieLetter2)
						//{
						//	AppendText(new string[] {
						//		$" 로어 성까지는 소문이 미치지 않았는지는 모르겠지만 로어 대륙의 남동쪽에는 알 수 없는 피라미드가 땅속으로부터 솟아올랐다네." +
						//		" 나와 로어 헌터 둘이서 그곳을 탐험했었지. 그곳에는 다시 두 개의 동굴이 있었고 그 두 곳은 지하 세계와 연결되어 있었다네." +
						//		" 그곳에 대해서는 지금 이 메모의 여백이 좁아서 말하기 어렵다네." +
						//		" 나는 지금 이곳, 저곳 떠돌아다니지만 북동쪽 해안의 오두막에 살고 있는 전투승 레굴루스에게 물어보면 내가 있는 곳을 자세히 알 수 있을 걸세.",
						//		"",
						//		"",
						//		"",
						//		"                          지니어스 기로부터"
						//	});
						//}
						//else if (specialEvent == SpecialEventType.TalkPrisoner)
						//{
						//	AppendText(new string[] {
						//		" 하지만 더욱 이상한 것은 로드 안 자신도 그에 대한 사실을 인정하면서도 왜 우리에게는 그를 배격하도록만 교육하는 가를 알고 싶을 뿐입니다." +
						//	" 로드 안께서는 나를 이해한다고 하셨지만 사회 혼란을 방지하기 위해 나를 이렇게 밖에 할 수 없다고 말씀하시더군요." +
						//	" 그리고 이것은 선을 대표하는 자기로서는 이 방법 밖에는 없다고 하시더군요.",
						//		" 하지만 로드 안의 마음은 사실 이렇지 않다는 걸 알수 있었습니다. 에인션트 이블의 말로는 사실 서로가 매우 절친한 관계임을 알 수가 있었기 때문입니다."
						//	});
						//}
						//else if (specialEvent == SpecialEventType.MeetLordAhn)
						//{
						//	Talk(" 또 자네의 힘을 빌릴 때가 온 것 같네. 새로운 마력이 온통 이 세계를 휘감고 있다네." +
						//	$" 그것을 내가 깨달았을 때는 이미 그 새로운 존재가 민심을 선동하고 있었다네." +
						//	$" 주민들은 [color={RGB.LightCyan}]다크 메이지[/color]가 있어서 곧 여기를 침공할 거라고 하며 나에게 방어를 요청했다네." +
						//	" 하지만 다크 메이지란 존재는 내가 알기로도 존재하지 않으며 나와 연대 관계에 있는 에인션트 이블도 역시 그런 존재를 모르고 있었다네." +
						//	" 하지만 주민들은 어떻게 알았는지 그 존재를 말하고 있다네. 그럼 이번에 해야 할 임무를 말해 주겠네.");

						//	mSpecialEvent = SpecialEventType.MeetLordAhn2;
						//}
						//else if (specialEvent == SpecialEventType.MeetLordAhn2)
						//{
						//	AppendText(new string[] {
						//		" 자네는 메너스란 동굴을 기억할 걸세. 그곳은 네크로만서가 사라진 후 파괴되었고 지금은 다시 원래대로 광산이 되었다네." +
						//	" 하지만 언제부턴가 그곳은 잇단 의문의 살인 때문에 지금은 거의 폐광이 되다시피 한 곳이라네." +
						//	" 주민들은 그 살인이 모두 다크 메이지의 짓이라고들 하고 있네. 나도 다크 메이지의 존재를 믿고 싶지는 않지만 일이 이렇게 되었으니 어쩔 수 없다네.",
						//		" 지금 즉시 메너스로 가서 진상을 밝혀 주게.",
						//		" 그리고 다크 메이지에 대한 정보도 알아 오도록 하게. 무기는 무기고에서 가져가도록 허락하지. 부탁하네.",
						//		$"[color={RGB.LightCyan}] [[ 경험치 + 10,000 ] [[ 황금 + 1,000 ][/color]"
						//	});

						//	mPlayerList[0].Experience += 10_000;
						//	mParty.Gold += 1_000;
						//	mParty.Etc[9]++;

						//	mSpecialEvent = SpecialEventType.None;
						//}
						//else if (specialEvent == SpecialEventType.MeetLordAhn3)
						//{
						//	Talk(new string[] {
						//		" 그리고 내가 다크 메이지의 기원을 여러 방면으로 알아보던 중에 이 책을 찾아내었네.",
						//		$" 이 책은 자네도 알다시피 [color={RGB.LightCyan}]알비레오[/color]라고 하는 타임 워커가 이 대륙에 남기고 간 예언서이지." +
						//		" 중요한 부분만 해석하면 다음과 같네."
						//	});

						//	mSpecialEvent = SpecialEventType.MeetLordAhn4;
						//}
						//else if (specialEvent == SpecialEventType.MeetLordAhn4)
						//{
						//	Talk(new string[] {
						//		$"[color={RGB.White}]첫 번째 흉성이 나타나는 날, 평화는 로어의 신과 함께 대지로 추락할 것이며 공간이 어긋나고 대륙이 진동하며 새로운 존재가 나타난다.[/color]",
						//		"",
						//		" 이 글은 저번에 네크로만서가 나타날 때의 그 광경을 묘사한 구절이란 것을 금방 알 수 있네.",
						//		"",
						//		$"[color={RGB.White}]두 번째 흉성이 나타나는 날, 그는 용암의 대륙으로부터 세상을 뒤흔들게 되며 그는 네크로만서라 불린다.[/color]",
						//		"",
						//		" 이 글 또한 몇 년 전의 일과 일치하네.",
						//		"",
						//		$"[color={RGB.White}]세 번째 흉성이 나타나는 날, 네크로만서를 이기는 자는 아무도 없게 된다.[/color]",
						//		"",
						//		" 하지만 이 글은 사실과 다르다네. 자네가 네크로만서를 물리쳤기 때문에 말일세.",
						//		"",
						//		$"[color={RGB.White}]네 번째 흉성이 나타나는 날, 메너스의 달이 붉게 물들 때 어둠의 영혼이 나타나 세계의 종말을 예고한다.[/color]",
						//		"",
						//		" 이 글이 가장 중요한 요지라네. '메너스의 달이 붉게 물들 때' 란 메너스에서 일어난 지금까지의 살인 사건을 말하는 것이고," +
						//		" '영혼'이란 예부터 전승되는 구전에 의하면 영혼의 힘을 이용할 수 있는 사람, 즉 마법사를 지칭하는 말이 된다네." +
						//		" 그러므로 '어둠의 영혼'은 바로 어둠의 마법사를 뜻하는 말이 되네." +
						//		" 다시 풀이하면 그 뜻은 '메너스에서 살인 사건이 일어날 때 다크 메이지가 세계의 종말을 예고한다'라는 말이 된다네.",
						//		"",
						//		$"[color={RGB.White}]다섯 번째 흉성이 나타나는 날, 내가 본 다섯 번의 흉성 중에 하나가 나타나지 않았음을 알아낸다.[/color]",
						//		"",
						//		" 위에서의 예언을 보면 하나가 틀려 있지. 바로 세 번째 흉성이 떨어질 때의 일 말일세. 그것이 틀렸다는 그 말일세." +
						//		" 알비레오 그 자신도 자네가 네크로만서를 물리치리라고는 생각하지 못했는데 자네 해 내었기 때문에 이런 말을 적었던 것 같네."
						//	});

						//	mSpecialEvent = SpecialEventType.MeetLordAhn5;
						//}
						//else if (specialEvent == SpecialEventType.MeetLordAhn5)
						//{
						//	AppendText(new string[] {
						//		" 이 예언에 의하면 반드시 다크 메이지가 세상을 멸망 시키게 된다네. 알비레오 그 자신도 그것을 알려 주려 했던 것이고." +
						//		" 그렇다면 우리는 이번에도 저번의 네크로만서때 처럼 스스로의 운명을 바꾸기 위해 도전 보아야 한다는 결론을 얻을 수 있게 되네.",
						//		" 그럼 자네에게 또 하나의 할 일을 주겠네. 자네는 지금 즉시 라스트디치 성에 가도록 하게." +
						//		" 그곳에서도 우리와 같은 위기를 느끼고 있을 것이고 이보다 더 많은 정보가 있을 수도 있을 테니 한시바삐 그곳으로 가보도록 하게."
						//	});

						//	mParty.Etc[9]++;
						//}
						//else if (specialEvent == SpecialEventType.MeetLordAhn6)
						//{
						//	Talk(new string[] {
						//		" 이 석판의 내용을 읽어 보겠네.",
						//		$"[color={RGB.White}] 어둠은 달이며 달은 여성이라." +
						//		$" 이 세계가 아르테미스를 3번 범할 때 어둠은 깨어나고 다크 메이지는[/color] [color={RGB.Yellow}]실리안 카미너스(Cyllian Cominus)[/color][color={RGB.White}]라고 불린다." +
						//		" 그리고 그녀는 누구도 당해낼 수 없는 마력으로 세계를 종말로 이끄노니....[/color]",
						//		"",
						//		" 이 글의 해석은 다음과 같다네.",
						//		" \"어둠의 마법사\"는 여자 마법사를 뜻하며, \"세계가 아르테미스를 범한다\"라는 구절은 지구가 달(아르테미스)을 가리는 현상," +
						//		" 즉 월식을 말하는 것이며 그 월식이 3번 일어난 후 \"어둠이 깨어난다\" 즉 실리안 카미너스라는 존재가 생겨난다는 것이네. 그러고는 그녀가 이 세계를 멸망시킨다는 것이네."
						//	});

						//	mSpecialEvent = SpecialEventType.MeetLordAhn7;
						//}
						//else if (specialEvent == SpecialEventType.MeetLordAhn7)
						//{
						//	Talk(new string[] {
						//		" 이 글이 정말이라면 알비레오의 예언은 실현되고 반드시 세계는 멸망하게 될 걸세. 하지만 자네는 언제나 운명에 대항에 왔으니 이런 운명적인 것에는 익숙하겠지?" +
						//		" 네크로만서와 싸울 때도 그랬으니까.",
						//		$" 그리고 [color={RGB.White}]흉성의 단서[/color]에서 가져온 이 석판은 도저히 나의 힘으로는 해독이 안되는군." +
						//		" 이 세계의 운명이 달린 일이니 지금 당장 에인션트 이블과 함께 상의해봐야겠네.",
						//		" 그럼 내일 아침에 보도록 하세."
						//	});

						//	mSpecialEvent = SpecialEventType.SleepLoreCastle;
						//	mParty.Etc[9]++;
						//}
						//else if (specialEvent == SpecialEventType.SleepLoreCastle)
						//	InvokeAnimation(AnimationType.SleepLoreCastle);
						//else if (specialEvent == SpecialEventType.MeetLordAhn8)
						//	InvokeAnimation(AnimationType.TalkLordAhn);
						//else if (specialEvent == SpecialEventType.MeetLordAhn9)
						//{
						//	mAnimationEvent = AnimationType.None;
						//	mAnimationFrame = 0;

						//	var eclipseDay = mParty.Day + 15;
						//	var eclipseYear = mParty.Year;

						//	if (eclipseDay > 360)
						//	{
						//		eclipseYear++;
						//		eclipseDay %= 360;
						//	}

						//	AppendText(new string[] {
						//		" 자네 말을 들어 보니 정말 큰일이군. 이러다간 정말 알비레오의 예언처럼 되어 버리겠는데." +
						//		" 자네가 지하 세계에 내려간 후 이곳에도 많은 변화가 있었네. 갑자기 달의 운행이 빨라졌다네." +
						//		" 원래는 월식이 일어날 수 있는 보름달이 29.5일에 한 번이었는데. 이제는 훨씬 더 빨라졌다네." +
						//		" 달의 운행이 빨라져서 달은 원심력이 증가했고 달을 붙잡아 두기 위해서 지구의 중력장이 증가했다네." +
						//		" 지금은 거의 문제가 안될 정도이지만 이 상황이 점점 악화된다면 지구는 백색 왜성이나 블랙홀처럼 스스로의 중력에 의해 파괴될지도 모른다네.",
						//		$" 그리고 다음 월식이 일어날 날짜가 계산되었다네. 날짜는 15일 뒤인 {eclipseYear}년 {eclipseDay / 30 + 1}월 {eclipseDay % 30 + 1}일로 예정되어 있다네." +
						//		" 그때까지 스스로를 단련 시키게. 그때 역시 잘 부탁하네."
						//	});

						//	mParty.Etc[36] = eclipseDay / 256;
						//	mParty.Etc[35] = eclipseDay % 256;
						//	mParty.Etc[38] = eclipseYear / 256;
						//	mParty.Etc[37] = eclipseYear % 256;

						//	mParty.Etc[9]++;
						//}
						//else if (specialEvent == SpecialEventType.MeetLordAhn10)
						//	InvokeAnimation(AnimationType.TalkLordAhn2);
						//else if (specialEvent == SpecialEventType.MeetLordAhn11)
						//{
						//	mAnimationEvent = AnimationType.None;
						//	mAnimationFrame = 0;

						//	var eclipseDay = mParty.Day + 5;
						//	var eclipseYear = mParty.Year;

						//	if (eclipseDay > 360)
						//	{
						//		eclipseYear++;
						//		eclipseDay %= 360;
						//	}

						//	AppendText(new string[] {
						//		" 음... 자네 말을 들어보니 정말 고생이 많았었군. 그리고 악의 추종자 두 명을 처단한 일도 정말 수고했네." +
						//		" 하지만 벌써 마지막 세 번째 월식 날짜가 임박했네. 사실 걱정은 바로 이것이네." +
						//		" 알비레오의 예언에 나오는 다크 메이지 실리안 카미너스가 부활하게 되는 시간은 바로 이번 월식이 일어나는 때라네. 아마 이번이 마지막 파견인 것 같군. 그럼 날짜를 알려주지.",
						//		$" 마지막 세 번째 월식은 바로 5일 뒤인 {eclipseYear}년 {eclipseDay / 30 + 1}월 {eclipseDay % 30 + 1}일이라네." +
						//		" 자네가 만약 성공한다면 다시 로어의 세계는 평화가 올 것이지만 만약 실패한다면 우리가 이렇게 만나는 것도 마지막이 된다네."
						//	});

						//	mParty.Etc[36] = eclipseDay / 256;
						//	mParty.Etc[35] = eclipseDay % 256;
						//	mParty.Etc[38] = eclipseYear / 256;
						//	mParty.Etc[37] = eclipseYear % 256;

						//	mParty.Etc[9]++;
						//}
						//else if (specialEvent == SpecialEventType.DestructCastleLore)
						//{
						//	Talk(new string[] {
						//		" 다크 메이지는 가공할 힘으로 전 대륙에 결계를 형성하기 시작했고 결계 속의 물체들은 서서히 형체를 잃어가기 시작했다." +
						//		" 이제는 실리안 카미너스를 제어할 수 있는 메피스토펠레스마저 사라져 버려서 그녀는 의지의 중심을 잃고 한없이 그녀의 힘을 방출하기 시작했다.",
						//		" 이제는 누구도 그녀의 기하급수적인 힘의 폭주를 막을 수가 없었고 이미 당신의 의식도 흐려져갔다."
						//	});

						//	mSpecialEvent = SpecialEventType.DestructCastleLore2;
						//}
						//else if (specialEvent == SpecialEventType.DestructCastleLore2)
						//{
						//	Talk(new string[] {
						//		" 하지만 이때, 실리안 카미너스의 마법 결계를 깨트리며 희미한 의지가 스며들어왔다." +
						//		" 그것은 레드 안타레스의 마지막 의지였고 그는 그녀의 결계를 버티며 마지막 말을 한다.",
						//		"",
						//		$"[color={RGB.LightMagenta}] 다크 메이지의 힘은 영혼인 나조차도 소멸 시킬 힘이 있다는 걸 알았네." +
						//		" 지금은 나의 마법력으로 나 혼자 정도를 보호할 수 있는 결계를 구성했지만 곧 나의 힘도 다하게 될 걸세. 나는 마지막으로 한 가지의 묘안을 생각해 내었네." +
						//		" 자네는 알비레오의 예언을 기억하겠지. 그중 마지막 예언을 외워보게. 거기에는 대단한 모순이 있네." +
						//		" 지금은 그의 세 번째 예언인 \"네크로만서에게 이긴 자는 아무도 없었다.\"라는 구절이 실행되지 않은 예언이라고 생각되고 있지. 하지만 반대로 생각해 보게." +
						//		" 자네가 만약 지금이라도 네크로만서에게 패한다면 네 번째의 예언이 실현되지 않는 예언이 된다네." +
						//		" 그렇게만 된다면 다크 메이지는 처음부터 없었던 존재가 되어 버리고 로어 대륙은 원래의 모습대로 시간을 진행해 나가게 된다네." +
						//		" 나의 결계가 점점 약해져 가는군. 나의 소멸을 헛되게 하지는 말게. 그리고 자신보다 로어의 운명을 먼저 생각하도록 하게.... 그.. 그럼... 다.. 다음 세상에서......[/color]"
						//	});

						//	mSpecialEvent = SpecialEventType.DestructCastleLore3;
						//}
						//else if (specialEvent == SpecialEventType.DestructCastleLore3)
						//{
						//	AppendText(new string[] {
						//		" 그의 의지는 점점 희미해지더니 결국 암흑으로 사라지고 말았다.",
						//		" 당신은 어떻게 하겠는가?"
						//	});

						//	ShowMenu(MenuMode.FinalChoice, new string[] {
						//		"레드 안타레스의 말대로 한다",
						//		"다크 메이지와 싸우고 싶다"
						//	});
						//}
						//else if (specialEvent == SpecialEventType.EnterUnderworld)
						//{
						//	InvokeAnimation(AnimationType.EnterUnderworld);
						//}
						//else if (specialEvent == SpecialEventType.SeeDeadBody)
						//{
						//	mFace = 5;
						//	InvokeAnimation(AnimationType.GoInsideMenace);
						//}
						//else if (specialEvent == SpecialEventType.BattleMenace)
						//{
						//	mBattleEvent = BattleEvent.MenaceMurder;
						//	StartBattle(false);
						//}
						//else if (specialEvent == SpecialEventType.BackToBattleMode)
						//{
						//	if (BattlePanel.Visibility == Visibility.Collapsed)
						//		DisplayEnemy();
						//	BattleMode();
						//}
						//else if (specialEvent == SpecialEventType.HelpRedAntares)
						//{
						//	AppendText(new string[] { $"[color={RGB.White}] 레드 안타레스는 '차원 이탈'을 모든 적에게 사용했다.[/color]", "" });

						//	foreach (var enemy in mEncounterEnemyList)
						//	{
						//		if (!enemy.Unconscious)
						//		{
						//			AppendText($"[color={RGB.LightRed}] {enemy.NameSubjectJosa} 의식을 잃었다[/color]", true);
						//			enemy.HP = 0;
						//			enemy.Unconscious = true;
						//		}
						//	}

						//	DisplayEnemy();
						//	ContinueText.Visibility = Visibility.Visible;

						//	mSpecialEvent = SpecialEventType.HelpRedAntares2;
						//}
						//else if (specialEvent == SpecialEventType.HelpRedAntares2)
						//{
						//	Talk($"[color={RGB.LightMagenta}] 나는 언제나 당신 주위에서 당신에게 도움을 주도록 하겠네. 그리고 내가 알아낸 바로는 다크 메이지는 존재한다네." +
						//	" 이제부터가 다크 메이지와의 결전이 시작되는 걸세. 그럼 다음에 또 보도록 하세.[/color]");

						//	foreach (var player in mPlayerList)
						//	{
						//		if (player.HP < 0 || player.Unconscious > 0 || player.Dead > 0)
						//		{
						//			player.HP = 1;
						//			player.Unconscious = 0;
						//			player.Dead = 0;
						//		}
						//	}

						//	if (mAssistPlayer != null && (mAssistPlayer.HP < 0 || mAssistPlayer.Unconscious > 0 || mAssistPlayer.Dead > 0))
						//	{
						//		mAssistPlayer.HP = 1;
						//		mAssistPlayer.Unconscious = 0;
						//		mAssistPlayer.Dead = 0;
						//	}

						//	DisplayHP();
						//	DisplayCondition();

						//	mSpecialEvent = SpecialEventType.HelpRedAntares3;
						//}
						//else if (specialEvent == SpecialEventType.HelpRedAntares3)
						//{
						//	AppendText("");

						//	mEncounterEnemyList.Clear();
						//	ShowMap();
						//}
						//else if (specialEvent == SpecialEventType.CantBuyWeapon)
						//	ShowWeaponTypeMenu(mWeaponTypeID);
						//else if (specialEvent == SpecialEventType.CantBuyExp)
						//	ShowExpStoreMenu();
						//else if (specialEvent == SpecialEventType.CantBuyItem)
						//	ShowItemStoreMenu();
						//else if (specialEvent == SpecialEventType.CantBuyMedicine)
						//	ShowMedicineStoreMenu();
						//else if (specialEvent == SpecialEventType.MeetGeniusKie)
						//{
						//	AppendText(new string[] {
						//		" 나는 이곳에서 너무 많이 머물렀다네.",
						//		" 자네도 만났으니 나의 할 일은 다 끝났다네.",
						//		" 조금만 있다가 나도 새로운 여행을 떠나야 하지. 그럼 다음에 보도록 하지"
						//	});

						//	mParty.Etc[34] |= 1 << 4;
						//}
						//else if (specialEvent == SpecialEventType.WizardEye)
						//{
						//	AppendText("");
						//	mWizardEyeTimer.Stop();
						//	mWizardEyePosBlink = false;
						//}
						//else if (specialEvent == SpecialEventType.Penetration)
						//	AppendText("");
						//else if (specialEvent == SpecialEventType.Telescope)
						//{
						//	if (mTelescopeXCount != 0 || mTelescopeYCount != 0)
						//	{
						//		if ((mTelescopeXCount != 0 && (mParty.XAxis + mTelescopeXCount <= 4 || mParty.XAxis + mTelescopeXCount >= mMapWidth - 4)) ||
						//			(mTelescopeXCount != 0 && (mParty.YAxis + mTelescopeYCount <= 5 || mParty.YAxis + mTelescopeYCount >= mMapHeight - 4)))
						//		{
						//			mTelescopeXCount = 0;
						//			mTelescopeYCount = 0;
						//			return;
						//		}

						//		if (mTelescopeXCount < 0)
						//			mTelescopeXCount++;

						//		if (mTelescopeXCount > 0)
						//			mTelescopeXCount--;

						//		if (mTelescopeYCount < 0)
						//			mTelescopeYCount++;

						//		if (mTelescopeYCount > 0)
						//			mTelescopeYCount--;

						//		if (mTelescopeXCount != 0 || mTelescopeYCount != 0)
						//		{
						//			Talk($"[color={RGB.White}]천리안 사용중 ...[/color]");
						//			mSpecialEvent = SpecialEventType.Telescope;
						//		}
						//		else
						//			AppendText("");
						//	}
						//}
						//else if (specialEvent == SpecialEventType.BattleCaveOfBerialEntrance)
						//{
						//	mBattleEvent = BattleEvent.CaveOfBerialEntrance;
						//	mEncounterEnemyList.Clear();
						//	for (var i = 0; i < 8; i++)
						//		JoinEnemy(31);

						//	DisplayEnemy();
						//	StartBattle(false);
						//}
						//else if (specialEvent == SpecialEventType.InvestigationCave)
						//{
						//	AppendText(new string[] {
						//		$" {mPlayerList[1].NameSubjectJosa} 동굴의 입구를 조사하더니 당신에게 말했다",
						//		"",
						//		$"[color={RGB.Cyan}] 잠깐 여기를 보게. 이 입구에 떨어져 있는 흙은 다른 곳의 흙과는 다르다네. 이걸 보게나." +
						//		"모래가 거의 유리처럼 변해있지 않은가. 이 건 분명히 핵폭발이 여기에서 일어났었다는 증거일세." +
						//		" 이 동굴의 입구가 함몰된 이유는 바로 핵무기나 아니면 거기에 필적하는 초자연적인 마법에 의해서인 것 같네." +
						//		" 그렇다면 이 동굴 안의 세계에 존재하는 인물 중에서 이 정도의 고수준 마법을 사용하는 사람이 있다는 셈이라네.[/color]"
						//	});

						//	mParty.Etc[30] |= 1 << 7;
						//}
						//else if (specialEvent == SpecialEventType.BattleCaveOfAsmodeusEntrance)
						//{
						//	mEncounterEnemyList.Clear();
						//	for (var i = 0; i < 8; i++)
						//	{
						//		if (i == 3)
						//			JoinEnemy(63);
						//		else if (i == 7)
						//			JoinEnemy(64);
						//		else
						//			JoinEnemy(54);
						//	}

						//	mBattleEvent = BattleEvent.CaveOfAsmodeusEntrance;
						//	StartBattle(false);
						//}
						//else if (specialEvent == SpecialEventType.GetCromaticShield)
						//{
						//	AppendText($"[color={RGB.LightGreen}] 유골이 지니고 있던 크로매틱 방패를 가질 사람을 고르시오.[/color]");

						//	ShowCharacterMenu(MenuMode.ChooseEquipCromaticShield, false);
						//}
						//else if (specialEvent == SpecialEventType.GetCromaticShield)
						//{
						//	AppendText($"[color={RGB.LightGreen}] 유골이 지니고 있던 양날 전투 도끼를 가질 사람을 고르시오.[/color]");

						//	ShowCharacterMenu(MenuMode.ChooseEquipBattleAxe, false);
						//}
						//else if (specialEvent == SpecialEventType.GetObsidianArmor)
						//{
						//	AppendText($"[color={RGB.LightGreen}] 호수에서 떠오른 흑요석 갑옷을 장착할 사람을 고르시오.[/color]");

						//	ShowCharacterMenu(MenuMode.ChooseEquipObsidianArmor, false);
						//}
						//else if (specialEvent == SpecialEventType.BattleGuardOfObsidianArmor)
						//{
						//	mBattleEvent = BattleEvent.GuardOfObsidianArmor;

						//	StartBattle(false);
						//}
						//else if (specialEvent == SpecialEventType.BattleSlaim)
						//{
						//	mBattleEvent = BattleEvent.Slaim;

						//	mEncounterEnemyList.Clear();

						//	for (var i = 0; i < 8; i++)
						//		JoinEnemy(22);

						//	DisplayEnemy();
						//	StartBattle(false);
						//}
						//else if (specialEvent == SpecialEventType.BattleCaveEntrance)
						//{
						//	mBattleEvent = BattleEvent.CaveEntrance;

						//	mEncounterEnemyList.Clear();

						//	for (var i = 0; i < 5; i++)
						//		JoinEnemy(i + 25);

						//	DisplayEnemy();
						//	StartBattle(false);
						//}
						//else if (specialEvent == SpecialEventType.AstronomyQuiz)
						//{
						//	mPyramidQuizAllRight = true;
						//	AppendText(" \"시그너스 X1\"이란 별은 현대 과학에서 어떤 별로 간주되고 있겠소?");

						//	ShowMenu(MenuMode.AstronomyQuiz1, new string[] {
						//		"혹성",
						//		"백색 왜성",
						//		"블랙홀",
						//		"중성자 별"
						//	});
						//}
						//else if (specialEvent == SpecialEventType.ComputerQuiz)
						//{
						//	mPyramidQuizAllRight = true;
						//	AppendText(" \"다크 메이지 실리안 카미너스\"라는 이 게임이 지원하는 그래픽 카드는 무엇이겠소?");

						//	ShowMenu(MenuMode.ComputerQuiz1, new string[] {
						//		"Hercules",
						//		"Color Graphics Adapter",
						//		"Enhanced Graphics Adapter",
						//		"Video Graphics Array"
						//	});
						//}
						//else if (specialEvent == SpecialEventType.PhysicsQuiz)
						//{
						//	mPyramidQuizAllRight = true;
						//	AppendText(" 빛보다 더 빠르게 달릴 수 있는 가상의 입자가 있소. 그 입자의 이름은 무엇이겠소?");

						//	ShowMenu(MenuMode.PhysicsQuiz1, new string[] {
						//		"플라즈마",
						//		"쿼크",
						//		"타키온",
						//		"타지온"
						//	});
						//}
						//else if (specialEvent == SpecialEventType.CommonSenseQuiz)
						//{
						//	mPyramidQuizAllRight = true;
						//	AppendText(" 이 게임의 전작이며 1부였던 \"또 다른 지식의 성전\"의 적의 최고 보스는 누구였소?");

						//	ShowMenu(MenuMode.CommonSenseQuiz1, new string[] {
						//		"에인션트 이블",
						//		"히드라",
						//		"드라코니안",
						//		"네크로만서",
						//		"로드 안"
						//	});
						//}
						//else if (specialEvent == SpecialEventType.MathQuiz)
						//{
						//	mPyramidQuizAllRight = true;
						//	AppendText(" X = 7, Y = 9, Z * Z = X + Y 라면 Z의 값은 어떻게 되겠소?");

						//	ShowMenu(MenuMode.MathQuiz1, new string[] {
						//		"<< 3 >>",
						//		"<< 4 >>",
						//		"<< 5 >>",
						//		"<< 6 >>",
						//		"<< 7 >>"
						//	});
						//}
						//else if (specialEvent == SpecialEventType.SkipTurn)
						//{
						//	AddBattleCommand(true);
						//}
						//else if (specialEvent == SpecialEventType.MeetRegulus)
						//{
						//	Dialog(new string[] {
						//		" 아참 그리고 내가 이번에는 해독 약초를 많이 캤다네. 자네에게도 몇 개 주지. 여행에 도움이 될걸세.",
						//		"",
						//		$"[color={RGB.LightCyan}] 일행은 약간의 해독 약초를 그에게 받았다.[/color]"
						//	});

						//	if (mParty.Etc[2] + 5 < 256)
						//		mParty.Etc[2] += 5;
						//	else
						//		mParty.Etc[2] = 255;

						//	mParty.Etc[31] |= 1 << 5;
						//}
						//else if (specialEvent == SpecialEventType.HoleInMenace)
						//	InvokeAnimation(AnimationType.LandUnderground);
						//else if (specialEvent == SpecialEventType.HoleInMenace2)
						//	InvokeAnimation(AnimationType.LandUnderground2);
						//else if (specialEvent == SpecialEventType.CompleteUnderground)
						//	InvokeAnimation(AnimationType.ReturnCastleLore);
						//else if (specialEvent == SpecialEventType.CompleteUnderground2)
						//	InvokeAnimation(AnimationType.ReturnCastleLore2);
						//else if (specialEvent == SpecialEventType.BattleWisp)
						//{
						//	mEncounterEnemyList.Clear();

						//	for (var i = 0; i < 8; i++)
						//	{
						//		var enemy = JoinEnemy(32);
						//		enemy.Endurance = 30;
						//		enemy.HP = enemy.Endurance * enemy.Level * 10;
						//		enemy.CastLevel = 6;
						//	}

						//	DisplayEnemy();
						//	StartBattle(true);

						//	mBattleEvent = BattleEvent.Wisp;
						//}
						//else if (specialEvent == SpecialEventType.BattleCyclopes)
						//{
						//	StartBattle(true);
						//	mBattleEvent = BattleEvent.Cyclopes;
						//}
						//else if (specialEvent == SpecialEventType.BattleDeathKnight)
						//{
						//	StartBattle(false);
						//	mBattleEvent = BattleEvent.DeathKnight;
						//}
						//else if (specialEvent == SpecialEventType.JoinDeathSoulServant)
						//{
						//	for (var i = 0; i < 2; i++)
						//		JoinEnemy(59);

						//	DisplayEnemy();

						//	Talk($"[color={RGB.LightMagenta}] 우리들은 몰록님을 위해서만 싸운다. 당신들과는 사적인 감정은 없지만 이것도 몰록님의 명령이니 우리도 어쩔 수 없다." +
						//	" 그럼 지옥에서나 우리 보도록 하지.[/color]");

						//	mSpecialEvent = SpecialEventType.BattleDeathSoul;
						//}
						//else if (specialEvent == SpecialEventType.BattleDeathSoul)
						//{
						//	StartBattle(true);
						//	mBattleEvent = BattleEvent.DeathSoul;
						//}
						//else if (specialEvent == SpecialEventType.MeetWarriorOfCrux)
						//{
						//	Ask(new string[] {
						//		$"[color={RGB.LightMagenta}] 우리들은 서로의 길로 가는 것이 어떨까?[/color]",
						//		$"[color={RGB.LightMagenta}] 그러면 서로가 편해지지. 나의 말에 무슨 계략이 있다던가 하는 것은 아니라네. 어떤가?[/color]"
						//	}, MenuMode.FightWarriorOfCrux, new string[] {
						//		"그러면 그렇게 하지",
						//		"당치 않는 소리, 분명 이건 계략이다"
						//	});
						//}
						//else if (specialEvent == SpecialEventType.ReadPapyrus)
						//{
						//	Dialog(new string[] {
						//		$" [color={RGB.LightCyan}]악의 추종자들[/color][color={RGB.White}]이 실리안 카미너스를 데려가 버렸다." +
						//		" 만약 이 글을 누가 본다면 그들을 응징해 주기를 바란다. 실리안 카미너스는 이곳 태생의 6살 난 여자아이며 그녀는 운명적으로 불멸의 힘을 가지고 태어났다." +
						//		$" 아직은 그녀의 힘이 강하게 봉인되어 있어서 걱정이 없지만 만약[/color] [color={RGB.LightCyan}]악의 추종자들[/color][color={RGB.White}]이" +
						//		" 봉인되어 있는 그녀의 숨은 힘을 이끌어 낸다면 그 힘에 의해서 세상은 걷잡을 수 없는 파국으로 치닫게 되어 멸망이 도래하게 될 것이다." +
						//		" 반드시 그들을 응징해 주기를 바란다. 세상의 평화를 위하여....[/color]"
						//	});

						//	if ((mParty.Etc[8] & (1 << 1)) == 0)
						//	{
						//		Dialog(new string[] {
						//			"",
						//			$"[color={RGB.LightCyan}] [[ 경험치 + 100,000 ][/color]"
						//		}, true);

						//		mParty.Etc[8] |= 1 << 1;

						//		foreach (var player in mPlayerList)
						//			player.Experience += 100_000;
						//	}
						//}
						//else if (specialEvent == SpecialEventType.ReadDiary)
						//{
						//	Talk(new string[] {
						//		$"[color={RGB.White}] 773년 10월 15일[/color]",
						//		"",
						//		" 지금 내가 하고 있는 일이 정말 옳은 일인지 스스로가 의심이 간다." +
						//		" 이미 빛의 사원은 이름과는 다른 어둠의 사원으로 변해가고 있고 나가 따르고 있는 메피스토펠레스님은 선을 표방하고만 있을 뿐 실지로는 선을 위시한 악을 행하고 있다."
						//	});

						//	mSpecialEvent = SpecialEventType.ReadDiary1;
						//}
						//else if (specialEvent == SpecialEventType.ReadDiary1)
						//{
						//	Talk(new string[] {
						//		$"[color={RGB.White}] 773년 10월 18일[/color]",
						//		"",
						//		" 오늘도 나는 또 한 사람의 재물을 죽였다. 아스모데우스님의 명령을 어길 수 없어서 어쩔 수 없이 죽였지만 나의 마음은 양심의 가책으로 번민하고 있다." +
						//		" 어쩌면 나는 메피스토펠레스님에게 이용만 당하고 있다는 느낌이 드는 게 사실이다." +
						//		" 하지만 이렇게 해서라도 세계가 좀 더 좋아질 수만 있다면 나는 이용당해도 좋다는 생각이 든다. 정말 세상은 너무 더러워졌다."
						//	});

						//	mSpecialEvent = SpecialEventType.ReadDiary2;
						//}
						//else if (specialEvent == SpecialEventType.ReadDiary2)
						//{
						//	Talk(new string[] {
						//		$"[color={RGB.White}] 773년 10월 23일[/color]",
						//		"",
						//		" 낮에 너무 끔찍한 말을 들었다. 지금도 나의 가슴은 방망이질하듯이 두근거린다. 메피스토펠레스님과 몰록님과의 대화를 우연히 엿들었다." +
						//		" 그들이 궁극적으로 하고자 하는 일은 바로 세계 멸망이었다." +
						//		" 그들이 표상하는 종교는 바로 마교였다. 그들이 나에게 재물을 죽이게 하고는 그 생명력을 받아들여 점점 그들의 마력을 강하게 해왔던 것이었다."
						//	});

						//	mSpecialEvent = SpecialEventType.ReadDiary3;
						//}
						//else if (specialEvent == SpecialEventType.ReadDiary3)
						//{
						//	Talk(new string[] {
						//		$"[color={RGB.White}] 773년 10월 25일[/color]",
						//		"",
						//		" 나는 드디어 빛의 사원을 탈출했다. 그러고는 여기 필멸의 생존이란 동굴에 은신했다. 그들의 보복이 두려웠기 때문이었다." +
						//		" 하지만 얼마 가지 않아 그들의 마력에 노출되어 버릴 것 같다. 나는 그들의 마력의 강함을 쭉 봐왔으니까 아마 틀림없을 것이다."
						//	});

						//	mSpecialEvent = SpecialEventType.ReadDiary4;
						//}
						//else if (specialEvent == SpecialEventType.ReadDiary4)
						//{
						//	Talk(new string[] {
						//		$"[color={RGB.White}] 773년 10월 26일[/color]",
						//		"",
						//		" 아침에 내가 조금 더 안전할 수 있는 방법이 문득 떠올라 실행하기로 했다. 동굴의 입구를 늪으로 막고 또 통로를 모두 차단해 버렸다." +
						//		"아무리 공간 이동의 달인이라고 하더라도 이렇게 깊은 동굴을 일일이 투시해서 나를 찾아낸 후 여기까지 공간 이동을 해 올 수는 없으리라고 생각한다." +
						//		" 이제는 편안히 잘 수 있을 것이다."
						//	});

						//	mSpecialEvent = SpecialEventType.ReadDiary5;
						//}
						//else if (specialEvent == SpecialEventType.ReadDiary5)
						//{
						//	Talk(new string[] {
						//		$"[color={RGB.White}] 773년 11월 3일[/color]",
						//		"",
						//		" 나의 계산은 틀렸다. 오늘 아침에 4명의 어둠의 추종자의 막내 격인 베리알이 나를 찾아내었다." +
						//		" 그는 그들을 배신한 나를 하루 종일 괴로워하며 죽도록 독이 있는 창으로 나를 찔렀다. 지금 고통이 너무 심하다." +
						//		" 손도 떨리고 정신도 맑지 않다. 이제는 마지막인가 보다."
						//	});

						//	mSpecialEvent = SpecialEventType.ReadDiary6;
						//}
						//else if (specialEvent == SpecialEventType.ReadDiary6)
						//{
						//	Dialog(new string[] {
						//		$"[color={RGB.White}] 773년 11월 4일[/color]",
						//		"",
						//		" 내 생명도 오늘 까지겠지." +
						//		" 그래서 내가 죽은 뒤에 누군가가 나를 발견할지도 모르니 그들에 대한 특징을 알려주어 다른 사람들도 주의할 수 있도록 해야겠다는 생각이 들었다.",
						//		" 4번째 보스인 베리알은 마법 능력을 제외하고는 보통 인간과 같을 정도의 체력을 가졌다." +
						//		" 하지만 마법 능력도 다른 3명에 비하면 보잘것없다. 하지만 결코 무시할 수는 없다.",
						//		" 3번째 보스인 몰록은 환상의 마법에 대가이다. 아마 최고의 환상 마법사이다.",
						//		" 2번째 보스인 아스모데우스는 계략의 천재이다. 하지만 마법사이기보다는 마법을 사용하는 전사 쪽에 가깝다.",
						//		" 최고 보스인 메피스토펠레스는 기사 격의 체력에 대마법사의 마법을 가지고 있다. 특기는 공중 핵융합에 의해 기를 뿜어내어 적을 공격하는 방법이다.",
						//		"",
						//		" 그리고 지금 그들은 또 다른 2개의 대륙에 각각 떨어져서 어둠의 영혼을 부활 시킬 힘을 분산해서 모으고 있다." +
						//		" 내가 아는 것은 이게 전부이다. 내가 죽더라도 이 글이 도움이 되었으면 한다. 그리고 ... ..."
						//	});

						//	if ((mParty.Etc[8] & (1 << 3)) == 0)
						//	{
						//		Dialog(new string[] {
						//			"",
						//			$"[color={RGB.LightCyan}] [[ 경험치 + 100,000 ][/color]"
						//		}, true);

						//		foreach (var player in mPlayerList)
						//		{
						//			player.Experience += 100_000;
						//		}
						//		mParty.Etc[8] |= 1 << 3;
						//	}
						//}
						//else if (specialEvent == SpecialEventType.BattleCaveOfBerial)
						//{
						//	StartBattle(false);
						//	mBattleEvent = BattleEvent.CaveOfBerial;
						//}
						//else if (specialEvent == SpecialEventType.BattleGagoyle)
						//{
						//	for (var i = 0; i < 7; i++)
						//		JoinEnemy(41);

						//	DisplayEnemy();
						//	StartBattle(false);

						//	mBattleEvent = BattleEvent.Gagoyle;
						//}
						//else if (specialEvent == SpecialEventType.BattleCaveOfBerialCyclopes)
						//{
						//	StartBattle(false);
						//	mBattleEvent = BattleEvent.CaveOfBerialCyclopes;
						//}
						//else if (specialEvent == SpecialEventType.SummonLizardMan)
						//{
						//	mEncounterEnemyList.Clear();
						//	for (var i = 0; i < 4; i++)
						//		JoinEnemy(58);
						//	JoinEnemy(71);

						//	DisplayEnemy();

						//	Talk(new string[] {
						//		"",
						//		$"[color={RGB.Yellow}] 오르트 일렘 .. 칼 젠 ..[/color]",
						//		"",
						//		" 베리알은 소환 마법으로 주위의 도마뱀을 도마뱀 인간으로 소생시켰다."
						//	}, true);

						//	mSpecialEvent = SpecialEventType.BattleBerial;
						//}
						//else if (specialEvent == SpecialEventType.BattleBerial)
						//{
						//	StartBattle(false);
						//	mBattleEvent = BattleEvent.Berial;
						//}
						//else if (specialEvent == SpecialEventType.BattleRevivalBerial)
						//{
						//	StartBattle(true);
						//	mBattleEvent = BattleEvent.RevivalBerial;
						//}
						//else if (specialEvent == SpecialEventType.ChaseLizardMan)
						//{
						//	Talk(new string[] {
						//		"",
						//		$"[color={RGB.Yellow}] 인 녹스 그라브 ..[/color]",
						//		"",
						//		" 일행은 \"유독 가스\" 마법을 동굴 안을 향해 사용했다.",
						//		"",
						//		$"[color={RGB.Yellow}] 바스 포르 그라브 ..[/color]",
						//		"",
						//		" 그리고 일행은 \"초음파\"를 이용하여 동굴 입구를 함몰시켰다."
						//	}, true);
						//}
						//else if (specialEvent == SpecialEventType.SummonIllusion)
						//{
						//	StartBattleEvent(BattleEvent.Illusion);
						//}
						//else if (specialEvent == SpecialEventType.RevealMolok)
						//{
						//	Talk(new string[] {
						//		$"[color={RGB.LightMagenta}] 너는 또 누구냐. 나의 환상을 걷어 내다니 대단한 실력이군." +
						//		" 하지만 당신의 의지가 여기로 스며들지 못하도록 결계를 맺어야겠군." +
						//		" 당신이 아무리 뛰어나다고 해도 육체가 없는 영혼인 주제에 에너지 장벽을 뚫지는 못하겠지." +
						//		" 그럼 나는 이 자들과 전투를 마무리 지어야겠소. 자 그럼 결계 밖에서 이 자들의 최후를 감상하기 바라오.[/color]"
						//	});

						//	mSpecialEvent = SpecialEventType.RecoverParty;
						//}
						//else if (specialEvent == SpecialEventType.RecoverParty)
						//{
						//	foreach (var player in mPlayerList)
						//	{
						//		if (player.HP < 0 || player.Unconscious > 0 || player.Dead > 0)
						//		{
						//			player.HP = 1;
						//			player.Unconscious = 0;
						//			player.Dead = 0;
						//		}
						//	}

						//	if (mAssistPlayer != null)
						//	{
						//		if (mAssistPlayer.HP < 0 || mAssistPlayer.Unconscious > 0 || mAssistPlayer.Dead > 0)
						//		{
						//			mAssistPlayer.HP = 1;
						//			mAssistPlayer.Unconscious = 0;
						//			mAssistPlayer.Dead = 0;
						//		}
						//	}

						//	UpdatePlayersStat();

						//	Talk(" 몰록이 레드 안타레스에 대한 결계를 만드는 순간 일행은 기회를 틈타서 선공권을 잡게 되었다.");

						//	mSpecialEvent = SpecialEventType.BattleMolok;
						//}
						//else if (specialEvent == SpecialEventType.BattleMolok)
						//{
						//	StartBattle(true);
						//	mBattleEvent = BattleEvent.Molok;
						//}
						//else if (specialEvent == SpecialEventType.WinMolok)
						//	await InvokeSpecialEvent(mPrevX, mPrevY);
						//else if (specialEvent == SpecialEventType.BattleDragon)
						//{
						//	mEncounterEnemyList.Clear();

						//	for (var i = 0; i < 8; i++)
						//		JoinEnemy(53);

						//	DisplayEnemy();

						//	StartBattle(false);

						//	mBattleEvent = BattleEvent.Dragon;
						//}
						//else if (specialEvent == SpecialEventType.BattleFlyingDragon)
						//{
						//	mEncounterEnemyList.Clear();

						//	for (var i = 0; i < 5; i++)
						//		JoinEnemy(42);

						//	DisplayEnemy();

						//	StartBattle(false);

						//	mBattleEvent = BattleEvent.FlyingDragon;
						//}
						//else if (specialEvent == SpecialEventType.BattleMachineRobot)
						//{
						//	StartBattle(false);

						//	mBattleEvent = BattleEvent.MachineRobot;
						//}
						//else if (specialEvent == SpecialEventType.RevealAsmodeus)
						//{
						//	mEncounterEnemyList.Clear();

						//	JoinEnemy(73);
						//	for (var i = 0; i < 2; i++)
						//		JoinEnemy(68);

						//	DisplayEnemy();

						//	Talk($"[color={RGB.LightMagenta}] 나의 명성은 익히 들어서 잘 알고 있겠지. 나는 세계 최고의 전투 마도사인 아스모데우스님이시다." +
						//	" 여기까지 침투해 오다니 굉장한 능력이군. 당신네들이 우리 편이었다면 상당히 도움이 되었을 텐데. 하지만 어쩔 수 없군. 여기를 당신네들의 무덤으로 만들 수밖에.");

						//	mSpecialEvent = SpecialEventType.BattleAsmodeus;
						//}
						//else if (specialEvent == SpecialEventType.BattleAsmodeus)
						//{
						//	mEncounterEnemyList.Clear();

						//	for (var i = 0; i < 5; i++)
						//		JoinEnemy(54 + i);
						//	for (var i = 0; i < 2; i++)
						//		JoinEnemy(68);
						//	JoinEnemy(73);

						//	DisplayEnemy();

						//	StartBattle(false);
						//	mBattleEvent = BattleEvent.Asmodeus;
						//}
						//else if (specialEvent == SpecialEventType.BattleUseItem)
						//{
						//	AddBattleCommand(true);
						//}
						//else if (specialEvent == SpecialEventType.BattlePrison || specialEvent == SpecialEventType.BattlePrison2)
						//{
						//	int soldierCount;

						//	if (specialEvent == SpecialEventType.BattlePrison)
						//		soldierCount = 2;
						//	else
						//		soldierCount = 7;

						//	mEncounterEnemyList.Clear();
						//	for (var i = 0; i < soldierCount; i++)
						//	{
						//		var enemy = JoinEnemy(25);
						//		enemy.Name = $"병사 {i + 1}";
						//		enemy.Special = 0;
						//		enemy.CastLevel = 0;
						//		enemy.ENumber = 1;
						//	}

						//	DisplayEnemy();

						//	if (mAssistPlayer != null && mAssistPlayer.Name == "미친 조")
						//	{
						//		Talk(" 우리들 뒤에 있던 미친 조가 전투가 일어나자마자 도망을 가버렸다.");
						//		mAssistPlayer = null;
						//		DisplayPlayerInfo();

						//		mSpecialEvent = SpecialEventType.RunawayMadJoe;
						//	}
						//	else
						//	{
						//		StartBattle(false);
						//		mBattleEvent = BattleEvent.Prison;
						//	}
						//}
						//else if (specialEvent == SpecialEventType.RunawayMadJoe)
						//{
						//	StartBattle(false);
						//	mBattleEvent = BattleEvent.Prison;
						//}
						//else if (specialEvent == SpecialEventType.BattleLastGuardian)
						//{
						//	StartBattle(true);
						//	mBattleEvent = BattleEvent.LastGuardian;
						//}
						//else if (specialEvent == SpecialEventType.MeetMephistopheles)
						//{
						//	mEncounterEnemyList.Clear();

						//	JoinEnemy(74);
						//	DisplayEnemy();

						//	Talk(new string[] {
						//		$"[color={RGB.LightMagenta}] 당신들이 다크 메이지 실리안 카미너스의 새로운 탄생을 방해하러 온 자들인가?" +
						//		" 그렇지만 이미 한발 늦었군. 나의 뒤쪽의 생체 배양 튜브 속을 보게나. 저것이 새로 부활하는 실리안 카미너스라네.[/color]",
						//		"",
						//		" 당신이 그가 가리키는 곳에서 본 것은 점액이 흘러내리고 있는 붉은 고기 덩어리 같은 육체였다." +
						//		" 머리에는 무수한 전기 봉이 꽂혀 있었고 등에는 영양분을 공급하는 관이 연결되어 있었다." +
						//		" 이따금 깜박이는 검은 눈에서는 어린 소녀의 눈빛이 아닌 증오로 가득 찬 짐승의 눈빛을 느낄 수 있었다."
						//		,"",
						//		$"[color={RGB.LightMagenta}] 잘 봤겠지. 이미 그녀는 스스로의 몸 안에 봉인되어 있는 힘을 깨달아 버렸지." +
						//		" 내가 이 스위치를 누름과 동시에 로어의 세계로 공간 이동이 되게 되어있다네.[/color]",
						//		"",
						//		" 그는 그의 손안에 있는 리모컨의 스위치를 눌렀고 순간 실리안 카미너스는 공간 이동을 하여 이곳에서 사라졌다.",
						//		"",
						//		$"[color={RGB.LightMagenta}] 그리고 당신들은 내가 처리하도록 하지." +
						//		" 당신들은 로어 성이 불타는 장면을 못 보게 되어 정말 아쉽겠군." +
						//		" 아무리 로드 안이라고 해도 실리안 카미너스의 악의 힘에는 당하지 못한다네. 그녀는 불멸의 생명체이니까 말일세.[/color]"
						//	});

						//	mSpecialEvent = SpecialEventType.BattleMephistopheles;
						//}
						//else if (specialEvent == SpecialEventType.BattleMephistopheles)
						//{
						//	mEncounterEnemyList.Clear();

						//	JoinEnemy(57);
						//	JoinEnemy(57);
						//	JoinEnemy(60);
						//	JoinEnemy(68);
						//	JoinEnemy(68);
						//	JoinEnemy(65);
						//	JoinEnemy(70);
						//	JoinEnemy(74);

						//	DisplayEnemy();

						//	StartBattle(false);
						//	mBattleEvent = BattleEvent.Mephistopheles;
						//}
						//else if (specialEvent == SpecialEventType.ReturnToGround)
						//	InvokeAnimation(AnimationType.ReturnToGround);
						//else if (specialEvent == SpecialEventType.MeetCyllianCominus)
						//{
						//	mEncounterEnemyList.Clear();

						//	JoinEnemy(74).Name = "실리안 카미너스";
						//	DisplayEnemy();

						//	Talk($"[color={RGB.LightMagenta}] 크악~~ 네가 이곳의 마지막 생존자냐? 그렇다면 너도 죽여주지.[/color]");
						//	mSpecialEvent = SpecialEventType.ReplicateCominus;
						//}
						//else if (specialEvent == SpecialEventType.ReplicateCominus)
						//{
						//	Talk(" 실리안 카미너스는 다시 8조각으로 자신을 분해해서 자신을 8명으로 만들었다.");

						//	mSpecialEvent = SpecialEventType.BattleCominus;
						//}
						//else if (specialEvent == SpecialEventType.BattleCominus)
						//{
						//	mEncounterEnemyList.Clear();

						//	for (var i = 0; i < 8; i++)
						//	{
						//		var enemy = JoinEnemy(74);
						//		enemy.Name = "실리안 카미너스";
						//		enemy.Resistance = 255;
						//	}
						//	DisplayEnemy();

						//	StartBattle(false);
						//	mBattleEvent = BattleEvent.CyllianCominus;
						//}
						//else if (specialEvent == SpecialEventType.NecromancerMessage)
						//{
						//	mAnimationEvent = AnimationType.None;
						//	mAnimationFrame = 0;

						//	BGMPlayer.Source = null;
						//	EndingMessage.Visibility = Visibility.Visible;
						//}
						//else if (specialEvent == SpecialEventType.CookieEvent)
						//{
						//	Window.Current.CoreWindow.KeyDown -= gamePageKeyDownEvent;
						//	Window.Current.CoreWindow.KeyUp -= gamePageKeyUpEvent;
						//	Frame.Navigate(typeof(MainPage), null);
						//}
					}


					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp ||
					args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft ||
					args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight ||
					args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
						return;

					ContinueText.Visibility = Visibility.Collapsed;

					//if (mBattleTurn == BattleTurn.None)
					//{
					//	DialogText.Blocks.Clear();
					//	DialogText.TextHighlighters.Clear();
					//}

					if (mRemainDialog.Count > 0)
					{
						ClearDialog();

						var remainDialog = new List<string>();
						remainDialog.AddRange(mRemainDialog);
						mRemainDialog.Clear();

						var added = true;
						while (added && remainDialog.Count > 0)
						{
							added = AppendText(remainDialog[0], true);
							if (added)
								remainDialog.RemoveAt(0);
						}

						if (mRemainDialog.Count > 0 || mBattleTurn != BattleTurn.None)
							ContinueText.Visibility = Visibility.Visible;
						else
						{
							switch (mAfterDialogType)
							{
								case AfterDialogType.PressKey:
									ContinueText.Visibility = Visibility.Visible;
									break;
								case AfterDialogType.Menu:
									ShowMenu(mAfterMenuMode, mAfterMenuList);
									break;
							}
						}
					}
					else if (mSpecialEvent != SpecialEventType.None)
						await InvokeSpecialEventLaterPart();
					else if (mBattleTurn == BattleTurn.Player)
					{
						if (mBattleCommandQueue.Count == 0)
						{
							var allUnavailable = true;
							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Dead && !enemy.Unconscious)
								{
									allUnavailable = false;
									break;
								}
							}

							if (allUnavailable)
							{
								mBattleTurn = BattleTurn.Win;
								mParty.Etc[5] = 0;
								await EndBattle();
							}
							else
								ExecuteBattle();
						}
						else
							ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.Enemy)
					{
						ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.RunAway || mBattleTurn == BattleTurn.Win || mBattleTurn == BattleTurn.Lose)
					{
						await EndBattle();
					}
				}
				else if (mMenuMode == MenuMode.None && mSpinnerType == SpinnerType.None && (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadMenu))
				{
					AppendText($"[color={RGB.Red}]당신의 명령을 고르시오 ===>[/color]");

					ShowMenu(MenuMode.Game, new string[]
					{
						"일행의 상황을 본다",
						"개인의 상황을 본다",
						"일행의 건강 상태를 본다",
						"마법을 사용한다",
						"초능력을 사용한다",
						"여기서 쉰다",
						"물품을 서로 교환한다",
						"물품을 사용한다",
						"게임 선택 상황"
					});
				}
				else if (mSpinnerType != SpinnerType.None)
				{
					void CloseSpinner()
					{
						SpinnerText.TextHighlighters.Clear();
						SpinnerText.Blocks.Clear();
						SpinnerText.Visibility = Visibility.Collapsed;

						mSpinnerItems = null;
						mSpinnerID = 0;
						mSpinnerType = SpinnerType.None;
					}

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						mSpinnerID = (mSpinnerID + 1) % mSpinnerItems.Length;

						AppendText(SpinnerText, mSpinnerItems[mSpinnerID].Item1);
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						if (mSpinnerID == 0)
							mSpinnerID = mSpinnerItems.Length - 1;
						else
							mSpinnerID--;

						AppendText(SpinnerText, mSpinnerItems[mSpinnerID].Item1);
					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
					{
						AppendText("");
						CloseSpinner();
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						var spinnerType = mSpinnerType;
						mSpinnerType = SpinnerType.None;

						if (spinnerType == SpinnerType.TeleportationRange)
						{
							int moveX = mParty.XAxis;
							int moveY = mParty.YAxis;

							switch (mTeleportationDirection)
							{
								case 0:
									moveY -= mSpinnerItems[mSpinnerID].Item2;
									break;
								case 1:
									moveY += mSpinnerItems[mSpinnerID].Item2;
									break;
								case 2:
									moveX += mSpinnerItems[mSpinnerID].Item2;
									break;
								case 3:
									moveX -= mSpinnerItems[mSpinnerID].Item2;
									break;
							}

							if (moveX < 4 || moveX > mMapWidth - 4 || moveY < 4 || moveY > mMapHeight - 4)
								AppendText("공간 이동이 통하지 않습니다.");
							else
							{
								var valid = false;
								if (mPosition == PositionType.Town)
								{
									if (27 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}
								else if (mPosition == PositionType.Ground)
								{
									if (24 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}
								else if (mPosition == PositionType.Den)
								{
									if (41 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}
								else if (mPosition == PositionType.Keep)
								{
									if (27 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}

								if (!valid)
									AppendText("공간 이동 장소로 부적합 합니다.");
								else
								{
									mMagicPlayer.SP -= 50;

									if (GetTileInfo(moveX, moveY) == 0 || ((mPosition == PositionType.Den || mPosition == PositionType.Keep) && GetTileInfo(moveX, moveY) == 52))
										AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신을 배척합니다.[/color]");
									else
									{
										mParty.XAxis = moveX;
										mParty.YAxis = moveY;

										AppendText($"[color={RGB.White}]공간 이동 마법이 성공했습니다.[/color]");
									}
								}
							}
						}
						else if (spinnerType == SpinnerType.RestTimeRange)
						{
							var append = false;
							var restTime = mSpinnerItems[mSpinnerID].Item2;

							void RestPlayer(Lore player)
							{
								if (mParty.Food <= 0)
									AppendText($"[color={RGB.Red}]일행은 식량이 바닥났다[/color]", append);
								else
								{
									if (player.Dead > 0)
										AppendText($"{player.NameSubjectJosa} 죽었다", append);
									else if (player.Unconscious > 0 && player.Poison == 0)
									{
										player.Unconscious = player.Unconscious - (player.Level * restTime / 2);
										if (player.Unconscious <= 0)
										{
											AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 의식이 회복되었다[/color]", append);
											player.Unconscious = 0;
											if (player.HP <= 0)
												player.HP = 1;

#if DEBUG
											//mParty.Food--;
#else
											mParty.Food--;
#endif

										}
										else
											AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 여전히 의식 불명이다[/color]", append);
									}
									else if (player.Unconscious > 0 && player.Poison > 0)
										AppendText($"독때문에 {player.Name}의 의식은 회복되지 않았다", append);
									else if (player.Poison > 0)
										AppendText($"독때문에 {player.Name}의 건강은 회복되지 않았다", append);
									else
									{
										var recoverPoint = player.Level * restTime;
										if (player.HP >= player.Endurance * player.Level * 10)
										{
											if (mParty.Food < 255)
											{
#if DEBUG
												//mParty.Food++;
#else
												mParty.Food++;
#endif
											}
										}

										player.HP += recoverPoint;

										if (player.HP >= player.Endurance * player.Level * 10)
										{
											player.HP = player.Endurance * player.Level * 10;

											AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 모든 건강이 회복되었다[/color]", append);
										}
										else
											AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 치료되었다[/color]", append);

#if DEBUG
										//mParty.Food--;
#else
										mParty.Food--;
#endif
									}

									if (append == false)
										append = true;
								}
							}

							void RecoverStat(Lore player)
							{
								if (player.ClassType == ClassCategory.Magic)
									player.SP = player.Mentality * player.Level * 10;
								else if (player.ClassType == ClassCategory.Sword)
									player.SP = player.Mentality * player.Level * 5;
								else
									player.SP = 0;

								AppendText("", true);

								if (player.IsAvailable)
								{
									var exp = player.PotentialExperience;
									var levelUp = 0;
									do
									{
										levelUp++;
									} while (exp > Common.GetLevelUpExperience(levelUp) && levelUp <= 40);
									levelUp--;

									if (player.Level < levelUp || levelUp == 40)
									{
										if (levelUp < 40 || player.Level != 40)
										{
											AppendText($"[color={RGB.LightCyan}]{player.Name}의 레벨은 {levelUp}입니다", true);
											player.Level = levelUp;
										}
									}
								}
							}

							foreach (var player in mPlayerList)
							{
								RestPlayer(player);
							}

							if (mAssistPlayer != null)
								RestPlayer(mAssistPlayer);

							if (mParty.Etc[0] > 0)
							{
								var decPoint = restTime / 3 + 1;
								if (mParty.Etc[0] >= decPoint)
									mParty.Etc[0] -= decPoint;
								else
									mParty.Etc[0] = 0;
							}

							for (var i = 1; i < 4; i++)
								mParty.Etc[i] = 0;

							foreach (var player in mPlayerList)
							{
								RecoverStat(player);
							}

							if (mAssistPlayer != null)
								RecoverStat(mAssistPlayer);

							UpdatePlayersStat();
							PlusTime(restTime, 0, 0);
						}

						CloseSpinner();
					}
				}
				else if (mMenuMode != MenuMode.None)
				{
					void ShowTrainMessage()
					{
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 군사 훈련소 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 충분한 전투 경험을 쌓았다면, 당신은 더욱 능숙하게 무기를 다룰 것입니다.[/color]",
						});

						mSpecialEvent = SpecialEventType.TrainSkill;
					}

					void ShowTrainMagicMessage()
					{
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 마법 학교 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 충분한 실전 경험을 쌓았다면, 당신은 더욱 능숙하게 마법을 다룰 것입니다.[/color]",
						});

						mSpecialEvent = SpecialEventType.TrainMagic;
					}

					void ShowChangeJobForSwordMessage()
					{
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 군사 훈련소 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 원한다면 새로운 계급으로 바꿀 수가 있습니다.[/color]",
						});

						mSpecialEvent = SpecialEventType.ChangeJobForSword;
					}

					void ShowChangeJobForMagicMessage()
					{
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 마법 학교 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 원한다면 새로운 계급으로 바꿀 수가 있습니다.[/color]",
						});

						mSpecialEvent = SpecialEventType.ChangeJobForMagic;
					}

					void ShowCastOneMagicMenu()
					{
						string[] menuStr;

						var player = mPlayerList[mBattlePlayerID];

						int availCount = player.AttackMagic / 10;

						if (availCount > 0)
						{
							menuStr = new string[availCount];
							for (var i = 1; i <= availCount; i++)
							{
								menuStr[i - 1] = Common.GetMagicName(0, i);
							}

							ShowMenu(MenuMode.CastOneMagic, menuStr);
						}
						else
							BattleMode();
					}

					void ShowCastAllMagicMenu()
					{
						string[] menuStr;

						var player = mPlayerList[mBattlePlayerID];

						int availCount = player.AttackMagic / 10;

						if (availCount > 0)
						{
							menuStr = new string[availCount];
							for (var i = 1; i <= availCount; i++)
							{
								menuStr[i - 1] = Common.GetMagicName(1, i);
							}

							ShowMenu(MenuMode.CastAllMagic, menuStr);
						}
						else
							BattleMode();
					}

					void ShowCastSpecialMenu()
					{
						var player = mPlayerList[mBattlePlayerID];

						int availCount = player.SpecialMagic / 10;

						if (availCount > 0)
						{
							var menuStr = new string[availCount];
							for (var i = 1; i <= availCount; i++)
							{
								menuStr[i - 1] = Common.GetMagicName(4, i);
							}

							ShowMenu(MenuMode.CastSpecial, menuStr);
						}
						else
							BattleMode();
					}

					void ShowCastESPMenu()
					{
						var menuStr = new string[] { Common.GetMagicName(5, 3), Common.GetMagicName(5, 5) };
						ShowMenu(MenuMode.CastESP, menuStr);
					}

					void ShowCureDestMenu(Lore player, MenuMode menuMode)
					{
						int curePoint;

						mMagicPlayer = player;
						if (mMagicPlayer.ClassType == ClassCategory.Magic)
							curePoint = mMagicPlayer.CureMagic / 10;
						else
							curePoint = mMagicPlayer.AxeSkill / 10;

						if (curePoint <= 0)
						{
							Talk("당신은 치료 마법을 사용할 능력이 없습니다.");
							if (menuMode == MenuMode.ChooseBattleCureSpell)
								mSpecialEvent = SpecialEventType.BackToBattleMode;
							else
								ContinueText.Visibility = Visibility.Visible;
						}
						else
						{
							AppendText(new string[] { "누구에게" });
							string[] playerList;

							int availCount;
							if (player.ClassType == ClassCategory.Magic)
								availCount = player.CureMagic / 10;
							else
								availCount = player.AxeSkill / 10;

							if (availCount < 6)
							{
								playerList = new string[mAssistPlayer == null ? mPlayerList.Count : mPlayerList.Count + 1];
								if (mAssistPlayer != null)
									playerList[playerList.Length - 1] = mAssistPlayer.Name;
							}
							else
							{
								playerList = new string[mAssistPlayer == null ? mPlayerList.Count + 1 : mPlayerList.Count + 2];
								if (mAssistPlayer != null)
									playerList[playerList.Length - 2] = mAssistPlayer.Name;
								playerList[playerList.Length - 1] = "모든 사람들에게";
							}


							for (var i = 0; i < mPlayerList.Count; i++)
								playerList[i] = mPlayerList[i].Name;

							ShowMenu(menuMode, playerList);
						}
					}

					void ShowSummonMenu()
					{
						var player = mPlayerList[mBattlePlayerID];

						var availCount = player.SummonMagic / 10;

						if (availCount > 0)
						{
							var menuStr = new string[availCount];
							for (var i = 1; i <= availCount; i++)
							{
								menuStr[i - 1] = Common.GetMagicName(6, i);
							}

							ShowMenu(MenuMode.CastSummon, menuStr);
						}
						else
							BattleMode();
					}

					void UseItem(Lore player, bool battle)
					{
						mUsableItemIDList.Clear();
						mItemUsePlayer = player;

						var itemNames = new string[] { "체력 회복약", "마법 회복약", "해독의 약초", "의식의 약초", "부활의 약초", "소환 문서", "대형 횃불", "수정 구슬", "비행 부츠", "이동 구슬" };

						var itemMenuItemList = new List<string>();
						for (var i = 0; i < mParty.Item.Length; i++)
						{
							if (mParty.Item[i] > 0 && (!battle || (battle && i < 5)))
							{
								itemMenuItemList.Add(itemNames[i]);
								mUsableItemIDList.Add(i);
							}
						}

						if (battle)
						{
							if (mUsableItemIDList.Count > 0)
								ShowMenu(MenuMode.BattleChooseItem, itemMenuItemList.ToArray());
							else
								BattleMode();
						}
						else
						{
							if (mUsableItemIDList.Count > 0)
								ShowMenu(MenuMode.ChooseItem, itemMenuItemList.ToArray());
							else
								AppendText("가지고 있는 아이템이 없습니다.");
						}
					}

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						if (mMenuMode == MenuMode.EnemySelectMode)
						{
							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

							var init = mEnemyFocusID;
							do
							{
								if (mEnemyFocusID == 0)
									mEnemyFocusID = mEncounterEnemyList.Count - 1;
								else
									mEnemyFocusID--;
							} while (init != mEnemyFocusID && mEncounterEnemyList[mEnemyFocusID].Dead == true);


							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}
						else
						{
							if (mMenuFocusID == 0)
								mMenuFocusID = mMenuCount - 1;
							else
								mMenuFocusID--;

							FocusMenuItem();
						}
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						if (mMenuMode == MenuMode.EnemySelectMode)
						{
							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

							var init = mEnemyFocusID;
							do
							{
								mEnemyFocusID = (mEnemyFocusID + 1) % mEncounterEnemyList.Count;
							} while (init != mEnemyFocusID && mEncounterEnemyList[mEnemyFocusID].Dead == true);

							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}
						else
						{
							mMenuFocusID = (mMenuFocusID + 1) % mMenuCount;
							FocusMenuItem();
						}
					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
					{
						// 닫을 수 없는 메뉴
						if (mMenuMode == MenuMode.BattleStart || mMenuMode == MenuMode.BattleCommand || mMenuMode == MenuMode.FinalChoice)
							return;

						AppendText("");
						var menuMode = HideMenu();

						if (menuMode == MenuMode.ChooseTrainSkill)
							ShowChooseTrainSkillMemberMenu();
						else if (menuMode == MenuMode.ChooseTrainMagic)
							ShowChooseTrainMagicMemberMenu();
						else if (menuMode == MenuMode.BuyWeapon || menuMode == MenuMode.BuyShield || menuMode == MenuMode.BuyArmor)
							ShowWeaponShopMenu();
						else if (menuMode != MenuMode.None && menuMode != MenuMode.BattleLose && menuMode != MenuMode.ChooseGameOverLoadGame && mSpecialEvent == SpecialEventType.None)
						{
							if (menuMode == MenuMode.CastOneMagic ||
							menuMode == MenuMode.CastAllMagic ||
							menuMode == MenuMode.CastSpecial ||
							menuMode == MenuMode.ChooseBattleCureSpell ||
							menuMode == MenuMode.CastESP ||
							menuMode == MenuMode.CastSummon)
							{
								BattleMode();
							}
							else if (menuMode == MenuMode.ChooseESPMagic)
							{
								ShowCastESPMenu();
							}
							else if (menuMode == MenuMode.EnemySelectMode)
							{
								mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

								switch (mBattleCommandID)
								{
									case 0:
										BattleMode();
										break;
									case 1:
										ShowCastOneMagicMenu();
										break;
									case 3:
										ShowCastSpecialMenu();
										break;
									case 5:
										BattleMode();
										break;
								}
							}
							else if (menuMode == MenuMode.ApplyBattleCureSpell || menuMode == MenuMode.ApplyBattleCureAllSpell)
								ShowCureDestMenu(mPlayerList[mBattlePlayerID], MenuMode.ChooseBattleCureSpell);
							else if (menuMode == MenuMode.ConfirmExitMap)
							{
								//mParty.YAxis--;
								ClearDialog();

								mParty.XAxis = mPrevX;
								mParty.YAxis = mPrevY;
							}
							else if (menuMode == MenuMode.AskEnter)
							{
								if (mTryEnterType == EnterType.CabinOfRegulus)
								{
									mParty.XAxis = mPrevX;
									mParty.YAxis = mPrevY;
								}
							}
						}
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						var abilityUpPrice = new int[] {
							30, 60, 125, 250, 500, 1_000, 2_000, 3_000, 4_000,6_000,
							8_000, 12_000, 16_000, 24_000, 32_000, 64_000,96_000, 128_000, 192_000, 256_000,
							384_000, 512_000, 770_000, 1_000_000, 1_500_000, 2_000_000, 2_500_000, 3_000_000, 3_500_000, 4_000_000
						};

						void SelectEnemy()
						{
							mMenuMode = MenuMode.EnemySelectMode;

							for (var i = 0; i < mEncounterEnemyList.Count; i++)
							{
								if (!mEncounterEnemyList[i].Dead)
								{
									mEnemyFocusID = i;
									break;
								}
							}

							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}

						void ShowFileMenu(MenuMode mode)
						{
							if (mode == MenuMode.ChooseLoadGame || mode == MenuMode.ChooseGameOverLoadGame)
								AppendText("불러내고 싶은 게임을 선택하십시오.");
							else
								AppendText("게임의 저장 장소를 선택하십시오.");

							ShowMenu(mode, new string[] {
													"본 게임 데이터",
													"게임 데이터 1 (부)",
													"게임 데이터 2 (부)",
													"게임 데이터 3 (부)",
													"게임 데이터 4 (부)",
													"게임 데이터 5 (부)",
													"게임 데이터 6 (부)",
													"게임 데이터 7 (부)",
													"게임 데이터 8 (부)"
												});
						}

						async Task<bool> LoadGame(int id)
						{
							mMenuMode = MenuMode.None;

							var success = await LoadFile(id);
							if (success)
							{
								mBattlePlayerID = 0;
								mBattleFriendID = 0;
								mBattleCommandID = 0;
								mBattleToolID = 0;
								mEnemyFocusID = 0;
								mBattleCommandQueue.Clear();
								mBatteEnemyQueue.Clear();
								mBattleTurn = BattleTurn.None;

								mSpecialEvent = SpecialEventType.None;
								mBattleEvent = BattleEvent.None;

								ShowMap();

								AppendText(new string[] { $"[color={RGB.LightCyan}]저장했던 게임을 다시 불러옵니다.[/color]" });

								return true;
							}
							else
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]해당 슬롯에는 저장된 게임이 없습니다. 다른 슬롯을 선택해 주십시오.[/color]" });

								ShowFileMenu(MenuMode.ChooseLoadGame);
								return false;
							}
						}

						void ShowApplyItemResult(MenuMode choiceMenuMode, string result)
						{
							switch (choiceMenuMode)
							{
								case MenuMode.BattleChooseItem:
								case MenuMode.BattleUseItemWhom:
									Talk(result);
									mSpecialEvent = SpecialEventType.BattleUseItem;
									break;
								default:
									AppendText(result);
									break;
							}
						}

						void ShowWizardEye()
						{
							var xInit = 0;
							var yInit = 0;
							var width = 0;
							var height = 0;

							if (mMapWidth <= 100)
							{
								xInit = 0;
								width = mMapWidth;
							}
							else
							{
								xInit = mParty.XAxis - 50;

								if (xInit <= 0)
								{
									xInit = 0;

									if (mMapWidth > 100)
										width = 100;
									else
										width = mMapWidth;
								}
								else if (xInit + 100 > mMapWidth)
								{
									if (mMapWidth > 100)
									{
										xInit = mMapWidth - 100;
										width = 100;
									}
									else
									{
										xInit = 0;
										width = mMapWidth;
									}
								}
								else
									width = 100;
							}

							if (mMapHeight <= 80)
							{
								yInit = 0;
								height = mMapHeight;
							}
							else
							{
								yInit = mParty.YAxis - 40;

								if (yInit <= 0)
								{
									yInit = 0;

									if (mMapHeight > 80)
										height = 80;
									else
										height = mMapHeight;
								}
								else if (yInit + 80 > mMapHeight)
								{
									if (mMapHeight > 80)
									{
										yInit = mMapHeight - 80;
										height = 80;
									}
									else
									{
										yInit = 0;
										height = mMapHeight;
									}
								}
								else
									height = 80;
							}

							MapCanvas.Width = width * 4;
							MapCanvas.Height = height * 4;

							lock (mWizardEye)
							{
								mWizardEye.Set(width, height);

								var offset = 0;

								const int BLACK = 0;
								const int CYAN = 3;
								const int LIGHTBLUE = 9;
								const int LIGHTGREEN = 10;
								const int LIGHTCYAN = 11;
								const int LIGHTRED = 12;
								const int LIGHTMAGENTA = 13;
								const int WHITE = 15;

								for (var y = yInit; y < yInit + height; y++)
								{
									for (var x = xInit; x < xInit + width; x++)
									{
										if (mParty.XAxis == x && mParty.YAxis == y)
										{
											mWizardEyePosX = offset % width;
											mWizardEyePosY = offset / width;
										}

										var tileInfo = GetTileInfo(x, y);
										if (mPosition == PositionType.Town)
										{
											if ((1 <= tileInfo && tileInfo <= 18) || tileInfo == 20 || tileInfo == 21)
												mWizardEye.Data[offset] = WHITE;
											else if (tileInfo == 22)
												mWizardEye.Data[offset] = LIGHTGREEN;
											else if (tileInfo == 23)
												mWizardEye.Data[offset] = LIGHTCYAN;
											else if (tileInfo == 24)
												mWizardEye.Data[offset] = LIGHTBLUE;
											else if (tileInfo == 25)
												mWizardEye.Data[offset] = CYAN;
											else if (tileInfo == 26)
												mWizardEye.Data[offset] = LIGHTRED;
											else if (tileInfo == 0 || tileInfo == 19 || (27 <= tileInfo && tileInfo <= 47))
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTMAGENTA;
										}
										else if (mPosition == PositionType.Ground)
										{
											if (1 <= tileInfo && tileInfo <= 20)
												mWizardEye.Data[offset] = WHITE;
											else if (tileInfo == 22)
												mWizardEye.Data[offset] = LIGHTCYAN;
											else if (tileInfo == 48)
												mWizardEye.Data[offset] = LIGHTBLUE;
											else if (tileInfo == 23 || tileInfo == 49)
												mWizardEye.Data[offset] = CYAN;
											else if (tileInfo == 50)
												mWizardEye.Data[offset] = LIGHTRED;
											else if (tileInfo == 0)
											{
												if (mParty.Map == 4)
													mWizardEye.Data[offset] = WHITE;
												else
													mWizardEye.Data[offset] = BLACK;
											}
											else if (24 <= tileInfo && tileInfo <= 47)
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTGREEN;

										}
										else if (mPosition == PositionType.Den)
										{
											if ((1 <= tileInfo && tileInfo <= 40) || tileInfo == 51)
												mWizardEye.Data[offset] = WHITE;
											else if (tileInfo == 54)
												mWizardEye.Data[offset] = LIGHTGREEN;
											else if (tileInfo == 53)
												mWizardEye.Data[offset] = LIGHTCYAN;
											else if (tileInfo == 48)
												mWizardEye.Data[offset] = LIGHTBLUE;
											else if (tileInfo == 49)
												mWizardEye.Data[offset] = CYAN;
											else if (tileInfo == 50)
												mWizardEye.Data[offset] = LIGHTRED;
											else if (tileInfo == 52)
											{
												if (mParty.Map == 2)
													mWizardEye.Data[offset] = LIGHTBLUE;
												else
													mWizardEye.Data[offset] = BLACK;
											}
											else if (tileInfo == 0 || (41 <= tileInfo && tileInfo <= 47))
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTMAGENTA;
										}
										else if (mPosition == PositionType.Keep)
										{
											if ((1 <= tileInfo && tileInfo <= 39) || tileInfo == 51)
												mWizardEye.Data[offset] = WHITE;
											else if (tileInfo == 54)
												mWizardEye.Data[offset] = LIGHTGREEN;
											else if (tileInfo == 53)
												mWizardEye.Data[offset] = LIGHTCYAN;
											else if (tileInfo == 48)
												mWizardEye.Data[offset] = LIGHTBLUE;
											else if (tileInfo == 49)
												mWizardEye.Data[offset] = CYAN;
											else if (tileInfo == 50)
												mWizardEye.Data[offset] = LIGHTRED;
											else if (tileInfo == 0 || tileInfo == 52 || (40 <= tileInfo && tileInfo <= 47))
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTMAGENTA;
										}

										offset++;
									}
								}
							}

							mWizardEyeTimer.Start();
							MapCanvas.Visibility = Visibility.Visible;

							Talk(" 주지사의 눈을 통해 이 지역을 바라보고 있다.");
							mSpecialEvent = SpecialEventType.WizardEye;
						}

						void ShowCureSpellMenu(Lore player, int whomID, MenuMode applyCureMode, MenuMode applyAllCureMode)
						{
							if ((whomID < mPlayerList.Count && mAssistPlayer == null) || (whomID < mPlayerList.Count + 1 && mAssistPlayer != null))
							{
								int availableCount;
								if (player.ClassType == ClassCategory.Magic)
									availableCount = player.CureMagic / 10;
								else
									availableCount = player.AxeSkill / 10;

								if (availableCount > 5)
									availableCount = 5;

								var cureMagicMenu = new string[availableCount];
								for (var i = 1; i <= availableCount; i++)
									cureMagicMenu[i - 1] = Common.GetMagicName(3, i);

								ShowMenu(applyCureMode, cureMagicMenu);
							}
							else
							{
								int availableCount;
								if (player.ClassType == ClassCategory.Magic)
									availableCount = player.CureMagic / 10 - 5;
								else
									availableCount = player.AxeSkill / 10 - 5;

								if (availableCount >= 1)
								{
									var cureMagicMenu = new string[availableCount];
									for (var i = 6; i < 6 + availableCount; i++)
										cureMagicMenu[i - 6] = Common.GetMagicName(3, i);

									ShowMenu(applyAllCureMode, cureMagicMenu);
								}
								else
								{
									if (applyCureMode == MenuMode.ApplyBattleCureSpell || applyAllCureMode == MenuMode.ApplyBattleCureAllSpell)
									{
										Talk(" 강한 치료 마법은 아직 불가능 합니다.");
										mSpecialEvent = SpecialEventType.BackToBattleMode;
									}
									else
										Dialog(" 강한 치료 마법은 아직 불가능 합니다.");
								}
							}
						}

						void Teleport(MenuMode newMenuMode)
						{
							if (mParty.Map == 15 || mParty.Map == 16 || mParty.Map == 17)
								AppendText($"[color={RGB.LightMagenta}]이 동굴의 악의 힘이 공간 이동을 방해 합니다.[/color]");
							else
							{
								AppendText($"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]");

								ShowMenu(newMenuMode, new string[] { "북쪽으로 공간이동",
														"남쪽으로 공간이동",
														"동쪽으로 공간이동",
														"서쪽으로 공간이동" });
							}
						}

						bool VerifyWeapon(Lore equipPlayer, int weapon)
						{
							if (equipPlayer.ClassType == ClassCategory.Magic)
								return false;
							else if ((equipPlayer.Class == 1 && 1 <= weapon && weapon <= 28) ||
								(equipPlayer.Class == 2 && 1 <= weapon && weapon <= 21) ||
								(equipPlayer.Class == 3 && 1 <= weapon && weapon <= 7) ||
								(equipPlayer.Class == 4 && 8 <= weapon && weapon <= 28) ||
								(equipPlayer.Class == 6 && ((1 <= weapon && weapon <= 7) || (15 <= weapon && weapon <= 28))) ||
								(equipPlayer.Class == 7 && ((1 <= weapon && weapon <= 7) || (15 <= weapon && weapon <= 28))))
								return true;
							else
								return false;
						}

						bool VerifyShield(Lore equipPlayer, int shield)
						{
							if (equipPlayer.ClassType != ClassCategory.Sword)
								return false;
							else if (equipPlayer.Class == 1 || equipPlayer.Class == 2 || equipPlayer.Class == 3 || equipPlayer.Class == 7)
								return true;
							else
								return false;
						}

						bool VerifyArmor(Lore equipPlayer, int armor)
						{
							if ((equipPlayer.ClassType == ClassCategory.Magic && armor == 1) ||
								(equipPlayer.ClassType == ClassCategory.Sword && ((1 <= armor && armor <= 10) || armor == 255)))
								return true;
							else
								return false;
						}

						var menuMode = HideMenu();
						ClearDialog();

						if (menuMode == MenuMode.EnemySelectMode)
						{
							AddBattleCommand();
						}
						else if (menuMode == MenuMode.Game)
						{
							if (mMenuFocusID == 0)
							{
								ShowPartyStatus();
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { "능력을 보고 싶은 인물을 선택하시오" });
								ShowCharacterMenu(MenuMode.ViewCharacter);
							}
							else if (mMenuFocusID == 2)
							{
								AppendText("");
								DialogText.Visibility = Visibility.Collapsed;

								for (var i = 0; i < 6; i++)
								{
									if (i < mPlayerList.Count)
										mHealthTextList[i].Update(mPlayerList[i].Name, mPlayerList[i].Poison, mPlayerList[i].Unconscious, mPlayerList[i].Dead);
									else if (i == mPlayerList.Count)
										mHealthTextList[i].Update(mAssistPlayer.Name, mAssistPlayer.Poison, mAssistPlayer.Unconscious, mAssistPlayer.Dead);
									else
										mHealthTextList[i].Clear();
								}

								StatHealthPanel.Visibility = Visibility.Visible;
								ContinueText.Visibility = Visibility.Visible;
							}
							else if (mMenuFocusID == 3)
							{
								ShowCharacterMenu(MenuMode.CastSpell, false);
							}
							else if (mMenuFocusID == 4)
							{
								ShowCharacterMenu(MenuMode.Extrasense, false);
							}
							else if (mMenuFocusID == 5)
							{
								Rest();
							}
							else if (mMenuFocusID == 6)
							{
								AppendText("서로 바꿀 물품을 고르시오");

								ShowMenu(MenuMode.ExchangeItem, new string[] {
									"사용중인 무기",
									"사용중인 방패",
									"사용중인 갑옷"
								});
							}
							else if (mMenuFocusID == 7)
								ShowCharacterMenu(MenuMode.UseItemPlayer, false);
							else if (mMenuFocusID == 8)
							{
								AppendText(new string[] { "게임 선택 상황" });

								var gameOptions = new string[]
								{
														"난이도 조절",
														"정식 일행의 순서 정렬",
														"일행의 장비를 해제",
														"일행에서 제외 시킴",
														"이전의 게임을 재개",
														"현재의 게임을 저장",
														"게임을 마침",
								};

								ShowMenu(MenuMode.GameOptions, gameOptions);
							}
						}
						else if (menuMode == MenuMode.ViewCharacter)
						{
							DialogText.Visibility = Visibility.Collapsed;
							StatPanel.Visibility = Visibility.Visible;

							var player = mMenuFocusID < mPlayerList.Count ? mPlayerList[mMenuFocusID] : mAssistPlayer;

							StatPlayerName.Text = player.Name;
							StatPlayerGender.Text = player.GenderStr;
							StatPlayerClass.Text = player.ClassStr;

							StatStrength.Text = player.Strength.ToString();
							StatMentality.Text = player.Mentality.ToString();
							StatConcentration.Text = player.Concentration.ToString();
							StatEndurance.Text = player.Endurance.ToString();
							StatResistance.Text = player.Resistance.ToString();
							StatAgility.Text = player.Agility.ToString();
							StatAccuracy.Text = player.Accuracy.ToString();
							StatLuck.Text = player.Luck.ToString();

							if (player.ClassType == ClassCategory.Sword)
							{
								StatAbility1Title.Text = "베는 무기 기술치 :";
								StatAbility1Value.Text = player.SwordSkill.ToString();

								if (player.Class != 7)
									StatAbility2Title.Text = "찍는 무기 기술치 :";
								else
									StatAbility2Title.Text = "치료 마법 능력치 :";
								StatAbility2Value.Text = player.AxeSkill.ToString();

								StatAbility3Title.Text = "찌르는 무기 기술치 :";
								StatAbility3Value.Text = player.SpearSkill.ToString();

								StatAbility4Title.Text = "쏘는 무기 기술치 :";
								StatAbility4Value.Text = player.BowSkill.ToString();

								StatAbility5Title.Text = "방패 사용 기술치 :";
								StatAbility5Value.Text = player.ShieldSkill.ToString();

								StatAbility6Title.Text = "맨손 사용 기술치 :";
								StatAbility6Value.Text = player.FistSkill.ToString();
							}
							else if (player.ClassType == ClassCategory.Magic)
							{
								StatAbility1Title.Text = "공격 마법 능력치 :";
								StatAbility1Value.Text = player.AttackMagic.ToString();

								StatAbility2Title.Text = "변화 마법 능력치 :";
								StatAbility2Value.Text = player.PhenoMagic.ToString();

								StatAbility3Title.Text = "치료 마법 능력치 :";
								StatAbility3Value.Text = player.CureMagic.ToString();

								StatAbility4Title.Text = "특수 마법 능력치 :";
								StatAbility4Value.Text = player.SpecialMagic.ToString();

								StatAbility5Title.Text = "초 자연력 능력치 :";
								StatAbility5Value.Text = player.ESPMagic.ToString();

								StatAbility6Title.Text = "소환 마법 능력치 :";
								StatAbility6Value.Text = player.SummonMagic.ToString();
							}

							StatExp.Text = player.Experience.ToString("#,#0");
							StatLevel.Text = player.Level.ToString();

							StatWeapon.Text = Common.GetWeaponName(mPlayerList[mMenuFocusID].Weapon);
							StatShield.Text = Common.GetShieldName(mPlayerList[mMenuFocusID].Shield);
							StatArmor.Text = Common.GetArmorName(mPlayerList[mMenuFocusID].Armor);
						}
						else if (menuMode == MenuMode.CastSpell)
						{
							if (!mPlayerList[mMenuFocusID].IsAvailable)
								AppendText($"{mPlayerList[mMenuFocusID].GenderPronoun}는 마법을 사용할 수 있는 상태가 아닙니다");
							else if (mPlayerList[mMenuFocusID].ClassType == ClassCategory.Sword && mPlayerList[mMenuFocusID].Class != 7)
								AppendText($"{mPlayerList[mMenuFocusID].NameSubjectJosa} 마법을 사용할 수 없는 계열입니다.");
							else
							{
								mMagicPlayer = mPlayerList[mMenuFocusID];
								AppendText("사용할 마법의 종류 ===>");
								ShowMenu(MenuMode.SpellCategory, new string[]
								{
									"치료 마법",
									"변화 마법"
								});
							}
						}
						else if (menuMode == MenuMode.SpellCategory)
						{
							if (mMenuFocusID == 0)
							{
								ShowCureDestMenu(mMagicPlayer, MenuMode.ChooseCureSpell);
							}
							else if (mMenuFocusID == 1)
							{
								if (mMagicPlayer.ClassType == ClassCategory.Sword)
									AppendText($"{mMagicPlayer.NameSubjectJosa} 변화 마법을 사용하는 계열이 아닙니다.");
								else
								{
									AppendText(new string[] { "선택" });

									int availableCount = mMagicPlayer.PhenoMagic / 10;
									if (availableCount > 10)
										availableCount = 10;

									var phenominaMagicMenu = new string[availableCount];
									for (var i = 0; i < availableCount; i++)
										phenominaMagicMenu[i] = Common.GetMagicName(2, i + 1);

									ShowMenu(MenuMode.ApplyPhenominaMagic, phenominaMagicMenu);
								}
							}
						}
						else if (menuMode == MenuMode.ChooseCureSpell || menuMode == MenuMode.ChooseBattleCureSpell)
						{
							AppendText(new string[] { "선택" });

							if (mMenuFocusID < mPlayerList.Count)
								mMagicWhomPlayer = mPlayerList[mMenuFocusID];
							else if (mMenuFocusID == mPlayerList.Count && mAssistPlayer != null)
								mMagicWhomPlayer = mAssistPlayer;
							else
							{
								int curePoint;
								if (mMagicPlayer.ClassType == ClassCategory.Magic)
									curePoint = mMagicPlayer.CureMagic / 10 - 5;
								else
									curePoint = mMagicPlayer.AxeSkill / 10 - 5;

								if (curePoint < 1)
								{
									Talk("강한 치료 마법은 아직 불가능 합니다.");

									if (menuMode == MenuMode.ChooseBattleCureSpell)
										mSpecialEvent = SpecialEventType.BackToBattleMode;
									else
										ContinueText.Visibility = Visibility.Visible;
									return;
								}
							}

							if (menuMode == MenuMode.ChooseCureSpell)
								ShowCureSpellMenu(mMagicPlayer, mMenuFocusID, MenuMode.ApplyCureMagic, MenuMode.ApplyCureAllMagic);
							else
								ShowCureSpellMenu(mPlayerList[mBattlePlayerID], mMenuFocusID, MenuMode.ApplyBattleCureSpell, MenuMode.ApplyBattleCureAllSpell);
						}
						else if (menuMode == MenuMode.ApplyCureMagic)
						{
							mMenuMode = MenuMode.None;

							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							mCureResult.Clear();
							CureSpell(mMagicPlayer, mMagicWhomPlayer, mMenuFocusID, mCureResult);

							ShowCureResult(false);
						}
						else if (menuMode == MenuMode.ApplyCureAllMagic)
						{
							mMenuMode = MenuMode.None;

							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							mCureResult.Clear();
							CureAllSpell(mMagicPlayer, mMenuFocusID, mCureResult);

							ShowCureResult(false);
						}
						else if (menuMode == MenuMode.ApplyPhenominaMagic)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								if (mMagicPlayer.SP < 1)
									ShowNotEnoughSP();
								else
								{
									if (mParty.Etc[0] + mMagicPlayer.PhenoMagic / 10 < 256)
										mParty.Etc[0] += mMagicPlayer.PhenoMagic / 10;
									else
										mParty.Etc[0] = 255;

									AppendText($"[color={RGB.White}]일행은 마법의 횃불을 밝혔습니다.[/color]");
									mMagicPlayer.SP--;
									DisplaySP();
									UpdateView();
								}
							}
							else if (mMenuFocusID == 1)
							{
								if (mMagicPlayer.SP < 5)
									ShowNotEnoughSP();
								else
								{
									mMagicPlayer.SP -= 5;
									ShowWizardEye();
								}
							}
							else if (mMenuFocusID == 2)
							{
								if (mMagicPlayer.SP < 5)
									ShowNotEnoughSP();
								else
								{
									mParty.Etc[3] = 255;

									AppendText(new string[] { $"[color={RGB.White}]일행은 공중 부상 중 입니다.[/color]" });
									mMagicPlayer.SP -= 5;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 3)
							{
								if (mMagicPlayer.SP < 10)
									ShowNotEnoughSP();
								else
								{
									mParty.Etc[1] = 255;

									AppendText(new string[] { $"[color={RGB.White}]일행은 물 위를 걸을 수 있습니다.[/color]" });
									mMagicPlayer.SP -= 10;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 4)
							{
								if (mMagicPlayer.SP < 20)
									ShowNotEnoughSP();
								else
								{
									mParty.Etc[2] = 255;

									AppendText(new string[] { $"[color={RGB.White}]일행은 늪 위를 걸을 수 있습니다.[/color]" });
									mMagicPlayer.SP -= 20;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 5)
							{
								if (mParty.Map == 15 || mParty.Map == 16 || mParty.Map == 17)
									AppendText($"[color={RGB.LightMagenta}]이 동굴의 악의 힘이 기화 이동을 방해 합니다.[/color]");
								else if (mMagicPlayer.SP < 25)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" });

									ShowMenu(MenuMode.VaporizeMoveDirection, new string[] { "북쪽으로 기화 이동",
														"남쪽으로 기화 이동",
														"동쪽으로 기화 이동",
														"서쪽으로 기화 이동" });
								}
							}
							else if (mMenuFocusID == 6)
							{
								if (mParty.Map == 3 || mParty.Map == 4 || mParty.Map == 15 || mParty.Map == 16 || mParty.Map == 17)
									AppendText(new string[] { $"[color={RGB.LightMagenta}]이 지역의 악의 힘이 지형 변화를 방해 합니다.[/color]" });
								else if (mMagicPlayer.SP < 30)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

									ShowMenu(MenuMode.TransformDirection, new string[] { "북쪽에 지형 변화",
														"남쪽에 지형 변화",
														"동쪽에 지형 변화",
														"서쪽에 지형 변화" });
								}
							}
							else if (mMenuFocusID == 7)
							{
								if (mMagicPlayer.SP < 50)
									ShowNotEnoughSP();
								else
									Teleport(MenuMode.TeleportationDirection);
							}
							else if (mMenuFocusID == 8)
							{
								if (mMagicPlayer.SP < 30)
									ShowNotEnoughSP();
								else
								{
									var count = mPlayerList.Count;
									if (mParty.Food + count > 255)
										mParty.Food = 255;
									else
										mParty.Food = mParty.Food + count;
									mMagicPlayer.SP -= 30;
									DisplaySP();

									AppendText(new string[] { $"[color={RGB.White}]식량 제조 마법은 성공적으로 수행되었습니다[/color]",
										$"[color={RGB.White}]            {count} 개의 식량이 증가됨[/color]",
										$"[color={RGB.LightCyan}]      일행의 현재 식량은 {mParty.Food} 개 입니다[/color]" });
								}
							}
							else if (mMenuFocusID == 9)
							{
								if (mParty.Map == 3 || mParty.Map == 4 || mParty.Map == 15 || mParty.Map == 16 || mParty.Map == 17)
									AppendText(new string[] { $"[color={RGB.LightMagenta}]이 지역의 악의 힘이 지형 변화를 방해 합니다.[/color]" });
								else if (mMagicPlayer.SP < 60)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

									ShowMenu(MenuMode.BigTransformDirection, new string[] { "북쪽에 지형 변화",
														"남쪽에 지형 변화",
														"동쪽에 지형 변화",
														"서쪽에 지형 변화" });
								}
							}

							//DisplaySP();
						}
						else if (menuMode == MenuMode.VaporizeMoveDirection)
						{
							int xOffset = 0, yOffset = 0;
							switch (mMenuFocusID)
							{
								case 0:
									yOffset = -1;
									break;
								case 1:
									yOffset = 1;
									break;
								case 2:
									xOffset = 1;
									break;
								case 3:
									xOffset = -1;
									break;
							}

							var newX = mParty.XAxis + 2 * xOffset;
							var newY = mParty.YAxis + 2 * yOffset;

							if (newX < 4 || newX >= mMapWidth - 4 || newY < 5 || newY >= mMapHeight - 4)
								return;

							var canMove = false;

							var moveTile = GetTileInfo(newX, newY);
							switch (mPosition)
							{
								case PositionType.Town:
									if (moveTile == 0 || (27 <= moveTile && moveTile <= 47))
										canMove = true;
									break;
								case PositionType.Ground:
									if (moveTile == 0 || (24 <= moveTile && moveTile <= 47))
										canMove = true;
									break;
								case PositionType.Den:
									if (moveTile == 0 || (41 <= moveTile && moveTile <= 47))
										canMove = true;
									break;
								case PositionType.Keep:
									if (moveTile == 0 || (40 <= moveTile && moveTile <= 47))
										canMove = true;
									break;

							}

							if (!canMove)
								AppendText($"기화 이동이 통하지 않습니다.");
							else
							{
								mMagicPlayer.SP -= 25;
								DisplaySP();

								if (GetTileInfo(mParty.XAxis + xOffset, mParty.YAxis + yOffset) == 0 ||
									((mPosition == PositionType.Den || mPosition == PositionType.Keep) && (GetTileInfo(newX, newY) == 52)))
								{
									AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]");
								}
								else
								{
									mParty.XAxis = newX;
									mParty.YAxis = newY;

									AppendText($"[color={RGB.White}]기화 이동을 마쳤습니다.[/color]");

									if (GetTileInfo(mParty.XAxis, mParty.YAxis) == 0)
										InvokeSpecialEvent(mParty.XAxis, mParty.YAxis);
								}

							}
						}
						else if (menuMode == MenuMode.TransformDirection)
						{

							mMenuMode = MenuMode.None;

							int xOffset = 0, yOffset = 0;
							switch (mMenuFocusID)
							{
								case 0:
									yOffset = -1;
									break;
								case 1:
									yOffset = 1;
									break;
								case 2:
									xOffset = 1;
									break;
								case 3:
									xOffset = -1;
									break;
							}

							var newX = mParty.XAxis + xOffset;
							var newY = mParty.YAxis + yOffset;


							mMagicPlayer.SP -= 30;
							DisplaySP();

							if (GetTileInfo(newX, newY) == 0 ||
									((mPosition == PositionType.Den || mPosition == PositionType.Keep) && GetTileInfo(newX, newY) == 52) ||
									(mPosition == PositionType.Town && GetTileInfo(newX, newY) == 48))
								AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]");
							else
							{
								byte tile;

								switch (mPosition)
								{
									case PositionType.Town:
										tile = 47;
										break;
									case PositionType.Ground:
										tile = 41;
										break;
									case PositionType.Den:
										tile = 43;
										break;
									default:
										tile = 43;
										break;
								}

								UpdateTileInfo(newX, newY, tile);

								AppendText($"[color={RGB.White}]지형 변화에 성공했습니다.[/color]");
							}
						}
						else if (menuMode == MenuMode.TransformDirection)
						{

							mMenuMode = MenuMode.None;

							int xOffset = 0, yOffset = 0;
							switch (mMenuFocusID)
							{
								case 0:
									yOffset = -1;
									break;
								case 1:
									yOffset = 1;
									break;
								case 2:
									xOffset = 1;
									break;
								case 3:
									xOffset = -1;
									break;
							}

							var newX = mParty.XAxis + xOffset;
							var newY = mParty.YAxis + yOffset;

							var range = xOffset == 0 ? 5 : 4;

							mMagicPlayer.SP -= 60;
							DisplaySP();

							byte tile;

							switch (mPosition)
							{
								case PositionType.Town:
									tile = 47;
									break;
								case PositionType.Ground:
									tile = 41;
									break;
								case PositionType.Den:
									tile = 43;
									break;
								default:
									tile = 43;
									break;
							}

							for (var i = 1; i <= range; i++)
							{
								if (GetTileInfo(mParty.XAxis + xOffset * i, mParty.YAxis + yOffset * i) == 0 ||
										((mPosition == PositionType.Den || mPosition == PositionType.Keep) && GetTileInfo(mParty.XAxis + xOffset * i, mParty.YAxis + yOffset * i) == 52) ||
										(mPosition == PositionType.Town && GetTileInfo(mParty.XAxis + xOffset * i, mParty.YAxis + yOffset * i) == 48))
								{
									AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]");
									return;
								}
								else
								{
									UpdateTileInfo(newX, newY, tile);
								}
							}

							AppendText($"[color={RGB.White}]지형 변화에 성공했습니다.[/color]");
						}
						else if (menuMode == MenuMode.TeleportationDirection)
						{
							mTeleportationDirection = mMenuFocusID;

							var rangeItems = new List<Tuple<string, int>>();
							for (var i = 1; i <= 9; i++)
							{
								rangeItems.Add(new Tuple<string, int>($"[color={RGB.White}]##[/color] [color={RGB.LightGreen}]{(i * 1_000):#,#0}[/color] [color={RGB.White}] 공간 이동력[/color]", i));
							}

							ShowSpinner(SpinnerType.TeleportationRange, rangeItems.ToArray(), 5);
						}
						else if (menuMode == MenuMode.Extrasense)
						{
							if (mPlayerList[mMenuFocusID].IsAvailable)
							{
								if (mPlayerList[mMenuFocusID].ClassType == ClassCategory.Sword)
									AppendText($"{mPlayerList[mMenuFocusID].GenderPronoun}에게는 초감각의 능력이 없습니다.");
								else
								{
									mMagicPlayer = mPlayerList[mMenuFocusID];

									AppendText(new string[] { "사용할 초감각의 종류 ===>" });

									var extrsenseMenu = new string[4];
									for (var i = 0; i < 4; i++)
										extrsenseMenu[i] = Common.GetMagicName(5, i + 1);

									ShowMenu(MenuMode.ChooseExtrasense, extrsenseMenu);
								}
							}
							else
								AppendText($"{mPlayerList[mMenuFocusID].GenderPronoun}는 초감각을 사용할 수 있는 상태가 아닙니다");
						}
						else if (menuMode == MenuMode.ChooseExtrasense)
						{
							HideMenu();
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								if (mMagicPlayer.ESPMagic < 70)
									AppendText($"{mMagicPlayer.GenderPronoun}는 투시를 시도해 보았지만 아직은 역부족이었다.");
								else if (mMagicPlayer.SP < 10)
									ShowNotEnoughSP();
								else
								{
									Talk($"[color={RGB.White}]일행은 주위를 투시하고 있다.[/color]");
									mMagicPlayer.SP -= 10;
									DisplaySP();

									mSpecialEvent = SpecialEventType.Penetration;
								}
							}
							else if (mMenuFocusID == 1)
							{
								if (mMagicPlayer.ESPMagic < 10)
									AppendText($"{mMagicPlayer.GenderPronoun}는 예언을 시도해 보았지만 아직은 역부족이었다.");
								else if (mMagicPlayer.SP < 5)
									ShowNotEnoughSP();
								else
								{
									var predictStr = new string[]
									{
										"로드 안을 만날",
										"메너스로 갈",
										"다시 로드 안에게 갈",
										"다시 메너스를 조사할",
										"다시 로어 성으로 돌아갈",
										"라스트디치의 성주를 만날",
										"피라미드 속의 로어 헌터를 찾을",
										"라스트디치의 군주에게로 돌아갈",
										"피라미드 속의 두 동굴에서 두 개의 석판을 찾을",
										"첫 번째 월식을 기다릴",
										"메너스를 통해 지하 세계로 갈",
										"빛의 지붕을 방문할",
										"빛의 사원에서 악의 추종자들의 메시지를 볼",
										"필멸의 생존을 탐험할",
										"지하 세계 중앙의 통로를 통해 지상으로 갈",
										"두 번째 월식을 기다릴",
										"베리알의 동굴에서 결전을 벌일",
										"몰록의 동굴에서 결전을 벌일",
										"마지막 세 번째 월식을 기다릴",
										"아스모데우스의 동굴에서 결전을 벌일",
										"메피스토펠레스와 대결을 벌일"
									};

									int predict = -1;
									switch (mParty.Etc[9])
									{
										case 0:
											predict = 0;
											break;
										case 1:
											predict = 1;
											break;
										case 2:
											predict = 2;
											break;
										case 3:
											predict = 3;
											break;
										case 4:
											predict = 4;
											break;
										case 5:
											switch (mParty.Etc[10])
											{
												case 0:
													predict = 5;
													break;
												case 1:
													predict = 6;
													break;
												case 2:
													predict = 7;
													break;
												case 3:
													predict = 8;
													break;
												case 4:
													predict = 7;
													break;
												case 5:
													predict = 2;
													break;
											}
											break;
										case 6:
											predict = 2;
											break;
										case 7:
											predict = 0;
											break;
										case 8:
											predict = 9;
											break;
										case 9:
											if ((mParty.Etc[8] & (1 << 3)) == 0)
												predict = 13;
											else if ((mParty.Etc[8] & (1 << 2)) == 0)
												predict = 12;
											else if ((mParty.Etc[8] & (1 << 1)) == 0)
												predict = 11;
											else if ((mParty.Etc[8] & 1) == 0)
												predict = 10;
											else
												predict = 14;
											break;
										case 10:
											predict = 2;
											break;
										case 11:
											predict = 15;
											break;
										case 12:
											if ((mParty.Etc[8] & (1 << 6)) == 0)
												predict = 17;
											else if ((mParty.Etc[8] & (1 << 5)) == 0)
												predict = 16;
											else if ((mParty.Etc[8] & (1 << 4)) == 0)
												predict = 10;
											break;
										case 13:
											predict = 2;
											break;
										case 14:
											predict = 18;
											break;
										case 15:
											if ((mParty.Etc[8] & (1 << 2)) == 0)
												predict = 20;
											else if ((mParty.Etc[8] & (1 << 1)) == 0)
												predict = 19;
											else if ((mParty.Etc[8] & 1) == 0)
												predict = 10;
											break;
										case 16:
											predict = 21;
											break;
									}

									AppendText(new string[] { $" 당신은 당신의 미래를 예언한다 ...", "" });
									if (0 < predict && predict >= predictStr.Length)
										AppendText(new string[] { $"[color={RGB.LightGreen}] #[/color] [color={RGB.White}]당신은 어떤 힘에 의해 예언을 방해받고 있다[/color]" }, true);
									else
										AppendText(new string[] { $"[color={RGB.LightGreen}] #[/color] [color={RGB.White}]당신은 {predictStr[predict]} 것이다[/color]" }, true);

									mMagicPlayer.SP -= 5;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 2)
							{
								if (mMagicPlayer.ESPMagic < 40)
									AppendText($"{mMagicPlayer.GenderPronoun}는 독심을 시도해 보았지만 아직은 역부족이었다.");
								else if (mMagicPlayer.SP < 20)
									ShowNotEnoughSP();
								else
								{
									AppendText($"[color={RGB.White}]당신은 잠시 동안 다른 사람의 마음을 읽을 수 있다.[/color]");
									mParty.Etc[4] = 3;

									mMagicPlayer.SP -= 20;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 3)
							{
								if (mMagicPlayer.ESPMagic < 55)
									AppendText($"{mMagicPlayer.GenderPronoun}는 천리안을 시도해 보았지만 아직은 역부족이었다.");
								else if (mMagicPlayer.SP < mMagicPlayer.Level * 5)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

									ShowMenu(MenuMode.TelescopeDirection, new string[] { "북쪽으로 천리안을 사용",
														"남쪽으로 천리안을 사용",
														"동쪽으로 천리안을 사용",
														"서쪽으로 천리안을 사용" });
								}
							}
						}
						else if (menuMode == MenuMode.TelescopeDirection)
						{
							mMagicPlayer.SP -= mMagicPlayer.Level * 5;
							DisplaySP();

							mTelescopePeriod = mMagicPlayer.Level;
							switch (mMenuFocusID)
							{
								case 0:
									mTelescopeYCount = -mMagicPlayer.Level;
									break;
								case 1:
									mTelescopeYCount = mMagicPlayer.Level;
									break;
								case 2:
									mTelescopeXCount = mMagicPlayer.Level;
									break;
								case 3:
									mTelescopeXCount = -mMagicPlayer.Level;
									break;
							}

							Talk($"[color={RGB.White}]천리안 사용중 ...[/color]");
							mSpecialEvent = SpecialEventType.Telescope;
						}
						else if (menuMode == MenuMode.ExchangeItem)
						{
							mExchangeCategory = mMenuFocusID;

							switch (mExchangeCategory)
							{
								case 0:
									AppendText("무기를 바꿀 일원");
									break;
								case 1:
									AppendText("방패를 바꿀 일원");
									break;
								case 2:
									AppendText("갑옷을 바꿀 일원");
									break;
							}

							ShowCharacterMenu(MenuMode.ExchangeItemWhom, false);
						}
						else if (menuMode == MenuMode.ExchangeItemWhom)
						{
							mExchangePlayer = mPlayerList[mMenuFocusID];

							switch (mExchangeCategory)
							{
								case 0:
									AppendText("무기를 바꿀 대상 일원");
									break;
								case 1:
									AppendText("방패를 바꿀 대상 일원");
									break;
								case 2:
									AppendText("갑옷을 바꿀 대상 일원");
									break;
							}

							ShowCharacterMenu(MenuMode.SwapItem, false);
						}
						else if (menuMode == MenuMode.SwapItem)
						{
							var whomPlayer = mPlayerList[mMenuFocusID];

							switch (mExchangeCategory)
							{
								case 0:
									{
										var temp = mExchangePlayer.Weapon;
										mExchangePlayer.Weapon = whomPlayer.Weapon;
										whomPlayer.Weapon = temp;
										break;
									}
								case 1:
									{
										var temp = mExchangePlayer.Shield;
										mExchangePlayer.Shield = whomPlayer.Shield;
										whomPlayer.Shield = temp;
										break;
									}
								case 2:
									{
										var temp = mExchangePlayer.Armor;
										mExchangePlayer.Armor = whomPlayer.Armor;
										whomPlayer.Armor = temp;
										break;
									}
							}

							UpdateItem(mExchangePlayer);
							UpdateItem(whomPlayer);

							UpdatePlayersStat();
							AppendText("");
						}
						else if (menuMode == MenuMode.UseItemPlayer)
						{
							if (mPlayerList[mMenuFocusID].IsAvailable)
								UseItem(mPlayerList[mMenuFocusID], false);
							else
								AppendText($" {mPlayerList[mMenuFocusID].NameSubjectJosa} 물품을 사용할 수 있는 상태가 아닙니다.");
						}
						else if (menuMode == MenuMode.GameOptions)
						{
							if (mMenuFocusID == 0)
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]한 번에 출현하는 적들의 최대치를 기입하십시오[/color]" });

								var maxEnemyStr = new string[5];
								for (var i = 0; i < maxEnemyStr.Length; i++)
									maxEnemyStr[i] = $"{i + 3}명의 적들";

								ShowMenu(MenuMode.SetMaxEnemy, maxEnemyStr);
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]현재의 일원의 전투 순서를 정렬하십시오.[/color]",
												"[color=e0ffff]순서를 바꿀 일원[/color]" });

								ShowCharacterMenu(MenuMode.OrderFromCharacter, false);
							}
							else if (mMenuFocusID == 2)
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]장비를 제거할 사람을 고르시오.[/color]" });

								ShowCharacterMenu(MenuMode.UnequipCharacter, false);
							}
							else if (mMenuFocusID == 3)
							{
								AppendText($"[color={RGB.LightRed}]일행에서 제외하고 싶은 사람을 고르십시오.[/color]");

								ShowCharacterMenu(MenuMode.DelistCharacter);
							}
							else if (mMenuFocusID == 4)
							{
								ShowFileMenu(MenuMode.ChooseLoadGame);
							}
							else if (mMenuFocusID == 5)
							{
								ShowFileMenu(MenuMode.ChooseSaveGame);
							}
							else if (mMenuFocusID == 6)
							{
								AppendText(new string[] { $"[color={RGB.LightGreen}]정말로 끝내겠습니까?[/color]" });

								ShowMenu(MenuMode.ConfirmExit, new string[] {
									"<< 아니오 >>",
									"<<   예   >>"
								});
							}
						}
						else if (menuMode == MenuMode.SetMaxEnemy)
						{
							mMaxEnemy = mMenuFocusID + 3;

							AppendText($"[color={RGB.LightRed}]일행들의 지금 성격은 어떻습니까?[/color]");

							ShowMenu(MenuMode.SetEncounterType, new string[]
							{
								"일부러 전투를 피하고 싶다",
								"너무 잦은 전투는 원하지 않는다",
								"마주친 적과는 전투를 하겠다",
								"보이는 적들과는 모두 전투하겠다",
								"그들은 피에 굶주려 있다"
							});
						}
						else if (menuMode == MenuMode.SetEncounterType)
						{
							mEncounter = 6 - (mMenuFocusID + 1);

							AppendText($"[color={RGB.LightRed}]의식 불명인 적까지 공격 하겠습니까?[/color]");

							ShowMenu(MenuMode.AttackCruelEnemy, new string[]
							{
								"물론 그렇다",
								"그렇지 않다"
							});
						}
						else if (menuMode == MenuMode.AttackCruelEnemy)
						{
							AppendText("");

							if (mMenuFocusID == 0)
								mParty.Cruel = true;
							else
								mParty.Cruel = false;
						}
						else if (menuMode == MenuMode.OrderFromCharacter)
						{
							mMenuMode = MenuMode.None;

							mOrderFromPlayerID = mMenuFocusID;

							AppendText($"[color={RGB.LightCyan}]순서를 바꿀 대상 일원[/color]");

							ShowCharacterMenu(MenuMode.OrderToCharacter, false);
						}
						else if (menuMode == MenuMode.OrderToCharacter)
						{
							var tempPlayer = mPlayerList[mOrderFromPlayerID];
							mPlayerList[mOrderFromPlayerID] = mPlayerList[mMenuFocusID];
							mPlayerList[mMenuFocusID] = tempPlayer;

							DisplayPlayerInfo();

							AppendText("");
						}
						else if (menuMode == MenuMode.UnequipCharacter)
						{
							if (mUnequipPlayer.Weapon == 0 && mUnequipPlayer.Shield == 0 && mUnequipPlayer.Armor == 0)
								AppendText($"해제할 장비가 없습니다.");
							else
							{
								mUnequipPlayer = mPlayerList[mMenuFocusID];

								AppendText($"[color={RGB.LightRed}]제거할 장비를 고르시오.[/color]");

								var menuList = new List<string>();
								if (mUnequipPlayer.Weapon != 0)
									menuList.Add(Common.GetWeaponName(mUnequipPlayer.Weapon));

								if (mUnequipPlayer.Shield != 0)
									menuList.Add(Common.GetShieldName(mUnequipPlayer.Shield));

								if (mUnequipPlayer.Armor != 0)
									menuList.Add(Common.GetArmorName(mUnequipPlayer.Armor));

								ShowMenu(MenuMode.Unequip, menuList.ToArray());
							}
						}
						else if (menuMode == MenuMode.Unequip)
						{
							var weaponType = 0;

							if (mMenuFocusID == 0)
							{
								if (mUnequipPlayer.Weapon != 0)
									weaponType = 0;
								else if (mUnequipPlayer.Shield != 0)
									weaponType = 1;
								else
									weaponType = 2;
							}
							else if (mMenuFocusID == 1)
							{
								if (mUnequipPlayer.Shield != 0)
									weaponType = 1;
								else
									weaponType = 2;
							}
							else if (mMenuFocusID == 2)
								weaponType = 2;

							switch (weaponType)
							{
								case 0:
									mUnequipPlayer.Weapon = 0;
									AppendText($"{mUnequipPlayer.Name}의 무기는 해제되었습니다.");
									break;
								case 1:
									mUnequipPlayer.Shield = 0;
									AppendText($"{mUnequipPlayer.Name}의 방패는 해제되었습니다.");
									break;
								case 2:
									mUnequipPlayer.Armor = 0;
									AppendText($"{mUnequipPlayer.Name}의 갑옷은 해제되었습니다.");
									break;
							}

							UpdateItem(mUnequipPlayer);
						}
						else if (menuMode == MenuMode.DelistCharacter)
						{
							mPlayerList.RemoveAt(mMenuFocusID);

							DisplayPlayerInfo();
							AppendText("");
						}
						else if (menuMode == MenuMode.ConfirmExit)
						{
							if (mMenuFocusID == 0)
								AppendText("");
							else
								CoreApplication.Exit();
						}
						else if (menuMode == MenuMode.ChooseWeaponType)
						{
							ShowWeaponTypeMenu(mMenuFocusID);
						}
						else if (menuMode == MenuMode.BuyWeapon)
						{
							var price = weaponPrice[mWeaponTypeID, mMenuFocusID];

							if (mParty.Gold < price)
								ShowNotEnoughMoney(SpecialEventType.CantBuyWeapon);
							else
							{
								mBuyWeaponID = mMenuFocusID + 1;

								AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetWeaponNameJosa(mBuyWeaponID)} 사용하시겠습니까?[/color]" });

								ShowCharacterMenu(MenuMode.UseWeaponCharacter);
							}
						}
						else if (menuMode == MenuMode.UseWeaponCharacter)
						{
							var player = mPlayerList[mMenuFocusID];

							if (VerifyWeapon(player, (mWeaponTypeID - 1) * 7 + mBuyWeaponID))
							{
								player.Weapon = (mWeaponTypeID - 1) * 7 + mBuyWeaponID;
								UpdateItem(player);

								mParty.Gold -= weaponPrice[mWeaponTypeID, mBuyWeaponID - 1];
							}
							else
							{
								Talk($" 이 무기는 {player.Name}에게는 맞지 않습니다.");
								mSpecialEvent = SpecialEventType.CantBuyWeapon;
							}
						}
						else if (menuMode == MenuMode.BuyShield)
						{
							var price = shieldPrice[mMenuFocusID];

							if (mParty.Gold < price)
								ShowNotEnoughMoney(SpecialEventType.CantBuyWeapon);
							else
							{
								mBuyWeaponID = mMenuFocusID + 1;

								AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetShieldNameJosa(mBuyWeaponID)} 사용하시겠습니까?[/color]" });

								ShowCharacterMenu(MenuMode.UseShieldCharacter);
							}
						}
						else if (menuMode == MenuMode.UseShieldCharacter)
						{
							var player = mPlayerList[mMenuFocusID];

							if (VerifyShield(player, mBuyWeaponID))
							{
								player.Shield = mBuyWeaponID;
								UpdateItem(player);

								mParty.Gold -= shieldPrice[mBuyWeaponID - 1];
							}
							else
							{
								Talk($" {player.NameSubjectJosa} 이 방패를 사용 할 수 없습니다.");
								mSpecialEvent = SpecialEventType.CantBuyWeapon;
							}
						}
						else if (menuMode == MenuMode.BuyArmor)
						{
							var price = armorPrice[mMenuFocusID];

							if (mParty.Gold < price)
								ShowNotEnoughMoney(SpecialEventType.CantBuyWeapon);
							else
							{
								mBuyWeaponID = mMenuFocusID + 1;

								AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetArmorNameJosa(mBuyWeaponID)} 사용하시겠습니까?[/color]" });

								ShowCharacterMenu(MenuMode.UseArmorCharacter);
							}
						}
						else if (menuMode == MenuMode.UseArmorCharacter)
						{
							var player = mPlayerList[mMenuFocusID];

							if (VerifyArmor(player, mBuyWeaponID))
							{
								player.Armor = mBuyWeaponID;
								UpdateItem(player);

								mParty.Gold -= armorPrice[mBuyWeaponID - 1];
							}
							else
							{
								Talk($" {player.NameSubjectJosa} 이 갑옷을 사용 할 수 없습니다.");
								mSpecialEvent = SpecialEventType.CantBuyWeapon;
							}
						}
						else if (menuMode == MenuMode.ChooseFoodAmount)
						{
							if (mParty.Gold < (mMenuFocusID + 1) * 100)
								ShowNotEnoughMoney(SpecialEventType.None);
							else
							{
								mParty.Gold -= (mMenuFocusID + 1) * 100;
								var food = (mMenuFocusID + 1) * 10;
								if (mParty.Food + food > 255)
									mParty.Food = 255;
								else
									mParty.Food += food;

								AppendText("매우 고맙습니다.");
							}
						}
						else if (menuMode == MenuMode.BuyExp)
						{
							if (mParty.Gold < (mMenuFocusID + 1) * 10_000)
							{
								Talk(" 일행은 충분한 금이 없었다.");
								mSpecialEvent = SpecialEventType.CantBuyExp;
							}
							else
							{
								AppendText($"[color={RGB.White}] 일행은 {mMenuFocusID + 1}시간동안 훈련을 받게 되었다.[/color]");
								mTrainTime = mMenuFocusID + 1;
								InvokeAnimation(AnimationType.BuyExp);
							}
						}
						else if (menuMode == MenuMode.SelectItem)
						{
							AppendText($"[color={RGB.White}] 개수를 지정하십시오.[/color]");

							mBuyItemID = mMenuFocusID;

							var itemCountArr = new string[10];
							for (var i = 0; i < itemCountArr.Length; i++)
							{
								if (mBuyItemID == 0)
									itemCountArr[i] = $"{mItems[mBuyItemID]} {(i + 1) * 10}개 : 금 {mItemPrices[mBuyItemID] * (i + 1):#,#0}개";
								else
									itemCountArr[i] = $"{mItems[mBuyItemID]} {i + 1}개 : 금 {mItemPrices[mBuyItemID] * (i + 1):#,#0}개";
							}

							ShowMenu(MenuMode.SelectItemAmount, itemCountArr);
						}
						else if (menuMode == MenuMode.SelectItemAmount)
						{
							if (mParty.Gold < mItemPrices[mBuyItemID] * (mMenuFocusID + 1))
							{
								Talk(" 당신에게는 이것을 살 돈이 없습니다.");
								mSpecialEvent = SpecialEventType.CantBuyItem;
								return;
							}

							mParty.Gold -= mItemPrices[mBuyItemID] * (mMenuFocusID + 1);
							if (mBuyItemID == 0)
							{
								if (mParty.Arrow + (mMenuFocusID + 1) * 10 < 32_768)
									mParty.Arrow += (mMenuFocusID + 1) * 10;
								else
									mParty.Arrow = 32_767;
							}
							else
							{
								if (mParty.Item[mBuyItemID + 4] + mMenuFocusID + 1 < 256)
									mParty.Item[mBuyItemID + 4] += mMenuFocusID + 1;
								else
									mParty.Item[mBuyItemID + 4] = 255;
							}

							ShowItemStoreMenu();
						}
						else if (menuMode == MenuMode.SelectMedicine)
						{
							AppendText($"[color={RGB.White}] 개수를 지정 하십시오.[/color]");

							mBuyMedicineID = mMenuFocusID;

							var itemCountArr = new string[10];
							for (var i = 0; i < itemCountArr.Length; i++)
							{
								itemCountArr[i] = $"{mMedicines[mBuyMedicineID]} {i + 1}개 : 금 {mMedicinePrices[mBuyMedicineID] * (i + 1):#,#0}개";
							}

							ShowMenu(MenuMode.SelectMedicineAmount, itemCountArr);
						}
						else if (menuMode == MenuMode.SelectMedicineAmount)
						{
							if (mParty.Gold < mMedicinePrices[mBuyMedicineID] * (mMenuFocusID + 1))
							{
								Talk(" 당신에게는 이것을 살 돈이 없습니다.");
								mSpecialEvent = SpecialEventType.CantBuyMedicine;
								return;
							}

							mParty.Gold -= mMedicinePrices[mBuyMedicineID] * (mMenuFocusID + 1);

							if (mParty.Item[mBuyMedicineID] + mMenuFocusID + 1 < 256)
								mParty.Item[mBuyMedicineID] += mMenuFocusID + 1;
							else
								mParty.Item[mBuyMedicineID] = 255;

							ShowMedicineStoreMenu();
						}
						else if (menuMode == MenuMode.Hospital)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == mPlayerList.Count)
								mCurePlayer = mAssistPlayer;
							else
								mCurePlayer = mPlayerList[mMenuFocusID];

							ShowHealType();
						}
						else if (menuMode == MenuMode.HealType)
						{
							if (mMenuFocusID == 0)
							{
								if (mCurePlayer.Dead > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 죽은 상태입니다");
								else if (mCurePlayer.Unconscious > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 의식불명입니다");
								else if (mCurePlayer.Poison > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 독이 퍼진 상태입니다");
								else if (mCurePlayer.HP >= mCurePlayer.Endurance * mCurePlayer.Level * 10)
									AppendText($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 치료가 필요하지 않습니다[/color]");

								if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious > 0 || mCurePlayer.Poison > 0 || mCurePlayer.HP >= mCurePlayer.Endurance * mCurePlayer.Level * 10)
								{
									ContinueText.Visibility = Visibility;
									mSpecialEvent = SpecialEventType.CureComplete;
								}
								else
								{
									var payment = mCurePlayer.Endurance * mCurePlayer.Level * 10 - mCurePlayer.HP;
									payment = payment * mCurePlayer.Level * 10 / 2 + 1;

									if (mParty.Gold < payment)
										ShowNotEnoughMoney(mSpecialEvent = SpecialEventType.NotCured);
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.HP = mCurePlayer.Endurance * mCurePlayer.Level * 10;

										DisplayHP();

										Talk($"[color={RGB.White}]{mCurePlayer.Name}의 모든 건강이 회복되었다[/color]");
										mSpecialEvent = SpecialEventType.CureComplete;
									}
								}
							}
							else if (mMenuFocusID == 1)
							{
								if (mCurePlayer.Dead > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 죽은 상태입니다");
								else if (mCurePlayer.Unconscious > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 의식불명입니다");
								else if (mCurePlayer.Poison == 0)
									AppendText($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 독에 걸리지 않았습니다[/color]");

								if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious > 0 || mCurePlayer.Poison == 0)
								{
									ContinueText.Visibility = Visibility;
									mSpecialEvent = SpecialEventType.CureComplete;
								}
								else
								{
									var payment = mCurePlayer.Level * 10;

									if (mParty.Gold < payment)
										ShowNotEnoughMoney(SpecialEventType.NotCured);
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.Poison = 0;

										DisplayCondition();

										Talk($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 독이 제거 되었습니다[/color]");
										mSpecialEvent = SpecialEventType.CureComplete;
									}
								}
							}
							else if (mMenuFocusID == 2)
							{
								if (mCurePlayer.Dead > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 죽은 상태입니다");
								else if (mCurePlayer.Unconscious == 0)
									AppendText($"[color={RGB.White}]{mCurePlayer.Name} 의식불명이 아닙니다[/color]");

								if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious == 0)
								{
									ContinueText.Visibility = Visibility;
									mSpecialEvent = SpecialEventType.CureComplete;
								}
								else
								{
									var payment = mCurePlayer.Unconscious * 2;

									if (mParty.Gold < payment)
										ShowNotEnoughMoney(SpecialEventType.NotCured);
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.Unconscious = 0;
										mCurePlayer.HP = 1;

										DisplayCondition();
										DisplayHP();

										Talk($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 의식을 차렸습니다[/color]");
										mSpecialEvent = SpecialEventType.CureComplete;
									}
								}
							}
							else if (mMenuFocusID == 3)
							{
								if (mCurePlayer.Dead == 0)
									AppendText($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 죽지 않았습니다[/color]");

								if (mCurePlayer.Dead == 0)
								{
									ContinueText.Visibility = Visibility;
									mSpecialEvent = SpecialEventType.CureComplete;
								}
								else
								{
									var payment = mCurePlayer.Dead * 100 + 400;

									if (mParty.Gold < payment)
										ShowNotEnoughMoney(SpecialEventType.NotCured);
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.Dead = 0;

										if (mCurePlayer.Unconscious > mCurePlayer.Endurance * mCurePlayer.Level)
											mCurePlayer.Unconscious = mCurePlayer.Endurance * mCurePlayer.Level;

										DisplayCondition();

										Talk($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 다시 살아났습니다[/color]");
										mSpecialEvent = SpecialEventType.CureComplete;
									}
								}
							}
						}
						else if (menuMode == MenuMode.TrainingCenter)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
								ShowTrainMessage();
							else if (mMenuFocusID == 1)
								ShowTrainMagicMessage();
							else if (mMenuFocusID == 2)
								ShowChangeJobForSwordMessage();
							else if (mMenuFocusID == 3)
								ShowChangeJobForMagicMessage();
						}
						else if (menuMode == MenuMode.ChooseTrainSkillMember)
						{
							mMenuMode = MenuMode.None;

							mTrainPlayer = mPlayerList[mMenuFocusID];

							if (mTrainPlayer.ClassType != ClassCategory.Sword)
							{
								AppendText(" 당신은 전투사 계열이 아닙니다.");
								return;
							}

							var readyToLevelUp = true;
							for (var i = 0; i < 6; i++)
							{
								int skill;
								switch (i)
								{
									case 0:
										skill = mTrainPlayer.SwordSkill;
										break;
									case 1:
										skill = mTrainPlayer.AxeSkill;
										break;
									case 2:
										skill = mTrainPlayer.SpearSkill;
										break;
									case 3:
										skill = mTrainPlayer.BowSkill;
										break;
									case 4:
										skill = mTrainPlayer.ShieldSkill;
										break;
									default:
										skill = mTrainPlayer.FistSkill;
										break;

								}

								if (swordEnableClass[mTrainPlayer.Class - 1, i] > 0)
								{
									if (skill < swordEnableClass[mTrainPlayer.Class - 1, i])
									{
										readyToLevelUp = false;
										break;
									}
								}
							}

							if (readyToLevelUp)
							{
								AppendText(" 당신은 모든 과정을 수료했으므로 모든 경험치를 레벨로 바꾸겠습니다.");

								mTrainPlayer.PotentialExperience += mTrainPlayer.Experience;
								mTrainPlayer.Experience = 0;

								return;
							}

							ShowTrainSkillMenu(0);
						}
						else if (menuMode == MenuMode.ChooseTrainSkill)
						{
							mMenuMode = MenuMode.None;

							int skill;
							switch (mTrainSkillList[mMenuFocusID].Item1)
							{
								case 0:
									skill = mTrainPlayer.SwordSkill;
									break;
								case 1:
									skill = mTrainPlayer.AxeSkill;
									break;
								case 2:
									skill = mTrainPlayer.SpearSkill;
									break;
								case 3:
									skill = mTrainPlayer.BowSkill;
									break;
								case 4:
									skill = mTrainPlayer.ShieldSkill;
									break;
								default:
									skill = mTrainPlayer.FistSkill;
									break;
							}

							if (skill >= mTrainSkillList[mMenuFocusID].Item2)
							{
								Talk("이 분야는 더 배울 것이 없습니다");
								mSpecialEvent = SpecialEventType.CantTrain;

								return;
							}

							var needExp = 15 * skill * skill;

							if (needExp > mTrainPlayer.Experience)
							{
								Talk("아직 경험치가 모자랍니다");
								mSpecialEvent = SpecialEventType.CantTrain;

								return;
							}

							mTrainPlayer.Experience -= needExp;
							mTrainPlayer.PotentialExperience += needExp;

							switch (mTrainSkillList[mMenuFocusID].Item1)
							{
								case 0:
									mTrainPlayer.SwordSkill++;
									break;
								case 1:
									mTrainPlayer.AxeSkill++;
									break;
								case 2:
									mTrainPlayer.SpearSkill++;
									break;
								case 3:
									mTrainPlayer.BowSkill++;
									break;
								case 4:
									mTrainPlayer.ShieldSkill++;
									break;
								default:
									mTrainPlayer.FistSkill++;
									break;
							}

							ShowTrainSkillMenu(mMenuFocusID);
						}
						else if (menuMode == MenuMode.ChooseTrainMagicMember)
						{
							mMenuMode = MenuMode.None;

							mTrainPlayer = mPlayerList[mMenuFocusID];

							if (mTrainPlayer.ClassType != ClassCategory.Magic)
							{
								AppendText(" 당신은 마법사 계열이 아닙니다.");
								return;
							}

							var readyToLevelUp = true;
							for (var i = 0; i < 6; i++)
							{
								int skill;
								switch (i)
								{
									case 0:
										skill = mTrainPlayer.AttackMagic;
										break;
									case 1:
										skill = mTrainPlayer.PhenoMagic;
										break;
									case 2:
										skill = mTrainPlayer.CureMagic;
										break;
									case 3:
										skill = mTrainPlayer.SpecialMagic;
										break;
									case 4:
										skill = mTrainPlayer.ESPMagic;
										break;
									default:
										skill = mTrainPlayer.SummonMagic;
										break;

								}

								if (magicEnableClass[mTrainPlayer.Class - 1, i] > 0)
								{
									if (skill < magicEnableClass[mTrainPlayer.Class - 1, i])
									{
										readyToLevelUp = false;
										break;
									}
								}
							}

							if (readyToLevelUp)
							{
								AppendText(" 당신은 모든 과정을 수료했으므로 모든 경험치를 레벨로 바꾸겠습니다.");

								mTrainPlayer.PotentialExperience += mTrainPlayer.Experience;
								mTrainPlayer.Experience = 0;

								return;
							}

							ShowTrainMagicMenu(0);
						}
						else if (menuMode == MenuMode.ChooseTrainMagic)
						{
							int skill;
							switch (mTrainSkillList[mMenuFocusID].Item1)
							{
								case 0:
									skill = mTrainPlayer.AttackMagic;
									break;
								case 1:
									skill = mTrainPlayer.PhenoMagic;
									break;
								case 2:
									skill = mTrainPlayer.CureMagic;
									break;
								case 3:
									skill = mTrainPlayer.SpecialMagic;
									break;
								case 4:
									skill = mTrainPlayer.ESPMagic;
									break;
								default:
									skill = mTrainPlayer.SummonMagic;
									break;
							}

							if (skill >= mTrainSkillList[mMenuFocusID].Item2)
							{
								Talk("이 분야는 더 배울 것이 없습니다");
								mSpecialEvent = SpecialEventType.CantTrainMagic;
							}
							else
							{

								var needExp = 15 * skill * skill;

								if (needExp > mTrainPlayer.Experience)
								{
									Talk("아직 경험치가 모자랍니다");
									mSpecialEvent = SpecialEventType.CantTrainMagic;
								}
								else
								{

									mTrainPlayer.Experience -= needExp;
									mTrainPlayer.PotentialExperience += needExp;

									switch (mTrainSkillList[mMenuFocusID].Item1)
									{
										case 0:
											mTrainPlayer.AttackMagic++;
											break;
										case 1:
											mTrainPlayer.PhenoMagic++;
											break;
										case 2:
											mTrainPlayer.CureMagic++;
											break;
										case 3:
											mTrainPlayer.SpecialMagic++;
											break;
										case 4:
											mTrainPlayer.ESPMagic++;
											break;
										default:
											mTrainPlayer.SummonMagic++;
											break;
									}

									ShowTrainMagicMenu(mMenuFocusID);
								}
							}
						}
						else if (menuMode == MenuMode.ConfirmExitMap)
						{
							if (mMenuFocusID == 0)
							{
								if (mParty.Map == 2)
								{
									mParty.Map = 1;
									mParty.XAxis = 82;
									mParty.YAxis = 85;

									await RefreshGame();
								}
								if (mParty.Map == 6)
								{
									mParty.Map = 1;
									mParty.XAxis = 19;
									mParty.YAxis = 11;

									await RefreshGame();
								}
								else if (mParty.Map == 7)
								{
									mParty.Map = 1;
									mParty.XAxis = 76;
									mParty.YAxis = 56;

									await RefreshGame();
								}
								else if (mParty.Map == 8)
								{
									mParty.Map = 3;

									if (mParty.XAxis < 5)
										mParty.XAxis = 64;
									else if (mParty.XAxis > 43)
										mParty.XAxis = 66;
									else
										mParty.XAxis = 65;

									if (mParty.YAxis < 6)
										mParty.YAxis = 76;
									else if (mParty.YAxis > 92)
										mParty.YAxis = 78;
									else
										mParty.YAxis = 77;

									await RefreshGame();
								}
								else if (mParty.Map == 9)
								{
									mParty.Map = 1;
									mParty.XAxis = 81;
									mParty.YAxis = 9;

									await RefreshGame();
								}
								else if (mParty.Map == 10)
								{
									mParty.Map = 1;
									mParty.XAxis = 17;
									mParty.YAxis = 88;

									await RefreshGame();
								}
								else if (mParty.Map == 11)
								{
									mParty.Map = 2;
									mParty.XAxis = 15;
									mParty.YAxis = 16;

									await RefreshGame();
								}
								else if (mParty.Map == 12)
								{
									mParty.Map = 2;
									mParty.XAxis = 148;
									mParty.YAxis = 65;

									await RefreshGame();
								}
								else if (mParty.Map == 13)
								{
									mParty.Map = 3;
									mParty.XAxis = 88;
									mParty.YAxis = 92;

									await RefreshGame();
								}
								else if (mParty.Map == 14)
								{
									mParty.Map = 3;
									mParty.XAxis = 32;
									mParty.YAxis = 49;

									await RefreshGame();

									if ((mParty.Etc[39] & (1 << 7)) == 0)
										mParty.Etc[39] |= 1 << 7;
									else
									{
										for (var y = 50; y < 67; y++)
											UpdateTileInfo(32, y, 0);
									}
								}
								else if (mParty.Map == 15)
								{
									mParty.Map = 3;
									mParty.XAxis = 35;
									mParty.YAxis = 16;

									await RefreshGame();

									if ((mParty.Etc[8] & (1 << 5)) > 0)
									{
										Talk(" 일행은 베리알을 처치하고 무사히 동굴을 빠져나왔다. 하지만 거대하게 환생한 도마뱀 인간은 집요하게 일행에게 따라붙었다.");

										mSpecialEvent = SpecialEventType.ChaseLizardMan;
									}
								}
								else if (mParty.Map == 16)
								{
									mParty.Map = 3;

									if ((mParty.Etc[8] & (1 << 6)) == 0)
									{
										mParty.XAxis = 92;
										mParty.YAxis = 6;
									}
									else
									{
										mParty.XAxis = 13;
										mParty.YAxis = 26;
									}

									await RefreshGame();
								}
								else if (mParty.Map == 17)
								{
									if (mParty.YAxis == 5)
									{
										mParty.Map = 4;
										mParty.XAxis = 8;
										mParty.YAxis = 21;
									}
									else
									{
										mParty.Map = 4;
										mParty.XAxis = 13;
										mParty.YAxis = 43;
									}

									await RefreshGame();
								}
							}
							else
							{
								ClearDialog();
								//if (mParty.Map == 9) {
								mParty.XAxis = mPrevX;
								mParty.YAxis = mPrevY;
								//}
								//else if (mParty.Map != 2)
								//	mParty.YAxis--;
							}
						}
						else if (menuMode == MenuMode.ChooseChangeSwordMember)
						{
							mTrainPlayer = mPlayerList[mMenuFocusID];

							mChangableClassList.Clear();
							mChangableClassIDList.Clear();

							var swordEnableClassMin = new int[,] {
								{  10,  10,  10,  10,  10,   0 },
								{  10,  10,   5,   0,  20,   0 },
								{  40,   0,   0,   0,   0,   0 },
								{   0,   5,   5,  40,   0,   0 },
								{   0,   0,   0,   0,   0,  40 },
								{  10,   0,   0,  10,   0,  20 },
								{  25,   0,   5,   0,  20,  10 }
							};

							for (var i = 0; i < swordEnableClassMin.GetLength(0); i++)
							{
								var changable = true;

								if (swordEnableClassMin[i, 0] > mTrainPlayer.SwordSkill)
									changable = false;

								if (swordEnableClassMin[i, 1] > mTrainPlayer.AxeSkill)
									changable = false;

								if (swordEnableClassMin[i, 2] > mTrainPlayer.SpearSkill)
									changable = false;

								if (swordEnableClassMin[i, 3] > mTrainPlayer.BowSkill)
									changable = false;

								if (swordEnableClassMin[i, 4] > mTrainPlayer.ShieldSkill)
									changable = false;

								if (swordEnableClassMin[i, 5] > mTrainPlayer.FistSkill)
									changable = false;

								if (changable)
								{
									mChangableClassIDList.Add(i + 1);
									mChangableClassList.Add(Common.SwordClass[i]);
								}
							}

							AppendText(new string[] {
								$"[color={RGB.LightRed}]당신이 바뀌고 싶은 계급을 고르시오.[/color]",
								$"[color={RGB.White}]비용 : 금 10,000 개[/color]"
							});

							ShowMenu(MenuMode.ChooseSwordJob, mChangableClassList.ToArray());
							return;
						}
						else if (menuMode == MenuMode.ChooseSwordJob)
						{
							if (mTrainPlayer.Class != mChangableClassIDList[mMenuFocusID])
							{
								mTrainPlayer.Class = mChangableClassIDList[mMenuFocusID];

								AppendText($"[color={RGB.LightGreen}]{mTrainPlayer.NameJosa} 이제 {mTrainPlayer.ClassStr} 계급이 되었다.");

								if (mTrainPlayer.Class < 7)
									mTrainPlayer.SP = 0;

								mParty.Gold -= 10_000;
								UpdateItem(mTrainPlayer);
								DisplaySP();
							}
						}
						else if (menuMode == MenuMode.ChooseChangeMagicMember)
						{
							mTrainPlayer = mPlayerList[mMenuFocusID];

							mChangableClassList.Clear();
							mChangableClassIDList.Clear();

							var magicEnableClassMin = new int[,] {
								{ 10, 10, 10,  0,  0,  0 },
								{  0, 10, 10,  0,  0, 10 },
								{  0,  0, 10,  0,  0, 10 },
								{ 40, 25, 25,  0,  0,  0 },
								{ 20, 20, 40,  0,  0, 40 },
								{ 10, 40, 30,  0,  0, 20 },
								{ 40, 40, 40, 20, 20, 20 }
							};

							for (var i = 0; i < magicEnableClassMin.GetLength(0); i++)
							{
								var changable = true;

								if (magicEnableClassMin[i, 0] > mTrainPlayer.AttackMagic)
									changable = false;

								if (magicEnableClassMin[i, 1] > mTrainPlayer.PhenoMagic)
									changable = false;

								if (magicEnableClassMin[i, 2] > mTrainPlayer.CureMagic)
									changable = false;

								if (magicEnableClassMin[i, 3] > mTrainPlayer.SpecialMagic)
									changable = false;

								if (magicEnableClassMin[i, 4] > mTrainPlayer.ESPMagic)
									changable = false;

								if (magicEnableClassMin[i, 5] > mTrainPlayer.SummonMagic)
									changable = false;

								if (changable)
								{
									mChangableClassIDList.Add(i + 1);
									mChangableClassList.Add(Common.MagicClass[i]);
								}
							}

							AppendText(new string[] {
								$"[color={RGB.LightRed}]당신이 바뀌고 싶은 계급을 고르시오.[/color]",
								$"[color={RGB.White}]비용 : 금 10,000 개[/color]"
							});

							ShowMenu(MenuMode.ChooseMagicJob, mChangableClassList.ToArray());
							return;
						}
						else if (menuMode == MenuMode.ChooseMagicJob)
						{
							if (mTrainPlayer.Class != mChangableClassIDList[mMenuFocusID])
							{
								mTrainPlayer.Class = mChangableClassIDList[mMenuFocusID];

								AppendText($"[color={RGB.LightGreen}]{mTrainPlayer.NameJosa} 이제 {mTrainPlayer.ClassStr} 계급이 되었다.");

								mParty.Gold -= 10_000;
								UpdateItem(mTrainPlayer);
							}
						}
						else if (menuMode == MenuMode.BattleStart)
						{
							var avgLuck = 0;
							foreach (var player in mPlayerList)
							{
								avgLuck += player.Luck;
							}

							var playerCount = mPlayerList.Count;
							if (mAssistPlayer != null)
							{
								avgLuck += mAssistPlayer.Luck;
								playerCount++;
							}

							avgLuck /= playerCount;

							var avgEnemyAgility = 0;
							foreach (var enemy in mEncounterEnemyList)
							{
								avgEnemyAgility += enemy.Agility;
							}
							avgEnemyAgility /= mEncounterEnemyList.Count;

							if (mMenuFocusID == 0)
							{
								var avgAgility = 0;
								foreach (var player in mPlayerList)
								{
									avgAgility += player.Agility;
								}

								if (mAssistPlayer != null)
									avgAgility = mAssistPlayer.Agility;

								avgAgility /= playerCount;

								if (avgAgility > avgEnemyAgility)
									StartBattle(true);
								else
									StartBattle(false);
							}
							else if (mMenuFocusID == 1)
							{
								if (avgLuck > avgEnemyAgility)
								{
									mBattleTurn = BattleTurn.RunAway;
									await EndBattle();
								}
								else
									StartBattle(false);
							}
						}
						else if (menuMode == MenuMode.BattleCommand)
						{
							mBattleCommandID = mMenuFocusID;

							if (mMenuFocusID == 0)
							{
								SelectEnemy();
							}
							else if (mMenuFocusID == 1)
							{
								ShowCastOneMagicMenu();
							}
							else if (mMenuFocusID == 2)
							{
								ShowCastAllMagicMenu();
							}
							else if (mMenuFocusID == 3)
							{
								ShowCastSpecialMenu();
							}
							else if (mMenuFocusID == 4)
							{
								ShowCureDestMenu(mPlayerList[mBattlePlayerID], MenuMode.ChooseBattleCureSpell);
							}
							else if (mMenuFocusID == 5)
							{
								ShowCastESPMenu();
							}
							else if (mMenuFocusID == 6)
							{
								ShowSummonMenu();
							}
							else if (mMenuFocusID == 7)
							{
								UseItem(mPlayerList[mBattlePlayerID], true);
							}
							else if (mMenuFocusID == 8)
							{
								if (mBattlePlayerID == 0)
								{
									mBattleTurn = BattleTurn.Player;
									mBattleCommandQueue.Clear();

									foreach (var player in mPlayerList)
									{
										if (player.IsAvailable)
										{
											var method = 0;
											var tool = 0;
											var enemyID = 0;

											if (player.ClassType == ClassCategory.Sword)
											{
												method = 0;
												tool = 0;
											}
											else if (player.ClassType == ClassCategory.Magic)
											{
												if (player.AttackMagic > 9 || player.SP > player.AttackMagic)
												{
													method = 1;
													tool = 1;
												}
												else
												{
													method = 0;
													tool = 0;
												}
											}

											mBattleCommandQueue.Enqueue(new BattleCommand()
											{
												Player = player,
												FriendID = 0,
												Method = method,
												Tool = tool,
												EnemyID = enemyID
											});
										}
									}

									AddAssistAttackCommand();

									DialogText.TextHighlighters.Clear();
									DialogText.Blocks.Clear();

									ExecuteBattle();
								}
								else
								{
									mBattleCommandID = mMenuFocusID;
									AddBattleCommand();
								}
							}

							return;
						}
						else if (menuMode == MenuMode.CastOneMagic)
						{
							mBattleToolID = mMenuFocusID + 1;

							SelectEnemy();
							return;
						}
						else if (menuMode == MenuMode.CastAllMagic)
						{
							mBattleToolID = mMenuFocusID + 1;
							mEnemyFocusID = -1;

							AddBattleCommand();
							return;
						}
						else if (menuMode == MenuMode.CastSpecial)
						{
							mBattleToolID = mMenuFocusID + 1;

							if (mMenuFocusID < 5)
								SelectEnemy();
							else
							{
								mEnemyFocusID = -1;
								AddBattleCommand();
							}

							return;
						}
						else if (menuMode == MenuMode.CastESP)
						{
							if (mMenuFocusID == 0)
							{
								mBattleToolID = mMenuFocusID;
								SelectEnemy();
							}
							else
							{
								var player = mPlayerList[mBattlePlayerID];

								var availCount = 0;
								if (player.ESPMagic > 19)
									availCount = 1;
								else if (player.ESPMagic > 29)
									availCount = 2;
								else if (player.ESPMagic > 79)
									availCount = 3;
								else if (player.ESPMagic > 89)
									availCount = 4;
								else if (player.ESPMagic > 99)
									availCount = 5;

								if (availCount == 0)
									BattleMode();
								else
								{
									var espMagicMenuItem = new string[availCount];

									for (var i = 1; i <= availCount; i++)
										espMagicMenuItem[i - 1] = Common.GetMagicName(5, i + 5);

									ShowMenu(MenuMode.ChooseESPMagic, espMagicMenuItem);
								}
							}

							return;
						}
						else if (menuMode == MenuMode.ChooseESPMagic)
						{
							mBattleToolID = mMenuFocusID + 1;

							AddBattleCommand();
						}
						else if (menuMode == MenuMode.CastESP)
						{
							mBattleToolID = mMenuFocusID + 1;
							mEnemyFocusID = -1;

							AddBattleCommand();
						}
						else if (menuMode == MenuMode.CastSummon)
						{
							mBattleToolID = mMenuFocusID + 1;
							mEnemyFocusID = -1;

							AddBattleCommand();
						}
						else if (menuMode == MenuMode.ApplyBattleCureSpell)
						{
							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							mCureResult.Clear();
							CureSpell(mMagicPlayer, mMagicWhomPlayer, mMenuFocusID, mCureResult);

							ShowCureResult(true);
						}
						else if (menuMode == MenuMode.ApplyBattleCureAllSpell)
						{
							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							mCureResult.Clear();
							CureAllSpell(mMagicPlayer, mMenuFocusID, mCureResult);

							ShowCureResult(true);
						}
						else if (menuMode == MenuMode.BattleLose)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
								ShowFileMenu(MenuMode.ChooseGameOverLoadGame);
							else
								CoreApplication.Exit();
						}
						else if (menuMode == MenuMode.AskEnter)
						{
							if (mMenuFocusID == 0)
							{
								switch (mTryEnterType)
								{
									case EnterType.CastleLore:
										if (mParty.Etc[9] == 16)
										{
											Talk(new string[] {
												" 당신이 서둘러 로어 성에 다다랐지만 이미 로어 성은 폐허가 되어 버렸다.",
												" 당신은 아찔한 감을 느끼며 알비레오의 예언에 굴복함을 느꼈다." +
												$" 이제는 다시 돌이킬 수가 없는 일이 일어나 버렸다. 우리는 [color={RGB.LightCyan}]다크 메이지 실리안 카미너스[/color]에게 완패한 것이었다."
											});

											mSpecialEvent = SpecialEventType.DestructCastleLore;
										}
										else
										{
											mParty.Map = 6;
											mParty.XAxis = 50;
											mParty.YAxis = 93;

											await RefreshGame();

											for (var x = 48; x < 53; x++)
												UpdateTileInfo(x, 87, 44);

											if ((mParty.Etc[49] & 1) > 0)
												UpdateTileInfo(88, 22, 44);

											if ((mParty.Etc[49] & (1 << 1)) > 0)
												UpdateTileInfo(8, 63, 44);

											if ((mParty.Etc[49] & (1 << 2)) > 0)
												UpdateTileInfo(20, 32, 44);

											if ((mParty.Etc[49] & (1 << 3)) > 0)
												UpdateTileInfo(87, 37, 44);

											if (mParty.Etc[9] == 4)
											{
												AppendText(" 당신이 로어 성에 들어오자 주민들의 열렬한 환영을 받았다." +
												" 하지만 주민들은 당신이 메너스에서 레드 안타레스가 악마 사냥꾼에게 거의 패배했던 당신을 도와준 사실을 알 턱이 없었다." +
												" 메너스의 살인 원인을 제거하는데 당신이 한 일은 사실 아무것도 없다는 점에 당신은 씁쓸한 웃음을 지을 수밖에 없었다.");
											}
										}

										break;
									case EnterType.CastleLastDitch:
										if (mParty.Etc[9] >= 16)
											AppendText(" 라스트디치 성은 용암의 대지로 변했다.");
										else
										{
											mParty.Map = 7;
											mParty.XAxis = 37;
											mParty.YAxis = 68;

											await RefreshGame();

											if ((mParty.Etc[49] & (1 << 7)) > 0)
												UpdateTileInfo(40, 17, 44);

											if ((mParty.Etc[50] & 1) == 0 && (mParty.Etc[31] & (1 << 5)) > 0 && (mParty.Etc[34] & (1 << 4)) > 0)
												UpdateTileInfo(53, 55, 53);
										}
										break;
									case EnterType.Menace:
										if (mParty.Etc[9] >= 16)
											AppendText(" 메너스는 형체도 알아볼 수 없을 정도로 무너져 버렸다.");
										else if (mParty.Etc[9] == 15)
										{
											mParty.Map = 4;
											mParty.XAxis = 9;
											mParty.YAxis = 91;

											await RefreshGame();

											mFace = -1;

											Talk("당신이 메너스에 입구에 들어서자마자 저항할 수 없는 강한 힘이 일행을 빨아들이기 시작했다." +
											" 순간 당신은 메너스의 입구 자체가 커다란 통로로 변해 있음을 알아챘다.");

											mSpecialEvent = SpecialEventType.EnterUnderworld;
										}
										else
										{
											mParty.Map = 10;
											mParty.XAxis = 25;
											mParty.YAxis = 43;

											await RefreshGame();

											switch (mParty.Etc[9])
											{
												case 1:
													for (var x = 24; x < 26; x++)
														UpdateTileInfo(x, 42, 52);
													break;
												case 3:
													for (var x = 24; x < 26; x++)
														UpdateTileInfo(x, 42, 52);
													break;
												case 9:
													for (var x = 23; x < 27; x++)
														UpdateTileInfo(x, 8, 0);
													for (var x = 22; x < 28; x++)
														UpdateTileInfo(x, 9, 0);
													for (var x = 22; x < 28; x++)
														UpdateTileInfo(x, 10, 0);
													for (var x = 23; x < 27; x++)
														UpdateTileInfo(x, 11, 0);
													break;
												case 12:
													for (var x = 11; x < 14; x++)
														UpdateTileInfo(x, 9, 0);
													for (var x = 10; x < 15; x++)
														UpdateTileInfo(x, 10, 0);
													for (var x = 11; x < 14; x++)
														UpdateTileInfo(x, 11, 0);
													break;
												case 15:
													break;
											}
										}
										break;
									case EnterType.UnknownPyramid:
										if (mParty.Etc[9] >= 16)
											AppendText(" 그러나, 피라미드는 파괴되었다.");
										else
										{
											mParty.Map = 2;
											mParty.XAxis = 98;
											mParty.YAxis = 99;

											await RefreshGame();
										}
										break;
									case EnterType.ProofOfInfortune:
										mParty.Map = 11;
										mParty.XAxis = 24;
										mParty.YAxis = 43;

										await RefreshGame();
										break;
									case EnterType.ClueOfInfortune:
										mParty.Map = 12;
										mParty.XAxis = 24;
										mParty.YAxis = 43;

										await RefreshGame();
										break;
									case EnterType.RoofOfLight:
										mParty.Map = 8;
										mParty.XAxis = 24;
										mParty.YAxis = 92;

										await RefreshGame();

										if ((mParty.Etc[39] & 1) > 0)
											UpdateTileInfo(24, 60, 48);
										break;
									case EnterType.TempleOfLight:
										mParty.Map = 13;
										mParty.XAxis = 24;
										mParty.YAxis = 6;

										await RefreshGame();
										break;
									case EnterType.SurvivalOfPerishment:
										mParty.Map = 14;
										mParty.XAxis = 24;
										mParty.YAxis = 43;

										await RefreshGame();

										if ((mParty.Etc[39] & (1 << 7)) == 0)
										{
											for (var y = 5; y < 17; y++)
											{
												for (var x = 16; x < 36; x++)
													UpdateTileInfo(x, y, 31);
											}
										}
										else
										{
											UpdateTileInfo(24, 40, 0);
											UpdateTileInfo(25, 40, 0);
										}
										break;
									case EnterType.CaveOfBerial:
										if ((mParty.Etc[8] & (1 << 5)) == 0)
										{
											Talk(" 일행이 동굴 입구에 다가서자 무언가 반짝이는 눈 같은 것이 보였다.");
											mSpecialEvent = SpecialEventType.BattleCaveOfBerialEntrance;
										}
										else
											AppendText(" 하지만 동굴의 입구는 막혀 있어서 들어갈 수가 없습니다.");
										break;
									case EnterType.CaveOfMolok:
										mParty.Map = 16;
										mParty.XAxis = 9;
										mParty.YAxis = 43;

										await RefreshGame();
										break;
									case EnterType.TeleportationGate1:
										mParty.XAxis = 12;
										mParty.YAxis = 71;

										AppendText(" 일행은 다른 곳으로 이동되었다.");
										break;
									case EnterType.TeleportationGate2:
										mParty.XAxis = 41;
										mParty.YAxis = 76;

										AppendText(" 일행은 다른 곳으로 이동되었다.");
										break;
									case EnterType.TeleportationGate3:
										mParty.XAxis = 12;
										mParty.YAxis = 71;

										AppendText(" 일행은 다른 곳으로 이동되었다.");
										break;
									case EnterType.CaveOfAsmodeus1:
										if (mParty.YAxis == 43 && (mParty.Etc[40] & (1 << 6)) == 0)
										{
											mEncounterEnemyList.Clear();
											JoinEnemy(63);
											JoinEnemy(64);
											DisplayEnemy();

											Talk(new string[] {
												$"[color={RGB.LightMagenta}] 너희들이 이 동굴에 들어가겠다고?[/color]",
												$"[color={RGB.LightMagenta}] 우리들은 아스모데우님의 경호를 맡고 있는 가디안 라이트와 가디안 레프트라고 한다." +
												" 소개는 이쯤에서 끝내도록하고 바로 대결을 하도록하지."
											});

											mSpecialEvent = SpecialEventType.BattleCaveOfAsmodeusEntrance;
										}
										else
										{
											mParty.Map = 17;
											mParty.XAxis = 24;
											mParty.YAxis = 43;

											await RefreshGame();

											mParty.Etc[42] = 0;
										}
										break;
									case EnterType.CaveOfAsmodeus2:
										mParty.Map = 17;
										mParty.XAxis = 24;
										mParty.YAxis = 6;

										await RefreshGame();

										mParty.Etc[42] = 0;
										break;
									case EnterType.FortressOfMephistopheles:
										mParty.Map = 18;
										mParty.XAxis = 24;
										mParty.YAxis = 43;

										await RefreshGame();

										mFace = 5;
										InvokeAnimation(AnimationType.EnterFortressOfMephistopheles);
										break;
									case EnterType.CabinOfRegulus:
										if (mParty.Etc[9] >= 16)
											AppendText(" 이미 오두막은 파괴된 후였다.");
										else
										{
											mParty.Map = 9;
											mParty.XAxis = 24;
											mParty.YAxis = 39;

											await RefreshGame();

											if ((mParty.Etc[32] != 0 || mParty.Etc[33] != 0) && (mParty.Etc[31] & (1 << 4)) == 0)
											{
												if (mParty.Year > mParty.Etc[32] || (mParty.Year == mParty.Etc[32] && mParty.Day >= mParty.Etc[33]))
												{
													if (mParty.Hour >= 12)
													{
														UpdateTileInfo(39, 12, 49);
														mParty.Etc[31] |= 1 << 4;
													}
												}
												else if ((mParty.Etc[31] & (1 << 4)) > 0)
												{
													if ((mParty.Etc[49] & (1 & 6)) == 0)
														UpdateTileInfo(39, 12, 49);
												}
											}
										}
										break;
								}
							}
							else
							{
								AppendText(new string[] { "" });

								if (mTryEnterType == EnterType.CabinOfRegulus)
								{
									mParty.XAxis = mPrevX;
									mParty.YAxis = mPrevY;
								}
							}
						}
						//else if (menuMode == MenuMode.SwapMember)
						//{
						//	mMenuMode = MenuMode.None;

						//	mPlayerList[mMenuFocusID + 1] = mReserveMember;
						//	mReserveMember = null;

						//	LeaveMemberPosition();

						//	DisplayPlayerInfo();

						//	AppendText(new string[] { "" });
						//}
						else if (menuMode == MenuMode.ChooseLoadGame || menuMode == MenuMode.ChooseGameOverLoadGame)
						{
							await LoadGame(mMenuFocusID);
						}
						else if (menuMode == MenuMode.ChooseSaveGame)
						{
							var saveData = new SaveData()
							{
								PlayerList = mPlayerList,
								AssistPlayer = mAssistPlayer,
								Party = mParty,
								Map = new Map()
								{
									Width = mMapWidth,
									Height = mMapHeight,
									Data = mMapLayer
								},
								Encounter = mEncounter,
								MaxEnemy = mMaxEnemy,
								SaveTime = DateTime.Now.Ticks
							};

							var saveJSON = JsonConvert.SerializeObject(saveData);

							var idStr = "";
							if (mMenuFocusID > 0)
								idStr = mMenuFocusID.ToString();

							var storageFolder = ApplicationData.Current.LocalFolder;
							var saveFile = await storageFolder.CreateFileAsync($"darkSave{idStr}.dat", CreationCollisionOption.ReplaceExisting);
							await FileIO.WriteTextAsync(saveFile, saveJSON);

							var users = await User.FindAllAsync();
							var gameSaveTask = await GameSaveProvider.GetForUserAsync(users[0], "00000000-0000-0000-0000-00007ede8c1b");

							if (gameSaveTask.Status == GameSaveErrorStatus.Ok)
							{
								var gameSaveProvider = gameSaveTask.Value;

								var gameSaveContainer = gameSaveProvider.CreateContainer("DarkSaveContainer");

								var buffer = Encoding.UTF8.GetBytes(saveJSON);

								var writer = new DataWriter();
								writer.WriteUInt32((uint)buffer.Length);
								writer.WriteBytes(buffer);
								var dataBuffer = writer.DetachBuffer();

								var blobsToWrite = new Dictionary<string, IBuffer>();
								blobsToWrite.Add($"darkSave{idStr}", dataBuffer);

								var gameSaveOperationResult = await gameSaveContainer.SubmitUpdatesAsync(blobsToWrite, null, "DarkSave");
								if (gameSaveOperationResult.Status == GameSaveErrorStatus.Ok)
									AppendText(new string[] { $"[color={RGB.LightRed}]현재의 게임을 저장합니다.[/color]" });
								else
									AppendText(new string[] {
										$"[color={RGB.LightRed}]현재의 게임을 기기에 저장했지만, 클라우드에 저장하지 못했습니다.[/color]",
										$"[color={RGB.LightRed}]에러 코드: {gameSaveOperationResult.Status}[/color]"
									});
							}
							else
							{
								AppendText(new string[] {
									$"[color={RGB.LightRed}]현재의 게임을 기기에 저장했지만, 클라우드에 연결할 수 없습니다.[/color]",
									$"[color={RGB.LightRed}]에러 코드: {gameSaveTask.Status}[/color]"
								});
							}
						}
						else if (menuMode == MenuMode.JoinMadJoe)
						{
							if (mMenuFocusID == 0)
							{
								Lore madJoe = new Lore()
								{
									Name = "미친 조",
									Gender = GenderType.Male,
									Class = 0,
									ClassType = ClassCategory.Unknown,
									Level = 2,
									Strength = 10,
									Mentality = 5,
									Concentration = 6,
									Endurance = 9,
									Resistance = 5,
									Agility = 7,
									Accuracy = 5,
									Luck = 20,
									Poison = 0,
									Unconscious = 0,
									Dead = 0,
									SP = 0,
									Experience = 0,
									Weapon = 0,
									Shield = 0,
									Armor = 0,
									PotentialAC = 1,
									SwordSkill = 5,
									AxeSkill = 5,
									SpearSkill = 0,
									BowSkill = 0,
									ShieldSkill = 0,
									FistSkill = 0
								};

								madJoe.HP = madJoe.Endurance * madJoe.Level * 10;
								madJoe.UpdatePotentialExperience();
								UpdateItem(madJoe);

								mAssistPlayer = madJoe;

								UpdateTileInfo(39, 14, 47);

								mParty.Etc[49] |= 1 << 4;

								DisplayPlayerInfo();
								AppendText("");
							}
						}
						else if (menuMode == MenuMode.JoinMercury)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									Lore mercury = new Lore()
									{
										Name = "머큐리",
										Gender = GenderType.Male,
										Class = 6,
										ClassType = ClassCategory.Sword,
										Level = 2,
										Strength = 12,
										Mentality = 5,
										Concentration = 6,
										Endurance = 11,
										Resistance = 18,
										Agility = 19,
										Accuracy = 16,
										Luck = 19,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 0,
										Shield = 0,
										Armor = 0,
										PotentialAC = 2,
										SwordSkill = 10,
										AxeSkill = 0,
										SpearSkill = 0,
										BowSkill = 20,
										ShieldSkill = 0,
										FistSkill = 20
									};

									mercury.HP = mercury.Endurance * mercury.Level * 10;
									mercury.UpdatePotentialExperience();
									UpdateItem(mercury);

									mPlayerList.Add(mercury);
									UpdateTileInfo(62, 9, 47);

									mParty.Etc[49] |= 1 << 5;

									DisplayPlayerInfo();

									AppendText(" 고맙소. 그리고 병사들에게 들키지 않게 여기를 나가야 된다는 것 정도는 알고 있겠지요.");
								}
								else
								{
									AppendText(" 벌써 사람이 모두 채워져 있군요. 다음 기회를 기다리지요.");
								}
							}
						}
						else if (menuMode == MenuMode.JoinHercules)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									Lore hercules = new Lore()
									{
										Name = "헤라클레스",
										Gender = GenderType.Male,
										Class = 2,
										ClassType = ClassCategory.Sword,
										Level = 2,
										Strength = 18,
										Mentality = 5,
										Concentration = 6,
										Endurance = 15,
										Resistance = 12,
										Agility = 14,
										Accuracy = 14,
										Luck = 12,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 3,
										Shield = 1,
										Armor = 1,
										PotentialAC = 2,
										SwordSkill = 10,
										AxeSkill = 10,
										SpearSkill = 5,
										BowSkill = 0,
										ShieldSkill = 20,
										FistSkill = 5
									};

									hercules.HP = hercules.Endurance * hercules.Level * 10;
									hercules.UpdatePotentialExperience();
									UpdateItem(hercules);

									mPlayerList.Add(hercules);
									UpdateTileInfo(88, 22, 44);

									mParty.Etc[49] |= 1;

									DisplayPlayerInfo();
									AppendText("");
								}
								else
									AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
							}
						}
						else if (menuMode == MenuMode.JoinTitan)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									Lore titan = new Lore()
									{
										Name = "타이탄",
										Gender = GenderType.Male,
										Class = 1,
										ClassType = ClassCategory.Sword,
										Level = 2,
										Strength = 19,
										Mentality = 3,
										Concentration = 4,
										Endurance = 17,
										Resistance = 10,
										Agility = 13,
										Accuracy = 11,
										Luck = 14,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 2,
										Shield = 0,
										Armor = 2,
										PotentialAC = 2,
										SwordSkill = 10,
										AxeSkill = 10,
										SpearSkill = 10,
										BowSkill = 10,
										ShieldSkill = 10,
										FistSkill = 10
									};

									titan.HP = titan.Endurance * titan.Level * 10;
									titan.UpdatePotentialExperience();
									UpdateItem(titan);

									mPlayerList.Add(titan);
									UpdateTileInfo(20, 32, 44);

									mParty.Etc[49] |= 1 << 2;

									DisplayPlayerInfo();
									AppendText("");
								}
								else
									AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
							}
						}
						else if (menuMode == MenuMode.JoinMerlin)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									Lore merlin = new Lore()
									{
										Name = "머린",
										Gender = GenderType.Male,
										Class = 1,
										ClassType = ClassCategory.Magic,
										Level = 2,
										Strength = 5,
										Mentality = 15,
										Concentration = 16,
										Endurance = 10,
										Resistance = 14,
										Agility = 8,
										Accuracy = 13,
										Luck = 17,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										Experience = 0,
										Weapon = 0,
										Shield = 0,
										Armor = 1,
										PotentialAC = 0,
										AttackMagic = 20,
										PhenoMagic = 10,
										CureMagic = 10,
										SpecialMagic = 0,
										ESPMagic = 10,
										SummonMagic = 0
									};

									merlin.HP = merlin.Endurance * merlin.Level * 10;
									merlin.SP = merlin.Mentality * merlin.Level * 10;
									merlin.UpdatePotentialExperience();
									UpdateItem(merlin);

									mPlayerList.Add(merlin);
									UpdateTileInfo(8, 63, 44);

									mParty.Etc[49] |= 1 << 1;

									DisplayPlayerInfo();
									AppendText("");
								}
								else
									AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
							}
						}
						else if (menuMode == MenuMode.JoinBetelgeuse)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									Lore betelgeuse = new Lore()
									{
										Name = "베텔규스",
										Gender = GenderType.Male,
										Class = 2,
										ClassType = ClassCategory.Magic,
										Level = 2,
										Strength = 7,
										Mentality = 17,
										Concentration = 15,
										Endurance = 8,
										Resistance = 12,
										Agility = 10,
										Accuracy = 15,
										Luck = 10,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										Experience = 0,
										Weapon = 0,
										Shield = 0,
										Armor = 1,
										PotentialAC = 0,
										AttackMagic = 10,
										PhenoMagic = 20,
										CureMagic = 10,
										SpecialMagic = 0,
										ESPMagic = 0,
										SummonMagic = 10
									};

									betelgeuse.HP = betelgeuse.Endurance * betelgeuse.Level * 10;
									betelgeuse.SP = betelgeuse.Mentality * betelgeuse.Level * 10;
									betelgeuse.UpdatePotentialExperience();
									UpdateItem(betelgeuse);

									mPlayerList.Add(betelgeuse);
									UpdateTileInfo(87, 37, 44);

									mParty.Etc[49] |= 1 << 3;

									DisplayPlayerInfo();
									AppendText("");
								}
								else
									AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
							}
						}
						else if (menuMode == MenuMode.JoinPolaris)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									Lore polaris = new Lore()
									{
										Name = "폴라리스",
										Gender = GenderType.Male,
										Class = 7,
										ClassType = ClassCategory.Sword,
										Level = 4,
										Strength = 18,
										Mentality = 10,
										Concentration = 6,
										Endurance = 12,
										Resistance = 16,
										Agility = 14,
										Accuracy = 17,
										Luck = 12,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 4,
										Shield = 3,
										Armor = 4,
										PotentialAC = 2,
										SwordSkill = 25,
										AxeSkill = 10,
										SpearSkill = 5,
										BowSkill = 0,
										ShieldSkill = 25,
										FistSkill = 10
									};

									polaris.HP = polaris.Endurance * polaris.Level * 10;
									polaris.UpdatePotentialExperience();
									UpdateItem(polaris);

									mPlayerList.Add(polaris);
									UpdateTileInfo(40, 17, 44);

									mParty.Etc[49] |= 1 << 7;

									DisplayPlayerInfo();
								}
								else
									AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
							}
						}
						else if (menuMode == MenuMode.JoinGeniusKie)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									Lore geniusKie = new Lore()
									{
										Name = "지니어스 기",
										Gender = GenderType.Male,
										Class = 7,
										ClassType = ClassCategory.Sword,
										Level = 10,
										Strength = 19,
										Mentality = 15,
										Concentration = 10,
										Endurance = 14,
										Resistance = 18,
										Agility = 16,
										Accuracy = 18,
										Luck = 17,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										Experience = 0,
										Weapon = 19,
										Shield = 4,
										Armor = 6,
										PotentialAC = 3,
										SwordSkill = 25,
										AxeSkill = 30,
										SpearSkill = 30,
										BowSkill = 10,
										ShieldSkill = 40,
										FistSkill = 15
									};

									geniusKie.HP = geniusKie.Endurance * geniusKie.Level * 10;
									geniusKie.SP = geniusKie.Mentality * geniusKie.Level * 5;
									geniusKie.UpdatePotentialExperience();
									UpdateItem(geniusKie);

									mPlayerList.Add(geniusKie);
									UpdateTileInfo(53, 55, 44);

									mParty.Etc[50] |= 1;

									DisplayPlayerInfo();
								}
								else
									AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
							}
						}
						else if (menuMode == MenuMode.BattleChooseItem || menuMode == MenuMode.ChooseItem)
						{
							void ShowRemainItemCount()
							{
								string itemName;
								if (mUseItemID < 5)
									itemName = mMedicines[mUseItemID];
								else
									itemName = mItems[mUseItemID - 4];


								var message = $"{Common.AddItemJosa(itemName)} 이제 {mParty.Item[mUseItemID]}개 남았습니다.";
								if (menuMode == MenuMode.BattleChooseItem)
									Talk(message, true);
								else
									Dialog(message, true);
							}

							var itemID = mUsableItemIDList[mMenuFocusID];
							mUseItemID = itemID;

							mParty.Item[itemID]--;

							if (itemID == 0)
							{
								mItemUsePlayer.HP += 1_000;
								if (mItemUsePlayer.HP >= mItemUsePlayer.Endurance * mItemUsePlayer.Level * 10)
								{
									mItemUsePlayer.HP = mItemUsePlayer.Endurance * mItemUsePlayer.Level * 10;
									ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 모든 건강이 회복 되었습니다.[/color]");
								}
								else
									ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 건강이 회복 되었습니다.[/color]");

								DisplayHP();
								ShowRemainItemCount();

							}
							else if (itemID == 1)
							{
								if (mItemUsePlayer.ClassType != ClassCategory.Magic && mItemUsePlayer.Class != 7)
									ShowApplyItemResult(menuMode, $" {mItemUsePlayer.NameSubjectJosa} 마법사 계열이 아닙니다.");
								else
								{
									if (mItemUsePlayer.ClassType == ClassCategory.Magic)
									{
										mItemUsePlayer.SP += 1_000;
										if (mItemUsePlayer.SP > mItemUsePlayer.Mentality * mItemUsePlayer.Level * 10)
										{
											mItemUsePlayer.SP = mItemUsePlayer.Mentality * mItemUsePlayer.Level * 10;
											ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 모든 마법 지수가 회복 되었습니다.[/color]");
										}
										else
											ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 마법 지수가 회복 되었습니다.[/color]");
									}
									else
									{
										mItemUsePlayer.SP += 1_000;
										if (mItemUsePlayer.SP > mItemUsePlayer.Mentality * mItemUsePlayer.Level * 5)
										{
											mItemUsePlayer.SP = mItemUsePlayer.Mentality * mItemUsePlayer.Level * 5;
											ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 모든 마법 지수가 회복 되었습니다.[/color]");
										}
										else
											ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 마법 지수가 회복 되었습니다.[/color]");
									}

									DisplaySP();
								}
								ShowRemainItemCount();
							}
							else if (itemID == 2)
							{
								if (mItemUsePlayer.Poison == 0)
									ShowApplyItemResult(menuMode, $" {mItemUsePlayer.NameSubjectJosa} 중독 되지 않았습니다.");
								else
								{
									mItemUsePlayer.Poison = 0;
									ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.NameSubjectJosa} 해독 되었습니다.[/color]");
								}

								DisplayCondition();
								ShowRemainItemCount();
							}
							else if (itemID == 3 || itemID == 4)
							{
								Dialog("아이템을 적용받을 대상을 고르시오.");

								if (menuMode == MenuMode.BattleChooseItem)
									ShowCharacterMenu(MenuMode.BattleUseItemWhom);
								else
									ShowCharacterMenu(MenuMode.UseItemWhom);
							}
							else if (itemID == 5)
							{
								int GetBonusPoint(int seed)
								{
									return mRand.Next(seed * 2 + 1) - seed;
								}

								var ability = 0;
								var livePlayerCount = 0;
								foreach (var player in mPlayerList)
								{
									if (player.IsAvailable)
									{
										livePlayerCount++;
										ability += player.Level;
									}
								}

								ability = (int)Math.Round((double)ability * 5 / livePlayerCount);

								var summonPlayer = new Lore();
								switch (mRand.Next(8))
								{
									case 0:
										summonPlayer.Name = "밴더스내치";
										summonPlayer.Endurance = 15 + GetBonusPoint(5);
										summonPlayer.Resistance = 8 + GetBonusPoint(5);
										summonPlayer.Accuracy = 12 + GetBonusPoint(5);
										summonPlayer.Weapon = 33;
										summonPlayer.WeaPower = ability * 3;
										summonPlayer.PotentialAC = 3;
										summonPlayer.AC = 3;
										break;
									case 1:
										summonPlayer.Name = "캐리온 크롤러";
										summonPlayer.Endurance = 20 + GetBonusPoint(5);
										summonPlayer.Resistance = 14 + GetBonusPoint(5);
										summonPlayer.Accuracy = 13 + GetBonusPoint(5);
										summonPlayer.Weapon = 34;
										summonPlayer.WeaPower = ability;
										summonPlayer.PotentialAC = 3;
										summonPlayer.AC = 3;
										break;
									case 2:
										summonPlayer.Name = "켄타우루스";
										summonPlayer.Endurance = 17 + GetBonusPoint(5);
										summonPlayer.Resistance = 12 + GetBonusPoint(5);
										summonPlayer.Accuracy = 18 + GetBonusPoint(5);
										summonPlayer.Weapon = 35;
										summonPlayer.WeaPower = (int)Math.Round((double)ability * 1.5);
										summonPlayer.PotentialAC = 2;
										summonPlayer.AC = 2;
										break;
									case 3:
										summonPlayer.Name = "데모고르곤'";
										summonPlayer.Endurance = 18 + GetBonusPoint(5);
										summonPlayer.Resistance = 5 + GetBonusPoint(5);
										summonPlayer.Accuracy = 17 + GetBonusPoint(5);
										summonPlayer.Weapon = 36;
										summonPlayer.WeaPower = ability * 4;
										summonPlayer.PotentialAC = 4;
										summonPlayer.AC = 4;
										break;
									case 4:
										summonPlayer.Name = "듈라한";
										summonPlayer.Endurance = 10 + GetBonusPoint(5);
										summonPlayer.Resistance = 20;
										summonPlayer.Accuracy = 17;
										summonPlayer.Weapon = 16;
										summonPlayer.WeaPower = ability;
										summonPlayer.PotentialAC = 3;
										summonPlayer.AC = 3;
										break;
									case 5:
										summonPlayer.Name = "에틴";
										summonPlayer.Endurance = 10 + GetBonusPoint(5);
										summonPlayer.Resistance = 20;
										summonPlayer.Accuracy = 10 + GetBonusPoint(9);
										summonPlayer.Weapon = 8;
										summonPlayer.WeaPower = (int)Math.Round((double)ability * 0.8);
										summonPlayer.PotentialAC = 1;
										summonPlayer.AC = 1;
										break;
									case 6:
										summonPlayer.Name = "헬하운드";
										summonPlayer.Endurance = 14 + GetBonusPoint(5);
										summonPlayer.Resistance = 9 + GetBonusPoint(5);
										summonPlayer.Accuracy = 11 + GetBonusPoint(5);
										summonPlayer.Weapon = 33;
										summonPlayer.WeaPower = ability * 3;
										summonPlayer.PotentialAC = 2;
										summonPlayer.AC = 2;
										break;
									case 7:
										summonPlayer.Name = "미노타우루스";
										summonPlayer.Endurance = 13 + GetBonusPoint(5);
										summonPlayer.Resistance = 11 + GetBonusPoint(5);
										summonPlayer.Accuracy = 14 + GetBonusPoint(5);
										summonPlayer.Weapon = 9;
										summonPlayer.WeaPower = ability * 3;
										summonPlayer.PotentialAC = 2;
										summonPlayer.AC = 2;
										break;
								}
								summonPlayer.Gender = GenderType.Neutral;
								summonPlayer.Class = 0;
								summonPlayer.ClassType = ClassCategory.Unknown;
								summonPlayer.Level = ability / 5;
								summonPlayer.Strength = 10 + GetBonusPoint(5);
								summonPlayer.Mentality = 10 + GetBonusPoint(5);
								summonPlayer.Concentration = 10 + GetBonusPoint(5);
								summonPlayer.Agility = 0;
								summonPlayer.Luck = 10 + GetBonusPoint(5);
								summonPlayer.Poison = 0;
								summonPlayer.Unconscious = 0;
								summonPlayer.Dead = 0;
								summonPlayer.HP = summonPlayer.Endurance * summonPlayer.Level * 10;
								summonPlayer.SP = 0;
								summonPlayer.Experience = 0;
								summonPlayer.PotentialExperience = 0;
								summonPlayer.Shield = 0;
								summonPlayer.ShiPower = 0;
								summonPlayer.Armor = 0;
								summonPlayer.SwordSkill = 0;
								summonPlayer.AxeSkill = 0;
								summonPlayer.SpearSkill = 0;
								summonPlayer.BowSkill = 0;
								summonPlayer.ShieldSkill = 0;
								summonPlayer.FistSkill = 0;

								mAssistPlayer = summonPlayer;

								DisplayPlayerInfo();

								ShowApplyItemResult(menuMode, $" [color={RGB.White}]{summonPlayer.NameSubjectJosa} 다른 차원으로부터 소환됐습니다.[/color]");
								ShowRemainItemCount();
							}
							else if (itemID == 6)
							{
								if (mParty.Etc[0] + 10 > 255)
									mParty.Etc[0] = 255;
								else
									mParty.Etc[0] = 255;

								UpdateView();

								ShowApplyItemResult(menuMode, $"[color={RGB.White}] 일행은 대형 횃불을 켰습니다.[/color]");
								ShowRemainItemCount();
							}
							else if (itemID == 7)
								ShowWizardEye();
							else if (itemID == 8)
							{
								mParty.Etc[1] = 255;
								mParty.Etc[2] = 255;
								mParty.Etc[3] = 255;

								ShowApplyItemResult(menuMode, $"[color={RGB.White}] 일행은 모두 비행 부츠를 신었습니다.[/color]");
								ShowRemainItemCount();
							}
							else if (itemID == 9)
							{
								Teleport(MenuMode.TeleportationDirection);
							}
						}
						else if (menuMode == MenuMode.BattleUseItemWhom || menuMode == MenuMode.UseItemWhom)
						{
							Lore player;

							if (mMenuFocusID < mPlayerList.Count)
								player = mPlayerList[mMenuFocusID];
							else
								player = mAssistPlayer;

							if (mUseItemID == 3)
							{
								if (player.Unconscious == 0)
								{
									ShowApplyItemResult(menuMode, $" {player.NameSubjectJosa} 의식이 있습니다.");
									return;
								}

								player.Unconscious = 0;
								if (player.Dead == 0)
								{
									if (player.HP <= 0)
										player.HP = 1;

									ShowApplyItemResult(menuMode, $"[color={RGB.White}] {player.NameSubjectJosa} 의식을 차렸습니다.[/color]");
								}
								else
									ShowApplyItemResult(menuMode, $" {player.NameSubjectJosa} 이미 죽은 상태입니다.");
							}
							else if (mUseItemID == 4)
							{
								if (player.Dead == 0)
								{
									ShowApplyItemResult(menuMode, $" {player.NameSubjectJosa} 죽지 않았습니다.");
									return;
								}

								if (player.Dead < 10_000)
								{
									player.Dead = 0;
									if (player.Unconscious >= player.Endurance * player.Level)
										player.Unconscious = player.Endurance * player.Level - 1;

									ShowApplyItemResult(menuMode, $"[color={RGB.White}] {player.NameSubjectJosa} 다시 살아났습니다.[/color]");
								}
								else
									ShowApplyItemResult(menuMode, $" {player.Name}의 죽음은 이 약초로는 살리지 못합니다.");
							}
						}
						else if (menuMode == MenuMode.ChooseEquipCromaticShield)
						{
							var player = mPlayerList[mMenuFocusID];
							if (VerifyShield(player, 4))
							{
								player.Shield = 4;
								UpdateItem(player);

								AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 크로매틱 방패를 장착했다.[/color]");
								mParty.Etc[48] |= 1 << 7;
							}
							else
								AppendText($"{player.Name}에게는 이 방패가 맞지 않습니다.");
						}
						else if (menuMode == MenuMode.ChooseEquipBattleAxe)
						{
							var player = mPlayerList[mMenuFocusID];
							if (VerifyWeapon(player, 13))
							{
								player.Weapon = 13;
								UpdateItem(player);

								AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 양날 전투 도끼를 장착했다.[/color]");
								mParty.Etc[44] |= 1;
							}
							else
								AppendText($"{player.Name}에게는 이 무기가 맞지 않습니다.");
						}
						else if (menuMode == MenuMode.ChooseEquipObsidianArmor)
						{
							var player = mPlayerList[mMenuFocusID];
							if (VerifyWeapon(player, 255))
							{
								player.Weapon = 255;
								UpdateItem(player);

								AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 흑요석 갑옷을 장착했다.[/color]");
								mParty.Etc[44] |= 1 << 2;
							}
							else
								AppendText($"{player.Name}에게는 이 갑옷이 맞지 않습니다.");
						}
						else if (menuMode == MenuMode.AstronomyQuiz1)
						{
							if (mMenuFocusID != 2)
								mPyramidQuizAllRight = false;

							AppendText(" 현재 알려진 항성 중에서 가장 밝은 항성은 무엇이라고 알려져 있소?");

							ShowMenu(MenuMode.AstronomyQuiz2, new string[] {
								"폴라리스 ( 북극성 )",
								"시리우스",
								"스피카",
								"베텔규스"
							});
						}
						else if (menuMode == MenuMode.AstronomyQuiz2)
						{
							if (mMenuFocusID != 1)
								mPyramidQuizAllRight = false;

							AppendText(" 금빛과 푸른빛의 이중성으로 알려진 아름다운 별의 이름은 무엇이겠소?");

							ShowMenu(MenuMode.AstronomyQuiz3, new string[] {
								"시리우스",
								"알콜과 미자르",
								"알비레오",
								"베라트릭스"
							});
						}
						else if (menuMode == MenuMode.AstronomyQuiz3)
						{
							if (mMenuFocusID != 2)
								mPyramidQuizAllRight = false;

							AppendText(" 다음 중 오리온자리의 별이 아닌 것을 골라보도록 하시오.");

							ShowMenu(MenuMode.AstronomyQuiz4, new string[] {
								"베텔규스",
								"리겔",
								"베라트릭스",
								"안타레스"
							});
						}
						else if (menuMode == MenuMode.AstronomyQuiz4)
						{
							if (mMenuFocusID != 3)
								mPyramidQuizAllRight = false;

							if (mPyramidQuizAllRight)
							{
								Dialog(new string[] {
									$"[color={RGB.White}] 당신은 정말 열심히 \"또 다른 지식의 성전\"을 했나 보군요. 내가 낸 모든 문제는 바로 거기에서 나오는 문제들이었다오.[/color]",
									$"[color={RGB.White}] 당신은 이 시험을 통과했소.[/color]",
									"",
									$"[color={RGB.LightCyan}] [[ 경험치 + 50,000 ][/color]"
								});

								foreach (var player in mPlayerList)
									player.Experience += 50_000;

								mParty.Etc[45] |= 1;
							}
							else
								Dialog($"[color={RGB.White}] 당신은 모든 문제를 모두 맞추지 못했소.[/color]");
						}
						else if (menuMode == MenuMode.PlusMentality)
						{
							var player = mPlayerList[mMenuFocusID];
							if (player.Mentality >= 30)
								AppendText($"{player.NameSubjectJosa} 지금 최고의 정신력 수치입니다.");
							else if (mParty.Gold < abilityUpPrice[player.Mentality])
								AppendText($"일행은 {abilityUpPrice[player.Mentality]:#,#0}개의 황금이 필요합니다.");
							else
							{
								player.Mentality++;
								mParty.Gold -= abilityUpPrice[player.Mentality];
								AppendText($"[color={RGB.White}]{player.Name}의 정신력은 이제 {player.Mentality}입니다.");
							}
						}
						else if (menuMode == MenuMode.PlusEndurance)
						{
							var player = mPlayerList[mMenuFocusID];
							if (player.Endurance >= 30)
								AppendText($"{player.NameSubjectJosa} 지금 최고의 인내력 수치입니다.");
							else if (mParty.Gold < abilityUpPrice[player.Endurance])
								AppendText($"일행은 {abilityUpPrice[player.Endurance]:#,#0}개의 황금이 필요합니다.");
							else
							{
								player.Endurance++;
								mParty.Gold -= abilityUpPrice[player.Endurance];
								AppendText($"[color={RGB.White}]{player.Name}의 인내력은 이제 {player.Endurance}입니다.");
							}
						}
						else if (menuMode == MenuMode.PlusStrength)
						{
							var player = mPlayerList[mMenuFocusID];
							if (player.Strength >= 30)
								AppendText($"{player.NameSubjectJosa} 지금 최고의 체력 수치입니다.");
							else if (mParty.Gold < abilityUpPrice[player.Strength])
								AppendText($"일행은 {abilityUpPrice[player.Strength]:#,#0}개의 황금이 필요합니다.");
							else
							{
								player.Strength++;
								mParty.Gold -= abilityUpPrice[player.Strength];
								AppendText($"[color={RGB.White}]{player.Name}의 체력은 이제 {player.Strength}입니다.");
							}
						}
						else if (menuMode == MenuMode.PlusAgility)
						{
							var player = mPlayerList[mMenuFocusID];
							if (player.Agility >= 20)
								AppendText($"{player.NameSubjectJosa} 지금 최고의 민첩성 수치입니다.");
							else if (mParty.Gold < abilityUpPrice[player.Agility])
								AppendText($"일행은 {abilityUpPrice[player.Agility]:#,#0}개의 황금이 필요합니다.");
							else
							{
								player.Agility++;
								mParty.Gold -= abilityUpPrice[player.Agility];
								AppendText($"[color={RGB.White}]{player.Name}의 민첩성은 이제 {player.Agility}입니다.");
							}
						}
						else if (menuMode == MenuMode.PlusAccuracy)
						{
							var player = mPlayerList[mMenuFocusID];
							if (player.Accuracy >= 20)
								AppendText($"{player.NameSubjectJosa} 지금 최고의 정확성 수치입니다.");
							else if (mParty.Gold < abilityUpPrice[player.Accuracy])
								AppendText($"일행은 {abilityUpPrice[player.Accuracy]:#,#0}개의 황금이 필요합니다.");
							else
							{
								player.Accuracy++;
								mParty.Gold -= abilityUpPrice[player.Accuracy + 1];
								AppendText($"[color={RGB.White}]{player.Name}의 정확성은 이제 {player.Accuracy}입니다.");
							}
						}
						else if (menuMode == MenuMode.ComputerQuiz1)
						{
							if (mMenuFocusID != 3)
								mPyramidQuizAllRight = false;

							AppendText(" 다음은 여러 OS간의 명령어라오. 이 중에서 성질이 다른 하나를 찾아보도록 하시오.");

							ShowMenu(MenuMode.ComputerQuiz2, new string[] {
								"Pro - Dos : \"prefix\"",
								"Unix      : \"ls\"",
								"MS - Dos  : \"dir\"",
								"Apple Dos : \"catalog\""
							});
						}
						else if (menuMode == MenuMode.ComputerQuiz2)
						{
							if (mMenuFocusID != 0)
								mPyramidQuizAllRight = false;

							AppendText(" 그럼 이 게임을 만들 때 쓰인 언어 중에서 주가 되는 언어는 무엇이었겠소?");

							ShowMenu(MenuMode.ComputerQuiz3, new string[] {
								"FORTRAN",
								"BASIC",
								"Pascal",
								"C ++"
							});
						}
						else if (menuMode == MenuMode.ComputerQuiz3)
						{
							if (mMenuFocusID != 2)
								mPyramidQuizAllRight = false;

							AppendText(" 다음은 여러 컴퓨터 언어의 출력 함수라오. 그중 결과가 다른 것은 무엇이겠소?");

							ShowMenu(MenuMode.ComputerQuiz4, new string[] {
								"FORTRAN : WRITE(*,*) \'Dark.exe\'",
								"BASIC   : PRINT \"Dark.exe\";",
								"Pascal  : writeln(\'Dark.exe\');",
								"C       : puts(\"Dark.exe\");"
							});
						}
						else if (menuMode == MenuMode.ComputerQuiz4)
						{
							if (mMenuFocusID != 1)
								mPyramidQuizAllRight = false;

							if (mPyramidQuizAllRight)
							{
								AppendText(new string[] {
									$"[color={RGB.White}] 당신은 정말 열심히 컴퓨터를 공부했나 보군요.[/color]",
									$"[color={RGB.White}] 당신은 이 시험을 통과했소.[/color]",
									"",
									$"[color={RGB.LightCyan}] [[ 경험치 + 50,000 ][/color]"
								});

								foreach (var player in mPlayerList)
									player.Experience += 50_000;

								mParty.Etc[45] |= 1 << 1;
							}
							else
								AppendText($"[color={RGB.White}] 당신은 모든 문제를 모두 맞추지 못했소.[/color]");
						}
						else if (menuMode == MenuMode.PhysicsQuiz1)
						{
							if (mMenuFocusID != 2)
								mPyramidQuizAllRight = false;

							AppendText(" 다음 중 뉴턴의 세 가지 법칙에 속하지 않는 것은 어느 것이겠소?");

							ShowMenu(MenuMode.PhysicsQuiz2, new string[] {
								"만유 인력의 법칙",
								"관성의 법칙",
								"작용 반작용의 법칙",
								"질량 보존의 법칙"
							});
						}
						else if (menuMode == MenuMode.PhysicsQuiz2)
						{
							if (mMenuFocusID != 3)
								mPyramidQuizAllRight = false;

							AppendText(" 다음 공식 중 틀린 것을 한번 골라내 보도록 하시오.");

							ShowMenu(MenuMode.PhysicsQuiz3, new string[] {
								"F = ma",
								"F = Blv",
								"F = mg",
								"F = Bli"
							});
						}
						else if (menuMode == MenuMode.PhysicsQuiz3)
						{
							if (mMenuFocusID != 1)
								mPyramidQuizAllRight = false;

							if (mPyramidQuizAllRight)
							{
								AppendText(new string[] {
									$"[color={RGB.White}] 당신은 물리학에도 역시 상당한 지식을 가지고 있군요.[/color]",
									$"[color={RGB.White}] 당신은 이 시험을 통과했소.[/color]",
									"",
									$"[color={RGB.LightCyan}] [[ 경험치 + 50,000 ][/color]"
								});

								foreach (var player in mPlayerList)
									player.Experience += 50_000;

								mParty.Etc[45] |= 1 << 2;
							}
							else
								AppendText($"[color={RGB.White}] 당신은 모든 문제를 모두 맞추지 못했소.[/color]");
						}
						else if (menuMode == MenuMode.CommonSenseQuiz1)
						{
							if (mMenuFocusID != 3)
								mPyramidQuizAllRight = false;

							AppendText(" 전작에서 드래곤이 있던 동굴에서 수도하던 최고의 초능력자는 누구였소?");

							ShowMenu(MenuMode.CommonSenseQuiz2, new string[] {
								"레드 안타레스",
								"스피카",
								"알비레오",
								"드라코니안",
								"로어 헌터"
							});
						}
						else if (menuMode == MenuMode.CommonSenseQuiz2)
						{
							if (mMenuFocusID != 1)
								mPyramidQuizAllRight = false;

							AppendText(" 전작에서 당신이 모험을 했던 대륙은 모두 몇 개로 이루어져 있었소?");

							ShowMenu(MenuMode.CommonSenseQuiz3, new string[] {
								"3 개",
								"4 개",
								"5 개",
								"6 개",
								"7 개"
							});
						}
						else if (menuMode == MenuMode.CommonSenseQuiz3)
						{
							if (mMenuFocusID != 2)
								mPyramidQuizAllRight = false;

							if (mPyramidQuizAllRight)
							{
								AppendText(new string[] {
									$"[color={RGB.White}] 당신은 \"또 다른 지식의 성전\"을 정말 열심히 한것 같군요.[/color]",
									$"[color={RGB.White}] 당신은 이 시험을 통과 했소.[/color]",
									"",
									$"[color={RGB.LightCyan}] [[ 경험치 + 50,000 ][/color]"
								});

								foreach (var player in mPlayerList)
									player.Experience += 50_000;

								mParty.Etc[45] |= 1 << 3;
							}
							else
								AppendText($"[color={RGB.White}] 당신은 모든 문제를 모두 맞추지 못했소.[/color]");
						}
						else if (menuMode == MenuMode.MathQuiz1)
						{
							if (mMenuFocusID != 1)
								mPyramidQuizAllRight = false;

							Ask(" X + Y = 7, X - Y = 1에서 X와 Y를 알아내서 X * Y의 답을 말해 주시오.", MenuMode.MathQuiz2, new string[] {
								"<<  0 >>",
								"<<  7 >>",
								"<< 10 >>",
								"<< 12 >>",
								"<< 14 >>"
							});
						}
						else if (menuMode == MenuMode.MathQuiz2)
						{
							if (mMenuFocusID != 3)
								mPyramidQuizAllRight = false;

							Ask(" 당신은 아마 로어 헌터를 만났을 것이오. 그가 있었던 좌표의 (X - Y)는 어떻게 되오?", MenuMode.MathQuiz3, new string[] {
								"<< 153 >>",
								"<< 168 >>",
								"<< 171 >>",
								"<< -57 >>",
								"<< -25 >>'"
							});
						}
						else if (menuMode == MenuMode.MathQuiz3)
						{
							if (mMenuFocusID != 0)
								mPyramidQuizAllRight = false;

							if (mPyramidQuizAllRight)
							{
								Dialog(new string[] {
									$"[color={RGB.White}] 당신은 수학 실력 또한 대단하군요,[/color]",
									$"[color={RGB.White}] 당신은 이 시험을 통과했소.[/color]",
									"",
									$"[color={RGB.LightCyan}] [[ 경험치 + 50,000 ][/color]"
								});

								foreach (var player in mPlayerList)
									player.Experience += 50_000;

								mParty.Etc[45] |= 1 << 5;
							}
							else
								AppendText($"[color={RGB.White}] 당신은 모든 문제를 모두 맞추지 못했소.[/color]");
						}
						else if (menuMode == MenuMode.JoinRegulus)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									Lore regulus = new Lore()
									{
										Name = "레굴루스",
										Gender = GenderType.Male,
										Class = 5,
										ClassType = ClassCategory.Sword,
										Level = 4,
										Strength = 19,
										Mentality = 5,
										Concentration = 6,
										Endurance = 15,
										Resistance = 10,
										Agility = 10,
										Accuracy = 17,
										Luck = 7,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 0,
										Shield = 0,
										Armor = 2,
										PotentialAC = 2,
										SwordSkill = 25,
										AxeSkill = 0,
										SpearSkill = 0,
										BowSkill = 0,
										ShieldSkill = 0,
										FistSkill = 0
									};

									regulus.HP = regulus.Endurance * regulus.Level * 10;
									regulus.UpdatePotentialExperience();
									UpdateItem(regulus);

									mPlayerList.Add(regulus);
									UpdateTileInfo(39, 12, 47);

									mParty.Etc[49] |= 1 << 6;

									DisplayPlayerInfo();
								}
								else
									AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
							}
						}
						else if (menuMode == MenuMode.MeetAhnYoungKi)
						{
							if (mMenuFocusID == 0)
								InvokeAnimation(AnimationType.RecallToCastleLore);
							else
								Dialog(" 자네 마음에 들었네. 그럼 잘해보게나.");
						}
						else if (menuMode == MenuMode.MeetRockMan)
						{
							if (mMenuFocusID == 0)
							{
								StartBattle(true);

								mBattleEvent = BattleEvent.RockMan;
							}
							else
							{
								mParty.Gold = 0;
								ShowMap();
							}
						}
						else if (menuMode == MenuMode.JoinDeathKnight)
						{
							if (mMenuFocusID == 0)
							{
								JoinMemberFromEnemy(62);
								mAssistPlayer.HP = 1;

								DisplayHP();
							}
							else
								ClearDialog();
						}
						else if (menuMode == MenuMode.FightWarriorOfCrux)
						{
							if (mMenuFocusID == 0)
							{
								mParty.Etc[41] |= 1 << 3;
								ShowMap();
							}
							else
							{
								for (var i = 0; i < 7; i++)
									JoinEnemy(58);

								DisplayEnemy();

								StartBattle(true);

								mBattleEvent = BattleEvent.WarriorOfCrux;
							}
						}
						else if (menuMode == MenuMode.GetArbalest)
						{
							var player = mPlayerList[mMenuFocusID];
							if (VerifyWeapon(player, 28))
							{
								player.Weapon = 28;
								UpdateItem(player);

								Dialog($"[color={RGB.White}]{player.NameSubjectJosa} 아르발레스트를 장착했다.");
								mParty.Etc[39] |= 1 << 5;
							}
							else
								Dialog($"{player.Name}에게는 이 무기가 맞지 않습니다.");
						}
						else if (menuMode == MenuMode.FinalChoice)
						{
							if (mMenuFocusID == 0)
							{
								InvokeAnimation(AnimationType.MeetNecromancer);
							}
							else
							{
								Talk(" 일행은 스스로의 능력을 믿기로 하고 실리안 카미너스를 공격하려 했다.");

								mSpecialEvent = SpecialEventType.MeetCyllianCominus;
							}
						}
					}
					//				else if (args.VirtualKey == VirtualKey.P || args.VirtualKey == VirtualKey.GamepadView)
					//				{
					//					ShowPartyStatus();
					//				}
					//				else if (args.VirtualKey == VirtualKey.V || args.VirtualKey == VirtualKey.GamepadLeftTrigger)
					//				{
					//					AppendText(new string[] { "능력을 보고 싶은 인물을 선택하시오" });
					//					ShowCharacterMenu(MenuMode.ViewCharacter);
					//				}
					//				else if (args.VirtualKey == VirtualKey.C || args.VirtualKey == VirtualKey.GamepadRightShoulder)
					//				{
					//					AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
					//					ShowCharacterMenu(MenuMode.CastSpell);
					//				}
					//				else if (args.VirtualKey == VirtualKey.E || args.VirtualKey == VirtualKey.GamepadRightShoulder)
					//				{
					//					AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
					//					ShowCharacterMenu(MenuMode.Extrasense);
					//				}

				}
				else if (args.VirtualKey == VirtualKey.R || args.VirtualKey == VirtualKey.GamepadLeftShoulder)
				{
					// 휴식 단축키
					//Rest();
				}
			};

			Window.Current.CoreWindow.KeyDown += gamePageKeyDownEvent;
			Window.Current.CoreWindow.KeyUp += gamePageKeyUpEvent;
		}

		private async Task<bool> InvokeSpecialEvent(int prevX, int prevY)
		{
			var triggered = true;

			void FindGold(int id, int bit, int gold)
			{
				if ((mParty.Etc[id] & bit) == 0)
				{
					AppendText($"당신은 금화 {gold.ToString("#,#0")}개를 발견했다.");
					mParty.Gold += gold;
					mParty.Etc[id] |= bit;
				}

				triggered = false;
			}

			void FindItem(int id, int bit, int item, int count)
			{
				var itemName = new string[] {
					"체력 회복약을", "마법 회복약을", "해독의 약초를", "의식의 약초를", "부활의 약초를",
					"소환 문서를", "대형 횃불을", "수정 구슬을", "비행 부츠를", "이동 구슬을"
				};

				if ((mParty.Etc[id] & bit) == 0)
				{
					AppendText($"일행은 {itemName[item]} {count}개 발견했다.");

					if (mParty.Item[item] + count < 256)
						mParty.Item[item] += count;
					else
						mParty.Item[item] = 255;

					mParty.Etc[id] |= bit;
				}

				triggered = false;
			}

			return triggered;
		}

		private void TalkMode(int moveX, int moveY, VirtualKey key = VirtualKey.None)
		{
			//void ShowClassTrainingMenu()
			//{
			//	AppendText("어떤 일을 원하십니까?");

			//	ShowMenu(MenuMode.TrainingCenter, new string[] {
			//		"전투사 계열의 기술을 습득",
			//		"마법사 계열의 능력을 습득",
			//		"전투사 계열의 계급을 바꿈",
			//		"마법사 계열의 계급을 바꿈"
			//	});
			//}

			//void ShowHospitalMenu()
			//{
			//	Talk($"[color={RGB.White}]여기는 병원입니다.[/color]");

			//	mSpecialEvent = SpecialEventType.CureComplete;
			//}

			//void ShowGroceryMenu()
			//{
			//	AppendText(new string[] {
			//		$"[color={RGB.White}]여기는 식료품점 입니다.[/color]",
			//		$"[color={RGB.White}]몇개를 원하십니까?[/color]",
			//	});

			//	ShowMenu(MenuMode.ChooseFoodAmount, new string[] {
			//		"10인분 : 금 100개",
			//		"20인분 : 금 200개",
			//		"30인분 : 금 300개",
			//		"40인분 : 금 400개",
			//		"50인분 : 금 500개"
			//	});
			//}
		}

		private void ShowSign(int x, int y)
		{
			Dialog(new string[] { "푯말에 쓰여있기로 ...", "", "" });
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter != null)
			{
				var payload = e.Parameter as Payload;
				mPlayerList = new List<Lore>();
				mPlayerList.Add(payload.Player);
				mParty = payload.Party;
				mMapName = payload.MapName;

				InitialFirstPlay();
			}
			//else
			//	LoadFile();
		}

		private void MovePlayer(int moveX, int moveY)
		{
			bool UpdatePlayersState(Lore player)
			{
				var needUpdate = false;
				if (player.Poison > 0)
					player.Poison++;

				if (player.Poison > 10)
				{
					player.Poison = 1;
					if (0 < player.Dead && player.Dead < 100)
						player.Dead++;
					else if (player.Unconscious > 0)
					{
						player.Unconscious++;
						if (player.Unconscious > player.Endurance * player.Level)
							player.Dead = 1;
					}
					else
					{
						player.HP--;
						if (player.HP <= 0)
							player.Unconscious = 1;
					}

					needUpdate = true;
				}

				return needUpdate;
			}

			mXAxis = moveX;
			mYAxis = moveY;

			bool needUpdateStat = false;
			foreach (var player in mPlayerList)
			{
				if (UpdatePlayersState(player))
					needUpdateStat = true;
			}

			if (mAssistPlayer != null)
			{
				if (UpdatePlayersState(mAssistPlayer))
					needUpdateStat = true;
			}

			if (needUpdateStat)
			{
				DisplayHP();
				DisplayCondition();
			}

			//if (!DetectGameOver())
			//{
			//	if (mParty.Etc[4] > 0)
			//		mParty.Etc[4]--;

			//	if (!(GetTileInfo(moveX, moveY) == 0 || (mPosition == PositionType.Den && GetTileInfo(moveX, moveY) == 52)) && mRand.Next(mEncounter * 20) == 0)
			//		EncounterEnemy();


			//	if (mPosition == PositionType.Ground)
			//		PlusTime(0, 2, 0);
			//	else
			//		PlusTime(0, 0, 5);
			//}
		}

		private bool EnterWater()
		{
			if (mParty.Etc[1] > 0)
			{
				mParty.Etc[1]--;

				//if (mRand.Next(mEncounter * 30) == 0)
				//	EncounterEnemy();

				return true;
			}
			else
				return false;
		}

		private bool EnterSwamp()
		{
			void PoisonEffectPlayer(Lore player)
			{
				if (player.Poison > 0)
					player.Poison++;

				if (player.Poison > 10)
				{
					player.Poison = 1;

					if (0 < player.Dead && player.Dead < 100)
						player.Dead++;
					else if (player.Unconscious > 0)
					{
						player.Unconscious++;

						if (player.Unconscious > player.Endurance * player.Level)
							player.Dead = 1;
					}
					else
					{
						player.HP--;
						if (player.HP <= 0)
							player.Unconscious = 1;
					}

				}
			}

			foreach (var player in mPlayerList)
			{
				PoisonEffectPlayer(player);
			}

			if (mAssistPlayer != null)
				PoisonEffectPlayer(mAssistPlayer);

			if (mParty.Etc[2] > 0)
				mParty.Etc[2]--;
			else
			{
				Dialog(new string[] { $"[color={RGB.LightRed}]일행은 독이 있는 늪에 들어갔다!!![/color]", "" });

				foreach (var player in mPlayerList)
				{
					if (mRand.Next(20) + 1 >= player.Luck)
					{
						Dialog($"[color={RGB.LightMagenta}]{player.NameSubjectJosa} 중독되었다.[/color]", true);
						if (player.Poison == 0)
							player.Poison = 1;
					}
				}

				if (mAssistPlayer != null)
				{
					if (mRand.Next(20) + 1 >= mAssistPlayer.Luck)
					{
						Dialog($"[color={RGB.LightMagenta}]{mAssistPlayer.NameSubjectJosa} 중독되었다.[/color]", true);
						if (mAssistPlayer.Poison == 0)
							mAssistPlayer.Poison = 1;
					}
				}
			}

			UpdatePlayersStat();
			if (!DetectGameOver())
				return true;
			else
				return false;
		}

		private bool EnterLava()
		{
			void LavaEffectPlayer(Lore player)
			{
				var damage = (mRand.Next(40) + 40 - 2 * mRand.Next(player.Luck)) * 10;

				if (player.HP > 0 && player.Unconscious == 0)
				{
					player.HP -= damage;
					if (player.HP <= 0)
						player.Unconscious = 1;
				}
				else if (player.HP > 0 && player.Unconscious > 0)
					player.HP -= damage;
				else if (player.Unconscious > 0 && player.Dead == 0)
				{
					player.Unconscious += damage;
					if (player.Unconscious > player.Endurance * player.Level)
						player.Dead = 1;
				}
				else if (player.Dead > 0)
				{
					if (player.Dead + damage > 30_000)
						player.Dead = 30_000;
					else
						player.Dead += damage;

				}
			}

			Dialog(new string[] { $"[color={RGB.LightRed}]일행은 용암지대로 들어섰다!!![/color]", "" });

			foreach (var player in mPlayerList)
			{
				LavaEffectPlayer(player);
			}

			UpdatePlayersStat();
			if (!DetectGameOver())
				return true;
			else
				return false;
		}

		private bool DetectGameOver()
		{
			var allPlayerDead = true;
			foreach (var player in mPlayerList)
			{
				if (player.IsAvailable)
				{
					allPlayerDead = false;
					break;
				}
			}

			if (allPlayerDead)
			{
				mParty.Etc[5] = 255;

				ShowGameOver(new string[] { "일행은 모험 중에 모두 목숨을 잃었다." });
				mTriggeredDownEvent = true;

				return true;
			}
			else
				return false;
		}

		private void ShowGameOver(string[] gameOverMessage)
		{
			AppendText(gameOverMessage);

			ShowMenu(MenuMode.BattleLose, new string[] {
						"이전의 게임을 재개한다",
						"게임을 끝낸다"
					});
		}

		private void ShowMenu(MenuMode menuMode, string[] menuItem, int focusID = 0)
		{
			mMenuMode = menuMode;
			mMenuCount = menuItem.Length;
			mMenuFocusID = focusID;

			for (var i = 0; i < mMenuList.Count; i++)
			{
				if (i < mMenuCount)
				{
					mMenuList[i].Text = menuItem[i];
					mMenuList[i].Visibility = Visibility.Visible;

					mMenuList[i].Foreground = i == focusID ? new SolidColorBrush(GetColor(RGB.White)) : new SolidColorBrush(GetColor(RGB.LightGray));
				}
				else
					mMenuList[i].Visibility = Visibility.Collapsed;
			}


			FocusMenuItem();
		}

		private void FocusMenuItem()
		{
			for (var i = 0; i < mMenuCount; i++)
			{
				if (i == mMenuFocusID)
					mMenuList[i].Foreground = new SolidColorBrush(GetColor(RGB.White));
				else
					mMenuList[i].Foreground = new SolidColorBrush(GetColor(RGB.LightGray));
			}
		}

		private async void InitialFirstPlay()
		{
			await LoadMapData();
			InitializeMap();

			DisplayPlayerInfo();

			UpdateTileInfo(23, 33, 43);
			UpdateTileInfo(34, 40, 43);
			UpdateTileInfo(14, 35, 43);
			UpdateTileInfo(26, 25, 43);
			UpdateTileInfo(26, 31, 21);

			mXAxis = 26;
			mYAxis = 28;

			mParty.Etc[0] = 4;
			UpdateView();

			Dialog(new string[] {
				" 여기는 로어 대륙의 메너스 금광.",
				" 때는 673년 11월 27일 오후였다."
			});

			mLoading = false;
		}

		private void InitializeMap()
		{
			Uri musicUri;
			//if (mParty.Map == 1 || mParty.Map == 3 || mParty.Map == 4)
			//{
			//	mPosition = PositionType.Ground;
			//	musicUri = new Uri("ms-appx:///Assets/ground.mp3");
			//}
			//else if (6 <= mParty.Map && mParty.Map <= 9)
			//{
			//	mPosition = PositionType.Town;
			//	musicUri = new Uri("ms-appx:///Assets/town.mp3");
			//}
			//else if (mParty.Map == 2 || (10 <= mParty.Map && mParty.Map <= 17))
			//{
			//	mPosition = PositionType.Den;
			//	musicUri = new Uri("ms-appx:///Assets/den.mp3");
			//}
			//else
			//{
			//	mPosition = PositionType.Keep;
			//	musicUri = new Uri("ms-appx:///Assets/keep.mp3");
			//}

			if ((mMapHeader.StartX != 255 || mMapHeader.StartY != 255) && mMapHeader.TileType != PositionType.Ground) {
				mXAxis = mMapHeader.StartX;
				mYAxis = mMapHeader.StartY;
			}

			if (mMapHeader.Height / 2 > mYAxis)
				mFace = 4;
			else
				mFace = 5;

			if (mPlayerList[0].ClassType == ClassCategory.Magic)
				mFace += 4;
			else if (mPlayerList[0].ClassType == ClassCategory.Sword) {
				if (mMapHeader.TileType == PositionType.Town)
					mFace -= 4;
				else if (mMapName == "Menace")
					mFace -= 4;
			}

			switch (mParty.Etc[11])
			{

			}

			if (1 > mEncounter || mEncounter > 3)
				mEncounter = 2;

			if (3 > mMaxEnemy || mMaxEnemy > 7)
				mMaxEnemy = 5;

			//ShowMap();
			//BGMPlayer.Source = musicUri;

			UpdateView();
		}

		private void UpdateView()
		{
			bool dark;
			if (mMapHeader.TileType == PositionType.Den)
			{
				dark = true;

				mXWide = 1;
				mYWide = 1;
			}
			else if (7 > mParty.Hour || mParty.Hour > 17)
			{
				dark = true;
				if (mParty.Hour == 18)
				{
					if (0 <= mParty.Min && mParty.Min <= 19)
					{
						mXWide = 4;
						mYWide = 4;
					}
					else if (20 <= mParty.Min && mParty.Min <= 39)
					{
						mXWide = 3;
						mYWide = 3;
					}
					else
					{
						mXWide = 2;
						mYWide = 2;
					}
				}
				else if (mParty.Hour == 6)
				{
					if (0 <= mParty.Min && mParty.Min <= 19)
					{
						mXWide = 2;
						mYWide = 2;
					}
					else if (20 <= mParty.Min && mParty.Min <= 39)
					{
						mXWide = 3;
						mYWide = 3;
					}
					else
					{
						mXWide = 4;
						mYWide = 4;
					}
				}
				else
				{
					mXWide = 1;
					mYWide = 1;
				}
			}
			else
				dark = false;

			if (dark && mParty.Etc[0] > 0)
			{
				if (1 <= mParty.Etc[0] && mParty.Etc[0] <= 2)
				{
					if (mXWide < 2 && mYWide < 2)
					{
						mXWide = 2;
						mYWide = 2;
					}
				}
				else if (3 <= mParty.Etc[0] && mParty.Etc[0] <= 4)
				{
					if (mXWide < 3 && mYWide < 3)
					{
						mXWide = 3;
						mYWide = 3;
					}
				}
				else if (5 <= mParty.Etc[0] && mParty.Etc[0] <= 6)
				{
					if (mXWide < 4 && mYWide < 4)
					{
						mXWide = 4;
						mYWide = 4;
					}
				}
				else
				{
					mXWide = 4;
					mYWide = 5;
				}
			}
			else if (!dark)
			{
				mXWide = 0;
				mYWide = 0;
			}
		}

		private async Task LoadMapData()
		{
			var mapFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/{mMapName.ToUpper()}.M"));
			var stream = (await mapFile.OpenReadAsync()).AsStreamForRead();
			var reader = new BinaryReader(stream);

			mMapHeader = new MapHeader();

			lock (mapLock)
			{
				var mapNameLen = reader.ReadByte();
				var mapName = Encoding.UTF8.GetString(reader.ReadBytes(mapNameLen));

				if (10 - mapNameLen > 0)
					reader.ReadBytes(10 - mapNameLen);
				
				mMapHeader.Width = reader.ReadByte();
				mMapHeader.Height = reader.ReadByte();

				switch (reader.ReadByte()) {
					case 0:
						mMapHeader.TileType = PositionType.Town;
						break;
					case 1:
						mMapHeader.TileType = PositionType.Ground;
						break;
					case 2:
						mMapHeader.TileType = PositionType.Den;
						break;
					default:
						mMapHeader.TileType = PositionType.Keep;
						break;
				}

				if (reader.ReadByte() == 0)
					mMapHeader.Encounter = false;
				else
					mMapHeader.Encounter = true;


				if (reader.ReadByte() == 0)
					mMapHeader.Handicap = false;
				else
					mMapHeader.Handicap = true;

				mMapHeader.StartX = reader.ReadByte() - 1;
				mMapHeader.StartY = reader.ReadByte() - 1;

				var exitMapNameLen = reader.ReadByte();
				if (exitMapNameLen > 0)
					mMapHeader.ExitMap = Encoding.UTF8.GetString(reader.ReadBytes(exitMapNameLen));
				else
					mMapHeader.ExitMap = "";

				if (10 - exitMapNameLen > 0)
					reader.ReadBytes(10 - exitMapNameLen);

				mMapHeader.ExitX = reader.ReadByte() - 1;
				mMapHeader.ExitY = reader.ReadByte() - 1;

				var enterMapNameLen = reader.ReadByte();
				if (enterMapNameLen > 0)
					mMapHeader.EnterMap = Encoding.UTF8.GetString(reader.ReadBytes(enterMapNameLen));
				else
					mMapHeader.EnterMap = "";

				if (10 - enterMapNameLen > 0)
					reader.ReadBytes(10 - enterMapNameLen);

				mMapHeader.EnterX = reader.ReadByte() - 1;
				mMapHeader.EnterY = reader.ReadByte() - 1;
				mMapHeader.Default = reader.ReadByte();
				mMapHeader.HandicapBit = reader.ReadByte();

				reader.ReadBytes(50);

				mMapHeader.Layer = reader.ReadBytes(mMapHeader.Width * mMapHeader.Height);
			}
		}

		private void DisplayPlayerInfo()
		{
			for (var i = 0; i < mPlayerNameList.Count; i++)
			{
				if (i < mPlayerList.Count)
				{
					mPlayerNameList[i].Text = mPlayerList[i].Name;
					mPlayerNameList[i].Foreground = new SolidColorBrush(Colors.White);
				}
				else if (i == mPlayerList.Count && mAssistPlayer != null)
				{
					mPlayerNameList[i].Text = mAssistPlayer.Name;
					mPlayerNameList[i].Foreground = new SolidColorBrush(Colors.White);
				}
				else
				{
					mPlayerNameList[i].Text = "";
				}
			}

			DisplayHP();
			DisplaySP();
			DisplayCondition();
		}

		private void UpdatePlayersStat()
		{
			DisplayHP();
			DisplaySP();
			DisplayCondition();
		}

		private void DisplayHP()
		{
			for (var i = 0; i < mPlayerHPList.Count; i++)
			{
				if (i < mPlayerList.Count)
					mPlayerHPList[i].Text = mPlayerList[i].HP.ToString("#,#0");
				else if (i == mPlayerList.Count && mAssistPlayer != null)
					mPlayerHPList[i].Text = mAssistPlayer.HP.ToString("#,#0");
				else
					mPlayerHPList[i].Text = "";
			}
		}

		private void DisplaySP()
		{
			for (var i = 0; i < mPlayerSPList.Count; i++)
			{
				if (i < mPlayerList.Count)
					mPlayerSPList[i].Text = mPlayerList[i].SP.ToString("#,#0");
				else if (i == mPlayerList.Count && mAssistPlayer != null)
					mPlayerSPList[i].Text = mAssistPlayer.SP.ToString("#,#0");
				else
					mPlayerSPList[i].Text = "";
			}
		}

		private void DisplayCondition()
		{
			void UpdateCondition(TextBlock conditionText, Lore player)
			{
				if (player.HP <= 0 && player.Unconscious == 0)
					player.Unconscious = 1;

				if (player.Unconscious > player.Endurance * player.Level && player.Dead == 0)
					player.Dead = 1;

				if (player.Dead > 0)
				{
					conditionText.Foreground = new SolidColorBrush(GetColor(RGB.LightRed));
					conditionText.Text = "죽은 상태";
				}
				else if (player.Unconscious > 0)
				{
					conditionText.Foreground = new SolidColorBrush(GetColor(RGB.LightRed));
					conditionText.Text = "의식불명";
				}
				else if (player.Poison > 0)
				{
					conditionText.Foreground = new SolidColorBrush(GetColor(RGB.LightRed));
					conditionText.Text = "중독";
				}
				else
				{
					conditionText.Foreground = new SolidColorBrush(GetColor(RGB.White));
					conditionText.Text = "좋음";
				}
			}

			for (var i = 0; i < mPlayerConditionList.Count; i++)
			{
				if (i < mPlayerList.Count)
					UpdateCondition(mPlayerConditionList[i], mPlayerList[i]);
				else if (i == mPlayerList.Count && mAssistPlayer != null)
					UpdateCondition(mPlayerConditionList[i], mAssistPlayer);
				else
					mPlayerConditionList[i].Text = "";
			}
		}

		private bool AppendText(string str, bool append = false)
		{
			//var dialogFull = IsDialogFull();
			return AppendText(DialogText, str, append);
		}

		private void AppendText(string[] text, bool append = false)
		{
			var added = true;
			for (var i = 0; i < text.Length; i++)
			{
				if (added)
				{
					if (i == 0)
						added = AppendText(text[i], append);
					else
						added = AppendText(text[i], true);
				}

				if (!added)
					mRemainDialog.Add(text[i]);
			}
		}

		private bool AppendText(RichTextBlock textBlock, string str, bool append = false)
		{

			if (PartyInfoPanel.Visibility == Visibility.Visible)
				PartyInfoPanel.Visibility = Visibility.Collapsed;

			if (StatPanel.Visibility == Visibility.Visible)
				StatPanel.Visibility = Visibility.Collapsed;

			if (StatHealthPanel.Visibility == Visibility.Visible)
				StatHealthPanel.Visibility = Visibility.Collapsed;

			if (DialogText.Visibility == Visibility.Collapsed)
				DialogText.Visibility = Visibility.Visible;

			var totalLen = 0;

			var noText = textBlock.Blocks.Count > 0 ? true : false;

			if (append)
			{
				foreach (Paragraph prevParagraph in textBlock.Blocks)
				{
					foreach (Run prevRun in prevParagraph.Inlines)
					{
						totalLen += prevRun.Text.Length;
					}
				}
			}
			else
			{
				textBlock.TextHighlighters.Clear();
				textBlock.Blocks.Clear();
			}

			var highlighters = new List<TextHighlighter>();
			var paragraph = new Paragraph();
			textBlock.Blocks.Add(paragraph);

			var startIdx = 0;
			while ((startIdx = str.IndexOf("[", startIdx)) >= 0)
			{
				if (startIdx < str.Length - 1 && str[startIdx + 1] == '[')
				{
					str = str.Remove(startIdx, 1);
					startIdx++;
					continue;
				}

				var preRun = new Run
				{
					Text = str.Substring(0, startIdx)
				};

				paragraph.Inlines.Add(preRun);
				var preTextHighlighter = new TextHighlighter()
				{
					Foreground = new SolidColorBrush(GetColor(RGB.LightGray)),
					Background = new SolidColorBrush(Colors.Transparent),
					Ranges = { new TextRange()
									{
										StartIndex = totalLen,
										Length = preRun.Text.Length
									}
								}
				};

				highlighters.Add(preTextHighlighter);
				textBlock.TextHighlighters.Add(preTextHighlighter);

				totalLen += preRun.Text.Length;
				str = str.Substring(startIdx + 1);

				startIdx = str.IndexOf("]");
				if (startIdx < 0)
					break;

				var tag = str.Substring(0, startIdx);
				str = str.Substring(startIdx + 1);
				var tagData = tag.Split("=");

				var endTag = $"[/{tagData[0]}]";
				startIdx = str.IndexOf(endTag);

				if (startIdx < 0)
					break;


				if (tagData[0] == "color" && tagData.Length > 1 && tagData[1].Length == 6)
				{
					var tagRun = new Run
					{
						Text = str.Substring(0, startIdx).Replace("[[", "[")
					};

					paragraph.Inlines.Add(tagRun);

					var textHighlighter = new TextHighlighter()
					{
						Foreground = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(tagData[1].Substring(0, 2), 16), Convert.ToByte(tagData[1].Substring(2, 2), 16), Convert.ToByte(tagData[1].Substring(4, 2), 16))),
						Background = new SolidColorBrush(Colors.Transparent),
						Ranges = { new TextRange()
									{
										StartIndex = totalLen,
										Length = tagRun.Text.Length
									}
								}
					};

					highlighters.Add(textHighlighter);
					textBlock.TextHighlighters.Add(textHighlighter);

					totalLen += tagRun.Text.Length;
				}

				str = str.Substring(startIdx + endTag.Length);
				startIdx = 0;
			}

			var run = new Run
			{
				Text = str
			};

			paragraph.Inlines.Add(run);
			var postTextHighlighter = new TextHighlighter()
			{
				Foreground = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(RGB.LightGray.Substring(0, 2), 16), Convert.ToByte(RGB.LightGray.Substring(2, 2), 16), Convert.ToByte(RGB.LightGray.Substring(4, 2), 16))),
				Background = new SolidColorBrush(Colors.Transparent),
				Ranges = { new TextRange()
								{
									StartIndex = totalLen,
									Length = run.Text.Length
								}
							}
			};

			highlighters.Add(postTextHighlighter);
			textBlock.TextHighlighters.Add(postTextHighlighter);

			totalLen += run.Text.Length;


			textBlock.UpdateLayout();
			//var DialogText.Tag

			var lineHeight = 0d;
			if (textBlock.Blocks.Count > 0)
			{
				var startRect = DialogText.Blocks[0].ContentStart.GetCharacterRect(LogicalDirection.Forward);
				lineHeight = startRect.Height;
			}

			var lineCount = lineHeight == 0 ? 0 : (int)Math.Ceiling(DialogText.ActualHeight / lineHeight);

			if (lineCount > DIALOG_MAX_LINES)
			{
				textBlock.Blocks.Remove(paragraph);
				foreach (var highlighter in highlighters)
					textBlock.TextHighlighters.Remove(highlighter);

				return false;
			}
			else
				return true;
		}

		private void ClearDialog()
		{
			DialogText.TextHighlighters.Clear();
			DialogText.Blocks.Clear();
		}

		private void Dialog(string dialog, bool append = false)
		{
			Dialog(new string[] { dialog }, append);
		}

		private void Dialog(string[] dialog, bool append = false)
		{
			AppendText(dialog, append);

			mAfterDialogType = AfterDialogType.None;

			if (mRemainDialog.Count > 0)
				ContinueText.Visibility = Visibility.Visible;
		}

		private void Talk(string dialog, bool append = false)
		{
			Talk(new string[] { dialog }, append);
		}

		private void Talk(string[] dialog, bool append = false)
		{
			AppendText(dialog, append);

			if (mRemainDialog.Count > 0)
				mAfterDialogType = AfterDialogType.PressKey;
			else
				mAfterDialogType = AfterDialogType.None;

			ContinueText.Visibility = Visibility.Visible;
		}

		private void canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			args.TrackAsyncAction(LoadImages(sender.Device).AsAsyncAction());
		}

		private async Task LoadImages(CanvasDevice device)
		{
			try
			{
				mMapTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_tile.png"), new Vector2(52, 52), Vector2.Zero);
				mCharacterTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_sprite.png"), new Vector2(52, 52), Vector2.Zero);

				//await LoadEnemyData();

			}
			catch (Exception e)
			{
				Debug.WriteLine($"에러: {e.Message}");
			}
		}

		private Color GetColor(string color)
		{
			return Color.FromArgb(0xff, Convert.ToByte(color.Substring(0, 2), 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16));
		}

		private byte GetTileInfo(int x, int y)
		{
			return (byte)(mMapHeader.Layer[x + mMapHeader.Width * y] & 0x7F);
		}
		private byte GetTileInfo(byte[] layer, int index)
		{
			return (byte)(layer[index] & 0x7F);
		}

		private void UpdateTileInfo(int x, int y, int tile)
		{
			mMapHeader.Layer[x + mMapHeader.Width * y] = (byte)((mMapHeader.Layer[x + mMapHeader.Width * y] & 0x80) | tile);
		}

		private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			//void AnimateTransition(int frame, int x, int y)
			//{
			//	// 총 117 프레임
			//	for (var i = 0; i < frame; i++)
			//	{
			//		args.DrawingSession.FillRectangle(new Rect((x - 4) * 52 + (i * 4), (y - 5) * 52, 2, 52 * 11), Colors.Black);
			//		args.DrawingSession.FillRectangle(new Rect((x - 4) * 52 + ((116 - i) * 4) + 2, (y - 5) * 52, 2, 52 * 11), Colors.Black);
			//	}
			//}

			var playerX = mXAxis;
			var playerY = mYAxis;

			var xOffset = 0;
			var yOffset = 0;
			//if (mTelescopeXCount != 0)
			//{
			//	if (mTelescopeXCount < 0)
			//		xOffset = -(mTelescopePeriod - Math.Abs(mTelescopeXCount));
			//	else
			//		xOffset = mTelescopePeriod - Math.Abs(mTelescopeXCount);
			//}

			//if (mTelescopeYCount != 0)
			//{
			//	if (mTelescopeYCount < 0)
			//		yOffset = -(mTelescopePeriod - Math.Abs(mTelescopeYCount));
			//	else
			//		yOffset = mTelescopePeriod - Math.Abs(mTelescopeYCount);
			//}

			var transform = Matrix3x2.Identity * Matrix3x2.CreateTranslation(-new Vector2(52 * (playerX - 4 + xOffset), 52 * (playerY - 5 + yOffset)));
			args.DrawingSession.Transform = transform;

			var size = sender.Size.ToVector2();

			//var options = CanvasSpriteOptions.None;
			//var interpolation = (CanvasImageInterpolation)Enum.Parse(typeof(CanvasImageInterpolation), InterpolationMode);
			var interpolation = (CanvasImageInterpolation)Enum.Parse(typeof(CanvasImageInterpolation), CanvasImageInterpolation.HighQualityCubic.ToString());

			using (var sb = args.DrawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.NearestNeighbor, CanvasSpriteOptions.ClampToSourceRect))
			{
				lock (mapLock)
				{
					for (int i = 0; i < mMapHeader.Layer.Length; ++i)
					{
						DrawTile(sb, mMapHeader.Layer, i, playerX, playerY);
					}
				}

				if (mCharacterTiles != null && mFace >= 0)
				{
				//	if (mAnimationEvent == AnimationType.EnterFortressOfMephistopheles && mAnimationFrame <= 6)
				//	{
				//		mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY + (6 - mAnimationFrame)), Vector4.One);
				//	}
				//	else
				//	{
						mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), Vector4.One);

				//		if (mMenuMode == MenuMode.MeetAhnYoungKi)
				//			mCharacterTiles.Draw(sb, 24, mCharacterTiles.SpriteSize * new Vector2(playerX - 1, playerY), Vector4.One);
				//	}
				}
			}

			//if ((mAnimationEvent == AnimationType.SleepLoreCastle ||
			//	mAnimationEvent == AnimationType.LandUnderground ||
			//	mAnimationEvent == AnimationType.LandUnderground2 ||
			//	mAnimationEvent == AnimationType.RecallToCastleLore ||
			//	mAnimationEvent == AnimationType.ReturnCastleLore ||
			//	mAnimationEvent == AnimationType.ReturnCastleLore2 ||
			//	mAnimationEvent == AnimationType.TalkLordAhn ||
			//	mAnimationEvent == AnimationType.TalkLordAhn2 ||
			//	mAnimationEvent == AnimationType.EnterUnderworld ||
			//	mAnimationEvent == AnimationType.ReturnToGround ||
			//	mAnimationEvent == AnimationType.MeetNecromancer) && mAnimationFrame <= 117)
			//	AnimateTransition(mAnimationFrame, playerX, playerY);
		}

		private void DrawTile(CanvasSpriteBatch sb, byte[] layer, int index, int playerX, int playerY)
		{
			int row = index / mMapHeader.Width;
			int column = index % mMapHeader.Width;

			Vector4 tint;

			if ((layer[index] & 0x80) > 0 || (mXWide == 0 && mYWide == 0) || (playerX - mXWide <= column && column <= playerX + mXWide && playerY - mYWide <= row && row <= playerY + mYWide))
				tint = Vector4.One;
			else
				tint = new Vector4(0.1f, 0.1f, 0.6f, 1);

			if (mMapTiles != null)
			{
				var mapIdx = 56;

				switch (mMapHeader.TileType) {
					case PositionType.Town:
						mapIdx = 0;
						break;
					case PositionType.Keep:
						mapIdx *= 1;
						break;
					case PositionType.Ground:
						mapIdx *= 2;
						break;
					case PositionType.Den:
						mapIdx *= 3;
						break;
				}


				byte tileIdx = GetTileInfo(layer, index);

				//	if (mSpecialEvent == SpecialEventType.Penetration)
				//	{
				//		if ((mPosition == PositionType.Den || mPosition == PositionType.Keep) && tileIdx == 52)
				//			tileIdx = 0;
				//	}
				//	else if (mParty.Map == 2 && tileIdx == 52)
				//		tileIdx = 48;
				//	//#if DEBUG
				//	//else if (tileIdx == 0 || ((mPosition == PositionType.Den || mPosition == PositionType.Keep) && tileIdx == 52))
				//	//#else
				//	else if (tileIdx == 0)
				//	//#endif
				//	{
				//		//#if DEBUG
				//		//					tileIdx = 0;
				//		//#else
				//		switch (mParty.Map)
				//		{
				//			case 1:
				//				tileIdx = 41;
				//				break;
				//			case 2:
				//				tileIdx = 44;
				//				break;
				//			case 3:
				//				tileIdx = 10;
				//				break;
				//			case 4:
				//				tileIdx = 10;
				//				break;
				//			case 5:
				//				tileIdx = 0;
				//				break;
				//			case 6:
				//				tileIdx = 44;
				//				break;
				//			case 7:
				//				tileIdx = 45;
				//				break;
				//			case 8:
				//				tileIdx = 47;
				//				break;
				//			case 9:
				//				tileIdx = 47;
				//				break;
				//			case 10:
				//				tileIdx = 0;
				//				break;
				//			case 11:
				//				tileIdx = 41;
				//				break;
				//			case 12:
				//				tileIdx = 0;
				//				break;
				//			case 13:
				//				tileIdx = 41;
				//				break;
				//			case 14:
				//				tileIdx = 43;
				//				break;
				//			case 15:
				//				tileIdx = 44;
				//				break;
				//			case 16:
				//				tileIdx = 42;
				//				break;
				//			case 17:
				//				tileIdx = 0;
				//				break;
				//			case 18:
				//				tileIdx = 41;
				//				break;
				//		}
				//		//#endif
				//	}

				//	if (mAnimationEvent == AnimationType.Cookie)
				//	{

				//		if (column == playerX - 1 && row == playerY + (6 - mAnimationFrame))
				//			mMapTiles.Draw(sb, 50 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//		else
				//			mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//	}
				//	else if (mAnimationEvent == AnimationType.LordAhnCall)
				//	{
				//		if (row == 48 && 1 <= mAnimationFrame && mAnimationFrame <= 4)
				//		{
				//			if (column == 23 - mAnimationFrame)
				//				mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//			else
				//				mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//		}
				//		else if (mAnimationFrame >= 5)
				//		{
				//			if (column == 19)
				//			{
				//				if (playerY > 48 && row == 44 + mAnimationFrame)
				//				{
				//					mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//				}
				//				else if (playerY < 48 && row == playerY - (mAnimationFrame - 5))
				//				{
				//					mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//				}
				//				else
				//					mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//			}
				//			else
				//				mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//		}
				//		else
				//			mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//	}
				//	else if (mAnimationEvent == AnimationType.LeaveSoldier)
				//	{
				//		if (mAnimationFrame <= 3)
				//		{
				//			if (column == mAnimationFrame + 18 && row == playerY)
				//				mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//			else
				//				mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//		}
				//		else if (mAnimationFrame <= Math.Abs(playerY - 48) + 3)
				//		{
				//			if (playerY < 48 && row == playerY + (mAnimationFrame - 3) && column == 21)
				//				mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//			else if (playerY > 48 && row == playerY - (mAnimationFrame - 3) && column == 21)
				//				mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//			else
				//				mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//		}
				//		else if (column == 22 && row == 48)
				//			mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//		else
				//			mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				//	}
				//	else
				mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
			}
		}

		private void MapCanvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			async Task LoadTile(CanvasDevice device)
			{
				try
				{
					mWizardEyeTile = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/WizardEyeTile.png"), new Vector2(2, 2), Vector2.Zero);

				}
				catch (Exception e)
				{
					Debug.WriteLine($"에러: {e.Message}");
				}
			}

			args.TrackAsyncAction(LoadTile(sender.Device).AsAsyncAction());
		}

		private void MapCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			using (var sb = args.DrawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.NearestNeighbor, CanvasSpriteOptions.ClampToSourceRect))
			{
				lock (mWizardEye)
				{
					for (var y = 0; y < mWizardEye.Height; y++)
					{
						for (var x = 0; x < mWizardEye.Width; x++)
						{
							if (mWizardEyePosX == x && mWizardEyePosY == y)
							{
								if (mWizardEyePosBlink)
									mWizardEyeTile.Draw(sb, 12, mWizardEyeTile.SpriteSize * new Vector2(x * 2, y * 2), Vector4.One);
								else
									mWizardEyeTile.Draw(sb, 0, mWizardEyeTile.SpriteSize * new Vector2(x * 2, y * 2), Vector4.One);
							}
							else
								mWizardEyeTile.Draw(sb, mWizardEye.GetData(x, y), mWizardEyeTile.SpriteSize * new Vector2(x * 2, y * 2), Vector4.One);
						}
					}
				}
			}
		}

		private class WizardEye
		{
			public void Set(int width, int height)
			{
				Data = new byte[width * height];
				Width = width;
				Height = height;
			}

			public byte[] Data
			{
				get;
				private set;
			}

			public byte GetData(int x, int y)
			{
				return Data[x + y * Width];
			}

			public int Width
			{
				get;
				private set;
			}

			public int Height
			{
				get;
				private set;
			}
		}

		public enum PositionType
		{
			Town,
			Ground,
			Den,
			Keep
		}

		public class MapHeader { 
			public string ID {
				get;
				set;
			}

			public int Width {
				get;
				set;
			}

			public int Height {
				get;
				set;
			}

			public PositionType TileType {
				get;
				set;
			}

			public bool Encounter {
				get;
				set;
			}

			public bool Handicap {
				get;
				set;
			}

			public int StartX {
				get;
				set;
			}

			public int StartY {
				get;
				set;
			}

			public string ExitMap {
				get;
				set;
			}

			public int ExitX {
				get;
				set;
			}

			public int ExitY {
				get;
				set;
			}

			public string EnterMap {
				get;
				set;
			}

			public int EnterX {
				get;
				set;
			}

			public int EnterY {
				get;
				set;
			}

			public int Default {
				get;
				set;
			}

			public int HandicapBit {
				get;
				set;
			}

			public byte[] Layer {
				get;
				set;
			}
		}

		private class HealthTextBlock
		{
			private TextBlock mName;
			private TextBlock mPoison;
			private TextBlock mUnconscious;
			private TextBlock mDead;

			public HealthTextBlock(TextBlock name, TextBlock poison, TextBlock unconscious, TextBlock dead)
			{
				mName = name;
				mPoison = poison;
				mUnconscious = unconscious;
				mDead = dead;
			}

			public void Update(string name, int poison, int unconscious, int dead)
			{
				mName.Text = name;
				mPoison.Text = poison.ToString();
				mUnconscious.Text = unconscious.ToString();
				mDead.Text = dead.ToString();
			}

			public void Clear()
			{
				mName.Text = "";
				mPoison.Text = "";
				mUnconscious.Text = "";
				mDead.Text = "";
			}
		}

		private enum SpecialEventType
		{
			None,
			
		}

		private enum BattleEvent
		{
			None,
		}

		private enum AnimationType
		{
			None,
		}

		private enum SpinnerType
		{
			None,
			TeleportationRange,
			RestTimeRange
		}

		private enum AfterDialogType
		{
			None,
			PressKey,
			Menu,
			Animation
		}

		enum MenuMode {
			None,
			BattleLose
		}
	}
}
