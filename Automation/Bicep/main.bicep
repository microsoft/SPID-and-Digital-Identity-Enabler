targetScope='subscription'

param ADFSAttributeConsumerServiceUrl string
param allowedOrigin string
param SPIDL string
param UpdateAssertionConsumerServiceUrl string
param WEBSITE_LOAD_CERTIFICATES string
param ADFSEntityId string
param OriginalEntityId string
param SPIDSAMLCHECK string
param SPIDSAMLCHECK_LOGOUT string
param CERTNAME string
param CERTPASSWORD string
param SpidSamlCheckMetadata string

// param environmentSuffix string = 'devtest'

param CUA string

@description('The type of environment. This must be nonprod or prod.')
@allowed([
  'devtest'
  'prod'
])
param environmentType string
// Define the SKUs for each component based on the environment type.
var environmentConfigurationMap = {
  devtest: {
    appServicePlan: {
      sku: {
        name: 'S1'
        capacity: 1
      }
    }
    storageAccount:{
      sku:{
        name:'Standard_LRS'
      }
      kind:'StorageV2'
    }
    containerRegistry: {
      sku: {
        name: 'Basic'
      }
    }
    aadB2cTenant:{
      sku:{
        name:'PremiumP1'
        tier: 'A0'
      }
    }
  }
  prod: {
    appServicePlan: {
      sku: {
        name: 'P1V2'
        capacity: 2
      }
    }
    storageAccount:{
      sku:{
        name:'Standard_LRS'
      }
      kind:'StorageV2'
    }
    containerRegistry:{
      sku:{
        name:'Standard'
      }
    }
    aadB2cTenant:{
      sku:{
        name:'PremiumP1'
        tier: 'A0'
      }
    }
  }
}

// Create resource group for tenant B2C
var resourceGroupNameB2C ='rg-${environmentType}-we-002'
resource rgAADB2CINL 'Microsoft.Resources/resourceGroups@2021-04-01'= {
  name: resourceGroupNameB2C
  location: deployment().location
  tags:{
    Name:'INL-SPID'
    Company: 'INL'
    Role:'Resource Group'
    BizOwner:'TBD'
    AppOwner:'TBD'
    UpTime:'Full time'
    Environment: environmentType 
    
  }
}

// Assign CUA for ACR. Actually at subscription level (needs check)
resource cua 'Microsoft.Resources/deployments@2021-04-01'= if(!empty(CUA)) {
  name: 'pid-${CUA}' 
  location: deployment().location
  properties: {
    mode:'Incremental' 
    template:{
      '$schema': 'https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#'
      contentVersion:'1.0.0.0'
      resources: []
      }
    }
} 

// Create Web App SPID Proxy

// 1) Create Resource group
var resourceGroupName ='rg-${environmentType}-we-001'
resource rgINL 'Microsoft.Resources/resourceGroups@2021-04-01'= if(!empty(CUA))  {
  name: resourceGroupName
  location: deployment().location
  tags:{
    Name:'INL-SPID'
    Company: 'INL'
    Role:'Resource Group'
    BizOwner:'TBD'
    AppOwner:'TBD'
    UpTime:'Full time'
    Environment:environmentType
  }
  dependsOn: [
    cua
  ]
}

// 2) Create App Service Plan
param planName string = 'plan-${environmentType}-we-001'
module spidProxyAppPlanDeploy 'SpidProxy/spidProxyAppPlan.bicep'= if(!empty(CUA)){
scope:rgINL
name:'INL-plan-${environmentType}-we-001-deploy'
params:{
  name :planName
  environmentType:environmentType
  sku:environmentConfigurationMap[environmentType].appServicePlan.sku
  }
  dependsOn: [
    rgINL
    cua
  ]
}

// 3) Create Application Insigths
param spidProxyApplicationInsigthsName string ='appi-${environmentType}-we-001'
module spidProxyApplicationInsigthsDeploy 'SpidProxy/spidProxyWebAppInsigths.bicep'= if(!empty(CUA)){
  scope: rgINL
  name: 'INL-appi-${environmentType}-we-001'
  params:{
    name:spidProxyApplicationInsigthsName
    environmentType:environmentType
    }
    dependsOn: [
      rgINL
      cua
    ]
}

