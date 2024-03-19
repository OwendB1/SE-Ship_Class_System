using System.Linq;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public class DetailedGridClassCheckResult
    {
        public DetailedGridClassCheckResult(GridCheckResult<int> maxBlocks,
            GridCheckResult<int> minBlocks, GridCheckResult<int> maxPCU, GridCheckResult<float> maxMass,
            BlockLimitCheckResult[] blockLimits)
        {
            MaxBlocks = maxBlocks;
            MinBlocks = minBlocks;
            MaxPCU = maxPCU;
            MaxMass = maxMass;
            BlockLimits = blockLimits;

            Passed = maxBlocks.Passed && minBlocks.Passed && maxPCU.Passed && maxMass.Passed &&
                     (blockLimits == null || blockLimits.All(blockLimit => blockLimit.Passed));
        }

        public bool Passed { get; private set; }
        public GridCheckResult<int> MaxBlocks { get; private set; }
        public GridCheckResult<int> MinBlocks { get; private set; }
        public GridCheckResult<int> MaxPCU { get; private set; }
        public GridCheckResult<float> MaxMass { get; private set; }
        public BlockLimitCheckResult[] BlockLimits { get; private set; }
    }

    public struct GridCheckResult<T>
    {
        public bool Active;
        public bool Passed;
        public T Value;
        public T Limit;

        public GridCheckResult(bool active, bool passed, T value, T limit)
        {
            Active = active;
            Passed = passed;
            Value = value;
            Limit = limit;
        }
    }

    public struct BlockLimitCheckResult
    {
        public bool Passed;
        public float Score;
        public int Blocks;
        public float Max;
    }
}