// Run against the built framework DLL using mcs/mono (set MONO_PATH to the output and game Bin64).
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using ShipCoreFramework;

class DirectionBudgetChecks
{
    static readonly MethodInfo Validate = typeof(ModConfig).GetMethod("ValidateDirectionBudgets", BindingFlags.NonPublic | BindingFlags.Static);
    static readonly MethodInfo Cap = typeof(BlockLimit).GetMethod("GetMaxCountForDirection", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly XmlSerializer Serializer = new XmlSerializer(typeof(BlockLimit));

    static BlockLimit Example()
    {
        return new BlockLimit
        {
            MaxCount = 100,
            MaxCountPerDirection = 10,
            AllowedDirections = new List<DirectionType> { DirectionType.Forward, DirectionType.Backward, DirectionType.Left, DirectionType.Right },
            DirectionBudgets = new[] {
                new DirectionBudget { Direction = DirectionType.Forward, MaxCount = 60 },
                new DirectionBudget { Direction = DirectionType.Backward, MaxCount = 20 }
            }
        };
    }

    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    static void Allocation(BlockLimit limit, bool valid)
    {
        try
        {
            Validate.Invoke(null, new object[] { limit, new ShipCore { UniqueName = "Test" }, "test", "test.xml" });
        }
        catch (TargetInvocationException error)
        {
            if (valid) throw error.InnerException;
            return;
        }
        Check(valid, "Expected allocation validation to fail.");
    }

    static BlockLimit Read(string xml)
    {
        using (var reader = new StringReader(xml)) return (BlockLimit)Serializer.Deserialize(reader);
    }

    static void Main()
    {
        var limit = Example();
        Allocation(limit, true);
        Check((float)Cap.Invoke(limit, new object[] { DirectionType.Forward }) == 60, "Forward override");
        Check((float)Cap.Invoke(limit, new object[] { DirectionType.Left }) == 10, "Inherited Left");
        limit.MaxCount = 99.99f;
        Allocation(limit, false);
        limit.MaxCountPerDirection = -1;
        Allocation(limit, true);
        Check(limit.HasDirectionalBudget, "Override-only limits must track and sync directions.");
        Check((float)Cap.Invoke(limit, new object[] { DirectionType.Left }) == -1, "Uncapped fallback");
        limit.MaxCount = 79;
        Allocation(limit, false);

        limit = Example();
        limit.DirectionBudgets = new DirectionBudget[0];
        limit.MaxCount = 40;
        Allocation(limit, true);
        limit.MaxCount = 39;
        Allocation(limit, false);
        limit.MaxCountPerDirection = -1;
        Allocation(limit, true);
        Check(!limit.HasDirectionalBudget, "Legacy disabled cap");

        limit = Example();
        limit.AllowedDirections = null;
        Allocation(limit, false);
        limit.MaxCount = 120;
        Allocation(limit, true);
        limit.AllowedDirections = new List<DirectionType> { DirectionType.Any };
        Allocation(limit, true);
        limit = Example();
        limit.BlockGroupReferences = new[] {
            new BlockGroupReference { Name = "Weapons", Directions = "Up", AllowedDirections = new List<DirectionType> { DirectionType.Up } },
            new BlockGroupReference { Name = "Tools", Directions = "Up", AllowedDirections = new List<DirectionType> { DirectionType.Up } }
        };
        limit.MaxCount = 90; // 60 + 20 retained overrides, Up inherited once.
        Allocation(limit, true);
        limit.MaxCount = 89;
        Allocation(limit, false);

        foreach (var invalid in new[] { -1f, float.NaN, float.PositiveInfinity })
        {
            limit = Example();
            limit.DirectionBudgets[0].MaxCount = invalid;
            Allocation(limit, false);
            limit = Example();
            limit.MaxCount = invalid;
            Allocation(limit, false);
        }
        limit = Example();
        limit.DirectionBudgets[1].Direction = DirectionType.Forward;
        Allocation(limit, false);
        limit.DirectionBudgets[1].Direction = DirectionType.Any;
        Allocation(limit, false);
        limit = Example();
        limit.MaxCount = 1;
        limit.MaxCountPerDirection = 0.1f;
        limit.DirectionBudgets[0].MaxCount = limit.DirectionBudgets[1].MaxCount = 0.4f;
        Allocation(limit, true);
        limit.MaxCount = limit.MaxCountPerDirection = 0;
        limit.DirectionBudgets[0].MaxCount = limit.DirectionBudgets[1].MaxCount = 0;
        Allocation(limit, true);

        limit = Example();
        using (var writer = new StringWriter())
        {
            Serializer.Serialize(writer, limit);
            var xml = writer.ToString();
            Check(xml.Contains("<DirectionBudget Direction=\"Forward\" MaxCount=\"60\""), "XML attributes");
            var restored = Read(xml);
            Allocation(restored, true);
            Check(restored.DirectionBudgets.Length == 2, "XML round trip");
            Check((float)Cap.Invoke(restored, new object[] { DirectionType.Backward }) == 20, "Restored override");
        }
        Allocation(Read("<BlockLimit><MaxCount>100</MaxCount><DirectionBudgets><DirectionBudget Direction=\"Forward\" /></DirectionBudgets></BlockLimit>"), false);
        Allocation(Read("<BlockLimit><MaxCount>100</MaxCount><DirectionBudgets><DirectionBudget MaxCount=\"1\" /></DirectionBudgets></BlockLimit>"), false);
        var legacy = Read("<BlockLimit><MaxCount>100</MaxCount></BlockLimit>");
        Check(!legacy.HasDirectionalBudget, "Legacy XML defaults");
        using (var writer = new StringWriter())
        {
            Serializer.Serialize(writer, legacy);
            Check(!writer.ToString().Contains("<DirectionBudgets"), "Omit absent overrides");
        }
        Console.WriteLine("Direction budget C# validation, lookup and XML round-trip checks passed.");
    }
}
