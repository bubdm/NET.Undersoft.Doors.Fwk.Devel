using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Collections.Concurrent;
using System.Collections;
using System.ComponentModel;
using System.Doors;
using System.Doors.Drive;

namespace System.Doors.Data
{   
   
    [JsonObject]
    [Serializable]
    public class DataFavorite : IDataGridBinder, IListSource, IDataConfig
    {
        private FavoriteType mode;

        public DataFavorites Favorites;

        public DataFavorite()
        {            
            FavoriteId = "Favorite#" + DateTime.Now.ToFileTime().ToString();
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();
            DisplayName = FavoriteId;
            Parameters = new DataParams();
        }
        public DataFavorite(string favoriteId, DataFavorites favorites)
        {
            Favorites = favorites;
            FavoriteId = (favoriteId != null) ? favoriteId : "Favorite#" + DateTime.Now.ToFileTime().ToString();
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();
            DisplayName = FavoriteId;
            Parameters = new DataParams();
        }

        public string FavoriteId
        { get; set; }
        public string DisplayName
        { get; set; }

        public DataConfig Config
        { get; set; }
        public DataState State
        { get; set; }

        public bool Checked
        {
            get
            {
                return State.Checked;
            }
            set
            {
                State.Checked = value;
            }
        }
        public bool Synced
        { get { return State.Synced; } set { State.Synced = value; } }
        public bool Saved
        { get { return State.Saved; } set { State.Saved = value; } }

        public DataParams Parameters
        { get; set; }

        public FavoriteType FavoriteObject
        { get { return mode; } set { mode = value; Favorite = CloneFavorite(mode); } }

        public Type ObjectType
        { get { return Favorite.GetType(); } }

        public object Favorite
        { get; set; } = new object();

        public object CloneFavorite(FavoriteType mode)
        {
            switch(mode)
            {                
                case FavoriteType.Filter:
                    return Favorites.Trell.Filter.Clone();                    
                case FavoriteType.Sort:
                    return Favorites.Trell.Sort.Clone();                    
                case FavoriteType.Pylons:
                    return Favorites.Trell.Pylons.Clone();                   
                case FavoriteType.Sims:
                    return Favorites.Trell.SimsView.Clone();                    
                default:
                    return new object();                  
            }
        }

        #region IDataGridBinder
        public object BindingSource
        { get { return Favorites; } }

        public IDataGridStyle GridStyle
        { get; set; }
        public IDepotSync DepotSync
        { get { return BoundedGrid.DepotSync; } set { BoundedGrid.DepotSync = value; } }
        public IDataGridBinder BoundedGrid
        { get { return Favorites.BoundedGrid; } set { Favorites.BoundedGrid = value; } }

        IList IListSource.GetList()
        {
            return (IBindingList)BindingSource;
        }
        bool IListSource.ContainsListCollection
        {
            get
            {
                if (this.GetType().GetInterfaces().Contains(typeof(IDataGridBinder)))
                    if (BindingSource != null)
                        return true;
                return false;
            }
        }
        #endregion

    }

    public enum FavoriteType
    {
        None,
        Pylons,
        Filter,
        Sort,
        Sims
    }

}
