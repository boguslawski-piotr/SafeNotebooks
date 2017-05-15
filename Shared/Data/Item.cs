using System;
namespace SafeNotebooks
{
    public class Item
    {
        public Item Parent = null;

        //public IFileSystem _Fs = null;
        //public IFileSystem Fs
        //{
        //    get {
        //        // TODO: walk to the root if neccessary
        //        return _Fs;
        //    }
        //    set {
        //        _Fs = value;
        //    }
        //}

        public DateTime CreatedOn { get; }

        public string Name { get; set; }

        public string DisplayName
        {
            get {
                return _DisplayName();
            }
        }

        protected virtual string _DisplayName()
        {
            return Name;
        }

        public string DisplayDetail
        {
            get {
                return _DisplayDetail();
            }
        }

        protected virtual string _DisplayDetail()
        {
            return CreatedOn.ToLocalTime().ToString();
        }


        public Item()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{this.GetType().ToString()}: {_DisplayName()} [{_DisplayDetail()}]";
        }

    }
}
