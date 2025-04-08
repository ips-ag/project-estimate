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

resource uiWebApp 'Microsoft.Web/sites@2024-04-01' = {
  name: uiWebAppName
  location: location
  tags: tags
  properties: {
    serverFarmId: appServicePlan.id
    clientAffinityEnabled: false
    httpsOnly: true
  }
  kind: 'app,linux'
}

resource apiWebApp 'Microsoft.Web/sites@2024-04-01' = {
  name: apiWebAppName
  location: location
  tags: tags
  properties: {
    serverFarmId: appServicePlan.id
    clientAffinityEnabled: false
    httpsOnly: true
  }
  kind: 'app,linux'

  resource config 'config' = {
    name: 'web'
    properties: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      cors: {
        allowedOrigins: [
          'https://${uiWebApp.properties.defaultHostName}'
        ]
        supportCredentials: false
      }
      appSettings: [
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
        { name: 'Azure__OpenAI__Endpoint', value: openAIService.outputs.endpoint }
        { name: 'Azure__OpenAI__ApiKey', value: openAIService.outputs.apiKey }
      ]
    }
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

output uiWebAppEndpoint string = 'https://${uiWebApp.properties.defaultHostName}'
output uiWebAppName string = uiWebApp.name

output apiWebAppEndpoint string = 'https://${apiWebApp.properties.defaultHostName}'
output apiWebAppName string = apiWebApp.name
