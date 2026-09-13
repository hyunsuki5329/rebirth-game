using System;
using System.IO;
using System.Text;
using Roguelite.Core;
namespace Roguelite
{
    public sealed class ProfileLoadResult
    {
        public ProfileData Profile { get; }
        public string Notice { get; }
        public ProfileLoadResult(ProfileData profile, string notice) { Profile = profile; Notice = notice; }
    }
    public sealed class ProfileFileStore
    {
        public string FilePath { get; }
        private readonly SaveCodec codec;
        private readonly Action<string, string, string> replace;
        private bool recoveryBlocked;
        public ProfileFileStore(string directory, SaveCodec codec, Action<string, string, string> replace = null)
        {
            FilePath = Path.Combine(directory, "profile.json"); this.codec = codec;
            this.replace = replace ?? ((source, target, backup) => File.Replace(source, target, backup));
        }
        public ProfileLoadResult Load()
        {
            recoveryBlocked = false; string notice = "";
            if (File.Exists(FilePath))
            {
                try { return new ProfileLoadResult(codec.Decode(File.ReadAllText(FilePath)), ""); }
                catch (SaveFormatException error)
                {
                    notice = error.Message + " Original preserved. ";
                    try { File.Move(FilePath, FilePath + ".corrupt-" + Guid.NewGuid().ToString("N")); }
                    catch (Exception io) when (IsFileError(io))
                    {
                        recoveryBlocked = true;
                        notice = error.Message + " Original could not be moved; saving is blocked to protect it. " + io.Message;
                    }
                }
                catch (Exception error) when (IsFileError(error))
                { recoveryBlocked = true; notice = "Cannot read profile; saving is blocked to protect it. " + error.Message; }
            }
            if (File.Exists(FilePath + ".bak"))
            {
                try { return new ProfileLoadResult(codec.Decode(File.ReadAllText(FilePath + ".bak")), notice + "Recovered from backup."); }
                catch (SaveFormatException error)
                {
                    notice += "Backup unavailable: " + error.Message + " ";
                    try { File.Move(FilePath + ".bak", FilePath + ".bak.corrupt-" + Guid.NewGuid().ToString("N")); }
                    catch (Exception io) when (IsFileError(io))
                    { recoveryBlocked = true; notice += "Cannot preserve rejected backup; saving is blocked. " + io.Message + " "; }
                }
                catch (Exception error) when (IsFileError(error))
                { recoveryBlocked = true; notice += "Cannot read backup; saving is blocked to protect it. " + error.Message + " "; }
            }
            return new ProfileLoadResult(new ProfileData(), notice.Length == 0 ? "" : notice + " Using a new profile.");
        }
        public bool TrySave(ProfileData profile, out string error)
        {
            error = "";
            if (recoveryBlocked) { error = "Save recovery is blocked. Check file access and restart; the original file is protected."; return false; }
            string json;
            try { json = codec.Encode(profile); }
            catch (SaveFormatException invalid) { error = invalid.Message; return false; }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                byte[] bytes = new UTF8Encoding(false).GetBytes(json);
                using (var stream = new FileStream(FilePath + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
                { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                if (File.Exists(FilePath)) replace(FilePath + ".tmp", FilePath, FilePath + ".bak");
                else File.Move(FilePath + ".tmp", FilePath);
                return true;
            }
            catch (Exception failure) when (IsFileError(failure))
            { error = failure.Message; return false; }
        }
        private static bool IsFileError(Exception error) => error is IOException || error is UnauthorizedAccessException || error is System.Security.SecurityException;
    }
}
