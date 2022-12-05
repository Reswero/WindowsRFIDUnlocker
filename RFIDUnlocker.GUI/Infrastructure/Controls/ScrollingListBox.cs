using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;

namespace RFIDUnlocker.GUI.Infrastructure.Controls
{
	internal class ScrollingListBox : ListBox
	{
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems?.Count > 0)
			{
				Border border = (Border)VisualTreeHelper.GetChild(this, 0);
				ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
				scrollViewer.ScrollToBottom();
			}

			base.OnItemsChanged(e);
		}
	}
}
