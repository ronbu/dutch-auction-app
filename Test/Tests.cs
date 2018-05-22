using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using DutchAuctionApp;

namespace Test
{
    [TestFixture]
    public class Tests
    {
        public Config Config = new Config();

        [TestFixtureSetUp]
        public async void Init()
        {
            var r = await new AuctionsPage().Login("user"); 
        }

        [Test]
        public async Task Login()
        {
            var r = await new AuctionsPage().Login("testUsername");
            Assert.IsTrue(r);
        }

        [Test]
        public async Task Create()
        {
            var a = new Auction();
            a.Creator = "user";
            a.InitialPrice = 12334;
            a.Title = "newTitle";
            var r = await new AuctionsPage().CreateAuction(a);
            Assert.IsTrue(r);
        }
        
//        [Test, Order(3)]
//        public void Sse()
//        {
//            new AuctionsPage().SubscribeEvents();
//        }
        
        [Test]
        public async Task Bid()
        {
            var a = new Auction();
            a.Title = "newTitle";
            a.HighestBidder = "user";
            a.HighestBid = 12334;
            var r = await new AuctionDetail().BidAuction(a);
            Assert.IsTrue(r);
        }
    }
}
