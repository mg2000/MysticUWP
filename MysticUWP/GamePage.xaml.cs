using Microsoft.Graphics.Canvas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Gaming.XboxLive.Storage;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
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
		private SpriteSheet mDecorateTiles;
		private readonly object mapLock = new object();

		private int mXWide; // 시야 범위
		private int mYWide; // 시야 범위

		private LorePlayer mParty;
		private List<Lore> mPlayerList;
		private List<Lore> mBackupPlayerList;
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
		private string mTryEnterMap = "";

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

		private int mAnswerID;
		private bool mAllPass = true;

		private MapHeader mCrystalMap = null;
		private int mCrystalX;
		private int mCrystalY;

		private int mLordAhnBattleCount = 0;

		private Lore mEquipPlayer;
		private List<Tuple<int, int>> mEquipGearIDList = new List<Tuple<int, int>>();
		private List<int> mEquipTypeList = new List<int>();
		private int mEquipTypeID;

		public GamePage()
		{
			this.InitializeComponent();

			mWizardEyeTimer.Interval = TimeSpan.FromMilliseconds(100);
			mWizardEyeTimer.Tick += (sender, e) =>
			{
				if (mWizardEyePosBlink)
					mWizardEyePosBlink = false;
				else
					mWizardEyePosBlink = true;
			};

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

			mEnterTypeMap["Lore"] = "로어 성";
			mEnterTypeMap["LastDtch"] = "라스트디치 성";
			mEnterTypeMap["Menace"] = "메너스";
			mEnterTypeMap["GaiaGate"] = "가이아 게이트";
			mEnterTypeMap["LoreGate"] = "로어 게이트";
			mEnterTypeMap["Valiant"] = "배리언트 피플즈성";
			mEnterTypeMap["Gaea"] = "가이아 테라성";
			mEnterTypeMap["OrcTown"] = "오크 마을";
			mEnterTypeMap["Vesper"] = "베스퍼성";
			mEnterTypeMap["TrolTown"] = "트롤 마을";
			mEnterTypeMap["Kobold"] = "코볼트 마을";
			mEnterTypeMap["Ancient"] = "에인션트이블 신전";
			mEnterTypeMap["Hut"] = "오두막집";
			mEnterTypeMap["DracTown"] = "드라코니안 마을";
			mEnterTypeMap["Imperium"] = "임페리엄 마이너성";
			mEnterTypeMap["HdsGate"] = "하데스 게이트";
			mEnterTypeMap["UnderGrd"] = "하데스 테라";

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
							if (mMapName == "Ground1") {
								if (x == 19 && y == 10)
									ShowEnterMenu("Lore");
								else if (x == 75 && y == 56)
								{
									if (mParty.Year > 699)
										Dialog(" 당신이  라스트디치 성으로  들어가려 했지만 그곳 주민으로 등록되지 않은 이방인이란 이유로 쫒겨났다.");
									else
										ShowEnterMenu("LastDtch");
								}
								else if (x == 16 && y == 88)
								{
									if (mParty.Year > 699)
										Dialog(" 당신이 메너스로 들어가려 했지만  이미 이곳은 폐광이 된데다가 입구마저 함몰되어 도저히 들어갈 수가 없었다.");
									else
										ShowEnterMenu("Menace");
								}
							}
							else if (mMapName == "LastDtch") {
								if ((x == 37 && y == 9) || (x == 38 && y == 9)) {
									ShowEnterMenu("GaiaGate");
								}
							}
							else if (mMapName == "Valiant" || mMapName == "Light") {
								if ((x == 37 && y == 9) || (x == 38 && y == 9)) {
									ShowEnterMenu("LoreGate");
								}
							}
							else if (mMapName == "Ground2") {
								if (x == 18 && y == 25)
									ShowEnterMenu("Valiant");
								else if (x == 30 && y == 81)
									ShowEnterMenu("Gaea");
								else if (x == 81 && y == 46)
									ShowEnterMenu("OrcTown");
							}
							else if (mMapName == "Ground3") {
								if (x == 68 && y == 28)
									ShowEnterMenu("Vesper");
								else if (x == 80 && y == 14)
									ShowEnterMenu("TrolTown");
								else if (x == 80 && y == 75) {
									if (GetBit(10))
									{
										if (!GetBit(9))
										{
											Dialog(" 당신이 기묘한 비석 앞에 섰을때 그것은 트롤 글로 적힌 주문이란걸 알았다. 당신은 낮은 소리로 그 주문을 읖조렸다." +
											"  순간 비석이  둘로 쪼개어지며 에테르체의 형상이 나타났다. 그리고는 서서히 생체 프라즈마질의 트롤 형상으로 변해갔다." +
											"  그것이 완전한 트롤의 형상으로 변하자  내게 무어라 알수 없는 말을 지껄이더니 다시 당신의 모습으로 닮아갔다." +
											"  당신의 모습으로 변한 플라즈마체의 기운은 서서히 당신의 몸속에 스며 들어갔다." +
											" 그 알수 없는 존재와의 결합이 끝났을때  당신은  그 존재가 지껄였던 말이 트롤족의 말이었음을 알게 되었다." +
											"  그리고  이제 당신이  트롤족의 말을 할 수 있다는 걸 깨닭게 되었다.");
											SetBit(9);
										}
										else
											Dialog(" 당신 앞에는  반으로 쪼개어진 비석만 남아있었다.");
									}
									else
										Dialog(" 당신 앞에는 기묘한 도형이 새겨진 비석이 서 있었다.");
								}
							}
							else if (mMapName == "Vesper" || mMapName == "TrolTown")
							{
								TalkMode(x, y);
							}
							else if (mMapName == "Ground4") {
								if (x == 81 && y == 15) {
									if (GetBit(86))
									{
										Dialog(" 당신은  코볼트킹을 가둬놓기 위해 만든 로드안의 결계 때문에 안으로 들어갈 수 없었다.");
									}
									else
										ShowEnterMenu("Kobold");
								}
								else if (x == 19 && y == 38) {
									ShowEnterMenu("Ancient");
								}
								else if (x == 47 && y == 56) {
									ShowEnterMenu("Hut");
								}
								else if (x == 25 && y == 15) {
									Dialog(new string[] {
										$"[color={RGB.LightRed}] 여기는 '발하라 전당'.[/color]",
										$"[color={RGB.LightRed}] 시간 관계상 제작을 하지 못했음.[/color]",
										"",
										"                       제작자 씀."
									});
								}
							}
							else if (mMapName == "Ancient") {
								if (x == 9 && y == 22) {
									if (GetBit(12))
									{
										if (!GetBit(11))
										{
											Dialog(" 당신은 코볼트글로 되어 있는 기둥을 보았다. 그리고 그곳에 적힌 글을 읽자  당신의 머리속을 꽉 채우는 무언가를 느꼈다.");
											SetBit(11);
										}
									}
									else
										Dialog(" 당신 앞에는  처음 보는 문자가 적힌  기둥이 서 있었다.");
								}
							}
							else if (mMapName == "Ground5") {
								if (x == 38 && y == 35)
									ShowEnterMenu("DracTown");
								else if (x == 14 && y == 30)
									ShowEnterMenu("Imperium");
							}
							else if (mMapName == "Tomb") {
								ShowExitMenu();
							}
							else if (mMapName == "HdsGate") {
								if (x == 20 && y == 32) {
									if (mParty.Etc[24] == 2)
										ShowEnterMenu("UnderGrd");
									else
										Dialog($"[color={RGB.LightMagenta}] 지하 세계의 입구에서 무언가 강한 힘이 뿜어져 나와 당신을 밀쳐내 버렸다.[/color]");
								}
							}
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
								if (mMapName == "TrolTown" || mMapName == "Tomb")
									TalkMode(x, y);
								else
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
							}
							else if ((1 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 39) || GetTileInfo(x, y) == 51)
							{

							}
							else if (GetTileInfo(x, y) == 53)
							{
								if (mMapName == "OrcTown" || mMapName == "Vesper" || mMapName == "Tomb")
									TalkMode(x, y);
								else
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

						if (battleEvent == BattleEvent.Pollux)
						{
							Ask($"[color={RGB.LightMagenta}] 윽!! 나의 패배를 인정하겠다." +
							$"  나는 원래 암살자로 일하던[/color] [color={RGB.LightMagenta}]폴록스[/color][color={RGB.LightMagenta}]라고한다." +
							" 수련을 목적으로 당신과 동행하고 싶다. 당신은 어떤가?[/color]",
							MenuMode.JoinPollux, new string[] {
								"좋소, 허락하겠소",
								"그렇게는 않되오"
							});
						}
						else if (battleEvent == BattleEvent.Orc1)
						{
							UpdateTileInfo(9, 38, 34);
							SetBit(57);
						}
						else if (battleEvent == BattleEvent.Orc2)
						{
							UpdateTileInfo(8, 18, 34);
							SetBit(58);
						}
						else if (battleEvent == BattleEvent.Orc3)
						{
							UpdateTileInfo(12, 11, 34);
							SetBit(59);
						}
						else if (battleEvent == BattleEvent.Orc4)
						{
							UpdateTileInfo(20, 8, 34);
							SetBit(60);
						}
						else if (battleEvent == BattleEvent.Orc5)
						{
							UpdateTileInfo(42, 11, 34);
							SetBit(61);
						}
						else if (battleEvent == BattleEvent.Orc6)
						{
							UpdateTileInfo(43, 29, 34);
							SetBit(62);
						}
						else if (battleEvent == BattleEvent.Orc7)
						{
							UpdateTileInfo(34, 28, 34);
							SetBit(63);
						}
						else if (battleEvent == BattleEvent.Orc8)
						{
							UpdateTileInfo(40, 38, 34);
							SetBit(64);
						}
						else if (battleEvent == BattleEvent.OrcTownEnterance)
						{
							UpdateTileInfo(24, 41, 43);
							UpdateTileInfo(24, 42, 43);

							SetBit(53);
						}
						else if (battleEvent == BattleEvent.OrcTempleEnterance)
						{
							UpdateTileInfo(23, 19, 43);
							UpdateTileInfo(25, 19, 43);

							SetBit(54);
						}
						else if (battleEvent == BattleEvent.OrcArchiMage)
						{
							UpdateTileInfo(22, 17, 43);
							SetBit(55);
						}
						else if (battleEvent == BattleEvent.OrcKing)
						{
							UpdateTileInfo(24, 16, 43);
							UpdateTileInfo(25, 16, 43);

							SetBit(56);
							SetBit(3);

							for (var i = 53; i <= 64; i++)
								SetBit(i);

							mParty.Etc[20] = 2;

							foreach (var player in mPlayerList)
								player.Experience += 15_000;

							if (mAssistPlayer != null)
								mAssistPlayer.Experience += 15_000;

							Dialog(new string[] {
								$"[color={RGB.Yellow}] 당신은 오크족의 보스인 오크킹을 물리쳤다.[/color]",
								$"[color={RGB.LightCyan}][[ 경험치 + 15000 ][/color]"
							});
						}
						else if (battleEvent == BattleEvent.VesperTroll1)
						{
							Dialog(new string[] {
								" 당신이 적들을 물리치자 동굴속에서 날렵하게 생긴 트롤이 걸어 나왔다.",
								""
							});

							if (GetBit(9))
							{
								Talk($"[color={RGB.LightMagenta}] 음... 나의 부하들을 물리치다니 제법 실력이 있는 것같군.  하지만  네가 이 동굴에 들어온 것은 정말 큰 불운이란걸 명심해라." +
								" 바로 베스퍼 점령대의 대장인 트롤 암살자님이  바로 나이니까.[/color]", SpecialEventType.BattleTrollAssassin);
							}
							else
								Talk(" 그리고는 도저히 알아들을수 없는 말을 몇 마디 지껄이더니 당신에게 달려 들었다.", SpecialEventType.BattleTrollAssassin);
						}
						else if (battleEvent == BattleEvent.TrollAssassin)
						{
							Dialog(new string[] {
								$"[color={RGB.Yellow}] 당신은 트롤 암살자를 처치했다.[/color]",
								$"[color={RGB.LightCyan}][[ 다크 크리스탈 + 1][/color]"
							});

							mParty.Crystal[2]++;
							UpdateTileInfo(29, 18, 43);

							SetBit(65);
							SetBit(1);
						}
						else if (battleEvent == BattleEvent.VesperTroll2)
						{
							Dialog($"[color={RGB.LightCyan}] [[ 민첩성 + 1 ][/color]", true);

							foreach (var player in mPlayerList)
							{
								if (player.Agility < 20)
									player.Agility++;
							}

							if (mAssistPlayer != null)
								mAssistPlayer.Agility++;

							UpdateTileInfo(49, 18, 43);
							SetBit(66);
						}
						else if (battleEvent == BattleEvent.VesperTroll3)
						{
							Dialog($"[color={RGB.LightCyan}] [[ 금화 + 20,000 ][/color]", true);

							mParty.Gold += 20_000;

							UpdateTileInfo(63, 25, 43);
							SetBit(67);
						}
						else if (battleEvent == BattleEvent.VesperTroll4)
						{
							Dialog($"[color={RGB.LightCyan}] [[ 식량 + 50 ][/color]", true);

							if (mParty.Food + 50 < 256)
								mParty.Food += 50;
							else
								mParty.Food = 255;

							UpdateTileInfo(60, 37, 43);
							SetBit(68);
						}
						else if (battleEvent == BattleEvent.VesperTroll5)
						{
							Dialog($"[color={RGB.LightCyan}] [[ 곤봉 + 5 ][/color]", true);

							if (mParty.Backpack[1, 0] + 5 < 256)
								mParty.Backpack[1, 0] += 5;
							else
								mParty.Backpack[1, 0] = 255;

							UpdateTileInfo(39, 39, 43);
							SetBit(69);
						}
						else if (battleEvent == BattleEvent.VesperTroll6)
						{
							Dialog($"[color={RGB.LightCyan}] [[ 화살 + 30 ][/color]", true);

							if (mParty.Arrow + 30 < 65_536)
								mParty.Arrow += 30;
							else
								mParty.Arrow = 65_535;

							UpdateTileInfo(21, 39, 43);
							SetBit(70);
						}
						else if (battleEvent == BattleEvent.VesperTroll7)
						{
							Dialog($"[color={RGB.LightCyan}] [[ 금화 + 1,000 ][/color]", true);

							mParty.Gold += 1_000;

							UpdateTileInfo(19, 25, 43);
							SetBit(71);
						}
						else if (battleEvent == BattleEvent.TrollKing)
						{
							Dialog(new string[] {
								" 당신은 경호대원들을 물리치고는 뒤에서 벌벌 떨고 있는 트롤킹에게 검을 겨누었다." +
								" 그는 겁에 질린채 주저 않았고 당신은 그를 단숨에 베어버렸다.",
								"",
								$"[color={RGB.Yellow}] 이제 당신은 전과자 신분이라기보다는 영웅으로써 로어성에 귀환 할 수 있게 되었다.[/color]"
							});

							mParty.Etc[19] = 4;
							SetBit(4);
							SetBit(72);

							UpdateTileInfo(24, 7, 44);
							UpdateTileInfo(25, 7, 44);
						}
						else if (battleEvent == BattleEvent.Troll1)
						{
							SetBit(73);
							UpdateTileInfo(16, 18, 44);
						}
						else if (battleEvent == BattleEvent.Troll2)
						{
							SetBit(74);
							UpdateTileInfo(33, 18, 44);
						}
						else if (battleEvent == BattleEvent.Troll3)
						{
							SetBit(75);
							UpdateTileInfo(12, 31, 44);
							UpdateTileInfo(12, 32, 44);
						}
						else if (battleEvent == BattleEvent.Troll4)
						{
							SetBit(76);
							UpdateTileInfo(37, 31, 44);
							UpdateTileInfo(37, 32, 44);
						}
						else if (battleEvent == BattleEvent.Troll5)
						{
							SetBit(77);
							UpdateTileInfo(23, 66, 44);
							UpdateTileInfo(24, 66, 44);
							UpdateTileInfo(25, 66, 44);
						}
						else if (battleEvent == BattleEvent.Troll6)
						{
							UpdateTileInfo(5, 30, 44);
							SetBit(78);
						}
						else if (battleEvent == BattleEvent.Troll7)
						{
							UpdateTileInfo(8, 29, 44);
							SetBit(79);
						}
						else if (battleEvent == BattleEvent.Troll8)
						{
							UpdateTileInfo(6, 32, 44);
							SetBit(80);
						}
						else if (battleEvent == BattleEvent.Troll9)
						{
							UpdateTileInfo(10, 34, 44);
							SetBit(81);
						}
						else if (battleEvent == BattleEvent.Troll10)
						{
							UpdateTileInfo(39, 29, 44);
							SetBit(82);
						}
						else if (battleEvent == BattleEvent.Troll11)
						{
							UpdateTileInfo(40, 34, 44);
							SetBit(83);
						}
						else if (battleEvent == BattleEvent.Troll12)
						{
							UpdateTileInfo(42, 31, 44);
							SetBit(84);
						}
						else if (battleEvent == BattleEvent.Troll13)
						{
							UpdateTileInfo(44, 33, 44);
							SetBit(85);
						}
						else if (battleEvent == BattleEvent.ExitKoboldKing)
						{
							BattleExitKobldKing();

							return;
						}
						else if (battleEvent == BattleEvent.KoboldKnight)
						{
							Dialog(" 당신은 쓰러진  코볼트 근위기사의 몸을 뒤져 에메랄드키를 찾아냈다.");
							SetBit(17);
						}
						else if (battleEvent == BattleEvent.GoldKey)
						{
							Dialog(" 당신은 벽에 걸려 있는 황금 키를 가졌다.");
							SetBit(18);
						}
						else if (battleEvent == BattleEvent.KoboldMagicUser)
						{
							Dialog(" 당신은 코볼트 매직유저의 몸을 뒤져서  백금 키를 발견했다.");
							SetBit(19);
						}
						else if (battleEvent == BattleEvent.SaphireKey)
						{
							Dialog(" 당신은 벽에 걸린 사파이어 키를 가졌다.");
							SetBit(20);
						}
						else if (battleEvent == BattleEvent.KoboldAlter)
						{
							Dialog(" 당신은 대제사장으로부터 다이아몬드키 두 개를 빼았았다.");
							SetBit(88);
						}
						else if (battleEvent == BattleEvent.TreasureBox1)
						{
							UpdateTileInfo(68, 24, 43);
							SetBit(134);
						}
						else if (battleEvent == BattleEvent.TreasureBox2)
						{
							UpdateTileInfo(72, 24, 43);
							SetBit(135);
						}
						else if (battleEvent == BattleEvent.TreasureBox3)
						{
							UpdateTileInfo(76, 24, 43);
							SetBit(136);
						}
						else if (battleEvent == BattleEvent.TreasureBox4)
						{
							UpdateTileInfo(80, 24, 43);
							SetBit(137);
						}
						else if (battleEvent == BattleEvent.KoboldSoldier)
							SetBit(92);
						else if (battleEvent == BattleEvent.KoboldSoldier2)
							SetBit(91);
						else if (battleEvent == BattleEvent.KoboldSecurity)
							SetBit(90);
						else if (battleEvent == BattleEvent.KoboldGuardian) {
							SetBit(87);
						}
						else if (battleEvent == BattleEvent.KoboldSummoner)
							SetBit(89);
						else if (battleEvent == BattleEvent.KoboldKing) {
							Dialog(" 당신이  코볼트킹을 물리쳤구나라고 생각했을때 코볼트킹은 공간이동의 마법을 써서 어디론가 사라져 버렸다." +
							" 당신은 당황해서 이곳 전지역을 투시해 보았지만 그는 없었다.");
							SetBit(86);
						}
						else if (battleEvent == BattleEvent.DraconianBeliever) {
							for (var x = 9; x < 11; x++)
								UpdateTileInfo(x, 16, 44);
							SetBit(28);
						}
						else if (battleEvent == BattleEvent.Vampire) {
							SetBit(105);
							UpdateTileInfo(66, 65, 44);
						}
						else if (battleEvent == BattleEvent.Dracula) {
							SetBit(109);
							UpdateTileInfo(61, 78, 44);
						}
						else if (battleEvent == BattleEvent.Draconian1) {
							SetBit(111);
							UpdateTileInfo(23, 97, 44);
						}
						else if (battleEvent == BattleEvent.Draconian2)
						{
							SetBit(112);
							UpdateTileInfo(23, 91, 44);
						}
						else if (battleEvent == BattleEvent.Draconian3)
						{
							SetBit(113);
							UpdateTileInfo(24, 80, 44);
						}
						else if (battleEvent == BattleEvent.Draconian4)
						{
							SetBit(114);
							UpdateTileInfo(33, 99, 44);
						}
						else if (battleEvent == BattleEvent.Draconian5)
						{
							SetBit(115);
							UpdateTileInfo(35, 92, 44);
						}
						else if (battleEvent == BattleEvent.Draconian6)
						{
							SetBit(116);
							UpdateTileInfo(41, 83, 44);
						}
						else if (battleEvent == BattleEvent.Draconian7)
						{
							SetBit(117);
							UpdateTileInfo(32, 81, 44);
						}
						else if (battleEvent == BattleEvent.Draconian8)
						{
							SetBit(118);
							UpdateTileInfo(40, 69, 44);
						}
						else if (battleEvent == BattleEvent.Draconian9)
						{
							SetBit(119);
							UpdateTileInfo(31, 21, 44);
						}
						else if (battleEvent == BattleEvent.Draconian10)
						{
							SetBit(120);
							UpdateTileInfo(39, 32, 44);
						}
						else if (battleEvent == BattleEvent.Draconian11)
						{
							SetBit(121);
							UpdateTileInfo(15, 18, 44);
						}
						else if (battleEvent == BattleEvent.Draconian12)
						{
							SetBit(122);
							UpdateTileInfo(15, 34, 44);
						}
						else if (battleEvent == BattleEvent.Draconian13)
						{
							SetBit(123);
							UpdateTileInfo(14, 47, 44);
						}
						else if (battleEvent == BattleEvent.DraconianEntrance) {
							SetBit(103);
							UpdateTileInfo(13, 104, 44);
							UpdateTileInfo(14, 105, 44);
						}
						else if (battleEvent == BattleEvent.DraconianEntrance2) {
							SetBit(102);
							for (var y = 58; y < 61; y++) {
								for (var x = 46; x < 48; x++)
									UpdateTileInfo(x, y, 44);
							}
						}
						else if (battleEvent == BattleEvent.DraconianBoss1) {
							SetBit(101);
							for (var x = 23; x < 27; x++)
								UpdateTileInfo(x, 53, 44);
						}
						else if (battleEvent == BattleEvent.DraconianBoss2) {
							SetBit(100);
							for (var y = 58; y < 61; y++)
								UpdateTileInfo(83, y, 44);
						}
						else if (battleEvent == BattleEvent.FrostDraconian) {
							SetBit(99);
							for (var x = 76; x < 79; x++)
								UpdateTileInfo(x, 87, 44);
						}
						else if (battleEvent == BattleEvent.DraconianHolyKnight) {
							SetBit(98);
							for (var x = 58; x < 61; x++)
								UpdateTileInfo(x, 47, 44);
						}
						else if (battleEvent == BattleEvent.DraconianMagician) {
							SetBit(97);
							for (var x = 72; x < 75; x++)
								UpdateTileInfo(x, 39, 44);
						}
						else if (battleEvent == BattleEvent.DraconianGuardian) {
							SetBit(96);
							for (var x = 93; x < 96; x++)
								UpdateTileInfo(x, 36, 44);
						}
						else if (battleEvent == BattleEvent.MessengerOfDeath) {
							SetBit(127);
						}
						else if (battleEvent == BattleEvent.Kerberos) {
							SetBit(128);
						}
						else if (battleEvent == BattleEvent.DraconianOldKing) {
							Dialog($"[color={RGB.LightCyan}] [[ 영혼의 크리스탈 + 1 ][/color]");
							SetBit(129);
							mParty.Crystal[4]++;
						}
						else if (battleEvent == BattleEvent.DraconianKing) {
							mAnimationEvent = AnimationType.None;

							SetBit(95);
							UpdateTileInfo(101, 19, 44);
							UpdateTileInfo(101, 20, 42);
							UpdateTileInfo(100, 20, 44);
							UpdateTileInfo(102, 20, 44);

							Talk(new string[] {
								" 당신은 마지막 적인  드라코니안족의 왕을 처치했다.  기쁨을 감추지 못한채 돌아 나오려고 할때 갑자기 난데없는 암흑이  성 전체를 뒤덮기 시작했다." +
								"  그와 동시에 정체불명의 목소리가 어디에선가 들려왔다.",
								"",
								$"[color={RGB.LightMagenta}] 나는  드라코니안족의 수호령이다.  너희들은 너무나 큰 실수를 나에게 저질렀다." +
								" 나는 죽어간  드라코니안족의 복수를 위해  너희들을 이 세상에서 소멸시켜 버리려한다. 너희들은 나의 상대가 전혀되지 않는다. 이쯤에서 작별인사나 해두는게 좋을 것이다.[/color]"
							}, SpecialEventType.BattleDraconianSpirit);
						}
						else if (battleEvent == BattleEvent.DraconianSpirit) {
							SetBit(6);
							Dialog($"[color={RGB.White}] 당신은 드라코니안족을 멸망 시켰다.[/color]");
						}
						else if (battleEvent == BattleEvent.Rebellion1) {
							mEncounterEnemyList.Clear();

							for (var i = 0; i < 7; i++)
								JoinEnemy(63);
							JoinEnemy(64);

							DisplayEnemy();
							HideMap();

							mBattleEvent = BattleEvent.Rebellion2;
							StartBattle(false);
							return;
						}
						else if (battleEvent == BattleEvent.Rebellion2) {
							mEncounterEnemyList.Clear();

							for (var i = 0; i < 4; i++)
								JoinEnemy(63);
							for (var i = 0; i < 3; i++)
								JoinEnemy(64);
							JoinEnemy(65);

							DisplayEnemy();
							HideMap();

							mBattleEvent = BattleEvent.Rebellion3;
							StartBattle(false);
							return;
						}
						else if (battleEvent == BattleEvent.Rebellion3) {
							SetBit(33);
							mParty.Etc[19] = 9;

							Dialog(" 하지만  카미너스는  배리언트 피플즈의 남은 주민들을 데리고  성을 버린채  게이트를 통해 도망가 버렸다.");
						}
						else if (battleEvent == BattleEvent.OldLordAhn1) {
							mEncounterEnemyList.Clear();

							for (var i = 0; i < 3; i++)
								JoinEnemy(70);
							JoinEnemy(71);
							JoinEnemy(73);

							DisplayEnemy();
							HideMap();
							
							mBattleEvent = BattleEvent.OldLordAhn2;
							StartBattle(false);
							return;
						}
						else if (battleEvent == BattleEvent.OldLordAhn2) {
							Talk($"[color={RGB.White}] 당신은 드디어 숙적 로드안을 물리쳤다.[/color]", SpecialEventType.End3);
						}
						else if (battleEvent == BattleEvent.OrcRevengeSpirit) {
							Dialog($"[color={RGB.White}] 당신은 오크의 원혼을 크리스탈 볼 속에 봉인시켰다.[/color]");
							mParty.Etc[25]++;
						}
						else if (battleEvent == BattleEvent.TrollRevengeSpirit)
						{
							Dialog($"[color={RGB.White}] 당신은 트롤의 원혼을 크리스탈 볼 속에 봉인시켰다.[/color]");
							mParty.Etc[25]++;
						}
						else if (battleEvent == BattleEvent.KoboldRevengeSpirit)
						{
							Dialog($"[color={RGB.White}] 당신은 코볼트의 원혼을 크리스탈 볼 속에 봉인시켰다.[/color]");
							mParty.Etc[25]++;
						}
						else if (battleEvent == BattleEvent.DraconianRevengeSpirit)
						{
							Dialog($"[color={RGB.White}] 당신은 드라콘의 원혼을 크리스탈 볼 속에 봉인시켰다.[/color]");
							mParty.Etc[25]++;
							mParty.Etc[19] = 11;
						}
						else if (battleEvent == BattleEvent.LordAhn) {
							if (mLordAhnBattleCount == 14) {
								mMapName = "Lore";

								await RefreshGame();

								mXAxis = 50;
								mYAxis = 30;

								Talk(" 당신은  힘겹게 로드안을 물리쳤다.  그리고 예상대로  로어성의 병사들은  자신의 군주가 패하자 뿔뿔이 흩어져 버렸다. 벌판에는 당신이 죽인 수 많은 시체가 널려있었다.", SpecialEventType.End2);

								mEncounterEnemyList.Clear();
								mBattleEvent = 0;
							}
							else if (mLordAhnBattleCount == 13)
								BattleLordAhnType5();
							else if (mLordAhnBattleCount == 12)
								BattleLordAhnType4();
							else if (mLordAhnBattleCount >= 10)
								BattleLordAhnType3();
							else if (mLordAhnBattleCount >= 6)
								BattleLordAhnType2();
							else
								BattleLordAhnType1();

							return;
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

						if (battleEvent == BattleEvent.OrcKing)
						{
							mXAxis = 24;
							mYAxis = 20;
						}
						else if (battleEvent == BattleEvent.VesperTroll1 ||
						battleEvent == BattleEvent.VesperTroll2 ||
						battleEvent == BattleEvent.VesperTroll3 ||
						battleEvent == BattleEvent.VesperTroll4 ||
						battleEvent == BattleEvent.VesperTroll5 ||
						battleEvent == BattleEvent.VesperTroll6 ||
						battleEvent == BattleEvent.VesperTroll7 ||
						battleEvent == BattleEvent.TrollAssassin)
						{
							mXAxis = mMapHeader.StartX;
							mYAxis = mMapHeader.StartY;
						}
						else if (battleEvent == BattleEvent.ExitKoboldKing)
						{
							mMapName = mMapHeader.ExitMap;
							mXAxis = mMapHeader.ExitX;
							mYAxis = mMapHeader.ExitY;

							await RefreshGame();

							if (GetBit(86))
							{
								Dialog(new string[] {
									" 당신이 성 밖으로 탈출하자  허공에서 로드안의 음성이 들려왔다.",
									"",
									$"[color={RGB.LightGreen}] 당신은 빨리 이곳을 나가시오.  나의 선의 봉인으로  악마의 힘을 빌린 코볼트킹을 막겠소. 아마 그는 다시는 이 성을 벗어나지 못할거요.[/color]",
									"",
									" 말이채 끝나기도 전에 코볼트성 주위에는  구형의 결계가 형성 되었고  곧이어 기의 파문이 일렁이기 시작했다."
								});
								SetBit(5);
							}
						}
						else if (battleEvent == BattleEvent.KoboldAlter)
						{
							mYAxis++;
						}
						else if (battleEvent == BattleEvent.TreasureBox1)
						{
							mYAxis++;
						}
						else if (battleEvent == BattleEvent.TreasureBox2)
						{
							mYAxis++;
						}
						else if (battleEvent == BattleEvent.TreasureBox3)
						{
							mYAxis++;
						}
						else if (battleEvent == BattleEvent.TreasureBox4)
						{
							mYAxis++;
						}
						else if (battleEvent == BattleEvent.KoboldSoldier)
						{
							mXAxis++;
						}
						else if (battleEvent == BattleEvent.KoboldSoldier2)
						{
							mXAxis--;
						}
						else if (battleEvent == BattleEvent.KoboldSecurity)
						{
							mXAxis++;
						}
						else if (battleEvent == BattleEvent.KoboldGuardian)
						{
							mYAxis++;
						}
						else if (battleEvent == BattleEvent.KoboldSummoner)
						{
							mYAxis--;
						}
						else if (battleEvent == BattleEvent.KoboldKing)
						{
							mBattleEvent = BattleEvent.KoboldKing;
							Talk(" 당신은 코볼트킹에게서 도망치려 했지만 그가 만든 결계를 벗어 날 수가 없었다.", SpecialEventType.BackToBattleMode);

							return;
						}
						else if (battleEvent == BattleEvent.DraconianOldKing)
						{
							mBattleCommandQueue.Clear();
							BattleMode();

							return;
						}
						else if (battleEvent == BattleEvent.DraconianKing)
						{
							mBattleCommandQueue.Clear();
							BattleMode();

							return;
						}
						else if (battleEvent == BattleEvent.DraconianSpirit)
						{
							mBattleCommandQueue.Clear();
							BattleMode();

							return;
						}
						else if (battleEvent == BattleEvent.Rebellion1 ||
							battleEvent == BattleEvent.Rebellion2 ||
							battleEvent == BattleEvent.Rebellion3)
						{
							mBattleCommandQueue.Clear();
							BattleMode();

							return;
						}
						else if (battleEvent == BattleEvent.OldLordAhn1 ||
							battleEvent == BattleEvent.OldLordAhn2)
						{
							mBattleCommandQueue.Clear();
							BattleMode();

							return;
						}
						else if (battleEvent == BattleEvent.OrcRevengeSpirit ||
							battleEvent == BattleEvent.TrollRevengeSpirit ||
							battleEvent == BattleEvent.KoboldRevengeSpirit ||
							battleEvent == BattleEvent.DraconianRevengeSpirit)
						{
							mBattleCommandQueue.Clear();
							BattleMode();

							return;
						}
						else if (battleEvent == BattleEvent.LordAhn)
						{
							mBattleEvent = BattleEvent.LordAhn;
							Talk(" 일행은 너무 많은 적들에게 포위되어서 도망 갈 수가 없었다.", SpecialEventType.BackToBattleMode);
							return;
						}
						
						mEncounterEnemyList.Clear();
						ShowMap();
					}
					else if (mBattleTurn == BattleTurn.Lose)
					{
						if (battleEvent == BattleEvent.LordAhn) {
							mEncounterEnemyList.Clear();
							ShowMap();

							Dialog(" 이제는 로드안에게 패하는구나하며 모든것을 단념하려는 순간  빛나는 광채가 당신을 둘러싸기 시작했다." +
							" 그리고는 빨려 들어가듯 당신을 감싸던 광채는 차원의 틈속으로 당신을 몰아넣었다.");

							InvokeAnimation(AnimationType.Dying);
						}
					}

					mBattleTurn = BattleTurn.None;
				}

				void ShowCureResult(bool battleCure)
				{
					if (battleCure)
						Talk(mCureResult.ToArray(), SpecialEventType.NextToBattleMode);
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

							mBattleCommandQueue.Clear();
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
								$"[color={RGB.LightBlue}] 당신의 성공을 빌겠습니다.[/color]"
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
							}, MenuMode.JoinFriendAgain, new string[] {
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

							if (mAssistPlayer != null)
								mAssistPlayer = null;

							DisplayPlayerInfo();

							UpdateTileInfo(mXAxis - 3, mYAxis, 44);
							UpdateTileInfo(mXAxis + 3, mYAxis, 44);
							UpdateTileInfo(mXAxis - 3, mYAxis + 2, 44);
							UpdateTileInfo(mXAxis + 3, mYAxis + 2, 44);
							UpdateTileInfo(mXAxis - 3, mYAxis + 4, 44);
							UpdateTileInfo(mXAxis + 3, mYAxis + 4, 44);

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
						else if (specialEvent == SpecialEventType.FollowSolider)
						{
							InvokeAnimation(AnimationType.FollowSoldier);
						}
						else if (specialEvent == SpecialEventType.LordAhnMission)
						{
							Talk($"[color={RGB.LightGreen}] 당신이  살인 죄로 잡혀오기 전까지는  주위 사람들에게 매우 인정받은 용감한 사람이었다는 것을 알고 있소." +
							"  내가 지금 부탁하는  몇가지를 들어 준다면 당신에게 그 댓가로 자유로운 삶을 보장하겠소.[/color]", SpecialEventType.LeaveSoldier);
						}
						else if (specialEvent == SpecialEventType.LeaveSoldier)
						{
							InvokeAnimation(AnimationType.LeaveSoldier);
						}
						else if (specialEvent == SpecialEventType.ConfirmPardon)
						{
							InvokeAnimation(AnimationType.ConfirmPardon);
						}
						else if (specialEvent == SpecialEventType.ConfirmPardon2)
						{
							InvokeAnimation(AnimationType.ConfirmPardon3);
						}
						else if (specialEvent == SpecialEventType.JoinCanopus)
						{
							InvokeAnimation(AnimationType.JoinCanopus);
						}
						else if (specialEvent == SpecialEventType.LearnTrollWriting)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							Dialog(" 이제 당신은  트롤의 글을  읽고 쓸 수 있게 되었다.");
						}
						else if (specialEvent == SpecialEventType.LearnOrcWriting)
						{
							InvokeAnimation(AnimationType.CompleteLearnOrcWriting);
						}
						else if (specialEvent == SpecialEventType.AskKillOrc1 ||
						specialEvent == SpecialEventType.AskKillOrc2 ||
						specialEvent == SpecialEventType.AskKillOrc3 ||
						specialEvent == SpecialEventType.AskKillOrc4 ||
						specialEvent == SpecialEventType.AskKillOrc5 ||
						specialEvent == SpecialEventType.AskKillOrc6 ||
						specialEvent == SpecialEventType.AskKillOrc7 ||
						specialEvent == SpecialEventType.AskKillOrc8)
						{
							var menuMode = MenuMode.None;

							switch (specialEvent)
							{
								case SpecialEventType.AskKillOrc1:
									menuMode = MenuMode.KillOrc1;
									break;
								case SpecialEventType.AskKillOrc2:
									menuMode = MenuMode.KillOrc2;
									break;
								case SpecialEventType.AskKillOrc3:
									menuMode = MenuMode.KillOrc3;
									break;
								case SpecialEventType.AskKillOrc4:
									menuMode = MenuMode.KillOrc4;
									break;
								case SpecialEventType.AskKillOrc5:
									menuMode = MenuMode.KillOrc5;
									break;
								case SpecialEventType.AskKillOrc6:
									menuMode = MenuMode.KillOrc6;
									break;
								case SpecialEventType.AskKillOrc7:
									menuMode = MenuMode.KillOrc7;
									break;
								case SpecialEventType.AskKillOrc8:
									menuMode = MenuMode.KillOrc8;
									break;
							}

							Ask(" 당신 앞에 있는 오크족을  당신은  어떻게 할 것인가를 선택하시오.", menuMode, new string[] {
								"죽여 버린다",
								"그냥 살려 준다"
							});
						}
						else if (specialEvent == SpecialEventType.MeetTraveler)
						{
							Dialog(" 저는 지금 아프로디테 테라에서 오는 길입니다. 거기서 우연히 트롤족의 계보를 보았는데 그 종족은 왕권 세습제이며 왕권 중심으로 사회가 구성되어 있습니다." +
							" 트롤족의 왕은 그리 강하지 않지만 그의 부하들 중에는 꽤 강한자가 있습니다.");

							SetBit(51);
						}
						else if (specialEvent == SpecialEventType.LordAhnCall)
						{
							Talk(new string[] {
								$"[color={RGB.LightGreen}] 드디어 이곳까지 왔군요. 내가 한 가지 부탁을 더 해도 되겠소?  이미 베스퍼성은 쑥밭이 되어서  사실상 원정은  더 이상 필요치 않게 되었소." +
								" 그래서 부탁하건데 우리의 원흉인 트롤족을  완전히 멸해 주시오.  그들의 마을은 베스퍼성 북동쪽 산악지역에 있소. 당신의 승리를 기원하리라.[/color]",
								$"[color={RGB.LightCyan}][[ 경험치 + 10,000 ][/color]"
							}, SpecialEventType.MoveGround3);

							foreach (var player in mPlayerList)
							{
								if (player.IsAvailable)
									player.Experience += 10_000;
							}

							SetBit(203);
						}
						else if (specialEvent == SpecialEventType.MoveGround3)
						{
							await MoveGround3();
						}
						else if (specialEvent == SpecialEventType.BattleOrcTempleEnterance)
						{
							mBattleEvent = BattleEvent.OrcTempleEnterance;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleOrcArchiMage)
						{
							mBattleEvent = BattleEvent.OrcArchiMage;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleOrcKing)
						{
							mBattleEvent = BattleEvent.OrcKing;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleVesperTroll1)
						{
							mBattleEvent = BattleEvent.VesperTroll1;

							mEncounterEnemyList.Clear();
							JoinEnemy(27);
							for (var i = 0; i < 6; i++)
								JoinEnemy(26);

							DisplayEnemy();
							HideMap();

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleTrollAssassin)
						{
							mBattleEvent = BattleEvent.TrollAssassin;

							mEncounterEnemyList.Clear();

							for (var i = 0; i < 2; i++)
								JoinEnemy(27);

							for (var i = 0; i < 4; i++)
								JoinEnemy(26);

							JoinEnemy(29);
							JoinEnemy(28);

							DisplayEnemy();
							HideMap();

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleVesperTroll2)
						{
							mBattleEvent = BattleEvent.VesperTroll2;

							mEncounterEnemyList.Clear();

							for (var i = 0; i < 3; i++)
								JoinEnemy(27);
							for (var i = 0; i < 3; i++)
								JoinEnemy(26);
							JoinEnemy(28);

							DisplayEnemy();
							HideMap();

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleVesperTroll3)
						{
							mBattleEvent = BattleEvent.VesperTroll3;

							mEncounterEnemyList.Clear();

							JoinEnemy(25);
							for (var i = 0; i < 6; i++)
								JoinEnemy(26);

							DisplayEnemy();
							HideMap();

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleVesperTroll4)
						{
							mBattleEvent = BattleEvent.VesperTroll4;

							mEncounterEnemyList.Clear();

							for (var i = 0; i < 4; i++)
								JoinEnemy(26);
							for (var i = 0; i < 2; i++)
								JoinEnemy(25);

							DisplayEnemy();
							HideMap();

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleVesperTroll5)
						{
							mBattleEvent = BattleEvent.VesperTroll5;

							mEncounterEnemyList.Clear();

							for (var i = 0; i < 5; i++)
								JoinEnemy(25);
							for (var i = 0; i < 2; i++)
								JoinEnemy(27);

							DisplayEnemy();
							HideMap();

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleVesperTroll6)
						{
							mBattleEvent = BattleEvent.VesperTroll6;

							mEncounterEnemyList.Clear();

							for (var i = 0; i < 6; i++)
								JoinEnemy(26);
							JoinEnemy(27);

							DisplayEnemy();
							HideMap();

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleVesperTroll7)
						{
							mBattleEvent = BattleEvent.VesperTroll7;

							mEncounterEnemyList.Clear();

							for (var i = 0; i < 5; i++)
								JoinEnemy(26);

							DisplayEnemy();
							HideMap();

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.ReadVesperMemo)
						{
							Talk(new string[] {
								$"[color={RGB.White}] 혹시나 이 글을 누가 읽기를 바라며 ...[/color]",
								"",
								$"[color={RGB.White}] 트롤족에 의한 베스퍼성의 괴멸은 예견되었던거나 다름없었다." +
								"  우리는 인간이기에  인간의 편을 들수밖에 없었고  또한 우리의 견해와 입장으로써 이 사실을 다른 대륙으로 알렸다." +
								" 하지만 그것은 잘못되었다는 것을  이내 알았다. 아마 내가 죽고 난 상황에서의 이곳은  인류와 트롤 모두가 자신들의 정의에 따라  서로를 저주하며 대립하고 있을 것이다." +
								" 어떻게 보면 서로의 이익을 찾기위해  명분을 세우고  침략을 정당화 시킨다고 할 수 있다. 우리는 인간이기에 우리쪽의 잘못은 감추려고한다." +
								"  항상 다른 종족의 잘못을 들추어내며 우리를 더욱 값지게 해왔던게 사실이다. 이번의 경우도 그렇다." +
								" 책임이 있다면  사실상 이번일은  인간의 책임이 더 크다.  자초지종을 말하자면  다음과 같다. 원래  아프로디테 테라는  트롤족의 땅이었다." +
								" 하지만 인간의 영역을 더욱 확보하려는 야심에서 명분을 세우고  그들의 땅을 침범하여 성을 구축한 곳이 바로 이곳 베스퍼성이었다." +
								" 그리고 그 일을 총지휘한 자는 바로 ............."
							}, SpecialEventType.NoMoreMemo);
						}
						else if (specialEvent == SpecialEventType.NoMoreMemo)
						{
							Talk(" 당신이 여기까지 읽어내려 갔을때 그 종이 쪽지는 산화하여 공중으로 흩어져 버렸다." +
							"  당신은  지금 이 쪽지에서 말하려고 했던 자로부터 천리안으로 감시를 당하고 있다는 사실을 순간 깨닭았다." +
							"  당신이 알지 못하는 제 3의 인물이 있다는 확신이 강해짐에 따라 더욱 더 긴장 할 수 밖에 없었다.  하지만 더 이상의 변화는 일어나지 않았다.", SpecialEventType.None);

							SetBit(15);
						}
						else if (specialEvent == SpecialEventType.BattleTrollKing)
						{
							mBattleEvent = BattleEvent.TrollKing;

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleTroll1)
						{
							mBattleEvent = BattleEvent.Troll1;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleTroll2)
						{
							mBattleEvent = BattleEvent.Troll2;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleTroll5)
						{
							mBattleEvent = BattleEvent.Troll5;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.KillPhysicist)
						{
							Lore murderPlayer = mPlayerList.Count > 1 ? mPlayerList[mPlayerList.Count - 1] : null;
							if (murderPlayer != null)
							{
								Talk(new string[] {
									$" 그때 당신 옆에 있던 {murderPlayer.NameSubjectBJosa} 검을 들어 그를 내리쳤다.",
									" 순간적으로 일어난 일이라 아무도 손을 쓸 수가 없었다. 그리고 일행들에게 말했다.",
									"",
									$"[color={RGB.LightGreen}] 쳇, 이런 겁장이가 지어내는 말을  믿고 있다니... 자네들 어서가게. 이런데서 시간을 낭비해 버리면 우리만 불리해진다네.[/color]"
								}, SpecialEventType.KillPhysicist2);

								UpdateTileInfo(5, 30, 44);
								SetBit(78);
							}
						}
						else if (specialEvent == SpecialEventType.KillPhysicist2)
						{
							Dialog(new string[] {
								" 당신은 그래도 아쉬운 마음에 한 마디를 던졌다.",
								"",
								$"[color={RGB.LightBlue}] 이건 자네가 좀 심했네.  그의 말이 황당하기는 했지만  거짓인것 같지는 않았다고  생각하네." +
								" 이왕 들은건데 그 장소까지 알았으면 좋았을걸...[/color]"
							});
						}
						else if (specialEvent == SpecialEventType.AskKillTroll6 ||
						specialEvent == SpecialEventType.AskKillTroll7 ||
						specialEvent == SpecialEventType.AskKillTroll8 ||
						specialEvent == SpecialEventType.AskKillTroll9 ||
						specialEvent == SpecialEventType.AskKillTroll10 ||
						specialEvent == SpecialEventType.AskKillTroll11 ||
						specialEvent == SpecialEventType.AskKillTroll12 ||
						specialEvent == SpecialEventType.AskKillTroll13)
						{
							var menuMode = MenuMode.None;

							switch (specialEvent)
							{
								case SpecialEventType.AskKillTroll6:
									menuMode = MenuMode.KillTroll6;
									break;
								case SpecialEventType.AskKillTroll7:
									menuMode = MenuMode.KillTroll7;
									break;
								case SpecialEventType.AskKillTroll8:
									menuMode = MenuMode.KillTroll8;
									break;
								case SpecialEventType.AskKillTroll9:
									menuMode = MenuMode.KillTroll9;
									break;
								case SpecialEventType.AskKillTroll10:
									menuMode = MenuMode.KillTroll10;
									break;
								case SpecialEventType.AskKillTroll11:
									menuMode = MenuMode.KillTroll11;
									break;
								case SpecialEventType.AskKillTroll12:
									menuMode = MenuMode.KillTroll12;
									break;
								case SpecialEventType.AskKillTroll13:
									menuMode = MenuMode.KillTroll13;
									break;
							}

							Ask(" 당신 앞에 있는 트롤족을  당신은  어떻게 할 것인가를 선택하시오.", menuMode, new string[] {
								"죽여 버린다",
								"그냥 살려 준다"
							});
						}
						else if (specialEvent == SpecialEventType.MeetBecrux)
						{
							Ask(" 나는 베스퍼성에서 베크룩스라고 불리던 사람이오.  나는 단독으로 트롤킹을 잡기위해 여기로 잠입했다가  적의 숫자에 밀려서 포로로 잡혀버렸소." +
							" 일대일의 대결이라면 자신이 있었는데 적의 수가 워낙 많아서 이런 곳까지 잡혀오게 되었던거요.  나는 이 일로  더욱 타종족에 대한 미움이 커져 버렸소." +
							" 나는 당신들에게 진정으로 부탁하나 하겠소.  나를 당신의 일행에 넣어 주시오.", MenuMode.JoinBercux, new string[] {
								"당신의 부탁을 들어주겠소",
								"미안하지만 그런 어렵겠소"
							});
						}
						else if (specialEvent == SpecialEventType.LearnKoboldWriting)
						{
							InvokeAnimation(AnimationType.CompleteLearnKoboldWriting);
						}
						else if (specialEvent == SpecialEventType.BattleExitKoboldKing)
						{
							BattleExitKobldKing();
						}
						else if (specialEvent == SpecialEventType.BattleKoboldAlter)
						{
							mBattleEvent = BattleEvent.KoboldAlter;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleTreasureBox1 ||
							specialEvent == SpecialEventType.BattleTreasureBox2 ||
							specialEvent == SpecialEventType.BattleTreasureBox3 ||
							specialEvent == SpecialEventType.BattleTreasureBox4)
						{
							switch (specialEvent)
							{
								case SpecialEventType.BattleTreasureBox1:
									mBattleEvent = BattleEvent.TreasureBox1;
									break;
								case SpecialEventType.BattleTreasureBox2:
									mBattleEvent = BattleEvent.TreasureBox2;
									break;
								case SpecialEventType.BattleTreasureBox3:
									mBattleEvent = BattleEvent.TreasureBox3;
									break;
								case SpecialEventType.BattleTreasureBox4:
									mBattleEvent = BattleEvent.TreasureBox4;
									break;
							}

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleKoboldSoldier)
						{
							mBattleEvent = BattleEvent.KoboldSoldier;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleKoboldSoldier2)
						{
							mBattleEvent = BattleEvent.KoboldSoldier2;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleKoboldSecurity)
						{
							mBattleEvent = BattleEvent.KoboldSecurity;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleKoboldGuardian)
						{
							mBattleEvent = BattleEvent.KoboldGuardian;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleKoboldSummoner)
						{
							mBattleEvent = BattleEvent.KoboldSummoner;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleKoboldKing)
						{
							mBattleEvent = BattleEvent.KoboldKing;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.AskTreasureboxQuestion1)
						{
							mAllPass = true;

							var options = ShuffleOptions(new string[] {
								"로드안",
								"네크로만서",
								"레드 안타레스",
								"알비레오"
							});

							for (var i = 0; i < options.Length; i++)
							{
								if (options[i] == "알비레오")
								{
									mAnswerID = i;
									break;
								}
							}

							Ask($"[color={RGB.White}] 1편 <또 다른 지식의 성전>과 2편 <다크 메이지 실리안 카미너스>에  모두 출현했던 인물이 아닌 것은 누구인가?[/color]",
							MenuMode.Answer1_1, options);
						}
						else if (specialEvent == SpecialEventType.AskTreasureboxQuestion2)
						{
							mAllPass = true;

							var argument = mRand.Next(100) + 1;

							var options = ShuffleOptions(new string[] {
								$"{argument * 3 / 100} x 10^15",
								$"{argument * 1 / 100} x 10^15",
								$"{argument * 10 / 100} x 10^15",
								$"{argument * 9 / 100} x 10^15"
							});

							for (var i = 0; i < options.Length; i++)
							{
								if (options[i] == $"{argument * 9 / 100} x 10^15")
								{
									mAnswerID = i;
									break;
								}
							}

							Ask($"[color={RGB.White}] 당신은  핵융합 마법을  종종 사용하곤 한다." +
							$" 당신이 공기 중에 분포 해 있는  수소원자 4개를 헬륨원자 1개로 핵융합 시켰을때  질량결손이 {argument}mg 이라면 적에게 방출된 에너지는 얼마인가?[/color]",
							MenuMode.Answer2_1, options);
						}
						else if (specialEvent == SpecialEventType.AskTreasureboxQuestion3)
						{
							mAllPass = true;

							Ask($"[color={RGB.White}] 달과 지구의 나이는 같거나 지구가 더 많다.[/color]",
							MenuMode.Answer3_1, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (specialEvent == SpecialEventType.AskTreasureboxQuestion4)
						{
							mAllPass = true;

							Ask($"[color={RGB.White}] BASIC 언어의  ';'(세미콜론)은  문장의 끝을 의미한다[/color]",
							MenuMode.Answer4_1, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (specialEvent == SpecialEventType.OpenTreasureBox)
						{
							if ((mParty.Etc[30] & (1 << 4)) > 0)
							{
								if (!GetBit(151))
									OpenTreasureBox();
							}
						}
						else if (specialEvent == SpecialEventType.PlusExperience)
						{
							Dialog($"[color={RGB.LightCyan}] [[ 경험치 + 500,000 ][/color]", true);

							foreach (var player in mPlayerList)
								player.Experience += 500_000;

							if (mAssistPlayer != null)
								mAssistPlayer.Experience += 500_000;

							SetBit(206);
						}
						else if (specialEvent == SpecialEventType.SendValiantToUranos)
						{
							InvokeAnimation(AnimationType.SendValiantToUranos);
						}
						else if (specialEvent == SpecialEventType.CaptureMermaid)
						{
							Ask(new string[] {
								$"[color={RGB.LightBlue}] 제발 살려주세요. 그냥 장난친것 뿐이예요.[/color]",
								$"[color={RGB.LightBlue}] 만약 살려 주시면  당신에게  드라코니안족의 글을 가르쳐 드릴께요.[/color]"
							}, MenuMode.KillMermaid, new string[] {
								"인어를 놓아준다",
								"죽여 버린다"
							});
						}
						else if (specialEvent == SpecialEventType.BattleVampire)
						{
							mEncounterEnemyList.Clear();

							for (var i = 0; i < 8; i++)
								JoinEnemy(3);

							DisplayEnemy();
							HideMap();

							mBattleEvent = BattleEvent.Vampire;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleDracula)
						{
							mEncounterEnemyList.Clear();

							for (var i = 0; i < 5; i++)
								JoinEnemy(13);

							DisplayEnemy();
							HideMap();

							mBattleEvent = BattleEvent.Dracula;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.OpenTomb)
						{
							Ask(new string[] {
								" 당신이 바닥을 밀어내자  바닥이 스르르 밀려나며 좁은 지하계단이 나타났다." +
								"  계단의 입구에는 스산한 기운이 맴돌고 있었고  자세히 다시 보니 뚜렸하게 이런 글자가 새겨져 있었다.",
								$"[color={RGB.LightRed}]'이곳으로 발을 디딘 자는 드라콘 제왕의 저주를 받게 될 것이다.'[/color]"
							}, MenuMode.EnterTomb, new string[] {
								"지하계단으로 내려간다",
								"다시 바닥을 원위치 시킨다"
							});
						}
						else if (specialEvent == SpecialEventType.RefuseJoinEnterTomb)
						{
							Ask($"[color={RGB.LightBlue}] 우리들의 목적은 따로 있는데 일부러 이런 모험을 할 필요가 없지 않은가?  우리들은  모두 여기에 있겠네." +
							"  게다가 계단에 쓰여 있는  이글도 마음에 걸린다네. 정말 가고 싶다면 자네 혼자서 가게나. 우리는 여기서 기다릴테니.[/color]", MenuMode.ForceEnterTomb, new string[] {
								"혼자라도 지하무덤으로 간다",
								"대원들의 의견대로 그만 둔다"
							});
						}
						else if (specialEvent == SpecialEventType.BattleDraconian1 ||
							specialEvent == SpecialEventType.BattleDraconian2 ||
							specialEvent == SpecialEventType.BattleDraconian4 ||
							specialEvent == SpecialEventType.BattleDraconian5 ||
							specialEvent == SpecialEventType.BattleDraconian6 ||
							specialEvent == SpecialEventType.BattleDraconian8 ||
							specialEvent == SpecialEventType.BattleDraconian9 ||
							specialEvent == SpecialEventType.BattleDraconian10 ||
							specialEvent == SpecialEventType.BattleDraconian11 ||
							specialEvent == SpecialEventType.BattleDraconian12 ||
							specialEvent == SpecialEventType.BattleDraconian13)
						{

							var menuMode = MenuMode.None;
							switch (specialEvent)
							{
								case SpecialEventType.BattleDraconian1:
									menuMode = MenuMode.BattleDraconian1;
									break;
								case SpecialEventType.BattleDraconian2:
									menuMode = MenuMode.BattleDraconian2;
									break;
								case SpecialEventType.BattleDraconian4:
									menuMode = MenuMode.BattleDraconian4;
									break;
								case SpecialEventType.BattleDraconian5:
									menuMode = MenuMode.BattleDraconian5;
									break;
								case SpecialEventType.BattleDraconian6:
									menuMode = MenuMode.BattleDraconian6;
									break;
								case SpecialEventType.BattleDraconian8:
									menuMode = MenuMode.BattleDraconian8;
									break;
								case SpecialEventType.BattleDraconian9:
									menuMode = MenuMode.BattleDraconian9;
									break;
								case SpecialEventType.BattleDraconian10:
									menuMode = MenuMode.BattleDraconian10;
									break;
								case SpecialEventType.BattleDraconian11:
									menuMode = MenuMode.BattleDraconian11;
									break;
								case SpecialEventType.BattleDraconian12:
									menuMode = MenuMode.BattleDraconian12;
									break;
								case SpecialEventType.BattleDraconian13:
									menuMode = MenuMode.BattleDraconian13;
									break;
							}

							Ask(" 당신 앞에 있는 드라코니안족을  당신은 어떻게 할 것인가를 선택하시오.", menuMode, new string[] {
								"죽여 버린다",
								"그냥 살려 준다"
							});
						}
						else if (specialEvent == SpecialEventType.BattleDraconian3 ||
							specialEvent == SpecialEventType.BattleDraconian7)
						{

							var battleEvent = BattleEvent.None;
							switch (specialEvent)
							{
								case SpecialEventType.BattleDraconian3:
									battleEvent = BattleEvent.Draconian3;
									break;
								case SpecialEventType.BattleDraconian7:
									battleEvent = BattleEvent.Draconian7;
									break;
							}

							BattleDraconian(battleEvent);
						}
						else if (specialEvent == SpecialEventType.SteelBoy)
						{
							Dialog(" 제 이름이 뭐냐구요 ? 저는 쇠돌이에요.");
						}
						else if (specialEvent == SpecialEventType.BattleDraconianEntrance2)
						{
							BattleDraconianEntrance2();
						}
						else if (specialEvent == SpecialEventType.BattleDraconianBoss1)
						{
							BattleDraconianBoss1();
						}
						else if (specialEvent == SpecialEventType.BattleDraconianBoss2)
						{
							BattleDraconianBoss2();
						}
						else if (specialEvent == SpecialEventType.BattleFrostDraconian)
						{
							BattleFrostDraconian();
						}
						else if (specialEvent == SpecialEventType.BattleDraconianHolyKnight)
						{
							BattleDraconianHolyKnight();
						}
						else if (specialEvent == SpecialEventType.BattleDraconianMagician)
						{
							BattleDraconianMagician();
						}
						else if (specialEvent == SpecialEventType.BattleDraconianGuardian)
						{
							BattleDraconianGuardian();
						}
						else if (specialEvent == SpecialEventType.MeetDraconianKing) {
							InvokeAnimation(AnimationType.TranformDraconianKing);
						}
						else if (specialEvent == SpecialEventType.BattleMessengerOfDeath) {
							mEncounterEnemyList.Clear();

							var enemy = JoinEnemy(0);
							enemy.Level = 30;
							enemy.HP = enemy.Endurance * enemy.Level * 10;

							DisplayEnemy();
							HideMap();

							mBattleEvent = BattleEvent.MessengerOfDeath;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleKerberos) {
							mEncounterEnemyList.Clear();

							for (var i = 0; i < 5; i++)
							{
								var enemy = JoinEnemy(11);
								enemy.Level = 15;
								enemy.HP = enemy.Endurance * enemy.Level * 10;
							}

							DisplayEnemy();
							HideMap();

							mBattleEvent = BattleEvent.Kerberos;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleDraconianOldKing) {
							mEncounterEnemyList.Clear();

							var enemy = JoinEnemy(57);
							enemy.Name = "17대 드라콘제왕";
							enemy.SpecialCastLevel |= 0x80;

							DisplayEnemy();
							HideMap();

							Talk($"[color={RGB.LightMagenta}] 100년만에 도굴꾼이 또 이곳에 나타났군.  너도 역시 내가 가지고 있는  영혼의 크리스탈을 훔칠 목적으로 여기에 온 것임에 틀림없다." +
							" 하지만 너도 별 수없이  이 앞의 해골과 같은 운명이 될것이다.[/color]", SpecialEventType.BattleDraconianOldKing2);
						}
						else if (specialEvent == SpecialEventType.BattleDraconianOldKing2) {
							mBattleEvent = BattleEvent.DraconianOldKing;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleDraconianKing) {
							mEncounterEnemyList.Clear();

							for (var i = 0; i < 3; i++)
								JoinEnemy(49);
							JoinEnemy(58);

							DisplayEnemy();
							HideMap();

							mBattleEvent = BattleEvent.DraconianKing;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleDraconianSpirit) {
							mEncounterEnemyList.Clear();

							var enemy = JoinEnemy(58);
							enemy.Name = "드라콘 수호령";
							enemy.SpecialCastLevel |= 0x80;

							DisplayEnemy();
							HideMap();

							mBattleEvent = BattleEvent.DraconianSpirit;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.TeleportCastleLore) {
							TeleportCastleLore();
						}
						else if (specialEvent == SpecialEventType.FindShelter) {
							mMapName = "Ground1";

							await RefreshGame();

							AddNextTimeEvent(3, 90);

							mXAxis = 79;
							mYAxis = 79;
						}
						else if (specialEvent == SpecialEventType.BattleOldLordAhn) {
							mEncounterEnemyList.Clear();

							for (var i = 0; i < 3; i++)
								JoinEnemy(68);
							for (var i = 0; i < 3; i++)
								JoinEnemy(67);
							JoinEnemy(69);

							mBattleEvent = BattleEvent.OldLordAhn1;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.SendNecromancer) {
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							Talk(" 네크로만서로 변한 당신은 공간의 틈을 통해 다른 공간으로 빠져 버렸다.  그리고 로어 세계의 평화가 너무 오래 유지될 때  당신은 운명에 의해 이 세계로 내려 올 것이다.", SpecialEventType.SendNecromancer2);
						}
						else if (specialEvent == SpecialEventType.SendNecromancer2) {
							InvokeAnimation(AnimationType.Ending1Cookie1);
						}
						else if (specialEvent == SpecialEventType.UseCromaticCrystal) {
							Talk($"[color={RGB.LightBlue}] 역시  당신은 제가 생각했던대로 정말 훌륭하십니다.  당신이 찾은 이곳이라면 아마 베리언트 피플즈의 사람들이 숨기에는 충분할 것입니다." +
							"  잠깐만 기다려 주십시오.  저의 염력으로 도시를 건설해 보겠습니다.[/color]", SpecialEventType.BuildDome);
						}
						else if (specialEvent == SpecialEventType.BuildDome) {
							mMapName = "Dome";

							await RefreshGame();

							for (var y = 0; y < mMapHeader.Height; y++) {
								for (var x = 0; x < mMapHeader.Width; x++) {
									if (mRand.Next(10) == 0)
										mMapHeader.Layer[x + y * mMapHeader.Width] = 45;
									else
										mMapHeader.Layer[x + y * mMapHeader.Width] = 47;
								}
							}

							mXAxis = 24;
							mYAxis = 49;

							AddNextTimeEvent(1, 20);
						}
						else if (specialEvent == SpecialEventType.ExitCrystal) {
							mCrystalMap = null;
							InvokeAnimation(AnimationType.ExitCrystal);
						}
						else if (specialEvent == SpecialEventType.Ending1Talk1) {
							Talk($"[color={RGB.LightRed}] 예, 메피스토펠레스님.  그것은 원혼이 봉인된 에너지 공인데  로드안이 지하세계로 버린 것입니다." +
							" 그런데 그것이 카미너스씨의 몸 속으로 들어갔습니다.[/color]", SpecialEventType.Ending1Talk2);
						}
						else if (specialEvent == SpecialEventType.Ending1Talk2) {
							Talk(new string[] {
							$"[color={RGB.LightMagenta}] 그래? 그렇다면 분명 카미너스씨의 자손중에서 원혼을 봉인한채 태어 나는 아이가 틀림없이 있을 것이다." +
							" 그 아이가 태어났을때, 우리의 힘으로 봉인된 원혼을 풀어 버린다면 우리는 그 힘을 이용해 세계를 지배할 수 있을 것이다." +
							" 후후후, 로드안은 정말 멍청한 짓을 했군.[/color]",
							$"[color={RGB.LightMagenta}] 우리에게 도리어 이런 기회를 줘 버리다니..[/color]"
							}, SpecialEventType.End1);
						}
						else if (specialEvent == SpecialEventType.End1) {
							Window.Current.CoreWindow.KeyDown -= gamePageKeyDownEvent;
							Window.Current.CoreWindow.KeyUp -= gamePageKeyUpEvent;
							Frame.Navigate(typeof(CreditPage), null);
						}
						else if (specialEvent == SpecialEventType.BattleLordAhn) {
							mLordAhnBattleCount = 0;
							BattleLordAhnType1();
						}
						else if (specialEvent == SpecialEventType.End2) {
							ShowMap();

							Dialog(" 얼마후 로어성에는 성대한 의식이 행해졌다.");

							InvokeAnimation(AnimationType.Ending2Cookie1);
						}
						else if (specialEvent == SpecialEventType.End2_2) {
							Window.Current.CoreWindow.KeyDown -= gamePageKeyDownEvent;
							Window.Current.CoreWindow.KeyUp -= gamePageKeyUpEvent;
							Frame.Navigate(typeof(Ending2), null);
						}
						else if (specialEvent == SpecialEventType.GoToFuture) {
							InvokeAnimation(AnimationType.GoToFuture);
						}
						else if (specialEvent == SpecialEventType.HearAlbireo) {
							Talk(new string[] {
								" 당신의 귓가에서 귀에 익은 목소리가 울려퍼졌다.",
								"",
								$"[color={RGB.LightGreen}] 나는 당신을 이런 운명으로 끌어들였던 알비레오라는 예언자이오. 물론 당신은 나를 알겠지요." +
								"  나는 이곳의 존재가 아니라서  이곳의 역사에 관여할 자격은 없소.  하지만  당신의 위급한 상황을 보고는 참을 수가 없어서 차원이탈 마법으로 당신을 795년으로 오게했소.[/color]",
								"",
								$"[color={RGB.LightGreen}] 이미 로어 세계는  당신을 잊혀진 인물로 평가할 정도의 세월이 지났소.  결코 당신을 알아볼 사람은 이곳에 없을거요." +
								" 세상의 최강자였던 로드안은 이제 기력이 많이 약해져서 지금 당신의 맞수로서 충분하리라 생각하오. 이제 당신은 별 제약없이 로어성으로 들어갈 수 있을거요.[/color]",
								$"[color={RGB.LightGreen}] 거기서  최후의 결전을 벌여  진정한 정의를 실현하시오. 행운을 빌겠소.[/color]"
							}, SpecialEventType.None);
						}
						else if (specialEvent == SpecialEventType.End3)
						{
							Window.Current.CoreWindow.KeyDown -= gamePageKeyDownEvent;
							Window.Current.CoreWindow.KeyUp -= gamePageKeyUpEvent;
							Frame.Navigate(typeof(Ending3), null);
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

						var added = true;
						while (added && mRemainDialog.Count > 0)
						{
							added = AppendText(mRemainDialog[0], true);
							if (added)
								mRemainDialog.RemoveAt(0);
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
						"일행이 가진 물품을 확인",
						"일행의 건강 상태를 본다",
						"마법을 사용한다",
						"초능력을 사용한다",
						"무기 장착 및 해제",
						"여기서 쉰다",
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
								specialEvent = SpecialEventType.NextToBattleMode;

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

					void UseCrystal(bool battle) {
						mUsableItemIDList.Clear();
						var crystalNameList = new List<string>();
						
						if (battle)
						{
							for (var i = 0; i < 7; i++)
							{
								if (mParty.Crystal[i] > 0)
								{
									crystalNameList.Add(Common.GetMagicName(7, i + 1));
									mUsableItemIDList.Add(i);
								}
							}

							if (mUsableItemIDList.Count > 0)
							{
								Ask(new string[] {
									$"[color={RGB.LightRed}]크리스탈을 사용 ----[/color]",
									"",
									$"[color={RGB.LightCyan}]당신이 사용할 크리스탈을 고르시오.[/color]"
								}, MenuMode.BattleChooseCrystal, crystalNameList.ToArray());
							}
							else
								BattleMode();
						}
						else
						{
							for (var i = 5; i < 9; i++)
							{
								if (mParty.Crystal[i] > 0)
								{
									crystalNameList.Add(Common.GetMagicName(7, i + 1));
									mUsableItemIDList.Add(i);
								}
							}

							if (mUsableItemIDList.Count > 0)
							{
								Ask(new string[] {
									$"[color={RGB.LightRed}]크리스탈을 사용 ----[/color]",
									"",
									$"[color={RGB.LightCyan}]당신이 사용할 크리스탈을 고르시오.[/color]"
								}, MenuMode.ChooseCrystal, crystalNameList.ToArray());
							}
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
						if (mMenuMode == MenuMode.BattleStart || mMenuMode == MenuMode.BattleCommand || mMenuMode == MenuMode.MeetPollux || mMenuMode == MenuMode.ChooseBetrayLordAhn || mMenuMode == MenuMode.JoinFriendAgain)
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
							menuMode == MenuMode.BattleChooseItemType ||
							menuMode == MenuMode.BattleChooseCrystal)
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
#if DEBUG
#else
							if (mMapHeader.Handicap && (mMapHeader.HandicapBit & (1 << 5)) > 0) {
								Dialog($"[color={RGB.LightMagenta}] 당신의 머리속에는 아무런 형상도 떠오르지 않았다.[/color]");
								return;
							}
#endif

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
											if (2 <= tileInfo && tileInfo <= 20)
												mWizardEye.Data[offset] = WHITE;
											else if (tileInfo == 22)
												mWizardEye.Data[offset] = LIGHTCYAN;
											else if (tileInfo == 1 || tileInfo == 48)
												mWizardEye.Data[offset] = LIGHTBLUE;
											else if (tileInfo == 23 || tileInfo == 49)
												mWizardEye.Data[offset] = CYAN;
											else if (tileInfo == 50)
												mWizardEye.Data[offset] = LIGHTRED;
											else if (tileInfo == 0)
													mWizardEye.Data[offset] = BLACK;
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
												mWizardEye.Data[offset] = BLACK;
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
										Talk(" 강한 치료 마법은 아직 불가능 합니다.", SpecialEventType.NextToBattleMode);
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

						void UseEnergyCrystal(bool battle) {
							foreach (var player in mPlayerList)
							{
								if (player.Dead == 0)
								{
									player.Unconscious = 0;
									player.HP = player.Endurance * player.Level * 10;
								}
								else
								{
									player.Dead = 0;
									player.Unconscious = 0;
									player.HP = 1;
								}
							}

							if (mAssistPlayer != null)
							{
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
							}

							DisplayPlayerInfo();

							var message = " 에너지 크리스탈은 강한 에너지를  우리 대원들의 몸속으로  방출하였고  그 에너지를 취한 대원들은 모두 원래의 기운을 되찾았다.";
							if (battle)
								Talk(message, SpecialEventType.NextToBattleMode);
							else
								Dialog(message);

							mParty.Crystal[6]--;
						}

						void ShowEquipUnequipMenu()
						{
							var hasGear = mEquipPlayer.Weapon > 0 || mEquipPlayer.Shield > 0 || mEquipPlayer.Armor > 0;

							var hasBackpackGear = false;
							for (var type = 0; type < 6; type++)
							{
								for (var gear = 0; gear < 7; gear++)
								{
									if (mParty.Backpack[type, gear] > 0)
									{
										if ((type < 4 && IsUsableWeapon(mEquipPlayer, (type + 1) * 7 + (gear + 1))) ||
											type == 5 && IsUsableShield(mEquipPlayer) ||
											type == 6 && IsUsableArmor(mEquipPlayer, gear + 1)) {
											hasBackpackGear = true;
											break;
										}
										
									}
								}

								if (hasBackpackGear)
									break;

							}

							if (!hasGear && !hasBackpackGear)
								Dialog($"{mEquipPlayer.NameSubjectJosa}에게 맞는 장비가 없습니다.");
							else
								ShowEquipTypeMenu();
						}

						void ShowEquipTypeMenu() {
							mEquipTypeList.Clear();

							if (mEquipPlayer.Weapon > 0)
								mEquipTypeList.Add(0);
							else {
								for (var type = 0; type < 4; type++)
								{
									for (var gear = 0; gear < 7; gear++)
									{
										if (mParty.Backpack[type, gear] > 0)
										{
											if (IsUsableWeapon(mEquipPlayer, (type + 1) * 7 + (gear + 1)))
											{
												mEquipTypeList.Add(0);
												break;
											}

										}
									}

									if (mEquipTypeList.Count > 0)
										break;
								}
							}

							if (mEquipPlayer.Shield > 0)
								mEquipTypeList.Add(1);
							else if (IsUsableShield(mEquipPlayer)) {
								for (var gear = 0; gear < 7; gear++)
								{
									if (mParty.Backpack[4, gear] > 0)
									{
										mEquipTypeList.Add(1);
										break;
									}
								}
							}

							if (mEquipPlayer.Armor > 0)
								mEquipTypeList.Add(2);
							else
							{
								for (var gear = 0; gear < 7; gear++)
								{
									if (mParty.Backpack[5, gear] > 0 && IsUsableArmor(mEquipPlayer, gear + 1))
									{
										mEquipTypeList.Add(2);
										break;
									}
								}
							}

							var equipNameList = new List<string>();
							foreach (var typeID in mEquipTypeList)
							{
								if (typeID == 0)
									equipNameList.Add($"무기: {Common.GetWeaponName(mEquipPlayer.Weapon)}");
								else if (typeID == 1)
									equipNameList.Add($"방패: {Common.GetShieldName(mEquipPlayer.Shield)}");
								else if (typeID == 2)
									equipNameList.Add($"갑옷: {Common.GetShieldName(mEquipPlayer.Armor)}");
							}

							Ask("장착 또는 교체할 장비를 선택해 주십시오.", MenuMode.EquipType, equipNameList.ToArray());
						}

						var menuMode = HideMenu();
						ClearDialog();

						if (menuMode == MenuMode.EnemySelectMode)
						{
							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

							if (mUseCrystalID == 2 || mUseCrystalID == 3)
							{
								var enemy = mEncounterEnemyList[mEnemyFocusID];
								if (enemy.Name == "로드 안")
								{
									Talk($" {mPlayerList[mBattlePlayerID].NameSubjectJosa} 로드안을 향해 다크 크리스탈과 에보니 크리스탈을 동시에 사용하였다." +
									" 크리스탈에서 뿜어져 나온  검은 기운은  금새 로드안에게 침투해 들어갔고 그는 순식간에 백여년을 늙어 버렸다", SpecialEventType.NextToBattleMode);

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
								else
								{
									Talk(" 이 크리스탈은  선의 힘을 사용하는 적에게만 반응한다.", SpecialEventType.NextToBattleMode);
								}

								mUseCrystalID = -1;
							}
							else if (mUseCrystalID == 4)
							{
								mUseCrystalID = -1;

								var enemy = mEncounterEnemyList[mEnemyFocusID];
								if (enemy.HP > 1)
								{
									Talk(" 당신이  영혼의 크리스탈을  적에게 사용하자 적의 영혼은 크리스탈에 의해 저주를 받아  황폐화 되어졌다.", SpecialEventType.NextToBattleMode);
									enemy.AuxHP = (enemy.AuxHP + 1) / 2;
									enemy.HP /= 2;

									DisplayEnemy();
									mParty.Crystal[4]--;
								}
								else
								{
									Talk(" 하지만 크리스탈은 전혀 반응을 보이지 않았다.", SpecialEventType.NextToBattleMode);
								}
							}
							else
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
								DialogText.Visibility = Visibility.Collapsed;

								HPPotionText.Text = mParty.Item[0].ToString();
								SPPotionText.Text = mParty.Item[1].ToString();
								AntidoteText.Text = mParty.Item[2].ToString();
								StimulantText.Text = mParty.Item[3].ToString();
								RevivalText.Text = mParty.Item[4].ToString();

								SummonScrollText.Text = mParty.Item[5].ToString();
								BigTorchText.Text = mParty.Item[6].ToString();
								QuartzBallText.Text = mParty.Item[7].ToString();
								FlyingBootsText.Text = mParty.Item[8].ToString();
								TransportationBallText.Text = mParty.Item[9].ToString();

								FireCrystalText.Text = mParty.Crystal[0].ToString();
								FrozenCrystalText.Text = mParty.Crystal[1].ToString();
								DarkCrystalText.Text = mParty.Crystal[2].ToString();
								EbonyCrystalText.Text = mParty.Crystal[3].ToString();
								SpiritCrystalText.Text = mParty.Crystal[4].ToString();
								SummonCrystalText.Text = mParty.Crystal[5].ToString();
								EnergyCrystalText.Text = mParty.Crystal[6].ToString();
								CromaticCrystalText.Text = mParty.Crystal[7].ToString();
								CrystalBallText.Text = mParty.Crystal[8].ToString();

								ItemInfoPanel.Visibility = Visibility.Visible;
							}
							else if (mMenuFocusID == 3)
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
							else if (mMenuFocusID == 4)
							{
								ShowCharacterMenu(MenuMode.CastSpell, false);
							}
							else if (mMenuFocusID == 5)
							{
								ShowCharacterMenu(MenuMode.Extrasense, false);
							}
							else if (mMenuFocusID == 6)
							{
								ShowCharacterMenu(MenuMode.Equip, false);
							}
							else if (mMenuFocusID == 7)
							{
								Rest();
							}
							else if (mMenuFocusID == 8)
							{
								ShowMenu(MenuMode.ChooseItemType, new string[] {
									"약초나 약물을 사용한다",
									"크리스탈을 사용한다"
								});
							}
							else if (mMenuFocusID == 9)
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
										specialEvent = SpecialEventType.NextToBattleMode;

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
									(mMapHeader.TileType == PositionType.Keep && GetTileInfo(newX, newY) >= 42) ||
									(mMapHeader.TileType == PositionType.Town && GetTileInfo(newX, newY) >= 48))
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
						else if (menuMode == MenuMode.BigTransformDirection)
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
									(mMapHeader.TileType == PositionType.Keep && GetTileInfo(mXAxis + xOffset * i, mYAxis + yOffset * i) >= 42) ||
									(mMapHeader.TileType == PositionType.Town && GetTileInfo(mXAxis + xOffset * i, mYAxis + yOffset * i) >= 48))
								{
									Dialog($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]");
								}
								else
								{
									UpdateTileInfo(mXAxis + xOffset * i, mYAxis + yOffset * i, tile);
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
										"메너스 광산을 나갈",
										"로드안에게 불려갈",
										"로드안과 계속 이야기 할",
										"가이아테라로 갈",
										"가이아 테라성의 성주에게 부탁을 받을",
										"오크 마을을 점령 할",
										"가이아 테라성의 성주를 다시 만날",
										"아프로디테 테라로 갈",
										"대륙의 중앙에 있는 베스퍼성을 점령 할",
										"베스퍼성 북동쪽의 트롤 마을을 점령 할",
										"로드안에게 돌아갈",
										"가이아 테라로 다시 갈",
										"가이아 테라성 북쪽에 있는 게이트를 통해 이쉬도 테라로 갈",
										"대륙 뷱동쪽에 있는 코볼트 마을을 점령 할",
										"가이아 테라로 돌아갈",
										"배리언트 피플즈의 성주를 만나 우라누스 테라로 갈",
										"드라코니안성을 점령 할",
										"대륙의 서쪽에 있는 임페리엄 마이너성에서 에인션트 이블을 만날",
										"다시 로드안을 만날",
										"배리언트 피플즈의 반란을 진압하려 갈",
										"가이아 테라에 있는 오크의 원혼을 봉인 할",
										"아프로디테 테라에 있는 트롤의 원혼을 봉인 할",
										"이쉬도 테라에 있는 코볼트의 원혼을 봉인 할",
										"로어 대륙에 있는 드라코니안의 원혼을 봉인 할",
										"하데스 게이트를 통해서 하데스 테라로 갈",
										"투시를 통해서 어떤 장소를 발견 할",
										"에인션트 이블의 도움을 받은뒤 다시 로어 대륙으로 나갈",
										"헤어졌던 동료를 만날",
										"로어성의 궁사들과 결전을 벌일",
										"로어성에서 로드안과 최후의 결전을 벌일"
									};

									int predict = -1;
									if (mParty.Etc[19] == 0)
									{
										if (!GetBit(0))
											predict = 0;
										else
											predict = 1;
									}
									else if (1 <= mParty.Etc[19] && mParty.Etc[19] <= 2)
										predict = 2;
									else if (mParty.Etc[19] == 3) {
										switch (mParty.Etc[20]) {
											case 0:
												if (mMapName != "Gaea")
													predict = 3;
												else
													predict = 4;
												break;
											case 1:
												predict = 5;
												break;
											case 2:
												predict = 6;
												break;
											case 3:
												if (GetBit(4))
													predict = 10;
												else if (GetBit(1))
													predict = 9;
												else if (mMapName == "Ground3" || mMapName == "TrolTown" || mMapName == "Vesper")
													predict = 8;
												else
													predict = 7;
												break;
										}
									}
									else if (4 <= mParty.Etc[19] && mParty.Etc[19] <= 6) {
										if (GetBit(6))
											predict = 17;
										else if (mMapName == "Ground5")
											predict = 16;
										else if (GetBit(5))
										{
											if (mMapName == "Ground2")
												predict = 15;
											else
												predict = 14;
										}
										else if (mMapName == "Ground4")
											predict = 13;
										else if (mMapName == "Gaea")
											predict = 12;
										else if (mMapName == "Ground2" || mMapName == "Valiant" || mMapName == "OrcTown")
											predict = 6;
										else if (mParty.Etc[19] == 5)
											predict = 2;
										else if (mParty.Etc[19] == 4)
											predict = 10;
										else
											predict = 11;
									}
									else if (mParty.Etc[19] == 7)
										predict = 18;
									else if (mParty.Etc[19] == 8)
										predict = 19;
										
		
									if (mParty.Etc[24] == 1) {
										switch (mParty.Etc[19]) {
											case 9:
												predict = 18;
												break;
											case 10:
												predict = mParty.Etc[25] + 20;
												break;
											case 11:
												predict = 10;
												break;
										}
									}
									else if (mParty.Etc[24] == 2) {
										if (mParty.Year >= 795)
											predict = 29;
										else if (GetBit(49))
											predict = 28;
										else if (GetBit(48))
											predict = 27;
										else if (GetBit(32))
											predict = 26;
										else if (mMapName == "UnderGrd")
											predict = 25;
										else
											predict = 24;
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
						else if (menuMode == MenuMode.Equip) {
							mEquipPlayer = mPlayerList[mMenuFocusID];

							ShowEquipUnequipMenu();

						}
						else if (menuMode == MenuMode.EquipUnequip) {
							if (mMenuFocusID == 0) {
								ShowEquipTypeMenu();
							}
						}
						else if (menuMode == MenuMode.EquipType) {
							mEquipGearIDList.Clear();

							mEquipTypeID = mEquipTypeList[mMenuFocusID];

							var backpackWeaponList = new List<string>();
							if (mEquipTypeID == 0) {
								if (mEquipPlayer.Weapon > 0)
									mEquipGearIDList.Add(new Tuple<int, int>(-1, -1));

								for (var type = 0; type < 4; type++) {
									for (var weapon = 0; weapon < 7; weapon++) {
										if (mParty.Backpack[type, weapon] > 0 && IsUsableWeapon(mEquipPlayer, (type + 1) * 7 + (weapon + 1)))
											mEquipGearIDList.Add(new Tuple<int, int>(type, weapon));
									}
								}

								foreach (var gearID in mEquipGearIDList) {
									if (gearID.Item1 == -1 && gearID.Item2 == -1)
										backpackWeaponList.Add($"장비 해제");
									else
										backpackWeaponList.Add($"{Common.GetWeaponName((gearID.Item1 + 1) * 7 + (gearID.Item2 + 1))}: {mParty.Backpack[gearID.Item1, gearID.Item2]}개");
								}
							}
							else if (mEquipTypeID == 1)
							{
								if (mEquipPlayer.Shield > 0)
									mEquipGearIDList.Add(new Tuple<int, int>(-1, -1));

								for (var weapon = 0; weapon < 10; weapon++)
								{
									if (mParty.Backpack[4, weapon] > 0 && IsUsableShield(mEquipPlayer))
										mEquipGearIDList.Add(new Tuple<int, int>(-1, weapon));
								}

								foreach (var gearID in mEquipGearIDList)
								{
									if (gearID.Item1 == -1 && gearID.Item2 == -1)
										backpackWeaponList.Add($"장비 해제");
									else
										backpackWeaponList.Add($"{Common.GetShieldName(gearID.Item2 + 1)}: {mParty.Backpack[4, gearID.Item2]}개");
								}

								Ask("장착할 장비를 선택해 주십시오.", MenuMode.ChooseEquip, backpackWeaponList.ToArray());
							}
							else if (mEquipTypeID == 2)
							{
								if (mEquipPlayer.Shield > 0)
									mEquipGearIDList.Add(new Tuple<int, int>(-1, -1));

								for (var weapon = 0; weapon < 10; weapon++)
								{
									if (mParty.Backpack[5, weapon] > 0 && IsUsableArmor(mEquipPlayer, weapon + 1))
										mEquipGearIDList.Add(new Tuple<int, int>(-1, weapon));
								}

								foreach (var gearID in mEquipGearIDList)
								{
									if (gearID.Item1 == -1 && gearID.Item2 == -1)
										backpackWeaponList.Add($"장비 해제");
									else
										backpackWeaponList.Add($"{Common.GetArmorName(gearID.Item2 + 1)}: {mParty.Backpack[5, gearID.Item2]}개");
								}
							}

							Ask("장착할 장비를 선택해 주십시오.", MenuMode.ChooseEquip, backpackWeaponList.ToArray());
						}
						else if (menuMode == MenuMode.ChooseEquip) {
							var equipID = mMenuFocusID;

							if (mEquipTypeID == 0) {
								if (mEquipPlayer.Weapon > 0)
								{
									var type = (mEquipPlayer.Weapon - 1) / 7;
									var gear = (mEquipPlayer.Weapon - 1) % 7;

									mParty.Backpack[type, gear]++;

									if (equipID == 0)
									{
										mEquipPlayer.Weapon = 0;
										UpdateItem(mEquipPlayer);
										ShowEquipTypeMenu();
										return;
									}
								}

								mEquipPlayer.Weapon = (mEquipGearIDList[equipID].Item1 + 1) * 7 + (mEquipGearIDList[equipID].Item2 + 1);
								mParty.Backpack[mEquipGearIDList[equipID].Item1, mEquipGearIDList[equipID].Item2]--;
								UpdateItem(mEquipPlayer);
								ShowEquipTypeMenu();
							}
							else if (mEquipTypeID == 1)
							{
								if (mEquipPlayer.Shield > 0)
								{
									mParty.Backpack[4, mEquipPlayer.Shield - 1]++;

									if (equipID == 0)
									{
										mEquipPlayer.Shield = 0;
										UpdateItem(mEquipPlayer);
										ShowEquipTypeMenu();
										return;
									}
								}

								mEquipPlayer.Shield = mEquipGearIDList[equipID].Item2 + 1;
								mParty.Backpack[4, mEquipGearIDList[equipID].Item2]--;
								UpdateItem(mEquipPlayer);
								ShowEquipTypeMenu();
							}
							else if (mEquipTypeID == 2)
							{
								if (mEquipPlayer.Armor > 0)
								{
									mParty.Backpack[4, mEquipPlayer.Armor - 1]++;

									if (equipID == 0)
									{
										mEquipPlayer.Armor = 0;
										UpdateItem(mEquipPlayer);
										ShowEquipTypeMenu();
										return;
									}
								}

								mEquipPlayer.Armor = mEquipGearIDList[equipID].Item2 + 1;
								mParty.Backpack[4, mEquipGearIDList[equipID].Item2]--;
								UpdateItem(mEquipPlayer);
								ShowEquipTypeMenu();
							}
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
								var prevMapName = mMapName;
								if (mMapName == "Vesper")
								{
									if (mXAxis == 4)
										mXAxis = mMapHeader.ExitX - 1;
									else if (mXAxis == 70)
										mXAxis = mMapHeader.ExitX + 1;
									else
										mXAxis = mMapHeader.ExitX;

									if (mYAxis == 5)
										mYAxis = mMapHeader.ExitY - 1;
									else if (mYAxis == 69)
										mYAxis = mMapHeader.ExitY + 1;
									else
										mYAxis = mMapHeader.ExitY;

									mMapName = "Ground3";
								}
								else if (mMapName == "Hut")
								{
									if (mXAxis == 4)
										mXAxis = mMapHeader.ExitX - 1;
									else if (mXAxis == 25)
										mXAxis = mMapHeader.ExitX + 1;
									else
										mXAxis = mMapHeader.ExitX;

									if (mYAxis == 5)
										mYAxis = mMapHeader.ExitY - 1;
									else if (mYAxis == 24)
										mYAxis = mMapHeader.ExitY + 1;
									else
										mYAxis = mMapHeader.ExitY;

									mMapName = "Ground4";
								}
								else if (mMapName == "Kobold")
								{
									if (GetBit(86))
									{
										Talk(new string[] {
										" 당신이 문을 나서려 하자  도망쳤던 코볼트킹이 당신 앞을 가로 막았다.",
										"",
										$"[color={RGB.LightMagenta}] 나는 너를 내 손으로 처치하지 않고는 편안히 죽을 수가 없었다." +
										" 그래서 나는 영원한 생명을 가지기를 원했고  그 댓가로 악마에게 혼을 팔았다. 나는 결코 죽지않고 너를 끝까지 괴롭힐 것이다.[/color]"
										}, SpecialEventType.BattleExitKoboldKing);

										return;
									}
									else
									{
										mMapName = mMapHeader.EnterMap;
										mXAxis = mMapHeader.ExitX - 1;
										mYAxis = mMapHeader.ExitY - 1;

										await RefreshGame();
									}
								}
								else if (mMapName == "Ancient")
								{
									if (mXAxis == 4)
										mXAxis = mMapHeader.ExitX - 1;
									else if (mXAxis == 15)
										mXAxis = mMapHeader.ExitX + 1;
									else
										mXAxis = mMapHeader.ExitX;

									if (mYAxis == 5)
										mYAxis = mMapHeader.ExitY - 1;
									else if (mYAxis == 24)
										mYAxis = mMapHeader.ExitY + 1;
									else
										mYAxis = mMapHeader.ExitY;

									mMapName = "Ground4";
								}
								else if (mMapName == "Tomb") {
									mXAxis = mMapHeader.ExitX;
									mYAxis = mMapHeader.ExitY;

									mMapName = mMapHeader.ExitMap;
								}
								else if (mMapName == "Dome") {
									Talk($"[color={RGB.LightBlue}] 당신이 발견한 이 장소는  분명 배리언트 피플즈의 사람들을 로드안의 시각범위에 벗어나게 하기에  충분한 곳일겁니다." +
									"  당신은 정말 훌륭한 일을 해내셨습니다.  제가  곧 당신을 지상으로 이동 시켜 드리겠습니다. 그리고 저는  몇 년 정도 휴식을 취하려 합니다." +
									"  저의 영적 에너지를  마을을 건설하고 사람들을 이동 시키는데 써버려서 에너지 충전 기간이 필요합니다. 여태껏 당신의 도움들 정말 고마왔습니다." +
									" 계속 자신이 결심한 의지대로 꿋꿋하게 나아가십시요.  저는 이제 뒷일을 모두 당신에게 맡기겠습니다.  당신이라면 분명히 해낼거라 믿습니다. 그럼, 안녕히...[/color]", SpecialEventType.FindShelter);

									return;
								}
								else
								{
									mXAxis = mMapHeader.ExitX;
									mYAxis = mMapHeader.ExitY;

									mMapName = mMapHeader.ExitMap;
								}

								await RefreshGame();

								if (mMapName == "Ground5") {
									if (GetBit(6))
										UpdateTileInfo(14, 30, 53);
								}
								else if (mMapName == "DracTown") {
									if (GetBit(111))
										UpdateTileInfo(23, 97, 44);

									if (GetBit(112))
										UpdateTileInfo(23, 91, 44);

									if (GetBit(113))
										UpdateTileInfo(24, 80, 44);

									if (GetBit(114))
										UpdateTileInfo(33, 99, 44);

									if (GetBit(115))
										UpdateTileInfo(35, 92, 44);

									if (GetBit(116))
										UpdateTileInfo(41, 83, 44);

									if (GetBit(117))
										UpdateTileInfo(32, 81, 44);

									if (GetBit(118))
										UpdateTileInfo(40, 69, 44);

									if (GetBit(119))
										UpdateTileInfo(31, 21, 44);

									if (GetBit(120))
										UpdateTileInfo(39, 32, 44);

									if (GetBit(121))
										UpdateTileInfo(15, 18, 44);

									if (GetBit(122))
										UpdateTileInfo(15, 34, 44);

									if (GetBit(123))
										UpdateTileInfo(14, 47, 44);

									if (GetBit(47))
										UpdateTileInfo(14, 56, 44);

									if (GetBit(104))
										UpdateTileInfo(76, 72, 44);

									if (GetBit(105))
										UpdateTileInfo(66, 65, 44);

									if (GetBit(106))
										UpdateTileInfo(66, 72, 44);

									if (GetBit(107))
										UpdateTileInfo(61, 70, 44);

									if (GetBit(108))
										UpdateTileInfo(66, 87, 44);

									if (GetBit(109))
										UpdateTileInfo(61, 78, 44);

									if (GetBit(110))
										UpdateTileInfo(64, 63, 44);

									if (GetBit(103))
									{
										UpdateTileInfo(13, 104, 44);
										UpdateTileInfo(14, 105, 44);
									}

									if (GetBit(102))
									{
										for (var y = 58; y < 61; y++)
										{
											for (var x = 46; x < 48; x++)
												UpdateTileInfo(x, y, 44);
										}
									}

									if (GetBit(101))
									{
										for (var x = 23; x < 27; x++)
											UpdateTileInfo(x, 53, 44);
									}

									if (GetBit(100))
									{
										for (var y = 58; y < 61; y++)
											UpdateTileInfo(83, y, 44);
									}

									if (GetBit(99))
									{
										for (var x = 76; x < 49; x++)
											UpdateTileInfo(x, 87, 44);
									}

									if (GetBit(98))
									{
										for (var x = 58; x < 61; x++)
											UpdateTileInfo(x, 47, 44);
									}

									if (GetBit(97))
									{
										for (var x = 72; x < 75; x++)
											UpdateTileInfo(x, 39, 44);
									}

									if (GetBit(96))
									{
										for (var x = 93; x < 96; x++)
											UpdateTileInfo(x, 36, 44);
									}

									if (GetBit(95))
									{
										UpdateTileInfo(101, 19, 44);
										UpdateTileInfo(101, 20, 42);
										UpdateTileInfo(100, 20, 44);
										UpdateTileInfo(102, 20, 44);
									}

									foreach (var player in mBackupPlayerList) {
										mPlayerList.Add(player);
									}

									mBackupPlayerList.Clear();

									DisplayPlayerInfo();
								}
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

							if (mTrainPlayer.ClassType != ClassCategory.Sword)
							{
								AppendText(" 당신은 전투사 계열이 아닙니다.");
								return;
							}

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

							if (mTrainPlayer.ClassType != ClassCategory.Magic)
							{
								AppendText(" 당신은 마법사 계열이 아닙니다.");
								return;
							}

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
								for (var i = 0; i < 7; i++)
								{
									if (mParty.Crystal[i] > 0)
									{
										hasCrystal = true;
										break;
									}
								}

								if (hasCrystal)
								{
									ShowMenu(MenuMode.BattleChooseItemType, new string[] {
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
								mBattleToolID = 3;
								SelectEnemy();
							}
							else
							{
								var player = mPlayerList[mBattlePlayerID];

								var availCount = 0;
								if (player.ESPMagic > 99)
									availCount = 5;
								else if (player.ESPMagic > 89)
									availCount = 4;
								else if (player.ESPMagic > 79)
									availCount = 3;
								else if (player.ESPMagic > 29)
									availCount = 2;
								else if (player.ESPMagic > 19)
									availCount = 1;

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
							mBattleToolID = mMenuFocusID + 6;

							if (mMenuFocusID > 0)
								SelectEnemy();
							else
							{
								mEnemyFocusID = -1;
								AddBattleCommand();
							}
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
								ShowCharacterMenu(MenuMode.UseItemPlayer);
							else
								UseCrystal(false);
						}
						else if (menuMode == MenuMode.BattleChooseItemType)
						{
							if (mMenuFocusID == 0)
								UseItem(mPlayerList[mBattlePlayerID], true);
							else
							{
								mItemUsePlayer = mPlayerList[mBattlePlayerID];
								UseCrystal(true);
							}
						}
						else if (menuMode == MenuMode.ChooseCrystal) {
							var crystalID = mUsableItemIDList[mMenuFocusID];

							if (crystalID == 5) {
								mAssistPlayer = new Lore()
								{
									Gender = GenderType.Neutral,
									Class = 0,
									Level = 30,
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
									FistSkill = 0,
									AttackMagic = 0,
									PhenoMagic = 0,
									CureMagic = 0,
									ESPMagic = 0,
									SummonMagic = 0,
									SpecialMagic = 0,
								};

								switch (mRand.Next(5))
								{
									case 0:
									case 1:
										mAssistPlayer.Name = "크리스탈 드래곤";
										mAssistPlayer.ClassType = ClassCategory.Dragon;
										mAssistPlayer.Strength = 25;
										mAssistPlayer.Mentality = 20;
										mAssistPlayer.Concentration = 20;
										mAssistPlayer.Endurance = 30;
										mAssistPlayer.Resistance = 20;
										mAssistPlayer.Agility = 0;
										mAssistPlayer.Accuracy = 20;
										mAssistPlayer.Luck = 20;
										mAssistPlayer.Weapon = 49;
										mAssistPlayer.WeaPower = 255;
										mAssistPlayer.PotentialAC = 4;
										break;
									case 2:
									case 3:
										mAssistPlayer.Name = "크리스탈 고렘";
										mAssistPlayer.ClassType = ClassCategory.Golem;
										mAssistPlayer.Strength = 20;
										mAssistPlayer.Mentality = 0;
										mAssistPlayer.Concentration = 0;
										mAssistPlayer.Endurance = 40;
										mAssistPlayer.Resistance = 25;
										mAssistPlayer.Agility = 0;
										mAssistPlayer.Accuracy = 13;
										mAssistPlayer.Luck = 0;
										mAssistPlayer.Weapon = 0;
										mAssistPlayer.WeaPower = 150;
										mAssistPlayer.PotentialAC = 5;

										break;
									case 4:
										mAssistPlayer.Name = "대천사장";
										mAssistPlayer.ClassType = ClassCategory.Magic;
										mAssistPlayer.Strength = 20;
										mAssistPlayer.Mentality = 20;
										mAssistPlayer.Concentration = 20;
										mAssistPlayer.Endurance = 30;
										mAssistPlayer.Resistance = 10;
										mAssistPlayer.Agility = 20;
										mAssistPlayer.Accuracy = 20;
										mAssistPlayer.Luck = 20;
										mAssistPlayer.Weapon = 44;
										mAssistPlayer.WeaPower = 200;
										mAssistPlayer.PotentialAC = 5;

										break;
								}

								mAssistPlayer.AC = mAssistPlayer.PotentialAC;
								mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;

								mParty.Crystal[crystalID]--;
							}
							else if (crystalID == 6) {
								UseEnergyCrystal(false);
							}
							else if (crystalID == 7) {
								if (mMapName == "UnderGrd" && mXAxis == 45 && mYAxis == 47)
								{
									Talk(" 갑자기 당신의 위쪽에서 귀에 익은 음성이 들려왔다.  그 목소리의 주인공은 에인션트 이블이었다.", SpecialEventType.UseCromaticCrystal);
								}
								else
									Dialog(" 크리스탈에는 아무런 반응이 없었다.");
							}
							else if (crystalID == 8) {
								if (mMapHeader.TileType != PositionType.Ground) {
									Dialog(" 크리스탈 볼은 주위가 트인곳에서만 작동합니다.");
								}
								else {
									if (mParty.Etc[25] == 0)
									{
										if (mMapName == "Ground2" && mXAxis == 91 && mYAxis == 11)
										{
											mEncounterEnemyList.Clear();

											JoinEnemy(59);

											mBattleEvent = BattleEvent.OrcRevengeSpirit;
											StartBattle(false);
										}
										else
											await ViewCrystal("Ground2", 91, 11);
									}
									else if (mParty.Etc[25] == 1)
									{
										if (mMapName == "Ground3" && mXAxis == 34 && mYAxis == 33)
										{
											mEncounterEnemyList.Clear();

											JoinEnemy(60);

											mBattleEvent = BattleEvent.TrollRevengeSpirit;
											StartBattle(false);
										}
										else
											await ViewCrystal("Ground3", 34, 33);
									}
									else if (mParty.Etc[25] == 2)
									{
										if (mMapName == "Ground4" && mXAxis == 10 && mYAxis == 90)
										{
											mEncounterEnemyList.Clear();

											JoinEnemy(61);

											mBattleEvent = BattleEvent.KoboldRevengeSpirit;
											StartBattle(false);
										}
										else
											await ViewCrystal("Ground4", 10, 90);
									}
									else if (mParty.Etc[25] == 3)
									{
										if (mMapName == "Ground1" && mXAxis == 43 && mYAxis == 49)
										{
											mEncounterEnemyList.Clear();

											JoinEnemy(62);

											mBattleEvent = BattleEvent.DraconianRevengeSpirit;
											StartBattle(false);
										}
										else
											await ViewCrystal("Ground1", 43, 49);
									}
									else
										Dialog(" 크리스탈 볼 속에는  아무것도 떠오르지 않았다.");
								}
							}
						}
						else if (menuMode == MenuMode.BattleChooseCrystal)
						{
							var crystalID = mUsableItemIDList[mMenuFocusID];

							if (crystalID == 0)
							{
								mBattleCommandID = 1;
								mBattleToolID = 11;
								SelectEnemy();

								mParty.Crystal[crystalID]--;

								mUseCrystalID = -1;
							}
							else if (crystalID == 1)
							{
								mBattleCommandID = 2;
								mBattleToolID = 11;
								mEnemyFocusID = -1;

								mParty.Crystal[crystalID]--;

								mUseCrystalID = -1;

								AddBattleCommand();
							}
							else if (crystalID == 2 || crystalID == 3)
							{
								if (mParty.Crystal[2] > 0 && mParty.Crystal[3] > 0)
								{
									mUseCrystalID = crystalID;
									SelectEnemy();
								}
								else
								{
									Talk(" 크리스탈은 전혀 반응하지 않았다.", SpecialEventType.NextToBattleMode);
									mUseCrystalID = -1;
								}
							}
							else if (crystalID == 4)
							{
								mUseCrystalID = crystalID;
								SelectEnemy();
							}
							else if (crystalID == 5)
							{
								mBattleCommandID = 6;
								mBattleToolID = 11;

								mParty.Crystal[5]--;

								AddBattleCommand();
							}
							else if (crystalID == 6)
								UseEnergyCrystal(true);
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
								switch (mTryEnterMap)
								{
									case "Lore":
										if (mParty.Year < 700)
										{
											mMapName = "Lore";
											await RefreshGame();

											if (GetBit(35))
												UpdateTileInfo(40, 9, 44);

											if (GetBit(36))
												UpdateTileInfo(21, 49, 44);

											if (GetBit(37))
												UpdateTileInfo(21, 53, 44);

											if (GetBit(38))
												UpdateTileInfo(17, 37, 47);
										}
										else
										{
											mMapName = "Dark";

											await RefreshGame();
										}
										break;
									case "LastDtch":
										mMapName = "LastDtch";
										await RefreshGame();

										if (GetBit(39))
											UpdateTileInfo(60, 57, 44);

										if (GetBit(41))
											UpdateTileInfo(33, 29, 47);
										break;
									case "Menace":
										mMapName = "Menace";
										await RefreshGame();

										break;
									case "GaiaGate":
										if (!GetBit(50))
										{
											var startX = mMapHeader.EnterX;
											var startY = mMapHeader.EnterY;

											mMapName = mMapHeader.EnterMap;

											await RefreshGame();

											mXAxis = startX;
											mYAxis = startY;

											if (GetBit(39))
												UpdateTileInfo(16, 23, 27);

											if (GetBit(41))
												UpdateTileInfo(55, 39, 27);
										}
										else
										{
											var startX = mMapHeader.EnterX;
											var startY = mMapHeader.EnterY;

											mMapName = "Light";

											await RefreshGame();

											mXAxis = startX;
											mYAxis = startY;
										}
										break;
									case "LoreGate":
										{
											var startX = mMapHeader.EnterX;
											var startY = mMapHeader.EnterY;

											mMapName = mMapHeader.EnterMap;

											await RefreshGame();

											mXAxis = startX;
											mYAxis = startY;

											if (GetBit(39))
												UpdateTileInfo(16, 23, 27);

											if (GetBit(41))
												UpdateTileInfo(55, 39, 27);
											break;
										}
									case "Valiant":
										if (!GetBit(50))
											mMapName = "Valiant";
										else
											mMapName = "Light";

										await RefreshGame();

										if (GetBit(40))
											UpdateTileInfo(16, 23, 27);

										if (GetBit(42))
											UpdateTileInfo(55, 39, 27);

										break;
									case "Gaea":
										mMapName = mTryEnterMap;

										await RefreshGame();

										if (GetBit(43))
											UpdateTileInfo(24, 8, 44);

										if (GetBit(44))
											UpdateTileInfo(10, 35, 44);

										break;
									case "OrcTown":
										mMapName = mTryEnterMap;

										await RefreshGame();

										if (GetBit(57))
											UpdateTileInfo(9, 38, 34);

										if (GetBit(58))
											UpdateTileInfo(8, 18, 34);

										if (GetBit(59))
											UpdateTileInfo(12, 11, 34);

										if (GetBit(60))
											UpdateTileInfo(20, 8, 34);

										if (GetBit(61))
											UpdateTileInfo(42, 11, 34);

										if (GetBit(62))
											UpdateTileInfo(43, 29, 34);

										if (GetBit(63))
											UpdateTileInfo(34, 28, 34);

										if (GetBit(64))
											UpdateTileInfo(40, 38, 34);

										if (GetBit(53))
										{
											UpdateTileInfo(24, 41, 43);
											UpdateTileInfo(24, 42, 43);
										}

										if (GetBit(54))
										{
											UpdateTileInfo(23, 19, 43);
											UpdateTileInfo(25, 19, 43);
										}

										if (GetBit(55))
											UpdateTileInfo(22, 17, 43);

										if (GetBit(56))
										{
											UpdateTileInfo(24, 16, 43);
											UpdateTileInfo(25, 16, 43);
										}
										break;
									case "Vesper":
										{
											var startX = 0;
											var startY = 0;

											if (mXAxis < 68)
											{
												startX = 5;
												startY = 37;
											}
											else if (mXAxis > 68)
											{
												startX = 69;
												startY = 37;
											}
											else if (mYAxis < 28)
											{
												startX = 37;
												startY = 6;
											}
											else if (mYAxis > 28)
											{
												startX = 37;
												startY = 68;
											}

											mMapName = mTryEnterMap;

											await RefreshGame();

											mXAxis = startX;
											mYAxis = startY;

											if (GetBit(65))
												UpdateTileInfo(29, 18, 43);

											if (GetBit(66))
												UpdateTileInfo(49, 18, 43);

											if (GetBit(67))
												UpdateTileInfo(63, 25, 43);

											if (GetBit(68))
												UpdateTileInfo(60, 32, 43);

											if (GetBit(69))
												UpdateTileInfo(39, 39, 43);

											if (GetBit(70))
												UpdateTileInfo(21, 39, 43);

											if (GetBit(71))
												UpdateTileInfo(19, 25, 43);

											break;
										}
									case "TrolTown":
										mMapName = mTryEnterMap;

										await RefreshGame();

										if (GetBit(46))
											UpdateTileInfo(36, 16, 44);

										if (GetBit(72))
										{
											UpdateTileInfo(24, 7, 44);
											UpdateTileInfo(25, 7, 44);
										}

										if (GetBit(73))
											UpdateTileInfo(16, 18, 44);

										if (GetBit(74))
											UpdateTileInfo(33, 18, 44);

										if (GetBit(75))
										{
											UpdateTileInfo(12, 31, 44);
											UpdateTileInfo(12, 32, 44);
										}

										if (GetBit(76))
										{
											UpdateTileInfo(37, 31, 44);
											UpdateTileInfo(37, 32, 44);
										}

										if (GetBit(77))
										{
											UpdateTileInfo(23, 66, 44);
											UpdateTileInfo(24, 66, 44);
											UpdateTileInfo(25, 66, 44);
										}

										if (GetBit(78))
											UpdateTileInfo(5, 30, 44);

										if (GetBit(79))
											UpdateTileInfo(8, 29, 44);

										if (GetBit(80))
											UpdateTileInfo(6, 32, 44);

										if (GetBit(81))
											UpdateTileInfo(10, 34, 44);

										if (GetBit(82))
											UpdateTileInfo(39, 29, 44);

										if (GetBit(83))
											UpdateTileInfo(40, 34, 44);

										if (GetBit(84))
											UpdateTileInfo(42, 31, 44);

										if (GetBit(85))
											UpdateTileInfo(44, 33, 44);

										break;
									case "Kobold":
										mMapName = mTryEnterMap;

										await RefreshGame();

										if (mParty.Etc[29] == 0)
											mParty.Etc[29] = (mRand.Next(5) + 1) << 4 + mRand.Next(5) + 1;

										if (mParty.Etc[30] == 0)
											mParty.Etc[30] = mRand.Next(31) + 1;

										break;
									case "Ancient":
										{
											var startX = 0;
											var startY = 0;

											if (mXAxis < 19)
											{
												startX = 5;
												startY = 14;
											}
											else if (mXAxis > 19)
											{
												startX = 13;
												startY = 14;
											}
											else if (mYAxis < 38)
											{
												startX = 9;
												startY = 6;
											}
											else if (mYAxis > 38)
											{
												startX = 9;
												startY = 23;
											}

											mMapName = mTryEnterMap;

											await RefreshGame();

											mXAxis = startX;
											mYAxis = startY;

											if (GetBit(28))
											{
												for (var x = 9; x < 11; x++)
													UpdateTileInfo(x, 16, 44);
											}

											if (GetBit(23))
											{
												for (var x = 9; x < 11; x++)
													UpdateTileInfo(x, 12, 44);
											}
											break;
										}
									case "Hut":
										{
											var startX = 0;
											var startY = 0;

											if (mXAxis < 47)
											{
												startX = 5;
												startY = 14;
											}
											else if (mXAxis > 47)
											{
												startX = 24;
												startY = 14;
											}
											else if (mYAxis < 56)
											{
												startX = 14;
												startY = 6;
											}
											else if (mYAxis > 56)
											{
												startX = 14;
												startY = 23;
											}

											mMapName = mTryEnterMap;

											await RefreshGame();

											mXAxis = startX;
											mYAxis = startY;

											if (GetBit(204))
												UpdateTileInfo(7, 12, 35);

											if (GetBit(205))
												UpdateTileInfo(7, 13, 35);
											break;
										}
									case "DracTown":
										mMapName = mTryEnterMap;

										await RefreshGame();

										if (GetBit(111))
											UpdateTileInfo(23, 97, 44);

										if (GetBit(112))
											UpdateTileInfo(23, 91, 44);

										if (GetBit(113))
											UpdateTileInfo(24, 80, 44);

										if (GetBit(114))
											UpdateTileInfo(33, 99, 44);

										if (GetBit(115))
											UpdateTileInfo(35, 92, 44);

										if (GetBit(116))
											UpdateTileInfo(41, 83, 44);

										if (GetBit(117))
											UpdateTileInfo(32, 81, 44);

										if (GetBit(118))
											UpdateTileInfo(40, 69, 44);

										if (GetBit(119))
											UpdateTileInfo(31, 21, 44);

										if (GetBit(120))
											UpdateTileInfo(39, 32, 44);

										if (GetBit(121))
											UpdateTileInfo(15, 18, 44);

										if (GetBit(122))
											UpdateTileInfo(15, 34, 44);

										if (GetBit(123))
											UpdateTileInfo(14, 47, 44);

										if (GetBit(47))
											UpdateTileInfo(14, 56, 44);

										if (GetBit(104))
											UpdateTileInfo(76, 72, 44);

										if (GetBit(105))
											UpdateTileInfo(66, 65, 44);

										if (GetBit(106))
											UpdateTileInfo(66, 72, 44);

										if (GetBit(107))
											UpdateTileInfo(61, 70, 44);

										if (GetBit(108))
											UpdateTileInfo(66, 87, 44);

										if (GetBit(109))
											UpdateTileInfo(61, 78, 44);

										if (GetBit(110))
											UpdateTileInfo(64, 63, 44);

										if (GetBit(103))
										{
											UpdateTileInfo(13, 104, 44);
											UpdateTileInfo(14, 105, 44);
										}

										if (GetBit(102))
										{
											for (var y = 58; y < 61; y++)
											{
												for (var x = 46; x < 48; x++)
													UpdateTileInfo(x, y, 44);
											}
										}

										if (GetBit(101)) {
											for (var x = 23; x < 27; x++)
												UpdateTileInfo(x, 53, 44);
										}

										if (GetBit(100)) {
											for (var y = 58; y < 61; y++)
												UpdateTileInfo(83, y, 44);
										}

										if (GetBit(99)) {
											for (var x = 76; x < 49; x++)
												UpdateTileInfo(x, 87, 44);
										}

										if (GetBit(98)) {
											for (var x = 58; x < 61; x++)
												UpdateTileInfo(x, 47, 44);
										}

										if (GetBit(97)) {
											for (var x = 72; x < 75; x++)
												UpdateTileInfo(x, 39, 44);
										}

										if (GetBit(96)) {
											for (var x = 93; x < 96; x++)
												UpdateTileInfo(x, 36, 44);
										}

										if (GetBit(95)) {
											UpdateTileInfo(101, 19, 44);
											UpdateTileInfo(101, 20, 42);
											UpdateTileInfo(100, 20, 44);
											UpdateTileInfo(102, 20, 44);
										}

										break;
									default:
										mMapName = mTryEnterMap;

										await RefreshGame();

										break;
								}
							}
							else
							{
								ClearDialog();
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
								BackupPlayerList = mBackupPlayerList,
								AssistPlayer = mAssistPlayer,
								Party = mParty,
								MapHeader = mMapHeader,
								Encounter = mEncounter,
								MaxEnemy = mMaxEnemy,
								Cruel = mCruel,
								Ebony = mEbony,
								MoonLight = mMoonLight,
								SaveTime = DateTime.Now.Ticks,
								WatchYear = mWatchYear,
								WatchDay = mWatchDay,
								WatchHour = mWatchHour,
								WatchMin = mWatchMin,
								WatchSec = mWatchSec,
								TimeWatch = mTimeWatch,
								TimeEvent = mTimeEvent,
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
						else if (menuMode == MenuMode.MeetPollux)
						{
							if (mMenuFocusID == 0)
							{
								mParty.Gold = 0;
								Dialog(" 당신은 강도에게 모든 돈을 빼았겼다.");

								mEncounterEnemyList.Clear();
								ShowMap();
							}
							else
							{
								mBattleEvent = BattleEvent.Pollux;
								StartBattle(true);
							}
						}
						else if (menuMode == MenuMode.JoinPollux)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
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
						else if (menuMode == MenuMode.LearnTrollWriting)
						{
							if (mMenuFocusID == 0)
							{
								if (mParty.Food >= 50)
								{
									InvokeAnimation(AnimationType.LearnTrollWriting);
								}
								else
									Dialog(" 하지만 당신에게는 충분한 식량이 없군요.");
							}
							else
								ClearDialog();
						}
						else if (menuMode == MenuMode.JoinAltair)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var altair = new Lore()
									{
										Name = "알타이르",
										Gender = GenderType.Male,
										Class = 1,
										ClassType = ClassCategory.Sword,
										Level = 3,
										Strength = 19,
										Mentality = 4,
										Concentration = 3,
										Endurance = 18,
										Resistance = 10,
										Agility = 16,
										Accuracy = 15,
										Luck = 14,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 9,
										Shield = 2,
										Armor = 2,
										PotentialAC = 2,
										SwordSkill = 10,
										AxeSkill = 30,
										SpearSkill = 10,
										BowSkill = 10,
										ShieldSkill = 10,
										FistSkill = 10
									};

									altair.HP = altair.Endurance * altair.Level * 10;
									altair.UpdatePotentialExperience();
									UpdateItem(altair);

									mPlayerList.Add(altair);
									DisplayPlayerInfo();

									SetBit(39);
									UpdateTileInfo(60, 57, 44);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
								}
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.JoinVega)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var vega = new Lore()
									{
										Name = "베가",
										Gender = GenderType.Female,
										Class = 2,
										ClassType = ClassCategory.Magic,
										Level = 4,
										Strength = 6,
										Mentality = 16,
										Concentration = 19,
										Endurance = 10,
										Resistance = 17,
										Agility = 9,
										Accuracy = 15,
										Luck = 18,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 0,
										Shield = 0,
										Armor = 1,
										PotentialAC = 0,
										AttackMagic = 10,
										PhenoMagic = 40,
										CureMagic = 10,
										SpecialMagic = 10,
										ESPMagic = 10,
										SummonMagic = 10
									};

									vega.HP = vega.Endurance * vega.Level * 10;
									vega.UpdatePotentialExperience();
									UpdateItem(vega);

									mPlayerList.Add(vega);
									DisplayPlayerInfo();

									SetBit(41);
									UpdateTileInfo(33, 29, 47);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
								}
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.JoinAlgol)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var algol = new Lore()
									{
										Name = "알골",
										Gender = GenderType.Male,
										Class = 3,
										ClassType = ClassCategory.Magic,
										Level = 3,
										Strength = 10,
										Mentality = 17,
										Concentration = 15,
										Endurance = 8,
										Resistance = 12,
										Agility = 11,
										Accuracy = 14,
										Luck = 20,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 0,
										Shield = 0,
										Armor = 1,
										PotentialAC = 0,
										AttackMagic = 10,
										PhenoMagic = 20,
										CureMagic = 20,
										SpecialMagic = 0,
										ESPMagic = 0,
										SummonMagic = 35
									};

									algol.HP = algol.Endurance * algol.Level * 10;
									algol.UpdatePotentialExperience();
									UpdateItem(algol);

									mPlayerList.Add(algol);
									DisplayPlayerInfo();

									SetBit(40);
									UpdateTileInfo(16, 23, 27);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
								}
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.JoinProxima)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var proxima = new Lore()
									{
										Name = "프록시마",
										Gender = GenderType.Male,
										Class = 4,
										ClassType = ClassCategory.Sword,
										Level = 4,
										Strength = 15,
										Mentality = 5,
										Concentration = 6,
										Endurance = 16,
										Resistance = 11,
										Agility = 16,
										Accuracy = 19,
										Luck = 10,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 26,
										Shield = 0,
										Armor = 2,
										PotentialAC = 1,
										SwordSkill = 0,
										AxeSkill = 0,
										SpearSkill = 10,
										BowSkill = 40,
										ShieldSkill = 0,
										FistSkill = 10
									};

									proxima.HP = proxima.Endurance * proxima.Level * 10;
									proxima.UpdatePotentialExperience();
									UpdateItem(proxima);

									mPlayerList.Add(proxima);
									DisplayPlayerInfo();

									SetBit(42);
									UpdateTileInfo(55, 39, 27);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
								}
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.JoinDenebola)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var denebola = new Lore()
									{
										Name = "데네볼라",
										Gender = GenderType.Male,
										Class = 5,
										ClassType = ClassCategory.Sword,
										Level = 10,
										Strength = 20,
										Mentality = 5,
										Concentration = 6,
										Endurance = 18,
										Resistance = 12,
										Agility = 15,
										Accuracy = 17,
										Luck = 10,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 0,
										Shield = 0,
										Armor = 3,
										PotentialAC = 3,
										SwordSkill = 0,
										AxeSkill = 0,
										BowSkill = 0,
										SpearSkill = 10,
										ShieldSkill = 0,
										FistSkill = 60
									};

									denebola.HP = denebola.Endurance * denebola.Level * 10;
									denebola.UpdatePotentialExperience();
									UpdateItem(denebola);

									mPlayerList.Add(denebola);
									DisplayPlayerInfo();

									SetBit(43);
									UpdateTileInfo(24, 8, 44);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
								}
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.JoinCapella)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var capella = new Lore()
									{
										Name = "카펠라",
										Gender = GenderType.Female,
										Class = 1,
										ClassType = ClassCategory.Magic,
										Level = 2,
										Strength = 5,
										Mentality = 15,
										Concentration = 20,
										Endurance = 10,
										Resistance = 19,
										Agility = 11,
										Accuracy = 17,
										Luck = 15,
										Poison = 0,
										Unconscious = 0,
										Dead = 0,
										SP = 0,
										Experience = 0,
										Weapon = 0,
										Shield = 0,
										Armor = 0,
										PotentialAC = 0,
										AttackMagic = 10,
										PhenoMagic = 10,
										CureMagic = 10,
										SpecialMagic = 10,
										ESPMagic = 10,
										SummonMagic = 0
									};

									capella.HP = capella.Endurance * capella.Level * 10;
									capella.UpdatePotentialExperience();
									UpdateItem(capella);

									mPlayerList.Add(capella);
									DisplayPlayerInfo();

									SetBit(44);
									UpdateTileInfo(10, 35, 44);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
								}
							}
							else
								Dialog(" 그렇다면 할 수 없군요.  제 스승인 안타레스 Jr.님께서 로어성에 가신후 좋은 스승을 못 찾았기에 당신에게 부탁 드렸던건데...");
						}
						else if (menuMode == MenuMode.LearnOrcWriting)
						{
							if (mMenuFocusID == 0)
							{
								if (mParty.Gold >= 2_000)
								{
									mParty.Gold -= 2_000;

									mParty.Day += 7;
									if (mParty.Day > 360)
									{
										mParty.Year++;
										mParty.Day = mParty.Day % 360 + 1;
									}

									mParty.Hour = 11;
									mParty.Min = 0;
									PlusTime(0, 0, 1);

									InvokeAnimation(AnimationType.LearnOrcWriting);
								}
								else
									Dialog(" 하지만 당신에게는 충분한 금이 없군요.");
							}
							else
								ClearDialog();
						}
						else if (menuMode == MenuMode.KillOrc1 ||
						menuMode == MenuMode.KillOrc2 ||
						menuMode == MenuMode.KillOrc3 ||
						menuMode == MenuMode.KillOrc4 ||
						menuMode == MenuMode.KillOrc5 ||
						menuMode == MenuMode.KillOrc6 ||
						menuMode == MenuMode.KillOrc7 ||
						menuMode == MenuMode.KillOrc8)
						{
							switch (menuMode)
							{
								case MenuMode.KillOrc1:
									mBattleEvent = BattleEvent.Orc1;
									break;
								case MenuMode.KillOrc2:
									mBattleEvent = BattleEvent.Orc2;
									break;
								case MenuMode.KillOrc3:
									mBattleEvent = BattleEvent.Orc3;
									break;
								case MenuMode.KillOrc4:
									mBattleEvent = BattleEvent.Orc4;
									break;
								case MenuMode.KillOrc5:
									mBattleEvent = BattleEvent.Orc5;
									break;
								case MenuMode.KillOrc6:
									mBattleEvent = BattleEvent.Orc6;
									break;
								case MenuMode.KillOrc7:
									mBattleEvent = BattleEvent.Orc7;
									break;
								case MenuMode.KillOrc8:
									mBattleEvent = BattleEvent.Orc8;
									break;
							}


							mEncounterEnemyList.Clear();
							JoinEnemy(17);

							DisplayEnemy();
							HideMap();

							StartBattle(true);
						}
						else if (menuMode == MenuMode.GuardOrcTown)
						{
							if (mMenuFocusID == 0)
								ClearDialog();
							else
							{
								mEncounterEnemyList.Clear();
								for (var i = 0; i < 7; i++)
									JoinEnemy(18);

								DisplayEnemy();
								HideMap();

								mBattleEvent = BattleEvent.OrcTownEnterance;
								StartBattle(true);
							}
						}
						else if (menuMode == MenuMode.NegotiateTrollKing)
						{
							if (mMenuFocusID == 0)
							{
								Ask($"[color={RGB.LightMagenta}] 당신이 무슨 목적으로  나의 방까지 쳐들어와서 나를 죽이려 하는지 모르겠지만" +
								"  어쨌든 당신이 여기서 물러나 준다면 지금 이 상자 안에 있는 금의 절반을 당신에게 주겠다. 이 정도면 충분한가?[/color]"
								, MenuMode.TrollKingSuggestion, new string[] {
									"좋소, 이걸 받고 물러 나지요",
									"그 상자 안의 금을 모두 주시오"
								});
							}
							else
							{
								BattleTrollKing();
							}
						}
						else if (menuMode == MenuMode.TrollKingSuggestion)
						{
							if (mMenuFocusID == 0)
							{
								mMapName = mMapHeader.ExitMap;
								mXAxis = mMapHeader.ExitX;
								mYAxis = mMapHeader.ExitY;

								await RefreshGame();

								Dialog(" 당신은 트롤킹에게 금을 받고는  동굴 밖으로 나왔다.");

								SetBit(16);
							}
							else
							{
								BattleTrollKing();
							}
						}
						else if (menuMode == MenuMode.PhysicistTeaching)
						{
							Talk(" 오, 정말 감사하오. 나는 드라코니안족에게서 물리학을 배우고 이곳에 왔던 참이라오.  나는 차원에 관한 여러가지 연구를 했었소." +
							"  그러던 중에  나는 로어 세계에서도 다른 차원으로 가는 길이 있다는걸 알아냈소." +
							" 그곳의 중력은 무슨 이유에서인지 상당히 밀도가 높게 압축되어 있었고" +
							" 태양의 인력이 반대로 작용하는 자정무렵에는 그 지역의 공간이 일그러져  다른 세계로의 문이 열리는 것을  목격했소." +
							"  그 공간의 창을 통해 내가 본것은  허허 벌판이나 죽어가는 행성의  최후의 모습같은 것들도  있었지만 때때로  생명체가 사는 별들과 연결 되는 것도 확인했소." +
							"  거대한 파충류가  사는 곳도 있고, 규질의 몸뚱이를 가지고 질소를 마시며 생활하는 생물도 보았소." +
							" 그중에서도 내가 가장 관심있게 본 것은, 우리와 같이 C,H,O,N 으로 구성된 피부를 가지고  산화작용으로 ATP를 합성하는 생물이 사는 곳이었소." +
							" 그들은 마법도 사용할 줄 모르고 생활하고 있지만  대단한 살상무기를 많이 보유하고 있었소.  또한 '뉴욕'이란 도시를 보았을때 정말 그들의 과학력과 건축기술에 탄복하고 말았소." +
							" 당신도 내 말을 믿을수 있겠소?", SpecialEventType.KillPhysicist);
						}
						else if (menuMode == MenuMode.KillTroll6 ||
						menuMode == MenuMode.KillTroll7 ||
						menuMode == MenuMode.KillTroll8 ||
						menuMode == MenuMode.KillTroll9 ||
						menuMode == MenuMode.KillTroll10 ||
						menuMode == MenuMode.KillTroll11 ||
						menuMode == MenuMode.KillTroll12 ||
						menuMode == MenuMode.KillTroll13)
						{
							if (mMenuFocusID == 0)
							{
								switch (menuMode)
								{
									case MenuMode.KillTroll6:
										mBattleEvent = BattleEvent.Troll6;
										break;
									case MenuMode.KillTroll7:
										mBattleEvent = BattleEvent.Troll7;
										break;
									case MenuMode.KillTroll8:
										mBattleEvent = BattleEvent.Troll8;
										break;
									case MenuMode.KillTroll9:
										mBattleEvent = BattleEvent.Troll9;
										break;
									case MenuMode.KillTroll10:
										mBattleEvent = BattleEvent.Troll10;
										break;
									case MenuMode.KillTroll11:
										mBattleEvent = BattleEvent.Troll11;
										break;
									case MenuMode.KillTroll12:
										mBattleEvent = BattleEvent.Troll12;
										break;
									case MenuMode.KillTroll13:
										mBattleEvent = BattleEvent.Troll13;
										break;
								}


								mEncounterEnemyList.Clear();
								JoinEnemy(24);

								DisplayEnemy();
								HideMap();

								StartBattle(true);
							}
						}
						else if (menuMode == MenuMode.JoinBercux)
						{
							if (mMenuFocusID == 0)
							{
								if (mPlayerList.Count < 5)
								{
									var bercux = new Lore()
									{
										Name = "베크룩스",
										Gender = GenderType.Male,
										Class = 1,
										ClassType = ClassCategory.Magic,
										Level = 9,
										Strength = 10,
										Mentality = 17,
										Concentration = 18,
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
										Weapon = 0,
										Shield = 0,
										Armor = 1,
										PotentialAC = 1,
										AttackMagic = 40,
										PhenoMagic = 40,
										CureMagic = 40,
										SpecialMagic = 0,
										ESPMagic = 30,
										SummonMagic = 20
									};

									bercux.HP = bercux.Endurance * bercux.Level * 10;
									bercux.UpdatePotentialExperience();
									UpdateItem(bercux);

									mPlayerList.Add(bercux);
									DisplayPlayerInfo();

									SetBit(46);
									UpdateTileInfo(36, 16, 44);
								}
								else
								{
									Dialog(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
								}
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.LearnKoboldWriting)
						{
							if (mMenuFocusID == 0)
							{
								if (mParty.Item[0] >= 5)
								{
									mParty.Item[0] -= 5;

									InvokeAnimation(AnimationType.LearnKoboldWriting);
								}
								else
								{
									Dialog(" 하지만 당신에게는 체력 회복약 5개가 없다.");
								}
							}
							else
								ClearDialog();
						}
						else if (menuMode == MenuMode.CrossLava)
						{
							if (mMenuFocusID == 0)
							{
								mXAxis = 57;
								mYAxis = 62;
							}
							else
								UpdateTileInfo(47, 72, 0);
						}
						else if (menuMode == MenuMode.Answer1_1)
						{
							if (mMenuFocusID != mAnswerID)
								mAllPass = false;

							var options = ShuffleOptions(new string[] {
								"실리안 카미너스",
								"안타레스 Jr.",
								"드라코니안",
								"에인션트 이블"
							});

							for (var i = 0; i < options.Length; i++)
							{
								if (options[i] == "에인션트 이블")
								{
									mAnswerID = i;
									break;
								}
							}

							Ask($"[color={RGB.White}] 이 게임의 1,2,3 편에 모두 출현한 인물은 누구인가?[/color]",
							MenuMode.Answer1_2, options);
						}
						else if (menuMode == MenuMode.Answer1_2)
						{
							if (mMenuFocusID != mAnswerID)
								mAllPass = false;

							var options = ShuffleOptions(new string[] {
								"카노푸스",
								"안타레스 Jr.",
								"드라코니안",
								"제작자(안영기)"
							});

							for (var i = 0; i < options.Length; i++)
							{
								if (options[i] == "제작자(안영기)")
								{
									mAnswerID = i;
									break;
								}
							}

							Ask($"[color={RGB.White}] 이 게임에서 1,3 편만 출현한 인물이 아닌 것은 누구인가?[/color]",
							MenuMode.Answer1_3, options);
						}
						else if (menuMode == MenuMode.Answer1_3)
						{
							if (mMenuFocusID != mAnswerID)
								mAllPass = false;

							if (mAllPass)
							{
								Dialog(new string[] {
								" 보물상자는 스스로 스르륵 열렸다.",
								"",
								$"[color={RGB.LightCyan}] [[ 인내력 + 0 에서 + 4 ][/color]"
								});

								foreach (var player in mPlayerList)
								{
									player.Endurance += player.Luck / 5;
								}

								if (mAssistPlayer != null)
									mAssistPlayer.Endurance += mAssistPlayer.Luck / 5;

								SetBit(147);
							}
							else {
								SetBit(24);
								BattleTreasureBox(SpecialEventType.BattleTreasureBox1);
							}
						}
						else if (menuMode == MenuMode.Answer2_1)
						{
							if (mMenuFocusID != mAnswerID)
								mAllPass = false;

							var options = ShuffleOptions(new string[] {
								"질량보존의 법칙",
								"작용 반작용의 법칙",
								"특수 상대성 이론",
								"일반 상대성 이론"
							});

							for (var i = 0; i < options.Length; i++)
							{
								if (options[i] == "일반 상대성 이론")
								{
									mAnswerID = i;
									break;
								}
							}

							Ask($"[color={RGB.White}] 당신은 종종 공간이동을 하여  위치를 이동하기도 한다." +
							"  그리고 그때 당신은 3차원의 공간에  강한 중력을 걸어  공간을 압축 시킨후 그 공간면을 뚫고  이동하고 나서  공간을 원위치시킴으로써(워프 방식) 그것을 가능하게 한다." +
							" 이것은 무슨 법칙으로 증명될 수 있겠는가?[/color]",
							MenuMode.Answer2_2, options);
						}
						else if (menuMode == MenuMode.Answer2_2)
						{
							if (mMenuFocusID != mAnswerID)
								mAllPass = false;

							var options = ShuffleOptions(new string[] {
								"Hydrogen",
								"Carbon",
								"Nitrogen",
								"Oxygen"
							});

							for (var i = 0; i < options.Length; i++)
							{
								if (options[i] == "Oxygen")
								{
									mAnswerID = i;
									break;
								}
							}

							Ask($"[color={RGB.White}] 당신은 '마법 화구'라는  불공을 만들어 적에게 던져 제압해본 경험이 있다." +
							" 당신은 당신의 손 위에 불덩어리를 만들기 위해서  이 물질을 결합시킨다. 이 물질의 이름은 무엇인가?[/color]",
							MenuMode.Answer2_3, options);
						}
						else if (menuMode == MenuMode.Answer2_3)
						{
							if (mMenuFocusID != mAnswerID)
								mAllPass = false;

							if (mAllPass)
							{
								Dialog(new string[] {
								" 보물상자는 스스로 스르륵 열렸다.",
								"",
								$"[color={RGB.LightCyan}] [[ 소환의 크리스탈 + 1 ][/color]"
								});

								mParty.Crystal[5]++;

								SetBit(148);
							}
							else
							{
								SetBit(25);
								BattleTreasureBox(SpecialEventType.BattleTreasureBox2);
							}
						}
						else if (menuMode == MenuMode.Answer3_1)
						{
							if (mMenuFocusID != 1)
								mAllPass = false;

							Ask($"[color={RGB.White}] 예전에는 화성과 목성 사이에 혹성이 하나 있었다.[/color]",
							MenuMode.Answer3_2, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (menuMode == MenuMode.Answer3_2)
						{
							if (mMenuFocusID != 0)
								mAllPass = false;

							Ask($"[color={RGB.White}] 태양의 나이는 50 억살이다.  80 억년 전에도 지금의 태양 자리에 항성이 있었을 것이다.[/color]",
							MenuMode.Answer3_3, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (menuMode == MenuMode.Answer3_3)
						{
							if (mMenuFocusID != 0)
								mAllPass = false;

							Ask($"[color={RGB.White}] 우주의 크기는 변함이 없다.[/color]",
							MenuMode.Answer3_3, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (menuMode == MenuMode.Answer3_4)
						{
							if (mMenuFocusID != 1)
								mAllPass = false;

							Ask($"[color={RGB.White}] 태양계 혹성의 순서는  수성, 금성, 지구,... 천왕성, 해왕성, 명왕성의 순서로 일정하다.[/color]",
							MenuMode.Answer3_5, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (menuMode == MenuMode.Answer3_5)
						{
							if (mMenuFocusID != 1)
								mAllPass = false;

							Ask($"[color={RGB.White}] 토성을 물에 띄우면 뜬다.[/color]",
							MenuMode.Answer3_6, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (menuMode == MenuMode.Answer3_6)
						{
							if (mMenuFocusID != 0)
								mAllPass = false;

							if (mAllPass)
							{
								Dialog(new string[] {
								" 보물상자는 스스로 스르륵 열렸다.",
								"",
								$"[color={RGB.LightCyan}] [[ 화염의 크리스탈 + 1 ][/color]"
								});

								mParty.Crystal[0]++;

								SetBit(149);
							}
							else
							{
								SetBit(26);
								BattleTreasureBox(SpecialEventType.BattleTreasureBox3);
							}
						}
						else if (menuMode == MenuMode.Answer4_1)
						{
							if (mMenuFocusID != 1)
								mAllPass = false;

							Ask($"[color={RGB.White}] Pascal에서 대입 기호는 ''=''이다.[/color]",
							MenuMode.Answer4_2, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (menuMode == MenuMode.Answer4_2)
						{
							if (mMenuFocusID != 1)
								mAllPass = false;

							Ask($"[color={RGB.White}] C 언어는  영문 소문자를  기본으로 설계되었다.[/color]",
							MenuMode.Answer4_3, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (menuMode == MenuMode.Answer4_3)
						{
							if (mMenuFocusID != 0)
								mAllPass = false;

							Ask($"[color={RGB.White}] FORTRAN은 subroutine 기능이 없다.[/color]",
							MenuMode.Answer3_3, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (menuMode == MenuMode.Answer4_4)
						{
							if (mMenuFocusID != 1)
								mAllPass = false;

							Ask($"[color={RGB.White}] Small Talk, C++, Pascal은 현재 모두 객체지향 프로그래밍을 지원한다.[/color]",
							MenuMode.Answer4_5, new string[] {
								"맞다",
								"아니다"
							});
						}
						else if (menuMode == MenuMode.Answer4_5)
						{
							if (mMenuFocusID != 0)
								mAllPass = false;

							if (mAllPass)
							{
								Dialog(new string[] {
								" 보물상자는 스스로 스르륵 열렸다.",
								"",
								$"[color={RGB.LightCyan}] [[ 체력 또는 정신력 + 0 에서 + 2 ][/color]"
								});

								foreach (var player in mPlayerList)
								{
									if (player.ClassType == ClassCategory.Magic)
										player.Mentality += (player.Luck / 5 + 1) / 2;
									else
										player.Strength += (player.Luck / 5 + 1) / 2;
								}

								if (mAssistPlayer != null)
									mAssistPlayer.Strength += (mAssistPlayer.Luck / 5 + 1) / 2;
								
								SetBit(150);
							}
							else
							{
								SetBit(27);
								BattleTreasureBox(SpecialEventType.BattleTreasureBox4);
							}
						}
						else if (menuMode == MenuMode.MeetAncientEvil) {
							if (mMenuFocusID == 0) {
								mEncounterEnemyList.Clear();

								for (var i = 0; i < 2; i++)
									JoinEnemy(47).Name = "드라코니안 신도";

								DisplayEnemy();
								HideMap();

								mBattleEvent = BattleEvent.DraconianBeliever;
								StartBattle();
							}
							else {
								Dialog($"[color={RGB.LightMagenta}] 하지만 그분은 벌써 몇 백년 전에 돌아가셨습니다. 지금은 그분의 말씀만 남아 우리들을 올바르게 지도하고 계십니다.[/color]");
							}
						}
						else if (menuMode == MenuMode.KillMermaid) {
							SetBit(29);
							if (mMenuMode == 0)
							{
								Dialog(new string[] {
									$"[color={RGB.LightBlue}] 그 인어는 당신에게 금빛의 가루를 뿌렸다.[/color]",
									$"[color={RGB.LightBlue}] 그러자 당신은 약간의 어지러움을 느꼈다.[/color]"
								});

								SetBit(14);
							}
							else
								Dialog(" 당신은 그 인어를 죽여 버렸다.");
						}
						else if (menuMode == MenuMode.EnterTomb) {
							if (mMenuFocusID == 0) {
								Lore friend = null;
								if (mPlayerList.Count > 1)
									friend = mPlayerList[mPlayerList.Count - 1];
								
								if (friend != null) {
									Talk(new string[] {
										" 당신이 일행들을 데리고 지하계단으로 내려가려 하자 당신을 저지시키는 사람이 있었다.",
										$" 그는 바로 {friend.NameSubjectCJosa}였다.",
										" 그는 말을 했다."
									}, SpecialEventType.RefuseJoinEnterTomb);
								}
								else {
									SetBit(30);

									mMapName = mMapHeader.EnterMap;
									mXAxis = mMapHeader.EnterX - 1;
									mYAxis = mMapHeader.EnterY - 1;

									await RefreshGame();
								}
							}
							else {
								Dialog(" 당신이 바닥을 끌어 당기자 다시 스르르 밀려와서 원위치 되었다.");
							}
						}
						else if (menuMode == MenuMode.ForceEnterTomb) {
							if (mMenuFocusID == 0) {
								SetBit(30);

								if (mBackupPlayerList == null)
									mBackupPlayerList = new List<Lore>();
								else
									mBackupPlayerList.Clear();

								while (mPlayerList.Count > 1) {
									mBackupPlayerList.Add(mPlayerList[1]);
									mPlayerList.RemoveAt(1);
								}

								DisplayPlayerInfo();

								mMapName = mMapHeader.EnterMap;
								mXAxis = mMapHeader.EnterX - 1;
								mYAxis = mMapHeader.EnterY - 1;

								await RefreshGame();
							}
							else {
								Dialog(" 당신은 다시 물러서서 바닥을 원위치 시켰다.");
							}
						}
						else if (menuMode == MenuMode.BattleDraconian1 ||
							menuMode == MenuMode.BattleDraconian2 ||
							menuMode == MenuMode.BattleDraconian4 ||
							menuMode == MenuMode.BattleDraconian5 ||
							menuMode == MenuMode.BattleDraconian6 ||
							menuMode == MenuMode.BattleDraconian8 ||
							menuMode == MenuMode.BattleDraconian9 ||
							menuMode == MenuMode.BattleDraconian10 ||
							menuMode == MenuMode.BattleDraconian11 ||
							menuMode == MenuMode.BattleDraconian12 ||
							menuMode == MenuMode.BattleDraconian13) {
							if (mMenuFocusID == 0) {
								var battleEvent = BattleEvent.None;

								switch (menuMode) {
									case MenuMode.BattleDraconian1:
										battleEvent = BattleEvent.Draconian1;
										break;
									case MenuMode.BattleDraconian2:
										battleEvent = BattleEvent.Draconian2;
										break;
									case MenuMode.BattleDraconian4:
										battleEvent = BattleEvent.Draconian4;
										break;
									case MenuMode.BattleDraconian5:
										battleEvent = BattleEvent.Draconian5;
										break;
									case MenuMode.BattleDraconian6:
										battleEvent = BattleEvent.Draconian6;
										break;
									case MenuMode.BattleDraconian8:
										battleEvent = BattleEvent.Draconian8;
										break;
									case MenuMode.BattleDraconian9:
										battleEvent = BattleEvent.Draconian9;
										break;
									case MenuMode.BattleDraconian10:
										battleEvent = BattleEvent.Draconian10;
										break;
									case MenuMode.BattleDraconian11:
										battleEvent = BattleEvent.Draconian11;
										break;
									case MenuMode.BattleDraconian12:
										battleEvent = BattleEvent.Draconian12;
										break;
									case MenuMode.BattleDraconian13:
										battleEvent = BattleEvent.Draconian13;
										break;
								}

								BattleDraconian(battleEvent);
							}
						}
						else if (menuMode == MenuMode.JoinDraconian) {
							if (mMenuFocusID == 0)
							{
								JoinMemberFromEnemy(47);
								UpdateTileInfo(14, 56, 44);
							}
							else
								ShowNoThanks();
						}
						else if (menuMode == MenuMode.BattleDraconianEntrance) {
							if (mMenuFocusID == 0)
								BattleDraconianEntrance();
						}
						else if (menuMode == MenuMode.TeleportCastleLore) {
							if (mMenuFocusID == 0) {
								mXAxis = mMapHeader.EnterX;
								mYAxis = mMapHeader.EnterY;

								mMapName = mMapHeader.EnterMap;

								await RefreshGame();

								mParty.Etc[19] = 7;
							}
						}
						else if (menuMode == MenuMode.ChooseBetrayLordAhn) {
							if (mMenuFocusID == 0) {
								SetBit(33);
								mParty.Etc[24] = 2;

								Dialog(" 우리 성의 사람들은 당신의 용기 있는 결단에 찬사를 보내고 있소. 정말 장한 일이오.");

								AddNextTimeEvent(0, 10);

								mBackupPlayerList.Clear();
								while (mPlayerList.Count > 1) {
									mBackupPlayerList.Add(mPlayerList[1]);
									mPlayerList.RemoveAt(1);
								}

								DisplayPlayerInfo();

								InvokeAnimation(AnimationType.LeaveCaminus);
							}
							else {
								mAnimationEvent = AnimationType.None;
								mAnimationFrame = 0;

								mParty.Etc[24] = 1;

								for (var i = 0; i < 8; i++)
									JoinEnemy(63);

								DisplayEnemy();
								HideMap();

								mBattleEvent = BattleEvent.Rebellion1;
								StartBattle(false);
							}
						}
						else if (menuMode == MenuMode.JoinFriendAgain) {
							if (mMenuFocusID == 0)
							{
								foreach (var player in mBackupPlayerList)
								{
									mPlayerList.Add(player);
								}

								mBackupPlayerList.Clear();

								DisplayPlayerInfo();

								SetBit(49);
							}

							AddNextTimeEvent(4, 30);
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
					Rest();
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

			mPrevX = prevX;
			mPrevY = prevY;

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
			else if (mMapName == "Ground1")
			{
				if (mXAxis == 29 && mYAxis == 19 && !GetBit(51))
				{
					mEncounterEnemyList.Clear();

					var enemy = JoinEnemy(35);
					enemy.Name = "폴록스";

					for (var i = 0; i < 6; i++)
					{
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
				else if (mXAxis == 83 && mYAxis == 85)
				{
					if (mParty.Year <= 699)
						ShowEnterMenu("HdsGate");
					else
						triggered = false;
				}
				else
					triggered = false;
			}
			else if (mMapName == "LastDtch")
			{
				if (mYAxis == 69)
					ShowExitMenu();
				else
					triggered = false;
			}
			else if (mMapName == "Valiant")
			{
				if (mYAxis == 69)
					ShowExitMenu();
				else
					triggered = false;
			}
			else if (mMapName == "Gaea")
			{
				if (mXAxis == 17 && mYAxis == 26)
					FindGold(142, 5_000);
				else if (mXAxis == 15 && mYAxis == 22)
					FindGold(143, 10_000);
				else if (mXAxis == 14 && mYAxis == 24)
					FindGold(144, (mRand.Next(9) + 1) * 1_000);
				else if (mXAxis == 11 && mYAxis == 25)
					FindGold(145, (mRand.Next(9) + 1) * 1_000);
				else if (mXAxis == 9 && mYAxis == 23 && !GetBit(146))
				{
					if (mParty.Etc[21] < 3)
						FindGold(146, (mRand.Next(9) + 1) * 1_000);
					else
					{
						Dialog($"당신은 [color={RGB.LightCyan}]화염의 크리스탈[/color]을 발견했다.");
						mParty.Crystal[0]++;
						SetBit(146);
					}
				}
				else if (mXAxis == 22 && mYAxis == 24 && mParty.Etc[20] == 3)
				{
					mXAxis = 19;
					triggered = false;
				}
				else if (mXAxis == 20 && mYAxis == 24 && mParty.Etc[20] == 3)
				{
					mXAxis = 23;
					triggered = false;
				}
				else if (mYAxis == 9 && (!GetBit(3) || !GetBit(4)))
				{
					mYAxis++;

					Dialog($"[color={RGB.LightMagenta}]원인을 알 수 없는 힘이 당신을 거부했다.[/color]");
				}
				else if (mYAxis == 5)
				{
					if (!GetBit(3) || !GetBit(4))
					{
						mYAxis++;
						Dialog("게이트는 전혀 작동하지 않았다.");
					}
					else
					{
						InvokeAnimation(AnimationType.MoveGround4);
					}
				}
				else if (mYAxis == 44)
				{
					ShowExitMenu();
				}
				else
					triggered = false;
			}
			else if (mMapName == "Ground2")
			{
				if (mXAxis == 29 && mYAxis == 46 && !GetBit(51) && !GetBit(50))
				{
					Talk(" 당신은 길을 가던 중  매우 지친듯이 보이는 한 여행자를 만났다. 그는 내가 베스퍼성으로 간다는 말을 듣더니 이렇게 말했다.", SpecialEventType.MeetTraveler);
				}
				else if (mXAxis == 89 && mYAxis == 79)
				{
					if (GetBit(8) && !GetBit(7))
					{
						Dialog(" 거기에는  오크의 글로 적혀진  비석이 있었다.  그리고 당신은  그 글을 읽어 내려갔다." +
						" 거기에 적힌 주문을 외자 갑자기 당신의 머리가 혼란스러워졌고 곧 정신을 잃었다.");
						SetBit(7);
						InvokeAnimation(AnimationType.LearnOrcSpeaking);
					}
					else if (GetBit(7))
						Dialog(" 거기에는 오크의 글로 쓰여진 비석이 여전히 있었다.");
					else
						Dialog(" 거기에는 비석이 하나  서 있었지만  도저히 무슨 글인지 알아 볼 수가 없었다.");
				}
				else if (mXAxis == 43 && mYAxis == 6)
				{
					if (GetBit(3))
					{
						if (!GetBit(203) && mParty.Etc[19] < 4)
						{
							Talk(" 당신이 아프로디테 게이트 안으로 막들어 가려고할 때, 허공에서 낯익은 목소리가 당신을 불렀다. 그 목소리의 주인공은 바로 로드안이었다", SpecialEventType.LordAhnCall);
						}
						else
							await MoveGround3();
					}
				}
				else
					triggered = false;
			}
			else if (mMapName == "OrcTown")
			{
				if (mYAxis == 18 && !GetBit(55))
				{
					for (var i = 0; i < 3; i++)
						JoinEnemy(18);
					for (var i = 0; i < 4; i++)
						JoinEnemy(19);
					JoinEnemy(21);

					Dialog(" 당신 앞에는  어느새인가 매직유저들이 길을 막고 서 있었다.");

					if (GetBit(7))
					{
						Talk(new string[] {
							" 그 중에서 마법사 차림을 제대로 한 오크 하나가 당신에게 말을했다.",
							"",
							$"[color={RGB.LightMagenta}] 나는 오크킹님을 위해 죽고 사는 아키메이지라고 한다. 나를 쓰러뜨리기 전에는 오크킹님을 만날 수가 없다.[/color]"
						}, SpecialEventType.BattleOrcArchiMage, true);
					}
					else
					{
						mSpecialEvent = SpecialEventType.BattleOrcArchiMage;
					}
				}
				else if (23 <= mXAxis && mXAxis <= 26 && 15 <= mYAxis && mYAxis <= 17 && !GetBit(56))
				{
					for (var i = 0; i < 2; i++)
						JoinEnemy(20);
					JoinEnemy(22);
					for (var i = 0; i < 2; i++)
						JoinEnemy(19);
					JoinEnemy(23);

					Dialog(" 당신이 오크킹에게 접근하자  모든 전투사들이 오크킹 주위를 막아섰다.");

					if (GetBit(7))
					{
						Talk(new string[] {
							" 그러자 오크킹이 당신에게 말했다.",
							"",
							$"[color={RGB.LightMagenta}] 당신이  왜 나를  죽이려 하는지 모르겠지만 결과는 뻔한 일이다.[/color]",
							$"[color={RGB.LightMagenta}] 자! 한바탕 붙어보자!![/color]"
						}, SpecialEventType.BattleOrcKing, true);
					}
					else
					{
						mSpecialEvent = SpecialEventType.BattleOrcKing;
					}
				}
				else if (mYAxis == 44)
				{
					ShowExitMenu();
				}
				else
					triggered = false;
			}
			else if (mMapName == "Ground3")
			{
				if (mXAxis == 12 && mYAxis == 10)
				{
					mMapName = "Ground2";

					await RefreshGame();

					mXAxis = 43;
					mYAxis = 7;

					Dialog(" 당신이 이 곳에 서자  새로운 풍경이 나타나기 시작했다.");

					InvokeAnimation(AnimationType.MoveGround2);
				}
				else
					triggered = false;
			}
			else if (mMapName == "Vesper")
			{
				if (mXAxis == 4 || mXAxis == 70 || mYAxis == 5 || mYAxis == 69)
					ShowExitMenu();
				else
					triggered = false;
			}
			else if (mMapName == "TrolTown")
			{
				if ((mXAxis == 5 && mYAxis == 35) || (mXAxis == 44 && mYAxis == 28))
				{
					Dialog(" 당신이 아래에 있는 벽돌을 밟자  벽돌은 약간 아래로 꺼졌다.  그리고 몇 초후에 둔탁한 기계음이 들렸다.");
					for (var x = 24; x < 26; x++)
					{
						if ((mXAxis == 5 && mYAxis == 35))
						{
							for (var y = 23; y < 25; y++)
								UpdateTileInfo(x, y, 44);
						}
						else
						{
							for (var y = 21; y < 23; y++)
								UpdateTileInfo(x, y, 44);
						}
					}
				}
				else if (24 <= mXAxis && mXAxis <= 25 && 25 <= mYAxis && mYAxis <= 29)
				{
					if (mParty.Etc[3] == 0)
					{
						ShowGameOver(new string[] { "일행은 계곡으로 떨어져 버렸다." });
					}
				}
				else if (mXAxis == 12 && mYAxis == 18)
				{
					ShowGameOver(new string[] { " 당신이 이곳에 서자 갑자기 부비트랩이 작동하며 거대한 폭발이 일어났고, 곧이어 의식이 흐려졌다" });
				}
				else if (mXAxis == 11 && mYAxis == 16 && !GetBit(130))
				{
					Dialog($"당신은 [color={RGB.LightCyan}]에너지 크리스탈[/color]을 발견했다.");
					mParty.Crystal[6]++;
					SetBit(130);
				}
				else if (mXAxis == 13 && mYAxis == 16 && !GetBit(131))
				{
					Dialog($"당신은 [color={RGB.LightCyan}]한파의 크리스탈[/color]을 발견했다.");
					mParty.Crystal[1]++;
					SetBit(131);
				}
				else if (mXAxis == 13 && mYAxis == 16 && !GetBit(131))
				{
					FindGold(132, 15_000);
				}
				else if (mXAxis == 13 && mYAxis == 20 && !GetBit(133))
				{
					Dialog($"당신은 식량 200개를 발견했다.");
					if (mParty.Food + 200 < 256)
						mParty.Food += 200;
					SetBit(133);
				}
				else if (mYAxis == 69)
					ShowExitMenu();
				else
					triggered = false;
			}
			else if (mMapName == "Ground4")
			{
				if (mXAxis == 47 && mYAxis == 34)
				{
					InvokeAnimation(AnimationType.MoveGaeaTerraCastle);
				}
				else
					triggered = false;
			}
			else if (mMapName == "Hut")
			{
				if (mXAxis == 8 && mYAxis == 12)
				{
					if (!GetBit(204))
					{
						Dialog(new string[] {
							" 당신은 앞에 있는 창을 가졌다",
							$"[color={RGB.LightCyan}] [[ 랜서 + 1 ][/color]"
						});

						if (mParty.Backpack[2, 5] + 1 < 255)
							mParty.Backpack[2, 5]++;

						SetBit(204);
						UpdateTileInfo(7, 12, 35);
					}
					else
						triggered = false;
				}
				else if (mXAxis == 8 && mYAxis == 13)
				{
					if (!GetBit(205))
					{
						Dialog(new string[] {
							" 당신은 앞에 있는 창을 가졌다",
							$"[color={RGB.LightCyan}] [[ 라멜라 + 1 ][/color]"
						});

						if (mParty.Backpack[5, 5] + 1 < 255)
							mParty.Backpack[5, 5]++;

						SetBit(205);
						UpdateTileInfo(7, 13, 35);
					}
					else
						triggered = false;
				}
				else if (4 <= mXAxis && mXAxis <= 25 || 5 <= mYAxis && mYAxis <= 24)
				{
					ShowExitMenu();
				}
				else
					triggered = false;
			}
			else if (mMapName == "Kobold")
			{
				if (mXAxis == 4)
					ShowExitMenu();
				else if (mXAxis == 7 && mYAxis == 29 && GetTileInfo(8, 29) != 53)
				{
					UpdateTileInfo(8, 29, 53);
					Dialog(" 갑자기 땅에서 푯말이 솟아 나왔다.");
				}
				else if (29 <= mXAxis && mXAxis <= 54 && 20 <= mYAxis && mYAxis <= 35)
				{
					if (mXAxis == 48 && mYAxis == 30 && !GetBit(17))
					{
						for (var i = 0; i < 5; i++)
							JoinEnemy(35);
						for (var i = 0; i < 2; i++)
							JoinEnemy(38);

						DisplayEnemy();
						HideMap();

						mBattleEvent = BattleEvent.KoboldKnight;
						StartBattle(false);
					}
					else
					{
						for (var y = mYAxis - 1; y <= mYAxis + 1; y++)
						{
							for (var x = mXAxis - 1; x <= mXAxis + 1; x++)
								UpdateTileInfo(x, y, 50);
						}

						triggered = false;
					}
				}
				else if (mXAxis == 37 && mYAxis == 12)
				{
					UpdateTileInfo(98, 20, 43);

					Dialog(" 당신이 바닥의 기묘한 벽돌을 밟자 돌이 약간 밑으로 들어갔다.");
				}
				else if (mXAxis == 120 && mYAxis == 35)
				{
					UpdateTileInfo(109, 25, 43);

					Dialog(" 당신이 바닥의 기묘한 벽돌을 밟자 돌이 약간 밑으로 들어갔다.");
				}
				else if (mXAxis == 111 && mYAxis == 28)
				{
					UpdateTileInfo(95, 20, 43);

					Dialog(" 당신이 바닥의 기묘한 벽돌을 밟자 돌이 약간 밑으로 들어갔다.");
				}
				else if (mXAxis == 95 && mYAxis == 35)
				{
					UpdateTileInfo(106, 29, 43);

					Dialog(" 당신이 바닥의 기묘한 벽돌을 밟자 돌이 약간 밑으로 들어갔다.");
				}
				else if (mXAxis == 104 && mYAxis == 26)
				{
					UpdateTileInfo(117, 34, 43);

					Dialog(" 당신이 바닥의 기묘한 벽돌을 밟자 돌이 약간 밑으로 들어갔다.");
				}
				else if (mXAxis == 99 && mYAxis == 35 && !GetBit(18))
				{
					for (var i = 0; i < 7; i++)
						JoinEnemy(35);
					JoinEnemy(38);

					DisplayEnemy();
					HideMap();

					mBattleEvent = BattleEvent.GoldKey;
					StartBattle(false);
				}
				else if (mXAxis == 31 && mYAxis == 89 && !GetBit(19))
				{
					for (var i = 0; i < 7; i++)
						JoinEnemy(35);
					JoinEnemy(36);

					DisplayEnemy();
					HideMap();

					mBattleEvent = BattleEvent.KoboldMagicUser;
					StartBattle(false);
				}
				else if (95 <= mXAxis && mXAxis <= 118 && 84 <= mYAxis && mYAxis <= 97)
				{
					var xOffset = 0;
					var yOffset = 0;
					if (mRand.Next(2) == 0)
					{
						xOffset = mXAxis - prevX;
						yOffset = mYAxis - prevY;
					}
					else
					{
						while (xOffset == 0 && yOffset == 0)
						{
							xOffset = mRand.Next(3) - 1;
							yOffset = mRand.Next(3) - 1;
						}
					}

					var newX = mXAxis + xOffset;
					var newY = mYAxis + yOffset;

					if (GetTileInfo(newX, newY) == 43)
					{
						UpdateTileInfo(mXAxis, mYAxis, 43);
						UpdateTileInfo(newX, newY, 0);
					}
					else
					{
						newX = mXAxis - 2 * xOffset;
						newY = mYAxis - 2 * yOffset;

						UpdateTileInfo(mXAxis, mYAxis, 43);
						UpdateTileInfo(newX, newY, 0);
					}

					if (GetTileInfo(mXAxis + (mXAxis - prevX), mYAxis + (mYAxis - prevY)) == 37 || GetTileInfo(mXAxis + (mXAxis - prevX), mYAxis + (mYAxis - prevY)) == 38)
					{
						mXAxis = mXAxis + 2 * (mXAxis - prevX);
						mYAxis = mYAxis + 2 * (mYAxis - prevY);
					}
				}
				else if (mXAxis == 94 && mYAxis == 90 && !GetBit(20))
				{
					for (var i = 0; i < 6; i++)
						JoinEnemy(35);
					for (var i = 0; i < 2; i++)
						JoinEnemy(36);

					DisplayEnemy();
					HideMap();

					mBattleEvent = BattleEvent.SaphireKey;
					StartBattle(false);
				}
				else if (mXAxis == 36 && GetBit(18))
				{
					UpdateTileInfo(27, 59, 43);
					UpdateTileInfo(27, 60, 43);
					UpdateTileInfo(28, 59, 0);
					UpdateTileInfo(28, 60, 0);

					triggered = false;
				}
				else if (mXAxis == 123 && GetBit(19) && GetBit(89))
				{
					UpdateTileInfo(122, 59, 43);
					UpdateTileInfo(122, 60, 43);
					UpdateTileInfo(121, 59, 0);
					UpdateTileInfo(121, 60, 0);

					triggered = false;
				}
				else if (mXAxis == 61 && GetBit(20))
				{
					for (var y = 59; y < 61; y++)
					{
						for (var x = 62; x <= 64; x++)
							UpdateTileInfo(x, y, 43);
					}
					UpdateTileInfo(64, 59, 0);
					UpdateTileInfo(64, 60, 0);
					triggered = false;
				}
				else if (mXAxis == 26 && GetBit(17))
				{
					for (var y = 59; y < 61; y++)
					{
						for (var x = 86; x < 88; x++)
							UpdateTileInfo(x, y, 43);
					}
					UpdateTileInfo(85, 59, 0);
					UpdateTileInfo(85, 60, 0);

					triggered = false;
				}
				else if (mXAxis == 121 && !GetBit(92))
				{
					for (var i = 0; i < 6; i++)
						JoinEnemy(38);
					for (var i = 0; i < 2; i++)
						JoinEnemy(39);

					DisplayEnemy();
					HideMap();

					Talk($"[color={RGB.LightMagenta}] 정말 끈질긴 인간들이군. 번번히 패하면서 또 시비를 걸어 오다니...", SpecialEventType.BattleKoboldSoldier);
				}
				else if (mXAxis == 28 && !GetBit(91))
				{
					for (var i = 0; i < 4; i++)
						JoinEnemy(38);
					for (var i = 0; i < 3; i++)
						JoinEnemy(36);
					JoinEnemy(40);

					DisplayEnemy();
					HideMap();

					Talk($"[color={RGB.LightMagenta}] 또 인간들이 우리 성을 침범하러 왔군.  너희들은 이 주위의 해골들이 보이지 않는가?" +
					" 이것들은 우리에게 무모한 싸움을 걸었다가 죽어간 사람들의 해골이다.[/color]", SpecialEventType.BattleKoboldSoldier2);
				}
				else if (mXAxis == 85 && !GetBit(90))
				{
					for (var i = 0; i < 7; i++)
						JoinEnemy(36);
					JoinEnemy(37);

					DisplayEnemy();
					HideMap();

					Talk($"[color={RGB.LightMagenta}] 나는 이 성의 경비대장이다.  나의 임무는 바로 너희들을  이 안으로  들여 놓지 않는 것이다. 나의 목숨을 바쳐서라도...", SpecialEventType.BattleKoboldSecurity);
				}
				else if (mXAxis == 85 && GetBit(90))
				{
					for (var y = 59; y < 61; y++)
					{
						for (var x = 62; x < 65; x++)
							UpdateTileInfo(x, y, 43);
					}

					for (var y = 59; y < 61; y++)
					{
						for (var x = 27; x < 29; x++)
							UpdateTileInfo(x, y, 43);
					}

					triggered = false;
				}
				else if (mXAxis == 64 && !GetBit(89))
				{
					for (var i = 0; i < 3; i++)
						JoinEnemy(38);
					JoinEnemy(41);

					DisplayEnemy();
					HideMap();

					Talk($"[color={RGB.LightMagenta}] 용케도 우리 성의 내부까지 들어왔군. 당신들은 소환 마법이란걸 들어봤나? 지금 바로 내가 소환 마법의 진가를 보여주지.", SpecialEventType.BattleKoboldSummoner);
				}
				else if (mXAxis == 47 && mYAxis == 72 && GetTileInfo(45, 46) == 50)
				{
					for (var y = 41; y < 50; y++)
					{
						for (var x = 36; x < 54; x++)
						{
							if (GetTileInfo(x, y) == 50)
								UpdateTileInfo(x, y, 49);
						}
					}

					triggered = false;
				}
				else if (mXAxis == 44 && mYAxis == 45 && GetTileInfo(55, 62) == 50)
				{
					for (var y = 50; y < 74; y++)
					{
						for (var x = 55; x < 60; x++)
						{
							if (GetTileInfo(x, y) == 50)
								UpdateTileInfo(x, y, 48);
						}
					}

					triggered = false;
				}
				else if (41 <= mXAxis && mXAxis <= 52 && 54 <= mYAxis && mYAxis <= 65)
				{
					if (mParty.Etc[35] > 3)
						triggered = false;
					else
					{
						mParty.Etc[35]++;
						UpdateTileInfo(mXAxis, mYAxis, 43);

						if (mParty.Etc[35] == 4)
						{
							Dialog(" 당신은 땅에 반쯤 묻혀있는  오팔키를 발견했다.");
							SetBit(21);
						}
						else
						{
							// 현재 위치 제외 보정 필요
							var newX = 0;
							var newY = 0;

							do
							{
								newX = mRand.Next(12) + 41;
								newY = mRand.Next(12) + 54;
							} while (newX == mXAxis && newY == mYAxis);

							UpdateTileInfo(newX, newY, 0);
							triggered = true;
						}
					}
				}
				else if (90 <= mXAxis && mXAxis <= 120 && 40 <= mYAxis && mYAxis <= 79)
				{
					if (mXAxis == 90 && mYAxis == 40 && !GetBit(22))
					{
						Dialog(" 당신은 어둠속에서도  반짝이는 물체를  하나 보았다. 자세히보니 그것은 흑요석키였고 즉시 그것을 가졌다.");

						SetBit(22);
					}
					else if (mXAxis == 105 && mYAxis == 53 && !GetBit(200))
					{
						Dialog(" 당신은 벽에서 무언가 반짝이는 것이 있어 벽으로 다가갔다.  가까이에서 보니 그것은 수정이었다.  하지만 그 수정은 특이하게도 스스로 희미한 빛을 발하고 있었다." +
						"  당신이 수정속을 들여다 보았을때  소스라치게 놀랄 수 밖에 없었다.  그속에는 형체를 분간하기 힘들 정도의 많은 영혼들이 배회하고 있었다.  그들이 아마이 희미한 빛을 발하는 것 같았다." +
						"  당신이 약간의 힘을 주어 빼어 보니 쉽게 벽에서 떨어져 나왔다.");

						var friend = mPlayerList.Count > 1 ? mPlayerList[1] : null;

						if (friend != null)
						{
							Talk(new string[] {
							"",
							$" 그걸 보더니 {friend.NameSubjectBJosa} 말했다.",
							"",
							$"[color={RGB.LightBlue}] 아니, 이건....  이건 바로 그 전설 속에서나 논해지던[/color] [color={RGB.LightCyan}]영혼의 크리스탈[/color][color={RGB.LightBlue}]임에 틀림없네." +
							" 이것은 전세계에 두개 밖에 존재하지 않는 귀한 것인데 우리가 발견했군.[/color]"
						}, SpecialEventType.None);
						}

						SetBit(200);
						mParty.Crystal[4]++;
					}
					else if (((mXAxis == 90 && mYAxis == 58) || (mXAxis == 120 && mYAxis == 61)) && mEbony)
					{
						mEbony = false;
						triggered = false;
					}
					else if (mXAxis == 92 && mYAxis == 59)
					{
						mEbony = false;

						mXAxis = 90;
						mYAxis = 59;
					}
					else if (mXAxis == 118 && mYAxis == 60)
					{
						mXAxis = 120;
						mYAxis = 60;
					}
					else if (mXAxis == 102 && mYAxis == 74)
					{
						mEbony = false;

						mXAxis = 106;
						mYAxis = 59;
					}
					else if (mXAxis == 107 && mYAxis == 74)
					{
						mXAxis = 101;
						mYAxis = 48;
					}
					else if (mXAxis == 104 && mYAxis == 57 && GetTileInfo(104, 58) != 43)
					{
						UpdateTileInfo(104, 58, 43);
						triggered = false;
					}
					else if (!mEbony)
					{
						mEbony = true;
						triggered = false;
					}
					else
						triggered = false;
				}
				else if (mXAxis == 57 && mYAxis == 100 && GetTileInfo(57, 99) != 49)
				{
					for (var y = 85; y < 100; y++)
						UpdateTileInfo(57, y, 49);
					UpdateTileInfo(57, 84, 0);

					Dialog(" 당신이 무심코 벽에 팔을 기대자 벽은 안으로 쑥 들어가 버렸다. 그리고는 새로운 길이 나타났다.");
				}
				else if (mXAxis == 57 && mYAxis == 84)
				{
					UpdateTileInfo(57, 84, 43);
					UpdateTileInfo(75, 73, 52);

					Dialog(" 당신이 발 밑의 벽돌을 밟자  벽돌은  유압에 의해 서서히 가라앉기 시작했다.  당신은 즉시 벽돌에서 발을 떼고 한 걸음 물러섰다." +
					" 하지만 벽돌은 빨려 들어가듯이 밑으로 가라 앉았다.");

					var friend = mPlayerList.Count > 1 ? mPlayerList[mPlayerList.Count - 1] : null;

					if (friend != null)
					{
						Dialog(new string[] {
							"",
							$" 그때 옆에 있던 {friend.NameSubjectBJosa} 말했다.",
							"",
							$"[color={RGB.LightBlue}] 이런 장치는 전에 한번 본 적이 있다네. 벽돌이 가라 앉는 무게와 압력으로 다른 곳의 물체를 떠올리는 장치라네." +
							" 그런데 어디가 다시 떠올랐는지 모르겠군.[/color]"
						}, true);
					}
				}
				else if (mXAxis == 75 && mYAxis == 73)
				{
					UpdateTileInfo(64, 24, 35);
					UpdateTileInfo(63, 24, 0);
					UpdateTileInfo(75, 73, 48);

					Dialog(" 당신이 물 위로 떠오른 땅을 밟자  다시 가라앉아 버렸다.");
				}
				else if (mXAxis == 63 && mYAxis == 24 && GetTileInfo(107, 52) != 0)
				{
					for (var y = 52; y < 54; y++)
					{
						for (var x = 107; x < 109; x++)
							UpdateTileInfo(x, y, 0);
					}
					UpdateTileInfo(63, 24, 43);

					Dialog(" 성의 남동쪽에서  돌연히 커다란 진동음과 함께 기계 소리가 들렸다.");
				}
				else if (mXAxis == 74 && mYAxis == 59 && GetBit(21) && GetTileInfo(74, 60) != 43)
				{
					UpdateTileInfo(74, 60, 43);
					triggered = false;
				}
				else if (mXAxis == 75 && mYAxis == 60 && GetBit(22) && GetTileInfo(76, 60) != 43)
				{
					UpdateTileInfo(76, 60, 43);
					triggered = false;
				}
				else if (mXAxis == 76 && mYAxis == 58 && !GetBit(88))
				{
					for (var i = 0; i < 3; i++)
						JoinEnemy(38);

					for (var i = 0; i < 2; i++)
						JoinEnemy(39);

					for (var i = 0; i < 2; i++)
						JoinEnemy(40);

					DisplayEnemy();
					HideMap();

					Talk($"[color={RGB.LightMagenta}] 신성한 신의 제단에 더러운 발을 디딘 놈들은 누구냐?  신을 모독한 너희네 인간들에게 몸소 가르침을 주겠다. 밧아랏!![/color]"
					, SpecialEventType.BattleKoboldAlter);
				}
				else if (mYAxis == 38 && GetBit(88) && ((mParty.Etc[29] >> 4) == (mXAxis - 62) / 4))
				{
					if (GetTileInfo(mXAxis, mYAxis - 1) != 43)
					{
						for (var y = mYAxis - 2; y <= mYAxis - 1; y++)
						{
							for (var x = mXAxis - 1; x <= mXAxis; x++)
								UpdateTileInfo(x, y, 43);
						}
					}

					triggered = false;
				}
				else if (mYAxis == 81 && GetBit(88) && ((mParty.Etc[29] & 0x0F) == (mXAxis - 63) / 4))
				{
					if (GetTileInfo(mXAxis, mYAxis + 1) != 43)
					{
						for (var y = mYAxis + 1; y <= mYAxis + 2; y++)
						{
							for (var x = mXAxis; x <= mXAxis + 1; x++)
								UpdateTileInfo(x, y, 43);
						}
					}

					triggered = false;
				}
				else if (mXAxis == 67 && mYAxis == 24)
				{
					if (GetBit(147))
						triggered = false;
					else if ((mParty.Etc[30] & 1) > 0)
					{
						if (GetBit(24))
						{
							if (GetBit(134))
								triggered = false;
							else
							{
								BattleTreasureBox(SpecialEventType.BattleTreasureBox1);
							}
						}
						else if (GetBit(11))
						{
							AskTreasureBoxQuestion(SpecialEventType.AskTreasureboxQuestion1);
						}
						else
						{
							Dialog(" 당신이 보물상자 앞으로 다가섰을때  상자 위의 두개골이 무어라 말을 했지만 전혀 알아 들을 수가 없었다.");
						}
					}
					else
					{
						Dialog(" 당신이 보물상자 앞으로 다가섰지만  상자 위의 두개골은 아무말도 하지 않았다.");
					}
				}
				else if (mXAxis == 71 && mYAxis == 24)
				{
					if (GetBit(148))
						triggered = false;
					else if ((mParty.Etc[30] & (1 << 1)) > 0)
					{
						if (GetBit(25))
						{
							if (GetBit(135))
								triggered = false;
							else
								BattleTreasureBox(SpecialEventType.BattleTreasureBox2);
						}
						else if (GetBit(11))
						{
							AskTreasureBoxQuestion(SpecialEventType.AskTreasureboxQuestion2);
						}
						else
						{
							Dialog(" 당신이 보물상자 앞으로 다가섰을때  상자 위의 두개골이 무어라 말을 했지만 전혀 알아 들을 수가 없었다.");
						}
					}
					else
					{
						Dialog(" 당신이 보물상자 앞으로 다가섰지만  상자 위의 두개골은 아무말도 하지 않았다.");
					}
				}
				else if (mXAxis == 75 && mYAxis == 24)
				{
					if (GetBit(149))
						triggered = false;
					else if ((mParty.Etc[30] & (1 << 2)) > 0)
					{
						if (GetBit(26))
						{
							if (GetBit(136))
								triggered = false;
							else
								BattleTreasureBox(SpecialEventType.BattleTreasureBox3);
						}
						else if (GetBit(11))
						{
							AskTreasureBoxQuestion(SpecialEventType.AskTreasureboxQuestion3);
						}
						else
						{
							Dialog(" 당신이 보물상자 앞으로 다가섰을때  상자 위의 두개골이 무어라 말을 했지만 전혀 알아 들을 수가 없었다.");
						}
					}
					else
					{
						Dialog(" 당신이 보물상자 앞으로 다가섰지만  상자 위의 두개골은 아무말도 하지 않았다.");
					}
				}
				else if (mXAxis == 79 && mYAxis == 24)
				{
					if (GetBit(150))
						triggered = false;
					else if ((mParty.Etc[30] & (1 << 3)) > 0)
					{
						if (GetBit(27))
						{
							if (GetBit(137))
								triggered = false;
							else
								BattleTreasureBox(SpecialEventType.BattleTreasureBox4);
						}
						else if (GetBit(11))
						{
							AskTreasureBoxQuestion(SpecialEventType.AskTreasureboxQuestion4);
						}
						else
						{
							Dialog(" 당신이 보물상자 앞으로 다가섰을때  상자 위의 두개골이 무어라 말을 했지만 전혀 알아 들을 수가 없었다.");
						}
					}
					else
					{
						Dialog(" 당신이 보물상자 앞으로 다가섰지만  상자 위의 두개골은 아무말도 하지 않았다.");
					}
				}
				else if (mXAxis == 83 && mYAxis == 24)
				{
					if (!GetBit(23))
					{
						Talk(new string[] {
							" 당신이 앞에 있는 보물상자를 보았을때  섬찟함을 느끼며 한 걸음 물러섰다.  그 상자 위에는 인간의 해골이 앉아 있었고 그 해골에는 두 눈동자와 머리카락이 듬성듬성 붙어 있었다.",
							" 스르르 해골이 고개를 들며 당신을 노려 보았다. 당신은 다시 한 걸음을 물러섰다.  그러자 그 해골은 뼈뿐인 두 손을 들어 당신을 가리켰다. 그리고 인간의 말을 하기 시작했다.",
							"",
							$"[color={RGB.LightBlue}] 당신은 당신들이 지금하고 있는 일이 어떤 것인지 모르고 있다." +
							"  지금 이 시대가  정의라고 생각하는 것을 역사가 반드시 정의라고 평가짓지는 않을 것이다. 당신이 만약 에인션트 이블을 만나지 않았다면 당장 그를 만나보아라." +
							" 그를 통해 당신은 진정한 정의를 체험하게 될 것이다. 게다가 너는 ...[/color]",
							"",
							" 그가  이런 말을 하고 있을때  갑자기 천정을 뚫고 직격뇌진이 그를 강타했다." +
							"  당신은 순간적으로 벌어진 이 일을 보고  넋나간듯 얼마동안 있다가 산산히 흩어진 뼈를 보며 방금의 말을 되새겼다."
						}, SpecialEventType.OpenTreasureBox);

						SetBit(23);
					}
					else
					{
						if ((mParty.Etc[30] & (1 << 4)) > 0)
						{
							if (GetBit(151))
								triggered = false;
							else
								OpenTreasureBox();
						}
						else
							triggered = false;
					}
				}
				else if (((mXAxis == 75 && mYAxis == 89) || (mXAxis == 76 && mYAxis == 89)) && !GetBit(87))
				{
					mEncounterEnemyList.Clear();

					for (var i = 0; i < 2; i++)
						JoinEnemy(40);
					JoinEnemy(39);
					JoinEnemy(41);
					JoinEnemy(43);

					DisplayEnemy();
					HideMap();

					Talk($"[color={RGB.LightMagenta}] 나는 코볼트킹님의 근위대의 대장이다.  내가 살아 있는한  너희들을 결코 이 안으로 한발자국도 들여놓지 못할 것이다.[/color]", SpecialEventType.BattleKoboldGuardian);
				}
				else if (((mXAxis == 75 && mYAxis == 85) || (mXAxis == 76 && mYAxis == 85)) && !GetBit(86))
				{
					mEncounterEnemyList.Clear();

					JoinEnemy(44);

					DisplayEnemy();
					HideMap();

					Talk($"[color={RGB.LightMagenta}] 지상 최상 최대의 마도사인 나, 코볼트킹에게 도전해 오다니 전말 배짱 한번 좋구나. 네놈들은 나 혼자서 상대하겠다. 가소로운 것들...[/color]", SpecialEventType.BattleKoboldKing);
				}
				else
					triggered = false;
			}
			else if (mMapName == "Ancient")
			{
				if ((mXAxis == 9 && mYAxis == 12) || (mXAxis == 10 && mYAxis == 12))
				{
					Dialog(" 당신은  무덤 앞으로 발을  내 디디려 했지만 어떤 힘에 의해 더 이상 들어 갈 수가 없었다.");
					mYAxis++;
				}
				else if ((mXAxis == 9 && mYAxis == 11) || (mXAxis == 10 && mYAxis == 11))
				{
					Dialog(new string[] {
						"",
						"",
						$"[color={RGB.White}]         Rest In Peace ----------[/color]",
						"",
						$"[color={RGB.LightGreen}]              에인션트 이블[/color]",
						"",
						$"[color={RGB.White}]        348 년 가을, 여기에 잠들다[/color]"
					});

					if (!GetBit(206))
					{
						mSpecialEvent = SpecialEventType.PlusExperience;
						ContinueText.Visibility = Visibility.Visible;
					}
				}
				else if (4 <= mXAxis && mXAxis <= 15 && 5 <= mYAxis && mYAxis <= 24)
				{
					ShowExitMenu();
				}
				else
					triggered = false;
			}
			else if (mMapName == "Ground5")
			{
				Dialog(new string[] {
					" 당신은 잠을 자고있는 어떤 은둔자를 보았다. 그는 당신의 기척에 잠을 깨었는지  눈을 뜨고 당신을 바라 보았다. 그리고는 말을 시작했다.",
					""
				});

				if (GetBit(14))
				{
					if (GetBit(13))
						Dialog($"[color={RGB.LightBlue}] 이제 당신은 드라코니안의 말을 들을 수 있을 것입니다.[/color]", true);
					else
					{
						Dialog($"[color={RGB.LightBlue}] 나에게는 당신이  뭘 원하는지 훤히 보이는군요. 당신은 지금 드라코니안의 말을 배우려 하고있죠?[/color]", true);

						if (mParty.Etc[4] > 0)
						{
							Dialog($"[color={RGB.LightBlue}] 그럼 가르쳐 드리죠.  이건 제가 발명한 자동통역기인데 귀속에다 넣을면  드라코니안의 말을 들을 수 있을 것입니다.[/color]", true);
							SetBit(13);
						}
						else
							Dialog($"[color={RGB.LightBlue}] 하지만 귀찮아서 가르쳐 주고 싶지 않군요.[/color]", true);
					}
				}
				else
					Dialog(" 혹시 당신 지금 드라코니안의 글을 배우려 하지 않습니까? 그렇다면 이 대륙 전체를 뒤져서 금빛의 인어를 찾아 보십시오.", true);
			}
			else if (mMapName == "DracTown")
			{
				if (mXAxis == 4)
				{
					ShowExitMenu();
				}
				else if (mXAxis == 64 && mYAxis == 100 && !GetBit(28))
				{
					Talk(" 갑자기 호수 밑에서 누군가가 당신의 발을 잡고 호수 밑으로 끌어 내리려 했다.  하지만 당신의 물 위를 걷는 마법때문에 당신은 전혀 끌려 들어가지 않았다." +
					" 도리어 당신이 정체 불명의 손을 끌어 올려 물 밖으로 들어 올렸다. 놀랍게도 당신이 물 밖으로 끌어 올린 것은 금빛의 인어였다. 그녀는 겁에 질려 당신에게 애원했다.", SpecialEventType.CaptureMermaid);
				}
				else if (mXAxis == 75 && mYAxis == 72 && !GetBit(104))
				{
					SetBit(104);

					Dialog($"[color={RGB.LightCyan}] [[ 화살 + 400 ][/color]");

					if (mParty.Arrow + 400 < 65_535)
						mParty.Arrow += 400;
					else
						mParty.Arrow = 65_535;

					UpdateTileInfo(76, 72, 44);
				}
				else if (mXAxis == 67 && mYAxis == 65 && !GetBit(105))
				{
					Talk(" 당신이 보물상자를 열자 갑자기 흡혈귀가 튀어나왔다.", SpecialEventType.BattleVampire);
				}
				else if (mXAxis == 67 && mYAxis == 72 && !GetBit(106))
				{
					SetBit(106);

					Dialog($"[color={RGB.LightCyan}] [[ 식량 + 255 ][/color]");

					mParty.Food = 255;
					UpdateTileInfo(66, 72, 44);
				}
				else if (mXAxis == 62 && mYAxis == 70 && !GetBit(107))
				{
					SetBit(107);

					Dialog($"[color={RGB.LightCyan}] [[ 황금 + 200,000 ][/color]");

					mParty.Gold += 200_000;
					UpdateTileInfo(61, 70, 44);
				}
				else if (mXAxis == 67 && mYAxis == 87 && !GetBit(108))
				{
					SetBit(108);

					Dialog($"[color={RGB.LightCyan}] [[ 소환의 크리스탈 + 1 ][/color]");

					mParty.Crystal[5]++;
					UpdateTileInfo(66, 87, 44);
				}
				else if (mXAxis == 62 && mYAxis == 78 && !GetBit(109))
				{
					Talk(" 당신이  보물상자를 열자  갑자기 드라큐라가 튀어나왔다.", SpecialEventType.BattleDracula);
				}
				else if (mXAxis == 63 && mYAxis == 63 && !GetBit(110))
				{
					SetBit(110);

					Dialog($"[color={RGB.LightCyan}] [[ 한파의 크리스탈 + 1 ][/color]");

					mParty.Crystal[1]++;
					UpdateTileInfo(64, 63, 44);
				}
				else if (mXAxis == 96 && mYAxis == 99)
				{
					Dialog($" 묘비에는 [color={RGB.White}]제 21대 드라콘 제왕의 묘[/color]라고 쓰여있었다.");
				}
				else if (mXAxis == 88 && mYAxis == 102)
				{
					var message = $" 묘비에는 [color={RGB.White}]제 17대 드라콘 제왕의 지하 묘[/color]라고 쓰여 있었고  바로 밑에는 기묘하게 생긴 바닥이 있었다.";
					if (GetBit(30))
						Dialog(message);
					else
						Talk(message, SpecialEventType.OpenTomb);

				}
				else
					triggered = false;
			}
			else if (mMapName == "Tomb")
			{
				if (mXAxis == 82 && mYAxis == 9)
					UpdateTileInfo(38, 30, 44);
				else if (mXAxis == 18 && mYAxis == 34 && !GetBit(124))
				{
					SetBit(124);
					Dialog($"[color={RGB.LightCyan}] [[ 플래티움 갑옷 + 1 ][/color]");
					if (mParty.Backpack[5, 9] < 255)
						mParty.Backpack[5, 9]++;
				}
				else if (mXAxis == 83 && mYAxis == 14 && !GetBit(125))
				{
					SetBit(125);
					Dialog($"[color={RGB.LightCyan}] [[ 황금 + 100,000 ][/color]");
					mParty.Gold += 100_000;
				}
				else if (mXAxis == 86 && mYAxis == 14 && !GetBit(126))
				{
					SetBit(126);
					Dialog($"[color={RGB.LightCyan}] [[ 황금 + 50,000 ][/color]");
					mParty.Gold += 50_000;
				}
				else if (mXAxis == 83 && mYAxis == 25 && !GetBit(127))
				{
					Talk(" 당신이  보물상자를 열자  갑자기 저승사자가 튀어나왔다.", SpecialEventType.BattleMessengerOfDeath);
				}
				else if (mXAxis == 86 && mYAxis == 25 && !GetBit(128))
				{
					Talk(" 당신이  보물상자를 열자  갑자기 켈베로스가 튀어나왔다.", SpecialEventType.BattleKerberos);
				}
				else if (mXAxis == 84 && mYAxis == 20 && !GetBit(129))
				{
					Talk(" 당신이 괸 앞에 섰을때 무언가 묘한 분위기가 당신의 전신을 스치고 지나갔다.  잠시후 관의 뚜껑이 스르르 열리기 시작했다." +
					"  당신은 무의식적으로 한 걸음 뒤로 물러났다. 곧이어 관속에서는  에테르화된 드라코니안의 형체가 일어났다. 그리고 말을 하기 시작했다.",
					SpecialEventType.BattleDraconianOldKing);
				}
			}
			else if (mMapName == "Imperium")
			{
				if (mYAxis == 44)
				{
					ShowExitMenu();
				}
				else if (mXAxis == 27 && mYAxis == 16)
				{
					Dialog($"[color={RGB.Yellow}] You  ask  Young-Kie  how to play this game easilly." +
					" Young-Kie's smile makes you uneasy, as uneasy as  staring down the sources of game programs. \" It's really very simple, \" he  laughs." +
					"\" This game's  secret  code is ascii #197. \"[/color]");
				}
				else if (mXAxis == 24 && mYAxis == 23)
				{
					if (!GetBit(31))
					{
						Talk(new string[] {
							$" 드디어 {mPlayerList[0].Name}님이 여기에 당도 하셨군요.",
							$" 저의 소개를 하자면 바로 그 에인션트 이블입니다. 저의 육체는 벌써 오래전에 사라지고 없습니다. 저는 로드안과는 달리 그저 평범한 인간이었으니까요." +
							"  지금은  나의 의지만이 남아 반신반인의 경지에서  이렇게 이곳에 존재하고 있습니다. 그럼 본론으로 들어가겠습니다.",
							" 저는 예전에 로드안에게, 선을 위해서라면 나의 이름을 이용하여  악을 비난해도 좋다고 약속했었습니다." +
							"  그래서 로어 세계에서  어떠한 안좋은 일이 생기더라도  저의 소행이었노라고 치부해버리며  로드안은 자신의 지위를 유지할 수 있었습니다." +
							"  하지만 로드안은 해가 갈수록 자신의 위치를 지키기 위해서  저의 이름을 이용한다는 느낌이 와닿기 시작했습니다." +
							" 원래의 그는 전혀 그럴 사람이 아니었는데  너무나 큰 명성과 지위에 올라있다보니  로어성의 성주라는 직책이 그를 자신만을 생각하는 현실주의자로 바꾸어 버린 것입니다." +
							"  선의 대표자로서의 그는 항상 주위를 환기 시킬만한  선행을 찾기 시작했고  결국은 남에게 보이기 위한 선을 행하기 시작했습니다." +
							"  그리고  급기야 이번같은 큰 실수를  저지르고야 말았습니다.  로드안은 인간을 너무 생각하는 마음에서 인간의 영역을 늘려주기 원했고  그 결과는 타종족에 대한 침범으로 이어졌습니다." +
							"  이건 분명 로드안의 잘못된 행동이며 거의 돌이킬 수 없는 결과를 낳고야 말았습니다.  지금의 그는 권력과 명예의 노예가 되어  광적으로 이 일을 강행하고 있습니다." +
							" 그리고 이 일이 다 끝나더라도 더 큰 명예를 얻기위해 선행을 빙자한 다른 일을 또 꾸밀 것입니다.",
							" 만약 저의 말이 맞다는게 확인되면 부디 저의 편에 서서 저를 도와주십시요. 저는 당신과 같은 분의 도움이 크게 필요합니다."
						}, SpecialEventType.TeleportCastleLore);
						SetBit(31);
					}
					else
						TeleportCastleLore();
				}
				else
					triggered = false;
			}
			else if (mMapName == "HdsGate")
			{
				if (mXAxis == 24 && mYAxis == 29)
				{
					int tileA, tileB;

					if (GetTileInfo(19, 26) == 40)
					{
						tileA = 40;
						tileB = 41;

						Dialog(" 당신은 레버를 왼쪽으로 당겼다.");
					}
					else
					{
						tileA = 41;
						tileB = 40;

						Dialog(" 당신은 레버를 오른쪽으로 당겼다.");
					}

					UpdateTileInfo(19, 26, tileB);
					UpdateTileInfo(21, 27, tileB);
					UpdateTileInfo(20, 28, tileA);
					UpdateTileInfo(22, 30, tileA);
					UpdateTileInfo(14, 31, tileA);
					UpdateTileInfo(15, 32, tileB);
					UpdateTileInfo(118, 33, tileB);
					UpdateTileInfo(16, 34, tileA);
					UpdateTileInfo(23, 32, tileB);
					UpdateTileInfo(27, 32, tileA);
					UpdateTileInfo(24, 34, tileB);
					UpdateTileInfo(26, 34, tileA);
					UpdateTileInfo(21, 35, tileA);
					UpdateTileInfo(22, 38, tileA);
				}
				else if (mXAxis == 22 && mYAxis == 14)
				{
					int tileA, tileB;

					if (GetTileInfo(19, 28) == 40)
					{
						tileA = 40;
						tileB = 41;

						Dialog(" 당신은 레버를 왼쪽으로 당겼다.");
					}
					else
					{
						tileA = 41;
						tileB = 40;

						Dialog(" 당신은 레버를 오른쪽으로 당겼다.");
					}

					UpdateTileInfo(22, 26, tileA);
					UpdateTileInfo(19, 28, tileB);
					UpdateTileInfo(21, 28, tileB);
					UpdateTileInfo(20, 30, tileA);
					UpdateTileInfo(15, 31, tileA);
					UpdateTileInfo(17, 32, tileA);
					UpdateTileInfo(15, 33, tileB);
					UpdateTileInfo(18, 34, tileA);
					UpdateTileInfo(24, 31, tileB);
					UpdateTileInfo(26, 32, tileB);
					UpdateTileInfo(23, 33, tileA);
					UpdateTileInfo(25, 33, tileA);
					UpdateTileInfo(21, 36, tileA);
					UpdateTileInfo(19, 37, tileA);
				}
				else if (mXAxis == 22 && mYAxis == 18)
				{
					int tileA, tileB;

					if (GetTileInfo(19, 28) == 40)
					{
						tileA = 40;
						tileB = 41;

						Dialog(" 당신은 레버를 왼쪽으로 당겼다.");
					}
					else
					{
						tileA = 41;
						tileB = 40;

						Dialog(" 당신은 레버를 오른쪽으로 당겼다.");
					}

					UpdateTileInfo(22, 26, tileA);
					UpdateTileInfo(19, 28, tileB);
					UpdateTileInfo(21, 28, tileB);
					UpdateTileInfo(20, 30, tileA);
					UpdateTileInfo(15, 31, tileA);
					UpdateTileInfo(17, 32, tileA);
					UpdateTileInfo(15, 33, tileB);
					UpdateTileInfo(18, 34, tileA);
					UpdateTileInfo(24, 31, tileB);
					UpdateTileInfo(26, 32, tileB);
					UpdateTileInfo(23, 33, tileA);
					UpdateTileInfo(25, 33, tileA);
					UpdateTileInfo(21, 36, tileA);
					UpdateTileInfo(19, 37, tileA);
				}
				else if (mXAxis == 26 && mYAxis == 14)
				{
					int tileA, tileB;

					if (GetTileInfo(20, 27) == 40)
					{
						tileA = 40;
						tileB = 41;

						Dialog(" 당신은 레버를 왼쪽으로 당겼다.");
					}
					else
					{
						tileA = 41;
						tileB = 40;

						Dialog(" 당신은 레버를 오른쪽으로 당겼다.");
					}

					UpdateTileInfo(21, 26, tileA);
					UpdateTileInfo(20, 27, tileB);
					UpdateTileInfo(22, 28, tileA);
					UpdateTileInfo(19, 30, tileB);
					UpdateTileInfo(17, 31, tileA);
					UpdateTileInfo(18, 32, tileA);
					UpdateTileInfo(16, 33, tileB);
					UpdateTileInfo(14, 34, tileA);
					UpdateTileInfo(23, 31, tileB);
					UpdateTileInfo(26, 31, tileB);
					UpdateTileInfo(25, 32, tileA);
					UpdateTileInfo(23, 34, tileB);
					UpdateTileInfo(27, 34, tileA);
					UpdateTileInfo(22, 36, tileA);
					UpdateTileInfo(20, 39, tileA);
				}
				else if (mXAxis == 26 && mYAxis == 18)
				{
					int tileA, tileB;

					if (GetTileInfo(19, 27) == 40)
					{
						tileA = 40;
						tileB = 41;

						Dialog(" 당신은 레버를 왼쪽으로 당겼다.");
					}
					else
					{
						tileA = 41;
						tileB = 40;

						Dialog(" 당신은 레버를 오른쪽으로 당겼다.");
					}

					UpdateTileInfo(19, 27, tileB);
					UpdateTileInfo(20, 29, tileB);
					UpdateTileInfo(22, 29, tileB);
					UpdateTileInfo(21, 30, tileA);
					UpdateTileInfo(16, 31, tileA);
					UpdateTileInfo(14, 32, tileA);
					UpdateTileInfo(14, 33, tileB);
					UpdateTileInfo(17, 34, tileB);
					UpdateTileInfo(24, 30, tileA);
					UpdateTileInfo(27, 33, tileA);
					UpdateTileInfo(25, 34, tileB);
					UpdateTileInfo(20, 37, tileB);
					UpdateTileInfo(21, 38, tileB);
				}
				else if (mMapHeader.Layer[mXAxis + mYAxis * mMapHeader.Width] == GetTileInfo(mXAxis, mYAxis))
				{
					ShowGameOver(new string[] { $"[color={RGB.LightRed}] 당신이 물 속으로 발을 디디자  순식간에 온몸이 타버렸다.[/color]" });

					foreach (var player in mPlayerList)
					{
						player.HP = 0;
						player.Dead = 1;
					}

					if (mAssistPlayer != null)
					{
						mAssistPlayer.HP = 0;
						mAssistPlayer.Dead = 1;
					}

					DisplayPlayerInfo();
				}
				else
					triggered = false;
			}
			else if (mMapName == "Dome")
			{
				if ((4 <= mXAxis && mXAxis <= 45) || (5 <= mYAxis && mYAxis <= 94))
					ShowExitMenu();
			}
			else if (mMapName == "Light")
			{
				if (mYAxis == 69)
					ShowExitMenu();
				else if (mYAxis == 42 && !GetBit(33))
				{
					InvokeAnimation(AnimationType.MeetCaminus);
				}
				else
					triggered = false;
			}
			else if (mMapName == "UnderGrd")
				triggered = false;
			
	
			return triggered;
		}

		private string[] ShuffleOptions(string[] options) {
			for (var i = 0; i < options.Length; i++) {
				var shuffleIdx = mRand.Next(options.Length);
				var temp = options[i];
				options[i] = options[shuffleIdx];
				options[shuffleIdx] = temp;
			}

			return options;
		}

		private void TeleportCastleLore() {
			Ask(" 지금 당신을 로어성 앞으로 공간이동 시켜 드리겠습니다.  거기서 저와 로드안의 진위를 확인해 주시기 바랍니다.", MenuMode.TeleportCastleLore, new string[] {
				"공간이동을 한다",
				"공간이동을 하지않는다"
			});
		}

		private void BattleTreasureBox(SpecialEventType specialEvent) {
			mEncounterEnemyList.Clear();

			JoinEnemy(45);

			Talk(" 갑자기 보물상자의 양 옆에서 팔이 나오고 밑에서 다리가 나오더니  괴물로 변해 당신을 공격하기 시작했다." +
			"  당신은  그것이 보물상자의 모습을 하고 있는  미미크라는 것을 금방 알아차렸다.", specialEvent);
		}

		private void AskTreasureBoxQuestion(SpecialEventType specialEvent) {
			Talk(" 당신이 보물상자 앞으로 다가섰을때  상자 위의 두개골이 당신에게 질문을 던졌다.", specialEvent);
		}

		private void OpenTreasureBox() {
			Dialog(new string[] {
				" 당신은 해골이 걸터 앉아 있던 보물상자를 열었다.",
				"",
				$"[color={RGB.LightCyan}] [[ 크로매틱 방패 + 1 ][/color]",
				$"[color={RGB.LightCyan}] [[ 브리간디 + 1 ][/color]",
				$"[color={RGB.LightCyan}] [[ 철퇴 + 1 ][/color]"
			});

			if (mParty.Backpack[1, 4] < 255)
				mParty.Backpack[1, 4]++;

			if (mParty.Backpack[4, 3] < 255)
				mParty.Backpack[4, 3]++;

			if (mParty.Backpack[5, 4] < 255)
				mParty.Backpack[5, 4]++;

			SetBit(151);
		}

		private async Task MoveGround3() {
			mMapName = "Ground3";

			await RefreshGame();

			mXAxis = 12;
			mYAxis = 11;

			Dialog(" 당신이 이 곳에 서자  새로운 풍경이 나타나기 시작했다.");

			InvokeAnimation(AnimationType.MoveGround3);
		}

		private void ShowEnterMenu(string enterMap)
		{
			mTryEnterMap = enterMap;

			AppendText(new string[] { $"{mEnterTypeMap[enterMap]}에 들어가기를 원합니까?" });

			ShowMenu(MenuMode.AskEnter, new string[] {
				"예, 그렇습니다.",
				"아니오, 원하지 않습니다."
			});
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
							" 지금은 비록 절도 죄로 잡혀 있는 몸이지만  원래 내 아들의 꿈은 로어성 최고의 전사가 되는 것이었습니다.  아마 짐은 되지 않을 것입니다. 제발 부탁합니다."
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
						Dialog(" 내가 준 크리스탈 볼을 이용하여  네 종족의 원혼을 모두 그 안에 봉인시켜서 나에게 가져오시오. 꼭 부탁하오.");
					}
					else if (mParty.Etc[19] == 11) {
						mXAxis = 50;
						mYAxis = 30;

						mFace = 1;
						if (mPlayerList[0].ClassType == ClassCategory.Magic)
							mFace += 8;

						InvokeAnimation(AnimationType.PassCrystal);
					}
				}
			}
			else if (mMapName == "LastDtch") {
				if ((moveX == 56 && moveY == 16) || (moveX == 53 && moveY == 19) || (moveX == 57 && moveY == 21))
					ShowGroceryMenu();
				else if ((moveX == 58 && moveY == 55) || (moveX == 58 && moveY == 57) || (moveX == 58 && moveY == 59))
					ShowWeaponShopMenu();
				else if ((moveX == 16 && moveY == 59) || (moveX == 16 && moveY == 55))
					ShowHospitalMenu();
				else if (moveX == 16 && moveY == 57)
					ShowItemStoreMenu();
				else if ((moveX == 15 && moveY == 23) || (moveX == 23 && moveY == 18))
					ShowClassTrainingMenu();
				else if (moveX == 17 && moveY == 18)
					ShowExpStoreMenu();
				else if (moveX == 20 && moveY == 60)
				{
					if (GetBit(4)) {
						Dialog(" 베스퍼성에 대한 복수를 해주셔서 대단히 감사합니다.");
					}
					else if (mParty.Etc[4] > 0 && !GetBit(10)) {
						Ask(" 원하신다면 제가 트롤의 글을 가르쳐 드리죠. 식량 50 개만 주시면 일주일안에 모두 가르쳐 드리겠습니다.", MenuMode.LearnTrollWriting, new string[] {
							"그에게 트롤의 글을 배운다",
							"그의 제안을 거절한다"
						});
					}
					else {
						Dialog(" 저는 베스퍼성에서 이곳으로  가까스로 탈출해 왔습니다. 아마 베스퍼성의 생존자는 아무도 없을 것입니다." +
						" 나는 내 눈으로 나의 아내와 어린 자식이 트롤족의 곤봉에 죽어가는 것을 봤습니다." +
						"  나는 차마 그곳을 떠나고 싶지는 않았지만  이 소식을  다른 대륙에 알리기 위해 겨우 탈출에 성공하여 여기서 치료를 받고 있습니다.");
					}
				}
				else if (moveX == 60 && moveY == 57) {
					Ask($" 나의 이름은 [color={RGB.LightCyan}]알타이르[/color]라고 하오. 독수리같은 용맹을 지녔다는 뜻이지요." +
					" 나는 이 무기상점에서 일하면서 파이터로서의 기술을 연마했소. 만약 당신이 나를 필요로 한다면 기꺼이 도와주겠소.", MenuMode.JoinAltair, new string[] {
						"나에게는 당신같은 사람이 필요하오",
						"그럴 필요까지는 없소"
					});
				}
				else if (moveX == 33 && moveY == 29) {
					Ask($" 나의 이름은  내가 가진 이 하프에서 유래된 [color={RGB.LightCyan}]베가[/color]라고 하며  현재는  이곳의 음유시인으로있습니다." +
					"  나는 원래  이곳 저곳을 돌아다는 성미라서 계속 이곳에 머무르는 것은 매우 답답합니다. 그래서 당신과 함께 여행하고 싶습니다.", MenuMode.JoinVega, new string[] {
						"좋소, 허락하지요",
						"우리가 하는 일은 너무 위험해서..."
					});
				}
				else if (moveX == 37 && moveY == 16) {
					if (!GetBit(4))
					{
						Dialog(" 나는 로드안에게서 당신이 여기로 온다는 소식을 들었소. 유감스럽게도 우리 성의 원정대는 벌써 떠났다오.  당신도 빨리 뒤따라 갔으면 하오." +
						"  성안의 북쪽에 가이아 테라로 통하는 게이트가 있소. 그곳으로 들어가면 가이아 테라로 이동 될 것이오.  돌아오는 방법도 마찬가지라오. 부디 성공하기를 빌겠소.");
					}
					else
						Dialog(" 당신은 역시 로드안이 지목한 용사답소.  정말 훌륭한 일을 해내었소.");
				}
				else if (moveX == 59 && moveY == 41) {
					Dialog(" 내가 전에  어떤 모험가를 통해서 얼핏 들었는데 이 세계에는 모두 여섯개의 테라가 있다고 들었습니다.");
				}
				else if (moveX == 60 && moveY == 45)
				{
					Dialog(" 정말  로드안은 선의 상징입니다.  이번에도 사건이 터지자 마자 악을 무찌르기 위해서 노력을 다 했으니까 말입니다.");
				}
				else if (moveX == 55 && moveY == 58) {
					Dialog(new string[] {
						" 전갈의 심장을 상징하는 이름을 가진 안타레스라는 자의 마법 능력은 정말 대단했습니다.",
						" 한때, 가이아 테라성의 수석 위저드로서  지상 최대의 공격 마법을 구사하더군요.  약 몇 년 전에 이곳에 머물렀던 적이 있습니다."
					});
				}
				else if (moveX == 35 && moveY == 18) {
					Dialog(" 이 세상에서 로드안만큼 칭송받는 인물도 드물 겁니다. 그는 항상 선의 선봉에 서서 주민들을 악으로부터 지켜내고 있으니까요.");
				}
				else if (moveX == 35 && moveY == 20) {
					Dialog(" '테라'라고 하는 말은 라틴어로 대륙이란 뜻이지요.");
				}
				else if (moveX == 40 && moveY == 19) {
					Dialog(" 이 성안에 있는 게이트는  가이아 테라의 배리언트 피플즈라는 성으로 통해 있습니다.");
				}
				else if (moveX == 40 && moveY == 17) {
					Dialog(" 라스트 디치성과 배리언트 피플즈성은  생김새가 똑같습니다.");
				}
				else if (moveX == 40 && moveY == 21) {
					Dialog(" 으윽! 요즘에도 음악 제어하기 힘든  파스칼 언어로 게임을 만드는 무식한 사람이 있다니.");
				}
				else if ((moveX == 36 && moveY == 40) || (moveX == 39 && moveY == 40)) {
					Dialog("... ...");
				}
			}
			else if (mMapName == "Valiant")
			{
				if ((moveX == 53 && moveY == 26) || (moveX == 57 && moveY == 25) || (moveX == 60 && moveY == 20))
					ShowGroceryMenu();
				else if ((moveX == 53 && moveY == 56) || (moveX == 53 && moveY == 58))
					ShowWeaponShopMenu();
				else if ((moveX == 23 && moveY == 58) || (moveX == 19 && moveY == 59) || (moveX == 18 && moveY == 55))
					ShowHospitalMenu();
				else if (moveX == 20 && moveY == 54)
					ShowMedicineStoreMenu();
				else if ((moveX == 19 && moveY == 20) || (moveX == 21 && moveY == 22))
					ShowClassTrainingMenu();
				else if (moveX == 14 && moveY == 18)
					ShowExpStoreMenu();
				else if (moveX == 16 && moveY == 23)
				{
					Ask(new string[] {
					$" 나의 이름은 메듀사의 머리를 뜻하는 [color={RGB.LightCyan}]알골[/color]이라고하오." +
					" 나는 이름처럼, 보기만해도 상대를 제압할 수 있는 용맹을 지녔소.  나는 소환계통의 마법을 이 곳에서 수 년을 연마했다오.",
					" 이제는  나의 능력을 실전에 이용할 때가 되었소. 나는 꼭 당신의 원정을 도와주고 싶소."
					}, MenuMode.JoinAlgol, new string[] {
					"당신같이 용맹을 지닌자라면 좋소",
					"않됐지만 저는 지금 바쁘군요"
					});
				}
				else if (moveX == 55 && moveY == 39)
				{
					Ask($" 나는 켄타우루스의 정기를 받은 사냥꾼으로, [color={RGB.LightCyan}]프록시마[/color]라고하오.  이름에서 알수 있듯이 나의 수호성은 알파 센타우리라오." +
					" 사냥을 나온 중에  당신이 이 곳에 있다는 소리를 듣고 당신의 일행에 참가 하려고 왔던거요.  나를 받아 들이겠소?", MenuMode.JoinProxima, new string[] {
					"좋소, 승락하지요",
					"글세요, 곤란한데요"
					});
				}
				else if (moveX == 37 && moveY == 16)
				{
					if (!GetBit(5) || GetBit(6))
					{
						Dialog(" 나의 성 배리언트 피플즈에 오신 것을 대단이 환영하오." +
						"  우리 성의 이름에서  알 수 있듯이 이 곳의 사람들은  진정한 용기에서 오는 참다운 용기를 아주 높이 평가하고 있소." +
						" 당신같이 용기있는 자들이라면  이 성 어느 곳에서도 환영을 받을 것이오.");
					}
					else
					{
						Talk(" 당신의 위용은 모두 전해 들었소.  인류를 위협하는 세 종족을 없에고  마지막 남은 우라누스 테라의 드라코니안족을 벌하러 간다고 들었소." +
						"  나에게는 우라누스 게이트를 여는 마법이 있소. 그럼 당신을 우라누스 테라로 보내 드리리다.", SpecialEventType.SendValiantToUranos);
					}
				}
				else if (moveX == 44 && moveY == 44)
				{
					Dialog("가이아 테라는 대지신을 상징하는 대륙입니다.");
				}
				else if (moveX == 31 && moveY == 60)
				{
					Dialog(" 오크족은 미개한 집단이지만 개중에는 머리가 약간 뛰어나서  마법 능력이 있는 자도 있다고 하더군요.");
				}
				else if (moveX == 49 && moveY == 55)
				{
					Dialog(" 오크족의 마을은 대륙의 남동쪽에 있습니다. 그들의 무기는 시원챦은 것들뿐이라  우리에게 위협이 되지는 않습니다.");
				}
				else if (moveX == 43 && moveY == 30)
				{
					if (!GetBit(5) || GetBit(6))
						Dialog(" 이 대륙에는 모두 4 개의 대륙으로 가는 게이트가 있습니다. 그 중, 2 개의 게이트가 이 성 안에 있습니다.");
					else
						Dialog(" 지금 당신은 우라누스 게이트를 찾고 있는 것 같은데,  그렇다면 저희 성주님을 만나 보십시요.");
				}
				else if (moveX == 35 && moveY == 18)
				{
					Dialog(" 고대 최강의 마법사였던  레드 안타레스의 아들이 시공간을 넘어  이 세계 어딘가에서 살고 있다고 들었습니다.");
				}
				else if (moveX == 40 && moveY == 20)
				{
					Dialog(" 세상에는  크리스탈에  여러가지 힘을 봉인한 물질이 있습니다.  가장 강력한 영혼의 크리스탈은  모든 적을 한꺼번에 유체이탈 시켜 버린다더군요." +
					" 그것뿐만 아니라 화염의 크리스탈과 한파의 크리스탈 같은 공격용 크리스탈,  소환의 크리스탈 같은 소환용 크리스탈도 있고, 치료용과  그 이외의 용도로 쓰이는 것도 있다고 들었습니다.");
				}
				else if (moveX == 40 && moveY == 18)
				{
					if (!GetBit(4))
					{
						Dialog(" 제 생각에는 당신이 지금하려고 하는 일이 옳지 않다고 봅니다.  비록 트롤족이 야만스럽고 호전적이라는 하지만  아무런 이유없이 베스퍼성을 침공하려 했겠습니까?" +
						"  당신은 지금 원인은 모른채  결과만을 가지고  분개하고 있다는 생각을 안하십니까? 좀 더 깊이 알아보고 햏동하는 것이 진정한 용기라고 봅니다.");
					}
					else
						Dialog(" 당신이 트롤족을 멸한 것은  후세에 오점으로 남을 것입니다.");
				}
				else if (moveX == 35 && moveY == 20)
				{
					if (!GetBit(5))
					{
						Dialog(" 이쉬도 테라에는  아직 인류의 문명이 뻗치지 못했습니다. 거기에는 코볼트족이 지배하고 있기 때문에 아무도 접근하지 못했던 것입니다." +
						"가기만하면 번번히 쫒겨 나오곤 하니까요.");
					}
					else
						Dialog(" 당신 덕분에  이제는 이쉬도 테라에도 인간의 문명을 뻗칠수 있게 되었습니다.");
				}
			}
			else if (mMapName == "Gaea") {
				if ((moveX == 36 && moveY == 38) || (moveX == 39 && moveY == 36) || (moveX == 40 && moveY == 40))
					ShowGroceryMenu();
				else if ((moveX == 36 && moveY == 9) || (moveX == 39 && moveY == 11) || (moveX == 40 && moveY == 14))
					ShowWeaponShopMenu();
				else if ((moveX == 8 && moveY == 38) || (moveX == 11 && moveY == 40))
					ShowHospitalMenu();
				else if ((moveX == 11 && moveY == 14) || (moveX == 14 && moveY == 11))
					ShowClassTrainingMenu();
				else if (moveX == 11 && moveY == 10)
					ShowExpStoreMenu();
				else if (moveX == 24 && moveY == 8) {
					Ask(new string[] {
						$" 당신이 {mPlayerList[0].Name}라는 사람이오?",
						$" 만나서 정말 반갑소.  나는 이곳의 전투승인 [color={RGB.LightCyan}]데네볼라[/color]라고하오." +
						"  사자를 뜻하는 나의 이름에서 알 수 있듯이  나는  맨손 전투에서만은 어느 누구에게도  지지 않소." +
						"  나는 이곳에서 이쉬도 테라의 코볼트족을 정벌하고 신대륙을 개척할 일행을 찾고 있었소.  내가 당신의 일행에 참가하려는데 어떻게 생각하오?"
					}, MenuMode.JoinDenebola, new string[] {
						"쾌히 승락하겠소",
						"하지만 전투승은 필요없소"
					});
				}
				else if (moveX == 10 && moveY == 35) {
					Ask(" 저는 이곳에서 초급마법을 배우는  카펠라라고 합니다.  사실 메이지로서의 제 능력은 아직 많이 부족합니다." +
					"  그래서  당신들과 함께 실전에서 많은 것들을 배우고 싶습니다. 제가 일행에 끼어도 되겠습니까?", MenuMode.JoinCapella, new string[] {
						"그렇다면 한번 동행해 보겠소",
						"당신은 아직 미숙하니 곤란하오"
					});
				}
				else if (moveX == 41 && moveY == 24) {
					if (GetBit(3) && GetBit(4)) {
						Dialog(" 당신이 원한다면 우리 성의 북쪽에 있는 이쉬도 테라로 가는 게이트 입구를 열어 주겠소.");
						if (!GetBit(5))
							Dialog(" 거기에는 코볼트족만이 살고 있으니 조심하시오.", true);
					}
					else if (mParty.Etc[20] == 0) {
						Talk(new string[] {
							$" 역시 와 주었군요, {mPlayerList[0].Name}.",
							" 당신이 지금 베스퍼성에 원정을 가고 있는 중이란 건 알고 있소.  하지만 나의 부탁을 하나 들어 주지 않겠소?",
							" 지금 우리가 있는 가이아 테라에는 인류와 오크족이 공존하고 있소.  오크족들은 우리 대륙는 했지만 그리 문제 될 것까지는 없었소." +
							"  하지만 베스퍼성의 참사를 듣고나니 우리성도 오크족에 대해서는 예외가 아니란걸 알았소." +
							"  우리 성에서도  몇 번은 그들을 정벌하자는 의견이 나왔지만  그들의 해악성이 별로 크지 않은데다가" +
							" 그들의 전염병이 우리에게 옮을수 있다는 생각에서  모두들 기피해 왔던게 사실이오." +
							" 하지만 사태가 사태이니만큼 한시 바삐 그들을 정벌해야만 우리가 편안히 지낼수 있겠소.",
							" 오크족을  정벌해 주지 않겠소?  만약 당신이 허락한다면 당신에게 이 성의 보물창고를 개방하리다." +
							"  이곳 주민을 대표하여 당신에게 부탁하겠소."
						}, SpecialEventType.None);

						mParty.Etc[20]++;
					}
					else if (mParty.Etc[20] == 1) {
						Dialog(" 오크족의 오크킹만 제거한다면 그들의 사회는 붕괴 될거요. 그리고 오크족의 마을은 이 성의 동쪽 멀리에 있소. 그럼 부탁하오.");
					}
					else if (mParty.Etc[20] == 2) {
						Dialog(" 당신이라면 해낼줄 알았소. 그 답례로 보물창고를 당신에게 개방하겠소.  거기서 마음껏 필요한 것을 가져 가도록하시오.");
						mParty.Etc[20]++;
					}
					else if (mParty.Etc[20] == 3) {
						Dialog(" 베스퍼성이 있는 아프로디테 테라로 가는  게이트가 대륙의 북동쪽 산악지역에 있소.  부디 당신이 성공하기를 바라오.");
					}
				}
				else if (moveX == 36 && moveY == 26) {
					Dialog(" 오크족은  강한자가 왕이되는 전통을  가지고 있습니다.");
				}
				else if (moveX == 37 && moveY == 13) {
					Dialog(" 오크족에서 제대로 마법을 행하는 자는  오크킹의 왼팔격인 이키메이지뿐입니다.");
				}
				else if ((moveX == 39 && moveY == 23) || (moveX == 39 && moveY == 26)) {
					Dialog(" 오크들은  정말 추하고 더럽고 비위생적인 종족입니다.");
				}
				else if (moveX == 36 && moveY == 23) {
					Dialog(" 저는 오크족에 대해 수 년동안 정보수집을 했었습니다." +
					" 제가 알게된 바로는, 오크의 보스는 오크킹이며 그 밑에  오크 아키몽크와 오크 아키메이지가 있습니다." +
					"  또  아키메이지 밑에는 그의 마법제자인 오크 매직유저가 7 명 있으며 아키몽크 밑에는 4 명의 오크 전사와 수 십 또는 수 백명의 오크 병사가 있습니다.");
				}
				else if (moveX == 41 && moveY == 33) {
					Dialog(" 드라코니안족은  인간과 드래곤의 트기라더군요." +
					"  인간처럼 두 팔과 두 다리가 있으나 머리는 드래곤의 머리이며 날개와 꼬리가 있고  지능 또한 상당히 높다고 하더군요.");
				}
				else if (moveX == 26 && moveY == 14) {
					if (!GetBit(5))
					{
						Dialog(" 코볼트족의 배타성 때문에  인류의 신세계 개척이 늦어지고 있습니다. 그들을 굴복 시켜 한시 바삐 새 시대를 열어야 합니다.");
					}
					else {
						Dialog(" 이제는 신세계 개척에 힘써야 할 때입니다.");
					}
				}
				else if (moveX == 13 && moveY == 37) {
					Dialog(" 저는 코볼트족이 지배하는  이쉬도 테라에 갔다가 겨우 도망쳐 나왔습니다." +
					" 그들은 외부 종족이 그들의 영역 내에 들어오는 것을 매우 싫어하더군요.  그들의 무장은 다른 종족보다 강하여 도저히 당해내기 힘들었습니다.");
				}
				else if (moveX == 23 && moveY == 28) {
					if (mParty.Etc[4] == 0 || GetBit(8)) {
						Dialog(" 당신이 오크의 말과 글을 배운다면 그들과 대화가 이루어 질거요.");
					}
					else {
						Ask(" 만약 당신이 나에게 금을 2,000개 준다면 기꺼이 일주일만에 오크의 글을 가르쳐 주리다.", MenuMode.LearnOrcWriting, new string[] {
						"그렇게 합시다",
						"배울 필요는 없을것 같소"
						});
					}
				}
			}
			else if (mMapName == "OrcTown") {
				if (moveX == 9 && moveY == 38) {
					if (GetBit(7))
						Talk(" 제발 살려 주십시요.  저는 여태껏 남에게 해 한 번 안입히고 살아왔습니다.", SpecialEventType.AskKillOrc1);
					else
						Talk(" 당신 앞의 오크 주민이 무어라 당신에게 애원하는듯 했지만 알아들을 수는 없었다.", SpecialEventType.AskKillOrc1);
				}
				else if (moveX == 8 && moveY == 18) {
					if (GetBit(7))
						Talk(" 우리는  당신네 인간들에게  아무런 나쁜짓도 안했는데 왜 당신들은 우리를 못 살게 구는 겁니까 ?", SpecialEventType.AskKillOrc2);
					else
						Talk(" 당신 앞의 오크 주민이 무어라 당신에게 하소연하는듯 했지만 알아들을 수는 없었다.", SpecialEventType.AskKillOrc2);
				}
				else if (moveX == 12 && moveY == 11)
				{
					if (GetBit(7))
						Talk(" 나는 이곳에서 50년 가까이 농사만 지으며 살아왔소.  이곳 밖으로 나가본 적도 거의 없고, 더우기 인간에게 밑보일 짓도 하지 않았소." +
						" 그런데 왜 당신들은 우리 마을을 짓밟고 우리 동족을 죽이려하오?", SpecialEventType.AskKillOrc3);
					else
						Talk(" 당신 앞에 있는 늙은 오크가 당신에게 무어라 질책하는듯 하였으나 도무지 말을 알아들을 수가 없었다.", SpecialEventType.AskKillOrc3);
				}
				else if (moveX == 20 && moveY == 8)
				{
					if (GetBit(7))
						Talk(" 분명 오크족 중에서도 나쁜자가 있어서  지나가는 여행자들을 위협하기도 했던게 사실이오. 하지만  대부분은 선량한 오크들이란걸 알아두시오.", SpecialEventType.AskKillOrc4);
					else
						Talk(" 당신 앞의 오크 주민이 무어라 당신에게 하소연하는듯 했지만 알아들을 수는 없었다.", SpecialEventType.AskKillOrc4);
				}
				else if (moveX == 42 && moveY == 11)
				{
					if (GetBit(7))
						Talk(" 당신네들이  이렇게 우리들을  못살게 군다면 오크킹님이 가만 있지 않을 것입니다.", SpecialEventType.AskKillOrc5);
					else
						Talk(" 당신 앞의 오크 주민이 무어라 당신에게 적개심을 보이며 이야기했지만 알아들을 수는 없었다.", SpecialEventType.AskKillOrc5);
				}
				else if (moveX == 43 && moveY == 29)
				{
					if (GetBit(7))
						Talk(" 우리들은 에인션트 이블님을 정신적인 지도자로 생각하고 있습니다.", SpecialEventType.AskKillOrc6);
					else
						Talk(" 당신 앞의 오크 주민이 무어라 당신에게 차분하게 이야기했지만 알아들을 수는 없었다.", SpecialEventType.AskKillOrc6);
				}
				else if (moveX == 34 && moveY == 28)
				{
					if (GetBit(7))
						Talk(" 로드안은 이 세계의 적입니다.  그리고  그를 따르는 당신도 적입니다.", SpecialEventType.AskKillOrc7);
					else
						Talk(" 당신앞에 있는 오크 청년은  당장이라도 달려들듯이 당신에게 무어라 소리쳐댔다.", SpecialEventType.AskKillOrc7);
				}
				else if (moveX == 40 && moveY == 38) {
					if (GetBit(7))
						Talk(" 우리 부족의 아키메이지는 코볼트족의 왕에게서 마법을 배웠습니다.  그라면 충분히 당신을 내 쫒을 겁니다.", SpecialEventType.AskKillOrc8);
					else
						Talk(" 당신앞에 있는 오크 청년은  당장이라도 달려들듯이 당신에게 무어라 소리쳐댔다.", SpecialEventType.AskKillOrc8);
				}
				else if ((moveX == 24 && moveY == 41) || (moveX == 24 && moveY == 42)) {
					Ask(new string[] {
						" 마을의 문 앞을 지키고 있는 오크가 당신에게 인간의 말로 이렇게 말했다.",
						"",
						"",
						$"[color={RGB.LightMagenta}] 인간은 오크 마을에 들어 올 수 없소.[/color]",
						$"[color={RGB.LightMagenta}] 썩 물러나시오 !![/color]"
					}, MenuMode.GuardOrcTown, new string[] {
						"알았소, 나가면 될거 아니오",
						"당신들을 쓰러뜨리고 지나가야겠소"
					});
				}
				else if ((moveX == 23 && moveY == 19) || (moveX == 25 && moveY == 19)) {
					mEncounterEnemyList.Clear();

					for (var i = 0; i < 3; i++)
						JoinEnemy(18);

					JoinEnemy(19);
					for (var i = 0; i < 2; i++)
						JoinEnemy(20);

					DisplayEnemy();
					HideMap();

					if (GetBit(7))
						Talk($"[color={RGB.LightMagenta}] 인간은 절대 이곳으로 들여 보내지 않겠다. 자! 덤벼라.[/color]", SpecialEventType.BattleOrcTempleEnterance);
					else
						Talk("무장을 제법 갖춘 2명의 오크 전사와 다른 전투사들이 당신을 공격하기 시작했다.", SpecialEventType.BattleOrcTempleEnterance);
				}
			}
			else if (mMapName == "Vesper")
			{
				if (GetTileInfo(moveX, moveY) == 53)
				{
					var specialEvent = SpecialEventType.None;

					if (moveX == 29 && moveY == 18)
						specialEvent = SpecialEventType.BattleVesperTroll1;
					else if (moveX == 49 && moveY == 18)
						specialEvent = SpecialEventType.BattleVesperTroll2;
					else if (moveX == 63 && moveY == 25)
						specialEvent = SpecialEventType.BattleVesperTroll3;
					else if (moveX == 60 && moveY == 37)
						specialEvent = SpecialEventType.BattleVesperTroll4;
					else if (moveX == 39 && moveY == 39)
						specialEvent = SpecialEventType.BattleVesperTroll5;
					else if (moveX == 21 && moveY == 39)
						specialEvent = SpecialEventType.BattleVesperTroll6;
					else if (moveX == 19 && moveY == 25)
						specialEvent = SpecialEventType.BattleVesperTroll7;


					if (GetBit(9))
						Talk($"[color={RGB.LightMagenta}] 아직도 남은 인간이 있었다니... 너도 나에게 죽어줘야겠다.[/color]", specialEvent);
					else
						Talk(" 당신 앞의 트롤이 무어라 말했지만 알아 들을 수가 없었다.", specialEvent);
				}
				else if (moveX == 46 && moveY == 54) {
					Dialog(new string[] {
						" 당신 앞의 시체는 다른 시체들보다 늦게 살해 당했는지 부패의 정도가 덜했다.  비교적 형태를 알아 볼 수 있었기에  당신은 그의 몸을 조사하기 시작했다.",
						""
					});

					if (GetBit(15))
						Dialog(" 하지만 더 이상 발견되지 않았다.", true);
					else
						InvokeAnimation(AnimationType.InvestigateDeadBody);
				}
				else {
					Dialog(" 당신 앞에는 형체도 알아보기 힘들 정도로 처참히 살해된 인간의 시체가 있었다.");
				}
			}
			else if (mMapName == "TrolTown") {
				if (GetTileInfo(moveX, moveY) == 54) {
					void CantSpeakTrollSpeaking(SpecialEventType specialEvent) {
						Talk(" 당신 앞의 트롤족이 당신에게 무어라 말을 했지만 알아 들을 수가 없었다.", specialEvent);
					}

					if ((moveX == 24 && moveY == 7) || (moveX == 25 && moveY == 7)) {
						if (!GetBit(16) && GetBit(9)) {
							Ask($"[color={RGB.LightMagenta}] 아니! 이곳까지 이방인이 쳐들어 오다니.... 나는 이곳의 왕인  트롤킹이라고 하는데  우리 협상으로 해결하는게 어떻겠는가?[/color]"
							, MenuMode.NegotiateTrollKing, new string[] {
								"그럼 조건부터 들어보겠소",
								"협상은 필요없소"
							});
						}
					}
					else if (moveX == 16 && moveY == 18) {
						mEncounterEnemyList.Clear();

						for (var i = 0; i < 3; i++) {
							var enemy = JoinEnemy(19);
							enemy.Name = "트롤매직유저";
							enemy.Level = 6;
						}

						for (var i = 0; i < 3; i++)
							JoinEnemy(27);

						JoinEnemy(33);

						DisplayEnemy();
						HideMap();

						if (GetBit(9)) {
							Talk($"[color={RGB.LightMagenta}] 감히 트롤의 보물을 탈취하려고  이곳에 오다니! 나는 이 보물창고의 경비대장으로써  목숨을 걸고 보물을 지킬테다.[/color]", SpecialEventType.BattleTroll1);
						}
						else {
							mBattleEvent = BattleEvent.Troll1;
							StartBattle(false);
						}
					}
					else if (moveX == 33 && moveY == 18) {
						for (var i = 0; i < 4; i++)
							JoinEnemy(30);

						for (var i = 0; i < 2; i++)
							JoinEnemy(31);

						JoinEnemy(32);

						DisplayEnemy();
						HideMap();

						if (GetBit(9)) {
							Talk($"[color={RGB.LightMagenta}] 바로 너희들이 우리 동굴에 잡혀 있는 인간들을 구하러온 자들인가?" +
							" 하지만 나의 파괴 마법앞에서는 너희들의 배짱도 산산히 부셔져 버릴 것이다.[/color]", SpecialEventType.BattleTroll2);
						}
						else {
							mBattleEvent = BattleEvent.Troll2;
							StartBattle(false);
						}
					}
					else if ((moveX == 12 && moveY == 31) || (moveX == 12 && moveY == 32))
					{
						for (var i = 0; i < 4; i++)
							JoinEnemy(30);

						for (var i = 0; i < 4; i++)
							JoinEnemy(27);

						DisplayEnemy();
						HideMap();


						mBattleEvent = BattleEvent.Troll3;
						StartBattle(true);
					}
					else if((moveX == 37 && moveY == 31) || (moveX == 37 && moveY == 32))
					{
						for (var i = 0; i < 2; i++)
							JoinEnemy(25);

						for (var i = 0; i < 3; i++)
							JoinEnemy(30);

						for (var i = 0; i < 3; i++)
							JoinEnemy(27);

						DisplayEnemy();
						HideMap();

						mBattleEvent = BattleEvent.Troll4;
						StartBattle(true);
					}
					else if ((moveX == 23 && moveY == 66) || (moveX == 24 && moveY == 66) || (moveX == 25 && moveY == 66))
					{
						for (var i = 0; i < 8; i++)
						{
							var enemy = JoinEnemy(25);
							enemy.Name = "트롤매직유저";
							enemy.Level = 5;
						}

						DisplayEnemy();
						HideMap();

						if (GetBit(9))
						{
							Talk($"[color={RGB.LightMagenta}] 너희들을 절대 이 안으로 들여 보낼수 없다!![/color]", SpecialEventType.BattleTroll5);
						}
						else
						{
							mBattleEvent = BattleEvent.Troll5;
							StartBattle(false);
						}
					}
					else if (moveX == 5 && moveY == 30) {
						if (GetBit(9)) {
							Ask(" 날 제발 살려 주시오.  나는 이곳의 물리학자인데  당신에게 중요한 발견하나를 알려줄테니 제발 목숨만은 살려주시오.",
							MenuMode.PhysicistTeaching, new string[] {
								"좋소, 한번 들어봅시다",
								"날 속이려고 드는군, 자 죽어랏!"
							});
						}
						else
							CantSpeakTrollSpeaking(SpecialEventType.AskKillTroll6);
					}
					else if (moveX == 8 && moveY == 29) {
						if (GetBit(9))
							Talk(" 으악! 인간이다 모두 도망쳐라!  인간이 우리 동굴에 침입했다.", SpecialEventType.AskKillTroll7);
						else
							CantSpeakTrollSpeaking(SpecialEventType.AskKillTroll6);
					}
					else if (moveX == 6 && moveY == 32)
					{
						if (GetBit(9))
							Talk(" 정말 당신들은 집요하군요. 또 우리에게 해를 입히려 쳐들어오다니...", SpecialEventType.AskKillTroll8);
						else
							CantSpeakTrollSpeaking(SpecialEventType.AskKillTroll8);
					}
					else if (moveX == 10 && moveY == 34)
					{
						if (GetBit(9))
							Talk(" 당신들이 나를 죽이더라도 나의 신인  에인션트 이블님이 복수를 해 주실 겁니다.", SpecialEventType.AskKillTroll9);
						else
							CantSpeakTrollSpeaking(SpecialEventType.AskKillTroll9);
					}
					else if (moveX == 39 && moveY == 29)
					{
						if (GetBit(9))
							Talk(" 침략자 !!", SpecialEventType.AskKillTroll10);
						else
							CantSpeakTrollSpeaking(SpecialEventType.AskKillTroll10);
					}
					else if (moveX == 40 && moveY == 34)
					{
						if (GetBit(9))
							Talk(" 아이고, 위대하신 용사님들.  제발 저에게 헤로인 주사를 좀 놔 주십시요. 부탁합니다.  지금 헤로인을 맞지 않으면 죽을 것 같습니다.", SpecialEventType.AskKillTroll11);
						else
							CantSpeakTrollSpeaking(SpecialEventType.AskKillTroll11);
					}
					else if (moveX == 42 && moveY == 31) {
						if (GetBit(9))
						{
							Talk(" 당신네 인간들은  우리 트롤족에게서 금을 뺏기위해,  진통과 설사에 좋다는 명목으로 헤로인을 주사하기 시작했소." +
							"  그 때문에 상당수의 트롤 주민들이 헤로인 중독에 고생하며,  또한 헤로인을 구하기 위해  인간들에게  많은 금을 지불하고 있소.", SpecialEventType.AskKillTroll12);
						}
						else
							CantSpeakTrollSpeaking(SpecialEventType.AskKillTroll12);
					}
					else if (moveX == 44 && moveY == 33) {
						if (GetBit(9))
							Talk(" 으윽, 제발 헤로인 좀 놔 주십시오. 금 일 만개를 드리리다.", SpecialEventType.AskKillTroll13);
						else
							CantSpeakTrollSpeaking(SpecialEventType.AskKillTroll13);
					}
				}
				else if (moveX == 36 && moveY == 16) {
					Talk(" 당신들이 우리를 구하러 온 사람들이오? 정말 감사하오.  소문에 의하면 베스퍼성이  완전히 전멸 당했다는데 그 말이 사실이오?  그렇다면 정말 유감이군요.", SpecialEventType.MeetBecrux);
				}
				else if (moveX == 41 && moveY == 15) {
					Dialog(" 헤로인을 만드는건 쉬워요. 설익은 양귀비 열매 꼬투리에 흠집을내면 나오는 우유같은 수액을  갈색 고무질로 건조 시키면 되는거니까요." +
					" 그건 일종의 중추 신경 완화제의 역할을 하죠. 그걸 트롤에게 팔아서 꽤 돈을 벌었는데  한날 트롤족들이 들고 일어나는 바람에 꼼짝없이 잡혀서 여기에 있는거죠.");
				}
				else if (moveX == 41 && moveY == 20) {
					Talk(" 베스퍼성의 사람들은  이 대륙을 확실한 인간의 영토로 굳히기 위해  번번히 트롤족의 동굴을 공격했소." +
					"  때로는 협박을하고 때로는 회유책을 쓰면서  그들을 대륙의 구석으로 몰아 붙이려 했지만 역부족이었소." +
					"  그래서 베스퍼 성주를 중심으로 트롤의 동굴에 대대적인 공격을 감행했었소.  그때 죽거나 잡힌 트롤들은 모두 사지가 절단된채  쇠 꼬챙이에 꿰어 매달려 있었소." +
					" 그들에게 겁을 주어 다시는 인간에게 대항하지 못하게 하자는 속셈이었던 거요.  하지만 예상은 빗나갔었소." +
					" 그 광경들을 본 트롤들은 치밀어 오르는 분노에 집단적으로 베스퍼성에 대한 야습을 개시했던 거요. 여지껏 당하고만 살아왔던 한을  이때 모두 폭발시켰던 거라오." +
					" 그래서 예상하지도 못했던 기습을 받은 베스퍼성은 순식간에 전멸되었던 것이었소. 모두가 다  우리가 자청한 일이나 다름없었던 거지요.", SpecialEventType.None);
				}
			}
			else if (mMapName == "Hut") {
				if (moveX == 14 && moveY == 8) {
					if (!GetBit(12)) {
						Ask(" 나는 어릴때부터 코볼트족에게 발견되어 그들의 손에 자라났습니다." +
						"  지금은 비록 인간이라는 이유로 그들의 집단에서 소외되어 이곳에서 홀로 살고 있지만  한때는 코볼트족의 말과 글을 사용하던 시절이 있습니다." +
						" 당신 혹시 나에게 체력 회복약 5개만 주지 않겠습니까?  그런다면  나는 기꺼이 당신에게 코볼트의 글을 가르쳐 주겠습니다." +
						"  그것도  일주일만에 말입니다.", MenuMode.LearnKoboldWriting, new string[] {
							"좋소, 그렇게 합시다",
							"체력 회복약 5개는 좀..."
						});
					}
					else if (GetBit(204) || GetBit(205)) {
						Dialog(" 당신들이 가지고 있는  창과 갑옷을 어디선가 본 것 같은데...");
					}
					else {
						Dialog(" 이제 당신은  웬만한 코볼트 글을 이해 할 수 있을 겁니다.");
					}
				}
			}
			else if (mMapName == "Ancient") {
				if ((moveX == 9 && moveY == 16) || (moveX == 10 && moveY == 16)) {
					Ask(new string[] {
						" 신전 입구인듯한 곳에  신자인것 같은 드라코니안 두명이 서 있었다. 당신들을 보고 뭐라고 알 수 없는 말을 서로 지껄이더니 서투른 인간의 말로 당신에게 말했다.",
						"",
						$"[color={RGB.LightMagenta}] 여기는  에인션트 이블님의 신전을 모시고 있는 신전입니다. 그리고 우리 둘은 이곳을 관리하는 신도들입니다.  어떤 일로 여기에 오셨습니까?[/color]"
					}, MenuMode.MeetAncientEvil, new string[] {
						"당신들을 죽이러 왔소",
						"에인션트 이블을 만나러 왔소"
					});
				}
			}
			else if (mMapName == "DracTown") {
				if (moveX == 23 && moveY == 97)
				{
					if (GetBit(13))
						Talk(" 아니! 인간이 여길 들어오다니...", SpecialEventType.BattleDraconian1);
					else
						BattleDraconian(BattleEvent.Draconian1);

				}
				else if (moveX == 23 && moveY == 91)
				{
					if (GetBit(13))
						Talk(" 음? 너는... 호모 사피엔스 종족이로군. 이처럼 하등한 포유류가  감히  드라코니안 종족의 영역을 침법하다니 놀랍군.", SpecialEventType.BattleDraconian2);
					else
						BattleDraconian(BattleEvent.Draconian2);
				}
				else if (moveX == 24 && moveY == 80) {
					if (GetBit(13))
						Talk(" 그래... 너 잘만났다. 한번 죽어봐라.", SpecialEventType.BattleDraconian3);
					else
						BattleDraconian(BattleEvent.Draconian3);
				}
				else if (moveX == 33 && moveY == 99) {
					if (GetBit(13))
						Talk(" 당신이 우리를 정벌하러 왔다는  말을 들었는데 뭔가 잘못 안 것은 아니오?" +
						" 여태껏 우리 종족과 당신들은 아무런 접촉도 없었는데 악의를 품은채 이곳으로 올 이유가 없지 않소. 어짜피 당신은 우리들의 상대가 안되니  여기서 썩 물러 나시오.",
						SpecialEventType.BattleDraconian4);
					else
						BattleDraconian(BattleEvent.Draconian4);
				}
				else if (moveX == 35 && moveY == 92)
				{
					if (GetBit(13))
						Talk(new string[] {
						" 우리 드라코니안족은 드래곤과 인간의 장점만을 모아 진화한 생명체로써 드라콘이라고 줄여 부르기도 하지." +
						" 지능은 IQ 200에서 250 정도로 인간의 두배 정도이고 키도 평균 2m 30cm로 인간보다 크고  게다가 날개도 있어서 하늘을 나는 것도 가능하네." +
						" 수명도 150 에서 200 세 정도라서 인간보다 발전 가능성도 뛰어나지.",
						" 흐흐흐... 어때? 이래도 우리에게 대항하겠는가?"
						}, SpecialEventType.BattleDraconian5);
					else
						BattleDraconian(BattleEvent.Draconian5);
				}
				else if (moveX == 41 && moveY == 83) {
					if (GetBit(13))
					{
						Talk(" 우와~~ 말로만 듣던 인간을 직접 이렇게 보게 되다니... 정말 신기하게 생겼네.", SpecialEventType.BattleDraconian6);
					}
					else
						BattleDraconian(BattleEvent.Draconian6);
				}
				else if (moveX == 32 && moveY == 81) {
					if (GetBit(13))
						Talk(" 콩알만한 것들이 겁도 없이 들어 오다니.  심심하던 차에 잘됐군. 싸움이나 걸어야지.", SpecialEventType.BattleDraconian7);
					else
						BattleDraconian(BattleEvent.Draconian7);
				}
				else if (moveX == 40 && moveY == 69) {
					if (GetBit(13))
						Talk(" 쩝쩝, 배고파.", SpecialEventType.BattleDraconian8);
					else
						BattleDraconian(BattleEvent.Draconian8);
				}
				else if (moveX == 31 && moveY == 21) {
					if (GetBit(13))
					{
						Talk(" 나는 이곳 최고의 물리학자일세. 그리고 나의 이름은 아인슈타인이라네. 내가 어릴때 머리가 완전히 돌이었지." +
						$"  그래서 항상 아이들이 나를 보며 [color={RGB.LightGreen}]\"Du bist ein Stein\"[/color]이라고 놀리곤했지. 거기서 나온 이름이 내 이름인 'Einstein'일세", SpecialEventType.BattleDraconian9);
					}
					else
						BattleDraconian(BattleEvent.Draconian9);
				}
				else if (moveX == 39 && moveY == 32) {
					if (GetBit(13))
					{
						Talk(" 만약, 당신이 자꾸 우리를 위협하겠다면 우리는 성의 서쪽 감옥에 있는 인간들을 모두 처형시켜 버릴테다.", SpecialEventType.BattleDraconian10);
					}
					else
						BattleDraconian(BattleEvent.Draconian10);
				}
				else if (moveX == 15 && moveY == 18) {
					if (GetBit(13))
					{
						Talk(" 나를 여기서 탈출 시켜 주시오.  그러면 우리 가문의 보물을 드리리다.", SpecialEventType.BattleDraconian11);
					}
					else
						BattleDraconian(BattleEvent.Draconian11);
				}
				else if (moveX == 15 && moveY == 34) {
					if (GetBit(13))
					{
						Talk(new string[] {
							" 나도 나쁜 놈이지만  당신은 더 나쁜 놈이오. 왜 그렇냐고 물으면 웃지요.",
							" 으하하하.. 낄낄.. 흐흐흐..."
						}, SpecialEventType.BattleDraconian12);
					}
					else
						BattleDraconian(BattleEvent.Draconian12);
				}
				else if (moveX == 14 && moveY == 47) {
					if (GetBit(13))
					{
						Talk(" 위대한 프로그래머이시며 이 세계의 창시자이신 분과 타임워커 알비레오는  동일인물이라는 생각이 안드오?", SpecialEventType.BattleDraconian13);
					}
					else
						BattleDraconian(BattleEvent.Draconian13);
				}
				else if (moveX == 14 && moveY == 56) {
					Ask(" 나는 드라콘족의 통치체제에 반발하다가 이곳에 반역죄로 잡혀오게 되었소. 당신 역시 이곳의 체제를  전복 시키려는게  주목적인것 같은데, 우리 힘을 합해 보는게 어떻겠소.",
					MenuMode.JoinDraconian, new string[] {
						"그렇게 하지요",
						"그건 안되겠소"
					});
				}
				else if (moveX == 21 && moveY == 23) {
					Talk(" 나는 마징가 Z 를 실제로 보았죠.  정말 무쇠팔, 무쇠다리, 로켓트주먹을 가지고 있더군요. 만약 전쟁이 일어나면 인천 앞바다가 갈라지며 출동한다고 들었어요. 그것뿐만 아니죠." +
					"  잠실구장이 열리면서 메칸더 V 도 나오고 비룡폭포에서는  그렌다이져가  뉴크프리트와 합체하여 출격하고  육군본부에 보호망이 쳐지며 그레이트 마징가와 비너스 A 가 같이 나타나고" +
					" 공군 본부에서 출동한 독수리 5 형제가 조국의 창공을 지키게 되죠. 물론 김박사가 만든 태권 V도 최후의 희망으로 버티고 있죠. 뭐,믿기지 않는다면 국방부 장관님께 직접 물어 보시죠.",
					SpecialEventType.SteelBoy);
				}
				else if (moveX == 23 && moveY == 49) {
					if (mParty.Etc[4] == 0)
						Dialog(" 그는 당신을 슬쩍본 후 다시 자기 할 일만 하고 있었다.");
					else {
						Talk(" 당신은 지금 로드안에게 속고 있소.  그는 표면적인  선을 행하기 위해서  당신을 이용하고 있는 것이오.  당신이 드라코니안족을  적대시 할 이유가 없소." +
						" 그들은 인간에게 아무런 피해도 간섭도 없이 살아왔소. 그들이 에인션트 이블을 숭상한다는 것이  정벌의 이유가 될 수는 없소." +
						"  당신은 에인션트 이블을 진정으로 만나보기라도 했소?  당신은 위선자 로드안의 말만 믿고 에인션트 이블과 그의 추종자들을 배척하고 있는거란 말이오." +
						"  당신은 어릴때부터 에인션트 이블과  악은 나쁘다고  사상교육을 받아왔던걸 아시오?" +
						" 진리를 모른채 단지 위에서 가르치는대로 배우며  그것을 진실로 받아들이고 머리속에 새겼던게 바로 화근이었던 것이오.", SpecialEventType.None);
					}
				}
				else if (moveX == 66 && moveY == 20) {
					if (mParty.Etc[4] == 0)
						Dialog(" 그는 당신을 보더니 외면해 버렸다.");
					else if (0 <= mParty.Etc[23] && mParty.Etc[23] <= 1) {
						Dialog(" 그는 당신을 힐끗 보더니 외면해 버렸다.");
						mParty.Etc[23]++;
					}
					else if (mParty.Etc[23] == 2) {
						if (GetBit(13)) {
							Dialog(" 나는 드라코니안족의 대예언자요. 나에게는 당신의 미래가 보인다오.  그러나  그것은 너무나 무서운 것이기에 함부로 얘기할 수 없소.");
							mParty.Etc[23]++;
						}
					}
					else if (mParty.Etc[23] == 3) {
						if (GetBit(13)) {
							Dialog(" 좋소, 애기해 주겠소.  당신은 곧 에인션트 이블님을 만나게 될거요. 그리고 로드안을 배신하려는 선택을 할지도 모르는 순간이 다가올 것이오." +
							"  그때의 선택은 당신의 결말을 크게 뒤흔들 것이오.");
							mParty.Etc[23]++;
						}
					}
					else
						Dialog(" 그는 당신을 힐끗 보더니 외면해 버렸다.");
				}
				else if ((moveX == 13 && moveY == 104) || (moveX == 14 && moveY == 105)) {
					if (GetBit(13))
					{
						Ask($"[color={RGB.LightMagenta}] 잠깐, 너희들은 누구냐? 이 안으로 들어갈 수 있는 것은 드라코니안족 뿐이다. 썩 물러나라.[/color]", MenuMode.BattleDraconianEntrance,
						new string[] {
							"적과 싸운다",
							"그냥 물러선다"
						});
					}
					else
						BattleDraconianEntrance();
				}
				else if ((moveX == 46 && moveY == 60) || (moveX == 47 && moveY == 60)) {
					if (GetBit(13))
					{
						Talk($"[color={RGB.LightMagenta}] 이곳은 너희같은 이방인은 절대 들어갈 수 없는 곳이다. 그래도 끝까지 들어가려 한다면 나와의 사생결단이 남았을뿐이다.[/color]", SpecialEventType.BattleDraconianEntrance2);
					}
					else
						BattleDraconianEntrance2();
				}
				else if (moveX == 26 & moveY == 53) {
					if (GetBit(13))
					{
						Talk($"[color={RGB.LightMagenta}] 오! 에인션트 이블님이시여! 그대의 이름으로 저들에게 패배를 명하소서.[/color]", SpecialEventType.BattleDraconianBoss1);
					}
					else
						BattleDraconianBoss1();
				}
				else if (moveX == 83 && moveY == 58) {
					if (GetBit(13)) {
						Talk($"[color={RGB.LightMagenta}] 내가  광란자라고 불리는 이유를  너희들에게 보여주지. 자, 맛 좀 봐랏![/color]", SpecialEventType.BattleDraconianBoss2);
					}
					else
						BattleDraconianBoss2();
				}
				else if (moveX == 78 && moveY == 87) {
					if (GetBit(13))
					{
						Talk($"[color={RGB.LightMagenta}] 나는 너희들과 같은  보물 약탈자들을 제거하기 위해 보물창고 앞을 지키고 있는  프로스트 드라코니안이다." +
						" 나의 냉기와 한파공격에는 아무 저항도 하지 못하고 나가 떨어질 것이다.[/color]", SpecialEventType.BattleFrostDraconian);
					}
					else
						BattleFrostDraconian();
				}
				else if (moveX == 58 && moveY == 47) {
					if (GetBit(13))
					{
						Talk($"[color={RGB.LightMagenta}] 멋모르고 날뛰고 있는 인간들아 들어라. 나는 성전사의 명예를 걸고  드라코니안족과 우리의 왕을 위해 싸울 것이다.[/color]", SpecialEventType.BattleDraconianHolyKnight);
					}
					else
						BattleDraconianHolyKnight();
				}
				else if (moveX == 72 && moveY == 39) {
					if (GetBit(13))
					{
						Talk($"[color={RGB.LightMagenta}] 네가 더 이상 이 안으로 들어갈 수 없는 이유는 바로 내가 있기 때문이다." +
						"  내가 만든 나의 환상속에서 너는 스스로의 능력에 대한 비애를 느낄수 밖에 없을 것이다.[/color]", SpecialEventType.BattleDraconianMagician);
					}
					else
						BattleDraconianMagician();
				}
				else if (moveX == 93 && moveY == 36) {
					if (GetBit(13))
					{
						Talk($"[color={RGB.LightMagenta}] 나는 아키드라코니안이라고 하는 여기 최강의 용사이다.  또한  드라코니안킹의 방을 지키는 경호대장이다." +
						"  너같은 자들은  나의 능력으로 충분히 제거해 버릴수 있다.[/color]", SpecialEventType.BattleDraconianGuardian);
					}
					else
						BattleDraconianGuardian();
				}
				else if (moveX == 101 && moveY == 20 && !GetBit(95)) {
					Talk(new string[] {
					$"[color={RGB.LightMagenta}] 나는  네가 목표로하는  드라코니안족의 왕이다.  아마 이곳에 살아 남은 우리 종족의 전투사들은 거의 없을 것으로 안다." +
					" 보이다시피 지금 너의 앞에는  나와 가드 드라코니안 3명 밖에는 없다.  너는 이 기회를 놓치지 않고 반드시  나를 쓰러뜨려서  드라코니안 종족을 멸망시키려 할 것이란건  알고 있다." +
					"  하지만 지금 나의 편이 불리하다고  내가 진다는 것은 아니다.  나 역시 반드시 너희들을 패배 시켜서 우리 종족을 존속 시키려고 필사적이다.[/color]",
					$"[color={RGB.LightMagenta}] 이제 나의 뜻을 알았을 것이라 믿는다.  그럼 이제 결전이다![/color]"
					}, SpecialEventType.MeetDraconianKing);
				}
			}
			else if (mMapName == "Tomb") {
				if ((moveX == 88 && moveY == 19) || (moveX == 87 && moveY == 20)) {
					Dialog(" 당신은 여기서 뼈만 남은 인간의 해골을 보았다.  아마도 오래전에 여기를 도굴하려다가 갇혔던 것같다.");
				}
			}
			else if (mMapName == "Imperium") {
				if (moveX == 8 && moveY == 6)
				{
					Dialog(" 예전에는 로어성 주민과도  교역 했었다고 하는데  몇 백년 전부터는 무슨 이유인지는 몰라도 왕래가 완전히 끊겨 버렸어요.");
				}
				else if (moveX == 7 && moveY == 34)
				{
					Dialog(" 이 성의 중앙에는  에인션트 이블님의 의지가 잠들어 있습니다.");
				}
				else if (moveX == 44 && moveY == 34)
				{
					Dialog(" 이곳의 도서관은 색다른 내용의 보고라오.");
				}
				else if (moveX == 23 && moveY == 38)
				{
					Dialog(" 당신은 못 보던 사람이군요. 아마 여행자인것 같은데 우리 성에 온 것을 환영하오.");
				}
				else if (moveX == 25 && moveY == 38)
				{
					Dialog(" 여기는  에인션트 이블님의  자비로써 세워진 '임페리엄 마이너성'입니다.");
				}
				else if ((moveX == 15 && moveY == 27) || (moveX == 17 && moveY == 26))
					ShowGroceryMenu();
				else if ((moveX == 31 && moveY == 26) || (moveX == 33 && moveY == 28))
					ShowHospitalMenu();
				else if (moveX == 17 && moveY == 17) {
					Talk(new string[] {
						$" 안녕하시오, {mPlayerList[0].Name}.",
						$" 나는  나의 게임속의 버그를 찾거나 난이도를 조절하기 위해서  항상 게임속을 떠도는 이 게임의 제작자 [color={RGB.Yellow}]안 영기[/color]라는 사람이오." +
						" 때로는 로어성의 병사로  때로는  다른 종족의 주민으로 계속 변장하며 이곳 저곳을 떠돌아 다닌다오.",
						" 당신은 벌써 이 게임의 반 이상을 했군요. 나는 당신에게 많은 것을 바라지는 않는다오." +
						" 다만, 내가 이 게임을 통해 말하고자하는 현실적인 주제를  알아 주었으면 하는 바램이 있을뿐이라오."
					}, SpecialEventType.MeetAhnYoungKi);
				}
				else if (moveX == 15 && moveY == 19) {
					Dialog(" 안 영기님이 그러시던데  '또 다른 지식의 성전'은 원래 멀티 엔딩이 아니었고" +
					"  '다크 메이지 실리안 카미너스'는  군입대 시간이 급박해서 멀티 엔딩을 만들지 못했다고 하는데  이번 게임은 확실한 멀티 엔딩이라더군요.");
				}
				else if (moveX == 32 && moveY == 17) {
					Dialog(" 저는  예언자로서의 능력을 키우는 중입니다. 저의 예언 능력은 아직 미숙하지만  당신의 미래가 조금은 옅보입니다." +
					"  당신은 머지않아 중요한 선택의 순간을 맞게 될 것입니다. 그때의 선택에 따라  당신은 큰 운명의 산맥을 넘어야 할 것입니다.");
				}
				else if (moveX == 32 && moveY == 19) {
					Dialog(" 이곳에는  안 영기님이 만든 게임들이 보관되어 있습니다." +
					"  첫번째로 만든 '3D 건담'이라는 시뮬레이션 게임의 APPLE SOFT BASIC 과 65C02 기계어 소스 프로그램부터" +
					"  13번째 게임인 '또 다른 지식의 성전'과  16번째 게임인 '다크 메이지 실리안 카미너스', 또 17번째 게임인 '비전 속으로'와 같은 게임이 소스와 함께 보관되어 있습니다.");
				}
				else if ((moveX == 21 && moveY == 16) || (moveX == 27 && moveY == 17)) {
					Dialog(" 죄송합니다.  프로그래밍 시간 관계상 제작자가 저희 가게를 프로그래밍 하지 못했군요. 그래서 저희 가게는 운영되지 않습니다.");
				}
				else if (moveX == 23 && moveY == 28) {
					Dialog(" 이 안은 '비전의 지식'이라는 신전이오. 에인션트 이블님의 의지가 잠든 곳이기도 하지요.");
				}
				else if (moveX == 25 && moveY == 28) {
					Dialog(" 이 안의 두 사람은 에인션트 이블님의 의지를 깨닳은 사람들이지요.  당신에게  결코 거짓말 따위는 하지 않을 거예요.");
				}
				else if (moveX == 21 && moveY == 23) {
					Talk(new string[] {
					" 에인션트 이블님은  예전에 아주 덕이 높은 현자였었지요.  또한 로드안과도 무척 친한 사이였어요. 수백년전에 로드안이 선이라는 전제를 걸고 이 세계를 통치 했었지요." +
					" 로드안의 훌륭한 정치에  모든 대륙은 평화를 누리게 되었고 그게 너무 만연된 나머지  현재의 평화를 아주 당연시 해버리고는 그것을 지키려는 생각은 접어둔채" +
					"  항상 남에 의해 평화를 보장 받으려는 사상이 팽배해졌죠." +
					"  후손들은 급기야  자신의 조상이 흘린  피의 보답으로  이렇게 평화로운 세상이 생겨났다는 것을 까맣게 잊어버리기 시작했고 마침내 로드안은 그런 사람들에게 경각심을 부추겨 보자는 의도로" +
					"  새로운 개념을 도입했죠.  그것이 바로 그대들이 '악'이라고 배운 것들이예요.  그때 로드안의 추상적인 '악'의 개념을  구체적인 '악'으로 만들기 위해 그 상징이 되어줄 사람을  수소문했죠.",
					"  로드안의 친구 대부분은  그의 뜻에 찬성헀지만  아무도 자신이 '악'을 대표하는 그런 역을 맡아  남들로부터 비난받고 저주받는 역을 하려고는 하지 않았죠." +
					" 그때 나타난 사람이 바로 에인션트 이블님이예요.  그분은 여태껏 지녔던 훌륭한 명성들을 버려둔채 악의 집대성으로 군림했지요. 본명 또한 버리고 지금의 이름을 사용하며  로드안의 일에 적극 동참했어요." +
					" 그의 존재 때문에  로어 세계의 사람들은  악으로부터 자신과 가족을 지키기 위해  스스로 선을 자각하고 현재의 평화에 감사하며  그 평화를 유지하려 노력하는 자세를 가지게 된거죠." +
					"  로드안은 사람들의 경각심을 더 부추기기 위해  지금 에인션트 이블이 여러 곳에서 이런 저런 구체적인 악행을 일삼고 있다고 거짓으로 소문을 퍼트리기 시작했고" +
					"  어릴때부터 교육과정에 그런 사상을 심어 넣어  어른이 되어서도 선을 지켜야 한다는 개념이 없어지지 않도록 해왔어요."
					}, SpecialEventType.None);
				}
				else if (moveX == 26 && moveY == 25) {
					Talk(new string[] {
					" 로드안은 선에 대한 너무 강한 집착 때문에 중대한 실수를 저질렀죠. 바로 당신을 시켜서 오크, 트롤, 코볼트, 드라코니안  이  네 종족을 멸망시킨게 그 실수예요." +
					"  로드안은 인간을 이롭게 하고자 아프로디테 테라의 트롤족을 몰아내고  그곳에 베스퍼성이란 전초기지를 건설했죠." +
					" 그곳은 인간에게 대단히 유용한 자원을 조달했고 그로인해 로드안의 평은 상당히 높아졌어요." +
					" 그리고 최근에는 다시 마지막 남은 트롤성을 공격하여 아프로디테 테라를 완전히 인간의 것으로 만들자는 계획을 세우고 베스퍼성의 주민들에게 명령했죠.",
					" 베스퍼성 사람들은 헤로인이란 마약을 트롤성으로 반입해서  트롤주민들에게 팔았어요. 즉, 수입도 올리고 트롤족을 마약 중독자로 만들어 마약 공급원인 베스퍼성 주민들에게 복종하게 하려는 의도였어요." +
					" 또한 베스퍼성의 강경파 세력은 수시로 트롤족의 동굴에 침입하여 그들을 괴롭히기 시작했고 마침내 대대적인 침략을 시작했지요." +
					"  트롤성을 침략한 이들은 정말 잔혹한 수법으로  트롤의 희생자를 처리하여 그들의 사기를 떨어뜨리려 했지만 결국 효과는 반대로 나타나서 더욱 더 트롤족을 분노에 떨게 했죠." +
					" 그리고는 도리어 그들에게 야습을 당해서 베스퍼성은 삽시간에 아수라장이 되고  급기야 모든 주민이 희생을 당하게 되었던 거예요.",
					" 이 일이 있은 후 로드안은 로어성의 주민들에게 편파적으로 소식을 알렸죠.  자신의 잘못은 은폐한채  베스퍼성이 당한 결과만을 바탕으로 트롤족에 대한 적개심을 부추겼죠." +
					"  그리고 그것을  다른 종족에 까지 확산시켜  지금에까지 이르렀던 거예요. 당신이 한 일 때문에 인간은 로어 세계에 있는 5개의 대륙 모두를  손에 넣게 되었죠." +
					"  악에 대한 정벌이란  거창한 목표 뒤에는 인간의 영토를 넓히겠다는 정복자의 야욕이 숨어있엇다는 것을 알아야 될거예요." +
					"  그 야욕 때문에  죄없는  수많은 타종족 주민들이 악인으로 몰려 비참한 최후를 맞이하게된 것을 당신도 인정하지요?",
					" 인간의 입장에서 생각할 때는 물론 당신은 영웅이죠. 하지만 모든 종족의 관점에서 본 당신은 비정한 정복자의 꼭두각시일뿐이예요." +
					" 나의 말을 깊이 새기고, 머지않은 미래에 있을 중요한 선택에 현명한 판단을 하기 바라겠어요."
					}, SpecialEventType.None);
				}
			}
			else if (mMapName == "Dome") {
				if (moveX == 25 && moveY == 13) {
					Dialog(" 저는 카미너스라는 그때 그 사람입니다. 우리는  이 마을을  '빛의 지붕'이라고 명명했습니다. '빛'은 정의를 뜻하고 '지붕'은 그것을 지키는 것 또는 지키는 사람이란 뜻입니다." +
					"  즉, 정의를 지키는 사람들의 마을이란 뜻입니다.");
				}
				else if (moveX == 12 && moveY == 26) {
					Dialog(" 당신은 우리들의 생명의 은인입니다. 또한 새로운 로드안이 되실  충분한 가능성을 갖고 계신분입니다.");
				}
				else if (moveX == 21 && moveY == 34) {
					Dialog(" 당신의 용기있는 결단에 정말 감격했습니다.");
				}
				else if (moveX == 38 && moveY == 28) {
					Dialog(" 당신과 에인션트 이블이 정말 진정한 선의 수호자입니다.");
				}
				else if (moveX == 18 && moveY == 53) {
					Dialog(" 당신은 비록 과거에 로드안의 심복으로써  큰 실수를 저질렀지만 이제는 명실공히 이런 난세의 구세주입니다.");
				}
				else if (moveX == 29 && moveY == 62) {
					Dialog(" 우리들은 투표로 이곳의 지도자를 선출했습니다. 그는 바로 우리들을 일어서게한 용감한 사람, 바로 카미너스씨입니다.");
				}
				else if (moveX == 18 && moveY == 72) {
					Dialog(" 이곳 사람들은  모두 당신에게 고마와하고 있고  당신이 우리가 못다한 일을 해줄거라 믿고 있습니다.");
				}
				else if (moveX == 37 && moveY == 40) {
					if (!GetBit(202)) {
						Dialog($" 당신의 용기에대한  나의 작은 정성이오.  자 이걸 받으시오. 우리 조상 대대로 내려오던 [color={RGB.LightCyan}]에보니 크리스탈[/color]이란거요.");
						mParty.Crystal[3]++;
						SetBit(202);
					}
					else {
						Dialog(" 상당히 귀한 물건이니 조심해서 간직하시오.");
					}
					
				}
			}
			else if (mMapName == "Dark") {
				if ((moveX == 86 && moveY == 72) || (moveX == 93 && moveY == 67) || (moveX == 90 && moveY == 64))
					ShowGroceryMenu();
				else if ((moveX == 7 && moveY == 70) || (moveX == 13 && moveY == 68) || (moveX == 13 && moveY == 72))
					ShowWeaponShopMenu();
				else if ((moveX == 85 && moveY == 11) || (moveX == 86 && moveY == 13))
					ShowHospitalMenu();
				else if (moveX == 82 && moveY == 15)
					ShowMedicineStoreMenu();
				else if ((moveX == 20 && moveY == 11) || (moveX == 24 && moveY == 12))
					ShowClassTrainingMenu();
				else if (moveX == 27 && moveY == 11)
					ShowExpStoreMenu();
				else if (moveX == 18 && moveY == 52)
					Dialog($"[color={RGB.Yellow}] 이 세계의 창시자는 안영기님이시며 그는 위대한 프로그래머입니다.[/color]");
				else if (moveX == 12 && moveY == 54)
					Dialog(" 이십 몇년 전에  이곳은  네크로만서란 자에 의해 큰 혼란이 있었습니다.");
				else if (moveX == 23 && moveY == 49)
					Dialog(" 안녕하시오.");
				else if ((moveX == 12 && moveY == 26) || (moveX == 17 && moveY == 26))
					Dialog(" 여기는 코리아 위스키가 제일 잘 팔립니다.");
				else if (moveX == 20 && moveY == 32)
					Dialog(" 요즈음은 너무 할 일이 없어서 따분합니다.");
				else if (moveX == 17 && moveY == 32)
					Dialog(" 당신  정말 이상한 차림이군요.  거의 100년 전에나 입던 옷인데...");
				else if (moveX == 12 && moveY == 31)
					Dialog(" 킥킥.. 정말 우스운 복장을 하고 있군요.");
				else if (moveX == 17 && moveY == 37)
					Dialog(" 요새는 너무 따분한 세상의 연속이에요.");
				else if ((moveX == 49 && moveY == 10) || (moveX == 52 && moveY == 10))
				{
					Dialog(" 여태껏 근 20년간  아무도 이곳에 갇히지 않았죠. 그래서 저는 너무 할 일이 없습니다.");
				}
				else if (moveX == 71 && moveY == 77)
					Dialog(" 여기는 로어성의 묘지입니다. 함부로 들어가지 마십시요.");
				else if (moveX == 76 && moveY == 45)
					Dialog(" 메너스는 폐광이 된지 50년이 넘었죠.");
				else if (moveX == 82 && moveY == 58)
					Dialog(" 라스트디치는 주민 등록을 마친 자만이 들어 갈 수있죠.");
				else if (moveX == 82 && moveY == 24)
					Dialog(" 20년 전에 알비레오라는 사람이 이상한 예언서를 남겼죠. 거기서 다크 메이지라는 사람이 나타난다고 했었는데 실제로는 나타나지 않았어요.");
				else if (moveX == 67 && moveY == 60)
					Dialog(" 당신들의 옷은 정말 특이하군요. 100년전 사람 같아요.");
				else if ((moveX == 49 && moveY == 50) || (moveX == 51 && moveY == 50))
					Dialog(" 안녕하시오.");
				else if (47 <= mXAxis && mXAxis <= 53 && 30 <= mYAxis && mYAxis <= 36)
					Dialog(" ...");
				else if (moveX == 50 && moveY == 27) {
					Talk(new string[] {
						$"[color={RGB.Brown}] 로드안!! 당신은 나를 기억하겠소?  100년전 쯤에  나를 이용하여  무고한 네 종족을 멸망시키게 만들었던것도 말이오.[/color]",
						"",
						" 당신의 말이 채 끝나기도 전에  로드안은 다급하게 외쳤다.",
						"",
						$"[color={RGB.LightBlue}] 경호원, 경호원 !![/color]",
						"",
						" 순간 몇명의 용사가 주위에서 나와 로드안을 감쌌다."
					}, SpecialEventType.BattleOldLordAhn);
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
						" 이로 인해  인간 이외의 다른 종족에 대한 호의를 일체 중지하며 이제는 적대 관계로 그들을 대할 것이다.[/color]", true);
					}
				}
			}
			else if (mMapName == "Dark") {
				if (x == 50 && y == 83) {
					Dialog(new string[] {
						$"[color={RGB.White}]          이곳은 로어성입니다.[/color]",
						$"[color={RGB.White}]          여러분을 환영합니다.[/color]",
						"",
						"",
						$"[color={RGB.LightGreen}]         이곳의 성주 로드안 씀[/color]"
					}, true);
				}
				else if ((x == 50 && y == 17) || (x == 51 && y == 17))
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
			else if (mMapName == "LastDtch") {
				if (x == 38 && y == 7)
					Dialog($"[color={RGB.White}] 이곳은 가이아 테라로 통하는 게이트 입니다.[/color]", true);
				else if (x == 38 && y == 67)
				{
					Dialog(new string[] {
						"",
						$"[color={RGB.White}] 선을 위해서라면 어떤 위험이나 어려움에도 굴하지 않는 사람들이 있는 곳, 라스트 디치.[/color]",
						"",
						$"[color={RGB.White}]           여러분을 환영합니다.[/color]",
						"",
						"",
						$"[color={RGB.LightMagenta}]            이곳의 성주로부터[/color]"
					}, true);
				}
			}
			else if (mMapName == "Valiant" || mMapName == "Light") {
				if (x == 38 && y == 7)
					Dialog($"[color={RGB.White}] 이곳은 로어 대륙으로 통하는 게이트 입니다.[/color]", true);
				else if (x == 38 && y == 67)
				{
					Dialog(new string[] {
						"",
						$"[color={RGB.White}]   여기는 가이아 테라의 배리언트 피플즈[/color]",
						$"[color={RGB.White}]  불의에 대해 가장 강력한 저항을 하는 곳[/color]",
						"",
						$"[color={RGB.White}]           여러분을 환영합니다.[/color]",
						"",
						"",
						$"[color={RGB.LightMagenta}]            이곳의 성주로부터[/color]"
					}, true);
				}
			}
			else if (mMapName == "Gaea") {
				if (x == 23 && y == 25) {
					Dialog(new string[] {
						"",
						$"[color={RGB.White}]        가이아 테라성의 보물창고[/color]",
						$"[color={RGB.White}]            훔쳐가면 죽어 ~~~[/color]"
					}, true);
				}
				else if (x == 28 && y == 25) {
					Dialog(new string[] {
						"",
						$"[color={RGB.White}]   가이아 테라성에 오신 것을 환영합니다[/color]",
						$"[color={RGB.White}]    이곳은 쓰레기 종량제 시범 성입니다[/color]",
						"",
						$"[color={RGB.White}]           여러분을 환영합니다.[/color]",
						"",
						"",
						$"[color={RGB.LightMagenta}]            이곳의 성주로부터[/color]"
					}, true);
				}
			}
			else if (mMapName == "Ground3") {
				Dialog(new string[] {
					"",
					"",
					$"[color={RGB.White}]   이 광산은 아직 개발 중이라 위험하므로 출입을 금지함[/color]"
				}, true);
			}
			else if (mMapName == "Kobold") {
				if (x == 8 && y == 29) {
					Dialog(new string[] {
						"",
						"",
						$"[color={RGB.LightGreen}] 여기서는 믿을만한 기술이[/color] [color={RGB.LightCyan}]투시[/color][color={RGB.LightGreen}]밖에는 없소[/color]",
						$"[color={RGB.LightGreen}]    상당히 난해한 곳이 많으니 주의하시오[/color]",
						"",
						"",
						"",
						$"[color={RGB.LightMagenta}]                  로드안[/color]"
					}, true);
				}
				else if (x == 53 && y == 62) {
					Ask(new string[] {
						"푯말에 쓰여있기로 ...",
						"",
						"",
						"",
						"  당신이 원한다면",
						"         당신 앞의 용암을 건너게 해주겠소"
					}, MenuMode.CrossLava, new string[] {
						"     용암을 건너겠다",
						"     건널 필요는 없다"
					});
				}
			}
			else if (mMapName == "DracTown") {
				if (x == 91 && y == 93) {
					Dialog(new string[] {
						"",
						"",
						$"[color={RGB.White}]     이곳은 신성한 드라코니안 묘지이므로 함부로 회손 시키지 마시오[/color]"
					}, true);
				}
			}
			else if (mMapName == "Imperium") {
				if ((x == 23 && y == 18) || (x == 25 && y == 18)) {
					Dialog(new string[] {
						"",
						"",
						$"[color={RGB.LightCyan}]                크리스탈 가게[/color]",
						$"[color={RGB.White}]                Crystal  Shop[/color]",
						"      ( 국제화시대에 부응하기 위해 썼음 )"
					}, true);
				}
				else if (x == 33 && y == 22) {
					Dialog(new string[] {
						"",
						"",
						$"[color={RGB.LightCyan}]                도서관[/color]"
					}, true);
				}
			}
			else if (mMapName == "HdsGate") {
				if (x == 12 && y == 21) {
					Dialog(new string[] {
						$"[color={RGB.White}] 진정한 길은 마음으로 보아야한다[/color]",
						$"[color={RGB.White}] 당신 주위에 있는 인위적인 시각 요소를 배제하라[/color]",
						$"[color={RGB.White}] 그리고 다시 눈을 떴을때 이미 길은 당신 앞에 펼쳐져 있을 것이다[/color]",
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
				mBackupPlayerList = new List<Lore>();
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

					
					int attackPoint;
					if (player == mAssistPlayer)
						attackPoint = (int)Math.Round((double)player.Strength * player.WeaPower * player.Level / 2);
					else
					{
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

						if ((player.ClassType == ClassCategory.Sword) && (player.Class == 5 || player.Class == 6))
							attackPoint = player.Strength * power * 5;
						else
							attackPoint = (int)Math.Round((double)player.Strength * player.WeaPower * power / 10);
					}

					attackPoint -= attackPoint * mRand.Next(50) / 100;

					if (mRand.Next(100) < enemy.Resistance[0])
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
						battleResult.Add($"적은 [color={RGB.White}]{attackPoint:#,#0}[/color]만큼의 피해를 입었다");
					}
				}

				void CastOne()
				{
					var enemy = GetDestEnemy();
					if (enemy == null)
						return;

					var player = battleCommand.Player;

					if (battleCommand.Tool < 11)
						GetBattleStatus(enemy);
					else if (battleCommand.Method == 1)
						battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} 화염의 크리스탈을 사용했고, 거기서 생성된 화염폭풍은 {enemy.Name}에게 명중했다.[/color]");
					else
						battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} 크리스탈로 생성시킨 한파의 회오리는 {enemy.NameJosa} 강타했다.[/color]");

					if (enemy.Unconscious)
					{
						battleResult.Add($"[color={RGB.LightRed}]{player.GenderPronoun}의 마법은 적을 완전히 제거해 버렸다[/color]");

						PlusExperience(enemy);
						enemy.HP = 0;
						enemy.Dead = true;
						DisplayEnemy();
						return;
					}

					int magicPoint;

					if (battleCommand.Tool < 11)
					{
						var needSP = (int)Math.Round((double)battleCommand.Player.AttackMagic * battleCommand.Tool * battleCommand.Tool / 10);
						if (battleCommand.Player.SP < needSP)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

#if DEBUG
						battleCommand.Player.SP -= 1;
#else
						battleCommand.Player.SP -= needSP;
#endif

						DisplaySP();

						if (mRand.Next(20) >= player.Accuracy)
						{
							battleResult.Add($"그러나, {enemy.NameJosa} 빗나갔다");
							return;
						}

						magicPoint = battleCommand.Tool * battleCommand.Tool * player.AttackMagic * 3;
					}
					else if (battleCommand.Method == 1)
						magicPoint = 32_500;
					else
						magicPoint = 20_000;

					if (mRand.Next(100) < enemy.Resistance[1])
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
						battleResult.Add($"{enemy.NameSubjectJosa} [color={RGB.White}]{magicPoint:#,#0}[/color]만큼의 피해를 입었다");
					}
				}

				void CastESP()
				{
					var player = battleCommand.Player;

					if (battleCommand.Tool == 3)
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

						if (!(5 <= enemy.ENumber && enemy.ENumber <= 8) &&
							!(10 <= enemy.ENumber && enemy.ENumber <= 15) && 
							enemy.ENumber != 17 &&
							enemy.ENumber != 18 &&
							enemy.ENumber != 24 &&
							enemy.ENumber != 25 &&
							enemy.ENumber != 46 &&
							enemy.ENumber != 63)
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
					else if (battleCommand.Tool == 6)
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
					else if (battleCommand.Tool == 7)
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

						if (mRand.Next(40) < enemy.Resistance[1])
						{
							if (enemy.Resistance[0] < 5)
								enemy.Resistance[0] = 0;
							else
								enemy.Resistance[0] -= 5;

							if (enemy.Resistance[1] < 5)
								enemy.Resistance[1] = 0;
							else
								enemy.Resistance[1] -= 5;

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
						PlusExperience(enemy);
					}
					else if (battleCommand.Tool == 8)
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

						if (mRand.Next(40) < enemy.Resistance[1])
						{
							if (enemy.Agility < 5)
								enemy.Agility = 0;
							else
								enemy.Agility -= 5;
							return;
						}
					}
					else if (battleCommand.Tool == 9)
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
							if (enemy.HP > 5_000)
								enemy.HP -= 5_000;
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
					else if (battleCommand.Tool == 10)
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

						if (mRand.Next(40) < enemy.Resistance[1])
						{
							if (enemy.Resistance[0] < 20)
								enemy.Resistance[0] = 0;
							else
								enemy.Resistance[0] -= 20;

							if (enemy.Resistance[1] < 20)
								enemy.Resistance[1] = 0;
							else
								enemy.Resistance[1] -= 20;

							return;
						}

						if (mRand.Next(80) > player.Concentration)
						{
							if (enemy.HP < 5_000)
							{
								enemy.HP = 0;
								enemy.Unconscious = true;
								PlusExperience(enemy);
							}
							else
								enemy.HP -= 5_000;

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

						if (mRand.Next(100) < enemy.Resistance[1])
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

						if (mRand.Next(100) < enemy.Resistance[0])
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
						if ((enemy.Resistance[0] < 31 || mRand.Next(2) == 0) && enemy.AC > 0)
							enemy.AC--;

						if (enemy.Resistance[0] > 10)
							enemy.Resistance[0] -= 10;
						else
							enemy.Resistance[0] = 0;

						if (enemy.Resistance[1] > 10)
							enemy.Resistance[1] -= 10;
						else
							enemy.Resistance[1] = 0;
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

						if (mRand.Next(200) < enemy.Resistance[1])
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

						if (enemy.Resistance[0] > 10)
							enemy.Resistance[0] -= 10;
						else
							enemy.Resistance[0] = 0;

						if (enemy.Resistance[1] > 10)
							enemy.Resistance[1] -= 10;
						else
							enemy.Resistance[1] = 0;
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

						if (mRand.Next(100) < enemy.Resistance[1])
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

						if (mRand.Next(100) < enemy.Resistance[1])
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
						{
							var levelPoint = 0;
							while ((enemy.SpecialCastLevel & (1 << levelPoint)) > 0 || levelPoint == 8) {
								levelPoint++;
							}

							if (levelPoint < 8)
								enemy.SpecialCastLevel ^= 1 << levelPoint;
						}
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
								mAssistPlayer.PotentialAC = 3;
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
							WeaPower = (int)Math.Round((double)battleCommand.Player.SummonMagic * (mRand.Next(5) + 1)),
							PotentialAC = 3,
							AC = 3,
							Gender = GenderType.Neutral,
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
						}

						mAssistPlayer.HP = mAssistPlayer.Endurance * mAssistPlayer.Level * 10;
					}
					else if (battleCommand.Tool == 10) {
						mAssistPlayer = new Lore()
						{
							Gender = GenderType.Neutral,
							Class = 0,
							Level = 30,
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

						switch (mRand.Next(3))
						{
							case 0:
								mAssistPlayer.Name = "크리스탈 드래곤";
								mAssistPlayer.ClassType = ClassCategory.Dragon;
								mAssistPlayer.Strength = 25;
								mAssistPlayer.Mentality = 20;
								mAssistPlayer.Concentration = 20;
								mAssistPlayer.Endurance = 30;
								mAssistPlayer.Resistance = 20;
								mAssistPlayer.Agility = 0;
								mAssistPlayer.Accuracy = 20;
								mAssistPlayer.Luck = 20;
								mAssistPlayer.Weapon = 49;
								mAssistPlayer.WeaPower = 255;
								mAssistPlayer.PotentialAC = 4;
								break;
							case 1:
							case 2:
								mAssistPlayer.Name = "크리스탈 고렘";
								mAssistPlayer.ClassType = ClassCategory.Golem;
								mAssistPlayer.Strength = 20;
								mAssistPlayer.Mentality = 0;
								mAssistPlayer.Concentration = 0;
								mAssistPlayer.Endurance = 40;
								mAssistPlayer.Resistance = 25;
								mAssistPlayer.Agility = 0;
								mAssistPlayer.Accuracy = 13;
								mAssistPlayer.Luck = 0;
								mAssistPlayer.Weapon = 0;
								mAssistPlayer.WeaPower = 150;
								mAssistPlayer.PotentialAC = 5;
								
								break;
						}

						mAssistPlayer.AC = mAssistPlayer.PotentialAC;
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

					bool noMoreAttack = false;
					if (enemy.SpecialCastLevel > 0 && enemy.ENumber > 0)
					{
						if ((enemy.SpecialCastLevel & 0x80) > 0)
						{
							if (mEncounterEnemyList.Count < 8)
							{
								foreach (var otherEnemy in mEncounterEnemyList)
								{
									if (otherEnemy != enemy)
									{
										otherEnemy.Name = enemy.Name;
										otherEnemy.ENumber = -1;
										otherEnemy.HP = 1;
										otherEnemy.AuxHP = 0;
									}
								}

								while (mEncounterEnemyList.Count < 8)
								{
									var otherEnemy = JoinEnemy(0);

									otherEnemy.ENumber = -1;
									otherEnemy.Name = enemy.Name;
									otherEnemy.ENumber = -1;
									otherEnemy.HP = 1;
									otherEnemy.AuxHP = 0;
								}
							}

							var moveID = mRand.Next(8);

							if (mEncounterEnemyList[moveID] != enemy)
							{
								mEncounterEnemyList[moveID] = enemy;
							}

							for (var i = 0; i < mEncounterEnemyList.Count; i++)
							{
								if (moveID != i)
								{
									mEncounterEnemyList[i].ENumber = -1;
									mEncounterEnemyList[i].HP = 1;
									mEncounterEnemyList[i].Posion = false;
									mEncounterEnemyList[i].Unconscious = false;
									mEncounterEnemyList[i].Dead = false;
									mEncounterEnemyList[i].Resistance[0] = 0;
									mEncounterEnemyList[i].Resistance[1] = 0;
									mEncounterEnemyList[i].AC = 0;
									mEncounterEnemyList[i].Level = 0;
								}
							}

							DisplayEnemy();
						}

						var specialAttackType = 0;
						for (var i = 0; i < 6; i++)
						{
							if ((enemy.SpecialCastLevel & (1 << i)) > 0)
								specialAttackType++;
						}

						if (specialAttackType > 0)
						{
							var method = 0;
							specialAttackType = mRand.Next(specialAttackType) + 1;

							var liveEnemyCount = 0;
							foreach (var otherEnemy in mEncounterEnemyList)
							{
								if (!otherEnemy.Dead)
									liveEnemyCount++;
							}

							var i = 0;
							do
							{
								if ((enemy.SpecialCastLevel & (1 << method)) > 0)
									i++;
								method++;
							} while (i == specialAttackType);

							if (method == 0)
							{
								if (mAssistPlayer != null && mAssistPlayer.Name[0] != '크' && mAssistPlayer.Name[1] != '대' && liveEnemyCount < 8 && (mRand.Next(5) == 0))
								{
									var turnEnemy = TurnMind(mAssistPlayer);
									mAssistPlayer = null;

									DisplayPlayerInfo();
									DisplayEnemy();

									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {turnEnemy.NameJosa} 자기편으로 끌어들였다[/color]");
								}
							}
							else if (method == 1)
							{
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
							}
							else if (method == 2)
							{
								if (liveEnemyCount < (mRand.Next(3) + 2))
								{
									var enemyNum = 0;
									if (enemy.ENumber == 28)
										enemyNum = mRand.Next(5);
									else if (enemy.ENumber == 41)
										enemyNum = 5 + mRand.Next(5);
									else if (enemy.ENumber == 44)
										enemyNum = 38 + mRand.Next(5);
									else if (enemy.ENumber == 51)
										enemyNum = 10 + mRand.Next(5);
									else if (enemy.ENumber == 58)
										enemyNum = 49;
									else if (72 <= enemy.ENumber && enemy.ENumber <= 73)
										enemyNum = 70;
									else
										enemyNum = mRand.Next(15);

									var newEnemy = JoinEnemy(enemyNum);
									DisplayEnemy();
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {newEnemy.NameJosa} 생성시켰다[/color]");
								}
							}
							else if (3 <= method && method <= 5)
							{
								var livePlayerCount = 0;
								foreach (var player in mPlayerList)
								{
									if (player.IsAvailable)
										livePlayerCount++;
								}

								if (mAssistPlayer != null && mAssistPlayer.IsAvailable)
									livePlayerCount++;

								if (livePlayerCount >= (mRand.Next(3) + 5))
								{
									void SubSpecialCastAttack(Lore player)
									{
										if (method == 3)
										{
											if (player.IsAvailable)
											{
												if (mRand.Next(40) > player.Luck)
												{
													player.Poison++;
													if (player.Poison == 0)
														player.Poison = 255;

													battleResult.Add($"[color={RGB.LightMagenta}]{player.NameSubjectJosa} {enemy.Name}에 의해 독에 감염 되었다.[/color]");
												}
											}
										}
										else if (method == 4)
										{
											if (player.Dead == 0 && player.Unconscious == 0 && mRand.Next(30) > player.Luck)
											{
												player.Unconscious = 1;

												battleResult.Add($"[color={RGB.LightMagenta}]{player.NameSubjectJosa} {enemy.Name}에 의해 의식불명이 되었다.[/color]");
											}
										}
										else if (method == 5)
										{
											if (player.Dead == 0 && mRand.Next(22) > player.Luck)
											{
												player.Dead = 1;

												battleResult.Add($"[color={RGB.LightMagenta}]{player.NameSubjectJosa} {enemy.Name}에 의해 급사 당했다.[/color]");
											}
										}
									}

									foreach (var player in mPlayerList)
										SubSpecialCastAttack(player);

									if (mAssistPlayer != null)
										SubSpecialCastAttack(mAssistPlayer);
								}
							}
							else if (method == 6)
							{
								foreach (var otherEnemy in mEncounterEnemyList)
								{
									CureEnemy(otherEnemy, enemy.Level * enemy.Mentality * 4);
								}

								if (enemy.ENumber == 4 || enemy.ENumber == 9 || enemy.ENumber == 46)
									noMoreAttack = true;
							}
						}
					}
				

					if (!noMoreAttack) {
						var enemyAgility = enemy.Agility;
						if (enemyAgility > 0)
							enemyAgility = 20;

						if (enemy.Special > 0 && mRand.Next(50) < enemyAgility) {
							var liveEnemyCount = 0;
							foreach (var otherEnemy in mEncounterEnemyList) {
								if (!otherEnemy.Unconscious && !otherEnemy.Dead)
									liveEnemyCount++;
							}

							if (liveEnemyCount > 3) {
								void SpecialAttack() {
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

								SpecialAttack();

								noMoreAttack = true;
							}
						}
					}


					if (!noMoreAttack)
					{
						if (mRand.Next(enemy.Accuracy[0] * 1_000) > mRand.Next(enemy.Accuracy[1] * 1_000) && enemy.Strength > 0 || enemy.CastLevel == 0)
						{
							void EnemyAttack() {
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
								battleResult.Add($"[color={RGB.Magenta}]{destPlayer.NameSubjectJosa}[/color] [color={RGB.LightMagenta}]{attackPoint:#,#0}[/color][color={RGB.Magenta}]만큼의 피해를 입었다[/color]");
							}

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
			mMapHeader = await LoadMapData();
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
			if (mMapHeader.TileType == PositionType.Town)
				musicUri = new Uri("ms-appx:///Assets/town.mp3");
			else if (mMapHeader.TileType == PositionType.Ground)
				musicUri = new Uri("ms-appx:///Assets/ground.mp3");
			else if (mMapHeader.TileType == PositionType.Den)
				musicUri = new Uri("ms-appx:///Assets/den.mp3");
			else
				musicUri = new Uri("ms-appx:///Assets/keep.mp3");

			if ((mMapHeader.StartX != 255 || mMapHeader.StartY != 255) && (mMapHeader.TileType != PositionType.Ground || mMapName == "UnderGrd")) {
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
			BGMPlayer.Source = musicUri;

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

		private async Task ViewCrystal(string mapName, int x, int y) {
			mCrystalX = x;
			mCrystalY = y;
			mCrystalMap = await LoadMapData(mapName);

			InvokeAnimation(AnimationType.ViewCrystal);
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


			if (mEbony && mParty.Etc[0] > 0) {
				InvokeAnimation(AnimationType.TurnOffTorch);
			}
			else if (mEbony && mParty.Etc[0] == 0) {
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
				Resistance = new int[] { player.Resistance * 5 > 99 ? 99 : player.Resistance * 5, player.Resistance * 5 > 99 ? 99 : player.Resistance * 5 },
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

			if (mAssistPlayer != null)
				HealOne(player, mAssistPlayer, cureResult);
		}

		private void CureAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				CureOne(player, whomPlayer, cureResult);
			});

			if (mAssistPlayer != null)
				CureOne(player, mAssistPlayer, cureResult);
		}

		private void ConsciousAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				ConsciousOne(player, whomPlayer, cureResult);
			});

			if (mAssistPlayer != null)
				ConsciousOne(player, mAssistPlayer, cureResult);
		}

		private void RevitalizeAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				RevitalizeOne(player, whomPlayer, cureResult);
			});

			if (mAssistPlayer != null)
				RevitalizeOne(player, mAssistPlayer, cureResult);
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
			mMapHeader = await LoadMapData();
			InitializeMap();

			mLoading = false;
		}

		private async Task<MapHeader> LoadMapData(string mapName = "")
		{
			if (mapName == "")
				mapName = mMapName.ToUpper();

			var mapFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/{mapName.ToUpper()}.M"));
			var stream = (await mapFile.OpenReadAsync()).AsStreamForRead();
			var reader = new BinaryReader(stream);

			
			var mapHeader = new MapHeader();

			var mapNameLen = reader.ReadByte();
			mapHeader.ID = Encoding.UTF8.GetString(reader.ReadBytes(mapNameLen));

			if (10 - mapNameLen > 0)
				reader.ReadBytes(10 - mapNameLen);
				
			mapHeader.Width = reader.ReadByte();
			mapHeader.Height = reader.ReadByte();

			switch (reader.ReadByte()) {
				case 0:
					mapHeader.TileType = PositionType.Town;
					break;
				case 1:
					mapHeader.TileType = PositionType.Ground;
					break;
				case 2:
					mapHeader.TileType = PositionType.Den;
					break;
				default:
					mapHeader.TileType = PositionType.Keep;
					break;
			}

			if (reader.ReadByte() == 0)
				mapHeader.Encounter = false;
			else
				mapHeader.Encounter = true;


			if (reader.ReadByte() == 0)
				mapHeader.Handicap = false;
			else
				mapHeader.Handicap = true;

			mapHeader.StartX = reader.ReadByte() - 1;
			mapHeader.StartY = reader.ReadByte() - 1;

			var exitMapNameLen = reader.ReadByte();
			if (exitMapNameLen > 0)
				mapHeader.ExitMap = Encoding.UTF8.GetString(reader.ReadBytes(exitMapNameLen));
			else
				mapHeader.ExitMap = "";

			if (10 - exitMapNameLen > 0)
				reader.ReadBytes(10 - exitMapNameLen);

			mapHeader.ExitX = reader.ReadByte() - 1;
			mapHeader.ExitY = reader.ReadByte() - 1;

			var enterMapNameLen = reader.ReadByte();
			if (enterMapNameLen > 0)
				mapHeader.EnterMap = Encoding.UTF8.GetString(reader.ReadBytes(enterMapNameLen));
			else
				mapHeader.EnterMap = "";

			if (10 - enterMapNameLen > 0)
				reader.ReadBytes(10 - enterMapNameLen);

			mapHeader.EnterX = reader.ReadByte() - 1;
			mapHeader.EnterY = reader.ReadByte() - 1;
			mapHeader.Default = reader.ReadByte();
			mapHeader.HandicapBit = reader.ReadByte();

			reader.ReadBytes(50);

			mapHeader.Layer = reader.ReadBytes(mapHeader.Width * mapHeader.Height);

			return mapHeader;
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

			if (ItemInfoPanel.Visibility == Visibility.Visible)
				ItemInfoPanel.Visibility = Visibility.Collapsed;

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
				mDecorateTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/decorate.png"), new Vector2(52, 52), Vector2.Zero);

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
			mBackupPlayerList = saveData.BackupPlayerList;
			mAssistPlayer = saveData.AssistPlayer;
			mMapHeader = saveData.MapHeader;
			mMapName = mMapHeader.ID;

			mXAxis = mMapHeader.StartX;
			mYAxis = mMapHeader.StartY;

			if (saveData.MapHeader.Layer == null || saveData.MapHeader.Layer.Length == 0)
			{
				mMapHeader = await LoadMapData();
			}

			mEncounter = saveData.Encounter;
			if (1 > mEncounter || mEncounter > 3)
				mEncounter = 2;

			mMaxEnemy = saveData.MaxEnemy;
			if (3 > mMaxEnemy || mMaxEnemy > 7)
				mMaxEnemy = 5;

			mEbony = saveData.Ebony;
			mMoonLight = saveData.MoonLight;
			mCruel = saveData.Cruel;
			mWatchYear = saveData.WatchYear;
			mWatchDay = saveData.WatchDay;
			mWatchHour = saveData.WatchHour;
			mWatchMin = saveData.WatchMin;
			mWatchSec = saveData.WatchSec;
			mTimeWatch = saveData.TimeWatch;
			mTimeEvent = saveData.TimeEvent;
			
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

				void AnimateFadeInOut()
				{
					for (var i = 1; i <= 10; i++)
					{
						mAnimationFrame = i;
						Task.Delay(100).Wait();
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
					for (var i = 1; i <= 4; i++)
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
					mAnimationFrame = 1;
					Task.Delay(1000).Wait();
					mAnimationFrame = 2;
				}
				else if (mAnimationEvent == AnimationType.TransformProtagonist)
					Task.Delay(3000).Wait();
				else if (mAnimationEvent == AnimationType.TransformProtagonist2)
				{
					mAnimationFrame = 1;
					Task.Delay(2000).Wait();
					mAnimationFrame = 2;
				}
				else if (mAnimationEvent == AnimationType.ComeSoldier)
				{
					for (var i = 1; i <= 3; i++)
					{
						mAnimationFrame = i;
						Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.FollowSoldier)
				{
					for (var i = 1; i <= 5; i++)
					{
						mAnimationFrame = i;
						if (i == 1)
							Task.Delay(2000).Wait();
						else
							Task.Delay(800).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.FollowSoldier2)
				{
					AnimateTransition();

					Task.Delay(2000).Wait();

					mXAxis = 50;
					mYAxis = 30;

					mFace = 1;
					if (mPlayerList[0].ClassType == ClassCategory.Magic)
						mFace += 8;

					for (var i = 118; i <= 123; i++)
					{
						mAnimationFrame = i;
						Task.Delay(700).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.LeaveSoldier)
				{
					for (var i = 1; i <= 6; i++)
					{
						mAnimationFrame = i;
						Task.Delay(700).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.VisitCanopus)
				{
					for (var i = 1; i <= 3; i++)
					{
						mAnimationFrame = i;
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.RequestPardon)
				{
					for (var i = 1; i <= 4; i++)
					{
						mAnimationFrame = i;
						Task.Delay(1500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.ConfirmPardon)
				{
					for (var i = 1; i <= 4; i++)
					{
						mAnimationFrame = i;
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.ConfirmPardon2)
				{
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
				else if (mAnimationEvent == AnimationType.JoinCanopus)
				{
					for (var i = 1; i <= 3; i++)
					{
						mAnimationFrame = i;

						if (i < 3)
							Task.Delay(1000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.LeavePrisonSoldier)
				{
					for (var i = 1; i <= 4; i++)
					{
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
				else if (mAnimationEvent == AnimationType.LearnOrcWriting)
				{
					for (var i = 1; i <= 10; i++)
					{
						mAnimationFrame = i;
						Task.Delay(100).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.CompleteLearnOrcWriting)
				{
					for (var i = 1; i <= 10; i++)
					{
						mAnimationFrame = i;
						Task.Delay(100).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.LearnOrcSpeaking)
				{
					Task.Delay(2_000).Wait();
				}
				else if (mAnimationEvent == AnimationType.LearnOrcSpeaking2)
				{
					AnimateTransition();
				}
				else if (mAnimationEvent == AnimationType.MoveGround2 ||
				mAnimationEvent == AnimationType.MoveGround3 ||
				mAnimationEvent == AnimationType.MoveGround4 ||
				mAnimationEvent == AnimationType.MoveGaeaTerraCastle)
				{
					AnimateFadeInOut();
				}
				else if (mAnimationEvent == AnimationType.InvestigateDeadBody)
				{
					Task.Delay(1000).Wait();
				}
				else if (mAnimationEvent == AnimationType.UseHerbOfRessurection)
				{
					Task.Delay(1500).Wait();
				}
				else if (mAnimationEvent == AnimationType.LearnTrollWriting)
				{
					AnimateTransition();
				}
				else if (mAnimationEvent == AnimationType.LearnKoboldWriting)
				{
					AnimateFadeInOut();
				}
				else if (mAnimationEvent == AnimationType.CompleteLearnKoboldWriting)
				{
					AnimateFadeInOut();
				}
				else if (mAnimationEvent == AnimationType.TurnOffTorch)
					Task.Delay(250).Wait();
				else if (mAnimationEvent == AnimationType.SendValiantToUranos)
					AnimateTransition();
				else if (mAnimationEvent == AnimationType.LandUranos)
					AnimateFadeInOut();
				else if (mAnimationEvent == AnimationType.TranformDraconianKing)
				{
					mAnimationFrame = 1;
					Task.Delay(500).Wait();
					mAnimationFrame = 2;
					Task.Delay(200).Wait();
					mAnimationFrame = 3;
					Task.Delay(300).Wait();
					mAnimationFrame = 4;
					Task.Delay(1500).Wait();
					mAnimationFrame = 5;
				}
				else if (mAnimationEvent == AnimationType.MeetCaminus)
				{
					for (var i = 1; i <= 5; i++)
					{
						mAnimationFrame = i;
						if (i < 5)
							Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.ViewCrystal)
				{
					AnimateFadeInOut();
				}
				else if (mAnimationEvent == AnimationType.ExitCrystal)
				{
					AnimateFadeInOut();
				}
				else if (mAnimationEvent == AnimationType.Ending1Cookie1) {
					AnimateFadeInOut();
				}
				else if (mAnimationEvent == AnimationType.Ending1Cookie2)
				{
					AnimateFadeInOut();
					while (mXAxis != 45 || mYAxis != 47)
					{
						mXAxis += Math.Sign(45 - mXAxis);
						mYAxis += Math.Sign(47 - mYAxis);
						Task.Delay(300).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.Ending1Cookie3) {
					AnimateFadeInOut();

					while (mXAxis != 25 || mYAxis != 13)
					{
						mXAxis += Math.Sign(25 - mXAxis);
						mYAxis += Math.Sign(13 - mYAxis);
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.Ending1Cookie4) {
					mAnimationFrame = 1;
					Task.Delay(5000).Wait();
					for (mYAxis = 14; mYAxis < 17; mYAxis++) {
						mAnimationFrame++;
						Task.Delay(1000).Wait();
					}
					Task.Delay(4000).Wait();
					for (var i = 0; i < 3; i++)
					{
						mAnimationFrame++;
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.LeaveCaminus) {
					for (var i = 1; i <= 5; i++)
					{
						mAnimationFrame = i;
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.Ending2Cookie1)
				{
					AnimateFadeInOut();
				}
				else if (mAnimationEvent == AnimationType.Ending2Cookie2)
				{
					mAnimationFrame = 0;
					for (var i = 1; i <= 9; i++)
					{
						mAnimationFrame++;
						Task.Delay(800).Wait();

					}
				}
				else if (mAnimationEvent == AnimationType.Ending2Cookie3)
				{
					mAnimationFrame = 1;
					Task.Delay(1_500).Wait();
					for (var i = 2; i < 5; i++)
					{
						mAnimationFrame = i;
						Task.Delay(800).Wait();
					}
					mAnimationFrame = 5;
					Task.Delay(1_500).Wait();
					for (var i = 6; i < 11; i++) {
						mAnimationFrame = i;
						Task.Delay(800).Wait();
					}
					mAnimationFrame = 11;
				}
				else if (mAnimationEvent == AnimationType.Dying) {
					Task.Delay(1_000).Wait();
				}
				else if (mAnimationEvent == AnimationType.GoToFuture) {
					AnimateTransition();
				}
				else if (mAnimationEvent == AnimationType.Wakeup)
					AnimateFadeInOut();
			});

			await animationTask;

			if (mAnimationEvent == AnimationType.HearAncientEvil)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				mSpecialEvent = SpecialEventType.HearAncientEvil;
				ContinueText.Visibility = Visibility.Visible;
			}
			else if (mAnimationEvent == AnimationType.MeetFriend)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

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
			else if (mAnimationEvent == AnimationType.PassCrystal)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(new string[] {
				$"[color={RGB.LightGreen}] 오, {mPlayerList[0].Name} 공작![/color]",
				$"[color={RGB.LightGreen}] 당신은 나의 마지막 부탁까지 훌륭하게 들어주었소.  이제는 당신에게 더 이상 이런 일을 시키지 않겠다고 약속하오." +
				"  먼저, 네 종족의 원혼을 봉인한 크리스탈 볼을 이리 주시오.[/color]"
				}, SpecialEventType.PassCrystal);
			}
			else if (mAnimationEvent == AnimationType.PassCrystal2)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Dialog($"[color={RGB.LightGreen}] 이제는 이 수정구슬을 완전히 봉인해서 다시는 그들의 원혼이 우리를 위협하지 못하게 해야겠소.[/color]");
				InvokeAnimation(AnimationType.SealCrystal);
			}
			else if (mAnimationEvent == AnimationType.SealCrystal)
			{
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
			else if (mAnimationEvent == AnimationType.TransformProtagonist)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(new string[] {
					"",
					" 지금  주위에는 아무도 없다는 것을  확인한 로드안은  갑자기 당신을 향해 독심을 사용하기 시작했다." +
					"  당신은 긴장을 완전히 풀고 있었던데다가 로드안의 갑작스런 최면에 말려들어  방어를 할 기회도 없이  그에게 독심술을 당하였다." +
					" 당신은 곧 당신의 자유의사를 잃어버렸고 정신이 아득해지기 시작하였다."
				}, SpecialEventType.TransformProtagonist2, true);
			}
			else if (mAnimationEvent == AnimationType.TransformProtagonist2)
			{
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
			else if (mAnimationEvent == AnimationType.LeaveSoldier)
			{
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
			else if (mAnimationEvent == AnimationType.VisitCanopus)
			{
				Ask(new string[] {
					$"[color={RGB.LightBlue}] {mPlayerList[0].Name}.[/color]",
					$"[color={RGB.LightBlue}] 나도 역시 자네처럼 베스퍼성으로 가고 싶네. 이런 감옥에서 평생을 지내느니 한 번 모험을 해보는게 좋을 것 같아서 말이지." +
					"  나는 전사로서의 교육도 받았으니 아마 자네 일원 중에서는 가장 전투력이 뛰어날 걸세.[/color]"
				}, MenuMode.JoinCanopus, new string[] {
					"역시 카노푸스 자네 뿐이야",
					"자네 도움까지는 필요 없다네"
				});
			}
			else if (mAnimationEvent == AnimationType.RequestPardon)
			{
				Talk(" 그렇다면 카노푸스도 당신과 같은 목적을 가진다는 이유로  사면 시켜 달라는  애기군요. 그럼 로드안 님에게 물어보고 오겠습니다."
				, SpecialEventType.ConfirmPardon);
			}
			else if (mAnimationEvent == AnimationType.ConfirmPardon)
			{
				InvokeAnimation(AnimationType.ConfirmPardon2);
			}
			else if (mAnimationEvent == AnimationType.ConfirmPardon2)
			{
				Talk(" 그로부터 30분이 지났다.", SpecialEventType.ConfirmPardon2);
				PlusTime(0, 20, 0);
			}
			else if (mAnimationEvent == AnimationType.ConfirmPardon3)
			{
				Talk(" 카노푸스를 풀어줘도 좋다고  로드안님이 허락하셨습니다. 정말 희안한 일입니다. 예전에는 전혀 없던 일인데 ... 어쨌든 카노푸스는 이제 자유의 몸입니다."
				, SpecialEventType.JoinCanopus);
			}
			else if (mAnimationEvent == AnimationType.JoinCanopus)
			{
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
			else if (mAnimationEvent == AnimationType.LearnOrcWriting)
			{
				Talk(" 일주일이 경과했다.", SpecialEventType.LearnOrcWriting);
			}
			else if (mAnimationEvent == AnimationType.CompleteLearnOrcWriting)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				UpdateView();

				Dialog(new string[] {
								" 당신은 이제 오크족의 글을 읽을 수가 있소. 만약 당신이 오크족의 말도 배우고 싶다면  이 대륙의 남동쪽 섬으로 가시오.",
								" 그리고, 다른 세 종족의 말과 글을 배울 수 있는 길이 반드시 있을거요."
							});

				SetBit(8);
			}
			else if (mAnimationEvent == AnimationType.LearnOrcSpeaking)
			{
				InvokeAnimation(AnimationType.LearnOrcSpeaking2);
			}
			else if (mAnimationEvent == AnimationType.LearnOrcSpeaking2)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				mParty.Day++;
				if (mParty.Day > 360)
				{
					mParty.Year++;
					mParty.Day = mParty.Day % 360 + 1;
				}

				mParty.Hour = 23;
				mParty.Min = 0;
				mParty.Sec = 0;

				UpdateView();

				mXAxis = 81;
				mYAxis = 51;

				ClearDialog();
			}
			else if (mAnimationEvent == AnimationType.InvestigateDeadBody)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Dialog(" 곧이어 당신은 그 시체의 오른 손에 무언가가 꽉 쥐어져 있음을 알아차렸고 그것이 메세지를 전하기 위한 메모 쪽지라는 것을 알게 되었다." +
				" 당신은 그의 손을 펴보려 했지만 그의 몸은 빳빳하게 굳어 있었고 좀처럼 펼 수가 없었다.", true);

				if (mParty.Item[4] > 0)
				{
					mParty.Item[4]--;

					InvokeAnimation(AnimationType.UseHerbOfRessurection);
				}
			}
			else if (mAnimationEvent == AnimationType.UseHerbOfRessurection)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(new string[] {
					"",
					" 문득 당신은 지금 가지고 있는 부활의 약초가 생각이 났다." +
					"  비록 지금의 그 시체는 살릴 수 없을 정도로 부패했기 때문에 살려내기는 불가능했지만  최소한 죽은지 얼마안된 상태까지는 만들 수 있을거라 생각했다." +
					" 당신은 그 시체에게 부활의 약초를 사용했고  예상대로 그의 몸은 약간의 핏기가 돌게 되었고  어렵지않게 그의 손에 쥐여진  종이 쪽지를  빼낼 수가 있었다. 내용은 이러했다."
				}, SpecialEventType.ReadVesperMemo, true);
			}
			else if (mAnimationEvent == AnimationType.LearnTrollWriting)
			{
				Talk(" 당신은 그에게 일주일간 트롤의 글을 배웠다.", SpecialEventType.LearnTrollWriting);

				mParty.Day += 7;
				mParty.Food -= 50;
				if (mParty.Day > 360)
				{
					mParty.Year++;
					mParty.Day = mParty.Day % 360 + 1;
				}

				SetBit(10);
			}
			else if (mAnimationEvent == AnimationType.MoveGround4)
			{
				mMapName = "Ground4";

				await RefreshGame();

				mXAxis = 47;
				mYAxis = 35;

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.MoveGaeaTerraCastle)
			{
				mMapName = mMapHeader.ExitMap;

				mXAxis = mMapHeader.ExitX - 1;
				mYAxis = mMapHeader.ExitY - 1;

				await RefreshGame();

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.LearnKoboldWriting)
			{
				Talk(" 일주일이 지났다.", SpecialEventType.LearnKoboldWriting);

				SetBit(12);
				mParty.Day += 7;
				PlusTime(0, 0, 1);
			}
			else if (mAnimationEvent == AnimationType.CompleteLearnKoboldWriting)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Dialog(" 당신은 이제 코볼트 글을 알게 되었다.");
			}
			else if (mAnimationEvent == AnimationType.TurnOffTorch)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				mParty.Etc[0] = 0;
				UpdateView();
			}
			else if (mAnimationEvent == AnimationType.SendValiantToUranos)
			{
				mMapName = "Ground5";

				await RefreshGame();

				InvokeAnimation(AnimationType.LandUranos);
			}
			else if (mAnimationEvent == AnimationType.TranformDraconianKing)
			{
				ClearDialog();
				mSpecialEvent = SpecialEventType.BattleDraconianKing;
				ContinueText.Visibility = Visibility.Visible;
			}
			else if (mAnimationEvent == AnimationType.MeetCaminus)
			{
				Ask(new string[] {
					" 나는 이번 반란을 지휘하고 있는 카미너스라는 사람이오. 몇일 전에 우리 성에 에인션트 이블의 의지가 다녀갔소. 그리고 이 세상의 진실을 어렴풋이 알게 되었소." +
					"  우리는 항상 로드안이 말하는 것을 그대로 믿어왔소. 그건 여태껏 알려진 그의 명성 때문이었다오.  하지만 이제는 예외가 있다는걸 깨닭게 되었소.",
					" 당신은 그의 명령대로 오크, 트롤, 코볼트, 드라코니안 이 네종족을 제거하여 인간을 이롭게 한건 사실이오. 당신 또한 그 일을 자랑스럽게 생각할지도 모르오." +
					"  하지만  우리의 입장에서 볼때 그렇다는 것이지, 절대로 다른 종족의 입장에서는 당신이 한 일이 정의를 위한 것이 아니라오." +
					"  실제로 잘못은 우리가 했으면서 도리어 그들에게 잘못을 덮어 씌워서  그들을 해한 것이지않소. 당신이 에인션트 이블을 만났다는 말을 들었소." +
					" 그렇다면 우리가 지금 하려는 일을 이해할거요.  우리들은 당신이 우리의 편에 서서 로드안을 응징해 주기를 바라고 있소.",
					" 자, 선택은 당신에게 있소."
				}, MenuMode.ChooseBetrayLordAhn, new string[] {
					"로드안을 배신한다",
					"로드안의 말대로 이들을 벌한다"
				});
			}
			else if (mAnimationEvent == AnimationType.ViewCrystal)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				Talk(" 당신은 수정구슬을 통해 낯선 풍경을 보았다.", SpecialEventType.ExitCrystal);
			}
			else if (mAnimationEvent == AnimationType.Ending1Cookie1)
			{
				mMapName = "UnderGrd";

				await RefreshGame();

				UpdateTileInfo(45, 47, 54);

				mXAxis = 30;
				mYAxis = 21;

				InvokeAnimation(AnimationType.Ending1Cookie2);
			}
			else if (mAnimationEvent == AnimationType.Ending1Cookie2) {
				mMapName = "Dome";

				await RefreshGame();

				UpdateTileInfo(27, 20, 53);

				mXAxis = 25;
				mYAxis = 5;

				InvokeAnimation(AnimationType.Ending1Cookie3);
			}
			else if (mAnimationEvent == AnimationType.Ending1Cookie3)
			{
				InvokeAnimation(AnimationType.Ending1Cookie4);
			}
			else if (mAnimationEvent == AnimationType.Ending1Cookie4) {
				Talk($"[color={RGB.LightMagenta}] 아스모데우스여,  방금 에너지 공이 이 방으로 들어가는 것을 보았는데 어떻게 된것인가?[/color]", SpecialEventType.Ending1Talk1);
			}
			else if (mAnimationEvent == AnimationType.Ending2Cookie1) {
				mFace = 1;
				if (mPlayerList[0].ClassType == ClassCategory.Magic)
					mFace += 8;

				InvokeAnimation(AnimationType.Ending2Cookie2);
			}
			else if (mAnimationEvent == AnimationType.Ending2Cookie2) {
				mFace = 0;
				if (mPlayerList[0].ClassType == ClassCategory.Magic)
					mFace += 8;

				Dialog(" 그것은  바로 새로운 군주를 탄생 시키는 자리였다. 진정한 선을 행할 수 있는 자만이 설 수 있는 자리.");

				InvokeAnimation(AnimationType.Ending2Cookie3);
			}
			else if (mAnimationEvent == AnimationType.Ending2Cookie3) {
				Talk(" 그 의식의 주인공은 바로 당신이었다.  새로운  로어 세계의 역사를 창조하는  그 순간을 모든 사람들은 두손 모아 찬미하였다.", SpecialEventType.End2_2);
			}
			else if (mAnimationEvent == AnimationType.Dying) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				mSpecialEvent = SpecialEventType.GoToFuture;
				ContinueText.Visibility = Visibility.Visible;
			}
			else if (mAnimationEvent == AnimationType.GoToFuture) {
				mMapName = "Ground1";

				await RefreshGame();

				mXAxis = 57;
				mYAxis = 78;

				mParty.Year = 795;
				mParty.Day = 18;
				mParty.Hour = 9;
				mParty.Min = 0;
				mParty.Sec = 0;

				UpdateView();

				foreach (var player in mPlayerList) {
					player.HP = 1;
					player.Poison = 0;
					player.Unconscious = 0;
					player.Dead = 0;
				}

				if (mAssistPlayer != null) {
					mAssistPlayer.HP = 1;
					mAssistPlayer.Poison = 0;
					mAssistPlayer.Unconscious = 0;
					mAssistPlayer.Dead = 0;
				}

				DisplayPlayerInfo();

				Dialog(new string[] {
					" 얼마나 아득한 순간이 지났을까...",
					" 당신은 문득 정신을 차렸다.  그리고 당신이 본 것은 뭔가 낯선듯한 로어 대륙의 모습이었다."
				});

				InvokeAnimation(AnimationType.Wakeup);
			}
			else if (mAnimationEvent == AnimationType.Wakeup) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				mSpecialEvent = SpecialEventType.HearAlbireo;
				ContinueText.Visibility = Visibility.Visible;
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

			if (GetBit(8))
				OrcWritingText.Text = "가능";
			else
				OrcWritingText.Text = "불가";

			if (GetBit(7))
				OrcSpeakingText.Text = "가능";
			else
				OrcSpeakingText.Text = "불가";

			if (GetBit(10))
				TrollWritingText.Text = "가능";
			else
				TrollWritingText.Text = "불가";

			if (GetBit(9))
				TrollSpeakingText.Text = "가능";
			else
				TrollSpeakingText.Text = "불가";

			if (GetBit(12))
				KoboldWritingText.Text = "가능";
			else
				KoboldWritingText.Text = "불가";

			if (GetBit(11))
				KoboldSpeakingText.Text = "가능";
			else
				KoboldSpeakingText.Text = "불가";

			if (GetBit(14))
				DraconianWritingText.Text = "가능";
			else
				DraconianWritingText.Text = "불가";

			if (GetBit(13))
				DraconianSpeakingText.Text = "가능";
			else
				DraconianSpeakingText.Text = "불가";

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
				Resistance = enemy.Resistance[0] / 2,
				Agility = enemy.Agility,
				Accuracy = enemy.Accuracy[0],
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

		private void BattleTrollKing()
		{
			Dialog(" 순간 사납게 생긴 두 명의 트롤 전사와  마법사 복장의 한 명이 나타났다.");
			if (GetBit(9)) {
				Dialog(new string[] {
					"",
					$"[color={RGB.LightMagenta}] 우리는 트롤킹님의 신변 보호를 맡고 있는 트롤의 광전사들과 위저드이다. 우리들은 목숨을 걸고 당신들로부터 우리의 왕을 지킬 것이다.[/color]"
				}, true);
			}

			for (var i = 0; i < 4; i++)
				JoinEnemy(31);

			for (var i = 0; i < 2; i++)
				JoinEnemy(32);

			JoinEnemy(33);

			DisplayEnemy();
			HideMap();

			mSpecialEvent = SpecialEventType.BattleTrollKing;
			ContinueText.Visibility = Visibility.Visible;
		}

		private void BattleExitKobldKing() {
			mEncounterEnemyList.Clear();

			JoinEnemy(44);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.ExitKoboldKing;
			StartBattle(false);
		}

		private void BattleDraconian(BattleEvent battleEvent) {
			mEncounterEnemyList.Clear();

			JoinEnemy(47);

			DisplayEnemy();
			HideMap();

			mBattleEvent = battleEvent;
			StartBattle();
		}

		private void BattleDraconianEntrance() {
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 4; i++)
				JoinEnemy(48);
			JoinEnemy(50);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.DraconianEntrance;
			StartBattle();
		}

		private void BattleDraconianEntrance2()
		{
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 7; i++)
				JoinEnemy(48);
			JoinEnemy(51);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.DraconianEntrance2;
			StartBattle(false);
		}

		private void BattleDraconianBoss1() {
			mEncounterEnemyList.Clear();
			
			for (var i = 0; i < 4; i++)
				JoinEnemy(48);
			for (var i = 0; i < 2; i++)
				JoinEnemy(49);
			JoinEnemy(46);
			JoinEnemy(52);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.DraconianBoss1;
			StartBattle(false);
		}

		private void BattleDraconianBoss2()
		{
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 7; i++)
				JoinEnemy(52);
			JoinEnemy(53);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.DraconianBoss2;
			StartBattle(false);
		}

		private void BattleFrostDraconian()
		{
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 7; i++)
				JoinEnemy(46);
			JoinEnemy(54);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.FrostDraconian;
			StartBattle(false);
		}

		private void BattleDraconianHolyKnight()
		{
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 2; i++)
				JoinEnemy(50);
			JoinEnemy(55);
			JoinEnemy(51);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.DraconianHolyKnight;
			StartBattle(false);
		}

		private void BattleDraconianMagician()
		{
			mEncounterEnemyList.Clear();

			JoinEnemy(56);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.DraconianMagician;
			StartBattle(false);
		}

		private void BattleDraconianGuardian()
		{
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 4; i++)
				JoinEnemy(52);
			for (var i = 0; i < 3; i++)
				JoinEnemy(50);
			JoinEnemy(57);
			
			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.DraconianGuardian;
			StartBattle(false);
		}

		private void BattleLordAhnType1() {
			mLordAhnBattleCount++;
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 8; i++)
				JoinEnemy(66);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.LordAhn;
			StartBattle(true);
		}

		private void BattleLordAhnType2()
		{
			mLordAhnBattleCount++;
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 6; i++)
				JoinEnemy(66);
			JoinEnemy(67);
			JoinEnemy(68);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.LordAhn;
			StartBattle(true);
		}

		private void BattleLordAhnType3()
		{
			mLordAhnBattleCount++;
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 4; i++)
				JoinEnemy(68);
			for (var i = 0; i < 4; i++)
				JoinEnemy(67);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.LordAhn;
			StartBattle(true);
		}

		private void BattleLordAhnType4()
		{
			mLordAhnBattleCount++;
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 4; i++)
				JoinEnemy(67);
			for (var i = 0; i < 3; i++)
				JoinEnemy(68);
			JoinEnemy(69);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.LordAhn;
			StartBattle(true);
		}

		private void BattleLordAhnType5()
		{
			mLordAhnBattleCount++;
			mEncounterEnemyList.Clear();

			for (var i = 0; i < 6; i++)
				JoinEnemy(70);
			JoinEnemy(71);
			JoinEnemy(72);

			DisplayEnemy();
			HideMap();

			mBattleEvent = BattleEvent.LordAhn;
			StartBattle(true);
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

			var size = sender.Size.ToVector2();
			var interpolation = (CanvasImageInterpolation)Enum.Parse(typeof(CanvasImageInterpolation), CanvasImageInterpolation.HighQualityCubic.ToString());

			var fadeIn = false;
			var fadeOut = false;

			if (mAnimationEvent == AnimationType.LearnOrcWriting ||
				mAnimationEvent == AnimationType.MoveGround4 ||
				mAnimationEvent == AnimationType.MoveGaeaTerraCastle ||
				mAnimationEvent == AnimationType.LearnKoboldWriting ||
				mAnimationEvent == AnimationType.Ending1Cookie1)
				fadeOut = true;
			else if (mAnimationEvent == AnimationType.CompleteLearnOrcWriting ||
				mAnimationEvent == AnimationType.MoveGround2 ||
				mAnimationEvent == AnimationType.MoveGround3 ||
				mAnimationEvent == AnimationType.CompleteLearnKoboldWriting ||
				mAnimationEvent == AnimationType.LandUranos ||
				mAnimationEvent == AnimationType.ViewCrystal ||
				mAnimationEvent == AnimationType.ExitCrystal ||
				mAnimationEvent == AnimationType.Ending1Cookie2 ||
				mAnimationEvent == AnimationType.Ending1Cookie3 ||
				mAnimationEvent == AnimationType.Ending2Cookie1 ||
				mAnimationEvent == AnimationType.Wakeup)
				fadeIn = true;

			var crystalMap = mCrystalMap;
			if (mCrystalMap != null)
			{
				var transform = Matrix3x2.Identity * Matrix3x2.CreateTranslation(-new Vector2(52 * (mCrystalX - 4), 52 * (mCrystalY - 5)));
				args.DrawingSession.Transform = transform;

				using (var sb = args.DrawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.NearestNeighbor, CanvasSpriteOptions.ClampToSourceRect))
				{
					for (int i = 0; i < crystalMap.Layer.Length; ++i)
					{
						DrawTile(sb, crystalMap.Layer, i, mCrystalX, mCrystalY, true);
					}
				}
			}
			else
			{
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

				using (var sb = args.DrawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.NearestNeighbor, CanvasSpriteOptions.ClampToSourceRect))
				{
					Vector4 GetTint(int x, int y)
					{
						if ((!mEbony || mParty.Etc[0] > 0 || mMoonLight) && ((mMapHeader.Layer[x + y * mMapHeader.Width] & 0x80) == 0) && (playerX - mXWide > x || x > playerX + mXWide || playerY - mYWide > y || y > playerY + mYWide))
							return new Vector4(0.1f, 0.1f, 0.6f, 1);
						else
							return Vector4.One;
					}

					lock (mapLock)
					{
						var mapHeader = mMapHeader;
						for (int i = 0; i < mapHeader.Layer.Length; ++i)
						{
							DrawTile(sb, mapHeader.Layer, i, playerX, playerY, false);
						}
					}

					//Vector4 tint;
					//if (fadeOut)
					//	tint = new Vector4(mAnimationFrame == 10 ? 0 : 0.1f, mAnimationFrame == 10 ? 0 : 0.1f, (10 - mAnimationFrame) / 10f, 1);
					//else if (fadeIn)
					//	tint = new Vector4(0.1f, 0.1f, mAnimationFrame / 10f, 1);
					//else if (!mEbony || mParty.Etc[0] > 0 || mMoonLight)
					//	tint = new Vector4(0.1f, 0.1f, 0.6f, 1);
					//else
					//	tint = Vector4.One;

					if (mCharacterTiles != null && mFace >= 0)
					{
						if (mAnimationEvent != AnimationType.GotoCourt2 && 
							mAnimationEvent != AnimationType.FollowSoldier && 
							mAnimationEvent != AnimationType.FollowSoldier2 &&
							mSpecialEvent != SpecialEventType.SendNecromancer2 &&
							mAnimationEvent != AnimationType.Ending1Cookie1 &&
							mAnimationEvent != AnimationType.Ending1Cookie2 &&
							mAnimationEvent != AnimationType.Ending1Cookie3 &&
							mAnimationEvent != AnimationType.Ending1Cookie4 &&
							mAnimationEvent != AnimationType.Ending2Cookie1 &&
							mAnimationEvent != AnimationType.Ending2Cookie2 &&
							mAnimationEvent != AnimationType.Ending2Cookie3)
						{
							if (fadeOut)
							{
								mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), new Vector4(mAnimationFrame == 10 ? 0 : 0.1f, mAnimationFrame == 10 ? 0 : 0.1f, (10 - mAnimationFrame) / 10f, 1));
							}
							else if (fadeIn)
							{
								mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), new Vector4(0.1f, 0.1f, mAnimationFrame / 10f, 1));
							}
							else if (!mEbony || mParty.Etc[0] > 0 || mMoonLight)
							{
								Vector4 GetPlayerTint() {
									if (mEbony && mMoonLight && mXWide == 0 && mYWide == 0 && mParty.Etc[0] == 0)
										return new Vector4(0.1f, 0.1f, 0.6f, 1);
									else
										return Vector4.One;
								}

								if (mAnimationEvent == AnimationType.PassCrystal2 && mAnimationEvent > 0)
								{
									if (mAnimationFrame <= 2)
										mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - mAnimationFrame), GetPlayerTint());
									else
										mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - (5 - mAnimationFrame)), GetPlayerTint());
								}
								else if (mAnimationEvent == AnimationType.TransformProtagonist2 && mAnimationFrame >= 2) {
									mCharacterTiles.Draw(sb, 25, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - 1), GetPlayerTint());
									mCharacterTiles.Draw(sb, 26, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), GetPlayerTint());
								}
								else
									mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), GetPlayerTint());
							}
						}

						if (mAnimationEvent == AnimationType.CaptureProtagonist && mAnimationFrame > 0)
						{
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(24, mYAxis + (6 - mAnimationFrame)), GetTint(24, mYAxis + (6 - mAnimationFrame)));
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(25, mYAxis + (6 - mAnimationFrame)), GetTint(25, mYAxis + (6 - mAnimationFrame)));
						}
						else if (mAnimationEvent == AnimationType.GotoCourt)
						{
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(24, mYAxis + 1), GetTint(24, mYAxis + 1));
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(25, mYAxis + 1), GetTint(25, mYAxis + 1));
						}
						else if (mAnimationEvent == AnimationType.GotoCourt2 && mAnimationFrame > 0)
						{
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis + (6 - mAnimationFrame)), GetTint(49, mYAxis + (6 - mAnimationFrame)));
							mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(50, mYAxis + (6 - mAnimationFrame)), GetTint(50, mYAxis + (6 - mAnimationFrame)));
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(51, mYAxis + (6 - mAnimationFrame)), GetTint(51, mYAxis + (6 - mAnimationFrame)));
						}
						else if (mAnimationEvent == AnimationType.SubmitProof && mAnimationFrame > 0)
						{
							if (mAnimationFrame <= 3)
								mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, playerY - mAnimationFrame), GetTint(49, playerY - mAnimationFrame));
							else
								mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, playerY - 3 + (mAnimationFrame - 3)), GetTint(49, playerY - 3 + (mAnimationFrame - 3)));

							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(51, mYAxis), GetTint(51, mYAxis));
						}
						else if (mAnimationEvent == AnimationType.GotoJail)
						{
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis), GetTint(49, mYAxis));
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(51, mYAxis), GetTint(51, mYAxis));
						}
						else if (mAnimationEvent == AnimationType.FollowSoldier)
							mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX + (mAnimationFrame - 1), playerY), GetTint(playerX + (mAnimationFrame - 1), playerY));
						else if (mAnimationEvent == AnimationType.FollowSoldier2 && mAnimationFrame >= 118)
						{
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis + (123 - mAnimationFrame)), GetTint(49, mYAxis + (123 - mAnimationFrame)));
							mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(50, mYAxis + (123 - mAnimationFrame)), GetTint(50, mYAxis + (123 - mAnimationFrame)));
						}
						else if (mAnimationEvent == AnimationType.LeaveSoldier && mAnimationFrame > 0)
						{
							mCharacterTiles.Draw(sb, 13, mCharacterTiles.SpriteSize * new Vector2(49, mYAxis + (mAnimationFrame - 1)), GetTint(49, mYAxis + (mAnimationFrame - 1)));
						}
						else if (mAnimationEvent == AnimationType.TranformDraconianKing && mAnimationFrame == 4)
						{
							mCharacterTiles.Draw(sb, 12, mCharacterTiles.SpriteSize * new Vector2(mXAxis, mYAxis - 1), GetTint(mXAxis, mYAxis - 1));
						}
						else if (mAnimationEvent == AnimationType.TranformDraconianKing && mAnimationFrame >= 5)
						{
							mCharacterTiles.Draw(sb, 14, mCharacterTiles.SpriteSize * new Vector2(mXAxis - 1, mYAxis - 2), GetTint(mXAxis - 1, mYAxis - 2));
							mCharacterTiles.Draw(sb, 15, mCharacterTiles.SpriteSize * new Vector2(mXAxis - 1, mYAxis - 1), GetTint(mXAxis - 1, mYAxis - 1));
							mCharacterTiles.Draw(sb, 16, mCharacterTiles.SpriteSize * new Vector2(mXAxis, mYAxis - 2), GetTint(mXAxis, mYAxis - 2));
							mCharacterTiles.Draw(sb, 17, mCharacterTiles.SpriteSize * new Vector2(mXAxis, mYAxis - 1), GetTint(mXAxis, mYAxis - 1));
							mCharacterTiles.Draw(sb, 18, mCharacterTiles.SpriteSize * new Vector2(mXAxis + 1, mYAxis - 2), GetTint(mXAxis + 1, mYAxis - 2));
							mCharacterTiles.Draw(sb, 19, mCharacterTiles.SpriteSize * new Vector2(mXAxis + 1, mYAxis - 1), GetTint(mXAxis + 1, mYAxis - 1));
						}
						else if (mSpecialEvent == SpecialEventType.MeetAhnYoungKi)
							mCharacterTiles.Draw(sb, 24, mCharacterTiles.SpriteSize * new Vector2(17, 17), GetTint(17, 17));
						else if ((mAnimationEvent == AnimationType.SealCrystal && mAnimationFrame > 1) || mAnimationEvent == AnimationType.SealCrystal2)
							mCharacterTiles.Draw(sb, 20, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - 2), GetTint(playerX, playerY - 2));
						else if (mAnimationEvent == AnimationType.Ending1Cookie2 || mAnimationEvent == AnimationType.Ending1Cookie3) {
							if (fadeIn && mAnimationFrame < 10f)
							{
								mCharacterTiles.Draw(sb, 20, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), new Vector4(0.1f, 0.1f, mAnimationFrame / 10f, 1));
							}
							else
								mCharacterTiles.Draw(sb, 20, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), Vector4.One);
						}
						else if (mAnimationEvent == AnimationType.Ending2Cookie2 && mAnimationFrame > 0)
						{
							mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY + 6 - mAnimationFrame), Vector4.One);
						}
						else if (mAnimationEvent == AnimationType.Ending2Cookie3 && 0 < mAnimationFrame)
						{
							if (mAnimationFrame <= 5)
							{
								mCharacterTiles.Draw(sb, 21, mCharacterTiles.SpriteSize * new Vector2(playerX - (6 - mAnimationFrame), playerY - 3), Vector4.One);
								mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY - 3), Vector4.One);
							}
							else if (mAnimationFrame <= 10)
								mCharacterTiles.Draw(sb, 22, mCharacterTiles.SpriteSize * new Vector2(playerX - (mAnimationFrame - 5), playerY - 3), Vector4.One);
						}
					}

					if (mDecorateTiles != null)
					{
						if (mAnimationEvent == AnimationType.MeetCaminus && mAnimationFrame > 0)
						{
							mDecorateTiles.Draw(sb, 8, mDecorateTiles.SpriteSize * new Vector2(playerX, playerY - (6 - mAnimationFrame)), GetTint(playerX, playerY - (6 - mAnimationFrame)));
						}
						else if (mAnimationEvent == AnimationType.LeaveCaminus && mAnimationEvent > 0) {
							mDecorateTiles.Draw(sb, 8, mDecorateTiles.SpriteSize * new Vector2(playerX, playerY - mAnimationFrame), GetTint(playerX, playerY - mAnimationFrame));
						}
					}
				}

				if ((mAnimationEvent == AnimationType.GotoCourt ||
					mAnimationEvent == AnimationType.GotoJail ||
					mAnimationEvent == AnimationType.LiveJail ||
					mAnimationEvent == AnimationType.FollowSoldier2 ||
					mAnimationEvent == AnimationType.ConfirmPardon2 ||
					mAnimationEvent == AnimationType.LearnOrcSpeaking2 ||
					mAnimationEvent == AnimationType.LearnTrollWriting ||
					mAnimationEvent == AnimationType.SendValiantToUranos ||
					mAnimationEvent == AnimationType.PassCrystal ||
					mAnimationEvent == AnimationType.GoToFuture) && mAnimationFrame <= 117)
					AnimateTransition(mAnimationFrame, playerX, playerY);
			}
		}

		private void DrawTile(CanvasSpriteBatch sb, byte[] layer, int index, int playerX, int playerY, bool viewCrystal)
		{
			int row = index / mMapHeader.Width;
			int column = index % mMapHeader.Width;

			Vector4 tint;

			var darkness = false;
			if (!viewCrystal && playerX - mXWide <= column && column <= playerX + mXWide && playerY - mYWide <= row && row <= playerY + mYWide)
				darkness = false;
			else
				darkness = true;

			if (!mDark)
				darkness = false;

			if (mEbony && mParty.Etc[0] == 0)
				darkness = true;

			var fadeIn = false;
			var fadeOut = false;

			if (mAnimationEvent == AnimationType.LearnOrcWriting ||
				mAnimationEvent == AnimationType.MoveGround4 ||
				mAnimationEvent == AnimationType.MoveGaeaTerraCastle ||
				mAnimationEvent == AnimationType.LearnKoboldWriting ||
				mAnimationEvent == AnimationType.Ending1Cookie1)
				fadeOut = true;
			else if (mAnimationEvent == AnimationType.CompleteLearnOrcWriting ||
				mAnimationEvent == AnimationType.MoveGround2 ||
				mAnimationEvent == AnimationType.MoveGround3 ||
				mAnimationEvent == AnimationType.CompleteLearnKoboldWriting ||
				mAnimationEvent == AnimationType.LandUranos ||
				mAnimationEvent == AnimationType.ViewCrystal ||
				mAnimationEvent == AnimationType.ExitCrystal ||
				mAnimationEvent == AnimationType.Ending1Cookie2 ||
				mAnimationEvent == AnimationType.Ending1Cookie3 ||
				mAnimationEvent == AnimationType.Ending2Cookie1 ||
				mAnimationEvent == AnimationType.Wakeup)
				fadeIn = true;

			if ((layer[index] & 0x80) > 0 || 
				mAnimationEvent == AnimationType.Ending1Cookie2 || 
				mAnimationEvent == AnimationType.Ending1Cookie3 || 
				mAnimationEvent == AnimationType.Ending1Cookie4 ||
				mAnimationEvent == AnimationType.Ending2Cookie1)
			{
				if (fadeOut)
					tint = new Vector4(mAnimationFrame == 10 ? 0 : 0.1f, mAnimationFrame == 10 ? 0 : 0.1f, (10 - mAnimationFrame) / 10f, 1);
				else if (fadeIn && mAnimationFrame < 10)
					tint = new Vector4(0.1f, 0.1f, mAnimationFrame / 10f, 1);
				else
					tint = Vector4.One;
			}
			else if (mMoonLight || !darkness)
			{
				if (darkness)
				{
					if (fadeOut && mAnimationFrame > 4)
						tint = new Vector4(mAnimationFrame == 10 ? 0 : 0.1f, mAnimationFrame == 10 ? 0 : 0.1f, (10 - mAnimationFrame) / 10f, 1);
					else if (fadeIn && mAnimationFrame < 6)
						tint = new Vector4(0.1f, 0.1f, mAnimationFrame / 10f, 1);
					else
						tint = new Vector4(0.1f, 0.1f, 0.6f, 1);
				}
				else
				{
					if (fadeOut)
						tint = new Vector4(mAnimationFrame == 10 ? 0 : 0.1f, mAnimationFrame == 10 ? 0 : 0.1f, (10 - mAnimationFrame) / 10f, 1);
					else if (fadeIn)
						tint = new Vector4(0.1f, 0.1f, mAnimationFrame / 10f, 1);
					else
						tint = Vector4.One;
				}
			}
			else
			{
				if (darkness)
					tint = new Vector4(0.0f, 0.0f, 0.0f, 1);
				else
				{
					if (fadeOut)
						tint = new Vector4(mAnimationFrame == 10 ? 0 : 0.1f, mAnimationFrame == 10 ? 0 : 0.1f, (10 - mAnimationFrame) / 10f, 1);
					else if (fadeIn)
						tint = new Vector4(0.1f, 0.1f, mAnimationFrame / 10f, 1);
					else
						tint = Vector4.One;
				}
			}

			if (mMapTiles != null)
			{
				var mapIdx = 56;

				switch (mMapHeader.TileType)
				{
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


				var tileIdx = GetTileInfo(layer, index);
				var oriTileIdx = tileIdx;

				if (mMapName == "OrcTown" && (tileIdx == 53 || tileIdx == 54 || tileIdx == 34))
					tileIdx = 43;
				else if (mMapName == "Ground3" && tileIdx == 21)
					tileIdx = 13;
				else if (mMapName == "Vesper")
				{
					if (tileIdx == 53)
						tileIdx = 43;
					else if (tileIdx == 54)
						tileIdx = 44;
				}
				else if (mMapName == "TrolTown" && (tileIdx == 53 || tileIdx == 54))
					tileIdx = 44;
				else if (mMapName == "Ground4")
				{
					if (tileIdx == 52)
						tileIdx = 14;
					else if (tileIdx == 54)
						tileIdx = 15;
				}
				else if (mMapName == "Hut" && tileIdx == 53)
				{
					tileIdx = 44;
				}
				else if (mMapName == "Ancient")
				{
					if (tileIdx == 53)
						tileIdx = 44;
					else if (tileIdx == 22)
					{
						mapIdx = 56 * 2;
						tileIdx = 13;
					}
					else if (tileIdx == 3)
					{
						mapIdx = 56 * 2;
						tileIdx = 16;
					}
					else if (tileIdx == 4)
					{
						mapIdx = 56 * 2;
						tileIdx = 17;
					}
				}
				else if (mMapName == "DracTown")
				{
					if (tileIdx == 19)
						tileIdx = 24;
					else if (tileIdx == 49 || tileIdx == 52 || tileIdx == 53)
						tileIdx = 44;
					else if (tileIdx == 8) {
						mapIdx = 56 * 2;
						tileIdx = 16;
					}
					else if (tileIdx == 9)
					{
						mapIdx = 56 * 2;
						tileIdx = 17;
					}
					else if (tileIdx == 14)
					{
						mapIdx = 56 * 2;
						tileIdx = 19;
					}
				}
				else if (mMapName == "Tomb") {
					if (tileIdx == 37) {
						mapIdx = 56 * 2;
						tileIdx = 16;
					}
					else if (tileIdx == 38)
					{
						mapIdx = 56 * 2;
						tileIdx = 17;
					}
					else if (tileIdx == 54) {
						mapIdx = 56 * 2;
						tileIdx = 14;
					}
					else if (tileIdx == 53) {
						mapIdx = 0;
						tileIdx = 48;
					}
				}
				else if (mMapName == "Dome") {
					if (tileIdx == 54) {
						tileIdx = 35;	
					}
				}
				
				if (mSpecialEvent == SpecialEventType.Penetration)
				{
					if (((mMapHeader.TileType == PositionType.Den || mMapHeader.TileType == PositionType.Keep) && tileIdx == 52) ||
						(mMapHeader.TileType == PositionType.Ground && tileIdx == 20))
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
				else if (mAnimationEvent == AnimationType.VisitCanopus && mAnimationFrame > 0)
				{
					if (column == playerX - (5 - mAnimationFrame) && row == playerY)
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.RequestPardon && mAnimationFrame > 0)
				{
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
				else if (mAnimationEvent == AnimationType.TranformDraconianKing && mAnimationEvent > 0) {
					if (column == playerX && row == playerY - 3 && mAnimationFrame >= 1)
					{
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						oriTileIdx = 53;
					}
					else if (column == playerX && row == playerY - 2 && mAnimationFrame >= 1)
					{
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						oriTileIdx = 44;
					}
					else if (column == playerX - 1 && row == playerY - 1 && mAnimationFrame >= 2)
					{
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						oriTileIdx = 44;
					}
					else if (column == playerX - 1 && row == playerY && mAnimationFrame >= 2)
					{
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						oriTileIdx = 53;
					}
					else if (column == playerX + 1 && row == playerY - 1 && mAnimationFrame >= 3)
					{
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						oriTileIdx = 44;
					}
					else if (column == playerX + 1 && row == playerY && mAnimationFrame >= 3)
					{
						mMapTiles.Draw(sb, 44 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						oriTileIdx = 53;
					}
					else if (column == playerX && row == playerY - 1 && mAnimationFrame >= 4)
					{
						mMapTiles.Draw(sb, 42 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					}
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mSpecialEvent == SpecialEventType.MeetAhnYoungKi) {
					if (column == 17 && row == 17)
						mMapTiles.Draw(sb, 47 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.Ending1Cookie4 && mAnimationFrame >= 5)
				{
					if (column == playerX + 3 && row == playerY + (10 - mAnimationFrame))
						mMapTiles.Draw(sb, 52 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.Ending2Cookie1 || mAnimationEvent == AnimationType.Ending2Cookie2)
				{
					if (column == playerX && row == playerY - 3)
						mMapTiles.Draw(sb, 42 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.Ending2Cookie3) {
					if (column == playerX && row == playerY - 3) {
						if (mAnimationFrame >= 5)
							mMapTiles.Draw(sb, 54 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						else
							mMapTiles.Draw(sb, 42 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					}
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else
				{
					mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}

				if (mMapName == "OrcTown")
				{
					switch (oriTileIdx)
					{
						case 53:
							mDecorateTiles.Draw(sb, 0, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
						case 54:
							mDecorateTiles.Draw(sb, 1, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
						case 34:
							mDecorateTiles.Draw(sb, 2, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
					}
				}
				else if (mMapName == "Vesper")
				{
					switch (oriTileIdx)
					{
						case 53:
							mDecorateTiles.Draw(sb, 3, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
						case 54:
							mDecorateTiles.Draw(sb, 2, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
					}
				}
				else if (mMapName == "TrolTown")
				{
					switch (oriTileIdx)
					{
						case 53:
							mDecorateTiles.Draw(sb, 4, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
						case 54:
							mDecorateTiles.Draw(sb, 3, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
					}
				}
				else if (mMapName == "Hut")
				{
					if (oriTileIdx == 53)
						mDecorateTiles.Draw(sb, 4, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mMapName == "Ancient")
				{
					if (oriTileIdx == 53)
						mDecorateTiles.Draw(sb, 5, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mMapName == "DracTown")
				{
					switch (oriTileIdx)
					{
						case 53:
							mDecorateTiles.Draw(sb, 6, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
						case 52:
							mDecorateTiles.Draw(sb, 7, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
						case 49:
							mDecorateTiles.Draw(sb, 5, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
							break;
					}
				}
				else if (mMapName == "Dome") {
					if (oriTileIdx == 54)
						mDecorateTiles.Draw(sb, 8, mDecorateTiles.SpriteSize * new Vector2(column, row), tint);
				}
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
			SendNecromancer2,
			FollowSolider,
			LordAhnMission,
			LeaveSoldier,
			ConfirmPardon,
			ConfirmPardon2,
			JoinCanopus,
			LearnTrollWriting,
			SendValiantToUranos,
			LearnOrcWriting,
			AskKillOrc1,
			AskKillOrc2,
			AskKillOrc3,
			AskKillOrc4,
			AskKillOrc5,
			AskKillOrc6,
			AskKillOrc7,
			AskKillOrc8,
			MeetTraveler,
			LordAhnCall,
			MoveGround3,
			BattleOrcTempleEnterance,
			BattleOrcArchiMage,
			BattleOrcKing,
			BattleTrollAssassin,
			BattleVesperTroll1,
			BattleVesperTroll2,
			BattleVesperTroll3,
			BattleVesperTroll4,
			BattleVesperTroll5,
			BattleVesperTroll6,
			BattleVesperTroll7,
			ReadVesperMemo,
			NoMoreMemo,
			BattleTrollKing,
			BattleTroll1,
			BattleTroll2,
			BattleTroll5,
			KillPhysicist,
			KillPhysicist2,
			AskKillTroll6,
			AskKillTroll7,
			AskKillTroll8,
			AskKillTroll9,
			AskKillTroll10,
			AskKillTroll11,
			AskKillTroll12,
			AskKillTroll13,
			MeetBecrux,
			LearnKoboldWriting,
			BattleExitKoboldKing,
			BattleKoboldAlter,
			BattleTreasureBox1,
			BattleTreasureBox2,
			BattleTreasureBox3,
			BattleTreasureBox4,
			BattleKoboldSoldier,
			BattleKoboldSoldier2,
			BattleKoboldSecurity,
			BattleKoboldGuardian,
			BattleKoboldSummoner,
			BattleKoboldKing,
			AskTreasureboxQuestion1,
			AskTreasureboxQuestion2,
			AskTreasureboxQuestion3,
			AskTreasureboxQuestion4,
			OpenTreasureBox,
			PlusExperience,
			CaptureMermaid,
			BattleVampire,
			BattleDracula,
			OpenTomb,
			RefuseJoinEnterTomb,
			BattleDraconian1,
			BattleDraconian2,
			BattleDraconian3,
			BattleDraconian4,
			BattleDraconian5,
			BattleDraconian6,
			BattleDraconian7,
			BattleDraconian8,
			BattleDraconian9,
			BattleDraconian10,
			BattleDraconian11,
			BattleDraconian12,
			BattleDraconian13,
			SteelBoy,
			BattleDraconianEntrance2,
			BattleDraconianBoss1,
			BattleDraconianBoss2,
			BattleFrostDraconian,
			BattleDraconianHolyKnight,
			BattleDraconianMagician,
			BattleDraconianGuardian,
			MeetDraconianKing,
			BattleMessengerOfDeath,
			BattleKerberos,
			BattleDraconianOldKing,
			BattleDraconianOldKing2,
			BattleDraconianKing,
			BattleDraconianSpirit,
			TeleportCastleLore,
			MeetAhnYoungKi,
			FindShelter,
			BattleOldLordAhn,
			UseCromaticCrystal,
			BuildDome,
			ExitCrystal,
			End1,
			End2,
			End3,
			Ending1Talk1,
			Ending1Talk2,
			End2_2,
			GoToFuture,
			HearAlbireo
		}

		private enum BattleEvent
		{
			None,
			Pollux,
			Orc1,
			Orc2,
			Orc3,
			Orc4,
			Orc5,
			Orc6,
			Orc7,
			Orc8,
			OrcTownEnterance,
			OrcTempleEnterance,
			OrcArchiMage,
			OrcKing,
			VesperTroll1,
			VesperTroll2,
			VesperTroll3,
			VesperTroll4,
			VesperTroll5,
			VesperTroll6,
			VesperTroll7,
			TrollAssassin,
			TrollKing,
			Troll1,
			Troll2,
			Troll3,
			Troll4,
			Troll5,
			Troll6,
			Troll7,
			Troll8,
			Troll9,
			Troll10,
			Troll11,
			Troll12,
			Troll13,
			ExitKoboldKing,
			KoboldKnight,
			GoldKey,
			KoboldMagicUser,
			SaphireKey,
			KoboldAlter,
			TreasureBox1,
			TreasureBox2,
			TreasureBox3,
			TreasureBox4,
			KoboldSoldier,
			KoboldSoldier2,
			KoboldSecurity,
			KoboldGuardian,
			KoboldSummoner,
			KoboldKing,
			DraconianBeliever,
			Vampire,
			Dracula,
			Draconian1,
			Draconian2,
			Draconian3,
			Draconian4,
			Draconian5,
			Draconian6,
			Draconian7,
			Draconian8,
			Draconian9,
			Draconian10,
			Draconian11,
			Draconian12,
			Draconian13,
			DraconianEntrance,
			DraconianEntrance2,
			DraconianBoss1,
			DraconianBoss2,
			FrostDraconian,
			DraconianHolyKnight,
			DraconianMagician,
			DraconianGuardian,
			MessengerOfDeath,
			Kerberos,
			DraconianOldKing,
			DraconianKing,
			DraconianSpirit,
			Rebellion1,
			Rebellion2,
			Rebellion3,
			OldLordAhn1,
			OldLordAhn2,
			OrcRevengeSpirit,
			TrollRevengeSpirit,
			KoboldRevengeSpirit,
			DraconianRevengeSpirit,
			LordAhn
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
			LeaveCanopus,
			LearnOrcWriting,
			CompleteLearnOrcWriting,
			LearnOrcSpeaking,
			LearnOrcSpeaking2,
			MoveGround2,
			MoveGround3,
			InvestigateDeadBody,
			UseHerbOfRessurection,
			LearnTrollWriting,
			MoveGround4,
			MoveGaeaTerraCastle,
			LearnKoboldWriting,
			CompleteLearnKoboldWriting,
			TurnOffTorch,
			SendValiantToUranos,
			LandUranos,
			TranformDraconianKing,
			MeetCaminus,
			ViewCrystal,
			ExitCrystal,
			LeaveCaminus,
			Ending1Cookie1,
			Ending1Cookie2,
			Ending1Cookie3,
			Ending1Cookie4,
			Ending2Cookie1,
			Ending2Cookie2,
			Ending2Cookie3,
			Dying,
			GoToFuture,
			Wakeup,
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
			Equip,
			EquipUnequip,
			EquipType,
			ChooseEquip,
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
			JoinAlcor,
			JoinMizar,
			JoinAntaresJr,
			JoinCanopus,
			MeetPollux,
			JoinPollux,
			ChooseItemType,
			BattleChooseItemType,
			ChooseCrystal,
			BattleChooseCrystal,
			LearnTrollWriting,
			JoinAltair,
			JoinVega,
			JoinAlgol,
			JoinProxima,
			JoinDenebola,
			JoinCapella,
			LearnOrcWriting,
			KillOrc1,
			KillOrc2,
			KillOrc3,
			KillOrc4,
			KillOrc5,
			KillOrc6,
			KillOrc7,
			KillOrc8,
			GuardOrcTown,
			NegotiateTrollKing,
			TrollKingSuggestion,
			PhysicistTeaching,
			KillTroll6,
			KillTroll7,
			KillTroll8,
			KillTroll9,
			KillTroll10,
			KillTroll11,
			KillTroll12,
			KillTroll13,
			JoinBercux,
			LearnKoboldWriting,
			CrossLava,
			Answer1_1,
			Answer1_2,
			Answer1_3,
			Answer2_1,
			Answer2_2,
			Answer2_3,
			Answer3_1,
			Answer3_2,
			Answer3_3,
			Answer3_4,
			Answer3_5,
			Answer3_6,
			Answer4_1,
			Answer4_2,
			Answer4_3,
			Answer4_4,
			Answer4_5,
			MeetAncientEvil,
			KillMermaid,
			EnterTomb,
			ForceEnterTomb,
			BattleDraconian1,
			BattleDraconian2,
			BattleDraconian4,
			BattleDraconian5,
			BattleDraconian6,
			BattleDraconian8,
			BattleDraconian9,
			BattleDraconian10,
			BattleDraconian11,
			BattleDraconian12,
			BattleDraconian13,
			JoinDraconian,
			BattleDraconianEntrance,
			TeleportCastleLore,
			ChooseBetrayLordAhn,
			JoinFriendAgain
		}
	}
}