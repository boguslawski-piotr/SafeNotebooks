using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using pbXNet;

namespace SafeNotebooks
{
	public class Item : Observable
	{
		public NotebooksManager NotebooksManager { get; set; }

		ISearchableStorage<string> _Storage;
		public ISearchableStorage<string> Storage
		{
			get => _Storage ?? Parent?.Storage;
			set => _Storage = value;
		}

		public Item Parent { get; private set; }

		public static T New<T>(NotebooksManager notebooksManager, Item parent) where T : Item, new()
		{
			T item = new T()
			{
				NotebooksManager = notebooksManager,
				Parent = parent,
			};

			item.BatchBegin();
			try
			{
				item.InternalNew();
			}
			finally
			{
				item.BatchEnd();
			}

			return item;
		}

		public static async Task<T> OpenAsync<T>(NotebooksManager notebooksManager, Item parent, ISearchableStorage<string> storage, string idInStorage, bool tryToUnlock) where T : Item, new()
		{
			T item = new T()
			{
				NotebooksManager = notebooksManager,
				Storage = storage,
				Parent = parent,
			};

			if (!await item.InternalOpenAsync(idInStorage, tryToUnlock))
				return null;

			return item;
		}


		[System.Serializable]
		class NotEncryptedData
		{
			public string Id;
			public DateTime CreatedOn;
			public DateTime ModifiedOn;
			public string Color = "#00ffffff";
			public string Nick;
			public SecretLifeTime CKeyLifeTime;
			public string IV;
		}

		NotEncryptedData nedata;

		public string Id
		{
			get => nedata?.Id;
			private set => nedata.Id = value;
		}

		public virtual string IdForStorage => Id;

		public DateTime CreatedOn => nedata.CreatedOn;

		public DateTime ModifiedOn => nedata.ModifiedOn;

		public bool Modified { get; protected set; }

		string _modifiedOnForLists;
		public virtual string ModifiedOnForLists
		{
			get => ModifiedOn.ToLocalTime().ToString();
			set => SetValueForLists(ref _modifiedOnForLists, value);
		}

		public string Color
		{
			get => nedata.Color;
			set => SetValue(ref nedata.Color, value);
		}

		public string Nick
		{
			get => nedata?.Nick;
			set {
				SetValue(ref nedata.Nick, value);
				if (!DataIsAvailable)
					NameForLists = value;
			}
		}

		public SecretLifeTime ThisCKeyLifeTime
		{
			get => nedata.CKeyLifeTime;
			// TODO: changing ThisCKeyLifeTime needs additional action like for example: decrypt this and all children, invalidate keys, etc. -> should be rethought more thoroughly
			set => SetValue(ref nedata.CKeyLifeTime, value);
		}

		IByteBuffer IV
		{
			get => nedata.IV == null ? Parent?.IV : SecureBuffer.NewFromHexString(nedata.IV);
			set => SetValue(ref nedata.IV, new SecureBuffer(value, true).ToHexString());
		}

		public bool ThisIsSecured => ThisCKeyLifeTime != SecretLifeTime.Undefined;

		public SecretLifeTime CKeyLifeTime => (ThisIsSecured || Parent == null) ? ThisCKeyLifeTime : Parent.CKeyLifeTime;

		public bool IsSecured => (ThisIsSecured || Parent == null) ? ThisIsSecured : Parent.IsSecured;

		[System.Serializable]
		class Data
		{
			public string Name;
			public string Detail;
		}

		Data data;

		public bool DataIsAvailable => data != null;

		public string Name
		{
			get => data.Name;
			set {
				SetValue(ref data.Name, value);
				NameForLists = value;
			}
		}

		string _nameForLists;
		public virtual string NameForLists
		{
			get => DataIsAvailable ? Name : Nick;
			set => SetValueForLists(ref _nameForLists, value);
		}

		public string Detail
		{
			get => data.Detail;
			set {
				SetValue(ref data.Detail, value);
				DetailForLists = value;
			}
		}

		string _detailForLists;
		public virtual string DetailForLists
		{
			get => DataIsAvailable && !string.IsNullOrEmpty(Detail) ? Detail : " ";
			set => SetValueForLists(ref _detailForLists, value);
		}

		string _lockedImageForListsName;
		public virtual string LockedImageForListsName
		{
			get => !DataIsAvailable ? NotebooksManager.UI.LockedImageForListsName : "";
			set => SetValueForLists(ref _lockedImageForListsName, value);
		}

		double _lockedImageForListsWidth;
		public virtual double LockedImageForListsWidth
		{
			get => !DataIsAvailable ? NotebooksManager.UI.LockedImageForListsWidth : 0;
			set => SetValueForLists(ref _lockedImageForListsWidth, value);
		}

		bool _selectModeEnabled;
		public virtual bool SelectModeEnabled
		{
			get => _selectModeEnabled;
			set {
				_selectModeEnabled = value;
				SelectedUnselectedImageForListsName = _selectModeEnabled ? "e" : "d";
				SelectedUnselectedImageForListsWidth = _selectModeEnabled ? 1 : 0;
			}
		}

