using Microsoft.Graphics.Canvas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.XboxLive.Storage;
using Windows.Storage;
using Windows.Storage.Streams;
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
		private readonly string[] mItems = new string[] { "화살", "소환 문서", "대형 횃불", "수정 구슬", "비행 부츠", "이동 구슬" };
		private readonly int[] mItemPrices = new int[] { 500, 4_000, 300, 500, 1_000, 5_000 };
		private int mBuyItemID;

		private readonly string[] mMedicines = new string[] { "체력 회복약", "마법 회복약", "해독의 약초", "의식의 약초", "부활의 약초" };
		private readonly int[] mMedicinePrices = new int[] { 2_000, 3_000, 1_000, 5_000, 10_000 };
		private int mBuyMedicineID;

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
		private bool mCruel = true;

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

		private List<EnemyData> mEnemyDataList = null;
		private List<BattleEnemyData> mEncounterEnemyList = new List<BattleEnemyData>();
		private int mBattlePlayerID = 0;
		private int mBattleFriendID = 0;
		private int mBattleCommandID = 0;
		private int mBattleToolID = 0;
		private int mEnemyFocusID = 0;
		private Queue<BattleCommand> mBattleCommandQueue = new Queue<BattleCommand>();
		private Queue<BattleEnemyData> mBatteEnemyQueue = new Queue<BattleEnemyData>();
		private BattleTurn mBattleTurn = BattleTurn.None;

		private bool mLoading = true;

		private const int DIALOG_MAX_LINES = 13;
		private readonly List<string> mCureResult = new List<string>();
		private readonly List<string> mRemainDialog = new List<string>();
		private AfterDialogType mAfterDialogType = AfterDialogType.None;
		private MenuMode mAfterMenuMode;
		private string[] mAfterMenuList;

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

		private int mUseItemID;
		private Lore mItemUsePlayer;
		private readonly List<int> mUsableItemIDList = new List<int>();

		private int mUseCrystalID = -1;


		private int mTeleportationDirection = 0;

		private Lore mMagicPlayer = null;
		private Lore mMagicWhomPlayer = null;

		private int mOrderFromPlayerID = -1;

		private int mPrevX;
		private int mPrevY;

		private int mTelescopePeriod = 0;
		private int mTelescopeXCount = 0;
		private int mTelescopeYCount = 0;

		private Dictionary<string, string> mEnterTypeMap = new Dictionary<string, string>();
		private string mTryEnterType = "";

		private int mExchangeCategory;
		private Lore mExchangePlayer;

		private Lore mUnequipPlayer;

		private int mWeaponTypeID;
		private int mBuyWeaponID = -1;

		private int mTrainTime;

		private Lore mCurePlayer = null;

		private TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyDownEvent = null;
		private TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyUpEvent = null;

		private Random mRand = new Random();

		private bool mEbony = false;
		private bool mDark = false;
		private bool mMoonLight = false;

		private bool mTimeWatch = false;
		private int mTimeEvent;
		private int mWatchYear;
		private int mWatchDay;
		private int mWatchHour;
		private int mWatchMin;
		private int mWatchSec;

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
							//if (mMapName == 1)
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
							//else if (mMapName == 2)
							//{
							//	if (x == 99 && y == 99)
							//		ShowExitMenu();
							//	else if (x == 15 && y == 15)
							//		ShowEnterMenu(EnterType.ProofOfInfortune);
							//	else if (x == 148 && y == 64)
							//		ShowEnterMenu(EnterType.ClueOfInfortune);
							//}
							//else if (mMapName == 3)
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
							//else if (mMapName == 4)
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
							//else if (mMapName == 7)
							//{
							//	AppendText(new string[] {
							//		" 당신이 동굴 입구에 들어가려 할 때 어떤 글을 보았다.",
							//		"",
							//		"",
							//		"",
							//		$"[color={RGB.White}]   여기는 한때 피라미드라고 불리는 악마의 동굴이었지만 지금은 폐쇄되어 아무도 들어갈 수가 없습니다.[/color]"
							//	});
							//}
							//else if (mMapName == 11)
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
							//else if (mMapName == 12)
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
								MovePlayer(x, y, true);
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
								MovePlayer(x, y, true);
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
								MovePlayer(x, y, true);
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
								MovePlayer(x, y, true);
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

				void ShowTrainSkillMenu(int defaultMenuID)
				{
					AppendText($"[color={RGB.White}]{mTrainPlayer.Name}의 현재 능력치[/color]");

					var trainSkillMenuList = new List<string>();
					mTrainSkillList.Clear();

					if (swordEnableClass[mTrainPlayer.Class - 1, 0] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  베는 무기 기술치 :\t{mTrainPlayer.SwordSkill}[/color]", true);
						trainSkillMenuList.Add("  베는 무기 기술치");
						mTrainSkillList.Add(new Tuple<int, int>(0, swordEnableClass[mTrainPlayer.Class - 1, 0]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 1] > 0)
					{
						if (mTrainPlayer.Class != 7)
						{
							AppendText($"[color={RGB.LightCyan}]  찍는 무기 기술치 :\t{mTrainPlayer.AxeSkill}[/color]", true);
							trainSkillMenuList.Add("  찍는 무기 기술치");
						}
						else
						{
							AppendText($"[color={RGB.LightCyan}]  치료 마법 능력치 :\t{mTrainPlayer.AxeSkill}[/color]", true);
							trainSkillMenuList.Add("  치료 마법 능력치");
						}

						mTrainSkillList.Add(new Tuple<int, int>(1, swordEnableClass[mTrainPlayer.Class - 1, 1]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 2] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  찌르는 무기 기술치 :\t{mTrainPlayer.SpearSkill}[/color]", true);
						trainSkillMenuList.Add("  찌르는 무기 기술치");
						mTrainSkillList.Add(new Tuple<int, int>(2, swordEnableClass[mTrainPlayer.Class - 1, 2]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 3] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  쏘는 무기 기술치 :\t{mTrainPlayer.BowSkill}[/color]", true);
						trainSkillMenuList.Add("  쏘는 무기 기술치");
						mTrainSkillList.Add(new Tuple<int, int>(3, swordEnableClass[mTrainPlayer.Class - 1, 3]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 4] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  방패 사용 기술치 :\t{mTrainPlayer.ShieldSkill}[/color]", true);
						trainSkillMenuList.Add("  방패 사용 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(4, swordEnableClass[mTrainPlayer.Class - 1, 4]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 5] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  맨손 사용 기술치 :\t{mTrainPlayer.FistSkill}[/color]", true);
						trainSkillMenuList.Add("  맨손 사용 기술치");
						mTrainSkillList.Add(new Tuple<int, int>(5, swordEnableClass[mTrainPlayer.Class - 1, 5]));
					}

					AppendText($"[color={RGB.LightGreen}] 여분의 경험치 :\t{mTrainPlayer.Experience.ToString("#,#0")}[/color]", true);

					AppendText($"[color={RGB.LightRed}]당신이 수련하고 싶은 부분을 고르시오.[/color]", true);

					ShowMenu(MenuMode.ChooseTrainSkill, trainSkillMenuList.ToArray(), defaultMenuID);
				}

				void ShowTrainMagicMenu(int defaultMenuID)
				{
					AppendText($"[color={RGB.White}]{mTrainPlayer.Name}의 현재 능력치[/color]");

					var trainSkillMenuList = new List<string>();
					mTrainSkillList.Clear();

					if (magicEnableClass[mTrainPlayer.Class - 1, 0] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  공격 마법 능력치 :\t{mTrainPlayer.AttackMagic}[/color]", true);
						trainSkillMenuList.Add("  공격 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(0, magicEnableClass[mTrainPlayer.Class - 1, 0]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 1] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  변화 마법 능력치 :\t{mTrainPlayer.PhenoMagic}[/color]", true);
						trainSkillMenuList.Add("  변화 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(1, magicEnableClass[mTrainPlayer.Class - 1, 1]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 2] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  치료 마법 능력치 :\t{mTrainPlayer.CureMagic}[/color]", true);
						trainSkillMenuList.Add("  치료 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(2, magicEnableClass[mTrainPlayer.Class - 1, 2]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 3] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  특수 마법 능력치 :\t{mTrainPlayer.SpecialMagic}[/color]", true);
						trainSkillMenuList.Add("  특수 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(3, magicEnableClass[mTrainPlayer.Class - 1, 3]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 4] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  초 자연력 능력치 :\t{mTrainPlayer.ESPMagic}[/color]", true);
						trainSkillMenuList.Add("  초 자연력 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(4, magicEnableClass[mTrainPlayer.Class - 1, 4]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 5] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  소환 마법 능력치 :\t{mTrainPlayer.SummonMagic}[/color]", true);
						trainSkillMenuList.Add("  소환 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(5, magicEnableClass[mTrainPlayer.Class - 1, 5]));
					}

					AppendText($"[color={RGB.LightGreen}] 여분의 경험치 :\t{mTrainPlayer.Experience.ToString("#,#0")}[/color]", true);

					AppendText($"[color={RGB.LightRed}]당신이 배우고 싶은 부분을 고르시오.[/color]", true);

					ShowMenu(MenuMode.ChooseTrainMagic, trainSkillMenuList.ToArray(), defaultMenuID);
				}

				void ShowChooseTrainSkillMemberMenu()
				{
					AppendText($"[color={RGB.White}]누가 훈련을 받겠습니까?[/color]");
					ShowCharacterMenu(MenuMode.ChooseTrainSkillMember, false);
				}

				void ShowChooseTrainMagicMemberMenu()
				{
					AppendText($"[color={RGB.White}]누가 가르침을 받겠습니까?[/color]");
					ShowCharacterMenu(MenuMode.ChooseTrainMagicMember, false);
				}

				bool EnoughMoneyToChangeJob()
				{
					if (mParty.Gold < 10_000)
					{
						Talk(" 그러나 일행에게는 직업을 바꿀 때 드는 비용인 금 10,000 개가 없습니다.", SpecialEventType.None);
						return false;
					}
					else
						return true;
				}

				void ShowChooseChangeSwordMemberMenu()
				{
					if (EnoughMoneyToChangeJob())
					{
						AppendText($"[color={RGB.White}]누가 전투사 계열의 직업을 바꾸겠습니까?[/color]");
						ShowCharacterMenu(MenuMode.ChooseChangeSwordMember);
					}
				}

				void ShowChooseChangeMagicMemberMenu()
				{
					if (EnoughMoneyToChangeJob())
					{

						AppendText($"[color={RGB.White}]누가 마법사 계열의 직업을 바꾸겠습니까?[/color]");
						ShowCharacterMenu(MenuMode.ChooseChangeMagicMember);
					}
				}

				void ShowHealType()
				{
					AppendText(new string[] { $"[color={RGB.White}]어떤 치료입니까?[/color]" });

					ShowMenu(MenuMode.HealType, new string[]
					{
						"상처를 치료",
						"독을 제거",
						"의식의 회복",
						"부활"
					});
				}

				async Task EndBattle()
				{
					var battleEvent = mBattleEvent;
					mBattleEvent = BattleEvent.None;

					if (mBattleTurn == BattleTurn.Win)
					{
						mBattleCommandQueue.Clear();
						mBatteEnemyQueue.Clear();

						var endMessage = "";

						if (mParty.Etc[5] == 2)
							endMessage = "";
						else
						{
#if DEBUG
							var goldPlus = 10_000;
#else
							var goldPlus = 0;
							foreach (var enemy in mEncounterEnemyList)
							{
								var enemyInfo = mEnemyDataList[enemy.ENumber];
								var point = enemyInfo.AC == 0 ? 1 : enemyInfo.AC;
								var plus = enemyInfo.Level;
								plus *= enemyInfo.Level;
								plus *= enemyInfo.Level;
								plus *= point;
								goldPlus += plus;
							}
#endif

							mParty.Gold += goldPlus;

							endMessage = $"[color={RGB.White}]일행은 {goldPlus:#,#0}개의 금을 얻었다.[/color]";

							AppendText(new string[] { endMessage, "" });
						}

						if (battleEvent == BattleEvent.Pollux) {
							Ask($"[color={RGB.LightMagenta}] 윽!! 나의 패배를 인정하겠다." +
							$"  나는 원래 암살자로 일하던[/color] [color={RGB.LightMagenta}]폴록스[/color][color={RGB.LightMagenta}]라고한다." +
							" 수련을 목적으로 당신과 동행하고 싶다. 당신은 어떤가?[/color]",
							MenuMode.JoinPollux, new string[] {
								"좋소, 허락하겠소",
								"그렇게는 않되오"
							});
						}

						mEncounterEnemyList.Clear();
						mBattleEvent = 0;

						ShowMap();

					}
					else if (mBattleTurn == BattleTurn.RunAway)
					{
						AppendText(new string[] { "" });

						mBattlePlayerID = 0;
						while (!mPlayerList[mBattlePlayerID].IsAvailable && mBattlePlayerID < mPlayerList.Count)
							mBattlePlayerID++;

						

						mEncounterEnemyList.Clear();
						ShowMap();
					}
					else if (mBattleTurn == BattleTurn.Lose)
					{
						
					}

					mBattleTurn = BattleTurn.None;
				}

				void ShowCureResult(bool battleCure)
				{
					if (battleCure)
						Talk(mCureResult.ToArray(), SpecialEventType.SkipTurn);
					else
						Dialog(mCureResult.ToArray());
				}

				void ShowWeaponTypeMenu(int weaponCategory)
				{
					mWeaponTypeID = weaponCategory;

					if (0 <= weaponCategory && weaponCategory <= 3)
					{
						AppendText($"[color={RGB.White}]어떤 무기를 원하십니까?[/color]");

						var weaponNameArr = new string[7];
						for (var i = 1; i <= 7; i++)
						{
							if (Common.GetWeaponName(mWeaponTypeID * 7 + i).Length < 3)
								weaponNameArr[i - 1] = $"{Common.GetWeaponName(mWeaponTypeID * 7 + i)}\t\t\t금 {weaponPrice[mWeaponTypeID, i - 1]:#,#0} 개";
							else if (Common.GetWeaponName(mWeaponTypeID * 7 + i).Length < 5)
								weaponNameArr[i - 1] = $"{Common.GetWeaponName(mWeaponTypeID * 7 + i)}\t\t금 {weaponPrice[mWeaponTypeID, i - 1]:#,#0} 개";
							else

								weaponNameArr[i - 1] = $"{Common.GetWeaponName(mWeaponTypeID * 7 + i)}\t금 {weaponPrice[mWeaponTypeID, i - 1]:#,#0} 개";
						}

						ShowMenu(MenuMode.BuyWeapon, weaponNameArr);
					}
					else if (weaponCategory == 4)
					{
						AppendText($"[color={RGB.White}]어떤 방패를 원하십니까?[/color]");

						var shieldNameArr = new string[5];
						for (var i = 1; i <= 5; i++)
						{
							if (Common.GetShieldName(i).Length <= 5)
								shieldNameArr[i - 1] = $"{Common.GetShieldName(i)}\t\t금 {shieldPrice[i - 1]:#,#0} 개";
							else
								shieldNameArr[i - 1] = $"{Common.GetShieldName(i)}\t금 {shieldPrice[i - 1]:#,#0} 개";
						}

						ShowMenu(MenuMode.BuyShield, shieldNameArr);
					}
					else if (weaponCategory == 5)
					{
						AppendText($"[color={RGB.White}]어떤 갑옷을 원하십니까?[/color]");

						var armorNameArr = new string[10];
						for (var i = 1; i <= 10; i++)
						{
							if (Common.GetArmorName(i).Length <= 5)
								armorNameArr[i - 1] = $"{Common.GetArmorName(i)}\t\t금 {armorPrice[i - 1].ToString("#,#0")} 개";
							else
								armorNameArr[i - 1] = $"{Common.GetArmorName(i)}\t금 {armorPrice[i - 1].ToString("#,#0")} 개";
						}

						ShowMenu(MenuMode.BuyArmor, armorNameArr);
					}
				}

				if (mLoading || (mAnimationEvent != AnimationType.None && ContinueText.Visibility != Visibility.Visible && mMenuMode == MenuMode.None) || mTriggeredDownEvent)
				{
					mTriggeredDownEvent = false;
					return;
				}
				else if (EndingMessage.Visibility == Visibility.Visible)
				{
					//EndingMessage.Visibility = Visibility.Collapsed;

					//mMapName = 6;
					//mXAxis = 90;
					//mYAxis = 79;

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

						if (specialEvent == SpecialEventType.CantTrain)
							ShowTrainSkillMenu(mMenuFocusID);
						else if (specialEvent == SpecialEventType.CantTrainMagic)
							ShowTrainMagicMenu(mMenuFocusID);
						else if (specialEvent == SpecialEventType.TrainSkill)
							ShowChooseTrainSkillMemberMenu();
						else if (specialEvent == SpecialEventType.TrainMagic)
							ShowChooseTrainMagicMemberMenu();
						else if (specialEvent == SpecialEventType.ChangeJobForSword)
							ShowChooseChangeSwordMemberMenu();
						else if (specialEvent == SpecialEventType.ChangeJobForMagic)
							ShowChooseChangeMagicMemberMenu();
						else if (specialEvent == SpecialEventType.CureComplete)
						{
							AppendText($"[color={RGB.White}]누가 치료를 받겠습니까?[/color]");

							ShowCharacterMenu(MenuMode.Hospital);
						}
						else if (specialEvent == SpecialEventType.NotCured)
						{
							ShowHealType();
						}
						else if (specialEvent == SpecialEventType.CantBuyWeapon)
							ShowWeaponTypeMenu(mWeaponTypeID);
						else if (specialEvent == SpecialEventType.CantBuyExp)
							ShowExpStoreMenu();
						else if (specialEvent == SpecialEventType.CantBuyItem)
							ShowItemStoreMenu();
						else if (specialEvent == SpecialEventType.CantBuyMedicine)
							ShowMedicineStoreMenu();
						else if (specialEvent == SpecialEventType.WizardEye)
						{
							AppendText("");
							mWizardEyeTimer.Stop();
							mWizardEyePosBlink = false;
						}
						else if (specialEvent == SpecialEventType.Penetration)
							AppendText("");
						else if (specialEvent == SpecialEventType.Telescope)
						{
							if (mTelescopeXCount != 0 || mTelescopeYCount != 0)
							{
								if ((mTelescopeXCount != 0 && (mXAxis + mTelescopeXCount <= 4 || mXAxis + mTelescopeXCount >= mMapHeader.Width - 4)) ||
									(mTelescopeXCount != 0 && (mYAxis + mTelescopeYCount <= 5 || mYAxis + mTelescopeYCount >= mMapHeader.Height - 4)))
								{
									mTelescopeXCount = 0;
									mTelescopeYCount = 0;
									return;
								}

								if (mTelescopeXCount < 0)
									mTelescopeXCount++;

								if (mTelescopeXCount > 0)
									mTelescopeXCount--;

								if (mTelescopeYCount < 0)
									mTelescopeYCount++;

								if (mTelescopeYCount > 0)
									mTelescopeYCount--;

								if (mTelescopeXCount != 0 || mTelescopeYCount != 0)
								{
									Talk($"[color={RGB.White}]천리안 사용중 ...[/color]", SpecialEventType.Telescope);
								}
								else
									AppendText("");
							}
						}
						else if (specialEvent == SpecialEventType.BackToBattleMode)
						{
							if (BattlePanel.Visibility == Visibility.Collapsed)
								DisplayEnemy();
							BattleMode();
						}
						else if (specialEvent == SpecialEventType.NextToBattleMode)
						{
							AddBattleCommand(true);
						}
						else if (specialEvent == SpecialEventType.HearAncientEvil)
						{
							Talk(new string[] {
								$"[color={RGB.LightBlue}] {mPlayerList[0].Name}, 정말 급한 일이 생겼습니다.[/color]",
								$"[color={RGB.LightBlue}] 방금 당신이 로드안에 대해 반란을 결심한 것을 로드안이 알아챘습니다." +
								"  아마 그는 천리안과 독심으로  당신을 계속  관찰했던것 같습니다." +
								"  지금 로어성에서는 당신과 배리언트 피플즈의 사람들을 제거할 용사들을 모으기 시작했습니다.  그리고 적어도 몇일후면 이곳으로 진격한다고 합니다." +
								"  당신의 능력이라면  충분히 자신을 방어할 수 있겠지만  모든 배리언트 피플즈의 사람들을 보호해 주기란 역부족 일것입니다." +
								"  그래서 한시바삐 이곳 사람들을 안전한 곳으로 이동 시켜 주었으면 합니다. 당신은 그들이 살기 좋을만하고 절대로 로드안의 투시능력이 미치지 않을 곳을 찾아 주십시요." +
								" 그리고 그 위치를 알려주시면 나머지는 제가 담당하겠습니다.[/color]"
							}, SpecialEventType.HearAncientEvil2);

						}
						else if (specialEvent == SpecialEventType.HearAncientEvil2)
						{
							mMapName = "HdsGate";
							await RefreshGame();

							Talk(new string[] {
								$"[color={RGB.LightBlue}]  지금 당신이 서있는 곳은 '하데스 게이트'라는 곳입니다. 이곳은 로어세계의 6번째 테라인 하데스 테라로  이동하는 문이  있는 곳이기도 합니다." +
								" 하데스 테라는 로어 대륙의 지하에 있으며  로드안과 나의 투시능력이  전혀 통하지 못하는 곳입니다." +
								"  당신은 이곳을 통해 하데스 테라로 간후 배리언트 피플즈의 주민들이 안전하게  살 수 있는 곳을  찾아내면 되는 것입니다. 아마 당신의 투시능력이 큰 도움을 줄것입니다." +
								"  제가 당신에게 크로매틱 크리스탈을 드리겠습니다.  당신이 가장 좋은 장소를 찾았을 때 이 크리스탈을 사용하십시요." +
								"  그리면 저의 투시력과  그 크리스탈이 서로 교감하여  나의 투시력이 그곳으로 작용할 것입니다.[/color]",
								$"[color={RGB.LightBlue}] 당신의 성공을 빌겠습니다."
							}, SpecialEventType.None);

							mParty.Crystal[7]++;
						}
						else if (specialEvent == SpecialEventType.MeetFriend)
						{
							Ask(new string[] {
								$"[color={RGB.Brown}] 드디어 만났군, {mPlayerList[0].Name}.[/color]",
								$"[color={RGB.Brown}] 로드안을 배신하겠다는 자네의 생각이 옳았다네." +
								"  우리가 자네와 헤어진후 로드안에게 갔을 때  로드안은 우리에게 멸망당한 네 종족의 원혼을 봉인하는 일을 시켰다네. 우리는 쾌히 승락하며 그 일을 해주었지." +
								" 하지만 우리의 도움이 더 이상 필요없게 되자 로드안은 그 자신을 위해  우리의 동료였던 카노푸스를  악의 전사 네크로만서로 개조하여 다른 차원으로 보내 버렸다네." +
								"  우리는 그 사실을 알아채자 로어성을 탈출하여 자네를 찾아 여기까지 왔다네.  자네와 다시 합류하여 로드안과 싸우고 싶네.[/color]"
							}, MenuMode.JoinFriend, new string[] {
								"쾌히 승락하겠네",
								"하지만 이미 늦었네"
							});
						}
						else if (specialEvent == SpecialEventType.ReadProphesy)
						{
							Talk(new string[] {
								$"[color={RGB.Yellow}]INDEX :[/color]",
								"",
								$"[color={RGB.White}]  Chapter 1. 지식의 성전[/color]",
								$"[color={RGB.White}]  Chapter 2. 다크메이지의 탄생[/color]",
								$"[color={RGB.White}]  Chapter 3. 제작자의 차원[/color]",
								$"[color={RGB.White}]  Chapter 4. 위선자의 최후[/color]",
								$"[color={RGB.White}]  Chapter 5. 이 예언의 결말[/color]"
							}, SpecialEventType.ReadProphesy1);
						}
						else if (specialEvent == SpecialEventType.ReadProphesy1)
						{
							Talk(new string[] {
								$"[color={RGB.White}]Chapter 1.[/color]",
								"",
								$" 약 150년 뒤, 지식의 성전의 북쪽 해안에 [color={RGB.LightCyan}]또다른 지식의 성전[/color]이라는 피라미드가 생긴다." +
								"그곳에서는 이 세계의 진실된 의미를 발견 할 수 있게 될 것이며  '또 다른 지식의 성전'이라는 뜻을 이해 할 수 있게 할 것이다.",
							}, SpecialEventType.ReadProphesy2);
						}
						else if (specialEvent == SpecialEventType.ReadProphesy2)
						{
							Talk(" 당신이 다음 장을 넘겼지만 Chapter 2.의 내용은 누군가에 의하여 찢겨져 나갔다.", SpecialEventType.ReadProphesy3);
						}
						else if (specialEvent == SpecialEventType.ReadProphesy3)
						{
							Talk(new string[] {
								$"[color={RGB.White}]Chapter 3.[/color]",
								"",
								" 이곳 3차원에서 4차원의 축으로 조금만 이동하면  전혀 다른 새로운 3차원의 세계가 펼쳐진다." +
								"  대부분은 텅 빈 공간 속이거나 생명체가 없는 거대한 땅덩이가 있기도 하지만 때때로 생명체가 있는 곳일 수도 있다." +
								$"  그 중 하나는 자신의 별을 스스로 [color={RGB.LightCyan}]지구[/color]라고 부르는 종족이 사는 곳이 있다." +
								" 그곳에는 이 공간의 창조자인 안영기님이  지금도 열심히 게임 시나리오와 시스템을 구상하고 있을 것이다."
							}, SpecialEventType.ReadProphesy4);
						}
						else if (specialEvent == SpecialEventType.ReadProphesy4)
						{
							Talk(new string[] {
								$"[color={RGB.White}]Chapter 4.[/color]",
								"",
								" 795년 위선자는 사라지고  로어 세계는 진정한 평화의 길로 접어든다." +
								"  이후의 세계는 안영기님이 창조하지 않아도  우리 스스로가 미래를 개척해 나가게 되고,  안영기님 역시 로어 세계에 대한 일들을 우리에게 모두 위임하고 더 이상 로어 역사에 개입하지 않게 된다."
							}, SpecialEventType.ReadProphesy5);
						}
						else if (specialEvent == SpecialEventType.ReadProphesy5)
						{
							Talk(new string[] {
								$"[color={RGB.White}]Chapter 5.[/color]",
								"",
								" 795년 이후의 미래는 더 이상 창조되지 않는다. 그때부터는 우리 스스로가 개척해 나가는 미래가 있을 뿐이다.  그리고  나의 예언서도 이것으로 끝을 맺게 된다."
							}, SpecialEventType.ReadProphesyLast);
						}
						else if (specialEvent == SpecialEventType.ReadProphesyLast)
						{
							Dialog(new string[] {
								" 그리고 마지막 장에 이렇게 써 있었다.",
								"",
								$"[color={RGB.White}] 이 책은 내가 죽은 다음 날[/color]",
								$"[color={RGB.White}]              세상에 나타 날 것이다.[/color]"
							});
						}
						else if (specialEvent == SpecialEventType.SeeMurderCase)
						{
							ClearDialog();

							mParty.Etc[0] = 10;
							UpdateView();
							InvokeAnimation(AnimationType.CaptureProtagonist);
						}
						else if (specialEvent == SpecialEventType.GotoCourt)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							InvokeAnimation(AnimationType.GotoCourt);
						}
						else if (specialEvent == SpecialEventType.Interrogation)
						{
							Dialog(new string[] {
								$"[color={RGB.LightBlue}] 이 자는 여기 이 창으로 광산업자를 찔러 죽였습니다." +
								"  우리 둘이 공교롭게도 이 자가 그를 찔렀던 이 창을 뽑아내는 것을 목격하고는 즉시 체포 했습니다." +
								" 우리들의 경해로는 광산업자와의 금전 관계로 인해  저질러진 살인이라고 보아집니다.[/color]",
								$"[color={RGB.LightBlue}] 그리고 이 창을 증거물로 제시하겠습니다.[/color]"
							});

							InvokeAnimation(AnimationType.SubmitProof);
						}
						else if (specialEvent == SpecialEventType.Punishment)
						{
							Talk(new string[] {
								" 로드안이 말했다.",
								"",
								$"[color={RGB.LightGreen}] 당신은 이곳의 법률을 알고 있겠지요?" +
								"  범죄없는 평화로운 세상을 만들기 위해  살인자는 모두 사형 또는 종신형에 처한다는 것도 알고 있을거요." +
								" 당신은 전과가 없기 때문에 사형을 선고하지는 않겠소.  그렇지만 최대한 자비를 베풀어도 종신형은 면치 못할 꺼요.[/color]"
							}, SpecialEventType.GotoJail);
						}
						else if (specialEvent == SpecialEventType.GotoJail)
						{
							ClearDialog();
							InvokeAnimation(AnimationType.GotoJail);
						}
						else if (specialEvent == SpecialEventType.LiveJail)
						{
							ClearDialog();
							InvokeAnimation(AnimationType.LiveJail);
						}
						else if (specialEvent == SpecialEventType.RequestSuppressVariantPeoples)
						{
							Dialog(" 하지만  그전에 먼저 할 일이 생겼소.  지금 배리언트 피플즈에서 대대적인 반란이 발생했는데 빨리 진압하지 않으면  다른 성으로까지 번질지도 모르오." +
							"  이번의 반란은  나에 대한 반역임과 동시에  선에 대한 도전이라고 받아들이고 있소.  그들의 이런 행위는 여지껏 지켜온 평화를 깨어 버리려는  에인션트 이블의 마지막 발악인것 같소.");

							SetBit(50);
							mParty.Etc[19]++;
						}
						else if (specialEvent == SpecialEventType.PassCrystal)
						{
							InvokeAnimation(AnimationType.PassCrystal2);
						}
						else if (specialEvent == SpecialEventType.SealCrystal)
						{
							Dialog(new string[] {
								$"[color={RGB.LightGreen}] 자, 이제 이것을 아무도 구제 할 수 없는 곳으로 보내 버려야겠소.  그곳은 바로 로어 대륙의 지하에 있는 하데스 테라라고들 하는 곳이오.[/color]"
							});

							InvokeAnimation(AnimationType.SealCrystal2);
						}
						else if (specialEvent == SpecialEventType.SendUranos)
						{
							Talk($"[color={RGB.LightGreen}] 그럼, 이제 당신을 우라누스 테라로 보내 주겠소.[/color]", SpecialEventType.ClearRoom);
						}
						else if (specialEvent == SpecialEventType.ClearRoom)
						{
							Talk($"[color={RGB.LightGreen}]  그전에 당신은 새로운 모습으로 태어날꺼요. 그러기 위해서는  당신의 일행들과 나의 신하들이 자리를 피해줘야만 하오." +
							"  그 이유는 차차 알려 드리리다.", SpecialEventType.TransformProtagonist);
						}
						else if (specialEvent == SpecialEventType.TransformProtagonist)
						{
							while (mPlayerList.Count > 1)
								mPlayerList.RemoveAt(1);

							Dialog(" 잠시간의 침묵이 흘렀다.");
							InvokeAnimation(AnimationType.TransformProtagonist);
						}
						else if (specialEvent == SpecialEventType.TransformProtagonist2)
						{
							Talk(new string[] {
								$"[color={RGB.LightGreen}] 정말 미안하게 됐네, {mPlayerList[0].Name}.[/color]",
								$"[color={RGB.LightGreen}] 당신은  여지껏 나를 위해 일해 주었는데 나는 당신에게 이렇게 할 수 밖에는 없다네. 나는  처음부터 이미 당신을  지목하고 있었지." +
								" 나와의 관계를 만들기 위해서  당신을 살인죄로 몰았던 것도 나였고,  항상 천리안과 투시로  당신을 감시하던 사람도  바로 나였다네." +
								" 지금의 당신은  나 로드안을 제외하고는 명실공히 이 세계 최강의 용사이지. 내가 지금 하려는 이야기를 잘 들어보게.[/color]"
							}, SpecialEventType.TransformProtagonist3);
						}
						else if (specialEvent == SpecialEventType.TransformProtagonist3)
						{
							Talk($"[color={RGB.LightGreen}] 이곳은 현재 너무 평화가 지속되어서 사람들이 현재의 평화를 전혀 고마워 할줄을 모른다네." +
							"  그래서 내가 새로운 일을 지금 꾸미려하지." +
							" 그 일이란 어떤 악의 추종자가 이 세계에 나타나  사람들을 위협하고 악을 퍼뜨려서 이 세계의 평화를 말살하려고 할때" +
							" 나와 내가 선택한 인물의 활약으로 세계는 다시 평화를 찾게되고  주민들은 더욱 나에게 감사하며 선을 지키려 노력한다는 내용일세." +
							" 이런 일을 내가 꾸몄다는 것을  아무도 알지못한다면 나는 분명히  평화의 구세주로서의 자리를  확보하게 된다네." +
							"  여기서 필요한 사람은  바로 위에서 말한 '악의 추종자'가 될  사람이네.  하지만 아무도 지원하지 않을거란건  뻔한 사실이지." +
							" 그래서 세계를 위협 할 만한 능력을 가진  당신을 이런식으로 끌어 들인거라네." +
							" 당신은 항상 악을 퍼뜨리려  이 세계에  발을 내리지만 번번히 나와 내가 선택한 용사에 의해 패배하게 되지. 그것이 바로 당신이 이제 지게될 운명이라네." +
							"  당신은 이제 스스로의 자유의사에 관계없이 운명에 묶여서 행동하게 될걸세. 하지만 패배를 너무 의식할 필요는 없네.  최종적인 승리는 당신이 하게 만들어 줄테니까...[/color]"
							, SpecialEventType.TransformProtagonist4);
						}
						else if (specialEvent == SpecialEventType.TransformProtagonist4)
						{
							Dialog(" 로드안은 말을 마치자  당신을 향해  백색의 가루를 뿌리며 주문을 외우기 시작했다.");
							InvokeAnimation(AnimationType.TransformProtagonist2);
						}
						else if (specialEvent == SpecialEventType.FollowSolider) {
							InvokeAnimation(AnimationType.FollowSoldier);
						}
						else if (specialEvent == SpecialEventType.LordAhnMission) {
							Talk($"[color={RGB.LightGreen}] 당신이  살인 죄로 잡혀오기 전까지는  주위 사람들에게 매우 인정받은 용감한 사람이었다는 것을 알고 있소." +
							"  내가 지금 부탁하는  몇가지를 들어 준다면 당신에게 그 댓가로 자유로운 삶을 보장하겠소.[/color]", SpecialEventType.LeaveSoldier);
						}
						else if (specialEvent == SpecialEventType.LeaveSoldier) {
							InvokeAnimation(AnimationType.LeaveSoldier);
						}
						else if (specialEvent == SpecialEventType.ConfirmPardon) {
							InvokeAnimation(AnimationType.ConfirmPardon);
						}
						else if (specialEvent == SpecialEventType.ConfirmPardon2) {
							InvokeAnimation(AnimationType.ConfirmPardon3);
						}
						else if (specialEvent == SpecialEventType.JoinCanopus) {
							InvokeAnimation(AnimationType.JoinCanopus);
						}
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
							int moveX = mXAxis;
							int moveY = mYAxis;

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

							if (moveX < 4 || moveX > mMapHeader.Width - 4 || moveY < 4 || moveY > mMapHeader.Height - 4)
								AppendText("공간 이동이 통하지 않습니다.");
							else
							{
								var valid = false;
								if (mMapHeader.TileType == PositionType.Town)
								{
									if (27 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}
								else if (mMapHeader.TileType == PositionType.Ground)
								{
									if (24 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}
								else if (mMapHeader.TileType == PositionType.Den)
								{
									if (41 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}
								else if (mMapHeader.TileType == PositionType.Keep)
								{
									if (27 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}

								if (!valid)
									AppendText("공간 이동 장소로 부적합 합니다.");
								else
								{
									mMagicPlayer.SP -= 50;

									if (GetTileInfo(moveX, moveY) == 0 || ((mMapHeader.TileType == PositionType.Den || mMapHeader.TileType == PositionType.Keep) && GetTileInfo(moveX, moveY) == 52))
										AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신을 배척합니다.[/color]");
									else
									{
										mXAxis = moveX;
										mYAxis = moveY;

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
						}, SpecialEventType.TrainSkill);
					}

					void ShowTrainMagicMessage()
					{
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 마법 학교 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 충분한 실전 경험을 쌓았다면, 당신은 더욱 능숙하게 마법을 다룰 것입니다.[/color]",
						}, SpecialEventType.TrainMagic);
					}

					void ShowChangeJobForSwordMessage()
					{
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 군사 훈련소 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 원한다면 새로운 계급으로 바꿀 수가 있습니다.[/color]",
						}, SpecialEventType.ChangeJobForSword);
					}

					void ShowChangeJobForMagicMessage()
					{
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 마법 학교 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 원한다면 새로운 계급으로 바꿀 수가 있습니다.[/color]",
						}, SpecialEventType.ChangeJobForMagic);
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
							var specialEvent = SpecialEventType.None;
							if (menuMode == MenuMode.ChooseBattleCureSpell)
								specialEvent = SpecialEventType.BackToBattleMode;

							Talk("당신은 치료 마법을 사용할 능력이 없습니다.", specialEvent);
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

					void AfterJoinFriendEvent() {
						AddNextTimeEvent(4, 30);
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
						if (mMenuMode == MenuMode.BattleStart || mMenuMode == MenuMode.BattleCommand || mMenuMode == MenuMode.MeetPollux)
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
							menuMode == MenuMode.CastSummon ||
							menuMode == MenuMode.ChooseItemType ||
							menuMode == MenuMode.ChooseCrystal)
							{
								BattleMode();
							}
							else if (menuMode == MenuMode.ChooseESPMagic)
							{
								ShowCastESPMenu();
							}
							else if (menuMode == MenuMode.EnemySelectMode)
							{
								if (mUseCrystalID == 2 || mUseCrystalID == 3) {
									var enemy = mEncounterEnemyList[mEnemyFocusID];
									if (enemy.Name == "로드 안") {
										Talk($" {mPlayerList[mBattlePlayerID].NameSubjectJosa} 로드안을 향해 다크 크리스탈과 에보니 크리스탈을 동시에 사용하였다." +
										"크리스탈에서 뿜어져 나온  검은 기운은  금새 로드안에게 침투해 들어갔고 그는 순식간에 백여년을 늙어 버렸다", SpecialEventType.NextToBattleMode);

										enemy.Strength /= 2;
										enemy.Mentality /= 2;
										enemy.Endurance /= 2;
										enemy.Resistance[0] /= 2;
										enemy.Resistance[1] /= 2;
										enemy.Agility /= 2;
										enemy.Accuracy[0] /= 2;
										enemy.Accuracy[1] /= 2;
										enemy.AC /= 2;
										enemy.Level /= 2;

										if (enemy.HP > enemy.Endurance * enemy.Level * 10)
										{
											enemy.HP = enemy.Endurance * enemy.Level * 10;
											DisplayEnemy();
										}


										mParty.Crystal[2]--;
										mParty.Crystal[3]--;
									}
									else {
										Talk(" 이 크리스탈은  선의 힘을 사용하는 적에게만 반응한다.", SpecialEventType.NextToBattleMode);
									}

									mUseCrystalID = -1;
								}
								else if (mUseCrystalID == 4) {
									mUseCrystalID = -1;

									var enemy = mEncounterEnemyList[mEnemyFocusID];
									if (enemy.HP > 1) {
										Talk(" 당신이  영혼의 크리스탈을  적에게 사용하자 적의 영혼은 크리스탈에 의해 저주를 받아  황폐화 되어졌다.", SpecialEventType.NextToBattleMode);
										enemy.AuxHP = (enemy.AuxHP + 1) / 2;
										enemy.HP /= 2;

										DisplayEnemy();
										mParty.Crystal[4]--;
									}
									else {
										Talk(" 하지만 크리스탈은 전혀 반응을 보이지 않았다.", SpecialEventType.NextToBattleMode);
									}
								}
								else {
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
							}
							else if (menuMode == MenuMode.ApplyBattleCureSpell || menuMode == MenuMode.ApplyBattleCureAllSpell)
								ShowCureDestMenu(mPlayerList[mBattlePlayerID], MenuMode.ChooseBattleCureSpell);
							else if (menuMode == MenuMode.ConfirmExitMap)
							{
								ClearDialog();

								mXAxis = mPrevX;
								mYAxis = mPrevY;
							}
							else if (menuMode == MenuMode.JoinCanopus)
								InvokeAnimation(AnimationType.LeaveCanopus);
							
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
									Talk(result, SpecialEventType.BattleUseItem);
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

							if (mMapHeader.Width <= 100)
							{
								xInit = 0;
								width = mMapHeader.Width;
							}
							else
							{
								xInit = mXAxis - 50;

								if (xInit <= 0)
								{
									xInit = 0;

									if (mMapHeader.Width > 100)
										width = 100;
									else
										width = mMapHeader.Width;
								}
								else if (xInit + 100 > mMapHeader.Width)
								{
									if (mMapHeader.Width > 100)
									{
										xInit = mMapHeader.Width - 100;
										width = 100;
									}
									else
									{
										xInit = 0;
										width = mMapHeader.Width;
									}
								}
								else
									width = 100;
							}

							if (mMapHeader.Height <= 80)
							{
								yInit = 0;
								height = mMapHeader.Height;
							}
							else
							{
								yInit = mYAxis - 40;

								if (yInit <= 0)
								{
									yInit = 0;

									if (mMapHeader.Height > 80)
										height = 80;
									else
										height = mMapHeader.Height;
								}
								else if (yInit + 80 > mMapHeader.Height)
								{
									if (mMapHeader.Height > 80)
									{
										yInit = mMapHeader.Height - 80;
										height = 80;
									}
									else
									{
										yInit = 0;
										height = mMapHeader.Height;
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
										if (mXAxis == x && mYAxis == y)
										{
											mWizardEyePosX = offset % width;
											mWizardEyePosY = offset / width;
										}

										var tileInfo = GetTileInfo(x, y);
										if (mMapHeader.TileType == PositionType.Town)
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
										else if (mMapHeader.TileType == PositionType.Ground)
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
												//if (mMapName == 4)
												//	mWizardEye.Data[offset] = WHITE;
												//else
													mWizardEye.Data[offset] = BLACK;
											}
											else if (24 <= tileInfo && tileInfo <= 47)
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTGREEN;

										}
										else if (mMapHeader.TileType == PositionType.Den)
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
												//if (mMapName == 2)
												//	mWizardEye.Data[offset] = LIGHTBLUE;
												//else
													mWizardEye.Data[offset] = BLACK;
											}
											else if (tileInfo == 0 || (41 <= tileInfo && tileInfo <= 47))
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTMAGENTA;
										}
										else if (mMapHeader.TileType == PositionType.Keep)
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

							Talk(" 주지사의 눈을 통해 이 지역을 바라보고 있다.", SpecialEventType.WizardEye);
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
										Talk(" 강한 치료 마법은 아직 불가능 합니다.", SpecialEventType.BackToBattleMode);
									else
										Dialog(" 강한 치료 마법은 아직 불가능 합니다.");
								}
							}
						}

						void Teleport(MenuMode newMenuMode)
						{
							if ((mMapHeader.HandicapBit & 8) > 0)
								AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 공간 이동을 방해 합니다.[/color]");
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
									var specialEvent = SpecialEventType.None;
									if (menuMode == MenuMode.ChooseBattleCureSpell)
										specialEvent = SpecialEventType.BackToBattleMode;

									Talk("강한 치료 마법은 아직 불가능 합니다.", specialEvent);
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
								if ((mMapHeader.HandicapBit & 4) > 0)
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
								if ((mMapHeader.HandicapBit & 1) > 0)
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
								if ((mMapHeader.HandicapBit & 2) > 0)
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

							var newX = mXAxis + 2 * xOffset;
							var newY = mYAxis + 2 * yOffset;

							if (newX < 4 || newX >= mMapHeader.Width - 4 || newY < 5 || newY >= mMapHeader.Height - 4)
								return;

							var canMove = false;

							var moveTile = GetTileInfo(newX, newY);
							switch (mMapHeader.TileType)
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

								if (GetTileInfo(mXAxis + xOffset, mYAxis + yOffset) == 0 ||
									((mMapHeader.TileType == PositionType.Den || mMapHeader.TileType == PositionType.Keep) && (GetTileInfo(newX, newY) == 52)))
								{
									AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]");
								}
								else
								{
									mXAxis = newX;
									mYAxis = newY;

									AppendText($"[color={RGB.White}]기화 이동을 마쳤습니다.[/color]");

									if (GetTileInfo(mXAxis, mYAxis) == 0)
										InvokeSpecialEvent(mXAxis, mYAxis);
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

							var newX = mXAxis + xOffset;
							var newY = mYAxis + yOffset;


							mMagicPlayer.SP -= 30;
							DisplaySP();

							if (GetTileInfo(newX, newY) == 0 ||
									((mMapHeader.TileType == PositionType.Den || mMapHeader.TileType == PositionType.Keep) && GetTileInfo(newX, newY) == 52) ||
									(mMapHeader.TileType == PositionType.Town && GetTileInfo(newX, newY) == 48))
								AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]");
							else
							{
								byte tile;

								switch (mMapHeader.TileType)
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

							var newX = mXAxis + xOffset;
							var newY = mYAxis + yOffset;

							var range = xOffset == 0 ? 5 : 4;

							mMagicPlayer.SP -= 60;
							DisplaySP();

							byte tile;

							switch (mMapHeader.TileType)
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
								if (GetTileInfo(mXAxis + xOffset * i, mYAxis + yOffset * i) == 0 ||
										((mMapHeader.TileType == PositionType.Den || mMapHeader.TileType == PositionType.Keep) && GetTileInfo(mXAxis + xOffset * i, mYAxis + yOffset * i) == 52) ||
										(mMapHeader.TileType == PositionType.Town && GetTileInfo(mXAxis + xOffset * i, mYAxis + yOffset * i) == 48))
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
								rangeItems.Add(new Tuple<string, int>($"[color={RGB.White}]##[/color] [color={RGB.LightGreen}]{i * 1_000:#,#0}[/color] [color={RGB.White}] 공간 이동력[/color]", i));
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
									Talk($"[color={RGB.White}]일행은 주위를 투시하고 있다.[/color]", SpecialEventType.Penetration);
									mMagicPlayer.SP -= 10;
									DisplaySP();
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

							Talk($"[color={RGB.White}]천리안 사용중 ...[/color]", SpecialEventType.Telescope);
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
								mCruel = true;
							else
								mCruel = false;
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
								Talk($" 이 무기는 {player.Name}에게는 맞지 않습니다.", SpecialEventType.CantBuyWeapon);
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
								Talk($" {player.NameSubjectJosa} 이 방패를 사용 할 수 없습니다.", SpecialEventType.CantBuyWeapon);
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
								Talk($" {player.NameSubjectJosa} 이 갑옷을 사용 할 수 없습니다.", SpecialEventType.CantBuyWeapon);
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
								Talk(" 일행은 충분한 금이 없었다.", SpecialEventType.CantBuyExp);
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
								Talk(" 당신에게는 이것을 살 돈이 없습니다.", SpecialEventType.CantBuyItem);
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
								Talk(" 당신에게는 이것을 살 돈이 없습니다.", SpecialEventType.CantBuyMedicine);
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
									payment = (int)Math.Round(payment * (double)mCurePlayer.Level / 10) + 1;

									if (mParty.Gold < payment)
										ShowNotEnoughMoney(mSpecialEvent = SpecialEventType.NotCured);
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.HP = mCurePlayer.Endurance * mCurePlayer.Level * 10;

										DisplayHP();

										Talk($"[color={RGB.White}]{mCurePlayer.GenderPronoun}의 모든 건강이 회복되었다[/color]", SpecialEventType.CureComplete);
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

										Talk($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 독이 제거 되었습니다[/color]", SpecialEventType.CureComplete);
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

										Talk($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 의식을 차렸습니다[/color]", SpecialEventType.CureComplete);
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

										Talk($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 다시 살아났습니다[/color]", SpecialEventType.CureComplete);
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

							if (mTrainSkillList[mMenuFocusID].Item2 == 0)
							{
								Talk("당신의 계급과 맞지 않습니다", SpecialEventType.CantTrainMagic);
								return;
							}

							if (skill >= mTrainSkillList[mMenuFocusID].Item2)
							{
								Talk("이 분야는 더 배울 것이 없습니다", SpecialEventType.CantTrain);
								return;
							}

							var needExp = 15 * skill * skill;

							if (needExp > mTrainPlayer.Experience)
							{
								Talk("아직 경험치가 모자랍니다", SpecialEventType.CantTrain);
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

							if (mTrainSkillList[mMenuFocusID].Item2 == 0)
								Talk("당신의 계급과 맞지 않습니다", SpecialEventType.CantTrainMagic);
							else if (skill >= mTrainSkillList[mMenuFocusID].Item2)
								Talk("이 분야는 더 배울 것이 없습니다", SpecialEventType.CantTrainMagic);
							else
							{

								var needExp = 15 * skill * skill;

								if (needExp > mTrainPlayer.Experience)
									Talk("아직 경험치가 모자랍니다", SpecialEventType.CantTrainMagic);
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
								mXAxis = mMapHeader.ExitX;
								mYAxis = mMapHeader.ExitY;

								mMapName = mMapHeader.ExitMap;

								await RefreshGame();
							}
							else
							{
								ClearDialog();

								mXAxis = mPrevX;
								mYAxis = mPrevY;
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
								var hasCrystal = false;
								for (var i = 0; i < 7; i++) {
									if (mParty.Crystal[i] > 0)
									{
										hasCrystal = true;
										break;
									}
								}

								if (hasCrystal) {
									ShowMenu(MenuMode.ChooseItemType, new string[] {
										"약초나 약물을 사용한다",
										"크리스탈을 사용한다"
									});
								}
								else
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
						else if (menuMode == MenuMode.ChooseItemType) {
							if (mMenuFocusID == 0)
								UseItem(mPlayerList[mBattlePlayerID], true);
							else
							{
								mItemUsePlayer = mPlayerList[mBattlePlayerID];
								mUsableItemIDList.Clear();
								var crystalNameList = new List<string>();
								for (var i = 0; i < 7; i++) {
									if (mParty.Crystal[i] > 0)
									{
										crystalNameList.Add(Common.GetMagicName(7, i + 1));
										mUsableItemIDList.Add(i);
									}
								}

								Ask("당신이 사용할 크리스탈을 고르시오.", MenuMode.ChooseCrystal, crystalNameList.ToArray());
							}
						}
						else if (menuMode == MenuMode.ChooseCrystal) {
							var crystalID = mUsableItemIDList[mMenuFocusID];

							if (crystalID == 0) {
								mBattleCommandID = 1;
								mBattleToolID = 11;
								SelectEnemy();

								mParty.Crystal[crystalID]--;

								mUseCrystalID = -1;
							}
							else if (crystalID == 1) {
								mBattleCommandID = 2;
								mBattleToolID = 11;
								mEnemyFocusID = -1;

								mParty.Crystal[crystalID]--;

								mUseCrystalID = -1;

								AddBattleCommand();
							}
							else if (crystalID == 2 || crystalID == 3) {
								if (mParty.Crystal[2] > 0 && mParty.Crystal[3] > 0) {
									mUseCrystalID = crystalID;
									SelectEnemy();
								}
								else {
									Talk(" 크리스탈은 전혀 반응하지 않았다.", SpecialEventType.NextToBattleMode);
									mUseCrystalID = -1;
								}
							}
							else if (crystalID == 4) {
								mUseCrystalID = crystalID;
								SelectEnemy();
							}
							else if (crystalID == 5) {
								mBattleCommandID = 6;
								mBattleToolID = 11;

								mParty.Crystal[5]--;

								AddBattleCommand();
							}
							else if (crystalID == 6) {
								foreach (var player in mPlayerList) {
									if (player.Dead == 0) {
										player.Unconscious = 0;
										player.HP = player.Endurance * player.Level * 10;
									}
									else {
										player.Dead = 0;
										player.Unconscious = 0;
										player.HP = 1;
									}
								}

								if (mAssistPlayer != null) {
									if (mAssistPlayer.Dead == 0)
									{
										mAssistPlayer.Unconscious = 0;
										mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
									}
									else
									{
										mAssistPlayer.Dead = 0;
										mAssistPlayer.Unconscious = 0;
										mAssistPlayer.HP = 1;
									}

									Talk(" 에너지 크리스탈은 강한 에너지를  우리 대원들의 몸속으로  방출하였고  그 에너지를 취한 대원들은 모두 원래의 기운을 되찾았다.", SpecialEventType.NextToBattleMode);

									mParty.Crystal[6]--;
								}
							}
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
									
								}
							}
							else
							{
								AppendText(new string[] { "" });
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
							mMapHeader.StartX = mXAxis;
							mMapHeader.StartY = mYAxis;

							var saveData = new SaveData()
							{
								PlayerList = mPlayerList,
								AssistPlayer = mAssistPlayer,
								Party = mParty,
								MapHeader = mMapHeader,
								Encounter = mEncounter,
								MaxEnemy = mMaxEnemy,
								Cruel = mCruel,
								SaveTime = DateTime.Now.Ticks
							};

							var saveJSON = JsonConvert.SerializeObject(saveData);

							var idStr = "";
							if (mMenuFocusID > 0)
								idStr = mMenuFocusID.ToString();

							var storageFolder = ApplicationData.Current.LocalFolder;
							var saveFile = await storageFolder.CreateFileAsync($"mysticSave{idStr}.dat", CreationCollisionOption.ReplaceExisting);
							await FileIO.WriteTextAsync(saveFile, saveJSON);

							var users = await User.FindAllAsync();
							var gameSaveTask = await GameSaveProvider.GetForUserAsync(users[0], "00000000-0000-0000-0000-00007e5a3fb0");

							if (gameSaveTask.Status == GameSaveErrorStatus.Ok)
							{
								var gameSaveProvider = gameSaveTask.Value;

								var gameSaveContainer = gameSaveProvider.CreateContainer("MysticSaveContainer");

								var buffer = Encoding.UTF8.GetBytes(saveJSON);

								var writer = new DataWriter();
								writer.WriteUInt32((uint)buffer.Length);
								writer.WriteBytes(buffer);
								var dataBuffer = writer.DetachBuffer();

								var blobsToWrite = new Dictionary<string, IBuffer>();
								blobsToWrite.Add($"mysticSave{idStr}", dataBuffer);

								var gameSaveOperationResult = await gameSaveContainer.SubmitUpdatesAsync(blobsToWrite, null, "MysticSave");
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
									Talk(message, SpecialEventType.None, true);
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
						else if (menuMode == MenuMode.JoinFriend) {
							if (mMenuFocusID == 0) {
								// 이름 추가
								SetBit(49);

								AfterJoinFriendEvent();
							}
						}
						else if (menuMode == MenuMode.JoinFriend)
							AfterJoinFriendEvent();
						else if (menuMode == MenuMode.JoinAlcor)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var alcor = new Lore()
									{
										Name = "알코르",
										Gender = GenderType.Male,
										Class = 3,
										ClassType = ClassCategory.Sword,
										Level = 2,
										Strength = 15,
										Mentality = 5,
										Concentration = 6,
										Endurance = 12,
										Resistance = 15,
										Agility = 14,
										Accuracy = 16,
										Luck = 12,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 3,
										Shield = 0,
										Armor = 1,
										PotentialAC = 2,
										SwordSkill = 40,
										AxeSkill = 0,
										SpearSkill = 0,
										BowSkill = 0,
										ShieldSkill = 0,
										FistSkill = 10
									};

									alcor.HP = alcor.Endurance * alcor.Level * 10;
									alcor.UpdatePotentialExperience();
									UpdateItem(alcor);

									mPlayerList.Add(alcor);
									DisplayPlayerInfo();

									UpdateTileInfo(21, 49, 44);
									SetBit(36);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
								}
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.JoinMizar)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var mizar = new Lore()
									{
										Name = "미자르",
										Gender = GenderType.Male,
										Class = 2,
										ClassType = ClassCategory.Sword,
										Level = 2,
										Strength = 13,
										Mentality = 6,
										Concentration = 8,
										Endurance = 15,
										Resistance = 16,
										Agility = 16,
										Accuracy = 15,
										Luck = 14,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 2,
										Shield = 1,
										Armor = 2,
										PotentialAC = 2,
										SwordSkill = 20,
										AxeSkill = 10,
										SpearSkill = 5,
										BowSkill = 0,
										ShieldSkill = 10,
										FistSkill = 10
									};

									mizar.HP = mizar.Endurance * mizar.Level * 10;
									mizar.UpdatePotentialExperience();
									UpdateItem(mizar);

									mPlayerList.Add(mizar);
									DisplayPlayerInfo();

									UpdateTileInfo(21, 53, 44);
									SetBit(37);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
								}
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.JoinAntaresJr)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var antaresJr = new Lore()
									{
										Name = "안타레스 Jr.",
										Gender = GenderType.Male,
										Class = 4,
										ClassType = ClassCategory.Magic,
										Level = 15,
										Strength = 10,
										Mentality = 20,
										Concentration = 20,
										Endurance = 15,
										Resistance = 17,
										Agility = 19,
										Accuracy = 17,
										Luck = 18,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 0,
										Shield = 0,
										Armor = 2,
										PotentialAC = 0,
										AttackMagic = 60,
										PhenoMagic = 30,
										CureMagic = 30,
										SpecialMagic = 0,
										ESPMagic = 0,
										SummonMagic = 0
									};

									antaresJr.HP = antaresJr.Endurance * antaresJr.Level * 10;
									antaresJr.UpdatePotentialExperience();
									UpdateItem(antaresJr);

									mPlayerList.Add(antaresJr);
									DisplayPlayerInfo();

									UpdateTileInfo(17, 37, 47);
									SetBit(38);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군.  다음 기회를 기다리도록 하지.");
								}
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.JoinCanopus)
						{
							if (mMenuFocusID == 0)
								InvokeAnimation(AnimationType.RequestPardon);
							else
								InvokeAnimation(AnimationType.LeaveCanopus);
						}
						else if (menuMode == MenuMode.MeetPollux) {
							if (mMenuFocusID == 0)
							{
								mParty.Gold = 0;
								Dialog(" 당신은 강도에게 모든 돈을 빼았겼다.");

								mEncounterEnemyList.Clear();
								ShowMap();
							}
							else {
								mBattleEvent = BattleEvent.Pollux;
								StartBattle(false);
							}
						}
						else if (menuMode == MenuMode.JoinPollux) {
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5) {
									var pollux = new Lore()
									{
										Name = "폴록스",
										Gender = GenderType.Female,
										Class = 6,
										ClassType = ClassCategory.Sword,
										Level = 5,
										Strength = 15,
										Mentality = 19,
										Concentration = 20,
										Endurance = 11,
										Resistance = 18,
										Agility = 20,
										Accuracy = 17,
										Luck = 10,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 18,
										Shield = 0,
										Armor = 1,
										PotentialAC = 2,
										SwordSkill = 10,
										AxeSkill = 0,
										SpearSkill = 35,
										BowSkill = 10,
										ShieldSkill = 0,
										FistSkill = 20
									};

									pollux.HP = pollux.Endurance * pollux.Level * 10;
									pollux.UpdatePotentialExperience();
									UpdateItem(pollux);

									mPlayerList.Add(pollux);
									DisplayPlayerInfo();

									SetBit(45);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군.  다음 기회를 기다리지.");
								}
							}
							else
								ShowNoThanks();
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

		private void ShowMap()
		{
			BattlePanel.Visibility = Visibility.Collapsed;
			canvas.Visibility = Visibility.Visible;
		}

		private void HideMap()
		{
			BattlePanel.Visibility = Visibility.Visible;
			canvas.Visibility = Visibility.Collapsed;
		}

		bool IsUsableWeapon(Lore player, int weapon)
		{
			if (player.ClassType == ClassCategory.Magic)
				return false;
			else
			{
				if ((player.Class == 1 || player.Class == 2 || player.Class == 3 || player.Class == 6 || player.Class == 7) && 1 <= weapon && weapon <= 7)
					return true;
				else if ((player.Class == 1 || player.Class == 2 || player.Class == 4) && 8 <= weapon && weapon <= 14)
					return true;
				else if ((player.Class == 1 || player.Class == 2 || player.Class == 4 || player.Class == 6 || player.Class == 7) && 15 <= weapon && weapon <= 21)
					return true;
				else if ((player.Class == 1 || player.Class == 4 || player.Class == 6 || player.Class == 7) && 22 <= weapon && weapon <= 28)
					return true;
				else
					return false;
			}
		}

		private bool IsUsableShield(Lore player)
		{
			if (player.ClassType == ClassCategory.Magic)
				return false;
			else
			{
				if (player.Class == 1 || player.Class == 2 || player.Class == 3 || player.Class == 7)
					return true;
				else
					return false;
			}
		}

		private bool IsUsableArmor(Lore player, int armor)
		{
			if (player.ClassType == ClassCategory.Magic && armor == 1)
				return true;
			else if (player.ClassType == ClassCategory.Sword && ((1 <= armor && armor <= 10) || armor == 255))
				return true;
			else
				return false;
		}

		private void UpdateItem(Lore player)
		{
			var weaponData = new int[,,] {
						{
							{ 15, 15, 15, 15, 15, 25, 15 },
							{ 30, 30, 25, 25, 25, 25, 30 },
							{ 35, 40, 35, 35, 35, 35, 40 },
							{ 45, 48, 50, 40, 40, 40, 40 },
							{ 50, 55, 60, 50, 50, 50, 55 },
							{ 60, 70, 70, 60, 60, 60, 65 },
							{ 70, 70, 80, 70, 70, 70, 70 }
						},
						{
							{ 15, 15, 15, 15, 15, 15, 15 },
							{ 35, 30, 30, 37, 30, 30, 30 },
							{ 35, 40, 35, 35, 35, 35, 35 },
							{ 52, 45, 45, 45, 45, 45, 45 },
							{ 60, 60, 55, 55, 55, 55, 55 },
							{ 75, 70, 70, 70, 70, 70, 70 },
							{ 80, 85, 80, 80, 80, 80, 80 }
						},
						{
							{ 10, 10, 10, 25, 10, 20, 10 },
							{ 35, 40, 35, 35, 35, 35, 40 },
							{ 35, 30, 30, 35, 30, 30, 30 },
							{ 40, 40, 40, 45, 40, 40, 40 },
							{ 60, 60, 60, 60, 60, 60, 60 },
							{ 80, 80, 80, 80, 80, 80, 80 },
							{ 90, 90, 90, 90, 90, 90, 90 }
						},
						{
							{ 10, 10, 10, 15, 10, 15, 10 },
							{ 10, 10, 10, 10, 10, 20, 10 },
							{ 20, 20, 20, 27, 20, 20, 20 },
							{ 35, 35, 35, 40, 35, 38, 35 },
							{ 45, 45, 45, 55, 45, 45, 45 },
							{ 55, 55, 55, 65, 55, 55, 55 },
							{ 70, 70, 70, 85, 70, 70, 70 }
						}
					};

			if (IsUsableWeapon(player, player.Weapon))
			{
				if (player.Weapon > 0)
				{
					int sort = (player.Weapon - 1) / 7;
					int order = player.Weapon % 7;
					player.WeaPower = weaponData[sort, order, player.Class - 1];
				}
				else
					player.WeaPower = 5;
			}

			if (IsUsableShield(player))
				player.ShiPower = player.Shield;
			else
				player.ShiPower = 0;

			if (IsUsableArmor(player, player.Armor))
			{
				player.ArmPower = player.Armor;
				if (player.Armor == 255)
					player.ArmPower = 20;
			}
			else
				player.ArmPower = 0;

			player.AC = player.PotentialAC + player.ArmPower;
		}

		private async Task<bool> InvokeSpecialEvent(int prevX, int prevY)
		{
			var triggered = true;

			void FindGold(int bit, int gold)
			{
				if (!GetBit(bit))
				{
					AppendText($"당신은 금화 {gold:#,#0}개를 발견했다.");
					mParty.Gold += gold;
					SetBit(bit);
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

			if (mMapName == "Menace")
			{
				if (mXAxis == 13 && mYAxis == 26)
					FindGold(138, 1_000);
				else if (mXAxis == 5 && mYAxis == 38)
					FindGold(139, 1_500);
				else if (mXAxis == 41 && mYAxis == 35)
					FindGold(140, 2_000);
				else if (mXAxis == 43 && mYAxis == 40)
					FindGold(141, 500);
				else if (mXAxis == 26 && mYAxis == 5)
				{
					Talk(new string[] {
						" 당신은 여기서 밀랍으로 봉인된 작은 상자를 발견하였다. 조심해서 열어보니 예언서 한 권이 그 속에 들어 있었다.",
						" 당신은 호기심에 그 책을 펼쳤다."
					}, SpecialEventType.ReadProphesy);
				}
				else if (mYAxis == 44)
				{
					if (GetBit(0))
						ShowExitMenu();
					else
					{
						Talk(" 당신이 광산을 나가려 했을때  당신은  당신앞에 벌어져 있는 광경을 보고 섬뜩함을 느꼈다. 조금 전에 당신에게 일당을 주었던 그 광산업자가 창에 찔린 채로 쓰러져 있었다." +
						"  당신은 그 자에게로 다가갔다.  그는 창으로 심장을 관통 당한채 쓰러져 있었다. 나는 그 창이 낯에 익어서 그 창을 뽑아 쥐었다.  그 창은 바로 내가 사냥용으로 쓰던 나의 창이었다."
						, SpecialEventType.SeeMurderCase);

					}
				}
				else
					triggered = false;
			}
			else if (mMapName == "Lore")
			{
				if (mXAxis == 43 && mYAxis == 9 && mParty.Etc[19] == 0)
				{
					InvokeAnimation(AnimationType.ComeSoldier);
				}
				else if (mXAxis == 45 && mYAxis == 9 && !GetBit(35) && !GetBit(50) && mPlayerList.Count < 5)
				{
					InvokeAnimation(AnimationType.VisitCanopus);
				}
				else if (mYAxis == 94)
					ShowExitMenu();
				else
					triggered = false;
			}
			else if (mMapName == "Ground1") {
				if (mXAxis == 29 && mYAxis == 19 && !GetBit(51)) {
					mEncounterEnemyList.Clear();

					var enemy = JoinEnemy(35);
					enemy.Name = "폴록스";

					for (var i = 0; i < 6; i++) {
						var others = JoinEnemy(33);
						others.Name = "강도";
					}

					DisplayEnemy();
					HideMap();

					Ask(new string[] {
						" 당신이 길을 가던 도중에 갑자기 강도가 나타났다.  그리고 제일 앞에 있는 여자 강도가 말을 꺼냈다.",
						"",
						$"[color={RGB.LightMagenta}] 이런 곳에 겁도 없이 돌아다니다니, 하하하.[/color]",
						$"[color={RGB.LightMagenta}] 우리 7 명의 능력을 당해내지 못할 것 같으면 순순히 가진 돈을 다 내놓아라.[/color]"
					}, MenuMode.MeetPollux, new string[] {
						"가진 돈을 모두 준다",
						"선제 공격을 가한다"
					});

					SetBit(51);
				}
			}
			

			return triggered;
		}

		private void ShowExitMenu()
		{
			AppendText(new string[] { $"[color={RGB.LightCyan}]여기서 나가기를 원합니까?[/color]" });

			ShowMenu(MenuMode.ConfirmExitMap, new string[] {
				"예, 그렇습니다.",
				"아니오, 원하지 않습니다."});
		}

		private void TalkMode(int moveX, int moveY, VirtualKey key = VirtualKey.None)
		{
			void ShowClassTrainingMenu()
			{
				AppendText("어떤 일을 원하십니까?");

				ShowMenu(MenuMode.TrainingCenter, new string[] {
					"전투사 계열의 기술을 습득",
					"마법사 계열의 능력을 습득",
					"전투사 계열의 계급을 바꿈",
					"마법사 계열의 계급을 바꿈"
				});
			}

			void ShowHospitalMenu()
			{
				Talk($"[color={RGB.White}]여기는 병원입니다.[/color]", SpecialEventType.CureComplete);
			}

			void ShowGroceryMenu()
			{
				AppendText(new string[] {
					$"[color={RGB.White}]여기는 식료품점 입니다.[/color]",
					$"[color={RGB.White}]몇개를 원하십니까?[/color]",
				});

				ShowMenu(MenuMode.ChooseFoodAmount, new string[] {
					"10인분 : 금 100개",
					"20인분 : 금 200개",
					"30인분 : 금 300개",
					"40인분 : 금 400개",
					"50인분 : 금 500개"
				});
			}

			if (mMapName == "Menace") {
				if (moveX == 26 && moveY == 31)
				{
					Dialog(new string[] {
						$" 오, {mPlayerList[0].Name}.",
						" 오늘 일도 다 끝냈군요. 자, 여기 일당 받으시오.",
						"",
						$"[color={RGB.LightCyan}] [[ 황금 + 4 ][/color]"
					});

					mParty.Gold += 4;

					InvokeAnimation(AnimationType.LeftManager);
				}
				else if (moveX == 23 && moveY == 33)
					Dialog(" 당신이 여기 나타나다니 정말 뻔뻔스럽군. 사람을 죽여 놓고도 태연한척 하다니...");
				else if (moveX == 34 && moveY == 40)
					Dialog(" 몇 개월 전의 사건은 잊어버리게. 이곳에 있는 대부분의 광부들은  자네가 살인을 했다고는 믿지 않네. 단지 자네가 약간 운이 나빴던거지");
				else if (moveX == 14 && moveY == 35)
					Dialog(" 내가 전해들은 전설에 의하면  로어 대륙 지하에 하데스 테라라는 지하 대륙이 있다고 들었다네. 하지만 전설인데 뭐.");
				else if (moveX == 26 && moveY == 25) {
					Dialog(" 자네 이것 좀 봐. 이건 내가 오래 전에 광산 일을 하면서 발견한 건데 이상한 점이 있어. 이건 어떤 형식의 표음 문자가 적힌 석판인데 공교롭게도 30만년 전 지층에서 나왔다네." +
					" 그리고  이건 더 오래된 지층에서 나온  공룡의 머리뼈인데  마법으로 관통 당한 구멍이 선명하게 나타나 있다네.  공룡과 인류는 서로 다른 시대를 살았는데 이건 정말 이상하지 않은가 ?");
				}
			}
			else if (mMapName == "Lore") {
				if ((moveX == 90 && moveY == 64) || (moveX == 93 && moveY == 67) || (moveX == 86 && moveY == 72))
					ShowGroceryMenu();
				else if ((moveX == 7 && moveY == 70) || (moveX == 13 && moveY == 68) || (moveX == 13 && moveY == 72))
					ShowWeaponShopMenu();
				else if ((moveX == 85 && moveY == 11) || (moveX == 86 && moveY == 13))
					ShowHospitalMenu();
				else if (moveX == 15 && moveY == 63)
					ShowItemStoreMenu();
				else if ((moveX == 20 && moveY == 11) || (moveX == 24 && moveY == 12))
					ShowClassTrainingMenu();
				else if (moveX == 11 && moveY == 12)
					ShowExpStoreMenu();
				else if (moveX == 11 && moveY == 48)
					Dialog($"[color={RGB.Yellow}] 이 세계의 창시자는 안영기님이시며 그는 위대한 프로그래머입니다.[/color]");
				else if (moveX == 14 && moveY == 54)
				{
					if (!GetBit(35))
					{
						Dialog(new string[] {
							$" 당신이 혹시  내 아들 카노푸스와 같은 감방을 사용했던 {mPlayerList[0].Name} 아닌가요?",
							" 역시 그렇군요. 당신이 베스퍼성으로 원정을 간다는 말을 들었습니다. 만약 여유가 있으시다면  내 아들을 당신의 일행에 참가 시켜 주실 수 있겠습니까?" +
							" 지금은 비록 절도 죄로 잡혀 있는 몸이지만  원래 내 아들의 꿈은 로어성 최의 전사가 되는 것이었습니다.  아마 짐은 되지 않을 것입니다. 제발 부탁합니다."
						});
					}
					else
						Dialog($" {mPlayerList[0].Name}님. 제 아들을 잘 부탁합니다.");
				}
				else if (moveX == 21 && moveY == 49)
				{
					Ask($" 나의 이름은 [color={RGB.LightCyan}]알코르[/color]라고 하오.  나는 이곳의 검사로서 나의 길을 개척하려하오. 특히 당신과 함께 베스퍼성의 원정을 하고 싶은데 당신 생각은 어떻소?"
					, MenuMode.JoinAlcor, new string[] {
						"그거 좋은 생각이군요",
						"미안하지만 그런 좀 어렵군요"
					});
				}
				else if (moveX == 21 && moveY == 53)
				{
					Ask($" 나는 이곳의 기사로 있는 [color={RGB.LightCyan}]미자르[/color]라고하오. 당신의 임무에 동참한다면  큰 영광으로 여기겠소."
						, MenuMode.JoinMizar, new string[] {
						"그럼 그렇게 하시오",
						"하지만 그건 좀 곤란하오"
						});
				}
				else if (moveX == 12 && moveY == 26)
					Dialog($" 거기 {mPlayerList[0].GenderStr}분, 어서 앉으시지요.");
				else if (moveX == 17 && moveY == 26)
					Dialog($" 우리 주점의 명물인 코리아 위스키 맛 좀 보시겠습니까?");
				else if (moveX == 20 && moveY == 32)
					Dialog($" 과음은 삼가하십시요.");
				else if (moveX == 17 && moveY == 37)
				{
					Dialog(" 자네를 못 볼줄 알았는데 다시 보게 되었군. 자! 그 기념으로 한 잔 받게나. 완~~~샷 !!");

					if (mParty.Etc[4] > 0)
					{
						Dialog(new string[] {
							"",
							" 당신의 모습을 보니  내가 가이아 테라의 수석 위저드로 있을 때가  생각나는군.  자네를 도와주고픈 마음이 있는데 자네는 어떤가?"
						});

						ShowMenu(MenuMode.JoinAntaresJr, new string[] {
							"역시 안타레스답군요",
							"안됐지만 사양하겠습니다"
						});
					}
				}
				else if (moveX == 12 && moveY == 31)
					Dialog(" 역시 술은 코리아 위스키(소주)가 최고야.");
				else if (moveX == 14 && moveY == 34)
					Dialog(" 음냐, 음냐, 쿨쿨쿨...");
				else if (moveX == 17 && moveY == 32)
				{
					Dialog(new string[] {
						" 이 세계는 다섯개의 대륙이 있다고 하더군요.",
						" 로어 대륙, 가이아 테라,  아프로디테 테라, 이쉬도 테라, 우라누스 테라가 있다고 들었습니다."
					});
				}
				else if (moveX == 9 && moveY == 29)
					Dialog(" 만약 당신이 베스퍼성으로 가려 한다면,  가이아 테라를 거쳐  그 성이 있는  아프로디테테라로 가야 합니다.");
				else if (moveX == 49 && moveY == 10)
					Dialog(" 당신은 정말 행운입니다.  로드안님은 한 번 선고내린 피고인에 대해서는  절대로 사면 시켜 주는 일이 없었는데 말입니다.");
				else if (moveX == 52 && moveY == 10)
					Dialog(" 당신은 로드안님에 의해서 선택 되어진 인간입니다.");
				else if (moveX == 40 && moveY == 9)
				{
					if (mParty.Etc[19] == 0)
						Dialog(" 특별한 일이 없는한 우리 둘은 평생 여기서 지낼거요.");
					else
						Dialog(" 나도 자네처럼 모험을 하고 싶소.");
				}
				else if (moveX == 62 && moveY == 8)
				{
					Dialog(" 나는 다른 종족과의 화합에 대한 책을  발간하려다가 사형 선고를 받았습니다. 나는 정말 억울합니다." +
					"  로드안님의 말인즉 악을 행하는 다른 종족과의 화합은  악과의 화합을 말하는 것이라는 겁니다." +
					" 내가 아는 몇몇의 종족들은 해들 끼치기도 하지만 그것은 그저 그들의 속성일 뿐이지 결코 본성이 악한 것은 아닙니다." +
					" 로드안님을 위시한 이곳의 교육체계는 너무나 선을 강조하며 악을 배척하려 합니다." +
					" 하지만 어느 정도의 필요악도 있어야 하며 이런 교육 방식에 의한 평화는 잠시뿐이지  마침내는 자체적으로 붕괴해 버릴겁니다.");
				}
				else if (moveX == 39 && moveY == 14)
				{
					Dialog(new string[] {
						$" 넌 옆 방에 있었던 {mPlayerList[0].Name} 아냐.",
						" 이제 제법 로드안의 충실한 개가 되었군. 그가 주는 달콤한 사탕에 꼬리를 흔드는 격이라..."
					});
				}
				else if (moveX == 60 && moveY == 14)
				{
					Dialog(" 나는 전과가 많아서 사형선고를 받았습니다. 로드안님도 너무하십니다.  범죄없는  세상을 만드는 것도 좋지만 나 같은 좀도둑에게도 사형을 선고하시다니...");
				}
				else if (moveX == 71 && moveY == 77)
				{
					Dialog(" 여기는 로어성의 묘지입니다. 함부로 들어가지 마십시요.");
				}
				else if (moveX == 63 && moveY == 75)
				{
					Dialog(new string[] {
						" 비석에 어떤 글이 적혀 있었다.",
						"",
						"",
						$"[color={RGB.White}]   여기는 위대한 예언자 데네브의 묘[/color]",
						$"[color={RGB.White}]     620년 8월 30일 여기에 잠들다[/color]",
						"",
						"",
						$" 당신이 자세히 보니 묘비의 아래쪽에 희미하게 [color={RGB.LightCyan}]27,6[/color]이라고 새겨져 있음을 알았다."
					});
				}
				else if (moveX == 55 && moveY == 63)
				{
					Dialog(" 이 세상에 존재하는 모든 선의 대표자는  로드안님이며 악의 대표자는 에인션트 이블입니다.");
				}
				else if (moveX == 58 && moveY == 81)
				{
					Dialog(" 세상의 악은 모두 응징되어야 합니다.  아마 그건 로드안님만이 할 수 있을 겁니다.");
				}
				else if (moveX == 9 && moveY == 72)
				{
					Dialog(" 로어 대륙에는 이곳 로어성과 대륙의 남동쪽에 있는 라스트 디치성이 있습니다. 로어성은 지식의 성전이란 뜻이며  라스트 디치성은 최후까지 버티는 자들의 성이란 뜻입니다.");
				}
				else if (moveX == 79 && moveY == 22)
				{
					Dialog(" 나는 대륙의 남동쪽에 있는 메너스 금광에서 일하고 있습니다.");
				}
				else if (moveX == 23 && moveY == 36)
				{
					Dialog(" 오크족과 트롤족은 미개 종족이라  우리와는 차원이 틀리지요.");
				}
				else if (moveX == 89 && moveY == 41)
				{
					Dialog(" 트롤족은 주로 동굴속에 살며 호전적인 성품을 지녔어요.");
				}
				else if (moveX == 30 && moveY == 63)
				{
					Dialog(" 에인션트 이블은  이 세상에 악을 뿌리고 다니는 아주 나쁜 자입니다.");
				}
				else if (moveX == 89 && moveY == 81)
				{
					Dialog(" 천신의 대륙과  대지신의 대륙은  큰 산맥을 경계로 나누어져 있다던데요.");
				}
				else if (moveX == 61 && moveY == 37)
				{
					Dialog(" 트롤족에 의한 베스퍼성 침공은 에인션트 이블이 배후 조종한 것이라고  로드안님이 그러시더군요.");
				}
				else if (moveX == 41 && moveY == 30)
				{
					Dialog(" 크리스탈로 소환 시킨 몇몇의 생물들은 절대 주인을 배반하지 않는다고 하더군요.");
				}
				else if ((moveX == 49 && moveY == 50) || (moveX == 51 && moveY == 50))
					Dialog(" 당신의 성공을 진심으로 기원합니다.");
				else if (47 <= moveX && moveX <= 53 && 30 <= moveY && moveY <= 36)
				{
					if (mParty.Etc[19] < 3)
						Dialog(" 어서 우리 성주님을 만나 보시지요.");
					else
						Dialog(" 만세, 만세, 만만세, 로드안 만세");
				}
				else if (moveX == 50 && moveY == 27)
				{
					if (mParty.Etc[19] == 2)
					{
						Talk(new string[] {
						" 나는 베스퍼성 원정에 따른  지원자를  찾고 있었소. 여러 방면으로 수소문한 끝에 당신을 선택하게 된거요." +
						" 만약 당신이 베스퍼성 원정을 성공리에 끝낸다면  당신의 전과는 말끔이 없에 주겠소.  그리고 당신이 원하는 삶을 제공해 줄 의양도 있소." +
						" 당신이 주축이 되어 여러 용사들을 합세 시킬 수도 있소. 당신이 성공적으로 임무를 완수하기 위한  자금도 주겠소." +
						"  베스퍼성의  위치를  잘 모르겠다면  마을 사람들에게 물어 보면 될거요.  다른 성에서 온 원정대와  별개로 행동해도  상관없소." +
						" 그리고  내가 다른 성의 성주들에게도 당신에게 호의를 베풀어 주도록 부탁했으니  아프로디테 테라까지 가는데 별 어려움은 없을거요. 당신을 믿겠소.",
						"",
						$"[color={RGB.LightCyan}] [[ 황금 + 5,000 ][/color]"
						}, SpecialEventType.None);

						mParty.Gold += 5_000;
						mParty.Etc[19]++;
					}
					else if (mParty.Etc[19] == 3)
					{
						Dialog(" 당신은 아프로디테 테라의 베스퍼성으로  지금 즉시 떠나야 하오.");
					}
					else if (mParty.Etc[19] == 4)
					{
						Dialog(new string[] {
							$" {mPlayerList[0].Name}, 정말 장한 일을 해주었소.",
							" 이제 당신은 완전한 자유의 몸이오. 이건 나의 성의라오.",
							$"[color={RGB.LightCyan}] [[ 황금 + 40,000 ] [[ 경험치 + 200,000 ][/color]"
						});

						mParty.Gold += 40_000;
						foreach (var player in mPlayerList)
						{
							player.Experience += 200_000;
						}

						if (mAssistPlayer != null)
							mAssistPlayer.Experience += 200_000;

						mParty.Etc[19]++;
					}
					else if (mParty.Etc[19] == 5)
					{
						Talk(" 당신에게  한 가지 부탁을  더 해도 되겠소? 이 일은 당신과 같은 영웅들만이  할 수 있기 때문에  당신에게 부탁하는 것이오." +
						"  아직 이 세계에는 인간 이외에 코볼트족과 드라코니안족이 있소." +
						"  그 종족들은 오크나 트롤같은 하급 종족에 비해서는  월등한 전투력을 가지고 있는데다가 드라코니안족 같은 경우에 있어서는 우리보다 더 뛰어난 지능과 문화를 가지고 있소." +
						"  그들은 2개의 대륙을 차지하고 있으면서 항상 인간들을 배척하고 있기 때문에 인간들의 진출을 방해하는 요소라고  할 수 있소." +
						" 만약 그들이 우리들이 있는 대륙을  침범한다면 어떻게 될런지는 충분히 예상될거요." +
						" 특히 드라코니안족이 덤벼든다면 로어 대륙을 제외하고는 모두 전멸 될것이 뻔한 일인데다가 이곳 역시도 그리 오래는 대항하기 힘들것이오." +
						" 그들이 숭배하고 있는 악의 화신 에인션트 이블이 그들을 선동하여 이곳으로 쳐들어왔을때를 상상해 봤소?  아마  수천년을 유지해오던 이곳의 평화는 산산히 깨어져 버릴 것이오."
						, SpecialEventType.None);

						mParty.Etc[19]++;
					}
					else if (mParty.Etc[19] == 6)
					{
						Dialog(" 당신에게 부탁하겠소.  코볼트족과 드라코니안족을 선제 공격하여  이곳의 평화를 보장하고  신대륙 개척을 앞당기게 해주시오." +
						"  만약 당신이 그 두 종족을 멸해준다면 나는 우라누스테라를 당신에게 넘겨줄 의양도 있소. 제발 부탁하오.");
					}
					else if (mParty.Etc[19] == 7)
					{
						Talk(new string[] {
						$" 돌아왔군요. {mPlayerList[0].Name}.",
						" 역시 내가 보는 눈은 정확했소.  당신이라면 분명히  우리에게  승리를 안겨줄거란걸 알았소. 당신에게 주기로 약속한 우라누스 테라도 곧 당신의 손에 들어갈 거요." +
						" 그리고 그때 당신은 그곳의 성주가 되는 것이오.",
						$"[color={RGB.LightCyan}]  [[ 황금 + 100,000 ] [[ 경험치 + 2,500,000][/color]"
						}, SpecialEventType.RequestSuppressVariantPeoples);

						mParty.Gold += 100_000;
						
						foreach (var player in mPlayerList) {
							player.Experience += 2_500_000;
						}

						if (mAssistPlayer != null)
							mAssistPlayer.Experience += 2_500_000;
					}
					else if (mParty.Etc[19] == 8) {
						Dialog(" 즉시 배리언트 피플즈로 가서 폭동을 진압해 주시오." +
						"  당신이 그 일을 끝내고 돌아오면 나는 당신에게 공작의 작위와 함께 우라누스 테라에  당신의 공국을 건설하도록 허락해 주겠소.");
					}
					else if (mParty.Etc[19] == 9) {
						Talk(new string[] {
							" 당신 덕택에  세상은 완전한 평화를 얻게 되었소.  그리고 약속한대로 당신에게 우라누스 테라의 통치권과 공작의 칭호를 내려주겠소.",
							$" 축하하오, {mPlayerList[0].Name} 공작...",
							" 그런데  미안하지만 한가지 부탁만  더 들어주겠소? 당신이 멸망시킨 네종족이 사실은 완전히 사라진게 아니오." +
							" 그들은 지금 현재, 죽어간 그 종족의 의지가  하나로 뭉쳐  자신이 있던 대륙에  원혼이 되어 남아있소." +
							"  그것들 중에서는  스스로의 의지로 이곳을 향해 오는 원혼도 있소." +
							"  그 원혼들이 이 대륙에 들어와서 우리에게 복수하기 전에 우리가 먼저 그들을 발견하여 강한 힘속에 봉인 시켜 버려야하오." +
							" 그래서 부탁하건데, 이 크리스탈 볼을 받으시오. 이 크리스탈 볼 속을 들여다 보면 오크, 트롤, 코볼트, 드라코니안 종족의 원혼이 있는 장소가 드러날꺼요." +
							" 그리고 그 장소에서 그들의 원혼을 발견하여 크리스탈 볼 속에 봉인 시켜 버리시오. 네 종족을 모두 봉인 시켰을때  그 크리스탈 볼을 나에게 가져오면  그이후는 내가 알아서 하겠소." +
							"  그리고  당신의 영토인 우라누스 테라로 이동 시켜 주겠소.",
							$"[color={RGB.LightCyan}] [[ 크리스탈 볼 + 1 ][/color]"
						}, SpecialEventType.None);

						mParty.Crystal[8]++;
						mParty.Etc[19]++;
					}
					else if (mParty.Etc[19] == 10) {
						mXAxis = 50;
						mYAxis = 30;

						mFace = 1;
						if (mPlayerList[0].ClassType == ClassCategory.Magic)
							mFace += 8;

						InvokeAnimation(AnimationType.PassCrystal);
					}
				}
			}
		}

		private void ShowSign(int x, int y)
		{
			Dialog(new string[] { "푯말에 쓰여있기로 ...", "", "" });

			if (mMapName == "Lore") {
				if (x == 50 && y==83)
				{
					Dialog(new string[] {
						$"[color={RGB.White}]          이곳은 로어성입니다.[/color]",
						$"[color={RGB.White}]          여러분을 환영합니다.[/color]",
						"",
						"",
						$"[color={RGB.LightGreen}]         이곳의 성주 로드안 씀[/color]"
					}, true);
				}
				else if (x == 50 && y == 14)
					Dialog($"[color={RGB.White}]      로어 왕립 죄수 수용소[/color]", true);
				else if (x == 23 && y == 30) {
					Dialog(new string[] {
						$"[color={RGB.White}]             로어 주점[/color]",
						"",
						$"[color={RGB.White}]      ( 코리아 위스키 전문점 )[/color]"
					}, true);
				}
				else if (x == 50 && y == 57) {
					if (GetBit(50)) {
						Dialog($"[color={RGB.White}]     이제 우리를 위협하던 네 종족은 정의의 심판에 의해 무릎을 꿓고 말았다. 이제부터는 새로운 인류의 역사가 시작 될 것이다.[/color]", true);
					}
					else {
						Dialog($"[color={RGB.White}]   674년 1월 17일. 아프로디테의 베스퍼성이 호전적인  트롤족에 의해  참변을 당했다. " +
						"이를 계기로  로어성과  그 이외의 성들은 오크족, 트롤족, 코볼트족,  드라코니안족에 대한 무차별 정벌을 선포하는 바이다." +
						"이로 인해  인간 이외의 다른 종족에 대한 호의를 일체 중지하며 이제는 적대 관계로 그들을 대할 것이다.[/color]", true);
					}
				}
			}
			else if (mMapName == "Dark") {
				if ((x == 50 && y == 17) || (x == 51 && y == 17))
					Dialog($"[color={RGB.White}]      로어 왕립 죄수 수용소[/color]", true);
				else if (x == 23 && y == 30)
				{
					Dialog(new string[] {
						$"[color={RGB.White}]             로어 주점[/color]",
						"",
						$"[color={RGB.White}]( 코리아 위스키가 잘 팔린다는 말 않겠음 )[/color]"
					}, true);
				}
			}
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
			else
				LoadFile();
		}

		private void MovePlayer(int moveX, int moveY, bool eventTile = false)
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

			if (!DetectGameOver())
			{
				if (mParty.Etc[4] > 0)
					mParty.Etc[4]--;

				if (!eventTile && mRand.Next(mEncounter * 20) == 0)
					EncounterEnemy();


				if (mMapHeader.TileType == PositionType.Ground)
					PlusTime(0, 2, 0);
				else
					PlusTime(0, 0, 5);
			}
		}

		private bool EnterWater()
		{
			if (mParty.Etc[1] > 0)
			{
				mParty.Etc[1]--;

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

		private MenuMode HideMenu()
		{
			var menuMode = mMenuMode;
			mMenuMode = MenuMode.None;

			for (var i = 0; i < mMenuCount; i++)
			{
				mMenuList[i].Visibility = Visibility.Collapsed;
			}

			return menuMode;
		}

		private void ShowCharacterMenu(MenuMode menuMode, bool includeAssistPlayer = true)
		{
			AppendText($"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]", true);

			var menuStr = new string[mPlayerList.Count + (includeAssistPlayer && mAssistPlayer != null ? 1 : 0)];
			for (var i = 0; i < mPlayerList.Count; i++)
				menuStr[i] = mPlayerList[i].Name;

			if (includeAssistPlayer && mAssistPlayer != null)
				menuStr[menuStr.Length - 1] = mAssistPlayer.Name;

			ShowMenu(menuMode, menuStr);
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

		private void ShowSpinner(SpinnerType spinnerType, Tuple<string, int>[] items, int defaultId)
		{
			mSpinnerType = spinnerType;

			mSpinnerItems = items;
			mSpinnerID = defaultId;

			AppendText(SpinnerText, mSpinnerItems[defaultId].Item1);
			SpinnerText.Visibility = Visibility.Visible;
		}

		private void StartBattle(bool assualt = true)
		{
			mParty.Etc[5] = 1;

			ClearDialog();

			if (assualt)
			{
				mBattleTurn = BattleTurn.Player;

				mBattlePlayerID = -1;
				for (var i = 0; i < mPlayerList.Count; i++)
				{
					if (mPlayerList[i].IsAvailable)
					{
						mBattlePlayerID = i;
						break;
					}
				}

				mBattleCommandQueue.Clear();

				BattleMode();
			}
			else
			{
				mBattleTurn = BattleTurn.Player;
				ExecuteBattle();
			}
		}

		private void BattleMode()
		{
			if (mBattlePlayerID == -1)
			{
				mBattlePlayerID = mPlayerList.Count;
				AddBattleCommand(true);
			}
			else
			{
				var player = mPlayerList[mBattlePlayerID];
				mBattleFriendID = 0;
				mBattleToolID = 0;

				AppendText($"{player.Name}의 전투 모드 ===>");

				ShowMenu(MenuMode.BattleCommand, new string[] {
					$"한 명의 적을 {Common.GetWeaponNameJosa(player.Weapon)}로 공격",
					"한 명의 적에게 마법 공격",
					"모든 적에게 마법 공격",
					"적에게 특수 마법 공격",
					"일행을 치료",
					"적에게 초능력 사용",
					"소환 마법 사용",
					"약초나 크리스탈을 사용",
					mBattlePlayerID == 0 ? "일행에게 무조건 공격 할 것을 지시" : "도망을 시도함"
				});
			}
		}

		private void AddBattleCommand(bool skip = false)
		{
			if (!skip)
			{
				mBattleCommandQueue.Enqueue(new BattleCommand()
				{
					Player = mPlayerList[mBattlePlayerID],
					FriendID = mBattleFriendID,
					Method = mBattleCommandID,
					Tool = mBattleToolID,
					EnemyID = mEnemyFocusID
				});

				if (mEnemyFocusID >= 0)
					mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);
			}

			do
			{
				mBattlePlayerID++;
			} while (mBattlePlayerID < mPlayerList.Count && !mPlayerList[mBattlePlayerID].IsAvailable);

			if (mBattlePlayerID < mPlayerList.Count)
			{
				BattleMode();
			}
			else
			{
				AddAssistAttackCommand();

				DialogText.TextHighlighters.Clear();
				DialogText.Blocks.Clear();

				mBattleTurn = BattleTurn.Player;

				ExecuteBattle();
			}
		}

		private void AddAssistAttackCommand()
		{
			if (mAssistPlayer != null && mAssistPlayer.IsAvailable)
			{
				void AssistAttack(bool attackAll)
				{
					mBattleCommandQueue.Enqueue(new BattleCommand()
					{
						Player = mAssistPlayer,
						FriendID = -1,
						Method = 0,
						Tool = 0,
						EnemyID = attackAll ? -1 : 0
					});
				}

				switch (mAssistPlayer.ClassType)
				{
					case ClassCategory.Sword:
					case ClassCategory.Unknown:
					case ClassCategory.Giant:
					case ClassCategory.Dragon:
						AssistAttack(false);
						break;
					case ClassCategory.Elemental:
						AssistAttack(mAssistPlayer.Weapon != 29);
						break;
					default:
						AssistAttack(true);
						break;
				}
			}
		}

		private void ExecuteBattle()
		{
			void CheckBattleStatus()
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

				if (mAssistPlayer != null && mAssistPlayer.IsAvailable)
					allPlayerDead = false;

				if (allPlayerDead)
				{
					mBattleTurn = BattleTurn.Lose;
					mParty.Etc[5] = 0;
				}
				else
				{
					var allEnemyDead = true;

					foreach (var enemy in mEncounterEnemyList)
					{
						if (!enemy.Dead && ((!mCruel && !enemy.Unconscious) || mCruel))
						{
							allEnemyDead = false;
							break;
						}
					}

					if (allEnemyDead)
					{
						mBattleTurn = BattleTurn.Win;
						mParty.Etc[5] = 0;
					}
				}
			}

			void ShowBattleResult(List<string> battleResult)
			{
				var lineHeight = 0d;
				if (DialogText.Blocks.Count > 0)
				{
					var startRect = DialogText.Blocks[0].ContentStart.GetCharacterRect(LogicalDirection.Forward);
					lineHeight = startRect.Height;
				}

				var lineCount = lineHeight == 0 ? 0 : (int)Math.Ceiling(DialogText.ActualHeight / lineHeight);

				var append = false;
				if (lineCount + battleResult.Count + 1 <= DIALOG_MAX_LINES)
				{
					if (lineHeight > 0)
						battleResult.Insert(0, "");
					append = true;
				}

				AppendText(battleResult.ToArray(), append);

				DisplayPlayerInfo();
				DisplayEnemy();

				ContinueText.Visibility = Visibility.Visible;

				CheckBattleStatus();
			}


			if (mBattleCommandQueue.Count == 0 && mBatteEnemyQueue.Count == 0)
			{
				DialogText.TextHighlighters.Clear();
				DialogText.Blocks.Clear();

				switch (mBattleTurn)
				{
					case BattleTurn.Player:
						mBattleTurn = BattleTurn.Enemy;
						break;
					case BattleTurn.Enemy:
						mBattleTurn = BattleTurn.Player;
						break;
				}

				switch (mBattleTurn)
				{
					case BattleTurn.Player:
						mBattlePlayerID = -1;
						for (var i = 0; i < mPlayerList.Count; i++)
						{
							if (mPlayerList[i].IsAvailable)
							{
								mBattlePlayerID = i;
								break;
							}
						}

						BattleMode();
						return;
					case BattleTurn.Enemy:
						foreach (var enemy in mEncounterEnemyList)
						{
							if (!enemy.Dead && !enemy.Unconscious)
								mBatteEnemyQueue.Enqueue(enemy);
						}

						break;
				}
			}


			if (mBattleCommandQueue.Count > 0)
			{
				var battleCommand = mBattleCommandQueue.Dequeue();
				var battleResult = new List<string>();

				BattleEnemyData GetDestEnemy()
				{
					bool AllEnemyDead()
					{
						for (var i = 0; i < mEncounterEnemyList.Count; i++)
						{
							if (!mEncounterEnemyList[i].Dead && ((!mCruel && !mEncounterEnemyList[i].Unconscious) || mCruel))
								return false;
						};

						return true;
					}

					if (!AllEnemyDead())
					{
						var enemyID = battleCommand.EnemyID;
						while (mEncounterEnemyList[enemyID].Dead || (!mCruel && mEncounterEnemyList[enemyID].Unconscious))
							enemyID = (enemyID + 1) % mEncounterEnemyList.Count;

						return mEncounterEnemyList[enemyID];
					}
					else
						return null;
				}

				void GetBattleStatus(BattleEnemyData enemy)
				{
					var player = battleCommand.Player;

					switch (battleCommand.Method)
					{
						case 0:
							int messageType;
							if (player.Weapon - 1 >= 0)
								messageType = (player.Weapon - 1) / 7;
							else
								messageType = 0;


							switch (messageType)
							{
								case 1:
									battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetWeaponNameJosa(player.Weapon)}로 {enemy.NameJosa} 내려쳤다[/color]");
									break;
								case 2:
									battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetWeaponNameJosa(player.Weapon)}로 {enemy.NameJosa} 찔렀다[/color]");
									break;
								case 3:
									if (mParty.Arrow > 0)
										battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetWeaponNameJosa(player.Weapon)}로 {enemy.NameJosa} 쏘았다[/color]");
									else
										battleResult.Add($"[color={RGB.White}]화살이 다 떨어져 공격할 수 없었다[/color]");
									break;
								default:
									battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetWeaponNameJosa(player.Weapon)}로 {enemy.NameJosa} 공격했다[/color]");
									break;
							}
							break;
						case 1:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetMagicNameJosa(0, battleCommand.Tool)}로 {enemy.Name}에게 공격했다[/color]");
							break;
						case 2:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetMagicNameJosa(1, battleCommand.Tool)}로 {enemy.Name}에게 공격했다[/color]");
							break;
						case 3:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {enemy.Name}에게 {Common.GetMagicNameJosa(4, battleCommand.Tool)}로 특수 공격을 했다[/color]");
							break;
						case 4:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {mPlayerList[battleCommand.FriendID].Name}에게 {Common.GetMagicNameMokjukJosa(3, battleCommand.Tool + 1)} 사용했다[/color]");
							break;
						case 5:
							if (enemy == null)
								battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} 모든 적에게 {Common.GetMagicNameMokjukJosa(5, battleCommand.Tool)} 사용했다[/color]");
							else
								battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {enemy.Name}에게 {Common.GetMagicNameMokjukJosa(5, battleCommand.Tool)} 사용했다[/color]");
							break;
						case 6:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetMagicNameMokjukJosa(6, 3)} 사용했다[/color]");
							break;
						case 8:
							battleResult.Add($"[color={RGB.White}]일행은 도망을 시도했다[/color]");
							break;
						default:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} 잠시 주저했다[/color]");
							break;
					}
				}

				void PlusExperience(BattleEnemyData enemy)
				{
#if DEBUG
					var exp = 50_000;
#else
					var exp = (enemy.ENumber + 1) * (enemy.ENumber + 1) * (enemy.ENumber + 1) / 8;
					if (exp == 0)
						exp = 1;
#endif

					if (!enemy.Unconscious)
					{
						battleResult.Add($"[color={RGB.Yellow}]{battleCommand.Player.NameSubjectJosa}[/color] [color={RGB.LightCyan}]{exp.ToString("#,#0")}[/color][color={RGB.Yellow}]만큼 경험치를 얻었다![/color]");
						battleCommand.Player.Experience += exp;
					}
					else
					{
						foreach (var player in mPlayerList)
						{
							if (player.IsAvailable)
								player.Experience += exp;
						};

						if (mAssistPlayer != null)
							mAssistPlayer.Experience += exp;
					}
				}

				void AttackOne()
				{
					var enemy = GetDestEnemy();
					if (enemy == null)
						return;

					GetBattleStatus(enemy);

					var player = battleCommand.Player;

					if (player.Weapon - 1 >= 0)
					{
						var weaponType = (player.Weapon - 1) / 7;
						if (weaponType == 3)
						{
							if (mParty.Arrow > 0)
								mParty.Arrow--;
							else
								return;
						}
					}


					if (enemy.Unconscious)
					{
						switch (mRand.Next(4))
						{
							case 0:
								battleResult.Add($"[color={RGB.LightRed}]{player.GenderPronoun}의 무기가 {enemy.Name}의 심장을 꿰뚫었다[/color]");
								break;
							case 1:
								battleResult.Add($"[color={RGB.LightRed}]{enemy.Name}의 머리는 {player.GenderPronoun}의 공격으로 산산 조각이 났다[/color]");
								break;
							case 2:
								battleResult.Add($"[color={RGB.LightRed}]적의 피가 사방에 뿌려졌다[/color]");
								break;
							case 3:
								battleResult.Add($"[color={RGB.LightRed}]적은 비명과 함께 찢겨 나갔다[/color]");
								break;

						}

						PlusExperience(enemy);
						enemy.HP = 0;
						enemy.Dead = true;
						DisplayEnemy();
						return;
					}

					if (mRand.Next(20) > player.Accuracy)
					{
						battleResult.Add($"{player.GenderPronoun}의 공격은 빗나갔다 ....");
						return;
					}

					int power;
					switch ((player.Weapon + 6) / 7)
					{
						case 0:
							power = player.FistSkill;
							break;
						case 1:
							power = player.SwordSkill;
							break;
						case 2:
							power = player.AxeSkill;
							break;
						case 3:
							power = player.SpearSkill;
							break;
						case 4:
							power = player.BowSkill;
							break;
						default:
							power = player.Level * 5;
							break;
					}

					int attackPoint;
					if ((player.ClassType == ClassCategory.Sword) && (player.Class == 5 || player.Class == 6))
						attackPoint = player.Strength * power * 5;
					else
						attackPoint = (int)(Math.Round((double)player.Strength * player.WeaPower * power / 10));

					attackPoint -= attackPoint * mRand.Next(50) / 100;

					if (mRand.Next(100) < enemy.Resistance)
					{
						battleResult.Add($"적은 {player.GenderPronoun}의 공격을 저지했다");
						return;
					}

					var defensePoint = enemy.AC * enemy.Level * (mRand.Next(10) + 1);
					attackPoint -= defensePoint;
					if (attackPoint <= 0)
					{
						battleResult.Add($"그러나, 적은 {player.GenderPronoun}의 공격을 막았다");
						return;
					}

					enemy.HP -= attackPoint;
					if (enemy.HP <= 0)
					{
						enemy.HP = 0;
						enemy.Unconscious = false;
						enemy.Dead = false;

						battleResult.Add($"[color={RGB.LightRed}]적은 {player.GenderPronoun}의 공격으로 의식불명이 되었다[/color]");
						PlusExperience(enemy);
						enemy.Unconscious = true;
					}
					else
					{
						battleResult.Add($"적은 [color={RGB.White}]{attackPoint.ToString("#,#0")}[/color]만큼의 피해를 입었다");
					}
				}

				void CastOne()
				{
					var enemy = GetDestEnemy();
					if (enemy == null)
						return;

					GetBattleStatus(enemy);

					var player = battleCommand.Player;

					if (enemy.Unconscious)
					{
						battleResult.Add($"[color={RGB.LightRed}]{player.GenderPronoun}의 마법은 적을 완전히 제거해 버렸다[/color]");

						PlusExperience(enemy);
						enemy.HP = 0;
						enemy.Dead = true;
						DisplayEnemy();
						return;
					}

					var magicPoint = (int)Math.Round((double)battleCommand.Player.AttackMagic * battleCommand.Tool * battleCommand.Tool / 10);
					if (battleCommand.Player.SP < magicPoint)
					{
						battleResult.Add($"마법 지수가 부족했다");
						return;
					}

#if DEBUG
					battleCommand.Player.SP -= 1;
#else
					battleCommand.Player.SP -= magicPoint;
#endif

					DisplaySP();

					if (mRand.Next(20) >= player.Accuracy)
					{
						battleResult.Add($"그러나, {enemy.NameJosa} 빗나갔다");
						return;
					}

					magicPoint = battleCommand.Tool * battleCommand.Tool * player.AttackMagic * 3;

					if (mRand.Next(100) < enemy.Resistance)
					{
						battleResult.Add($"{enemy.NameSubjectJosa} {battleCommand.Player.GenderPronoun}의 마법을 저지했다");
						return;
					}

					var defensePoint = enemy.AC * enemy.Level * (mRand.Next(10) + 1);
					magicPoint -= defensePoint;

					if (magicPoint <= 0)
					{
						battleResult.Add($"그러나, {enemy.NameSubjectJosa} {battleCommand.Player.GenderPronoun}의 마법 공격을 막았다");
						return;
					}

					enemy.HP -= magicPoint;
					if (enemy.HP <= 0)
					{
						battleResult.Add($"[color={RGB.LightRed}]{enemy.NameSubjectJosa} {battleCommand.Player.GenderPronoun}의 마법에 의해 의식불능이 되었다[/color]");
						PlusExperience(enemy);

						enemy.HP = 0;
						enemy.Unconscious = true;
					}
					else
					{
						battleResult.Add($"{enemy.NameSubjectJosa} [color={RGB.White}]{magicPoint.ToString("#,#0")}[/color]만큼의 피해를 입었다");
					}
				}

				void CastESP()
				{
					var player = battleCommand.Player;

					if (battleCommand.Tool == 0)
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						if (player.ESPMagic < 40)
						{
							battleResult.Add("독심을 샤용하려 하였지만 아직 능력이 부족했다");
							return;
						}

						if (player.SP < 15)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 15;
						DisplaySP();

						if (enemy.ENumber != 5 &&
							enemy.ENumber != 9 &&
							enemy.ENumber != 19 &&
							enemy.ENumber != 23 &&
							enemy.ENumber != 26 &&
							enemy.ENumber != 28 &&
							enemy.ENumber != 32 &&
							enemy.ENumber != 34 &&
							enemy.ENumber != 39 &&
							enemy.ENumber != 46 &&
							enemy.ENumber != 52 &&
							enemy.ENumber != 61 &&
							enemy.ENumber != 69)
						{
							battleResult.Add($"독심술은 전혀 통하지 않았다");
							return;
						}

						var requireLevel = enemy.Level;
						if (enemy.ENumber == 69)
							requireLevel = 17;

						if (requireLevel > player.Level && mRand.Next(2) == 0)
						{
							battleResult.Add($"적의 마음을 끌어들이기에는 아직 능력이 부족했다");
							return;
						}

						if (mRand.Next(60) > (player.Level - requireLevel) * 2 + battleCommand.Player.Concentration)
						{
							battleResult.Add($"적의 마음은 흔들리지 않았다");
							return;
						}

						battleResult.Add($"[color={RGB.LightCyan}]적은 우리의 편이 되었다[/color]");
						JoinMemberFromEnemy(enemy.ENumber);
						enemy.Dead = true;
						enemy.Unconscious = true;
						enemy.HP = 0;
						enemy.Level = 0;
					}
					else if (battleCommand.Tool == 1)
					{
						GetBattleStatus(null);

						if (player.ESPMagic < 20)
						{
							battleResult.Add($"{player.GenderPronoun}는 주위의 원소들을 이용하려 했으나 역부족이었다");
							return;
						}

						if (player.SP < 100)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 100;
						DisplaySP();

						if (mRand.Next(2) == 0)
							battleResult.Add("갑자기 땅속의 우라늄이 핵분열을 일으켜 고온의 열기가 적의 주위를 감싸기 시작한다");
						else
							battleResult.Add("공기 중의 수소가 돌연히 핵융합을 일으켜 질량 결손의 에너지를 적들에게 방출하기 시작한다");

						foreach (var enemy in mEncounterEnemyList)
						{
							if (enemy.Dead || (enemy.Unconscious && !mCruel))
								continue;

							var impactPoint = enemy.HP;
							if (impactPoint < player.Concentration * player.Level)
								impactPoint = 0;
							else
								impactPoint -= player.Concentration * player.Level;
							enemy.HP = impactPoint;

							if (enemy.Unconscious && !enemy.Dead)
							{
								enemy.Dead = true;
								PlusExperience(enemy);
							}
							else if (impactPoint == 0 && !enemy.Unconscious)
							{
								enemy.Unconscious = true;
								PlusExperience(enemy);
							}
						}
					}
					else if (battleCommand.Tool == 2)
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						battleResult.Add($"{player.GenderPronoun}는 적에게 공포심을 불어 넣었다");

						if (player.ESPMagic < 30)
						{
							battleResult.Add($"{player.GenderPronoun}는 아직 능력이 부족 했다");
							return;
						}

						if (player.SP < 100)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 100;
						DisplaySP();

						if (mRand.Next(40) < enemy.Resistance)
						{
							if (enemy.Resistance < 5)
								enemy.Resistance = 0;
							else
								enemy.Resistance -= 5;
							return;
						}

						if (mRand.Next(60) > battleCommand.Player.Concentration)
						{
							if (enemy.Endurance < 5)
								enemy.Endurance = 0;
							else
								enemy.Endurance -= 5;

							return;
						}

						enemy.Dead = true;
						battleResult.Add($"[color={RGB.LightGreen}]{enemy.NameSubjectJosa} 겁을 먹고는 도망가 버렸다[/color]");
					}
					else if (battleCommand.Tool == 3)
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						battleResult.Add($"{player.GenderPronoun}는 적을 환상속에 빠지게 하려한다");

						if (player.ESPMagic < 80)
						{
							battleResult.Add($"{player.GenderPronoun}는 아직 능력이 부족 했다");
							return;
						}

						if (player.SP < 300)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 300;
						DisplaySP();

						if (mRand.Next(30) < player.Concentration)
						{
							for (var i = 0; i < 2; i++)
							{
								if (enemy.Accuracy[i] < 4)
									enemy.Accuracy[i] = 0;
								else
									enemy.Accuracy[i] -= 4;
							}
						}

						if (mRand.Next(40) < enemy.Resistance)
						{
							if (enemy.Agility < 5)
								enemy.Agility = 0;
							else
								enemy.Agility -= 5;
							return;
						}
					}
					else if (battleCommand.Tool == 4)
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						battleResult.Add($"{battleCommand.Player.GenderPronoun}는 적의 신진대사를 조절하여 적의 체력을 점차 약화 시키려 한다");

						if (player.ESPMagic < 90)
						{
							battleResult.Add($"{player.GenderPronoun}는 아직 능력이 부족 했다");
							return;
						}

						if (player.SP < 500)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 500;
						DisplaySP();

						if (mRand.Next(40) > battleCommand.Player.Concentration)
							return;

						if (enemy.Posion)
						{
							if (enemy.HP > 500)
								enemy.HP -= 50;
							else
							{
								enemy.HP = 0;
								enemy.Unconscious = true;
								PlusExperience(enemy);
							}
						}
						else
							enemy.Posion = true;
					}
					else if (battleCommand.Tool == 5)
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						battleResult.Add($"{battleCommand.Player.GenderPronoun}는 염력으로 적의 영혼을 분리시키려 한다");

						if (player.ESPMagic < 100)
						{
							battleResult.Add($"{player.GenderPronoun}는 아직 능력이 부족 했다");
							return;
						}

						if (player.SP < 1_000)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 1_000;
						DisplaySP();

						if (mRand.Next(40) < enemy.Resistance)
						{
							if (enemy.Resistance < 20)
								enemy.Resistance = 0;
							else
								enemy.Resistance -= 20;
							return;
						}

						if (mRand.Next(80) > player.Concentration)
						{
							if (enemy.HP < 500)
							{
								enemy.HP = 0;
								enemy.Unconscious = true;
								PlusExperience(enemy);
							}
							else
								enemy.HP -= 500;

							return;
						}

						enemy.Unconscious = true;
					}
				}


				if (battleCommand.Method == 0)
				{
					if (battleCommand.EnemyID == -1)
					{
						for (var i = 0; i < mEncounterEnemyList.Count; i++)
						{
							if (!mEncounterEnemyList[i].Dead && ((!mEncounterEnemyList[i].Unconscious && !mCruel) || mCruel))
							{
								battleCommand.EnemyID = i;
								AttackOne();
							}
						}
					}
					else
						AttackOne();
				}
				else if (battleCommand.Method == 1)
				{
					CastOne();
				}
				else if (battleCommand.Method == 2)
				{
					for (var i = 0; i < mEncounterEnemyList.Count; i++)
					{
						if (!mEncounterEnemyList[i].Dead)
						{
							battleCommand.EnemyID = i;
							CastOne();
						}
					}
				}
				else if (battleCommand.Method == 3)
				{
					void RemoveSkill()
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						const int SKILL_POINT = 200;

						if (battleCommand.Player.SP < SKILL_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= SKILL_POINT;
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"기술 무력화 공격은 저지 당했다");
							return;
						}

						if (mRand.Next(60) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"기술 무력화 공격은 빗나갔다");
							return;
						}

						battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 특수 공격 능력이 제거되었다[/color]");
						enemy.Special = 0;
					}

					void RemoveDefense()
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						const int DEFENCE_POINT = 50;

						if (battleCommand.Player.SP < DEFENCE_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= DEFENCE_POINT;
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"방어 무력화 공격은 저지 당했다");
							return;
						}


						int resistancePoint;
						if (enemy.AC < 5)
							resistancePoint = 40;
						else
							resistancePoint = 25;

						if (mRand.Next(resistancePoint) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"방어 무력화 공격은 빗나갔다");
							return;
						}

						battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 방어 능력이 저하되었다[/color]");
						if ((enemy.Resistance < 31 || mRand.Next(2) == 0) && enemy.AC > 0)
							enemy.AC--;
						else
						{
							enemy.Resistance -= 10;
							if (enemy.Resistance > 0)
								enemy.Resistance = 0;
						}
					}

					void RemoveAbility()
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						const int ABILITY_POINT = 100;

						if (battleCommand.Player.SP < ABILITY_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= ABILITY_POINT;
						DisplaySP();

						if (mRand.Next(200) < enemy.Resistance)
						{
							battleResult.Add($"능력 저하 공격은 저지 당했다");
							return;
						}


						if (mRand.Next(30) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"능력 저하 공격은 빗나갔다");
							return;
						}

						battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 전체적인 능력이 저하되었다[/color]");
						if (enemy.Level > 0)
							enemy.Level--;

						enemy.Resistance -= 10;
						if (enemy.Resistance < 0)
							enemy.Resistance = 0;
					}

					void RemoveMagic()
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						const int MAGIC_POINT = 150;

						if (battleCommand.Player.SP < MAGIC_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= MAGIC_POINT;
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"마법 불능 공격은 저지 당했다");
							return;
						}


						if (mRand.Next(100) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"마법 불능 공격은 빗나갔다");
							return;
						}

						if (enemy.CastLevel > 1)
							battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 마법 능력이 저하되었다[/color]");
						else
							battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 마법 능력은 사라졌다[/color]");

						if (enemy.CastLevel > 0)
							enemy.CastLevel--;
					}

					void RemoveSuperman()
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						const int SUPERMAN_POINT = 400;

						if (battleCommand.Player.SP < SUPERMAN_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= SUPERMAN_POINT;
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"탈 초인화 공격은 저지 당했다");
							return;
						}


						if (mRand.Next(100) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"탈 초인화 공격은 빗나갔다");
							return;
						}

						if (enemy.SpecialCastLevel > 1)
							battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 초자연적 능력이 저하되었다[/color]");
						else
							battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 초자연적 능력은 사라졌다[/color]");

						if (enemy.SpecialCastLevel > 0)
							enemy.SpecialCastLevel--;
					}

					switch (battleCommand.Tool)
					{
						case 1:
							RemoveSkill();
							break;
						case 2:
							RemoveMagic();
							break;
						case 3:
							RemoveAbility();
							break;
						case 4:
							RemoveMagic();
							break;
						case 5:
							RemoveSuperman();
							break;
						case 6:
							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Dead && (mCruel || (!mCruel && !enemy.Unconscious)))
									RemoveSkill();
							}
							break;
						case 7:
							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Dead && (mCruel || (!mCruel && !enemy.Unconscious)))
									RemoveDefense();
							}
							break;
						case 8:
							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Dead && (mCruel || (!mCruel && !enemy.Unconscious)))
									RemoveAbility();
							}
							break;
						case 9:
							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Dead && (mCruel || (!mCruel && !enemy.Unconscious)))
									RemoveMagic();
							}
							break;
						case 10:
							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Dead && (mCruel || (!mCruel && !enemy.Unconscious)))
									RemoveSuperman();
							}
							break;

					}
				}
				else if (battleCommand.Method == 5)
				{
					CastESP();
				}
				else if (battleCommand.Method == 6)
				{
					GetBattleStatus(null);

					int GetBonusPoint(int seed)
					{
						return mRand.Next(seed * 2 + 1) - seed;
					}

					if (battleCommand.Tool == 0)
					{
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Male,
							Class = 0,
							ClassType = ClassCategory.Elemental,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Endurance = 10 + GetBonusPoint(5),
							Resistance = 10 + GetBonusPoint(5),
							Agility = 0,
							Accuracy = 10 + GetBonusPoint(5),
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Weapon = 29,
							WeaPower = battleCommand.Player.SummonMagic * 3,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							AC = 0,
							PotentialAC = 0,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							ShieldSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(4))
						{
							case 0:
								mAssistPlayer.Name = "불의 정령";
								break;
							case 1:
								mAssistPlayer.Name = "사라만다";
								break;
							case 2:
								mAssistPlayer.Name = "아저";
								break;
							default:
								mAssistPlayer.Name = "이프리트";
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 1)
					{
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Female,
							Class = 0,
							ClassType = ClassCategory.Elemental,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Endurance = 8 + GetBonusPoint(5),
							Resistance = 12 + GetBonusPoint(5),
							Agility = 0,
							Accuracy = 10 + GetBonusPoint(5),
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Weapon = 30,
							WeaPower = battleCommand.Player.SummonMagic * 2,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							AC = 0,
							PotentialAC = 1,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							ShieldSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(9))
						{
							case 0:
								mAssistPlayer.Name = "님프";
								break;
							case 1:
								mAssistPlayer.Name = "드리어드스";
								break;
							case 2:
								mAssistPlayer.Name = "네레이드스";
								break;
							case 3:
								mAssistPlayer.Name = "나이어드스";
								break;
							case 4:
								mAssistPlayer.Name = "나파이어스";
								break;
							case 5:
								mAssistPlayer.Name = "오레이어드스";
								break;
							case 6:
								mAssistPlayer.Name = "알세이드스";
								break;
							case 7:
								mAssistPlayer.Name = "마리드";
								break;
							default:
								mAssistPlayer.Name = "켈피";
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 2)
					{
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Female,
							Class = 0,
							ClassType = ClassCategory.Elemental,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Endurance = 6 + GetBonusPoint(5),
							Resistance = 14 + GetBonusPoint(5),
							Agility = 0,
							Accuracy = 13 + GetBonusPoint(5),
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Weapon = 31,
							WeaPower = battleCommand.Player.SummonMagic,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							AC = 1,
							PotentialAC = 1,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							ShieldSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(5))
						{
							case 0:
								mAssistPlayer.Name = "공기의 정령";
								break;
							case 1:
								mAssistPlayer.Name = "실프";
								break;
							case 2:
								mAssistPlayer.Name = "실피드";
								break;
							case 3:
								mAssistPlayer.Name = "디지니";
								break;
							default:
								mAssistPlayer.Name = "인비저블 스토커";
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 3)
					{
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Male,
							Class = 0,
							ClassType = ClassCategory.Elemental,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Endurance = 14 + GetBonusPoint(5),
							Resistance = 10 + GetBonusPoint(5),
							Agility = 0,
							Accuracy = 6 + GetBonusPoint(5),
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Weapon = 32,
							WeaPower = battleCommand.Player.SummonMagic * 4,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							AC = 3,
							PotentialAC = 3,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							ShieldSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(3))
						{
							case 0:
								mAssistPlayer.Name = "땅의 정령";
								break;
							case 1:
								mAssistPlayer.Name = "놈";
								break;
							default:
								mAssistPlayer.Name = "다오";
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 4)
					{
						mAssistPlayer = new Lore()
						{
							Level = battleCommand.Player.SummonMagic / 5,
							Concentration = 10 + GetBonusPoint(5),
							Agility = 0,
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							Experience = 0,
							PotentialExperience = 0,
						};

						if (mRand.Next(2) == 0)
						{
							mAssistPlayer.Name = "에인션트 나이트";
							mAssistPlayer.Class = 2;
							mAssistPlayer.ClassType = ClassCategory.Sword;
							mAssistPlayer.Strength = 12 + GetBonusPoint(5);
							mAssistPlayer.Mentality = 6 + GetBonusPoint(5);
							mAssistPlayer.Endurance = 15 + GetBonusPoint(5);
							mAssistPlayer.Resistance = 10 + GetBonusPoint(5);
							mAssistPlayer.Accuracy = 13 + GetBonusPoint(5);
							mAssistPlayer.SP = 0;
							mAssistPlayer.Weapon = 6;
							mAssistPlayer.WeaPower = 60 + GetBonusPoint(10);
							mAssistPlayer.Shield = 2;
							mAssistPlayer.ShiPower = 2;
							mAssistPlayer.Armor = 3 + GetBonusPoint(1);
							mAssistPlayer.PotentialAC = 2;
							mAssistPlayer.AC = mAssistPlayer.Armor + mAssistPlayer.PotentialAC;
							mAssistPlayer.SwordSkill = battleCommand.Player.SummonMagic;
							mAssistPlayer.AxeSkill = 0;
							mAssistPlayer.SpearSkill = 0;
							mAssistPlayer.BowSkill = 0;
							mAssistPlayer.FistSkill = 0;
							mAssistPlayer.ShieldSkill = battleCommand.Player.SummonMagic;
						}
						else
						{
							mAssistPlayer.Name = "에인션트 메이지";
							mAssistPlayer.Class = 1;
							mAssistPlayer.ClassType = ClassCategory.Magic;
							mAssistPlayer.Strength = 7 + GetBonusPoint(5);
							mAssistPlayer.Mentality = 12 + GetBonusPoint(5);
							mAssistPlayer.Endurance = 7 + GetBonusPoint(5);
							mAssistPlayer.Resistance = 10 + GetBonusPoint(5);
							mAssistPlayer.Accuracy = 13 + GetBonusPoint(5);
							mAssistPlayer.SP = mAssistPlayer.Mentality * mAssistPlayer.Level;
							mAssistPlayer.Weapon = 29;
							mAssistPlayer.WeaPower = 2;
							mAssistPlayer.Shield = 0;
							mAssistPlayer.ShiPower = 0;
							mAssistPlayer.Armor = 0;
							mAssistPlayer.PotentialAC = 0;
							mAssistPlayer.AC = 0;
							mAssistPlayer.SwordSkill = 0;
							mAssistPlayer.AxeSkill = 0;
							mAssistPlayer.SpearSkill = 0;
							mAssistPlayer.BowSkill = 0;
							mAssistPlayer.FistSkill = 0;
							mAssistPlayer.ShieldSkill = 0;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 5)
					{
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Neutral,
							Class = 0,
							ClassType = ClassCategory.Unknown,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Agility = 0,
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(8))
						{
							case 0:
								mAssistPlayer.Name = "밴더스내치";
								mAssistPlayer.Endurance = 15 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 8 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 12 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 33;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 3;
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 3;
								break;
							case 1:
								mAssistPlayer.Name = "캐리온 크롤러";
								mAssistPlayer.Endurance = 20 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 14 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 13 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 34;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic;
								mAssistPlayer.PotentialAC = 3;
								mAssistPlayer.AC = 3;
								break;
							case 2:
								mAssistPlayer.Name = "켄타우루스";
								mAssistPlayer.Endurance = 17 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 12 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 18 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 35;
								mAssistPlayer.WeaPower = (int)Math.Round(battleCommand.Player.SummonMagic * 1.5);
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
							case 3:
								mAssistPlayer.Name = "데모고르곤";
								mAssistPlayer.Endurance = 18 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 5 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 17 + GetBonusPoint(3);
								mAssistPlayer.Weapon = 36;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 4;
								mAssistPlayer.PotentialAC = 4;
								mAssistPlayer.AC = 4;
								break;
							case 4:
								mAssistPlayer.Name = "듈라한";
								mAssistPlayer.Endurance = 10 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 20;
								mAssistPlayer.Accuracy = 17;
								mAssistPlayer.Weapon = 16;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic;
								mAssistPlayer.PotentialAC = 3;
								mAssistPlayer.AC = 3;
								break;
							case 5:
								mAssistPlayer.Name = "에틴";
								mAssistPlayer.Endurance = 10 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 10;
								mAssistPlayer.Accuracy = 10 + GetBonusPoint(9);
								mAssistPlayer.Weapon = 8;
								mAssistPlayer.WeaPower = (int)Math.Round(battleCommand.Player.SummonMagic * 0.8);
								mAssistPlayer.PotentialAC = 1;
								mAssistPlayer.AC = 1;
								break;
							case 6:
								mAssistPlayer.Name = "헬하운드";
								mAssistPlayer.Endurance = 14 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 9 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 11 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 33;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 3;
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
							case 7:
								mAssistPlayer.Name = "미노타우루스";
								mAssistPlayer.Endurance = 13 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 11 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 14 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 9;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 3;
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 6)
					{
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Male,
							Class = 0,
							ClassType = ClassCategory.Unknown,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Agility = 0,
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(8))
						{
							case 0:
								mAssistPlayer.Name = "밴더스내치";
								mAssistPlayer.Endurance = 15 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 8 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 12 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 33;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 3;
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 3;
								break;
							case 1:
								mAssistPlayer.Name = "캐리온 크롤러";
								mAssistPlayer.Endurance = 20 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 14 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 13 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 34;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic;
								mAssistPlayer.PotentialAC = 3;
								mAssistPlayer.AC = 3;
								break;
							case 2:
								mAssistPlayer.Name = "켄타우루스";
								mAssistPlayer.Endurance = 17 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 12 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 18 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 35;
								mAssistPlayer.WeaPower = (int)Math.Round(battleCommand.Player.SummonMagic * 1.5);
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
							case 3:
								mAssistPlayer.Name = "데모고르곤";
								mAssistPlayer.Endurance = 18 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 5 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 17 + GetBonusPoint(3);
								mAssistPlayer.Weapon = 36;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 4;
								mAssistPlayer.PotentialAC = 4;
								mAssistPlayer.AC = 4;
								break;
							case 4:
								mAssistPlayer.Name = "듈라한";
								mAssistPlayer.Endurance = 10 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 20;
								mAssistPlayer.Accuracy = 17;
								mAssistPlayer.Weapon = 16;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic;
								mAssistPlayer.PotentialAC = 3;
								mAssistPlayer.AC = 3;
								break;
							case 5:
								mAssistPlayer.Name = "에틴";
								mAssistPlayer.Endurance = 10 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 10;
								mAssistPlayer.Accuracy = 10 + GetBonusPoint(9);
								mAssistPlayer.Weapon = 8;
								mAssistPlayer.WeaPower = (int)Math.Round(battleCommand.Player.SummonMagic * 0.8);
								mAssistPlayer.PotentialAC = 1;
								mAssistPlayer.AC = 1;
								break;
							case 6:
								mAssistPlayer.Name = "헬하운드";
								mAssistPlayer.Endurance = 14 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 9 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 11 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 33;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 3;
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
							case 7:
								mAssistPlayer.Name = "미노타우루스";
								mAssistPlayer.Endurance = 13 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 11 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 14 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 9;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 3;
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 6)
					{
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Male,
							Class = 0,
							ClassType = ClassCategory.Giant,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Agility = 0,
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(6))
						{
							case 0:
								mAssistPlayer.Name = "클라우드자이언트";
								mAssistPlayer.Endurance = 20 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 15 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 10 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 37;
								mAssistPlayer.WeaPower = (int)Math.Round(battleCommand.Player.SummonMagic * 2.5);
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
							case 1:
								mAssistPlayer.Name = "파이어 자이언트";
								mAssistPlayer.Endurance = 25 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 5 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 12 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 38;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 4;
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
							case 2:
								mAssistPlayer.Name = "프로스트자이언트";
								mAssistPlayer.Endurance = 30 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 8 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 8 + GetBonusPoint(2);
								mAssistPlayer.Weapon = 2;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 2;
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
							case 3:
								mAssistPlayer.Name = "힐 자이언트";
								mAssistPlayer.Endurance = 40 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 5 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 7 + GetBonusPoint(3);
								mAssistPlayer.Weapon = 39;
								mAssistPlayer.WeaPower = (int)Math.Round(battleCommand.Player.SummonMagic * 1.5);
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
							case 4:
								mAssistPlayer.Name = "스톤 자이언트";
								mAssistPlayer.Endurance = 20 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 10 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 11 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 37;
								mAssistPlayer.WeaPower = (int)Math.Round(battleCommand.Player.SummonMagic * 2.5);
								mAssistPlayer.PotentialAC = 4;
								mAssistPlayer.AC = 4;
								break;
							case 5:
								mAssistPlayer.Name = "스토엄 자이언트";
								mAssistPlayer.Endurance = 20 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 10 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 15 + GetBonusPoint(9);
								mAssistPlayer.Weapon = 40;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 6;
								mAssistPlayer.PotentialAC = 1;
								mAssistPlayer.AC = 1;
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 7)
					{
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Neutral,
							Class = 0,
							ClassType = ClassCategory.Golem,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Agility = 0,
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(4))
						{
							case 0:
								mAssistPlayer.Name = "글레이 고렘";
								mAssistPlayer.Endurance = 20 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 15 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 13 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 41;
								mAssistPlayer.WeaPower = (int)Math.Round(battleCommand.Player.SummonMagic * 0.5);
								mAssistPlayer.PotentialAC = 3;
								mAssistPlayer.AC = 3;
								break;
							case 1:
								mAssistPlayer.Name = "프레쉬 고렘";
								mAssistPlayer.Endurance = 20 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 10 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 12 + GetBonusPoint(5);
								mAssistPlayer.Weapon = 0;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic;
								mAssistPlayer.PotentialAC = 1;
								mAssistPlayer.AC = 1;
								break;
							case 2:
								mAssistPlayer.Name = "아이언 고렘";
								mAssistPlayer.Endurance = 20 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 5 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 10 + GetBonusPoint(2);
								mAssistPlayer.Weapon = 42;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 4;
								mAssistPlayer.PotentialAC = 5;
								mAssistPlayer.AC = 5;
								break;
							case 3:
								mAssistPlayer.Name = "스톤 고렘";
								mAssistPlayer.Endurance = 25 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 10 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 13 + GetBonusPoint(3);
								mAssistPlayer.Weapon = 0;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 2;
								mAssistPlayer.PotentialAC = 4;
								mAssistPlayer.AC = 4;
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 8)
					{
						mAssistPlayer = new Lore()
						{
							Endurance = 30 + GetBonusPoint(10),
							Resistance = 10 + GetBonusPoint(10),
							Accuracy = 15 + GetBonusPoint(5),
							WeaPower = battleCommand.Player.SummonMagic * mRand.Next(5) + 1,
							PotentialAC = 3,
							AC = 3,
							Class = 0,
							ClassType = ClassCategory.Dragon,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Agility = 0,
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(12))
						{
							case 0:
								mAssistPlayer.Name = "블랙 드래곤";
								mAssistPlayer.Weapon = 43;
								break;
							case 1:
								mAssistPlayer.Name = "블루 드래곤";
								mAssistPlayer.Weapon = 44;
								break;
							case 2:
								mAssistPlayer.Name = "블래스 드래곤";
								mAssistPlayer.Weapon = 45;
								break;
							case 3:
								mAssistPlayer.Name = "브론즈 드래곤";
								mAssistPlayer.Weapon = 44;
								break;
							case 4:
								mAssistPlayer.Name = "크로매틱 드래곤";
								mAssistPlayer.Weapon = 46;
								break;
							case 5:
								mAssistPlayer.Name = "코퍼 드래곤";
								mAssistPlayer.Weapon = 43;
								break;
							case 6:
								mAssistPlayer.Name = "골드 드래곤";
								mAssistPlayer.Weapon = 46;
								break;
							case 7:
								mAssistPlayer.Name = "그린 드래곤";
								mAssistPlayer.Weapon = 47;
								break;
							case 8:
								mAssistPlayer.Name = "플래티움 드래곤";
								mAssistPlayer.Weapon = 48;
								break;
							case 9:
								mAssistPlayer.Name = "레드 드래곤";
								mAssistPlayer.Weapon = 46;
								break;
							case 10:
								mAssistPlayer.Name = "실버 드래곤";
								mAssistPlayer.Weapon = 49;
								break;
							case 11:
								mAssistPlayer.Name = "화이트 드래곤";
								mAssistPlayer.Weapon = 49;
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 9)
					{
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Male,
							Class = 0,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Agility = 0,
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							FistSkill = 0
						};

						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Neutral,
							Class = 0,
							ClassType = ClassCategory.Golem,
							Level = battleCommand.Player.SummonMagic / 5,
							Strength = 10 + GetBonusPoint(5),
							Mentality = 10 + GetBonusPoint(5),
							Concentration = 10 + GetBonusPoint(5),
							Agility = 0,
							Luck = 10 + GetBonusPoint(5),
							Poison = 0,
							Unconscious = 0,
							Dead = 0,
							SP = 0,
							Experience = 0,
							PotentialExperience = 0,
							Shield = 0,
							ShiPower = 0,
							Armor = 0,
							SwordSkill = 0,
							AxeSkill = 0,
							SpearSkill = 0,
							BowSkill = 0,
							FistSkill = 0
						};

						switch (mRand.Next(2))
						{
							case 0:
								mAssistPlayer.Name = "늑대 인간";
								mAssistPlayer.ClassType = ClassCategory.Unknown;
								mAssistPlayer.Endurance = 25;
								mAssistPlayer.Resistance = 15;
								mAssistPlayer.Accuracy = 18;
								mAssistPlayer.Weapon = 36;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 3;
								mAssistPlayer.PotentialAC = 2;
								mAssistPlayer.AC = 2;
								break;
							case 1:
								mAssistPlayer.Name = "드래곤 뉴트";
								mAssistPlayer.ClassType = ClassCategory.Dragon;
								mAssistPlayer.Endurance = 30;
								mAssistPlayer.Resistance = 18;
								mAssistPlayer.Accuracy = 19;
								mAssistPlayer.Weapon = 6;
								mAssistPlayer.WeaPower = (int)(Math.Round(battleCommand.Player.SummonMagic * 4.5));
								mAssistPlayer.PotentialAC = 4;
								mAssistPlayer.AC = 4;
								break;
							case 2:
								mAssistPlayer.Name = "아이언 고렘";
								mAssistPlayer.Endurance = 20 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 5 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 10 + GetBonusPoint(2);
								mAssistPlayer.Weapon = 42;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 4;
								mAssistPlayer.PotentialAC = 5;
								mAssistPlayer.AC = 5;
								break;
							case 3:
								mAssistPlayer.Name = "스톤 고렘";
								mAssistPlayer.Endurance = 25 + GetBonusPoint(5);
								mAssistPlayer.Resistance = 10 + GetBonusPoint(5);
								mAssistPlayer.Accuracy = 13 + GetBonusPoint(3);
								mAssistPlayer.Weapon = 0;
								mAssistPlayer.WeaPower = battleCommand.Player.SummonMagic * 2;
								mAssistPlayer.PotentialAC = 4;
								mAssistPlayer.AC = 4;
								break;
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}

					DisplayPlayerInfo();
				}
				else if (battleCommand.Method == 8)
				{
					GetBattleStatus(null);

					if (mRand.Next(50) > battleCommand.Player.Agility)
						battleResult.Add($"그러나, 일행은 성공하지 못했다");
					else
					{
						mBattleTurn = BattleTurn.RunAway;
						battleResult.Add($"[color={RGB.LightCyan}]성공적으로 도망을 갔다[/color]");

						mParty.Etc[5] = 2;
					}
				}

				ShowBattleResult(battleResult);
			}
			else
			{
				BattleEnemyData enemy = null;

				do
				{
					if (mBatteEnemyQueue.Count == 0)
						break;

					enemy = mBatteEnemyQueue.Dequeue();

					if (enemy.Posion)
					{
						if (enemy.Unconscious)
							enemy.Dead = true;
						else
						{
							enemy.HP--;
							if (enemy.HP <= 0)
								enemy.Unconscious = true;
						}
					}

					if (!enemy.Unconscious && !enemy.Dead)
						break;
					else
						enemy = null;
				} while (mBatteEnemyQueue.Count > 0);


				if (enemy == null)
				{
					mParty.Etc[5] = 0;
					mBattleTurn = BattleTurn.Win;
					ContinueText.Visibility = Visibility.Visible;
				}
				else
				{
					var battleResult = new List<string>();

					var liveEnemyCount = 0;
					foreach (var otherEnemy in mEncounterEnemyList)
					{
						if (!otherEnemy.Dead)
							liveEnemyCount++;
					}

					if (enemy.SpecialCastLevel > 0 && enemy.ENumber > 0)
					{
						if (liveEnemyCount < (mRand.Next(3) + 2) && mRand.Next(3) == 0)
						{
							var newEnemy = JoinEnemy(enemy.ENumber + mRand.Next(4) - 20);
							DisplayEnemy();
							battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {newEnemy.NameJosa} 생성시켰다[/color]");
						}

						if (enemy.SpecialCastLevel > 1)
						{
							liveEnemyCount = 0;
							foreach (var otherEnemy in mEncounterEnemyList)
							{
								if (!otherEnemy.Dead)
									liveEnemyCount++;
							}

							if (mAssistPlayer != null && liveEnemyCount < 7 && (mRand.Next(5) == 0))
							{
								var turnEnemy = TurnMind(mAssistPlayer);
								mAssistPlayer = null;

								DisplayPlayerInfo();
								DisplayEnemy();

								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {turnEnemy.NameJosa} 자기편으로 끌어들였다[/color]");
							}
						}

						if (enemy.SpecialCastLevel > 2 && enemy.Special != 0 && mRand.Next(5) == 0)
						{
							void Cast(Lore player)
							{
								if (player.Dead == 0)
								{
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {player.Name}에게 죽음의 공격을 시도했다[/color]");

									if (mRand.Next(60) > player.Agility)
										battleResult.Add($"죽음의 공격은 실패했다");
									else if (mRand.Next(20) < player.Luck)
										battleResult.Add($"그러나, {player.NameSubjectJosa} 죽음의 공격을 피했다");
									else
									{
										battleResult.Add($"[color={RGB.Red}]{player.NameSubjectJosa} 죽었다!![/color]");

										if (player.Dead == 0)
										{
											player.Dead = 1;
											if (player.HP > 0)
												player.HP = 0;
										}
									}
								}
							}

							foreach (var player in mPlayerList)
							{
								Cast(player);
							}

							if (mAssistPlayer != null)
								Cast(mAssistPlayer);
						}
					}

					var agility = enemy.Agility;
					if (agility > 20)
						agility = 20;

					var specialAttack = false;
					if (enemy.Special > 0 && mRand.Next(50) < agility)
					{
						void EnemySpecialAttack()
						{
							if (enemy.Special == 1)
							{
								var normalList = new List<Lore>();

								foreach (var player in mPlayerList)
								{
									if (player.Poison == 0)
										normalList.Add(player);
								}

								if (mAssistPlayer != null && mAssistPlayer.Poison == 0)
									normalList.Add(mAssistPlayer);

								var destPlayer = normalList[mRand.Next(normalList.Count)];

								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {destPlayer.Name}에게 독 공격을 시도했다[/color]");
								if (mRand.Next(40) > enemy.Agility)
								{
									battleResult.Add($"독 공격은 실패했다");
									return;
								}

								if (mRand.Next(20) < destPlayer.Luck)
								{
									battleResult.Add($"그러나, {destPlayer.NameSubjectJosa} 독 공격을 피했다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{destPlayer.NameSubjectJosa} 중독 되었다!![/color]");

								if (destPlayer.Poison == 0)
									destPlayer.Poison = 1;
							}
							else if (enemy.Special == 2)
							{
								var normalList = new List<Lore>();

								foreach (var player in mPlayerList)
								{
									if (player.Unconscious == 0)
										normalList.Add(player);
								}

								if (mAssistPlayer != null && mAssistPlayer.Poison == 0)
									normalList.Add(mAssistPlayer);

								var destPlayer = normalList[mRand.Next(normalList.Count)];

								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {destPlayer.Name}에게 치명적 공격을 시도했다[/color]");
								if (mRand.Next(50) > enemy.Agility)
								{
									battleResult.Add($"치명적 공격은 실패했다");
									return;
								}

								if (mRand.Next(20) < destPlayer.Luck)
								{
									battleResult.Add($"그러나, {destPlayer.NameSubjectJosa} 치명적 공격을 피했다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{destPlayer.NameSubjectJosa} 의식불명이 되었다!![/color]");

								if (destPlayer.Unconscious == 0)
								{
									destPlayer.Unconscious = 1;

									if (destPlayer.HP > 0)
										destPlayer.HP = 0;
								}
							}
							else if (enemy.Special == 3)
							{
								var normalList = new List<Lore>();

								foreach (var player in mPlayerList)
								{
									if (player.Dead == 0)
										normalList.Add(player);
								}

								if (mAssistPlayer != null && mAssistPlayer.Poison == 0)
									normalList.Add(mAssistPlayer);

								var destPlayer = normalList[mRand.Next(normalList.Count)];

								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {destPlayer.Name}에게 죽음의 공격을 시도했다[/color]");
								if (mRand.Next(60) > enemy.Agility)
								{
									battleResult.Add($"죽음의 공격은 실패했다");
									return;
								}

								if (mRand.Next(20) < destPlayer.Luck)
								{
									battleResult.Add($"그러나, {destPlayer.NameSubjectJosa} 죽음의 공격을 피했다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{destPlayer.NameSubjectJosa} 죽었다!![/color]");

								if (destPlayer.Dead == 0)
								{
									destPlayer.Dead = 1;

									if (destPlayer.HP > 0)
										destPlayer.HP = 0;
								}
							}
						}

						liveEnemyCount = 0;
						foreach (var otherEnemy in mEncounterEnemyList)
						{
							if (!otherEnemy.Dead)
								liveEnemyCount++;
						}

						if (liveEnemyCount > 3)
						{
							EnemySpecialAttack();
							specialAttack = true;
						}
					}


					if (!specialAttack)
					{
						void EnemyAttack()
						{
							if (mRand.Next(20) >= enemy.Accuracy[0])
							{
								battleResult.Add($"{enemy.NameSubjectJosa} 빗맞추었다");
								return;
							}

							var normalList = new List<Lore>();

							foreach (var player in mPlayerList)
							{
								if (player.IsAvailable)
									normalList.Add(player);
							}

							if (mAssistPlayer != null && mAssistPlayer.IsAvailable)
								normalList.Add(mAssistPlayer);

							var destPlayer = normalList[mRand.Next(normalList.Count)];

							if (destPlayer.ClassType == ClassCategory.Sword && destPlayer.Shield > 0)
							{
								if (mRand.Next(550) < destPlayer.ShieldSkill * destPlayer.ShiPower)
								{
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {destPlayer.NameJosa} 공격했다[/color]");
									battleResult.Add($"그러나, {destPlayer.NameSubjectJosa} 방패로 적의 공격을 저지했다");
									return;
								}
							}

							var attackPoint = enemy.Strength * enemy.Level * (mRand.Next(10) + 1) / 5 - (destPlayer.AC * destPlayer.Level * (mRand.Next(10) + 1) / 10);

							if (attackPoint <= 0)
							{
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {destPlayer.NameJosa} 공격했다[/color]");
								battleResult.Add($"그러나, {destPlayer.NameSubjectJosa} 적의 공격을 방어했다");
								return;
							}

							if (destPlayer.Dead > 0)
								destPlayer.Dead += attackPoint;

							if (destPlayer.Unconscious > 0 && destPlayer.Dead == 0)
								destPlayer.Unconscious += attackPoint;

							if (destPlayer.HP > 0)
								destPlayer.HP -= attackPoint;

							battleResult.Add($"[color={RGB.LightMagenta}]{destPlayer.NameSubjectJosa} {enemy.Name}에게 공격받았다[/color]");
							battleResult.Add($"[color={RGB.Magenta}]{destPlayer.NameSubjectJosa}[/color] [color={RGB.LightMagenta}]{attackPoint.ToString("#,#0")}[/color][color={RGB.Magenta}]만큼의 피해를 입었다[/color]");
						}

						if (mRand.Next(enemy.Accuracy[0] * 1_000) > mRand.Next(enemy.Accuracy[1] * 1_000) && enemy.Strength > 0 || enemy.CastLevel == 0)
						{
							EnemyAttack();
						}
						else
						{
							void CastAttack(int castPower, Lore player)
							{
								if (mRand.Next(20) >= enemy.Accuracy[1])
								{
									battleResult.Add($"{enemy.Name}의 마법공격은 빗나갔다");
									return;
								}

								if (mRand.Next(50) < player.Resistance)
								{
									battleResult.Add($"그러나, {player.NameSubjectJosa} 적의 마법을 저지했다");
									return;
								}

								castPower -= mRand.Next(castPower / 2);
								castPower -= player.AC * player.Level * (mRand.Next(10) + 1) / 10;
								if (castPower <= 0)
								{
									battleResult.Add($"그러나, {player.NameSubjectJosa} 적의 마법을 막아냈다");
									return;
								}

								if (player.Dead > 0)
									player.Dead += castPower;

								if (player.Unconscious > 0 && player.Dead == 0)
									player.Unconscious += castPower;

								if (player.HP > 0)
									player.HP -= castPower;

								battleResult.Add($"[color={RGB.Magenta}]{player.NameSubjectJosa}[/color] [color={RGB.LightMagenta}]{castPower.ToString("#,#0")}[/color][color={RGB.Magenta}]만큼의 피해를 입었다[/color]");
							}

							void CastAttackOne(Lore player)
							{
								string castName;
								int castPower;
								if (1 <= enemy.Mentality && enemy.Mentality <= 3)
								{
									castName = "충격";
									castPower = 1;
								}
								else if (4 <= enemy.Mentality && enemy.Mentality <= 8)
								{
									castName = "냉기";
									castPower = 2;
								}
								else if (9 <= enemy.Mentality && enemy.Mentality <= 10)
								{
									castName = "고통";
									castPower = 4;
								}
								else if (11 <= enemy.Mentality && enemy.Mentality <= 14)
								{
									castName = "혹한";
									castPower = 6;
								}
								else if (15 <= enemy.Mentality && enemy.Mentality <= 18)
								{
									castName = "화염";
									castPower = 7;
								}
								else
								{
									castName = "번개";
									castPower = 10;
								}

								castPower *= enemy.Level;
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {player.Name}에게 {castName}마법을 사용했다[/color]");

								CastAttack(castPower, player);
							}

							void CastAttackAll(List<Lore> destPlayerList)
							{
								string castName;
								int castPower;
								if (1 <= enemy.Mentality && enemy.Mentality <= 6)
								{
									castName = "열파";
									castPower = 1;
								}
								else if (7 <= enemy.Mentality && enemy.Mentality <= 12)
								{
									castName = "에너지";
									castPower = 2;
								}
								else if (13 <= enemy.Mentality && enemy.Mentality <= 16)
								{
									castName = "초음파";
									castPower = 3;
								}
								else if (17 <= enemy.Mentality && enemy.Mentality <= 20)
								{
									castName = "혹한기";
									castPower = 5;
								}
								else
								{
									castName = "화염폭풍";
									castPower = 8;
								}

								castPower *= enemy.Level;
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} 일행 모두에게 {castName}마법을 사용했다[/color]");

								foreach (var player in destPlayerList)
									CastAttack(castPower, player);
							}

							void CureEnemy(BattleEnemyData whomEnemy, int curePoint)
							{
								if (enemy == whomEnemy)
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} 자신을 치료했다[/color]");
								else
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {whomEnemy.NameJosa} 치료했다[/color]");

								if (whomEnemy.Dead)
									whomEnemy.Dead = false;
								else if (whomEnemy.Unconscious)
								{
									whomEnemy.Unconscious = false;
									if (whomEnemy.HP <= 0)
										whomEnemy.HP = 1;
								}
								else
								{
									whomEnemy.HP += curePoint;
									if (whomEnemy.HP > whomEnemy.Endurance * whomEnemy.Level * 10)
										whomEnemy.HP = whomEnemy.Endurance * whomEnemy.Level * 10;
								}
							}

							void CastHighLevel(List<Lore> destPlayerList)
							{
								if ((enemy.HP < enemy.Endurance * enemy.Level * 4) && mRand.Next(3) == 0)
								{
									CureEnemy(enemy, enemy.Level * enemy.Mentality * 3);
									return;
								}

								var avgAC = 0;
								var avgCount = 0;

								foreach (var player in mPlayerList)
								{
									if (player.IsAvailable)
									{
										avgAC += player.AC;
										avgCount++;
									}
								}

								if (mAssistPlayer != null && mAssistPlayer.IsAvailable)
								{
									avgAC += mAssistPlayer.AC;
									avgCount++;
								}

								avgAC /= avgCount;

								if (avgAC > 4 && mRand.Next(5) == 0)
								{
									void BreakArmor(Lore player)
									{
										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {player.Name}의 갑옷파괴를 시도했다[/color]");
										if (player.Luck > mRand.Next(21))
											battleResult.Add($"그러나, {enemy.NameSubjectJosa} 성공하지 못했다");
										else
										{
											battleResult.Add($"[color={RGB.Magenta}]{player.Name}의 갑옷은 파괴되었다[/color]");

											if (player.AC > 0)
												player.AC--;
										}
									}

									foreach (var player in mPlayerList)
									{
										BreakArmor(player);
									}

									if (mAssistPlayer != null)
										BreakArmor(mAssistPlayer);

									DisplayPlayerInfo();
								}
								else
								{
									var totalCurrentHP = 0;
									var totalFullHP = 0;

									foreach (var enemyOne in mEncounterEnemyList)
									{
										totalCurrentHP += enemyOne.HP;
										totalFullHP += enemyOne.Endurance * enemyOne.Level * 10;
									}

									totalFullHP /= 3;

									if (mEncounterEnemyList.Count > 2 && totalCurrentHP < totalFullHP && mRand.Next(3) != 0)
									{
										foreach (var enemyOne in mEncounterEnemyList)
											CureEnemy(enemyOne, enemy.Level * enemy.Mentality * 2);
									}
									else if (mRand.Next(destPlayerList.Count) < 2)
									{
										Lore weakestPlayer = null;

										foreach (var player in mPlayerList)
										{
											if (player.IsAvailable && (weakestPlayer == null || weakestPlayer.HP > player.HP))
												weakestPlayer = player;
										}

										if (mAssistPlayer != null && mAssistPlayer.IsAvailable && (weakestPlayer == null || mAssistPlayer.HP < weakestPlayer.HP))
											weakestPlayer = mAssistPlayer;

										CastAttackOne(weakestPlayer);
									}
									else
										CastAttackAll(destPlayerList);
								}
							}


							var normalList = new List<Lore>();

							foreach (var player in mPlayerList)
							{
								if (player.IsAvailable)
									normalList.Add(player);
							}

							if (mAssistPlayer != null && mAssistPlayer.IsAvailable)
								normalList.Add(mAssistPlayer);

							var destPlayer = normalList[mRand.Next(normalList.Count)];

							if (enemy.CastLevel == 1)
							{
								CastAttackOne(destPlayer);
							}
							else if (enemy.CastLevel == 2)
							{
								CastAttackOne(destPlayer);
							}
							else if (enemy.CastLevel == 3)
							{
								if (mRand.Next(normalList.Count) < 2)
									CastAttackOne(destPlayer);
								else
									CastAttackAll(normalList);
							}
							else if (enemy.CastLevel == 4)
							{
								if ((enemy.HP < enemy.Endurance * enemy.Level * 3) && mRand.Next(2) == 0)
									CureEnemy(enemy, enemy.Level * enemy.Mentality * 3);
								else if (mRand.Next(normalList.Count) < 2)
									CastAttackOne(destPlayer);
								else
									CastAttackAll(normalList);
							}
							else if (enemy.CastLevel == 5)
							{
								if ((enemy.HP < enemy.Endurance * enemy.Level * 3) && mRand.Next(3) == 0)
									CureEnemy(enemy, enemy.Level * enemy.Mentality * 3);
								else if (mRand.Next(normalList.Count) < 2)
								{
									var totalCurrentHP = 0;
									var totalFullHP = 0;

									foreach (var enemyOne in mEncounterEnemyList)
									{
										totalCurrentHP += enemyOne.HP;
										totalFullHP += enemyOne.Endurance * enemyOne.Level * 10;
									}

									totalFullHP /= 3;

									if (mEncounterEnemyList.Count > 2 && totalCurrentHP < totalFullHP && mRand.Next(2) == 0)
									{
										foreach (var enemyOne in mEncounterEnemyList)
											CureEnemy(enemyOne, enemy.Level * enemy.Mentality * 2);
									}
									else
									{
										Lore weakestPlayer = null;

										foreach (var player in mPlayerList)
										{
											if (player.IsAvailable && (weakestPlayer == null || weakestPlayer.HP > player.HP))
												weakestPlayer = player;
										}

										if (mAssistPlayer != null && mAssistPlayer.IsAvailable && mAssistPlayer.HP < weakestPlayer.HP)
											weakestPlayer = mAssistPlayer;

										CastAttackOne(weakestPlayer);
									}
								}
								else
									CastAttackAll(normalList);
							}
							else if (enemy.CastLevel >= 6)
							{
								CastHighLevel(normalList);
							}
						}
					}

					ShowBattleResult(battleResult);
				}
			}
		}

		private void ShowWeaponShopMenu()
		{
			AppendText(new string[] {
				$"[color={RGB.White}]여기는 무기상점입니다.[/color]",
				$"[color={RGB.White}]우리들은 무기, 방패, 갑옷을 팔고있습니다.[/color]",
				$"[color={RGB.White}]어떤 종류를 원하십니까?[/color]",
			});

			ShowMenu(MenuMode.ChooseWeaponType, new string[] {
				"베는 무기류",
				"찍는 무기류",
				"찌르는 무기류",
				"쏘는 무기류",
				"방패류",
				"갑옷류"
			});
		}

		private void ShowExpStoreMenu()
		{
			AppendText(new string[] {
					$"[color={RGB.White}] 여기에서는 황금의 양만큼 훈련을 시켜서 경험치를 올려 주는 곳입니다.[/color]",
					$"[color={RGB.LightCyan}] 원하시는 금액을 고르십시오.[/color]"
				});

			ShowMenu(MenuMode.BuyExp, new string[] {
					"금 10,000개; 소요시간 : 1 시간",
					"금 20,000개; 소요시간 : 2 시간",
					"금 30,000개; 소요시간 : 3 시간",
					"금 40,000개; 소요시간 : 4 시간",
					"금 50,000개; 소요시간 : 5 시간",
					"금 60,000개; 소요시간 : 6 시간",
					"금 70,000개; 소요시간 : 7 시간",
					"금 80,000개; 소요시간 : 8 시간",
					"금 90,000개; 소요시간 : 9 시간",
					"금 100,000개; 소요시간 : 10 시간",
				});
		}

		private void ShowItemStoreMenu()
		{
			AppendText(new string[] {
				$"[color={RGB.White}] 여기는 여러가지 물품을 파는 곳입니다.[/color]",
				"",
				$"[color={RGB.White}] 당신이 사고 싶은 물건을 고르십시오.[/color]"
			});

			ShowMenu(MenuMode.SelectItem, mItems);
		}

		private void ShowMedicineStoreMenu()
		{
			AppendText(new string[] {
				$"[color={RGB.White}] 여기는 약초를 파는 곳입니다.[/color]",
				"",
				$"[color={RGB.White}] 당신이 사고 싶은 약이나 약초를 고르십시오.[/color]"
			});

			ShowMenu(MenuMode.SelectMedicine, mMedicines);
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
			//if (mMapName == 1 || mMapName == 3 || mMapName == 4)
			//{
			//	mMapHeader.TileType = PositionType.Ground;
			//	musicUri = new Uri("ms-appx:///Assets/ground.mp3");
			//}
			//else if (6 <= mMapName && mMapName <= 9)
			//{
			//	mMapHeader.TileType = PositionType.Town;
			//	musicUri = new Uri("ms-appx:///Assets/town.mp3");
			//}
			//else if (mMapName == 2 || (10 <= mMapName && mMapName <= 17))
			//{
			//	mMapHeader.TileType = PositionType.Den;
			//	musicUri = new Uri("ms-appx:///Assets/den.mp3");
			//}
			//else
			//{
			//	mMapHeader.TileType = PositionType.Keep;
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

		private void PlusTime(int hour, int min, int sec)
		{
			mParty.Hour += hour;
			mParty.Min += min;
			mParty.Sec += sec;

			while (mParty.Sec > 59)
			{
				mParty.Sec -= 60;
				mParty.Min++;
			}

			while (mParty.Min > 59)
			{
				mParty.Min -= 60;
				mParty.Hour++;
			}

			while (mParty.Hour > 23)
			{
				mParty.Hour -= 24;
				mParty.Day++;
			}

			while (mParty.Day > 360)
			{
				mParty.Day -= 360;
				mParty.Year++;
			}

			UpdateView();

			CheckTimeEvent();
		}

		private async void CheckTimeEvent() {
			if (mTimeWatch) {
				if (mParty.Year > mWatchYear || 
					(mParty.Year == mWatchYear && mParty.Day > mWatchDay) ||
					(mParty.Year == mWatchYear && mParty.Day == mWatchDay && mParty.Hour > mWatchHour) ||
					(mParty.Year == mWatchYear && mParty.Day == mWatchDay && mParty.Hour == mWatchHour && mParty.Min >= mWatchMin)) {
					mTimeWatch = false;

					if (mTimeEvent == 0) {
						Dialog(" 갑자기 허공 속에서  에인션트 이블의 음성이 들려왔다.");
						InvokeAnimation(AnimationType.HearAncientEvil);
					}
					else if (mTimeEvent == 1) {
						mMapName = "Dome";

						await RefreshGame();

						for (var y = 0; y < mMapHeader.Height; y++) {
							for (var x = 0; x < mMapHeader.Width; x++) {
								if (GetTileInfo(x, y) == 0)
									UpdateTileInfo(x, y, 47);
							}
						}

						UpdateTileInfo(25, 13, 35);
						UpdateTileInfo(12, 26, 44);
						UpdateTileInfo(21, 34, 47);
						UpdateTileInfo(38, 28, 30);
						UpdateTileInfo(18, 53, 44);
						UpdateTileInfo(29, 62, 47);
						UpdateTileInfo(18, 72, 47);
						UpdateTileInfo(37, 40, 31);

						mXAxis = 24;
						mYAxis = 49;

						UpdateView();

						Dialog($"[color={RGB.LightBlue}] 휴...  저의 염력을 총동원하여 어느 정도 사람이 살 수 있는 도시를  건설해  보았습니다." +
						" 이제는 배리언트 피플즈의 사람들을 여기로 공간이동 시켜 보겠습니다.  이번에도 조금 기다려야 할 것입니다[/color]");

						AddNextTimeEvent(2, 30);
					}
					else if (mTimeEvent == 2) {
						mMapName = "Dome";

						var oriX = mXAxis;
						var oriY = mYAxis;

						await RefreshGame();

						mYAxis = oriX;
						mYAxis = oriY;

						while (27 > GetTileInfo(mXAxis, mYAxis) || GetTileInfo(mXAxis, mYAxis) > 47)
							mXAxis++;

						Dialog($"[color={RGB.LightBlue}] 이곳으로 오기 원하는  모든 배리언트 피플즈의 사람들을 이곳으로 불렀습니다." +
						"  여기의 사람들과 이야기를 나눈후 성밖으로 나올때 당신을 지상세계로 이동 시켜 주겠습니다. 좋은 이야기 많이 나누시기 바랍니다.[/color]");

						SetBit(32);
					}
					else if (mTimeEvent == 3) {
						Dialog(" 당신이 길을 가던중 당신의 동료였던 그 사람들을 만나게 되었다.");
						InvokeAnimation(AnimationType.MeetFriend);
					}
					else if (mTimeEvent == 4) {
						Talk(" 당신이 길을 가던중 갑자기 강한 기운이 둘러싸고 있음을 알아챘다.  기척을 없에고 투시로 주위를 보았을때 사방을 포위하고 있는 로어성의 병사들을 볼 수 있었다." +
						" 로드안이 천리안으로  우리들을 찾아내어  이곳으로 대군을 보낸 것임에 틀림없었다.  우리들은 곧 벌어질 전투를 위해 태세를 갖추었다." +
						" 당신이 투시로 주위를 경계하던중 놀랍게도 대군을 직접 지휘하는 로드안을 보았다.  그가 직접 나섰다면 그만큼 나의 승산도 줄어든다는걸  알고있었다." +
						"  이제 결론은 단 한가지였다.  바로 로드안이 있는쪽으로 뚫고 들어가 그와 상대하는 것이었다." +
						" 그리고 내가 그를 꺽어버린다면 대부분의 병사들은 사기를 잃고  도망가버릴 것이기 때문이다. 일행은  서로에게 신호를 보내며  나의 생각에 동의를 표했다." +
						" 이제는 지체할 시간이 없었다. 우리들은  곧바로 로드안이 있는 쪽을 향해 뚫고 들어가기 시작했다.", SpecialEventType.BattleLordAhn);
					}
				}
			}
		}

		private void AddNextTimeEvent(int eventNo, int nextMin)
		{
			mWatchYear = mParty.Year;
			mWatchDay = mParty.Day;
			mWatchHour = mParty.Hour;
			mWatchMin = mParty.Min;
			mWatchSec = mParty.Sec;

			mTimeWatch = true;
			mTimeEvent = eventNo;

			mWatchMin += 30;
			if (mWatchMin > 59)
			{
				mWatchMin -= 60;

				mWatchHour++;
				if (mWatchHour > 23)
				{
					mWatchHour -= 24;

					mWatchDay++;
					if (mWatchDay > 359)
					{
						mWatchDay -= 360;

						mWatchYear++;
					}
				}
			}
		}

		private void UpdateView()
		{
			if (mMapHeader.TileType == PositionType.Den)
				mDark = true;
			else
				mDark = false;

			if (10 <= mParty.Day % 30 + 1 && mParty.Day % 30 + 1 <= 20)
				mMoonLight = true;
			else
				mMoonLight = false;

			if (mMapHeader.TileType == PositionType.Den)
				mMoonLight = false;
			else if (mMapHeader.TileType == PositionType.Town)
				mMoonLight = true;

			if (7 > mParty.Hour || mParty.Hour > 17)
			{
				mDark = true;
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

			if (mMapHeader.TileType == PositionType.Den) {
				mXWide = 1;
				mYWide = 1;
			}


			if (mDark && mParty.Etc[0] > 0)
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
				else if (5 <= mParty.Etc[0] && mParty.Etc[0] <= 255)
				{
					if (mXWide < 4 && mYWide < 4)
					{
						mXWide = 3;
						mYWide = 3;
						mMoonLight = true;
					}
				}
			}


			if (mEbony & mParty.Etc[0] == 0) {
				mMoonLight = false;
				mXWide = 0;
				mYWide = 0;
			}

		}

		void SetBit(int bit) {
			var idx = (bit / 8) + 49;
			var val = bit % 8;

			mParty.Etc[idx] |= 1 << val;
		}

		bool GetBit(int bit)
		{
			var idx = (bit / 8) + 49;
			var val = bit % 8;

			return (mParty.Etc[idx] & (1 << val)) > 0;
		}

		private void EncounterEnemy()
		{
			int range;
			int init;
			if (mMapName == "Ground2" && !GetBit(3)) {
				range = 3;
				init = 17;
			}
			else if (mMapName == "Ground3" && !GetBit(4))
			{
				range = 4;
				init = 24;
			}
			else if (mMapName == "Ground4" && !GetBit(5))
			{
				range = 3;
				init = 34;
			}
			else if (mMapName == "Ground5" && !GetBit(6))
			{
				range = 2;
				init = 47;
			}
			else if (mMapName == "UnderGrd")
			{
				range = 4;
				init = 5;
			}
			else if (mMapName == "TrolTown" && !GetBit(4))
			{
				range = 2;
				init = 30;
			}
			else if (mMapName == "OrcTown" && !GetBit(3))
			{
				range = 4;
				init = 17;
			}
			else if (mMapName == "Vesper" && !GetBit(4))
			{
				range = 3;
				init = 25;
			}
			else if (mMapName == "Kobold" && !GetBit(5))
			{
				range = 3;
				init = 34;
			}
			else if (mMapName == "DracTown" && !GetBit(6))
			{
				range = 2;
				init = 47;
			}
			else {
				range = 0;
				init = 0;
			}

			if (range == 0)
				return;

			mTriggeredDownEvent = true;

			var enemyNumber = mRand.Next(mMaxEnemy) + 1;
			if (enemyNumber > mMaxEnemy)
				enemyNumber = mMaxEnemy;

			mEncounterEnemyList.Clear();
			for (var i = 0; i < enemyNumber; i++)
			{
				var enemyID = mRand.Next(range) + init;

				mEncounterEnemyList.Add(new BattleEnemyData(enemyID, mEnemyDataList[enemyID]));
			}

			DisplayEnemy();
			HideMap();

			var avgAgility = 0;
			mEncounterEnemyList.ForEach(delegate (BattleEnemyData enemy)
			{
				avgAgility += enemy.Agility;
			});

			avgAgility /= mEncounterEnemyList.Count;

			AppendText(new string[] {
				$"[color={RGB.LightMagenta}]적이 출현했다!!![/color]", "",
				$"[color={RGB.LightCyan}]적의 평균 민첩성 : {avgAgility}[/color]"
			});

			ShowMenu(MenuMode.BattleStart, new string[] {
				"적과 교전한다",
				"도망간다"
			});
		}


		private BattleEnemyData JoinEnemy(int ENumber)
		{
			BattleEnemyData enemy = new BattleEnemyData(ENumber, mEnemyDataList[ENumber]);

			AssignEnemy(enemy);

			return enemy;
		}

		private void AssignEnemy(BattleEnemyData enemy)
		{
			var inserted = false;
			for (var i = 0; i < mEncounterEnemyList.Count; i++)
			{
				if (mEncounterEnemyList[i].Dead)
				{
					mEncounterEnemyList[i] = enemy;
					inserted = true;
					break;
				}
			}

			if (!inserted)
			{
				if (mEncounterEnemyList.Count == 8)
					mEncounterEnemyList[mEncounterEnemyList.Count - 1] = enemy;
				else
					mEncounterEnemyList.Add(enemy);
			}
		}

		private BattleEnemyData TurnMind(Lore player)
		{
			var enemy = new BattleEnemyData(0, new EnemyData()
			{
				Name = player.Name,
				Strength = player.Strength,
				Mentality = player.Mentality,
				Endurance = player.Endurance,
				Resistance = player.Resistance * 5 > 99 ? 99 : player.Resistance * 5,
				Agility = player.Agility,
				Accuracy = new int[] { player.Accuracy, player.Accuracy },
				AC = player.AC,
				Special = player.Class == 7 ? 2 : 0,
				CastLevel = player.Level / 4,
				SpecialCastLevel = 0,
				Level = player.Level,
			});

			enemy.HP = enemy.Endurance * enemy.Level * 10;
			enemy.Posion = false;
			enemy.Unconscious = false;
			enemy.Dead = false;

			AssignEnemy(enemy);

			return enemy;
		}

		private void HealOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0 || whomPlayer.Poison > 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 치료될 상태가 아닙니다.");
			}
			else if (whomPlayer.HP >= whomPlayer.Endurance * whomPlayer.Level * 10)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 치료할 필요가 없습니다.");
			}
			else
			{
				var needSP = 2 * player.Level;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP(cureResult);
				}
				else
				{
					player.SP -= needSP;
					DisplaySP();

					whomPlayer.HP += needSP * 10;
					if (whomPlayer.HP > whomPlayer.Level * whomPlayer.Endurance * 10)
						whomPlayer.HP = whomPlayer.Level * whomPlayer.Endurance * 10;

					cureResult.Add($"[color={RGB.White}]{whomPlayer.NameSubjectJosa} 치료되어 졌습니다.[/color]");
				}
			}
		}

		private void CureOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 독이 치료될 상태가 아닙니다.");
			}
			else if (whomPlayer.Poison == 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 독에 걸리지 않았습니다.");
			}
			else if (player.SP < 15)
			{
				if (mParty.Etc[5] == 0)
					ShowNotEnoughSP(cureResult);

			}
			else
			{
				player.SP -= 15;
				DisplaySP();

				whomPlayer.Poison = 0;

				cureResult.Add($"[color={RGB.White}]{whomPlayer.Name}의 독은 제거 되었습니다.[/color]");
			}
		}

		private void ConsciousOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 의식이 돌아올 상태가 아닙니다.");
			}
			else if (whomPlayer.Unconscious == 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 의식불명이 아닙니다.");
			}
			else
			{
				var needSP = 10 * whomPlayer.Unconscious;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP(cureResult);
				}
				else
				{
					player.SP -= needSP;
					DisplaySP();

					whomPlayer.Unconscious = 0;
					if (whomPlayer.HP <= 0)
						whomPlayer.HP = 1;

					cureResult.Add($"[color={RGB.White}]{whomPlayer.NameSubjectJosa} 의식을 되찾았습니다.[/color]");
				}
			}
		}

		private void RevitalizeOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead == 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 아직 살아 있습니다.");
			}
			else
			{
				var needSP = player.Dead * 30;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP(cureResult);
				}
				else
				{
					player.SP -= needSP;
					DisplaySP();

					whomPlayer.Dead = 0;
					if (whomPlayer.Unconscious > whomPlayer.Endurance * whomPlayer.Level)
						whomPlayer.Unconscious = whomPlayer.Endurance * whomPlayer.Level;

					if (whomPlayer.Unconscious == 0)
						whomPlayer.Unconscious = 1;

					cureResult.Add($"[color={RGB.White}]{whomPlayer.NameSubjectJosa} 다시 생명을 얻었습니다.[/color]");

				}
			}
		}

		private void HealAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				HealOne(player, whomPlayer, cureResult);
			});
		}

		private void CureAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				CureOne(player, whomPlayer, cureResult);
			});
		}

		private void ConsciousAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				ConsciousOne(player, whomPlayer, cureResult);
			});
		}

		private void RevitalizeAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				RevitalizeOne(player, whomPlayer, cureResult);
			});
		}

		private void CureSpell(Lore player, Lore whomPlayer, int magic, List<string> cureResult = null)
		{
			switch (magic)
			{
				case 0:
					HealOne(player, whomPlayer, cureResult);
					break;
				case 1:
					CureOne(player, whomPlayer, cureResult);
					break;
				case 2:
					ConsciousOne(player, whomPlayer, cureResult);
					break;
				case 3:
					RevitalizeOne(player, whomPlayer, cureResult);
					break;
				case 4:
					RevitalizeOne(player, whomPlayer, cureResult);
					ConsciousOne(player, whomPlayer, cureResult);
					CureOne(player, whomPlayer, cureResult);
					HealOne(player, whomPlayer, cureResult);
					break;
			}

			UpdatePlayersStat();
		}

		private void CureAllSpell(Lore player, int magic, List<string> cureResult = null)
		{
			switch (magic)
			{
				case 0:
					HealAll(player, cureResult);
					break;
				case 1:
					CureAll(player, cureResult);
					break;
				case 2:
					CureAll(player, cureResult);
					HealAll(player, cureResult);
					break;
				case 3:
					ConsciousAll(player, cureResult);
					break;
				case 4:
					ConsciousAll(player, cureResult);
					CureAll(player, cureResult);
					HealAll(player, cureResult);
					break;
				case 5:
					RevitalizeAll(player, cureResult);
					break;
				case 6:
					RevitalizeAll(player, cureResult);
					ConsciousAll(player, cureResult);
					CureAll(player, cureResult);
					HealAll(player, cureResult);
					break;

			}

			UpdatePlayersStat();
		}

		private async Task RefreshGame()
		{
			mLoading = true;

			MapCanvas.Visibility = Visibility.Collapsed;

			AppendText("");
			await LoadMapData();
			InitializeMap();

			mLoading = false;
		}

		private async Task LoadMapData()
		{
			var mapFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/{mMapName.ToUpper()}.M"));
			var stream = (await mapFile.OpenReadAsync()).AsStreamForRead();
			var reader = new BinaryReader(stream);

			lock (mapLock)
			{
				mMapHeader = new MapHeader();

				var mapNameLen = reader.ReadByte();
				mMapHeader.ID = Encoding.UTF8.GetString(reader.ReadBytes(mapNameLen));

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

		private void DisplayEnemy()
		{
			for (var i = 0; i < mEnemyTextList.Count; i++)
			{
				if (i < mEncounterEnemyList.Count)
				{
					mEnemyBlockList[i].Visibility = Visibility.Visible;
					mEnemyTextList[i].Text = mEncounterEnemyList[i].Name;
					mEnemyTextList[i].Foreground = new SolidColorBrush(GetEnemyColor(mEncounterEnemyList[i]));
				}
				else
					mEnemyBlockList[i].Visibility = Visibility.Collapsed;
			}

			HideMap();
		}

		private Color GetEnemyColor(BattleEnemyData enemy)
		{
			if (enemy.Dead)
				return GetColor(RGB.Black);
			else if (enemy.HP == 0 || enemy.Unconscious)
				return GetColor(RGB.DarkGray);
			else if ((enemy.SpecialCastLevel & 0x80) > 0)
				return GetColor(RGB.LightGreen);
			else if (1 <= enemy.HP && enemy.HP <= 199)
				return GetColor(RGB.LightRed);
			else if (200 <= enemy.HP && enemy.HP <= 499)
				return GetColor(RGB.Red);
			else if (500 <= enemy.HP && enemy.HP <= 999)
				return GetColor(RGB.Brown);
			else if (1_000 <= enemy.HP && enemy.HP <= 1_999)
				return GetColor(RGB.Yellow);
			else if (2_000 <= enemy.HP && enemy.HP <= 3_999)
				return GetColor(RGB.Green);
			else
				return GetColor(RGB.LightGreen);
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

		private void Talk(string dialog, SpecialEventType eventType, bool append = false)
		{
			Talk(new string[] { dialog }, eventType, append);
		}

		private void Talk(string[] dialog, SpecialEventType eventType, bool append = false)
		{
			AppendText(dialog, append);

			mAfterDialogType = mRemainDialog.Count > 0 ? AfterDialogType.PressKey : AfterDialogType.None;

			mSpecialEvent = eventType;
			ContinueText.Visibility = Visibility.Visible;
		}

		private void Ask(string dialog, MenuMode menuMode, string[] menuList)
		{
			Ask(new string[] { dialog }, menuMode, menuList);
		}

		private void Ask(string[] dialog, MenuMode menuMode, string[] menuList)
		{
			AppendText(dialog);

			if (mRemainDialog.Count > 0)
			{
				mAfterDialogType = AfterDialogType.Menu;
				mAfterMenuMode = menuMode;
				mAfterMenuList = menuList;
				ContinueText.Visibility = Visibility.Visible;
			}
			else
			{
				ShowMenu(menuMode, menuList);
			}
		}

		private void ShowNotEnoughSP(List<string> result = null)
		{
			var message = "그러나, 마법 지수가 충분하지 않습니다.";
			if (result == null)
				AppendText(message);
			else
				result.Add(message);
		}

		private void ShowNotEnoughMoney(SpecialEventType specialEvent)
		{
			var noMoneyStr = "당신은 충분한 돈이 없습니다.";
			if (specialEvent == SpecialEventType.None)
				Dialog(noMoneyStr);
			else
				Talk(noMoneyStr, specialEvent);
		}

		private void ShowNoThanks()
		{
			Dialog("당신이 바란다면 ...");
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

				await LoadEnemyData();

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

		private async Task LoadEnemyData()
		{
			var enemyFileFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/EnemyData.json"));
			mEnemyDataList = JsonConvert.DeserializeObject<List<EnemyData>>(await FileIO.ReadTextAsync(enemyFileFile));
		}

		private async Task<bool> LoadFile(int id = 0)
		{
			mLoading = true;

			var storageFolder = ApplicationData.Current.LocalFolder;

			var idStr = "";
			if (id > 0)
				idStr = id.ToString();

			var saveFile = await storageFolder.CreateFileAsync($"mysticSave{idStr}.dat", CreationCollisionOption.OpenIfExists);
			var saveData = JsonConvert.DeserializeObject<SaveData>(await FileIO.ReadTextAsync(saveFile));

			if (saveData == null)
			{
				mLoading = false;
				return false;
			}

			mParty = saveData.Party;
			mPlayerList = saveData.PlayerList;
			mAssistPlayer = saveData.AssistPlayer;
			mMapHeader = saveData.MapHeader;
			mMapName = mMapHeader.ID;

			mXAxis = mMapHeader.StartX;
			mYAxis = mMapHeader.StartY;

			if (saveData.MapHeader.Layer == null || saveData.MapHeader.Layer.Length == 0)
			{
				await LoadMapData();
			}

			mEncounter = saveData.Encounter;
			if (1 > mEncounter || mEncounter > 3)
				mEncounter = 2;

			mMaxEnemy = saveData.MaxEnemy;
			if (3 > mMaxEnemy || mMaxEnemy > 7)
				mMaxEnemy = 5;

			mCruel = saveData.Cruel;
			
			DisplayPlayerInfo();

			InitializeMap();

			MapCanvas.Visibility = Visibility.Collapsed;

			mLoading = false;

			return true;
		}

		private async void InvokeAnimation(AnimationType animationEvent, int aniX = 0, int aniY = 0)
		{
			mAnimationEvent = animationEvent;

			var animationTask = Task.Run(() =>
			{
				void AnimateTransition()
				{
					for (var i = 1; i <= 117; i++)
					{
						mAnimationFrame = i;
						Task.Delay(5).Wait();
					}
				}

				if (mAnimationEvent == AnimationType.BuyExp)
				{
					var totalTrainTime = mTrainTime * 3;

					for (var i = 0; i < totalTrainTime; i++)
					{
						PlusTime(0, 20, 0);
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.HearAncientEvil)
					Task.Delay(1000).Wait();
				else if (mAnimationEvent == AnimationType.MeetFriend)
					Task.Delay(1000).Wait();
				else if (mAnimationEvent == AnimationType.LeftManager)
				{
					var tile = 43;
					for (var y = 32; y < 36; y++)
					{
						UpdateTileInfo(26, y - 1, tile);
						tile = GetTileInfo(26, y);
						UpdateTileInfo(26, y, 21);
						Task.Delay(1000).Wait();
					}
					UpdateTileInfo(26, 35, tile);
				}
				else if (mAnimationEvent == AnimationType.CaptureProtagonist)
				{
					for (var i = 1; i <= 5; i++)
					{
						mAnimationFrame = i;
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.GotoCourt)
				{
					AnimateTransition();
				}
				else if (mAnimationEvent == AnimationType.GotoCourt2)
				{
					for (var i = 1; i <= 6; i++)
					{
						mAnimationFrame = i;
						Task.Delay(700).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.SubmitProof)
				{
					for (var i = 1; i <= 6; i++)
					{
						mAnimationFrame = i;
						if (i == 3)
							Task.Delay(2000).Wait();
						else
							Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.GotoJail)
				{
					AnimateTransition();
				}
				else if (mAnimationEvent == AnimationType.LiveJail)
				{
					AnimateTransition();
					Task.Delay(2000).Wait();

				}
				else if (mAnimationEvent == AnimationType.MeetCanopus)
				{
					Task.Delay(2000).Wait();
				}
				else if (mAnimationEvent == AnimationType.PassCrystal)
				{
					AnimateTransition();
					Task.Delay(1000).Wait();
				}
				else if (mAnimationEvent == AnimationType.PassCrystal2)
				{
					for (var i = 1; i <= 5; i++)
					{
						mAnimationFrame = i;
						if (i == 3)
							Task.Delay(1000).Wait();
						else
							Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.SealCrystal || mAnimationEvent == AnimationType.SealCrystal2)
				{
					Task.Delay(1000).Wait();
				}
				else if (mAnimationEvent == AnimationType.TransformProtagonist)
					Task.Delay(3000).Wait();
				else if (mAnimationEvent == AnimationType.TransformProtagonist2)
					Task.Delay(2000).Wait();
				else if (mAnimationEvent == AnimationType.ComeSoldier) {
					for (var i = 1; i <= 3; i++) {
						mAnimationFrame = i;
						Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.FollowSoldier) {
					for (var i = 1; i <= 5; i++)
					{
						mAnimationFrame = i;
						if (i == 1)
							Task.Delay(2000).Wait();
						else
							Task.Delay(800).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.FollowSoldier2) {
					AnimateTransition();

					Task.Delay(2000).Wait();

					mXAxis = 50;
					mYAxis = 30;

					mFace = 1;
					if (mPlayerList[0].ClassType == ClassCategory.Magic)
						mFace += 8;

					for (var i = 118; i <= 123; i++) {
						mAnimationFrame = i;
						Task.Delay(700).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.LeaveSoldier) {
					for (var i = 1; i <= 6; i++) {
						mAnimationFrame = i;
						Task.Delay(700).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.VisitCanopus) {
					for (var i = 1; i <= 3; i++) {
						mAnimationFrame = i;
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.RequestPardon)
				{
					for (var i = 1; i <= 4; i++) {
						mAnimationFrame = i;
						Task.Delay(1500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.ConfirmPardon) {
					for (var i = 1; i <= 4; i++)
					{
						mAnimationFrame = i;
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.ConfirmPardon2) {
					AnimateTransition();
				}
				else if (mAnimationEvent == AnimationType.ConfirmPardon3)
				{
					for (var i = 1; i <= 5; i++)
					{
						mAnimationFrame = i;
						Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.JoinCanopus) {
					for (var i = 1; i <= 3; i++) {
						mAnimationFrame = i;

						if (i < 3)
							Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.LeavePrisonSoldier) {
					for (var i = 1; i <= 4; i++) {
						mAnimationFrame = i;
						Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.LeaveCanopus)
				{
					for (var i = 1; i <= 3; i++)
					{
						mAnimationFrame = i;
						Task.Delay(1000).Wait();
					}
				}
			});

			await animationTask;

			if (mAnimationEvent == AnimationType.HearAncientEvil)
			{
				mSpecialEvent = SpecialEventType.HearAncientEvil;
				ContinueText.Visibility = Visibility.Visible;
			}
			else if (mAnimationEvent == AnimationType.MeetFriend)
			{
				mSpecialEvent = SpecialEventType.MeetFriend;
				ContinueText.Visibility = Visibility.Visible;
			}
			else if (mAnimationEvent == AnimationType.CaptureProtagonist)
			{
				Talk(new string[] {
					" 갑자기 나타난 로어성의 병사들은 당신의 창을 빼앗아 들고 말했다.",
					"",
					$"[color={RGB.LightBlue}] 우리는 당신을 살인 혐의로 체포합니다.  그리고 이 창은 증거물로 압수 하겠습니다.  변명은 법정에서 하십시요.[/color]"
				}, SpecialEventType.GotoCourt);
			}
			else if (mAnimationEvent == AnimationType.GotoCourt)
			{
				mMapName = "Lore";

				await RefreshGame();

				mXAxis = 50;
				mYAxis = 30;

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				if (mPlayerList[0].ClassType == ClassCategory.Magic)
					mFace = 9;
				else
					mFace = 1;

				InvokeAnimation(AnimationType.GotoCourt2);
			}
			else if (mAnimationEvent == AnimationType.GotoCourt2)
			{
				Talk(new string[] {
					" 로드안이 병사에게 말했다.",
					"",
					$"[color={RGB.LightGreen}] 이 자는 누구인데 여기로 끌고 왔소?[/color]"
				}, SpecialEventType.Interrogation);
			}
			else if (mAnimationEvent == AnimationType.SubmitProof)
			{
				mSpecialEvent = SpecialEventType.Punishment;
				ContinueText.Visibility = Visibility.Visible;
			}
			else if (mAnimationEvent == AnimationType.GotoJail)
			{
				mXAxis = 38;
				mYAxis = 9;

				InvokeAnimation(AnimationType.MeetCanopus);
			}
			else if (mAnimationEvent == AnimationType.MeetCanopus)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				var tile = GetTileInfo(mXAxis + 2, mYAxis);
				UpdateTileInfo(mXAxis + 2, mYAxis, GetTileInfo(mXAxis + 1, mYAxis));
				UpdateTileInfo(mXAxis + 1, mYAxis, tile);

				Talk(new string[] {
					$"[color={RGB.LightBlue}] 나의 동료가 한 명 생겼군.  나는 1년 반 전에 절도 상해 혐의로  종신형을 선고 받은[/color] [color={RGB.LightCyan}]카노푸스[/color][color={RGB.LightBlue}]라고 하네." +
					"  우리는 같이 평생을 살 동이니 잘 지내도록 하지.[/color]"
				}, SpecialEventType.LiveJail);
			}
			else if (mAnimationEvent == AnimationType.LiveJail)
			{
				var tile = GetTileInfo(mXAxis + 2, mYAxis);
				UpdateTileInfo(mXAxis + 2, mYAxis, GetTileInfo(mXAxis + 1, mYAxis));
				UpdateTileInfo(mXAxis + 1, mYAxis, tile);

				mParty.Year = 674;
				mParty.Day = 18;
				mParty.Hour = 8;
				mParty.Min = 0;
				mParty.Sec = 0;
				UpdateView();

				Dialog(" 때는 674년 1월 19일.  당신은 여기서 약 두달을 지냈다.");

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.PassCrystal) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(new string[] {
				$"[color={RGB.LightGreen}] 오, {mPlayerList[0].Name} 공작![/color]",
				$"[color={RGB.LightGreen}] 당신은 나의 마지막 부탁까지 훌륭하게 들어주었소.  이제는 당신에게 더 이상 이런 일을 시키지 않겠다고 약속하오." +
				"  먼저, 네 종족의 원혼을 봉인한 크리스탈 볼을 이리 주시오.[/color]"
				}, SpecialEventType.PassCrystal);
			}
			else if (mAnimationEvent == AnimationType.PassCrystal2) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Dialog($"[color={RGB.LightGreen}] 이제는 이 수정구슬을 완전히 봉인해서 다시는 그들의 원혼이 우리를 위협하지 못하게 해야겠소.[/color]");
				InvokeAnimation(AnimationType.SealCrystal);
			}
			else if (mAnimationEvent == AnimationType.SealCrystal) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(new string[] {
					"",
					" 로드안의  손에 들린 크리스탈 볼이  가볍게 떠오르기 시작했다. 로드안은 차츰 양손의 기력을 증강 시켰고 크리스탈 볼은 붉게 달아오르기 시작했다." +
					" 곧이어 크리스탈 볼은 이글거리는 에너지 공으로 변하였다."
				}, SpecialEventType.SealCrystal, true);
			}
			else if (mAnimationEvent == AnimationType.SealCrystal2)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(new string[] {
					"",
					" 로드안이 양손을 내리자 에너지 공은 서서히 밑으로 떨어지기 시작했다.  그리고는 바닥에 스며드는 것처럼 땅속으로 사라져 버렸다."
				}, SpecialEventType.SendUranos, true);
			}
			else if (mAnimationEvent == AnimationType.TransformProtagonist) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(new string[] {
					"",
					" 지금  주위에는 아무도 없다는 것을  확인한 로드안은  갑자기 당신을 향해 독심을 사용하기 시작했다." +
					"  당신은 긴장을 완전히 풀고 있었던데다가 로드안의 갑작스런 최면에 말려들어  방어를 할 기회도 없이  그에게 독심술을 당하였다." +
					" 당신은 곧 당신의 자유의사를 잃어버렸고 정신이 아득해지기 시작하였다."
				}, SpecialEventType.TransformProtagonist2, true);
			}
			else if (mAnimationEvent == AnimationType.TransformProtagonist2) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(new string[] {
					$"[color={RGB.LightGreen}] 당신은 이제 악의 추종자로서의 형체를 갖추었다네. 4 m 가량의 키와 강력한 마력을 겸비한 악의 최대 전투사로서..." +
					$"  당신은 이제[/color] [color={RGB.LightCyan}]네크로만서[/color][color={RGB.LightGreen}]란 이름으로  이 세상의 평화가 오래 지속 될 때  이 세상을 위협하려 내려올 운명을 가지게 되었다네." +
					"  당신은 그때  우라누스 테라를 기점으로 세력을 뻗치게 될걸세. 그때는 당신이 우라누스 테라의 주인이 될거라네. 아마 그때서야  내가 당신에게 우라누스 테라를 주겠다는 약속이 지켜질 걸세.[/color]",
					"",
					$"[color={RGB.LightGreen}] 그럼... 안녕, 네크로만서.[/color]"
				}, SpecialEventType.SendNecromancer);
			}
			else if (mAnimationEvent == AnimationType.ComeSoldier)
			{
				Talk($"[color={RGB.LightBlue}] 어이, {mPlayerList[0].Name}. 로드안님이 널 찾으신다. 따라와![/color]", SpecialEventType.FollowSolider);
			}
			else if (mAnimationEvent == AnimationType.FollowSoldier)
			{
				InvokeAnimation(AnimationType.FollowSoldier2);
			}
			else if (mAnimationEvent == AnimationType.FollowSoldier2)
			{
				Talk(new string[] {
					$"[color={RGB.LightGreen}] 음.. 당신이 {mPlayerList[0].NameObjectJosa} 사람이오?[/color]",
					$"[color={RGB.LightGreen}] 내가 당신을 특별히 부른 이유는  다름이 아니라  당신을 특별 사면 시켜주려는 의도에서라오.[/color]"
				}, SpecialEventType.LordAhnMission);
			}
			else if (mAnimationEvent == AnimationType.LeaveSoldier) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(" 674년 1월 17일 날, 로어 세계에서는 커다란 참변이 있었소. 당신은 이전에도 인류와 타종족간에 사이가 않좋았다는 것을 기억 할 거요." +
				"또한  사소한 영토분쟁이 많았던 것도 사실이오." +
				"  그런데 바로 이날 수백년의 역사를 지님 아프로디테 테라의 베스퍼성이 트롤족에 의해 완전히 함락 당했던 것이오." +
				" 거기서 가까스로 탈출한 사람들의 말을 빌면 그 날 베스퍼성은 처절한 절규와  아비규환 속에서  아이들이나 부녀자 할것없이 모두 처참히 죽어 갔다고 하오." +
				"  트롤족은 시체가 즐비한 거리에서  환각 작용제를 맞으며  광적인 살인을 계속 해대었다고 하는 탈출자의 말을 듣고" +
				"  나는 분을 참지 못해 모든 성주들을 불러서 타종족과의 전쟁을 결의 했소. 이렇게 함으로서 베스퍼성에서 죽어간 사람들의 원혼을 달래 주려는 의도라오.",
				SpecialEventType.None);

				mParty.Etc[19] = 2;
				SetBit(0);
			}
			else if (mAnimationEvent == AnimationType.VisitCanopus) {
				Ask(new string[] {
					$"[color={RGB.LightBlue}] {mPlayerList[0].Name}.[/color]",
					$"[color={RGB.LightBlue}] 나도 역시 자네처럼 베스퍼성으로 가고 싶네. 이런 감옥에서 평생을 지내느니 한 번 모험을 해보는게 좋을 것 같아서 말이지." +
					"  나는 전사로서의 교육도 받았으니 아마 자네 일원 중에서는 가장 전투력이 뛰어날 걸세.[/color]"
				}, MenuMode.JoinCanopus, new string[] {
					"역시 카노푸스 자네 뿐이야",
					"자네 도움까지는 필요 없다네"
				});
			}
			else if (mAnimationEvent == AnimationType.RequestPardon) {
				Talk(" 그렇다면 카노푸스도 당신과 같은 목적을 가진다는 이유로  사면 시켜 달라는  애기군요. 그럼 로드안 님에게 물어보고 오겠습니다."
				, SpecialEventType.ConfirmPardon);
			}
			else if (mAnimationEvent == AnimationType.ConfirmPardon) {
				InvokeAnimation(AnimationType.ConfirmPardon2);
			}
			else if (mAnimationEvent == AnimationType.ConfirmPardon2) {
				Talk(" 그로부터 30분이 지났다.", SpecialEventType.ConfirmPardon2);
				PlusTime(0, 20, 0);
			}
			else if (mAnimationEvent == AnimationType.ConfirmPardon3) {
				Talk(" 카노푸스를 풀어줘도 좋다고  로드안님이 허락하셨습니다. 정말 희안한 일입니다. 예전에는 전혀 없던 일인데 ... 어쨌든 카노푸는 이제 자유의 몸입니다."
				, SpecialEventType.JoinCanopus);
			}
			else if (mAnimationEvent == AnimationType.JoinCanopus) {
				if (mPlayerList.Count < 5)
				{
					var canopus = new Lore()
					{
						Name = "카노푸스",
						Gender = GenderType.Male,
						Class = 7,
						ClassType = ClassCategory.Sword,
						Level = 5,
						Strength = 19,
						Mentality = 6,
						Concentration = 4,
						Endurance = 18,
						Resistance = 17,
						Agility = 20,
						Accuracy = 17,
						Luck = 12,
						Poison = 0,
						Unconscious = 0,
						Dead = 0,
						SP = 0,
						Experience = 0,
						Weapon = 0,
						Shield = 0,
						Armor = 0,
						PotentialAC = 0,
						SwordSkill = 20,
						AxeSkill = 10,
						SpearSkill = 5,
						BowSkill = 0,
						ShieldSkill = 50,
						FistSkill = 10
					};

					canopus.HP = canopus.Endurance * canopus.Level * 10;
					canopus.UpdatePotentialExperience();
					UpdateItem(canopus);

					mPlayerList.Add(canopus);
					DisplayPlayerInfo();

					UpdateTileInfo(40, 9, 44);
					SetBit(35);
				}

				InvokeAnimation(AnimationType.LeavePrisonSoldier);
			}
			else
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
		}

		private void ShowPartyStatus()
		{
			string CheckEnable(int i)
			{
				if (mParty.Etc[i] == 0)
					return "불가";
				else
					return "가능";
			}

			DialogText.Visibility = Visibility.Collapsed;
			PartyInfoPanel.Visibility = Visibility.Visible;

			XPosText.Text = (mXAxis + 1).ToString();
			YPosText.Text = (mYAxis + 1).ToString();

			FoodText.Text = mParty.Food.ToString();
			GoldText.Text = mParty.Gold.ToString("#,#0");
			ArrowText.Text = mParty.Arrow.ToString();

			EnableLightText.Text = CheckEnable(0);
			EnableLevitationText.Text = CheckEnable(3);
			EnableFloatingWaterText.Text = CheckEnable(1);
			EnableFloatingSwampText.Text = CheckEnable(2);

			HPPotionText.Text = mParty.Item[0].ToString();
			SPPotionText.Text = mParty.Item[1].ToString();
			AntidoteText.Text = mParty.Item[2].ToString();
			ConsciousText.Text = mParty.Item[3].ToString();
			RevivalText.Text = mParty.Item[4].ToString();

			SummonScrollText.Text = mParty.Item[5].ToString();
			BigTorchText.Text = mParty.Item[6].ToString();
			CrystalText.Text = mParty.Item[7].ToString();
			FlyingBootsText.Text = mParty.Item[8].ToString();
			TransportationMarbleText.Text = mParty.Item[9].ToString();

			DateText.Text = $"{mParty.Year}년 {mParty.Day / 30 + 1}월 {mParty.Day % 30 + 1}일";
			TimeText.Text = $"{mParty.Hour}시 {mParty.Min}분";
		}

		private Lore GetMemberFromEnemy(int id)
		{
			var enemy = mEnemyDataList[id];

			var player = new Lore()
			{
				Name = enemy.Name,
				Gender = GenderType.Neutral,
				Class = 0,
				Strength = enemy.Strength,
				Mentality = enemy.Mentality,
				Concentration = 0,
				Endurance = enemy.Endurance,
				Resistance = enemy.Resistance / 2,
				Agility = enemy.Agility,
				Accuracy = enemy.Accuracy[1],
				Luck = 10,
				Poison = 0,
				Unconscious = 0,
				Dead = 0,
				Level = enemy.Level,
				AC = enemy.AC,
				Weapon = 50,
				Shield = 6,
				Armor = 11
			};

			player.HP = player.Endurance * player.Level * 10;
			player.SP = player.Mentality * player.Level * 10;
			player.WeaPower = player.Level * 2 + 10;
			player.ShiPower = 0;
			player.ArmPower = player.AC;
			player.Experience = 0;

			return player;
		}

		private void JoinMemberFromEnemy(int id)
		{
			mAssistPlayer = GetMemberFromEnemy(id);

			DisplayPlayerInfo();
		}

		private void Rest()
		{
			AppendText($"[color={RGB.White}]일행이 여기서 쉴 시간을 지정 하십시오.[/color]");

			var rangeItems = new List<Tuple<string, int>>();
			for (var i = 1; i <= 24; i++)
			{
				rangeItems.Add(new Tuple<string, int>($"[color={RGB.White}]##[/color] [color={RGB.LightGreen}]{i}[/color][color={RGB.White}] 시간 동안[/color]", i));
			}

			ShowSpinner(SpinnerType.RestTimeRange, rangeItems.ToArray(), 0);
		}

		private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			void AnimateTransition(int frame, int x, int y)
			{
				// 총 117 프레임
				for (var i = 0; i < frame; i++)
				{
					args.DrawingSession.FillRectangle(new Rect((x - 4) * 52 + (i * 4), (y - 5) * 52, 2, 52 * 11), Colors.Black);
					args.DrawingSession.FillRectangle(new Rect((x - 4) * 52 + ((116 - i) * 4) + 2, (y - 5) * 52, 2, 52 * 11), Colors.Black);
				}
			}

			var playerX = mXAxis;
			var playerY = mYAxis;

			var xOffset = 0;
			var yOffset = 0;
			if (mTelescopeXCount != 0)
			{
				if (mTelescopeXCount < 0)
					xOffset = -(mTelescopePeriod - Math.Abs(mTelescopeXCount));
				else
					xOffset = mTelescopePeriod - Math.Abs(mTelescopeXCount);
			}

			if (mTelescopeYCount != 0)
			{
				if (mTelescopeYCount < 0)
					yOffset = -(mTelescopePeriod - Math.Abs(mTelescopeYCount));
				else
					yOffset = mTelescopePeriod - Math.Abs(mTelescopeYCount);
			}

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
					if (mAnimationEvent != AnimationType.GotoCourt2 && mAnimationEvent != AnimationType.FollowSoldier && mAnimationEvent != AnimationType.FollowSoldier2)
						mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), Vector4.One);

					if (mAnimationEvent == AnimationType.CaptureProtagonist && mAnimationFrame > 0)
					{
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(24, mYAxis + (6 - mAnimationFrame)), Vector4.One);
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(25, mYAxis + (6 - mAnimationFrame)), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.GotoCourt) {
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(24, mYAxis + 1), Vector4.One);
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(25, mYAxis + 1), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.GotoCourt2 && mAnimationFrame > 0)
					{
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis + (6 - mAnimationFrame)), Vector4.One);
						mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(50, mYAxis + (6 - mAnimationFrame)), Vector4.One);
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(51, mYAxis + (6 - mAnimationFrame)), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.SubmitProof && mAnimationFrame > 0)
					{
						if (mAnimationFrame <= 3)
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis - mAnimationFrame), Vector4.One);
						else
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis - 3 + (mAnimationFrame - 3)), Vector4.One);
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(51, mYAxis), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.GotoJail)
					{
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis), Vector4.One);
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(51, mYAxis), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.FollowSoldier) 
						mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX + (mAnimationFrame - 1), playerY), Vector4.One);
					else if (mAnimationEvent == AnimationType.FollowSoldier2 && mAnimationFrame >= 118)
					{
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis + (123 - mAnimationFrame)), Vector4.One);
						mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(50, mYAxis + (123 - mAnimationFrame)), Vector4.One);
					}
					else if (mAnimationEvent == AnimationType.LeaveSoldier && mAnimationFrame > 0)
					{
						mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis + (mAnimationFrame - 1)), Vector4.One);
					}
				}
			}

			if ((mAnimationEvent == AnimationType.GotoCourt ||
				mAnimationEvent == AnimationType.GotoJail ||
				mAnimationEvent == AnimationType.LiveJail || 
				mAnimationEvent == AnimationType.FollowSoldier2 ||
				mAnimationEvent == AnimationType.ConfirmPardon2) && mAnimationFrame <= 117)
				AnimateTransition(mAnimationFrame, playerX, playerY);
		}

		private void DrawTile(CanvasSpriteBatch sb, byte[] layer, int index, int playerX, int playerY)
		{
			int row = index / mMapHeader.Width;
			int column = index % mMapHeader.Width;

			Vector4 tint;

			var darkness = false;
			if (playerX - mXWide <= column && column <= playerX + mXWide && playerY - mYWide <= row && row <= playerY + mYWide)
				darkness = false;
			else
				darkness = true;

			if (!mDark)
				darkness = false;

			if (mEbony && mParty.Etc[0] == 0)
				darkness = true;

			if ((layer[index] & 0x80) > 0)
				tint = Vector4.One;
			else if (mMoonLight || !darkness)
			{
				if (darkness)
					tint = new Vector4(0.1f, 0.1f, 0.6f, 1);
				else
					tint = Vector4.One;
			}
			else
			{
				if (darkness)
					tint = new Vector4(0.0f, 0.0f, 0.0f, 1);
				else
					tint = Vector4.One;
			}

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

				if (mSpecialEvent == SpecialEventType.Penetration)
				{
					if ((mMapHeader.TileType == PositionType.Den || mMapHeader.TileType == PositionType.Keep) && tileIdx == 52)
						tileIdx = 0;
				}
				else if (tileIdx == 0)
					tileIdx = mMapHeader.Default;

				if (mAnimationEvent == AnimationType.ComeSoldier && mAnimationFrame > 0)
				{
					if (column == playerX + (5 - mAnimationFrame) && playerY == row)
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.FollowSoldier && mAnimationFrame > 0)
				{
					if (column == playerX + 1 && playerY == row)
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else if (column == playerX + 2 && playerY == row && mAnimationFrame <= 2)
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else if (column == playerX + mAnimationFrame && playerY == row && mAnimationFrame >= 3)
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.FollowSoldier2 && mAnimationFrame > 0)
				{
					if (column == playerX + 1 && playerY == row && mAnimationFrame <= 117)
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.VisitCanopus && mAnimationFrame > 0) {
					if (column == playerX - (5 - mAnimationFrame) && row == playerY)
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.RequestPardon && mAnimationFrame > 0) {
					if (column == playerX + 4 && row == playerY + 1)
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else if ((column == playerX + (4 - mAnimationFrame) && row == playerY + 1) || (column == playerX - 2 && row == playerY))
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.ConfirmPardon && mAnimationFrame > 0)
				{
					if ((column == playerX + mAnimationFrame && row == playerY + 1) || (column == playerX - 2 && row == playerY))
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else if (column == playerX + 4 && row == playerY + 1)
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.ConfirmPardon2 && mAnimationFrame > 0)
				{
					if (column == playerX - 2 && row == playerY)
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else if (column == playerX + 4 && row == playerY + 1)
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.ConfirmPardon3 && mAnimationFrame > 0)
				{
					if ((column == playerX + (5 - mAnimationFrame) && row == playerY + 1) || (column == playerX - 2 && row == playerY))
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else if (column == playerX + 4 && row == playerY + 1)
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.JoinCanopus && mAnimationFrame > 0)
				{
					if ((column == playerX - (3 - mAnimationFrame) && row == playerY && mAnimationFrame <= 2) || (column == playerX && row == playerY + 1))
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else if (column == playerX + 4 && row == playerY + 1)
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.LeavePrisonSoldier && mAnimationFrame > 0)
				{
					if (column == playerX + (mAnimationFrame - 1) && row == playerY + 1)
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else if (column == playerX + 4 && row == playerY + 1)
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.LeaveCanopus && mAnimationFrame > 0)
				{
					if (column == playerX - (mAnimationFrame + 1) && row == playerY)
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else
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

		private class BattleCommand
		{
			public Lore Player
			{
				get;
				set;
			}

			public int FriendID
			{
				get;
				set;
			}

			public int EnemyID
			{
				get;
				set;
			}

			public int Method
			{
				get;
				set;
			}

			public int Tool
			{
				get;
				set;
			}
		}

		private enum SpecialEventType
		{
			None,
			TrainSkill,
			TrainMagic,
			ChangeJobForSword,
			ChangeJobForMagic,
			BackToBattleMode,
			NextToBattleMode,
			BattleUseItem,
			CantTrainMagic,
			WizardEye,
			SkipTurn,
			Penetration,
			Telescope,
			CantBuyWeapon,
			CantBuyExp,
			CantBuyItem,
			CantBuyMedicine,
			CureComplete,
			NotCured,
			CantTrain,
			HearAncientEvil,
			HearAncientEvil2,
			MeetFriend,
			BattleLordAhn,
			ReadProphesy,
			ReadProphesy1,
			ReadProphesy2,
			ReadProphesy3,
			ReadProphesy4,
			ReadProphesy5,
			ReadProphesyLast,
			SeeMurderCase,
			GotoCourt,
			Interrogation,
			Punishment,
			GotoJail,
			LiveJail,
			RequestSuppressVariantPeoples,
			PassCrystal,
			SealCrystal,
			SendUranos,
			ClearRoom,
			TransformProtagonist,
			TransformProtagonist2,
			TransformProtagonist3,
			TransformProtagonist4,
			SendNecromancer,
			FollowSolider,
			LordAhnMission,
			LeaveSoldier,
			ConfirmPardon,
			ConfirmPardon2,
			JoinCanopus
		}

		private enum BattleEvent
		{
			None,
			Pollux,
		}

		private enum BattleTurn
		{
			None,
			Player,
			Enemy,
			RunAway,
			AlmostWin,
			Win,
			Lose
		}

		private enum AnimationType
		{
			None,
			BuyExp,
			HearAncientEvil,
			MeetFriend,
			LeftManager,
			CaptureProtagonist,
			GotoCourt,
			GotoCourt2,
			SubmitProof,
			GotoJail,
			MeetCanopus,
			LiveJail,
			PassCrystal,
			PassCrystal2,
			SealCrystal,
			SealCrystal2,
			TransformProtagonist,
			TransformProtagonist2,
			SendNecromancer,
			ComeSoldier,
			FollowSoldier,
			FollowSoldier2,
			LeaveSoldier,
			VisitCanopus,
			RequestPardon,
			ConfirmPardon,
			ConfirmPardon2,
			ConfirmPardon3,
			JoinCanopus,
			LeavePrisonSoldier,
			LeaveCanopus
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
			BattleLose,
			Game,
			CastOneMagic,
			CastAllMagic,
			CastSpecial,
			CastESP,
			ChooseBattleCureSpell,
			CastSummon,
			BattleChooseItem,
			BattleUseItemWhom,
			ChooseItem,
			EnemySelectMode,
			BattleStart,
			BattleCommand,
			ChooseTrainSkill,
			ChooseTrainMagic,
			BuyWeapon,
			BuyShield,
			BuyArmor,
			ChooseGameOverLoadGame,
			ChooseESPMagic,
			ApplyBattleCureSpell,
			ApplyBattleCureAllSpell,
			ConfirmExitMap,
			AskEnter,
			ChooseLoadGame,
			UseItemWhom,
			TeleportationDirection,
			ChooseChangeSwordMember,
			ChooseSwordJob,
			ChooseChangeMagicMember,
			HealType,
			ChooseTrainSkillMember,
			ChooseTrainMagicMember,
			ChooseWeaponType,
			BuyExp,
			SelectItem,
			SelectMedicine,
			ChooseSaveGame,
			ApplyPhenominaMagic,
			VaporizeMoveDirection,
			ViewCharacter,
			CastSpell,
			Extrasense,
			ExchangeItem,
			UseItemPlayer,
			GameOptions,
			SpellCategory,
			ChooseCureSpell,
			ApplyCureMagic,
			ApplyCureAllMagic,
			TransformDirection,
			BigTransformDirection,
			ChooseExtrasense,
			TelescopeDirection,
			ExchangeItemWhom,
			SwapItem,
			SetMaxEnemy,
			OrderFromCharacter,
			UnequipCharacter,
			DelistCharacter,
			ConfirmExit,
			SetEncounterType,
			AttackCruelEnemy,
			OrderToCharacter,
			Unequip,
			UseWeaponCharacter,
			UseShieldCharacter,
			UseArmorCharacter,
			ChooseFoodAmount,
			SelectItemAmount,
			SelectMedicineAmount,
			Hospital,
			TrainingCenter,
			ChooseMagicJob,
			JoinFriend,
			JoinAlcor,
			JoinMizar,
			JoinAntaresJr,
			JoinCanopus,
			MeetPollux,
			JoinPollux,
			ChooseItemType,
			ChooseCrystal
		}
	}
}
