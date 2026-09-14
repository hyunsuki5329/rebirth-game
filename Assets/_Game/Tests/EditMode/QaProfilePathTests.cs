using System;
using System.IO;
using NUnit.Framework;
namespace Roguelite.Tests
{
    public class QaProfilePathTests
    {
        [Test] public void NormalLaunchUsesDefaultPathAndDevelopmentProfileStaysUnderQaRoot()
        {
            string root = Path.GetFullPath("TestResults/profile-root");
            Assert.That(QaProfilePath.Resolve(root, new string[0], true), Is.EqualTo(root));
            Assert.That(QaProfilePath.Resolve(root, new[] { "--qa-profile", "m13-check" }, false), Is.EqualTo(root));
            Assert.That(QaProfilePath.Resolve(root, new[] { "--qa-profile", "m13-check" }, true), Is.EqualTo(Path.Combine(root, "QA", "m13-check")));
            foreach (string invalid in new[] { "..", "a/b", "a\\b", "", "a:b" })
                Assert.Throws<ArgumentException>(() => QaProfilePath.Resolve(root, new[] { "--qa-profile", invalid }, true));
        }
    }
}
