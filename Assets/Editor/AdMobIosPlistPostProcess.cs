using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using LuminaMatch.Monetization;

namespace LuminaMatch.Editor
{
    /// <summary>
    /// Google's PListProcessor is compiled only when UNITY_IOS is defined in the
    /// Editor, so batchmode Mac builds can ship without GADApplicationIdentifier
    /// and crash on launch. This always runs for iOS player builds.
    /// </summary>
    public class AdMobIosPlistPostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder => 9999;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
                return;

            var plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
            if (!File.Exists(plistPath))
            {
                Debug.LogWarning($"[LuminaMatch] iOS Info.plist missing at {plistPath}");
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            var appId = (AdsConfig.Current.iosAppId ?? "").Trim();
            if (appId.Length == 0)
            {
                Debug.LogWarning("[LuminaMatch] iOS AdMob App ID empty — ads will fail on device.");
            }
            else
            {
                plist.root.SetString("GADApplicationIdentifier", appId);
            }

            // Standard HTTPS only — answer "No" to export compliance in ASC.
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            // AdMob may use the advertising ID; keep the ATT string for App Store privacy.
            // Runtime ATT prompt is optional until UMP/ATT flow is wired; string alone is safe.
            plist.root.SetString("NSUserTrackingUsageDescription",
                "Usamos esse identificador para medir anúncios e melhorar a experiência.");
            plist.root.SetString("GADUUnityVersion", Application.unityVersion);

            foreach (var id in ReadSkAdNetworkIds())
            {
                AddSkAdNetworkId(plist, id);
            }

            File.WriteAllText(plistPath, plist.WriteToString());
            Debug.Log($"[LuminaMatch] Wrote AdMob keys to iOS Info.plist (GADApplicationIdentifier={appId}).");
        }

        static void AddSkAdNetworkId(PlistDocument plist, string id)
        {
            PlistElementArray array;
            if (plist.root.values.TryGetValue("SKAdNetworkItems", out var existing))
                array = existing.AsArray();
            else
                array = plist.root.CreateArray("SKAdNetworkItems");

            if (array == null)
                return;

            foreach (var elem in array.values)
            {
                try
                {
                    if (elem.AsDict().values.TryGetValue("SKAdNetworkIdentifier", out var value)
                        && value.AsString() == id)
                        return;
                }
                catch
                {
                    // ignore malformed entries
                }
            }

            var dict = array.AddDict();
            dict.SetString("SKAdNetworkIdentifier", id);
        }

        static List<string> ReadSkAdNetworkIds()
        {
            var ids = new List<string>();
            foreach (var path in CandidateSkAdNetworkXmlPaths())
            {
                if (!File.Exists(path))
                    continue;
                try
                {
                    var doc = new XmlDocument();
                    doc.Load(path);
                    var nodes = doc.SelectNodes("//SKAdNetworkIdentifier");
                    if (nodes == null)
                        continue;
                    foreach (XmlNode node in nodes)
                    {
                        var text = (node.InnerText ?? "").Trim();
                        if (text.Length > 0)
                            ids.Add(text);
                    }
                    if (ids.Count > 0)
                        return ids;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[LuminaMatch] SKAdNetwork XML {path}: {ex.Message}");
                }
            }
            return ids;
        }

        static IEnumerable<string> CandidateSkAdNetworkXmlPaths()
        {
            yield return Path.Combine(Application.dataPath, "GoogleMobileAds/Editor/GoogleMobileAdsSKAdNetworkItems.xml");
            var packageCache = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "Library", "PackageCache");
            if (!Directory.Exists(packageCache))
                yield break;
            foreach (var dir in Directory.GetDirectories(packageCache, "com.google.ads.mobile*"))
            {
                yield return Path.Combine(dir, "GoogleMobileAds", "Editor", "GoogleMobileAdsSKAdNetworkItems.xml");
            }
        }
    }
}
