using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class ActionSequenceTest
{
    [Test]
    public void TestTimeScale()
    {
        Debug.Log("TestTimeScale");
    }
    
    [UnityTest]
    public IEnumerator TestLoop()
    {
        int index = 0;
        while (true)
        {
            yield return null;
            Debug.Log($"TestTimeScale {Time.deltaTime}");
            index++;
            if(index > 100)
            {
                break;
            }
        }
    }
}