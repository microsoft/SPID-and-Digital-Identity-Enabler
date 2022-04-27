param name string
param environmentType string

resource spidSamlCheckAppPlan 'Microsoft.Web/serverfarms@2021-01-15'={
  name: name
  location: resourceGroup().location
  kind:'linux'
  properties:{
    reserved:true
    targetWorkerSizeId:0
    targetWorkerCount:1
  }
  sku:{
    name:'B1'
    tier: 'Basic'
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

output SpidSamlCheckPlanId string = spidSamlCheckAppPlan.id
