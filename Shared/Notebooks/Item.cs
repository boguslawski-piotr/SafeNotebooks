using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pbXNet;
using pbXSecurity;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public class Item : BindableObject
    {
        public Item()
        {
            EditItemCommand = new Command(ExecuteEditItemCommand, CanExecuteEditMoveDelete);
            MoveItemCommand = new Command(ExecuteMoveItemCommand, CanExecuteEditMoveDelete);
            DeleteItemCommand = new Command(ExecuteDeleteItemCommand, CanExecuteEditMoveDelete);
        }

        public NotebooksManager NotebooksManager { get; set; }

        ISearchableStorage<string> _Storage;
        public ISearchableStorage<string> Storage
        {
            get => _Storage ?? Parent?.Storage;
            set => _Storage = value;
        }


        // 

        protected Item Parent { get; private set; }

        public virtual async Task ChangeParentAsync(Item newParent)
        {
            if (Parent == newParent)
                return;

            // TODO: ChangeParent: load all data, delete old file, save (-> Page should override this and do it for all Notes)
            Parent = newParent;
        }


        //

        public class NotEncryptedData
        {
            public string Id;
            public DateTime CreatedOn;
            public DateTime ModifiedOn;
            public string Nick;
            public CKeyLifeTime CKeyLifeTime;
            public byte[] IV;
        }

        NotEncryptedData nedata;

        public string Id
        {
            get => nedata.Id;
            private set => nedata.Id = value;
        }

        public DateTime CreatedOn
        {
            get => nedata.CreatedOn;
            private set => nedata.CreatedOn = value;
        }

        public DateTime ModifiedOn
        {
            get => nedata.ModifiedOn;
            private set => nedata.ModifiedOn = value;
        }

        public string Nick
        {
            get => nedata.Nick;
            set => SetValue(ref nedata.Nick, value);
        }

        public CKeyLifeTime ThisCKeyLifeTime
        {
            get => nedata.CKeyLifeTime;
            // TODO: changing ThisCKeyLifeTime needs additional action like for example: decrypt this and all children, invalidate keys, etc. -> should be rethought more thoroughly
            set => SetValue(ref nedata.CKeyLifeTime, value);
        }

        byte[] IV
        {
            get => nedata.IV ?? Parent?.IV;
            set => SetValue(ref nedata.IV, value);
        }

        public bool ThisIsSecured => ThisCKeyLifeTime != CKeyLifeTime.Undefined;

        public CKeyLifeTime CKeyLifeTime => (ThisIsSecured || Parent == null) ? ThisCKeyLifeTime : Parent.CKeyLifeTime;

        public bool IsSecured => (ThisIsSecured || Parent == null) ? ThisIsSecured : Parent.IsSecured;


        //

        public class Data
        {
            public string Name;
            public string Detail;
        }

        Data data;

        public bool DataIsAvailable => data != null;

        public string Name
        {
            get => data.Name;
            set => SetValue(ref data.Name, value);
        }

        public static readonly BindableProperty NameForListsProperty = BindableProperty.Create("NameForLists", typeof(string), typeof(string), null,
            propertyChanged: (bo, o, n) => { });

        public virtual string NameForLists
        {
            get => DataIsAvailable ? Name : Nick;
            set => SetValue(NameForListsProperty, value);
        }

        public string Detail
        {
            get => data.Detail;
            set => SetValue(ref data.Detail, value);
        }

		public static readonly BindableProperty DetailForListsProperty = BindableProperty.Create("DetailForLists", typeof(string), typeof(string), null,
			propertyChanged: (bo, o, n) => { });
		
        public virtual string DetailForLists
        {
            get => DataIsAvailable ? Detail : "";
            set => SetValue(DetailForListsProperty, value);
        }

        public static readonly BindableProperty LockedImageNameProperty = BindableProperty.Create("LockedImageName", typeof(string), typeof(string), null,
            propertyChanged: (bo, o, n) => { ((Item)bo).NameForLists = (string)n; ((Item)bo).DetailForLists = (string)n; });

        public virtual string LockedImageName
        {
            get => !DataIsAvailable ? NotebooksManager.UI.LockedImageName : "";
            set => SetValue(LockedImageNameProperty, value);
        }

		
        //

		protected override void OnPropertyChanged(string propertyName)
		{
            string[] propertiesForBindableHack = { "NameForLists", "DetailForLists", "LockedImageName", };

            if(!propertiesForBindableHack.Any((n) => n == propertyName))
                Touch();

            base.OnPropertyChanged(propertyName);
		}
		

        //

		public bool Modified
        {
            get;
            protected set;
        }

        public virtual void Touch()
        {
            Modified = true;
            ModifiedOn = DateTime.UtcNow;
        }

        public virtual Task TouchAsync()
        {
            Touch();
            return Task.FromResult(true);
        }


        //

        public virtual Item ObjectFotCKey => (ThisIsSecured || Parent == null) ? this : Parent?.ObjectFotCKey;

        public virtual string IdForCKey => ObjectFotCKey?.Id;

        public virtual async Task InitializePasswordAsync(string passwd)
        {
            // If user decided to secure this item with a password
            if (ThisIsSecured && !string.IsNullOrEmpty(passwd))
            {
                // Create ckey (in repository) for future use
                // Ignore the result because it is not needed here
                await CreateCKeyAsync(passwd);
            }
        }

        public virtual async Task<byte[]> CreateCKeyAsync(string passwd)
        {
            if (string.IsNullOrEmpty(passwd))
            {
                passwd = await NotebooksManager.UI?.GetPasswordAsync(ObjectFotCKey, IV == null);
                if (passwd == null)
                    throw new NotebooksException(NotebooksException.ErrorCode.PasswordNotGiven);
            }

            if (IV == null)
                IV = NotebooksManager.SecretsManager.GenerateIV();

            return await NotebooksManager.SecretsManager.CreateCKeyAsync(IdForCKey, CKeyLifeTime, passwd);
        }

        public virtual async Task<byte[]> GetCKeyAsync()
        {
            byte[] ckey = await NotebooksManager.SecretsManager.GetCKeyAsync(IdForCKey);
            if (ckey == null)
                ckey = await CreateCKeyAsync(null);

            return ckey;
        }

        protected virtual async Task<string> EncryptAsync(string d)
        {
            if (IsSecured)
            {
                byte[] ckey = await GetCKeyAsync();
                return await NotebooksManager.SecretsManager.EncryptAsync(d, ckey, IV);
            }
            return d;
        }

        protected virtual async Task<string> DecryptAsync(string d)
        {
            if (IsSecured)
            {
                byte[] ckey = await GetCKeyAsync();
                try
                {
                    d = await NotebooksManager.SecretsManager.DecryptAsync(d, ckey, IV);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SafeNotebooks:Item: DecryptAsync: error: {ex.Message}");
                    return null;
                }

                if (string.IsNullOrEmpty(d))
                {
                    // Most likely, a bad password has been entered 
                    // To be safe delete ckey from repository in order to give a chance to ask again
                    await NotebooksManager.SecretsManager.DeleteCKeyAsync(IdForCKey);
                    throw new NotebooksException(NotebooksException.ErrorCode.BadPassword);
                }
            }

            return d;
        }


        //

        protected virtual string SerializeNotEncryptedData()
        {
            return JsonConvert.SerializeObject(nedata, pbXNet.Settings.JsonSerializer);
        }

        protected virtual void DeserializeNotEncryptedData(string ned)
        {
            nedata = JsonConvert.DeserializeObject<NotEncryptedData>(ned, pbXNet.Settings.JsonSerializer);
        }

        protected virtual string Serialize()
        {
            return "'d':" + JsonConvert.SerializeObject(data, pbXNet.Settings.JsonSerializer);
        }

        protected virtual void Deserialize(JObject d)
        {
            data = JsonConvert.DeserializeObject<Data>(d["d"].ToString(), pbXNet.Settings.JsonSerializer);
        }


        //

        public virtual string IdForStorage => Id;

        public virtual async Task NewAsync(Item parent)
        {
            nedata = new NotEncryptedData();
            data = new Data();

            Parent = parent;
            Id = pbXNet.Tools.CreateGuid();

            CreatedOn = DateTime.UtcNow;
            ModifiedOn = CreatedOn;
        }

        public virtual async Task<bool> OpenAsync(Item parent, string id, bool tryToUnlock)
        {
            Parent = parent;

            // OpenAsync -> loads minimum data or none
            try
            {
                string d = await Storage?.GetACopyAsync(id);

                //d = Obfuscator.DeObfuscate(d);

                int _nedEnd = d.IndexOf('}');
                string _ned = d.Substring(0, _nedEnd + 1);

                DeserializeNotEncryptedData(_ned);

                if (tryToUnlock || !IsSecured || (!ThisIsSecured && Parent.DataIsAvailable))
                {
                    string _d = d.Substring(_nedEnd + 1);

                    _d = await DecryptAsync(_d);

                    JObject g = JObject.Parse("{" + _d + "}");
                    Deserialize(g);

                    LockedImageName = "u";
                }
                else
                {
                    LockedImageName = "l";
                }

                NotebooksManager.OnItemOpened(this);
                return true;
            }
            catch (NotebooksException dmex)
            {
                await NotebooksManager.UI?.DisplayError(dmex.Err);
                return false;
            }
            catch (Exception ex)
            {
                await NotebooksManager.UI?.DisplayError(ex.Message);
                return false;
            }
        }

        public virtual async Task<bool> LoadAsync(bool tryToUnlockChildren)
        {
            if (!DataIsAvailable)
            {
                if (!await OpenAsync(Parent, IdForStorage, true))
                    return false;
            }
            return true;
        }

        public virtual async Task<bool> SaveAsync(bool force = false)
        {
            if ((!Modified && !force) || _batchMode)
                return true;

            BatchBegin();
            try
            {
                string ned = SerializeNotEncryptedData();
                string d = Serialize();

                string ed = await EncryptAsync(d);

                d = ned + ed;

                //d = Obfuscator.Obfuscate(d);

                await Storage?.StoreAsync(IdForStorage, d);

                if (Modified && Parent != null)
                    await Parent.TouchAsync();  // to update ModifiedOn, TODO: is this really neccessary?

                Modified = false;

                NotebooksManager.OnItemSaved(this);
                return true;
            }
            catch (NotebooksException dmex)
            {
                await NotebooksManager.UI?.DisplayError(dmex.Err);
                return false;
            }
            catch (Exception ex)
            {
                await NotebooksManager.UI?.DisplayError(ex.Message);
                return false;
            }
            finally
            {
                BatchEnd();
            }
        }


        //

        public ICommand EditItemCommand { private set; get; }
        public ICommand MoveItemCommand { private set; get; }
        public ICommand DeleteItemCommand { private set; get; }

        public virtual async Task EditAsync()
        {
            await NotebooksManager.UI.EditItemAsync(this);
        }

        void ExecuteEditItemCommand(object sender)
        {
            ((Item)sender)?.EditAsync();
        }

        public virtual async Task MoveAsync()
        {
            await App.Current.MainPage.DisplayAlert("Move", $"{GetType().Name}: {NameForLists}", null, T.Localized("Cancel"));
        }

        void ExecuteMoveItemCommand(object sender)
        {
            ((Item)sender)?.MoveAsync();
        }

        public virtual async Task DeleteAsync()
        {
            await App.Current.MainPage.DisplayAlert("Delete", $"{GetType().Name}: {NameForLists}", null, T.Localized("Cancel"));
        }

        protected virtual void ExecuteDeleteItemCommand(object sender)
        {
            ((Item)sender)?.DeleteAsync();
        }

        protected virtual bool CanExecuteEditMoveDelete(object sender)
        {
            Item item = (Item)sender;
            return item != null ? item.DataIsAvailable : false;
        }


        //

        bool _batchMode;
        Object _batchModeLock = new Object();

        public virtual void BatchBegin()
        {
            lock (_batchModeLock)
            {
                _batchMode = true;
            }
        }

        protected virtual void BatchEnd()
        {
            lock (_batchModeLock)
            {
                _batchMode = false;
            }
        }

        public virtual async Task BatchCommitAsync(bool forceSave = false)
        {
            if (!_batchMode)
                return;

            BatchEnd();
            await SaveAsync(forceSave);
        }


		//

        protected void SetValue<T>(ref T storage, T value, [CallerMemberName]string name = null)
		{
			if (Equals(storage, value))
			{
				return;
			}

			storage = value;
			OnPropertyChanged(name);
		}
	}
}
