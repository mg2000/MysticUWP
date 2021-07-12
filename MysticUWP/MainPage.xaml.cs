using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using Windows.Foundation;
using Windows.Gaming.XboxLive.Storage;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MysticUWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private int mFirstLine;
		private int mLastLine;

		private float mOffset = 0;
		private float mVelocity = 0;
		private const float mTargetSpeed = 0.5f;
		private float mTargetVelocity = 0;

		private CanvasLinearGradientBrush mTextOpacityBrush;
		private CanvasLinearGradientBrush mBlurOpacityBrush;

		static CanvasTextFormat symbolText = new CanvasTextFormat()
		{
			FontSize = 30,
			FontFamily = "Segoe UI",
			HorizontalAlignment = CanvasHorizontalAlignment.Center,
			VerticalAlignment = CanvasVerticalAlignment.Center
		};

		private static float mLineHeight = symbolText.FontSize * 1.5f;

		private List<TextBlock> mMainMenuItemList = new List<TextBlock>();
		private int mFocusItem = 0;


		public MainPage()
		{
			this.InitializeComponent();

			SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) =>
			{
				if (!e.Handled)
				{
					e.Handled = true;
				}
			};

			mMainMenuItemList.Add(newGameItem);
			mMainMenuItemList.Add(loadGameItem);
			mMainMenuItemList.Add(showPrologItem);
			mMainMenuItemList.Add(showCreditItem);
			mMainMenuItemList.Add(exitGameItem);

			SyncSaveData();
		}

		private async void SyncSaveData()
		{
			var users = await User.FindAllAsync();
			var gameSaveTask = await GameSaveProvider.GetForUserAsync(users[0], "00000000-0000-0000-0000-00007e5a3fb0");

			Debug.WriteLine($"클라우드 동기화 연결 결과: {gameSaveTask.Status}");

			if (gameSaveTask.Status == GameSaveErrorStatus.Ok)
			{
				var gameSaveProvider = gameSaveTask.Value;

				var gameSaveContainer = gameSaveProvider.CreateContainer("MysticSaveContainer");

				var saveData = new List<SaveData>();
				var loadFailed = false;

				for (var i = 0; i < 9; i++)
				{
					string saveName;
					if (i == 0)
						saveName = "mysticSave";
					else
						saveName = $"mysticSave{i}";

					var result = await gameSaveContainer.GetAsync(new string[] { saveName });
					if (result.Status == GameSaveErrorStatus.Ok)
					{
						IBuffer loadedBuffer;

						result.Value.TryGetValue(saveName, out loadedBuffer);

						if (loadedBuffer == null)
						{
							loadFailed = true;
							break;
						}

						var reader = DataReader.FromBuffer(loadedBuffer);
						var dataSize = reader.ReadUInt32();

						var buffer = new byte[dataSize];

						reader.ReadBytes(buffer);

						var loadData = Encoding.UTF8.GetString(buffer);

						saveData.Add(JsonConvert.DeserializeObject<SaveData>(loadData));
					}
					else if (result.Status == GameSaveErrorStatus.BlobNotFound)
					{
						saveData.Add(null);
					}
					else if (result.Status != GameSaveErrorStatus.BlobNotFound)
					{
						loadFailed = true;
						break;
					}
				}

				if (loadFailed)
					await new MessageDialog("클라우드 서버에서 세이브를 가져올 수 없습니다. 기기에 저장된 세이브를 사용합니다.").ShowAsync();
				else
				{
					var storageFolder = ApplicationData.Current.LocalFolder;
					var differentBuilder = new StringBuilder();
					var differentID = new List<int>();

					for (var i = 0; i < saveData.Count; i++)
					{
						string GetSaveName(int id)
						{
							if (id == 0)
								return "본 게임 데이터";
							else
								return $"게임 데이터 {id} (부)";
						}


						string idStr;
						if (i == 0)
							idStr = "";
						else
							idStr = i.ToString();

						try
						{
							var localSaveFile = await storageFolder.GetFileAsync($"mysticSave{idStr}.dat");
							var localSaveData = JsonConvert.DeserializeObject<SaveData>(await FileIO.ReadTextAsync(localSaveFile));

							if (localSaveData == null)
							{
								if (saveData[i] != null)
								{
									differentBuilder.Append(GetSaveName(i)).Append("\r\n");
									differentBuilder.Append("클라우드 데이터만 존재").Append("\r\n\r\n");

									differentID.Add(i);
								}
							}
							else
							{
								if (saveData[i] == null)
								{
									differentBuilder.Append(GetSaveName(i)).Append("\r\n");
									differentBuilder.Append("기기 데이터만 존재").Append("\r\n\r\n"); ;

									differentID.Add(i);
								}
								else
								{
									if (saveData[i].SaveTime != localSaveData.SaveTime)
									{
										differentBuilder.Append(GetSaveName(i)).Append("\r\n");
										differentBuilder.Append($"클라우드: {new DateTime(saveData[i].SaveTime):yyyy.MM.dd HH:mm:ss}").Append("\r\n");
										differentBuilder.Append($"기기: {new DateTime(localSaveData.SaveTime):yyyy.MM.dd HH:mm:ss}").Append("\r\n\r\n");

										differentID.Add(i);
									}
								}
							}
						}
						catch (FileNotFoundException e)
						{
							Debug.WriteLine($"세이브 파일 없음: {e.Message}");

							if (saveData[i] != null)
							{
								differentBuilder.Append(GetSaveName(i)).Append("\r\n");
								differentBuilder.Append("클라우드 데이터만 존재").Append("\r\n\r\n");

								differentID.Add(i);
							}
						}
					}

					if (differentID.Count > 0)
					{
						var differentDialog = new MessageDialog("클라우드/기기간 데이터 동기화가 되어 있지 않습니다. 어느 데이터를 사용하시겠습니까?\r\n\r\n" + differentBuilder.ToString());
						differentDialog.Commands.Add(new UICommand("클라우드"));
						differentDialog.Commands.Add(new UICommand("기기"));

						differentDialog.DefaultCommandIndex = 0;
						differentDialog.CancelCommandIndex = 1;

						var chooseData = await differentDialog.ShowAsync();
						if (chooseData.Label == "클라우드")
						{
							for (var i = 0; i < differentID.Count; i++)
							{
								if (saveData[differentID[i]] != null)
								{
									string idStr;
									if (differentID[i] == 0)
										idStr = "";
									else
										idStr = differentID[i].ToString();

									var saveFile = await storageFolder.CreateFileAsync($"mysticSave{idStr}.dat", CreationCollisionOption.ReplaceExisting);
									await FileIO.WriteTextAsync(saveFile, JsonConvert.SerializeObject(saveData[differentID[i]]));
								}
							}
						}
					}
				}

				InitializeKeyEvent();
			}
			else
			{
				await new MessageDialog("클라우드 서버에 접속할 수 없습니다. 기기에 저장된 세이브를 사용합니다.").ShowAsync();
				InitializeKeyEvent();
			}
		}

		private void InitializeKeyEvent()
		{
			SyncPanel.Visibility = Visibility.Collapsed;
			prologControl.Visibility = Visibility.Visible;
			mTargetVelocity = mTargetSpeed;

			TypedEventHandler<CoreWindow, KeyEventArgs> mainPageKeyUpEvent = null;
			mainPageKeyUpEvent = async (sender, args) =>
			{
				Debug.WriteLine($"키보드 테스트: {args.VirtualKey}");

				if (prologControl.Visibility == Visibility.Visible)
				{
					mTargetVelocity = 0;
					prologControl.Visibility = Visibility.Collapsed;

					mainmenuPanel.Visibility = Visibility.Visible;
				}
				else
				{
					if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						if (mFocusItem == 0)
						{
							Window.Current.CoreWindow.KeyUp -= mainPageKeyUpEvent;

							var dialog = new InputNameBox();
							var result = await dialog.ShowAsync();

							if (result == ContentDialogResult.Primary)
							{
								if (dialog.PlayerName == "")
									Window.Current.CoreWindow.KeyUp += mainPageKeyUpEvent;
								else
									Frame.Navigate(typeof(NewGamePage), dialog.PlayerName);
							}
							else
								Window.Current.CoreWindow.KeyUp += mainPageKeyUpEvent;
						}
						else if (mFocusItem == 1)
						{
							var saveFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("mysticSave.dat");
							if (saveFile == null)
							{
								await new MessageDialog("저장된 게임이 없습니다. 새로운 게임을 시작해 주십시오.", "저장된 게임 없음").ShowAsync();
							}
							else
							{
								Window.Current.CoreWindow.KeyUp -= mainPageKeyUpEvent;
								Frame.Navigate(typeof(GamePage), null);
							}
						}
						else
						{
							await new MessageDialog("원작: 다크 메이지 실리안 카미너스(안영기, 1994)\r\n\r\n" +
							"음악: \r\n" +
							"Town, Ground: https://www.zapsplat.com/\r\n" +
							"Den: https://juhanijunkala.com/\r\n" +
							"Keep: https://opengameart.org/content/boss-battle-theme", "저작권 정보").ShowAsync();
						}
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown) {
						mFocusItem = (mFocusItem + 1) % mMainMenuItemList.Count;

						for (var i = 0; i < mMainMenuItemList.Count; i++) {
							mMainMenuItemList[i].Foreground = mFocusItem == i
								? new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff))
								: new SolidColorBrush(Color.FromArgb(0xff, 0x55, 0x55, 0xff));
						}
					}
					else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
					{
						if (mFocusItem == 0)
							mFocusItem = mMainMenuItemList.Count - 1;
						else
							mFocusItem--;

						for (var i = 0; i < mMainMenuItemList.Count; i++)
						{
							mMainMenuItemList[i].Foreground = mFocusItem == i
								? new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff))
								: new SolidColorBrush(Color.FromArgb(0xff, 0x55, 0x55, 0xff));
						}
					}
				}
			};

			Window.Current.CoreWindow.KeyUp += mainPageKeyUpEvent;
		}

		private void prologControl_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			var stops = new CanvasGradientStop[]
			{
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.0f },
				new CanvasGradientStop() { Color=Color.FromArgb(0xff, 0x53, 0xef, 0xef), Position = 0.1f },
				new CanvasGradientStop() { Color=Color.FromArgb(0xff, 0x53, 0xef, 0xef), Position = 0.9f },
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 1.0f }
			};

			mTextOpacityBrush = new CanvasLinearGradientBrush(sender, stops, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Premultiplied);

			stops = new CanvasGradientStop[]
			{
				new CanvasGradientStop() { Color=Colors.White, Position=0.0f },
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.3f },
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.7f },
				new CanvasGradientStop() { Color=Colors.White, Position = 1.0f },
			};

			mBlurOpacityBrush = new CanvasLinearGradientBrush(sender, stops, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Premultiplied);
		}

		private void prologControl_Update(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
		{
			float height = (float)sender.Size.Height;
			float totalHeight = characters.Length * mLineHeight + height;

			//if (mOffset >= 400)
			//    return;

			mVelocity = mVelocity * 0.90f + mTargetVelocity * 0.10f;

			mOffset = mOffset + mVelocity;

			mOffset = mOffset % totalHeight;
			while (mOffset < 0)
				mOffset += totalHeight;

			float top = height - mOffset;
			mFirstLine = Math.Max(0, (int)(-top / mLineHeight));
			mLastLine = Math.Min(characters.Length, (int)((height + mLineHeight - top) / mLineHeight));
		}

		private void prologControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			var textDisplay = GenerateTextDisplay(sender, (float)sender.Size.Width, (float)sender.Size.Height);

			var blurEffect = new GaussianBlurEffect()
			{
				Source = textDisplay,
				BlurAmount = 10
			};

			mTextOpacityBrush.StartPoint = mBlurOpacityBrush.StartPoint = new Vector2(0, 0);
			mTextOpacityBrush.EndPoint = mBlurOpacityBrush.EndPoint = new Vector2(0, (float)sender.Size.Height);

			var ds = args.DrawingSession;

			//using (ds.CreateLayer(mBlurOpacityBrush))
			//{
			//    ds.DrawImage(blurEffect);
			//}

			using (ds.CreateLayer(mTextOpacityBrush))
			{
				ds.DrawImage(textDisplay);
			}
		}

		private CanvasCommandList GenerateTextDisplay(ICanvasResourceCreator resourceCreator, float width, float height)
		{
			var cl = new CanvasCommandList(resourceCreator);

			using (var ds = cl.CreateDrawingSession())
			{
				float top = height - mOffset;

				float center = width / 2.0f;
				float symbolPos = center - 5.0f;
				float labelPos = center + 5.0f;

				for (int i = mFirstLine; i < mLastLine; ++i)
				{
					float y = top + mLineHeight * i;
					int index = i;

					if (index < characters.Length)
					{
						ds.DrawText(characters[index], labelPos, y, Color.FromArgb(0xff, 0x53, 0xef, 0xef), symbolText);
					}
				}
			}

			return cl;
		}

		private static string[] characters = new string[]
		{
			" 오래 전부터 만날 운명이었던 당신에게.",
			"",
			" 나는 시공의 여행자인 타임워커 알비레오라고 하오.  내가 로어 세",
			"계가 있는 공간으로 접어 들었을 때  마침 이 세계는 로드안의 통치",
			"에 있었소.  그 때 내가 그대들에게 닥쳐 올 불확실한 미래를  나의",
			"다섯 가지 예언으로 계시해 주었던 것을 기억 할거요. 그리고 그 예",
			"언이 너무나도 큰 희생을 강요했던 것도 기억하고 있을 거라 믿소.",
			"나는 세상을 구하기 위해 자신을 희생 해야만 했던 그를 이해하오.",
			"또한 숙명의 적이었던 네크로만서와 불멸의 다크메이지 실리안 카미",
			"너스 역시 나는 용서 할 수 있었소.  하지만 그들을 용서하는데에는",
			"대단한 용기를 감수해야만 했다오. 그들의 진심을 이해하기 위해 나",
			"는 서둘러 로어 세계의 과거로 도약을 시작했고 거기서 내가 보았던",
			"것은 네크로만서와 다크메이지의 계획된 탄생이었소.  내가 그 계기",
			"로 진정한 이 세계의 진실을 알게 되었을 때,  나 스스로가 이 세계",
			"에 대한 경멸로 마음을 닫아 버렸소.  그래서 나를 이해 할 수 있을",
			"것같은 당신에게 이렇게 도움을 청하는 거요.  당신이 네크로만서와",
			"다크메이지를 완전히 소멸 시키는 그 날, 이 세계는 다시 원래의 운",
			"명의 궤도로 접어들게 되고 나는 그때야 비로소 다른 공간으로의 여",
			"행을 다시 시작하게 될거요.",
			"",
			"                                     타임워커 알비레오로 부터",
		};
	}
}
