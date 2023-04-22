using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI
{
    public class Message
    {
        public HorizontalAlignment MsgAlignment { get; set; }
        public Brush BgColor { get; set; }
        
        public string MsgText { get; set; }
        public DateTime MsgDateTime { get; set; }
    }
   
}
