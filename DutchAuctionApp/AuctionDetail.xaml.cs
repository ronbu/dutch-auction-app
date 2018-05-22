using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NForza.Transit;
using Xamarin.Forms;

namespace DutchAuctionApp
{
    public partial class AuctionDetail : ViewCell
    {
        HttpClient hc = new HttpClient();
        
        public AuctionDetail()
        {
            InitializeComponent();
        }

        public async Task<bool> BidAuction(Auction a)
        {
            var bs = new MemoryStream();
            var w = TransitFactory.Writer<Object>(TransitFactory.Format.Json, bs);

            var d = new Dictionary<IKeyword, Object>();
            d[TransitFactory.Keyword("title")] = a.Title;
            d[TransitFactory.Keyword("highestBid")] = a.HighestBid;
            d[TransitFactory.Keyword("highestBidder")] = a.HighestBidder;

            w.Write(d);
            bs.Seek(0, SeekOrigin.Begin);
            var sc = new StreamContent(bs);
            sc.Headers.ContentType = new MediaTypeHeaderValue("application/transit");
            var r = await hc.PostAsync(Config.ServerRoot + "/auction", sc);

            return r.StatusCode == HttpStatusCode.OK;
            
            return true;
        }

        public void BidClicked(object s, object e){
            try
            {
                // TODO: Do not access Elements directly    
                var el = ((this.Parent.Parent as StackLayout).Children[1] as Entry);
                var a = new Auction();
                a.Title = Title.Text;
                a.HighestBid = Int64.Parse(Price.Text);
                a.HighestBidder = el.Text;
                BidAuction(a);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

        }
    }
}
