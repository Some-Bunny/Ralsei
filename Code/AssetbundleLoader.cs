using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace Ralsei.Code
{
    //MikuMikuModule.FilePathFolder 
    internal class AssetBundleLoader
    {
        public static AssetBundle LoadAssetBundleFromLiterallyAnywhere(string FilePath, string name, bool logs = false)
        {
            AssetBundle result = null;
            {
                string platformAssetName = name + GetPlatformBundleExtension();
                if (File.Exists(FilePath + "/" + platformAssetName))
                {
                    try
                    {
                        result = AssetBundle.LoadFromFile(Path.Combine(FilePath, platformAssetName));
                        if (logs == true)
                        {
                            global::ETGModConsole.Log("Successfully loaded assetbundle!", false);
                        }
                    }
                    catch (Exception ex)
                    {
                        global::ETGModConsole.Log("Failed loading asset bundle from file.", false);
                        global::ETGModConsole.Log(ex.ToString(), false);
                    }
                }
                else
                {
                    global::ETGModConsole.Log("AssetBundle NOT FOUND!", false);
                }
            }
            return result;
        }

        private static string GetPlatformBundleExtension()
        {
            if (Application.platform == RuntimePlatform.LinuxPlayer)
                return "-linux";
            if (Application.platform == RuntimePlatform.OSXPlayer)
                return "-macos";
            return "-windows";
        }
    }
}