		bool _isSelected;
		public bool IsSelected
		{
			get => _isSelected;
			set {
				_isSelected = value;
				SelectedUnselectedImageForListsName = _isSelected ? "s" : "u";
				SelectedUnselectedImageForListsWidth = _isSelected ? 2 : 3;
			}
		}

		string _selectedUnselectedImageForListsName;
		public virtual string SelectedUnselectedImageForListsName
		{
			get {
				if (SelectModeEnabled)
					return IsSelected ? NotebooksManager.UI.SelectedImageForListsName : NotebooksManager.UI.UnselectedImageForListsName;
				else
					return "";
			}
			set => SetValueForLists(ref _selectedUnselectedImageForListsName, value);
		}

		double _selectedUnselectedImageForListsWidth;
		public virtual double SelectedUnselectedImageForListsWidth
		{
			get {
				if (SelectModeEnabled)
					return NotebooksManager.UI.SelectedUnselectedImageForListsWidth;
				else
					return 0;
			}
			set => SetValueForLists(ref _selectedUnselectedImageForListsWidth, value);
		}

		protected void SetValueForLists<T>(ref T storage, T value, [CallerMemberName]string name = null)
		{
			if (Equals(storage, value))
				return;

			storage = value;
			base.OnPropertyChanged(name);
		}

		protected void SetValue<T>(ref T storage, T value, bool touchWithParent = true, [CallerMemberName]string name = null)
		{
			if (Equals(storage, value))
				return;

			storage = value;
			Touch(touchWithParent);

			base.OnPropertyChanged(name);
		}

		public virtual void Touch(bool withParent = true)
		{
			nedata.ModifiedOn = DateTime.UtcNow;
			Modified = true;

			if (withParent)
				TouchParent();
			if (!BatchInProgress)
				NotebooksManager.OnItemModifiedOnChanged(this);

			ModifiedOnForLists = ModifiedOnForLists + nedata.ModifiedOn.ToLocalTime().ToString();
		}

		protected virtual void TouchParent()
		{
			if (Parent != null)
				Parent.Touch();
		}


		#region Serialization

		protected virtual string SerializeNotEncryptedData()
		{
			return NotebooksManager.Serializer.Serialize(nedata, "ned");
		}

		protected virtual void DeserializeNotEncryptedData(string d)
		{
			nedata = NotebooksManager.Serializer.Deserialize<NotEncryptedData>(d, "ned");
		}

		protected virtual string Serialize()
		{
			return NotebooksManager.Serializer.Serialize(data, "d");
		}

		protected virtual void Deserialize(string d)
		{
			data = NotebooksManager.Serializer.Deserialize<Data>(d, "d");
		}

		#endregion

		//

		protected virtual void InternalNew()
		{
			nedata = new NotEncryptedData();
			data = new Data();

			Id = pbXNet.Tools.CreateGuid();

			Touch();
			nedata.CreatedOn = ModifiedOn;
		}

		const string NotEncyptedDataEndMarker = "72d26030-0d4d-4625-b6e8-785de17db815";

		public async Task<bool> OpenAsync(bool tryToUnlock)
		{
			return await TryExecute(InternalOpenAsync(IdForStorage, tryToUnlock));
		}

		protected virtual async Task<bool> InternalOpenAsync(string idInStorage, bool tryToUnlock)
		{
			string d = null;
			try
			{
				d = await Storage?.GetACopyAsync(idInStorage);
			}
			catch (StorageThingNotFoundException) { }
			if (string.IsNullOrEmpty(d))
			{
				// No data for the item in storage probably means that this object was just created (it is new).
				// If the item has a name then we treat this as a correct situation.
				return !string.IsNullOrEmpty(Name);
			}

			d = Obfuscator.DeObfuscate(d);

			int nedEnd = d.IndexOf(NotEncyptedDataEndMarker, StringComparison.Ordinal);
			string ned = d.Substring(0, nedEnd);
			DeserializeNotEncryptedData(ned);

			if (tryToUnlock || !IsSecured || (!ThisIsSecured && Parent != null && Parent.DataIsAvailable))
			{
				d = d.Substring(nedEnd + NotEncyptedDataEndMarker.Length);
				d = await DecryptAsync(d);
				Deserialize(d);

				NameForLists = Name;
				DetailForLists = Detail;
				LockedImageForListsName = "u";
			}
			else
			{
				LockedImageForListsName = "l";
			}

			NotebooksManager.OnItemOpened(this);
			return true;
		}

		public async Task<bool> LoadAsync(bool tryToUnlockChildren)
		{
			return await TryExecute(InternalLoadAsync(tryToUnlockChildren));
		}

		protected virtual async Task<bool> InternalLoadAsync(bool tryToUnlockChildren)
		{
			if (!DataIsAvailable)
			{
				if (!await InternalOpenAsync(IdForStorage, true))
					return false;
			}
			NotebooksManager.OnItemLoaded(this);
			return true;
		}

		public async Task<bool> SaveAsync(bool force = false)
		{
			if ((!Modified && !force) || BatchInProgress)
				return true;

			return await TryExecute(InternalSaveAsync(force));
		}

