﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure.Core.TestFramework;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.EventHubs.Models;
using Azure.ResourceManager.EventHubs.Tests;

using NUnit.Framework;

namespace Azure.Management.EventHub.Tests
{
    public partial class ScenarioTests : EventHubsManagementClientBase
    {
        [Test]
        public async Task EventhubCreateGetUpdateDeleteAuthorizationRules_Length()
        {
            var location = await GetLocation();
            var resourceGroupName = Recording.GenerateAssetName(Helper.ResourceGroupPrefix);
            Subscription sub = await ArmClient.GetDefaultSubscriptionAsync();
            await sub.GetResourceGroups().CreateOrUpdateAsync(resourceGroupName, new ResourceGroupData(location));;
            var namespaceName = Recording.GenerateAssetName(Helper.NamespacePrefix);
            var createNamespaceResponse = await NamespacesOperations.StartCreateOrUpdateAsync(resourceGroupName, namespaceName,
                new EHNamespace()
                {
                    Location = location,
                    //Sku = new Sku("as")
                    Tags =
                        {
                            {"tag1", "value1"},
                            {"tag2", "value2"}
                        }
                }
                );
            var np = (await WaitForCompletionAsync(createNamespaceResponse)).Value;
            Assert.NotNull(createNamespaceResponse);
            Assert.AreEqual(np.Name, namespaceName);
            DelayInTest(5);
            //get the created namespace
            var getNamespaceResponse = await NamespacesOperations.GetAsync(resourceGroupName, namespaceName);
            if (string.Compare(getNamespaceResponse.Value.ProvisioningState, "Succeeded", true) != 0)
                DelayInTest(5);
            getNamespaceResponse = await NamespacesOperations.GetAsync(resourceGroupName, namespaceName);
            Assert.NotNull(getNamespaceResponse);
            Assert.AreEqual("Succeeded", getNamespaceResponse.Value.ProvisioningState,StringComparer.CurrentCultureIgnoreCase.ToString());
            Assert.AreEqual(location, getNamespaceResponse.Value.Location);
            // Create Eventhub
            var eventHubName = Helper.EventHubPrefix + "thisisthenamewithmorethan53charschecktoverifytheremovlaof50charsnamelengthlimit";
            var createEventHubResponse = await EventHubsOperations.CreateOrUpdateAsync(resourceGroupName, namespaceName, eventHubName, new Eventhub()
            {
                MessageRetentionInDays = 5
            });
            Assert.NotNull(createEventHubResponse);
            Assert.AreEqual(createEventHubResponse.Value.Name, eventHubName);
            //Get the created EventHub
            var getEventHubResponse = await EventHubsOperations.GetAsync(resourceGroupName, namespaceName, eventHubName);
            Assert.NotNull(getEventHubResponse.Value);
            Assert.AreEqual(EntityStatus.Active, getEventHubResponse.Value.Status);
            Assert.AreEqual(getEventHubResponse.Value.Name, eventHubName);
            // Create a EventHub AuthorizationRule
            var authorizationRuleName = Helper.AuthorizationRulesPrefix + "thisisthenamewithmorethan53charschecktoverifytheremovlaof50charsnamelengthlimit";
            string createPrimaryKey = Recording.GetVariable("CreatePrimaryKey", Helper.GenerateRandomKey());
            var createAutorizationRuleParameter = new AuthorizationRule()
            {
                Rights = { AccessRights.Listen, AccessRights.Send }
            };
            var createEventhubAuthorizationRuleResponse = await EventHubsOperations.CreateOrUpdateAuthorizationRuleAsync(resourceGroupName, namespaceName, eventHubName,
                authorizationRuleName, createAutorizationRuleParameter);
            Assert.NotNull(createEventhubAuthorizationRuleResponse);
            Assert.True(createEventhubAuthorizationRuleResponse.Value.Rights.Count == createAutorizationRuleParameter.Rights.Count);

            Assert.True(isContains(createAutorizationRuleParameter.Rights, createEventhubAuthorizationRuleResponse.Value.Rights));
            // Get created Eventhub AuthorizationRules
            var getEventhubAuthorizationRulesResponse = await EventHubsOperations.GetAuthorizationRuleAsync(resourceGroupName, namespaceName, eventHubName, authorizationRuleName);
            Assert.NotNull(getEventhubAuthorizationRulesResponse);
            Assert.True(getEventhubAuthorizationRulesResponse.Value.Rights.Count == createAutorizationRuleParameter.Rights.Count);
            Assert.True(isContains(createAutorizationRuleParameter.Rights, getEventhubAuthorizationRulesResponse.Value.Rights));
            // Get all Eventhub AuthorizationRules
            var getAllNamespaceAuthorizationRulesResponse = EventHubsOperations.ListAuthorizationRulesAsync(resourceGroupName, namespaceName, eventHubName);
            Assert.NotNull(getAllNamespaceAuthorizationRulesResponse);
            var getAllNSPAuthRulesRespList = await getAllNamespaceAuthorizationRulesResponse.ToEnumerableAsync();
            Assert.True(getAllNSPAuthRulesRespList.Count() == 1);
            bool isContainauthorizationRuleName = false;
            foreach (var detail in getAllNSPAuthRulesRespList)
            {
                if (detail.Name == authorizationRuleName)
                {
                    isContainauthorizationRuleName = true;
                    break;
                }
            }
            Assert.True(isContainauthorizationRuleName);
            // Update Eventhub authorizationRule
            string updatePrimaryKey = Recording.GetVariable("UpdatePrimaryKey", Helper.GenerateRandomKey());
            AuthorizationRule updateEventhubAuthorizationRuleParameter = new AuthorizationRule();
            updateEventhubAuthorizationRuleParameter.Rights.Add(AccessRights.Listen);
            var updateEventhubAuthorizationRuleResponse = await EventHubsOperations.CreateOrUpdateAuthorizationRuleAsync(resourceGroupName,
                namespaceName, eventHubName, authorizationRuleName, updateEventhubAuthorizationRuleParameter);
            Assert.NotNull(updateEventhubAuthorizationRuleResponse);
            Assert.AreEqual(authorizationRuleName, updateEventhubAuthorizationRuleResponse.Value.Name);
            Assert.True(updateEventhubAuthorizationRuleResponse.Value.Rights.Count == updateEventhubAuthorizationRuleParameter.Rights.Count);
            Assert.True(isContains(updateEventhubAuthorizationRuleParameter.Rights, updateEventhubAuthorizationRuleResponse.Value.Rights));
            // Get the updated Eventhub AuthorizationRule
            var getEventhubAuthorizationRuleResponse = await EventHubsOperations.GetAuthorizationRuleAsync(resourceGroupName, namespaceName, eventHubName,
                authorizationRuleName);
            Assert.NotNull(getEventhubAuthorizationRuleResponse);
            Assert.AreEqual(authorizationRuleName, getEventhubAuthorizationRuleResponse.Value.Name);
            Assert.True(getEventhubAuthorizationRuleResponse.Value.Rights.Count == updateEventhubAuthorizationRuleParameter.Rights.Count);
            Assert.True(isContains(updateEventhubAuthorizationRuleParameter.Rights, getEventhubAuthorizationRuleResponse.Value.Rights));
            // Get the connectionString to the Eventhub for a Authorization rule created
            var listKeysResponse = await EventHubsOperations.ListKeysAsync(resourceGroupName, namespaceName, eventHubName, authorizationRuleName);
            Assert.NotNull(listKeysResponse);
            Assert.NotNull(listKeysResponse.Value.PrimaryConnectionString);
            Assert.NotNull(listKeysResponse.Value.SecondaryConnectionString);
            //New connection string
            var regenerateConnection_primary = await EventHubsOperations.RegenerateKeysAsync(resourceGroupName, namespaceName, eventHubName, authorizationRuleName, new RegenerateAccessKeyParameters(KeyType.PrimaryKey));
            Assert.NotNull(regenerateConnection_primary);
            // Assert.AreNotEqual(listKeysResponse.Value.PrimaryConnectionString, regenerateConnection_primary.Value.PrimaryConnectionString);
            Assert.AreEqual(listKeysResponse.Value.SecondaryConnectionString, regenerateConnection_primary.Value.SecondaryConnectionString);
            var regenerateConnection_Secondary = await EventHubsOperations.RegenerateKeysAsync(resourceGroupName, namespaceName, eventHubName, authorizationRuleName, new RegenerateAccessKeyParameters(KeyType.SecondaryKey));
            Assert.NotNull(regenerateConnection_Secondary);
            // Assert.AreNotEqual(listKeysResponse.Value.SecondaryConnectionString, regenerateConnection_Secondary.Value.SecondaryConnectionString);
            Assert.AreEqual(regenerateConnection_primary.Value.PrimaryConnectionString, regenerateConnection_Secondary.Value.PrimaryConnectionString);
            // Delete Eventhub authorizationRule
            await EventHubsOperations.DeleteAuthorizationRuleAsync(resourceGroupName, namespaceName, eventHubName, authorizationRuleName);
            DelayInTest(5);
            // Delete Eventhub and check for the NotFound exception
            await EventHubsOperations.DeleteAsync(resourceGroupName, namespaceName, eventHubName);
            // Delete namespace and check for the NotFound exception
            await WaitForCompletionAsync (await NamespacesOperations.StartDeleteAsync(resourceGroupName, namespaceName));
        }
    }
}
