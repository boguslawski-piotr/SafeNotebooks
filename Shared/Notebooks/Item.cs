using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;

#if !NOT_XAMARIN_FORMS
using Xamarin.Forms;
#endif

namespace SafeNotebooks
{
	public class Item : BindableObject
	{
		public NotebooksManager NotebooksManager { get; set; }

		ISearchableStorage<string> _Storage;
		public ISearchableStorage<string> Storage
		{
			get => _Storage ?? Parent?.Storage;
			set => _Storage = value;
		}

		protected Item Parent { get; private set; }

		[Serializable]
		class NotEncryptedData
		{
			public string Id;
			public DateTime CreatedOn;
			public DateTime ModifiedOn;
			public string Color = "#00ffffff";
			public string Nick;
			public CKeyLifeTime CKeyLifeTime;
			public byte[] IV;
		}

		NotEncryptedData nedata;

		public string Id
		{
			get => nedata?.Id;
			private set => nedata.Id = value;
		}

		public DateTime CreatedOn => nedata.CreatedOn;

		public DateTime ModifiedOn => nedata.ModifiedOn;
		public bool Modified { get; protected set; }

		public Color Color
		{
			get => Color.FromHex(nedata.Color);
			set
			{
				SetValue(ref nedata.Color, value.ToHex());
				ColorForLists = value;
			}
		}

		public string ComparableColor => nedata.Color;

		public static readonly BindableProperty ColorForListsProperty = BindableProperty.Create("ColorForLists", typeof(Color), typeof(Item), Color.Transparent);

		public Color ColorForLists
		{
			get => Color;
			set => SetValue(ColorForListsProperty, value);
		}

		public string Nick
		{
			get => nedata?.Nick;
			set
			{
				SetValue(ref nedata.Nick, value);
				if (!DataIsAvailable)
					NameForLists = value;
			}
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

		[Serializable]
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
			set
			{
				SetValue(ref data.Name, value);
				NameForLists = value;
			}
		}

		public static readonly BindableProperty NameForListsProperty = BindableProperty.Create("NameForLists", typeof(string), typeof(Item));

		public virtual string NameForLists
		{
			get => DataIsAvailable ? Name : Nick;
			set => SetValue(NameForListsProperty, value);
		}

		public string Detail
		{
			get => data.Detail;
			set
			{
				SetValue(ref data.Detail, value);
				DetailForLists = value;
			}
		}

		public static readonly BindableProperty DetailForListsProperty = BindableProperty.Create("DetailForLists", typeof(string), typeof(Item));

		public virtual string DetailForLists
		{
			get => ModifiedOn.ToLocalTime().ToString() + (DataIsAvailable && !string.IsNullOrEmpty(Detail) ? ", " + Detail : "");
			set => SetValue(DetailForListsProperty, value);
		}

		public static readonly BindableProperty LockedImageNameForListsProperty = BindableProperty.Create("LockedImageNameForLists", typeof(string), typeof(Item));

		public virtual string LockedImageNameForLists
		{
			get => !DataIsAvailable ? NotebooksManager.UI.LockedImageNameForLists : "";
			set => SetValue(LockedImageNameForListsProperty, value);
		}

		public static readonly BindableProperty LockedImageWidthForListsProperty = BindableProperty.Create("LockedImageWidthForLists", typeof(double), typeof(Item), 0d);

		public virtual double LockedImageWidthForLists
		{
			get => !DataIsAvailable ? NotebooksManager.UI.LockedImageWidthForLists : 0;
			set => SetValue(LockedImageWidthForListsProperty, value);
		}

		bool _SelectModeEnabled;
		public virtual bool SelectModeEnabled
		{
			get => _SelectModeEnabled;
			set
			{
				_SelectModeEnabled = value;
				SelectedUnselectedImageNameForLists = _SelectModeEnabled ? "e" : "d";
				SelectedUnselectedImageWidthForLists = _SelectModeEnabled ? 1 : 0;
			}
		}

		bool _IsSelected;
		public bool IsSelected
		{
			get => _IsSelected;
			set
			{
				_IsSelected = value;
				SelectedUnselectedImageNameForLists = _IsSelected ? "s" : "u";
				SelectedUnselectedImageWidthForLists = _IsSelected ? 2 : 3;
			}
		}

		public static readonly BindableProperty SelectedUnselectedImageNameForListsProperty = BindableProperty.Create("SelectedUnselectedImageNameForLists", typeof(string), typeof(Item));

		public virtual string SelectedUnselectedImageNameForLists
		{
			get
			{
				if (SelectModeEnabled)
					return IsSelected ? NotebooksManager.UI.SelectedImageNameForLists : NotebooksManager.UI.UnselectedImageNameForLists;
				else
					return "";
			}
			set => SetValue(SelectedUnselectedImageNameForListsProperty, value);
		}

		public static readonly BindableProperty SelectedUnselectedImageWidthForListsProperty = BindableProperty.Create("SelectedUnselectedImageWidthForLists", typeof(double), typeof(Item), -1d);

		public virtual double SelectedUnselectedImageWidthForLists
		{
			get
			{
				if (SelectModeEnabled)
					return NotebooksManager.UI.SelectedUnselectedImageWidthForLists;
				else
					return 0;
			}
			set => SetValue(SelectedUnselectedImageWidthForListsProperty, value);
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

			// Xamarin.Forms binding system support
			DetailForLists = DetailForLists + nedata.ModifiedOn.ToLocalTime().ToString();
		}

