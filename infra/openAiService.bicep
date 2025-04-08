@description('Required. The name of the Azure OpenAI Service to create.')
param openAIServiceName string

@description('Optional. Resource location. Defaults to resource group location')
param location string = resourceGroup().location

@description('Optional. Resource tags. Defaults to resource group tags.')
param tags object = resourceGroup().tags

resource openAIService 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: openAIServiceName
  location: location
  tags: tags
  sku: {
    name: 'S0'
  }
  kind: 'OpenAI'
  properties: {
    publicNetworkAccess: 'Enabled'
  }

  resource defender 'defenderForAISettings' = {
    name: 'Default'
    properties: {
      state: 'Disabled'
    }
  }

  resource gpt4o 'deployments' = {
    name: 'gpt-4o'
    dependsOn: [
      defender
    ]
    sku: {
      name: 'GlobalStandard'
      capacity: 1
    }
    properties: {
      model: {
        format: 'OpenAI'
        name: 'gpt-4o'
      }
      versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
      currentCapacity: 1
      raiPolicyName: 'Microsoft.DefaultV2'
    }
  }

  resource gpt4omini 'deployments' = {
    name: 'gpt-4o-mini'
    dependsOn: [
      gpt4o
    ]
    sku: {
      name: 'GlobalStandard'
      capacity: 1
    }
    properties: {
      model: {
        format: 'OpenAI'
        name: 'gpt-4o-mini'
      }
      versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
      currentCapacity: 1
      raiPolicyName: 'Microsoft.DefaultV2'
    }
  }
}

output endpoint string = 'https://${openAIService.name}.openai.azure.com/'
#disable-next-line outputs-should-not-contain-secrets
output apiKey string = openAIService.listKeys().key1
