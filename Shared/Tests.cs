using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using pbXForms;
using pbXNet;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;

namespace SafeNotebooks
{
	//[Serializable]
	//class NotEncryptedData
	//{
	//	public string Id;
	//	public DateTime CreatedOn;
	//	public DateTime ModifiedOn;
	//	public string Color = "#00ffffff";
	//	public string Nick;
	//	public CKeyLifeTime CKeyLifeTime;
	//	public byte[] IV;
	//}

	public partial class App : Application
	{
		//void SerializerTest(ISerializer s)
		//{
		//	DateTime _startTime = DateTime.Now;

		//	for (int i = 0; i < 1000; i++)
		//	{
		//		NotEncryptedData d = new NotEncryptedData();

		//		d.ModifiedOn = DateTime.UtcNow;
		//		d.Nick = "1";
		//		string sd = s.ToString(d, "ala1");

		//		d.ModifiedOn = DateTime.UtcNow + TimeSpan.FromHours(2);
		//		d.Nick = "2";
		//		sd += s.ToString(d, "ala2");

		//		d.ModifiedOn = DateTime.UtcNow + TimeSpan.FromHours(2);
		//		d.Nick = "3";
		//		sd += s.ToString(d, "ala3");

		//		sd = Obfuscator.Obfuscate(sd);
		//		sd = Obfuscator.DeObfuscate(sd);

		//		NotEncryptedData dd = s.FromString<NotEncryptedData>(sd, "ala3");

		//		Debug.Assert(d.ModifiedOn == dd.ModifiedOn);
		//	}

		//	Log.D($"{s.GetType().FullName}: {DateTime.Now - _startTime}", this);
		//}

