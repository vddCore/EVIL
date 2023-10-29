using System.Collections.Generic;
using System.IO;
using Ceres.Runtime.Modules;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public partial class FsModuleTest
    {
        [Test]
        public void PathCombine()
        {
            var result = EvilTestResult(
                "fn test() -> fs.path.cmb('c:', 'weird stuff', 'test');"
            );

            result.Type.ShouldBe(DynamicValueType.String);
            result.ShouldBe("c:/weird stuff/test");
        }
    }
}