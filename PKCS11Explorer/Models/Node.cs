using Avalonia.Media.Imaging;
using PKCS11Explorer.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PKCS11Explorer.Models
{
    public class Node
    {
        private ObservableCollection<Node> _children;
        public string IconURI { get; set; }
        public string Header { get; set; }
        public bool IsVisible
        {
            get
            {
                return !string.IsNullOrWhiteSpace(IconURI);
            }
        }
        public IBitmap Icon
        {
            get
            {
                if (IsVisible)
                    return (Bitmap)BitmapValueConverter.Instance.Convert((object)IconURI, typeof(IBitmap), null, null);
                else
                    return null;
            }
        }

        public ObservableCollection<Node> Children
        {
            get
            {
                if (_children == null)
                    _children = new ObservableCollection<Node>();
                return _children;
            }
            set
            {
                _children = value;
            }
        }
    }
}