		protected virtual void TouchParent()
		{
			if (Parent != null)
				Parent.Touch();
		}


		//

		public Item()
		{
			//InitalizeCommands();
		}


		//

		protected virtual string SerializeNotEncryptedData()
		{
			return NotebooksManager.Serializer.ToString(nedata, "ned");
		}

		protected virtual void DeserializeNotEncryptedData(string d)
		{
			nedata = NotebooksManager.Serializer.FromString<NotEncryptedData>(d, "ned");
		}

		protected virtual string Serialize()
		{
			return NotebooksManager.Serializer.ToString(data, "d");
		}

		protected virtual void Deserialize(string d)
		{
			data = NotebooksManager.Serializer.FromString<Data>(d, "d");
		}


		//

		public virtual string IdForStorage => Id;

		public Item New(Item parent)
		{
			Parent = parent;

			BatchBegin();
			try
			{
				InternalNew();
			}
			finally
			{
				BatchEnd();
			}

			return this;
		}

		protected virtual void InternalNew()
		{
			nedata = new NotEncryptedData();
			data = new Data();

			Id = pbXNet.Tools.CreateGuid();

			Touch();
			nedata.CreatedOn = ModifiedOn;
		}

		const string NotEncyptedDataEndMarker = "72d26030-0d4d-4625-b6e8-785de17db815";

		public async Task<bool> OpenAsync(Item parent, string idInStorage, bool tryToUnlock)
		{
			Parent = parent;
			return await TryExecute(InternalOpenAsync(idInStorage, tryToUnlock));
		}

		protected virtual async Task<bool> InternalOpenAsync(string idInStorage, bool tryToUnlock)
		{
			string d = await Storage?.GetACopyAsync(idInStorage);
			d = Obfuscator.DeObfuscate(d);

			int nedEnd = d.IndexOf(NotEncyptedDataEndMarker, StringComparison.Ordinal);
			string ned = d.Substring(0, nedEnd);
			DeserializeNotEncryptedData(ned);

			if (tryToUnlock || !IsSecured || (!ThisIsSecured && Parent.DataIsAvailable))
			{
				d = d.Substring(nedEnd + NotEncyptedDataEndMarker.Length);
				d = await DecryptAsync(d);
				Deserialize(d);

				// Xamarin.Forms binding system support
				NameForLists = Name;
				DetailForLists = Detail;
				LockedImageNameForLists = "u";
			}
			else
			{
				// Xamarin.Forms binding system support
				LockedImageNameForLists = "l";
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
			Debug.WriteLine($"SafeNotebooks: Item: InternalSaveAsync: for {GetType().FullName}: {Id}");

			DateTime modifiedOn = await Storage?.GetModifiedOnAsync(IdForStorage);
			if (modifiedOn > nedata.ModifiedOn)
			{
				throw new Exception($"SafeNotebooks: Item: InternalSaveAsync: {GetType().FullName}: {Id}: modified date in storage is newer than modified date in memory!");
				// TODO: obsluzyc problem gdy dane zapisane sa nowsze niz te, ktore chcemy zapisac
			}

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

		// Xamarin.Forms binding system support

		//public ICommand EditItemCommand { get; private set; }
		//public ICommand MoveItemCommand { get; private set; }
		//public ICommand DeleteItemCommand { get; private set; }
		//public ICommand SelectUnselectItemCommand { get; private set; }

		//void InitalizeCommands()
		//{
		//    EditItemCommand = new Command(ExecuteEditItemCommand, CanExecuteEditMoveDelete);
		//    MoveItemCommand = new Command(ExecuteMoveItemCommand, CanExecuteEditMoveDelete);
		//    DeleteItemCommand = new Command(ExecuteDeleteItemCommand, CanExecuteEditMoveDelete);
		//    SelectUnselectItemCommand = new Command(ExecuteSelectUnselectItemCommand, CanExecuteSelectUnselectItemCommand);
		//}

		//protected virtual bool CanExecuteEditMoveDelete(object sender) => true;
		//protected virtual void ExecuteEditItemCommand(object sender) => ((Item)sender)?.EditAsync();
		//protected virtual void ExecuteMoveItemCommand(object sender) => ((Item)sender)?.MoveAsync();
		//protected virtual void ExecuteDeleteItemCommand(object sender) => ((Item)sender)?.DeleteAsync();

		//protected virtual bool CanExecuteSelectUnselectItemCommand(object sender) => true;
		//protected virtual void ExecuteSelectUnselectItemCommand(object sender)
		//{
		//    Item item = (Item)sender;
		//    if (item != null)
		//        item.IsSelected = !item.IsSelected;
		//}


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
					Debug.WriteLine($"SafeNotebooks: Item: DecryptAsync: error: {ex.Message}");
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


		//

		protected async Task<bool> TryExecute(Task<bool> t)
		{
			BatchBegin();
			try
			{
				return await t;
			}
			catch (NotebooksException dmex)
			{
				await NotebooksManager.UI?.DisplayError(dmex);
				return false;
			}
			catch (Exception ex)
			{
				await NotebooksManager.UI?.DisplayError(ex);
				return false;
			}
			finally
			{
				BatchEnd();
			}
		}
	}
}