// 4) Create Web App SPID Proxy
param spidProxyWebAppName string = 'ap-${environmentType}-we-001'
module spidProxyWebAppDeploy 'SpidProxy/spidProxyWebApp.bicep'=if(!empty(CUA)){
  scope: rgINL
  name: 'INL-ap-${environmentType}-we-001-deploy'
  params: {
    name:spidProxyWebAppName
    environmentType:environmentType
    spidProxyAppPlanID: spidProxyAppPlanDeploy.outputs.spidProxyPlanID
    spidProxyAppInsigthsInstrumentationKey:spidProxyApplicationInsigthsDeploy.outputs.spidProxyInstrumentationKey
    spidProxyAppInsightsConnectionString:spidProxyApplicationInsigthsDeploy.outputs.spidProxyInstrumentationKeyConnectionString
    ADFSAttributeConsumerServiceUrl:ADFSAttributeConsumerServiceUrl
    SPIDL:SPIDL
    ADFSEntityId:ADFSEntityId
    OriginalEntityId:OriginalEntityId
    SPIDSAMLCHECK:SPIDSAMLCHECK
    SPIDSAMLCHECK_LOGOUT:SPIDSAMLCHECK_LOGOUT
    UpdateAssertionConsumerServiceUrl:UpdateAssertionConsumerServiceUrl
    WEBSITE_LOAD_CERTIFICATES:WEBSITE_LOAD_CERTIFICATES
    CERTNAME:CERTNAME
    CERTPASSWORD:CERTPASSWORD
    SpidSamlCheckMetadata:SpidSamlCheckMetadata
  }
  dependsOn:[
    rgINL
    cua
    spidProxyApplicationInsigthsDeploy
  ]
} 

  /* module spidProxyGitRepositoryDeploy 'spidProxyWebAppGitRepo.bicep'={
  scope: rgINL
  name: 'ap-devtest-we-002git'
  params: {
    spidProxyAppPlanName: 'ap-devtest-we-002' 
    spidProxyBranch: 'Master'
    spidProxyRepositoryUrl: 'https://fumer.visualstudio.com/SPID/_git/SPID_ALM'
  }
}  */ 

// 5) Create Storage Account
param spidCustomUIStorageAccount string ='st${environmentType}we001'
module spidCustomUIStorageAccountDeploy 'spidStorageAccount.bicep'=if(!empty(CUA)){
  scope: rgINL
  name: 'INL-st${environmentType}we001-deploy'
  params:{
    allowedOrigin: allowedOrigin
    name:spidCustomUIStorageAccount
    environmentType:environmentType
    sku:environmentConfigurationMap[environmentType].storageAccount.sku
    kind:environmentConfigurationMap[environmentType].storageAccount.kind
  }
  dependsOn: [
    rgINL
    cua
  ]
} 

// // Create SPID Saml Check
// // 1) Create Azure container registry
// param spidSamlCheckContainerRegistryName string ='cr${environmentType}we001'
// module spidSamlCheckContainerRegistryDeploy 'SpidSamlCheck/spidContainerRegistry.bicep'=if(!empty(CUA)){
//   scope: rgINL
//   name: 'INL-cr-${environmentType}-we-001-deploy'
//   params:{
//     name: spidSamlCheckContainerRegistryName
//     environmentType:environmentType
//     sku:environmentConfigurationMap[environmentType].containerRegistry.sku
//   }
//   dependsOn: [
//     rgINL
//     cua
//   ]
// } 

// // 2) Create App Service Plan (we use the same resource group used for SPID Proxy)
// param spidSamlCheckAppPlanName string ='plan-${environmentType}-we-002'
// module spidSamlCheckAppPlanDeploy 'SpidSamlCheck/spidSamlCheckAppPlan.bicep'=if(!empty(CUA)){
//   scope: rgINL
//   name:'INL-plan-${environmentType}-we-002-deploy'
//   params:{
//     name : spidSamlCheckAppPlanName
//     environmentType:environmentType
//   }
//   dependsOn: [
//     rgINL
//     cua
//   ]
// }

// // 3) Create Web App SPID Saml Check
// param spidSamlCheckWebAppName string ='spidINLSamlCheck'
// module spidSamlCheckWebAppDeploy 'SpidSamlCheck/spidSamlCheckWebApp.bicep'=if(!empty(CUA)){
//   scope: rgINL
//   name: 'INL-spidSamlCheckWebApp-Deploy'
//   params: {
//     name:spidSamlCheckWebAppName
//     environmentType:environmentType
//     spidSamlCheckAppInsightsConnectionString:spidProxyApplicationInsigthsDeploy.outputs.spidProxyInstrumentationKeyConnectionString
//     spidSamlCheckAppInsigthsInstrumentationKey: spidProxyApplicationInsigthsDeploy.outputs.spidProxyInstrumentationKey
//     spidSamlCheckAppPlanID: spidSamlCheckAppPlanDeploy.outputs.SpidSamlCheckPlanId
//   }
//   dependsOn: [
//     rgINL
//     cua
//   ]
// }  

// output mgmtStatus string = ((!empty(CUA)) ? 'Resources created' : 'You MUST get a valid CUA!')


