using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Tests.IntegrationTests;

namespace Staffing.HttpAggregator.Tests.IntegrationsTests.Controller
{
    [Trait("IntegrationTest", "Staffing.HttpAggregator.EmployeeStaffingPreference")]
    public class EmployeeStaffingPreferenceControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public EmployeeStaffingPreferenceControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }


    }
}
