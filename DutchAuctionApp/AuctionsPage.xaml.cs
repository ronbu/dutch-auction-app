using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Xsl;
using EvtSource;
using Xamarin.Forms;
using NForza.Transit;

namespace DutchAuctionApp
{
    public partial class AuctionsPage : ContentPage
    {
        HttpClient hc = new HttpClient();
        private ObservableCollection<Auction> oc;

        public AuctionsPage()
        {
            InitializeComponent();

            SubscribeEvents();

            oc = new ObservableCollection<Auction>();

            listView.ItemsSource = oc;
        }

        public async Task<bool> Login(string username)
        {
            var c = new ByteArrayContent(new Byte[0]);
            var r = await hc.PutAsync(Config.ServerRoot + "/login/" + username, c);
            return r.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> CreateAuction(Auction a)
        {
            var bs = new MemoryStream();
            var w = TransitFactory.Writer<Object>(TransitFactory.Format.Json, bs);

            var d = new Dictionary<IKeyword, Object>();
            d[TransitFactory.Keyword("title")] = a.Title;
            d[TransitFactory.Keyword("initialPrice")] = a.InitialPrice;
            d[TransitFactory.Keyword("creator")] = login.Text ?? a.Creator;

            w.Write(d);
            bs.Seek(0, SeekOrigin.Begin);
            var sc = new StreamContent(bs);
            sc.Headers.ContentType = new MediaTypeHeaderValue("application/transit");
            var r = await hc.PostAsync(Config.ServerRoot + "/auction", sc);

            return r.StatusCode == HttpStatusCode.OK;
        }

        public void addAuction(Auction a)
        {
            foreach (var oa in oc)
            {
                if (a.Title != oa.Title) continue;
                oa.CurrentPrice = a.CurrentPrice;
                oa.HighestBidder = a.HighestBidder;
                oa.State = a.State;
//                oc.Remove(oa);
//                oc.Add(oa);
                
                return;
            }

            oc.Add(a);
        }

        public void SubscribeEvents()
        {
            new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                var evt = new EventSourceReader(new Uri(Config.ServerRoot + "/updates"));
                evt.MessageReceived += async (
                    object sender,
                    EventSourceMessageEventArgs e) =>
                {
                    try
                    {
                        if (e.Event == "connected") return;
                        var ms = new MemoryStream();
                        var sw = new StreamWriter(ms);
                        sw.Write(e.Message);
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        var r = TransitFactory.Reader(TransitFactory.Format.Json, ms);
                        var d = r.Read<IDictionary<Object, Object>>();

                        var a = new Auction();
                        a.Title = (string) d[TransitFactory.Keyword("title")];
                        a.Creator = (string) d[TransitFactory.Keyword("creator")];
//                        a.CreatedAt = (DateTime) d[TransitFactory.Keyword("createdAt")];
//                        a.InitialPrice = (long) d[TransitFactory.Keyword("initialPrice")];
                        a.HighestBidder = (string) d[TransitFactory.Keyword("highestBidder")];
                        a.CurrentPrice = (long) d[TransitFactory.Keyword("curPrice")];
                        a.State = ((IKeyword) d[TransitFactory.Keyword("state")]).Value;

                        Console.WriteLine(a);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            addAuction(a);       
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                };
                evt.Disconnected += async (object sender, DisconnectEventArgs e) =>
                {
                    await Task.Delay(e.ReconnectDelay);
                    evt.Start();
                };
                evt.Start();
            })).Start();
        }

        void loginClicked(object sender, object e)
        {
            Login(login.Text);
        }

        void createClicked(object sender, object e)
        {
            var a = new Auction();
            a.Creator = login.Text;
            a.InitialPrice = Int64.Parse(price.Text);
            a.Title = title.Text;
            CreateAuction(a);
        }
    }
}