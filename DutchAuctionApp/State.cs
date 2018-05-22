using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Reducto;

namespace DutchAuctionApp
{
    public class Auction : INotifyPropertyChanged
    {
        string title;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        string creator;

        public string Creator
        {
            get => creator;
            set => SetProperty(ref creator, value);
        }

        DateTime createdAt;

        public DateTime CreatedAt
        {
            get => createdAt;
            set => SetProperty(ref createdAt, value);
        }

        long initialPrice;

        public long InitialPrice
        {
            get => initialPrice;
            set => SetProperty(ref initialPrice, value);
        }

        long highestBid;

        public long HighestBid
        {
            get => highestBid;
            set => SetProperty(ref highestBid, value);
        }

        string highestBidder;

        public string HighestBidder
        {
            get => highestBidder;
            set => SetProperty(ref highestBidder, value);
        }

        long currentPrice;

        public long CurrentPrice
        {
            get => currentPrice;
            set => SetProperty(ref currentPrice, value);
        }

        string state;

        public string State
        {
            get => state;
            set => SetProperty(ref state, value);
        }

        bool isBusy = false;

        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }


    public class State
    {
        public State()
        {
            Auctions = new Dictionary<string, Auction>();
        }

        public string Login { get; set; }
        public Dictionary<String, Auction> Auctions { get; set; }
    }

//    public interface IAction {}

    public struct SetUsername
    {
        public string Username { get; set; }
    }

    public struct Bid
    {
        public long Price { get; set; }
    }

    public struct UpdateAuction
    {
        public Auction Auction;
    }

    public class CreateAuction
    {
    }

    public class Config
    {
        public static string ServerRoot = "http://localhost:3000";

        private SimpleReducer<State> _reducer = new SimpleReducer<State>()
            .When<SetUsername>((state, action) =>
            {
                var a = (SetUsername) action;
                state.Login = a.Username;
                return state;
            })
            .When<UpdateAuction>((state, action) =>
            {
                var a = (UpdateAuction) action;
                state.Auctions[a.Auction.Title] = a.Auction;
                return state;
            });

        public Store<State> Store;
        public Store<State>.AsyncAction<State> Login;

        public Config()
        {
            Store = new Store<State>(_reducer);
            Login = Store.asyncAction<State>(async (dispatch, getState) =>
            {
                var State = (State) getState();
                var hc = new HttpClient();
                var c = new ByteArrayContent(new Byte[0]);

                var r = await hc.PutAsync(Config.ServerRoot + "/login/" + State.Login, c);

                if (r.StatusCode != HttpStatusCode.OK)
                {
                    dispatch(new SetUsername {Username = null});
                }

                return State;
            });
        }
    }
}