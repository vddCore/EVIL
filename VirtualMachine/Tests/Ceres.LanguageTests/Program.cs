﻿using Ceres.ExecutionEngine;
using Ceres.LanguageTests;

var vm = new CeresVM();
vm.Start();

var testRunner = new TestRunner("tests", vm);
await testRunner.RunTests();