param sku object
param name string
param environmentType string

resource spidProxyAppPlan 'Microsoft.Web/serverfarms@2021-01-15'={
  name: name

  location: resourceGroup().location
  kind:'windows'
  /* sku:{
    name:sku
    tier:'Standard'
  } */
  sku:sku
  properties:{
    targetWorkerSizeId:0
    targetWorkerCount:1
  }
  tags:{
    Name:'INL-SPID'
    Company:'INL'
    Role:'App Service Plan'
    BizOwner:'TBD'
    AppOwner:'TBD'
    UpTime:'Full time'
    Environment:environmentType
  }
}
output spidProxyPlanID string = spidProxyAppPlan.id
output spidProxyPlanName string = spidProxyAppPlan.name
