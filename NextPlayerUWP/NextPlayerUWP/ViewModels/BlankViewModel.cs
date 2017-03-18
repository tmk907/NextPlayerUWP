
using GalaSoft.MvvmLight.Messaging;
using NextPlayerUWP.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels
{
    public class BlankViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        public BlankViewModel()
        {
            Init();
        }

        private void Init()
        {
            abcd(abc);
            Messenger.Default.Register<NotificationMessage<Message1>>(this, abc);
            
        }

        private void abc(NotificationMessage<Message1> message)
        {
            Text1 = "acb";
        }

        private int abcd(Action<NotificationMessage<Message1>> action)
        {
            var info = action.GetMethodInfo();
            if (info == null)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        private string text1 = "";
        public string Text1
        {
            get { return text1; }
            set { Set(nameof(text1), ref text1, value); }
        }
    }
}
