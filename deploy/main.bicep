param location string = resourceGroup().location
param appNameSuffix string = uniqueString(resourceGroup().id)
@allowed([
  'standard'
  'premium'
])
param keyVaultSku string = 'standard'
param userId string
param googleAccountEmail string
@secure()
param googleSecret string

var functionAppName = 'fn-googleassistant-${appNameSuffix}'
var appServicePlanName = 'FunctionPlan-${resourceGroup().location}'
var appInsightsName = 'AppInsights-${appNameSuffix}'
var storageAccountName = 'fnsto${replace(appNameSuffix, '-', '')}'
var functionRuntime = 'dotnet'
var keyVaultName = 'kv-func-${appNameSuffix}'
var functionAppKeySecretName = 'GoogleKey'

var roleArray = [
  'admin'
  'user'
]

var rolesObject = json('''
{
  "admin": {
    "roleId": "00482a5a-887f-4fb3-b363-3b7fe8e74483"
  },
  "user": {
    "roleId": "4633458b-17de-408a-b874-0445c86b69e6"
  }
}
''')

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource plan 'Microsoft.Web/serverfarms@2021-01-15' = {
  name: appServicePlanName
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2021-01-15' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${appInsights.properties.InstrumentationKey}'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionRuntime
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'SERVICE_ACCOUNT_EMAIL'
          value: googleAccountEmail
        }
        {
          name: 'SERVICE_ACCOUNT_KEY'
          value: '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/${functionAppKeySecretName})'
        }
      ]
    }
    httpsOnly: true
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: keyVaultName
  location: location
  properties: {
    enableRbacAuthorization: true
    enabledForTemplateDeployment: true
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: keyVaultSku
    }
    accessPolicies: []
  }
}

resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2021-06-01-preview' = {
  name: '${keyVault.name}/${functionAppKeySecretName}'
  properties: {
    value: googleSecret
  }
}

// See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#all
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [for (name, i) in roleArray: {
  scope: keyVault
  name: guid(resourceGroup().id, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '${rolesObject[name].roleId}'))
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '${rolesObject[name].roleId}')
    principalId: name == 'user' ? functionApp.identity.principalId : userId
  }
}]

output functionAppHostName string = functionApp.properties.defaultHostName
output functionName string = functionApp.name
