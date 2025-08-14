@description('Required. The name of the App Service resource.')
param name string

@description('Optional. Resource location. Defaults to resource group location')
param location string = resourceGroup().location

@description('Optional. Resource tags. Defaults to resource group tags.')
param tags object = resourceGroup().tags

@description('Required. The id of the App Service Plan resource.')
param appServicePlanId string

@description('Optional. Enable client affinity.')
param clientAffinityEnabled bool = false

@description('Optional. Allow only HTTPS traffic.')
param httpsOnly bool = true

@description('Optional. Enable Always On. Defaults to true.')
param alwaysOn bool = true

@allowed([
  'app,linux'
  'app,linux,container'
])
@description(' Optional. Web app kind.')
param kind string = 'app,linux'

@description('Optional. Enable system-assigned managed identity.')
param useManagedIdentity bool = false

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: name
  location: location
  tags: tags
  identity: useManagedIdentity ? {
    type: 'SystemAssigned'
  } : null
  properties: {
    serverFarmId: appServicePlanId
    clientAffinityEnabled: clientAffinityEnabled
    httpsOnly: httpsOnly
    siteConfig: {
      alwaysOn: alwaysOn
    }
  }
  kind: kind
}

output endpoint string = 'https://${webApp.properties.defaultHostName}'
output name string = webApp.name
output id string = webApp.id
output principalId string = useManagedIdentity ? webApp.identity.principalId : ''
