targetScope = 'resourceGroup'

@description('Required. Environment name.')
param env string

@description('Optional. Resource location. Defaults to resource group location')
param location string = resourceGroup().location

@description('Optional. Resource tags. Defaults to resource group tags.')
param tags object = resourceGroup().tags

@description('Optional. Log Analytics workspace resource name.')
param logAnalyticsName string = 'law-projectestimate-${env}'

@description('Optional. Application Insights resource name.')
param appInsightsName string = 'ai-projectestimate-${env}'

@description('Optional. The name of the App Service Plan to create.')
param appServicePlanName string = 'asp-projectestimate-${env}'

@description('Optional. The SKU for the App Service Plan.')
@allowed([
  'B1'
  'P1'
])
param appServicePlanSku string = 'B1'

@description('Optional. The name of the Azure OpenAI Service to create.')
param openAIServiceName string = 'openai-projectestimate-${env}'

@description('Optional. The name of the Azure AI Document Intelligence to create.')
param documentIntelligenceName string = 'di-projectestimate-${env}'

@description('Optional. The name of the UI App Service to create.')
param uiWebAppName string = 'app-projectestimate-ui-${env}'

@description('Optional. Custom command to start UI App Service.')
param uiCustomCommand string = 'pm2 serve /home/site/wwwroot --spa --no-daemon'

@description('Optional. The name of the API App Service to create.')
param apiWebAppName string = 'app-projectestimate-api-${env}'

@description('Optional. The name of the Storage Account to create.')
param storageAccountName string = 'stoprojectestimate${env}'

@description('Optional. The name of the Key Vault to create.')
param keyVaultName string = 'kv-projectestimate-${env}'

@description('Optional. Indicates number fo days to retain deleted items (containers, blobs, snapshosts, versions). Default value is 7')
param daysSoftDelete int = 7

@description('Optional. Enable Key Vault purge protection. Default is false.')
param enableKeyVaultPurgeProtection bool = false

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: 5
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  kind: 'web'
  location: location
  tags: tags
  properties: {
    Application_Type: 'web'
    IngestionMode: 'LogAnalytics'
    SamplingPercentage: 50
    WorkspaceResourceId: logAnalytics.id
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    enablePurgeProtection: enableKeyVaultPurgeProtection ? true : null
    softDeleteRetentionInDays: 10
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

module storageAccount 'storageAccount.bicep' = {
  name: storageAccountName
  params: {
    storageAccountName: storageAccountName
    location: location
    tags: tags
    daysSoftDelete: daysSoftDelete
  }
}

module openAIService 'openAiService.bicep' = {
  name: openAIServiceName
  params: {
    openAIServiceName: openAIServiceName
    location: location
    tags: tags
  }
}

resource documentIntelligence 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: documentIntelligenceName
  location: location
  tags: tags
  kind: 'FormRecognizer'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: toLower(documentIntelligenceName)
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      ipRules: []
      virtualNetworkRules: []
    }
  }
}

resource applicationInsightsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  parent: keyVault
  name: 'ApplicationInsights--ConnectionString'
  properties: {
    value: appInsights.properties.ConnectionString
  }
}

resource storageConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'StorageAccount--ConnectionString'
  properties: {
    value: storageAccount.outputs.connectionString
  }
}

resource openAiEndpointSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Azure--OpenAI--Endpoint'
  properties: {
    value: openAIService.outputs.endpoint
  }
}

resource openAiApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Azure--OpenAI--ApiKey'
  properties: {
    value: openAIService.outputs.apiKey
  }
}

resource documentIntelligenceEndpointSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Azure--DocumentIntelligence--Endpoint'
  properties: {
    value: documentIntelligence.properties.endpoint
  }
}

resource documentIntelligenceApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Azure--DocumentIntelligence--ApiKey'
  properties: {
    value: documentIntelligence.listKeys().key1
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: appServicePlanSku
    capacity: 1
  }
  properties: {
    reserved: true
  }
  kind: 'linux'
}

module uiWebApp 'webApp.bicep' = {
  name: uiWebAppName
  params: {
    name: uiWebAppName
    location: location
    tags: tags
    appServicePlanId: appServicePlan.id
    clientAffinityEnabled: false
    httpsOnly: true
    kind: 'app,linux'
  }
}

module apiWebApp 'webApp.bicep' = {
  name: apiWebAppName
  dependsOn: [uiWebApp]
  params: {
    name: apiWebAppName
    location: location
    tags: tags
    appServicePlanId: appServicePlan.id
    clientAffinityEnabled: false
    httpsOnly: true
    kind: 'app,linux'
    useManagedIdentity: true
  }
}

resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, apiWebAppName, 'Key Vault Secrets User')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    ) // Key Vault Secrets User
    principalId: apiWebApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

resource uiWebAppConfig 'Microsoft.Web/sites/config@2024-04-01' = {
  name: '${uiWebApp.name}/web'
  properties: {
    linuxFxVersion: 'NODE|22-lts'
    appCommandLine: uiCustomCommand
  }
}

resource apiWebAppConfig 'Microsoft.Web/sites/config@2024-04-01' = {
  name: '${apiWebAppName}/web'
  dependsOn: [apiWebApp, keyVaultRoleAssignment]
  properties: {
    linuxFxVersion: 'DOTNETCORE|9.0'
    cors: {
      allowedOrigins: [uiWebApp.outputs.endpoint]
      supportCredentials: true
    }
    appSettings: [
      {
        name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
        value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=ApplicationInsights--ConnectionString)'
      }
      {
        name: 'Azure__StorageAccount__ConnectionString'
        value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=StorageAccount--ConnectionString)'
      }
      {
        name: 'Azure__OpenAI__Endpoint'
        value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Azure--OpenAI--Endpoint)'
      }
      {
        name: 'Azure__OpenAI__ApiKey'
        value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Azure--OpenAI--ApiKey)'
      }
      {
        name: 'Azure__DocumentIntelligence__Endpoint'
        value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Azure--DocumentIntelligence--Endpoint)'
      }
      {
        name: 'Azure__DocumentIntelligence__ApiKey'
        value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Azure--DocumentIntelligence--ApiKey)'
      }
      {
        name: 'Security__Authentication__Authority'
        value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Authentication--Authority)'
      }
      {
        name: 'Security__Authentication__Audience'
        value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Authentication--Audience)'
      }
    ]
  }
}
