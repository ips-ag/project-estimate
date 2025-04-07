targetScope = 'resourceGroup'

@description('Required. Environment name.')
param env string

@description('The location for all resources.')
param location string = 'swedencentral'

@description('The name of the App Service Plan to create.')
param appServicePlanName string = 'asp-projectestimate-${env}'

@description('The SKU for the App Service Plan.')
@allowed([
  'B1'
  'P1'
])
param appServicePlanSku string = 'B1'

@description('The name of the UI App Service to create.')
param uiAppServiceName string = 'app-projectestimate-ui-${env}'

@description('The name of the API App Service to create.')
param apiAppServiceName string = 'app-projectestimate-api-${env}'

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
  }
  properties: {
    reserved: true
  }
}
