using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestDatabase", menuName = "ScriptableObjects/TestDatabase", order = 1)]
public class TestDatabase : ScriptableObject
{
    public List<TestData> tests;
}
