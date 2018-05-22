using System;
using Xamarin.Forms;

namespace DutchAuctionApp
{
    public class AuctionDetail : ViewCell
    {

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create("Title", typeof(string), typeof(AuctionDetail));

        public AuctionDetail(){
            var p = this.Parent;
            //var tp = (TitleProperty);
        }
    }
}
