using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MidtermProject
{
    class MailViewModel
    {
        public ObservableCollection<Mail> allMails;
        public ObservableCollection<Mail> receive_mails;
        public ObservableCollection<Mail> send_mails;
        public Mail selectmail;

        public string Addmail(int judge, string send, string receiver,string title, string time, string content)
        {
            string id = Guid.NewGuid().ToString();
            switch (judge)
            {
                case 0:
                    send_mails.Insert(0,new Mail { sender = send, receiver = receiver, title = title, content = content, time = time });
                    break;
                case 1:
                    receive_mails.Insert(0, new Mail { sender = send, receiver = receiver, title = title, content = content, time = time });
                    break;
            }
            return id;
        }



        public MailViewModel()
        {
            allMails = new ObservableCollection<Mail>();
            receive_mails = new ObservableCollection<Mail>();
            send_mails = new ObservableCollection<Mail>();
            Addmail(0, "szy", "tanxiao","hello", "2015", "123");
            Addmail(1, "tanxiao", "szy","hi", "2016", "456");
        }
        public void RemoveItem()
        {
            this.allMails.Remove(this.selectmail);
            this.selectmail = null;
        }
    }


}
