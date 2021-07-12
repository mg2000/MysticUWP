using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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
	public sealed partial class Ending2 : Page
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

		public Ending2()
		{
			this.InitializeComponent();

			TypedEventHandler<CoreWindow, KeyEventArgs> ending2PageKeyUpEvent = null;
			ending2PageKeyUpEvent = async (sender, args) =>
			{
				Window.Current.CoreWindow.KeyUp -= ending2PageKeyUpEvent;
				Frame.Navigate(typeof(CreditPage), null);
			};

			Window.Current.CoreWindow.KeyUp += ending2PageKeyUpEvent;

			mTargetVelocity = mTargetSpeed;
		}

		private void endingControl_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			var stops = new CanvasGradientStop[]
			{
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.0f },
				new CanvasGradientStop() { Color=Color.FromArgb(0xff, 0xff, 0x00, 0x00), Position = 0.1f },
				new CanvasGradientStop() { Color=Color.FromArgb(0xff, 0xff, 0x00, 0x00), Position = 0.9f },
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

		private void endingControl_Update(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
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

		private void endingControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
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
						ds.DrawText(characters[index], labelPos, y, Color.FromArgb(0xff, 0xff, 0x00, 0x00), symbolText);
					}
				}
			}

			return cl;
		}

		private static string[] characters = new string[]
		{
			" 새로운 로드안이 된 당신에게..",
			"",
			" 역시 당신은 나의 기대를 저버리지 않았소. 나 알비레오는 그저 로",
			"어세계의 역사에 대해  방관자적인 입장일뿐이었지만  비뚤어져가는",
			"로어세계의 미래를 그냥 보고만 있을 수는 없어서  당신을 선택하게",
			"되었고 당신을 이런 모험에 말려 들게 했던 것이오.  이제 위선자는",
			"사라졌고  로드안의 칭호도 당신에게 돌아갔소.  하지만 당신에게는",
			"아직 2가지의 과제가 더 남아있소.  첫째는 네크로만서가 된 카노푸",
			"스가 이땅에 내려올때 벌어지게 되고  둘째는 멸망당한 네종족의 원",
			"혼이 부활할 때 발생하게 될거요.  이 일들은 약 100년 후에 발생할",
			"것이오. 하지만 당신이라면 충분히 이겨낼 수 있을거라 믿소.",
			"",
			" 나는  이제 이 세계를 떠날 때가 되었소.  아직도 내가 가보아야할",
			"차원이 수없이 많이있기 때문이오.  로어 세계가 있는 이곳도  그저",
			"내가 스쳐갔던 수 많은 차원들 중의 하나로  기억될지도 모르겠지만",
			"쉽게 잊어 버리기에는 너무 아까운 기억이 많이있소. 그중의 하나는",
			"당신의 존재 때문일거라오.  이제 나는 이곳 역사에 더 이상 신경을",
			"쓰지 않으려고 하오. 앞으로 다시 올 계획도 아직은 없소.  그저 영",
			"원의 차원으로 나의 여행을 계속 할 뿐이라오.",
			"",
			"                                    타임워커 알비레오로 부터.",
		};
	}
}
