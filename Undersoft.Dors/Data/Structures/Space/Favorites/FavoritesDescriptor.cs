using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Dors;
using System.Dors.Drive;
using System.Reflection;
using System.Linq;

namespace System.Dors.Data
{
    public class FavoriteDescriptor : PropertyDescriptor
    {
        public FavoriteDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                            new DataIdxAttribute()})
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataFavorite); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(object); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            ((DataFavorite)component).Favorite = value;
        }
        public override object GetValue(object component)
        {
            PropertyDescriptorCollection pdc = null;
            DataFavorite parent = ((DataFavorite)component);
            object child = parent.Favorite;
            if (child != null)
            {
                DataSphere sphereOn = parent.Favorites.Trell.Sphere;
                string dataidx = ((IDataConfig)child).Config.DataIdx;
                ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)]).DataIdx = dataidx;
                pdc = ((IDataDescriptor)child).CreateDescriptors(Name, dataidx);

                if (sphereOn != null)
                {
                    pdc = sphereOn.Trells.CreateDescriptors(Name, dataidx, pdc);

                    DataSpheres spheres = sphereOn.SpheresOn;
                    if (spheres != null)
                    {
                        DataSpheres tempspheres = spheres;
                        while (spheres != null)
                        {
                            pdc = spheres.CreateDescriptors(Name, dataidx, pdc);
                            tempspheres = spheres;
                            spheres = spheres.SpheresUp;
                        }
                        spheres = tempspheres;
                        if (spheres.AreaOn != null)
                            pdc = spheres.AreaOn.CreateDescriptors(Name, dataidx, pdc);
                    }
                }

                if (((IDataGridBinder)component).BoundedGrid != null)
                    ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)component).BoundedGrid;
            }
            return child;
        }
    }

    public class FavoritesDescriptor : PropertyDescriptor
    {
        public FavoritesDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                            new DataIdxAttribute()})
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataTrellis); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(DataFavorites); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            ((DataTrellis)component).Favorites = (DataFavorites)value;
        }
        public override object GetValue(object component)
        {
            PropertyDescriptorCollection pdc = null;
            DataTrellis parent = ((DataTrellis)component);
            DataFavorites child = parent.Favorites;
            if (child != null)
            {
                DataSphere sphereOn = parent.Sphere;
                ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)]).DataIdx = child.Config.DataIdx;
                pdc = child.CreateDescriptors(Name, child.Config.DataIdx);

                if (sphereOn != null)
                {
                    pdc = sphereOn.Trells.CreateDescriptors(Name, child.Config.DataIdx, pdc);

                    DataSpheres spheres = sphereOn.SpheresOn;
                    if (spheres != null)
                    {
                        DataSpheres tempspheres = spheres;
                        while (spheres != null)
                        {
                            pdc = spheres.CreateDescriptors(Name, child.Config.DataIdx, pdc);
                            tempspheres = spheres;
                            spheres = spheres.SpheresUp;
                        }
                        spheres = tempspheres;
                        if (spheres.AreaOn != null)
                            pdc = spheres.AreaOn.CreateDescriptors(Name, child.Config.DataIdx, pdc);
                    }
                }

                if (((IDataGridBinder)component).BoundedGrid != null)
                    ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)component).BoundedGrid;

                if (((IDataGridBinder)child).BoundedGrid != null)
                {
                    IDataGridStyle gridStyle = (((IDataGridBinder)child).BoundedGrid).GridStyle;
                    gridStyle.GetGridStyle("Favorites");
                    foreach (PropertyDescriptor pd in pdc)
                    {
                        if (pd.PropertyType == typeof(FavoriteType))
                            gridStyle.EnumComboField(typeof(FavoriteType), pd.Name, pd.DisplayName);
                        else
                            gridStyle.CustomTextField(pd.Name, pd.Name);
                    }
                    gridStyle.SetGridStyle(((IDataGridBinder)child).BoundedGrid.BindingSource, "Favorites");
                }
            }
            return child;
        }
    }
}
