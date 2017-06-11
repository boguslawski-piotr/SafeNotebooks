using System;
using System.Diagnostics;
using System.Reflection;
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
			//string d1 = "";

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
			//    Debug.WriteLine("{0}={1}", folder, System.Environment.GetFolderPath((Environment.SpecialFolder)folder));
			//}

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

			//DeviceFileSystem fs = new DeviceFileSystem(DeviceFileSystemRoot.Personal);
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

			//DeviceFileSystem fs = new DeviceFileSystem(DeviceFileSystemRoot.Personal);
			//await fs.SetCurrentDirectoryAsync("a");
			//IFileSystem fsc = await fs.MakeCopyAsync();
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

			//***

			//var tests = new AesCryptographerTests();
			//tests.BasicEncryptDecrypt();
		}
	}
}