		async Task Tests()
		{
			//***

			//AzureStorageSettings settings = new AzureStorageSettings
			//{
			//	ConnectionString = "UseDevelopmentStorage=true",
			//	Type = AzureStorageSettings.BlobType.Block,
			//};

			//StorageOnAzureStorage<string> storage = new StorageOnAzureStorage<string>("test1", settings, new NewtonsoftJsonSerializer());
			//await storage.InitializeAsync();

			//await storage.StoreAsync("1", "ółżź test", new DateTime(2015, 5, 7, 20, 11, 59));

			//DateTime dt = await storage.GetModifiedOnAsync("1");
			//string data = await storage.GetACopyAsync("1");

			//await storage.DiscardAsync("1");
			//data = await storage.GetACopyAsync("1");

			//await storage.StoreAsync("1", "zupełnie coś innego", new DateTime(2015, 5, 7, 20, 11, 59));
			//data = await storage.GetACopyAsync("1");

			//await storage.StoreAsync("2", "ala ma kota i psa", DateTime.Now);
			//await storage.StoreAsync("3", "ala ma kota i psa", DateTime.Now);

			//IEnumerable<string> l = await storage.FindIdsAsync("[12]");


			//***

			//AesCryptographer cr = new AesCryptographer();

			//IByteBuffer key = cr.GenerateKey(new Password("ala ma kota"), new ByteBuffer("to jest salt", Encoding.UTF8));
			//string skey = key.ToString();

			//// PasswordDeriveBytes: FBBE8CE5A293A76E4E48BE7E1EDBD30893C587A3EF5CBE20245AFDF9850EEB4DC65A00
			//// Rfc2898DeriveBytes:  012000DFFFE56761A0798B58E1A94CD81DACF7DFDC1AFE987596B465BB716685FB7A342877
			//// UWP:                 012000DFFFE56761A0798B58E1A94CD81DACF7DFDC1AFE987596B465BB716685FB7A342877

			//IByteBuffer iv = cr.GenerateIV();
			//ByteBuffer e = cr.Encrypt(new ByteBuffer("wiadomość", Encoding.UTF8), key, iv);
			//ByteBuffer d = cr.Decrypt(e, key, iv);
			//string s = d.ToString(Encoding.UTF8);
			//Log.D($"decrypted: {s}");

			//***

			//ByteBuffer b = new ByteBuffer("ala ma kota", Encoding.UTF8);
			//foreach (var item in b)
			//{
			//	byte c = item;
			//	if (c == 0)
			//		return;
			//}

			//***

			//Password p = new Password("ąałlcć");
			//byte[] bp = p.GetBytes();

			//***

			//byte[] data = new byte[] { 10, 12, 14, 16, 18, 20, 8, 6, 4, 2 };
			//SecureBuffer b = new SecureBuffer(data, true);

			//byte[] dataCopy = b.GetBytes();
			//b.DisposeBytes();

			//string d1 = b.ToString();

			//b.Dispose();


			//***

			//Assembly pbXNetA = typeof(pbXNet.Tools).GetTypeInfo().Assembly;
			//Assembly pbXFormsA = typeof(pbXForms.AppBarLayout).GetTypeInfo().Assembly;
			//Assembly appA = typeof(App).GetTypeInfo().Assembly;

			//***

			//ISerializer s = new BinarySerializer();
			//SerializerTest(s);
			//s = new NewtonsoftJsonSerializer();
			//SerializerTest(s);
			//s = new BinarySerializer();
			//SerializerTest(s);
			//s = new NewtonsoftJsonSerializer();
			//SerializerTest(s);

			//***

			//foreach (var folder in Enum.GetValues(typeof(Environment.SpecialFolder)))
			//{
			//	if(!string.IsNullOrWhiteSpace(System.Environment.GetFolderPath((Environment.SpecialFolder)folder)))
			//	   Debug.WriteLine("{0}={1}", folder, System.Environment.GetFolderPath((Environment.SpecialFolder)folder));
			//}

			//DeviceFileSystem fs = new DeviceFileSystem(DeviceFileSystemRoot.UserDefined, "~/Safe Notebooks");

			//***

			//string s = DeviceEx.Id;


			//***

			//string obs = "Ala ma kota";
			//string obo = Obfuscator.Obfuscate(obs);
			//string obd = Obfuscator.DeObfuscate(obo);

			//***

			//await SecretsManager.DeletePasswordAsync(App.Name);
			//await SecretsManager.AddOrUpdatePasswordAsync(App.Name, "1");
			//await SecretsManager.DeletePasswordAsync(App.Name);
			//char[] pp = { '1' };
			//await SecretsManager.AddOrUpdatePasswordAsync(App.Name, pp);
			//bool b = await SecretsManager.ComparePasswordAsync(App.Name, pp);
			//Debug.Assert(b);
			//byte[] ckey = await SecretsManager.CreateCKeyAsync(App.Name, CKeyLifeTime.WhileAppRunning, pp);
			//byte[] ckey2 = await SecretsManager.GetCKeyAsync(App.Name);
			//Debug.Assert(object.Equals(ckey, ckey2));

			//***

			//DeviceFileSystem fs = new DeviceFileSystem(DeviceFileSystemRoot.Local);
			//try
			//{
			//    string fn = "O-" + pbXNet.Tools.CreateGuid() + "-" + pbXNet.Tools.CreateGuid() + "-" + pbXNet.Tools.CreateGuid() + "-1";
			//    await fs.WriteTextAsync(fn, "1");
			//    string d = await fs.ReadTextAsync(fn);
			//    Debug.WriteLine(d);
			//}
			//catch (Exception e)
			//{
			//    Debug.WriteLine(e.ToString());
			//}

			//DeviceFileSystem fs = new DeviceFileSystem(DeviceFileSystemRoot.Local);
			//await fs.SetCurrentDirectoryAsync("a");
			//IFileSystem fsc = await fs.CloneAsync();
			//await fs.SetCurrentDirectoryAsync("..");

			//IEnumerable<string> d = await fs.GetDirectoriesAsync();
			//IEnumerable<string> f = await fs.GetFilesAsync();

			//bool de = await fs.DirectoryExistsAsync(".config");

			//await fs.WriteTextAsync("ala", "jakiś tekst");

			//bool fe = await fs.FileExistsAsync("ala");

			//try
			//{
			//    string tt = await fs.ReadTextAsync("ala");
			//}
			//catch { }

			//await fs.CreateDirectoryAsync("dir1");

			//DeviceFileSystem fs = new DeviceFileSystem(DeviceFileSystemRoot.Local);
			//await fs.WriteTextAsync("ala", "jakiś tekst");
			//await fs.SetModifiedOnAsync("ala", DateTime.UtcNow - TimeSpan.FromMinutes(5));
			//DateTime dd = await fs.GetModifiedOnAsync("ala");
			//await fs.DeleteFileAsync("ala");
		}
	}
}
