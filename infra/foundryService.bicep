@description('Required. The name of the Azure AI Foundry account to create.')
param foundryServiceName string

@description('Required. The name of the Azure AI Foundry project to create.')
param projectName string

@description('Optional. Resource location. Defaults to resource group location')
param location string = resourceGroup().location

@description('Optional. Resource tags. Defaults to resource group tags.')
param tags object = resourceGroup().tags

@description('Optional. Deployment capacity.')
param capacity int = 20

resource foundryAccount 'Microsoft.CognitiveServices/accounts@2025-06-01' = {
  name: foundryServiceName
  location: location
  tags: tags
  sku: {
    name: 'S0'
  }
  kind: 'AIServices'
  properties: {
    allowProjectManagement: true
    customSubDomainName: foundryServiceName
    publicNetworkAccess: 'Enabled'
  }

  resource defender 'defenderForAISettings' = {
    name: 'Default'
    properties: {
      state: 'Disabled'
    }
  }

  resource gpt5mini 'deployments' = {
    name: 'gpt-5-mini'
    dependsOn: [
      defender
    ]
    sku: {
      name: 'GlobalStandard'
      capacity: capacity
    }
    properties: {
      model: {
        format: 'OpenAI'
        name: 'gpt-5-mini'
      }
      versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
      currentCapacity: capacity
      raiPolicyName: 'Microsoft.DefaultV2'
    }
  }

  resource project 'projects' = {
    name: projectName
    location: location
    properties: {
      displayName: projectName
    }
  }
}

output endpoint string = foundryAccount.properties.endpoint
#disable-next-line outputs-should-not-contain-secrets
output apiKey string = foundryAccount.listKeys().key1
