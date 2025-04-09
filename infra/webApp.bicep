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

@allowed([
  'app,linux'
  'app,linux,container'
])
@description(' Optional. Web app kind.')
param kind string = 'app,linux'

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    serverFarmId: appServicePlanId
    clientAffinityEnabled: clientAffinityEnabled
    httpsOnly: httpsOnly
  }
  kind: kind
}

output endpoint string = 'https://${webApp.properties.defaultHostName}'
output name string = webApp.name
output id string = webApp.id
