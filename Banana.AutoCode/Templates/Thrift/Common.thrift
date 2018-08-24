struct DateTime2 {
	1: i64 Value
}

/*
using System;

public partial class DateTime2
{
    private static readonly DateTime _startTime = new DateTime(1, 1, 1);

    public static implicit operator Int64(DateTime2 time)
    {
        if (time == null)
        {
            return default(Int64);
        }

        return time.Value;
    }

    public static implicit operator DateTime2(Int64 value)
    {
        return new DateTime2() { Value = value };
    }

    public static implicit operator DateTime(DateTime2 time)
    {
        if (time == null)
        {
            return default(DateTime);
        }

        return _startTime.AddMilliseconds(time.Value);
    }

    public static implicit operator DateTime2(DateTime time)
    {
        var value = (time.Ticks - _startTime.Ticks) / 10000;
        return new DateTime2() { Value = value };
    }

    public string ToString(string format)
    {
        var time = (DateTime)this;
        return time.ToString(format);
    }
}

*/