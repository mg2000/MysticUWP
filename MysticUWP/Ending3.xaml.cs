using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
	public sealed partial class Ending3 : Page
	{
		public Ending3()
		{
			this.InitializeComponent();

			TypedEventHandler<CoreWindow, KeyEventArgs> ending3PageKeyUpEvent = null;
			ending3PageKeyUpEvent = async (sender, args) =>
			{
				Window.Current.CoreWindow.KeyUp -= ending3PageKeyUpEvent;
				Frame.Navigate(typeof(CreditPage), null);
			};

			Window.Current.CoreWindow.KeyUp += ending3PageKeyUpEvent;
		}
	}
}