		protected virtual async Task<bool> InternalSaveAsync(bool force = false)
		{
			Log.D($"for {GetType().FullName}: {Id}", this);

			try
			{
				DateTime modifiedOn = await Storage?.GetModifiedOnAsync(IdForStorage);
				if (modifiedOn > nedata.ModifiedOn)
				{
					throw new Exception($"SafeNotebooks: Item: InternalSaveAsync: {GetType().FullName}: {Id}: modified date in storage is newer than modified date in memory!");
					// TODO: obsluzyc problem gdy dane zapisane sa nowsze niz te, ktore chcemy zapisac
				}
			}
			catch (StorageThingNotFoundException) { }

			string ned = SerializeNotEncryptedData();
			string d = await EncryptAsync(Serialize());

			d = ned + NotEncyptedDataEndMarker + d;
			d = Obfuscator.Obfuscate(d);

			await Storage?.StoreAsync(IdForStorage, d, nedata.ModifiedOn);

			Modified = false;

			NotebooksManager.OnItemSaved(this);
			return true;
		}

		public virtual async Task<bool> SaveAllAsync(bool force = false)
		{
			return await SaveAsync(force);
		}


		//

		public virtual async Task EditAsync()
		{
			// TODO: should ask for a password is locked
			await NotebooksManager.UI.EditItemAsync(this);
		}

		public virtual async Task MoveAsync()
		{
			await App.Current.MainPage.DisplayAlert("Move", $"{GetType().Name}: {NameForLists}", null, T.Localized("Cancel"));
		}

		public virtual async Task DeleteAsync()
		{
			// TODO: should ask for a password is locked
			await App.Current.MainPage.DisplayAlert("Delete", $"{GetType().Name}: {NameForLists}", null, T.Localized("Cancel"));
		}


		#region Security Tools

		Item ItemForCKey => (ThisIsSecured || Parent == null) ? this : Parent?.ItemForCKey;

		string IdForCKey => ItemForCKey?.Id;

		public virtual async Task InitializePasswordAsync(IPassword passwd)
		{
			// If user decided to secure this item with a password...
			if (ThisIsSecured && passwd != null && passwd.Length > 0)
			{
				// ...create ckey (in repository) for future use.
				await CreateCKeyAsync(passwd);
			}
		}

		public virtual async Task CreateCKeyAsync(IPassword passwd)
		{
			if (passwd == null)
			{
				passwd = await NotebooksManager.UI?.GetPasswordAsync(ItemForCKey, IV == null);
				if (passwd == null)
					throw new NotebooksException(NotebooksException.ErrorCode.PasswordNotGiven);
			}

			if (IV == null)
				IV = NotebooksManager.SecretsManager.GenerateIV();

			NotebooksManager.SecretsManager.CreateCKey(IdForCKey, CKeyLifeTime, passwd);
		}

		public virtual async Task PrepareCKeyAsync()
		{
			if (!NotebooksManager.SecretsManager.CKeyExists(IdForCKey))
				await CreateCKeyAsync(null);
		}

		protected virtual async Task<string> EncryptAsync(string d)
		{
			if (IsSecured)
			{
				await PrepareCKeyAsync();
				return NotebooksManager.SecretsManager.Encrypt(d, IdForCKey, IV);
			}

			return d;
		}

		protected virtual async Task<string> DecryptAsync(string d)
		{
			if (IsSecured)
			{
				await PrepareCKeyAsync();
				try
				{
					d = NotebooksManager.SecretsManager.Decrypt(d, IdForCKey, IV);
				}
				catch (Exception)
				{
					// Most likely, a bad password has been entered. 
					// To be safe delete ckey from repository in order to give a chance to ask again.
					NotebooksManager.SecretsManager.DeleteCKey(IdForCKey);
					throw new NotebooksException(NotebooksException.ErrorCode.BadPassword);
				}
			}

			return d;
		}

		#endregion

		#region Tools

		volatile int _batchCounter = 0;
		readonly Object _batchCounterLock = new Object();

		public virtual bool BatchInProgress => _batchCounter > 0;

		public virtual void BatchBegin()
		{
			lock (_batchCounterLock)
			{
				_batchCounter++;
				if (Parent != null)
					Parent.BatchBegin();
			}
		}

		public virtual void BatchEnd()
		{
			lock (_batchCounterLock)
			{
				Debug.Assert(_batchCounter > 0);
				_batchCounter--;

				if (Parent != null)
					Parent.BatchEnd();

				if (Modified && _batchCounter == 0)
					NotebooksManager.OnItemModifiedOnChanged(this);
			}
		}

		protected async Task<bool> TryExecute(Task<bool> t, [CallerMemberName]string callerName = null)
		{
			BatchBegin();
			try
			{
				return await t;
			}
			catch (NotebooksException nex)
			{
				await NotebooksManager.UI?.DisplayError(nex, this, callerName);
				return false;
			}
			catch (Exception ex)
			{
				Log.E(ex.ToString(), this);
				await NotebooksManager.UI?.DisplayError(ex, this, callerName);
				return false;
			}
			finally
			{
				BatchEnd();
			}
		}

		#endregion
	}
}
