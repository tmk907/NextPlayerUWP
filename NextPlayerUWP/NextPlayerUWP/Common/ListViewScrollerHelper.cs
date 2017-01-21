using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Common
{
    public class ListViewScrollerHelper
    {
        public ListView listView { get; set; }
        public int firstVisibleIndex { get; set; }
        public string positionKey { get; set; }

        public ListViewScrollerHelper()
        {
            firstVisibleIndex = 0;
            positionKey = "";
            listView = null;
        }

        public void GetPositionKey()
        {
            if (listView != null)
            {
                positionKey = ListViewPersistenceHelper.GetRelativeScrollPosition(listView, ItemToKeyHandler);
            }
            else
            {
                positionKey = "";
            }
        }

        public void GetFirstVisibleIndex()
        {
            var isp = (ItemsStackPanel)listView?.ItemsPanelRoot;
            if (isp != null)
            {
                firstVisibleIndex = isp.FirstVisibleIndex;
            }
            else
            {
                firstVisibleIndex = 0;
            }
        }

        public void ScrollAfterTrackChanged(int index)
        {
            if (listView!=null && listView.Items.Count > 3)
            try
            {
                var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
                if (isp == null) return;
                int firstVisibleIndex = isp.FirstVisibleIndex;
                int lastVisibleIndex = isp.LastVisibleIndex;
                if (index <= lastVisibleIndex && index >= firstVisibleIndex) return;
                if (index < firstVisibleIndex)
                {
                    listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
                }
                else if (index > lastVisibleIndex)
                {
                    listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Default);
                }
            }
            catch (Exception ex)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("NowPlayingPlaylsitViewModel ScrollAfterTrackChanged " + ex.ToString(), NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
            }
        }

        public async Task ScrollToPosition()
        {
            if (listView != null && listView.Items.Count > 3 && listView.Items.Count > firstVisibleIndex) 
            {
                listView.ScrollIntoView(listView.Items[firstVisibleIndex], ScrollIntoViewAlignment.Leading);
                if (!String.IsNullOrEmpty(positionKey))
                {
                    await ListViewPersistenceHelper.SetRelativeScrollPositionAsync(listView, positionKey, KeyToItemHandler);
                }
            }
        }

        private string ItemToKeyHandler(object item)
        {
            if (item == null) return null;
            MusicItem mi = (MusicItem)item;
            return mi.GetParameter();
        }

        private IAsyncOperation<object> KeyToItemHandler(string key)
        {
            return Task.Run(() =>
            {
                if (listView.Items.Count <= 0)
                {
                    return null;
                }
                else
                {
                    var i = listView.Items[firstVisibleIndex];
                    if (((MusicItem)i).GetParameter() == key)
                    {
                        return i;
                    }
                    foreach (var item in listView.Items)
                    {
                        if (((MusicItem)item).GetParameter() == key) return item;
                    }
                    return null;
                }
            }).AsAsyncOperation();
        }
    }
}
