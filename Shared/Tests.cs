﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using pbXNet;
using pbXSecurity;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class App : Application
    {
        //public class SampleData
        //{
        //    public string Id;
        //    public DateTime CreatedOn;
        //    public DateTime ModifiedOn;
        //    public Color Color = Color.Transparent;
        //    public string Nick;
        //    public CKeyLifeTime CKeyLifeTime;
        //    public byte[] IV;
        //}

        async void Tests()
        {
            //***

            //SampleData dd = new SampleData();
            //dd.Color = Color.Red;
            //string d = JsonConvert.SerializeObject(dd);
            //SampleData ddd = JsonConvert.DeserializeObject<SampleData>(d);

            //Color c = Color.Red;
            //string d = JsonConvert.SerializeObject(c);
            //Color cd = JsonConvert.DeserializeObject<Color>(d);


            //***

            //string obs = "Ala ma kota";
            //string obo = Obfuscator.Obfuscate(obs);
            //string obd = Obfuscator.DeObfuscate(obo);

            //***

            //await SecretsManager.DeletePasswordAsync(App.Name);
            //await SecretsManager.AddOrUpdatePasswordAsync(App.Name, "1");
            //await SecretsManager.DeletePasswordAsync(App.Name);

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