using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MysticUWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class NewGamePage : Page
	{
		private Lore mPlayer = new Lore();

		private FocusItem mFocusItem = FocusItem.Gender;

		private List<TextBlock> mAnswerList = new List<TextBlock>();
		private int mAnswer = 0;

		private List<QuestionInfo> mQuestionList = new List<QuestionInfo>();
		private int mQuestionIdx;
		private int[] mTransData = new int[5];

		private Random mRand = new Random();

		private List<TextBlock> mAssignCursorList = new List<TextBlock>();
		private List<TextBlock> mAssignValueList = new List<TextBlock>();
		private int mAssignID = 0;
		private int mRemainPoint = 40;

		public NewGamePage()
		{
			this.InitializeComponent();

			mAnswerList.Add(Answer1);
			mAnswerList.Add(Answer2);
			mAnswerList.Add(Answer3);

			mAssignCursorList.Add(AgilityCursorText);
			mAssignCursorList.Add(AccuracyCursorText);
			mAssignCursorList.Add(LuckCursorText);

			mAssignValueList.Add(AgilityText);
			mAssignValueList.Add(AccuracyText);
			mAssignValueList.Add(LuckText);

			var question = new string[] {
				"당신이 한 밤중에 공부하고 있을때 밖에서 무슨 소리가 들렸다",
				"당신이 한 시골에서 버스를 타고가던 중  깊 앞을 막고 쓰러져 있는 나무 때문에 버스가 지나갈 수 없게 되었다."
			};

			var answer = new string[][] {
				new string[] {
					"1] 밖으로 나가서 알아본다",
					"2] 그 소리가 무엇일까 생각을 한다",
					"3] 공부에만 열중한다"
				},
				new string[] {
					"1] 버스에서 내려 나무를 치운다",
					"2] 남이 치울거라 생각하며 기다린다",
					"3] 버스 안에서 나무를 치룰 방법을 궁리한다'"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 0, 1, 2 }));

			question = new string[] {
				"당신은 체력장 오래달리기에서 포기할 수 없는 한 바퀴를 남겨 놓고 거의 탈진 상태가 되었다",
				"당신은 별것도 아닌데 다른 사람에게 심한 모욕을 당했다. 이때 당신의 행동은 ?"
			};

			answer = new string[][] {
				new string[] {
					"1] 힘으로 밀고 나간다",
					"2] 정신력으로 버티며 달린다",
					"3] 그래도 여태까지와 마찬가지로 달린다"
				},
				new string[] {
					"1] 당장 그 사람과 싸운다",
					"2] 그냥 그 일을 잊어 버린다",
					"3] 속에서는 끓어 오르지만 참고 넘어간다"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 0, 1, 3 }));

			question = new string[] {
				"당신은 이 게임 속에서 적들에게 완전히 포위되어 승산 없이 싸우고 있다",
				"당신은 별것도 아닌데 다른 사람에게 심한 모욕을 당했다. 이때 당신의 행동은 ?"
			};

			answer = new string[][] {
				new string[] {
					"1] 힘이 남아 있는한 죽을때까지 싸운다",
					"2] 한가지라도 탈출할 가능성을 찾는다",
					"3] 일단 싸우면서 여러 방법을 생각한다"
				},
				new string[] {
					"1] 탈출을 시도한다",
					"2] 현실을 받아들인다",
					"3] 죽지 않기 위해 강한 저항을 한다"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 0, 1, 4 }));

			question = new string[] {
				"당신은 매우 복잡한 매듭을 풀어야하는 일이 생겼다",
				"당신이 이 게임속에서 벌이는 활약상은 ?"
			};

			answer = new string[][] {
				new string[] {
					"1] 칼로 매듭을 잘라 버린다",
					"2] 매듭의 끝부분 부터 차근차근 훓어본다",
					"3] 어쨌든 계속 풀려고 손을 놀린다"
				},
				new string[] {
					"1] 거대한 검을 휘두르며 적을 무찌르는 당신",
					"2] 상념의 힘만으로 적을 무력화 시키는 당신",
					"3] 쓰러지고 또 쓰러져도 끝까지 싸워 승리하는 당신"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 0, 2, 3 }));

			question = new string[] {
				"허허 벌판을 걸어가던 당신은 갑작스런 우박을 만난다",
				"당신은 해발 5000m 가 넘는 고지에서 눈사태 때문에 고립되었다. 구조대가 오려면 12시간. 그동안 당신은 어떻게 하겠는가 ?"
			};

			answer = new string[][] {
				new string[] {
					"1] 당항한 나머지 피할곳을 찾아 뛴다",
					"2] 침착하게 주위를 살펴 안전한곳을 찾는다",
					"3] 우박 정도는 그냥 견딘다"
				},
				new string[] {
					"1] 힘을 다해 구조대의 눈에 잘 띄는 곳으로 이동한다",
					"2] 만약의 사태를 대비해서 힘을 최대한 아껴둔다",
					"3] 추위를 가볍게 견뎌내며 그곳에서 기다린다"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 0, 2, 4 }));

			question = new string[] {
				"집안에 불이나서 탈출하려는데 나무로 만든 문이 좀처럼 열리지 않는다",
				"당신은 사막을 여행중이다. 오아시스까지는 앞으로 1 km, 당신의 행돈은 ?"
			};

			answer = new string[][] {
				new string[] {
					"1] 다른 탈출구를 찾아간다",
					"2] 1] 번과 같은 불확실한 도전을 하는것 보다는 확실한 탈출구인 이 문을 끝까지 열려한다",
					"3] 나무문이 타서 구멍이 생길때까지 기다려 탈출한다"
				},
				new string[] {
					"1] 체력을 소모해서라도 좀 더 빨리 걸음을 재촉한다",
					"2] 여태껏 걸어왔던것처럼 꿋꿋하게 간다",
					"3] 당신은 원래 더위를 타지 않아서 빨리갈 필요가 없다"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 0, 3, 4 }));

			question = new string[] {
				"고대에 태어난 당신은, 한날 당신의 눈앞에서 물체가 사라지는 마술을 보았을때 당신의 해석은 ?",
				"이 게임 속에서는 당신이 주인공이며 전능해질 수있다. 그렇다면 지금 당신 앞에있는 수많은 괴물들을 어떻게 처리하겠는가 ?"
			};

			answer = new string[][] {
				new string[] {
					"1] 이것은 마법이다",
					"2] 이것은 사람의 새로운 능력이다",
					"3] 단순한 사람의 눈속임이다"
				},
				new string[] {
					"1] 화염의 폭풍으로 전부 태워버린다",
					"2] 적의 마음을 조작하여 적개심을 없에 버린다",
					"3] 핼버드로 적을 종횡무진 쓰러뜨린다"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 1, 2, 3 }));

			question = new string[] {
				"시험 기간에 당신이 도서관에서 공부를 하려는데 주위가 너무 시끄럽다",
				"당신은 레드 드래곤에 의해 궁지에 몰리게 되었다. 레드 드래곤은 마지막 일격으로 화염을 뿜었다. 당신의 대응은 ?"
			};

			answer = new string[][] {
				new string[] {
					"1] 상관없이 참으며 공부한다",
					"2] 너무 공부를 열심히해서 그런 소리가 안 들린다",
					"3] 시끄러움에 대항하는 마음으로 공부한다"
				},
				new string[] {
					"1] 남극의 브리자드를 불러와서 화염을 무력화 시킨다",
					"2] 당신 앞의 땅이 솟아 오르게 하여 화염을 차단 시킨다",
					"3] 당신의 신체를 불에 견딜 수 있는 상태로 바꾸어 버린다"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 1, 2, 4 }));

			question = new string[] {
				"직장 생활을 하던 당신은 아무 이유없이 상관에게 심한 욕을 들었다",
				"당신은 게임속에서 거대한 악마를 만났다. 당신은 혼자뿐이고 이길 승산은 희박했다. 어떻게 할 것인가 ?"
			};

			answer = new string[][] {
				new string[] {
					"1] 겉으로는 순종하면서 속으로는 감정을 샇는다",
					"2] 웬만하면 참고 넘긴다",
					"3] 상관인걸 무시하고 이유를 들라며 대든다"
				},
				new string[] {
					"1] 일단 싸우다가 불리하다고 판단되면 도망간다.",
					"2] 끝까지 악마와 싸운다",
					"3] 재빨리 몸을 숨긴다"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 1, 3, 4 }));

			question = new string[] {
				"당신이 새로운 프로그램을 짜던중 알수없는 오류가 생겼다",
				"지금은 당신의 국민학교 1 학년 시절. 산수 시험 문제지를 받고보니 앞이 깜깜했다. 당신은 어떻게 하겠는가?"
			};

			answer = new string[][] {
				new string[] {
					"1] 차근차근 순서도를 생각하며 오류를 찾는다",
					"2] 여러번 실행 시키며 오류를 찾는다",
					"3] 오류가 작으면 그냥 사용한다"
				},
				new string[] {
					"1] 배운대로 차근차근 풀어본다",
					"2] 모든 능력(?)을 동원하여 답을 알아낸다",
					"3] 그냥 몇대 맞을 셈치고 백지를 낸다"
				}
			};

			mQuestionList.Add(new QuestionInfo(question, answer, new int[] { 2, 3, 4 }));

			TypedEventHandler<CoreWindow, KeyEventArgs> newGamePageKeyEvent = null;
			newGamePageKeyEvent = async (sender, args) =>
			{
				void FocusMenuItem()
				{
					for (var i = 0; i < mAnswerList.Count; i++)
					{
						if (i == mAnswer)
							mAnswerList[i].Foreground = new SolidColorBrush(Colors.Yellow);
						else
							mAnswerList[i].Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x28, 0xf3, 0xf3));
					}
				}

				void ShowQuestion()
				{
					var id = mRand.Next(2);

					QuestionTitle.Text = mQuestionList[mQuestionIdx].Question[id];

					mAnswer = 0;
					for (var i = 0; i < 3; i++)
					{
						mAnswerList[i].Text = mQuestionList[mQuestionIdx].Answer[id][i];
					}

					FocusMenuItem();
				}

				if (mFocusItem == FocusItem.Gender)
				{
					if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft)
					{
						mPlayer.Gender = GenderType.Male;

						GenderMale.Foreground = new SolidColorBrush(Colors.Yellow);
						GenderFemale.Foreground = new SolidColorBrush(Colors.White);
					}
					else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight)
					{
						mPlayer.Gender = GenderType.Female;

						GenderMale.Foreground = new SolidColorBrush(Colors.White);
						GenderFemale.Foreground = new SolidColorBrush(Colors.Yellow);
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						mFocusItem = FocusItem.Question;

						GenderMale.Foreground = new SolidColorBrush(Colors.White);
						GenderFemale.Foreground = new SolidColorBrush(Colors.White);

						QuestionPanel.Visibility = Visibility.Visible;

						ShowQuestion();
					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
					{
						Window.Current.CoreWindow.KeyUp -= newGamePageKeyEvent;
						Frame.Navigate(typeof(MainPage));
					}
				}
				else if (mFocusItem == FocusItem.Question)
				{
					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						if (mAnswer == 0)
							mAnswer = mAnswerList.Count - 1;
						else
							mAnswer--;

						FocusMenuItem();
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						mAnswer = (mAnswer + 1) % mAnswerList.Count;

						FocusMenuItem();
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						mTransData[mQuestionList[mQuestionIdx].TransIdx[mAnswer]]++;

						mQuestionIdx++;

						if (mQuestionIdx < mQuestionList.Count)
							ShowQuestion();
						else
						{
							for (var i = 0; i < 5; i++)
							{
								switch (mTransData[i])
								{
									case 0:
										mTransData[i] = 5;
										break;
									case 1:
										mTransData[i] = 7;
										break;
									case 2:
										mTransData[i] = 11;
										break;
									case 3:
										mTransData[i] = 14;
										break;
									case 4:
										mTransData[i] = 17;
										break;
									case 5:
										mTransData[i] = 19;
										break;
									case 6:
										mTransData[i] = 20;
										break;
									default:
										mTransData[i] = 10;
										break;
								}
							}

							mPlayer.Strength = mTransData[0];
							mPlayer.Mentality = mTransData[1];
							mPlayer.Concentration = mTransData[2];
							mPlayer.Endurance = mTransData[3];
							mPlayer.Resistance = mTransData[4];

							if (mPlayer.Strength > mPlayer.Mentality)
								mPlayer.ClassType = ClassCategory.Sword;
							else if (mPlayer.Mentality > mPlayer.Strength)
								mPlayer.ClassType = ClassCategory.Magic;
							else if (mPlayer.Endurance > mPlayer.Concentration)
								mPlayer.ClassType = ClassCategory.Sword;
							else if (mPlayer.Concentration > mPlayer.Endurance)
								mPlayer.ClassType = ClassCategory.Magic;
							else
								mPlayer.ClassType = ClassCategory.Magic;
							

							var addPoint = 4;
							if (mPlayer.Gender == GenderType.Male)
							{
								mPlayer.Strength += addPoint;
								if (mPlayer.Strength <= 20)
									addPoint = 0;
								else
								{
									addPoint = mPlayer.Strength - 20;
									mPlayer.Strength = 20;
								}

								mPlayer.Endurance += addPoint;
								if (mPlayer.Endurance <= 20)
									addPoint = 0;
								else
								{
									addPoint = mPlayer.Endurance - 20;
									mPlayer.Endurance = 20;
								}

								mPlayer.Resistance += addPoint;
								if (mPlayer.Resistance > 20)
									mPlayer.Resistance = 20;
							}
							else
							{
								mPlayer.Mentality += addPoint;
								if (mPlayer.Mentality <= 20)
									addPoint = 0;
								else
								{
									addPoint = mPlayer.Mentality - 20;
									mPlayer.Mentality = 20;
								}

								mPlayer.Concentration += addPoint;
								if (mPlayer.Concentration <= 20)
									addPoint = 0;
								else
								{
									addPoint = mPlayer.Concentration - 20;
									mPlayer.Endurance = 20;
								}

								mPlayer.Resistance += addPoint;
								if (mPlayer.Resistance > 20)
									mPlayer.Resistance = 20;
							}

							mPlayer.HP = mPlayer.Endurance * 10;
							mPlayer.SP = mPlayer.Mentality * 10;

							StrengthText.Text = mPlayer.Strength.ToString();
							MentalityText.Text = mPlayer.Mentality.ToString();
							ConcentrationText.Text = mPlayer.Concentration.ToString();
							EnduranceText.Text = mPlayer.Endurance.ToString();
							ResistanceText.Text = mPlayer.Resistance.ToString();

							HPText.Text = mPlayer.HP.ToString();
							if (mPlayer.ClassType == ClassCategory.Magic)
								SPText.Text = mPlayer.SP.ToString();
							else
								SPText.Text = "0";

							QuestionPanel.Visibility = Visibility.Collapsed;
							StatPanel.Visibility = Visibility.Visible;

							for (var i = 0; i < 3; i++)
								mTransData[i] = 0;

							mFocusItem = FocusItem.AssignPoint;
						}
					}
				}
				else if (mFocusItem == FocusItem.AssignPoint)
				{
					void UpdateCursor()
					{
						for (var i = 0; i < mAssignCursorList.Count; i++)
						{
							if (mAssignID == i)
								mAssignCursorList[i].Visibility = Visibility.Visible;
							else
								mAssignCursorList[i].Visibility = Visibility.Collapsed;
						}
					}

					void UpdateStat()
					{
						RemainPointText.Text = mRemainPoint.ToString();

						mAssignValueList[mAssignID].Text = mTransData[mAssignID].ToString();
					}

					if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft)
					{
						if (mTransData[mAssignID] > 0)
						{
							mTransData[mAssignID]--;
							mRemainPoint++;

							UpdateStat();
						}
					}
					else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight)
					{
						if (mTransData[mAssignID] < 20 && mRemainPoint > 0)
						{
							mTransData[mAssignID]++;
							mRemainPoint--;

							UpdateStat();
						}
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						mAssignID = (mAssignID + 1) % mAssignCursorList.Count;

						UpdateCursor();
					}
					else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						if (mAssignID == 0)
							mAssignID = mAssignCursorList.Count - 1;
						else
							mAssignID--;

						UpdateCursor();
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						if (mRemainPoint > 0)
						{
							await new MessageDialog("남은 포인트를 모두 할당해 주십시오.", "할당 미완료").ShowAsync();
						}
						else
						{
							mPlayer.Agility = mTransData[0];
							mPlayer.Accuracy = mTransData[1];
							mPlayer.Luck = mTransData[2];

							Window.Current.CoreWindow.KeyUp -= newGamePageKeyEvent;
							//Frame.Navigate(typeof(ChooseClassPage), mPlayer);
						}
					}
				}
			};

			Window.Current.CoreWindow.KeyUp += newGamePageKeyEvent;
		}

		private class QuestionInfo
		{
			public string[] Question
			{
				get;
				private set;
			}

			public string[][] Answer
			{
				get;
				private set;
			}

			public int[] TransIdx
			{
				get;
				private set;
			}

			public QuestionInfo(string[] question, string[][] answer, int[] transIdx)
			{
				Question = question;
				Answer = answer;
				TransIdx = transIdx;
			}
		}

		private enum FocusItem
		{
			Gender,
			Question,
			AssignPoint,
			SelectClass,
			CompleteCreate,
			SelectFriend,
			Confirm
		}
	}
}
