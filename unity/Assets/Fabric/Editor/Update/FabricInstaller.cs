namespace Fabric.Internal.Editor.Update
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.IO;
	using System;

	internal class FabricInstaller
	{
		internal class Config
		{
			public readonly string PackageUrl;
			public readonly string Filename;
			public readonly string ReleaseNotesUrl;

			public Config(
				string packageUrl,
				string filename,
				string releaseNotesUrl
			)
			{
				PackageUrl = packageUrl;
				Filename = filename;
				ReleaseNotesUrl = releaseNotesUrl;
			}
		}

		internal enum VerificationStatus {
			Success,
			Failure,
			Error
		}

		public delegate void ReportInstallProgress (float progress);
		public delegate void DownloadComplete (string downloadPath);
		public delegate void DownloadError (System.Exception exception);
		public delegate void VerificationError ();
		public delegate bool IsCancelled ();

		private Config config;
		private TimeoutWebClient webClient = new TimeoutWebClient (1000 * 30);
		private string releaseNotes = null;
		private static readonly string SignatureCertificatePath = Application.dataPath + FileUtils.NormalizePathForPlatform (
			"/Fabric/Managed/Certificates/FabricPublic.XML"
		);

		public FabricInstaller(Config config)
		{
			SwapConfig (config);
		}

		public void SwapConfig(Config config)
		{
			this.config = config;
			this.releaseNotes = null;
		}

		public string FetchReleaseNotes()
		{
			if (releaseNotes != null) {
				return releaseNotes;
			}

			try {
				if (config.ReleaseNotesUrl != null) {
					releaseNotes = Net.Validator.MakeRequest (() => {
						return webClient.DownloadString (config.ReleaseNotesUrl);
					});
					return releaseNotes;
				}
			} catch (System.Exception e) {
				if (Net.Utils.IsNetworkUnavailableFrom (e)) {
					Utils.Log ("No valid network connection available.");
				} else {
					Utils.Warn ("Couldn't fetch release notes from {0}; {1}", config.ReleaseNotesUrl, e.ToString ());
				}
			}

			releaseNotes = "No release notes available!";
			return releaseNotes;
		}

		public void DownloadAndInstallPackage(
			ReportInstallProgress reportProgress,
			DownloadComplete downloadComplete,
			DownloadError downloadError,
			VerificationError verificationError,
			IsCancelled isCancelled
		)
		{
			string downloadPath = PrepareDownloadFilePath (FileUtil.GetUniqueTempPathInProject (), config.Filename);

			new Detail.AsyncTaskRunnerBuilder<byte[]> ().Do ((object[] args) => {
				return Net.Validator.MakeRequest (() => {
					return API.V1.DownloadFile (config.PackageUrl, (progress) => reportProgress(progress), () => { return isCancelled (); });
				});
			}).OnError ((System.Exception e) => {
				downloadError(e);
				return Detail.AsyncTaskRunner<byte[]>.ErrorRecovery.Nothing;
			}).OnCompletion ((byte[] downloadedBytes) => {
				if (downloadedBytes.Length == 0) {
					return;
				}
				try {
					System.IO.File.WriteAllBytes (downloadPath, downloadedBytes);
					string signatureUrl = SignatureUrlFromPackageUrl (config.PackageUrl);

					VerifySignature (signatureUrl, downloadedBytes, verificationError, downloadError, isCancelled, () => {
						downloadComplete (downloadPath);
						InstallPackage (downloadPath);
					});
				} catch (IOException e) {
					downloadError (e as Exception);
				}
			}).Run ();
		}

		private static string SignatureUrlFromPackageUrl(string packageUrl)
		{
			return packageUrl.Substring (0, packageUrl.LastIndexOf ('.')) + ".signature";
		}

		private static void VerifySignature(
			string signatureUrl,
			byte[] fileToVerify,
			VerificationError verificationError,
			DownloadError downloadError,
			IsCancelled isCancelled,
			Action onSuccess
		)
		{
			new Detail.AsyncTaskRunnerBuilder<byte[]> ().Do ((object[] args) => {
				return Net.Validator.MakeRequest (() => {
					return API.V1.DownloadFile (signatureUrl, (progress) => {}, () => { return isCancelled (); });
				});
			}).OnError ((System.Exception e) => {
				downloadError (e);
				return Detail.AsyncTaskRunner<byte[]>.ErrorRecovery.Nothing;
			}).OnCompletion ((byte[] signature) => {
				if (SignatureMatches (signature, fileToVerify) == VerificationStatus.Success) {
					onSuccess ();
					return;
				}

				verificationError ();
			}).Run ();
		}

		internal static VerificationStatus SignatureMatches(byte[] signature, byte[] bytesToVerify)
		{
			if (!File.Exists (SignatureCertificatePath)) {
				return VerificationStatus.Error;
			}

			try {
				string key = File.OpenText (SignatureCertificatePath).ReadToEnd ();

				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider ();
				rsa.FromXmlString (key);

				string base64Signature = System.Text.Encoding.ASCII.GetString (signature);
				byte[] bin = Convert.FromBase64String (base64Signature);

				return rsa.VerifyData (bytesToVerify, "SHA256", bin) ?
					VerificationStatus.Success :
					VerificationStatus.Failure;
			} catch (System.Exception e) {
				Utils.Log ("Unable to verify signature; {0}", e.ToString ());
				return VerificationStatus.Error;
			}
		}
		
		private static void InstallPackage(string downloadPath)
		{
			AssetDatabase.ImportPackage (downloadPath, true);
		}

		private static string PrepareDownloadFilePath(string downloadDirPath, string downloadedFileName)
		{
			DirectoryInfo downloadDirectory = new DirectoryInfo (downloadDirPath);
			if (!downloadDirectory.Exists) {
				downloadDirectory.Create ();
			}

			return Path.Combine (downloadDirectory.FullName, downloadedFileName);
		}
	}
}
