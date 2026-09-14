using System;
using System.IO;
namespace Roguelite
{
    public static class QaProfilePath
    {
        public static string Resolve(string root, string[] args, bool development)
        {
            if (!development) return root;
            int index = Array.IndexOf(args, "--qa-profile"); if (index < 0) return root;
            if (index + 1 >= args.Length) throw new ArgumentException("--qa-profile needs a name.");
            string name = args[index + 1];
            if (string.IsNullOrEmpty(name) || name.Length > 64) throw new ArgumentException("Invalid QA profile name.");
            foreach (char value in name)
                if (!(value >= 'a' && value <= 'z' || value >= 'A' && value <= 'Z' || value >= '0' && value <= '9' || value == '-' || value == '_'))
                    throw new ArgumentException("QA profile names use letters, digits, - and _ only.");
            return Path.Combine(root, "QA", name);
        }
    }
}
