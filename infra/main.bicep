targetScope = 'subscription'

@allowed([
  'Dev'
  'Test'
  'Prod'
])
@description('Required. Environment name.')
param environment string

@description('Optional. The name of the resource group to create.')
param rgName string = 'rg-projectestimate-${toLower(environment)}'

@description('Optional. The location for all resources.')
param location string = 'swedencentral'

@description('Optional. Resource Group tags.')
param tags object = {}

var env = toLower(environment)
var deploymentName = deployment().name

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-11-01' = {
  name: rgName
  location: location
}

module rg 'resourceGroup.bicep' = {
  scope: resourceGroup
  name: '${deploymentName}-resources'
  params: {
    env: env
    tags: tags
  }
}
