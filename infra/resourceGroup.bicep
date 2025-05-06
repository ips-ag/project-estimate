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

@description('Optional. The name of the API App Service to create.')
param apiWebAppName string = 'app-projectestimate-api-${env}'

@description('Optional. The name of the Storage Account to create.')
param storaceAccountName string = 'stoprojectestimate${env}'

@description('Optional. Indicates number fo days to retain deleted items (containers, blobs, snapshosts, versions). Default value is 7')
param daysSoftDelete int = 7

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
  }
}

resource apiWebAppConfig 'Microsoft.Web/sites/config@2024-04-01' = {
  name: '${apiWebAppName}/web'
  dependsOn: [apiWebApp]
  properties: {
    linuxFxVersion: 'DOTNETCORE|9.0'
    cors: {
      allowedOrigins: [uiWebApp.outputs.endpoint]
      supportCredentials: true
    }
    appSettings: [
      { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
      { name: 'Azure__OpenAI__Endpoint', value: openAIService.outputs.endpoint }
      { name: 'Azure__OpenAI__ApiKey', value: openAIService.outputs.apiKey }
      { name: 'Azure__DocumentIntelligence__Endpoint', value: documentIntelligence.properties.endpoint }
      { name: 'Azure__DocumentIntelligence__ApiKey', value: documentIntelligence.listKeys().key1 }
    ]
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: storaceAccountName
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: true
    allowSharedKeyAccess: true
    supportsHttpsTrafficOnly: true
  }

  resource blobService 'blobServices' = {
    name: 'default'
    properties: {
      cors: {
        corsRules: [
          {
            allowedHeaders: [
              '*'
            ]
            exposedHeaders: [
              '*'
            ]
            allowedOrigins: [
              '*'
            ]
            allowedMethods: [
              'GET'
            ]
            maxAgeInSeconds: 3600
          }
        ]
      }
      isVersioningEnabled: true
      lastAccessTimeTrackingPolicy: {
        enable: true
      }
      containerDeleteRetentionPolicy: {
        days: daysSoftDelete - 1
        enabled: true
      }
      restorePolicy: {
        enabled: true
        days: daysSoftDelete - 1
      }
      deleteRetentionPolicy: {
        enabled: true
        days: daysSoftDelete
      }
      changeFeed: {
        enabled: true
      }
    }
  }
}
