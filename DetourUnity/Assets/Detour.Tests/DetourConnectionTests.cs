using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using DetourClient;

public class DetourConnectionTests {

    DetourConnection mockConn;

    [SetUp]
    public void SetupDetourConnectionTests()
    {
        mockConn = new DetourConnection();
    }

    [Test]
    public void DetourConnectionTestsSimplePasses() {
        // Use the Assert class to test conditions.

    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator DetourConnectionTestsWithEnumeratorPasses() {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }
}
