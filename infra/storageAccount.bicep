@description('Required. The name of the Azure Storage Account to create.')
param storageAccountName string

@description('Optional. Resource location. Defaults to resource group location')
param location string = resourceGroup().location

@description('Optional. Resource tags. Defaults to resource group tags.')
param tags object = resourceGroup().tags

@description('Optional. Indicates number fo days to retain deleted items (containers, blobs, snapshosts, versions). Default value is 7')
param daysSoftDelete int = 7

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: storageAccountName
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

#disable-next-line outputs-should-not-contain-secrets
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
