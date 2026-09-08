using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Roguelite.Tests
{
    public class FoundationPlayTests
    {
        [UnityTest]
        public IEnumerator FoundationBodyDoesNotFallAndCannotCrossWall()
        {
            yield return SceneManager.LoadSceneAsync("PlayerTest");
            var body = GameObject.Find("FoundationBody").GetComponent<Rigidbody2D>();
            var start = body.position;
            yield return new WaitForSeconds(0.2f);
            Assert.That(body.position.y, Is.EqualTo(start.y).Within(0.01f));
            body.linearVelocity = Vector2.right * 10;
            yield return new WaitForSeconds(1f);
            Assert.That(body.position.x, Is.LessThan(7.1f), "The right wall must stop a moving body.");
            Assert.That(body.position.x, Is.GreaterThan(5f), "The body must actually move before collision.");
        }
    }
}
