using System;
using System.ComponentModel.DataAnnotations;

namespace SourceGenerator.Annotations.Sample
{
    public static class Demo
    {
        public static void RangeMethod([Range(0, 1)] double factor) { }
    }
}
