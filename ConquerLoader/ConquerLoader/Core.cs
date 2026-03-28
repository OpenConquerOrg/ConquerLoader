using CLCore;
using CLCore.Models;
using ConquerLoader.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ConquerLoader
{
    public static class Core
    {
        public static LogWritter LogWritter = new LogWritter("conquerloader.log");
        public static string ConfigJsonPath = "config.json";
        public static bool UseEncryptedConfig = false;
        public static List<TextTranslation> TextTranslations = new List<TextTranslation>();
        public static LoaderConfig GetLoaderConfig()
        {
            LoaderConfig lConfig = null;
            if (File.Exists(ConfigJsonPath + ".lock"))
            {
                UseEncryptedConfig = true;
                lConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<LoaderConfig>(ConfigFilesEncryption.AESEncription.DecryptString(Constants.LockConfigurationKey, File.ReadAllText(ConfigJsonPath + ".lock")));
            }
            else if (File.Exists(ConfigJsonPath))
            {
                UseEncryptedConfig = false;
                lConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<LoaderConfig>(File.ReadAllText(ConfigJsonPath));
            }
            if (lConfig != null)
            {
                DetectLang(lConfig);
            }
            return lConfig;
        }
        public static void DetectLang(LoaderConfig lConfig)
        {
            if (lConfig.Lang != null)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(lConfig.Lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lConfig.Lang);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            }
            switch (Thread.CurrentThread.CurrentCulture.Name)
            {
                case "es":
                    {
                        TextTranslations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TextTranslation>>(Encoding.UTF8.GetString(Properties.Resources.lang_es));
                        break;
                    }
                case "en":
                    {
                        TextTranslations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TextTranslation>>(Encoding.UTF8.GetString(Properties.Resources.lang_en));
                        break;
                    }
                case "pt":
                    {
                        TextTranslations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TextTranslation>>(Encoding.UTF8.GetString(Properties.Resources.lang_pt));
                        break;
                    }
            }
        }
        public static string TranslateText(string key, string fallback = null)
        {
            TextTranslation translation = TextTranslations.Where(x => x.Id == key).FirstOrDefault();
            if (translation != null && !string.IsNullOrWhiteSpace(translation.Text))
            {
                return translation.Text;
            }

            return fallback ?? key;
        }
        public static int DirectXVersion()
        {
            int directxMajorVersion = 0;
            var OSVersion = Environment.OSVersion;
            // if Windows Vista or later
            if (OSVersion.Version.Major >= 6)
            {
                // if Windows 7 or later
                if (OSVersion.Version.Major > 6 || OSVersion.Version.Minor >= 1)
                {
                    directxMajorVersion = 11;
                }
                // if Windows Vista
                else
                {
                    directxMajorVersion = 10;
                }
            }
            // if Windows XP or earlier.
            else
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DirectX"))
                {
                    string versionStr = key.GetValue("Version") as string;
                    if (!string.IsNullOrEmpty(versionStr))
                    {
                        var versionComponents = versionStr.Split('.');
                        if (versionComponents.Length > 1)
                        {
                            int directXLevel;
                            if (int.TryParse(versionComponents[1], out directXLevel))
                            {
                                directxMajorVersion = directXLevel;
                            }
                        }
                    }
                }
            }

            return directxMajorVersion;
        }

        internal static void LoadAvailablePlugins()
        {
            try
            {
                PluginLoader loader = new PluginLoader();
                loader.LoadPlugins();
                LogWritter.Write("Loaded " + PluginLoader.Plugins.Count + " enabled plugins.");
            }
            catch (Exception e)
            {
                LogWritter.Write(string.Format("Plugins couldn't be loaded: {0}", e.Message));
                Environment.Exit(0);
            }
        }

        public static void LoadRemotePlugins()
        {
            try
            {
                PluginLoader loader = new PluginLoader();
                int available = loader.LoadPluginsFromAPI(GetLoaderConfig()).Result;
                Core.LogWritter.Write("Remote plugin catalog reports " + available + " available plugins.");
            }
            catch (Exception ex)
            {
                Core.LogWritter.Write("Error remote plugins init: " + ex.ToString());
            }
        }

        public static void InitPlugins()
        {
            LoaderConfig currentConfig = GetLoaderConfig();
            if (currentConfig != null && !string.IsNullOrWhiteSpace(currentConfig.LicenseKey))
            {
                Constants.LicenseKey = currentConfig.LicenseKey;
            }

            foreach (IPlugin plugin in PluginLoader.Plugins)
            {
                try
                {
                    plugin.Init();
                    LogWritter.Write("Init plugin: " + plugin.Name + ".");
                }
                catch (Exception ex)
                {
                    {
                        MessageBox.Show($"Error loading plugin {plugin.Name}: " + ex.Message.ToString(), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        public static PluginPreLaunchResult RunPreLaunchPlugins(PluginPreLaunchContext context)
        {
            if (PluginLoader.Plugins == null || PluginLoader.Plugins.Count == 0)
            {
                return PluginPreLaunchResult.Success();
            }

            foreach (IPlugin plugin in PluginLoader.Plugins)
            {
                IPreLaunchPlugin preLaunchPlugin = plugin as IPreLaunchPlugin;
                if (preLaunchPlugin == null)
                {
                    continue;
                }

                try
                {
                    LogWritter.Write("Running pre-launch plugin: " + plugin.Name + ".");
                    PluginPreLaunchResult result = preLaunchPlugin.BeforeLaunch(context) ?? PluginPreLaunchResult.Success();
                    if (!result.ContinueLaunch)
                    {
                        LogWritter.Write("Pre-launch plugin canceled launch: " + plugin.Name + ". Message: " + (result.Message ?? "No message."));
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    LogWritter.Write("Error on pre-launch plugin " + plugin.Name + ": " + ex);
                    return PluginPreLaunchResult.Fail("Plugin \"" + plugin.Name + "\" failed before launch: " + ex.Message);
                }
            }

            return PluginPreLaunchResult.Success();
        }

        public static void SaveLoaderConfig(LoaderConfig LoaderConfig)
        {
            if (UseEncryptedConfig)
            {
                File.WriteAllText(ConfigJsonPath + ".lock", ConfigFilesEncryption.AESEncription.EncryptString(Constants.LockConfigurationKey, Newtonsoft.Json.JsonConvert.SerializeObject(LoaderConfig, Newtonsoft.Json.Formatting.Indented)));
            } else
            {
                File.WriteAllText(ConfigJsonPath, Newtonsoft.Json.JsonConvert.SerializeObject(LoaderConfig, Newtonsoft.Json.Formatting.Indented));
            }
        }
        public static bool ServerAvailable(string Host, uint Port)
        {
            var result = false;
            using (var client = new TcpClient())
            {
                try
                {
                    client.ReceiveTimeout = 1 * 1000;
                    client.SendTimeout = 1 * 1000;
                    var asyncResult = client.BeginConnect(Host, (int)Port, null, null);
                    var waitHandle = asyncResult.AsyncWaitHandle;
                    try
                    {
                        if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(0.6), false))
                        {
                            // wait handle didn't came back in time
                            client.Close();
                        }
                        else
                        {
                            // The result was positiv
                            result = client.Connected;
                            // ensure the ending-call
                            client.EndConnect(asyncResult);
                        }
                    }
                    catch(Exception)
                    {
                    }
                    finally
                    {
                        // Ensure to close the wait handle.
                        waitHandle.Close();
                    }
                }
                catch(Exception)
                {
                }
            }
            return result;
        }

        public static void LoadControlTranslations(Control.ControlCollection Controls)
        {
            foreach (Control c in Controls)
            {
                if (c is MetroFramework.Controls.MetroLabel || c is MetroFramework.Controls.MetroButton)
                {
                    TextTranslation str = TextTranslations.Where(x => x.Id == c.Name).FirstOrDefault();
                    if (str != null && str.Text.Length > 0)
                    {
                        c.Text = str.Text;
                    }
                    else
                    {
                        if (c.Text != "-")
                        {
                            c.Text = c.Name;
                        }
                    }
                }

                if (c.HasChildren)
                {
                    LoadControlTranslations(c.Controls);
                }
            }
        }
        public static bool IsVCRuntimeInstalled(string arch)
        {
            string subKey = $@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\{arch}";

            bool Check(RegistryView view)
            {
                var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                var key = baseKey.OpenSubKey(subKey);
                if (key == null) return false;

                var installed = key.GetValue("Installed");
                return installed is int i && i == 1;
            }

            // Probamos ambas vistas por si la app es 32-bit o 64-bit
            return Check(RegistryView.Registry64) || Check(RegistryView.Registry32);
        }
    }
    public static class SafeIO
    {
        public static bool TryWriteAllBytes(
            string path,
            byte[] data,
            Action<Exception> onError = null)
        {
            try
            {
                File.WriteAllBytes(path, data);
                return true;
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return false;
            }
        }
    }
}
