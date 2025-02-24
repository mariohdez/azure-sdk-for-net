﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.TestFramework;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Network.Tests.Helpers;
using NUnit.Framework;

namespace Azure.ResourceManager.Network.Tests
{
    public class RouteFilterTests : NetworkServiceClientTestBase
    {
        public RouteFilterTests(bool isAsync) : base(isAsync)
        {
        }

        [SetUp]
        public void ClearChallengeCacheforRecord()
        {
            if (Mode == RecordedTestMode.Record || Mode == RecordedTestMode.Playback)
            {
                Initialize();
            }
        }

        private const string Filter_Commmunity = "12076:51006";

        private async Task<RouteFilterCollection> GetCollection()
        {
            var resourceGroup = await CreateResourceGroup(Recording.GenerateAssetName("route_filter_test_"));
            return resourceGroup.GetRouteFilters();
        }

        [Test]
        [RecordedTest]
        public async Task RouteFilterApiTest()
        {
            Subscription subscription = await ArmClient.GetDefaultSubscriptionAsync();
            var filterCollection = await GetCollection();

            // Create route filter
            string filterName = Recording.GenerateAssetName("filter");
            string ruleName = Recording.GenerateAssetName("rule");

            RouteFilter filter = await CreateDefaultRouteFilter(filterCollection,
                filterName);
            Assert.AreEqual("Succeeded", filter.Data.ProvisioningState.ToString());
            Assert.AreEqual(filterName, filter.Data.Name);
            Assert.IsEmpty(filter.Data.Rules);

            var filters = await filterCollection.GetAllAsync().ToEnumerableAsync();
            Has.One.Equals(filters);
            Assert.AreEqual(filterName, filters[0].Data.Name);
            Assert.IsEmpty(filters[0].Data.Rules);

            var allFilters = await subscription.GetRouteFiltersAsync().ToEnumerableAsync();
            // there could be other filters in the current subscription
            Assert.True(allFilters.Any(f => filterName == f.Data.Name && f.Data.Rules.Count == 0));

            // Crete route filter rule
            RouteFilterRule filterRule = await CreateDefaultRouteFilterRule(filter, ruleName);
            Assert.AreEqual("Succeeded", filterRule.Data.ProvisioningState.ToString());
            Assert.AreEqual(ruleName, filterRule.Data.Name);

            Response<RouteFilter> getFilterResponse = await filterCollection.GetAsync(filterName);
            Assert.AreEqual(filterName, getFilterResponse.Value.Data.Name);

            filter = await filterCollection.GetAsync(filterName);
            Assert.AreEqual(filterName, filter.Data.Name);
            Has.One.Equals(filter.Data.Rules);
            Assert.AreEqual(ruleName, filter.Data.Rules[0].Name);

            filterRule = await filter.GetRouteFilterRules().GetAsync(ruleName);
            Assert.AreEqual(ruleName, filterRule.Data.Name);

            // Update route filter
            filterRule.Data.Access = Access.Deny;
            filterRule = await filter.GetRouteFilterRules().CreateOrUpdate(ruleName, filterRule.Data).WaitForCompletionAsync();
            Assert.AreEqual(ruleName, filterRule.Data.Name);
            Assert.AreEqual(Access.Deny, filterRule.Data.Access);

            // Add filter rule, this will fail due to the limitation of maximum 1 rule per filter
            Assert.ThrowsAsync<RequestFailedException>(async () => await CreateDefaultRouteFilterRule(filter, Recording.GenerateAssetName("rule2")));

            filter = await filterCollection.GetAsync(filterName);
            Has.One.Equals(filter.Data.Rules);
            Assert.AreEqual(ruleName, filter.Data.Rules[0].Name);

            // Delete fileter rule
            await filterRule.DeleteAsync();

            var rules = await filter.GetRouteFilterRules().GetAllAsync().ToEnumerableAsync();
            Assert.IsEmpty(rules);

            // Delete filter
            await filter.DeleteAsync();
            allFilters = await subscription.GetRouteFiltersAsync().ToEnumerableAsync();
            Assert.False(allFilters.Any(f => filter.Id == f.Id));
        }

        private async Task<RouteFilter> CreateDefaultRouteFilter(RouteFilterCollection filterCollection, string filterName,
            bool containsRule = false)
        {
            var filter = new RouteFilterData()
            {
                Location = TestEnvironment.Location,
                Tags = { { "key", "value" } }
            };

            if (containsRule)
            {
                var rule = new RouteFilterRuleData()
                {
                    Name = Recording.GenerateAssetName("test"),
                    Access = Access.Allow,
                    Communities = { Filter_Commmunity },
                    Location = TestEnvironment.Location
                };

                filter.Rules.Add(rule);
            }

            // Put route filter
            Operation<RouteFilter> filterOperation = await filterCollection.CreateOrUpdateAsync(filterName, filter);
            return await filterOperation.WaitForCompletionAsync();;
        }

        private async Task<RouteFilterRule> CreateDefaultRouteFilterRule(RouteFilter filter,  string ruleName)
        {
            var rule = new RouteFilterRuleData()
            {
                Access = Access.Allow,
                Communities = { Filter_Commmunity },
                Location = filter.Data.Location
            };

            // Put route filter rule
            Operation<RouteFilterRule> ruleOperation = await filter.GetRouteFilterRules().CreateOrUpdateAsync(ruleName, rule);
            Response<RouteFilterRule> ruleResponse = await ruleOperation.WaitForCompletionAsync();;
            return ruleResponse;
        }
    }
}
